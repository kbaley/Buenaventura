using System.Globalization;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Dtos;

namespace Buenaventura.Api;

public class TransactionParser(CoronadoDbContext context)
{
    public IEnumerable<TransactionForDisplay> Parse(IFormFile file, Guid accountId, DateTime? fromDate)
    {
        var transactions = new List<TransactionForDisplay>();
        if (file.Length > 0)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var peek = reader.Peek();
            if (peek != -1)
            {
                if ((char)peek == '!')
                    transactions = ParseQif(reader, accountId, fromDate);
                else if ((char)peek == 'O')
                    transactions = ParseOfx(reader, accountId);
                else
                    transactions = ParseCsv(reader, accountId, fromDate);
            }
        }
        fromDate ??= DateTime.MinValue;
        return transactions
            .Where(t => t.TransactionDate >= fromDate)
            .Reverse();
    }

    // Assumes we're parsing for a credit card, specifically AMEX
    private List<TransactionForDisplay> ParseCsv(StreamReader reader, Guid accountId, DateTime? fromDate)
    {
        var transactions = new List<TransactionForDisplay>();
        // Trash the header line
        reader.ReadLine();

        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine()!.Split(',');
            var trx = new TransactionForDisplay
            {
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                EnteredDate = DateTime.Now,
                Vendor = "",
                TransactionDate = DateTime.Parse(line[0]),
                Description = line[1],
                Amount = -decimal.Parse(line[4])
            };
            trx.SetDebitAndCredit();
            if (!fromDate.HasValue || trx.TransactionDate >= fromDate.Value)
                transactions.Add(trx);
        }
        return transactions;
    }


    private List<TransactionForDisplay> ParseOfx(StreamReader reader, Guid accountId)
    {
        var transactions = new List<TransactionForDisplay>();

        // This is OFX 1.0.2 and it's not valid XML. Alas...
        var line = reader.ReadLine();
        while (!line!.StartsWith("<STMTTRN>"))
        {

            line = reader.ReadLine();
            if (line!.StartsWith("</OFX>"))
            {
                return transactions;
            }
        }

        var trx = new TransactionForDisplay
        {
            TransactionId = Guid.NewGuid(),
            AccountId = accountId,
            EnteredDate = DateTime.Now,
            TransactionType = TRANSACTION_TYPE.REGULAR,
            Vendor = ""
        };

        while (reader.Peek() >= 0)
        {
            line = reader.ReadLine();
            var data = GetOfxDataFrom(line!);
            switch (data.Field.ToUpper())
            {
                case "DTPOSTED":
                    trx.TransactionDate = DateTime.ParseExact(data.Value.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                    break;
                case "TRNAMT":
                    trx.Amount = decimal.Parse(data.Value);
                    break;
                case "MEMO":
                    trx.Description = data.Value;
                    break;
                case "FITID":
                    trx.DownloadId = data.Value;
                    break;
                case "/STMTTRN":
                    // Transaction is over
                    trx.SetDebitAndCredit();
                    transactions.Add(trx);

                    trx = new TransactionForDisplay
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountId = accountId,
                        EnteredDate = DateTime.Now,
                        TransactionType = TRANSACTION_TYPE.REGULAR,
                        Vendor = ""
                    };
                    break;
            }
        }

        return transactions;
    }

    private (string Field, string Value) GetOfxDataFrom(string line)
    {
        var items = line.Split(">");
        items[0] = items[0].Substring(1);
        if (items.Length == 1) return (items[0], "");
            
        return (items[0], items[1]);
    }

    private List<TransactionForDisplay> ParseQif(StreamReader reader, Guid accountId, DateTime? fromDate)
    {
        var transactions = new List<TransactionForDisplay>();
        var trx = new TransactionForDisplay
        {
            TransactionId = Guid.NewGuid(),
            AccountId = accountId,
            EnteredDate = DateTime.Now,
            TransactionType = TRANSACTION_TYPE.REGULAR,
            Vendor = ""
        };
        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();
            if (line!.Length > 1 || line == "^")
            {
                switch (line[0])
                {
                    case '^':
                        if (!fromDate.HasValue || trx.TransactionDate >= fromDate.Value)
                        {
                            if (trx.Vendor.StartsWith("Transfer :", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var relatedAccountName = trx.Vendor.Replace("Transfer : ", "");
                                var relatedAccount = context.Accounts.Single(a => a.Name == relatedAccountName);
                                trx.TransactionType = TRANSACTION_TYPE.TRANSFER;
                                trx.RelatedAccountId = relatedAccount.AccountId;
                                trx.Vendor = "";
                            }
                            trx.SetDebitAndCredit();
                            transactions.Add(trx);
                        }
                        trx = new TransactionForDisplay
                        {
                            TransactionId = Guid.NewGuid(),
                            AccountId = accountId,
                            EnteredDate = DateTime.Now,
                            TransactionType = TRANSACTION_TYPE.REGULAR,
                            Vendor = ""
                        };
                        break;
                    case 'D':
                        trx.TransactionDate = DateTime.Parse(line.Substring(1));
                        break;
                    case 'T':
                        trx.Amount = decimal.Parse(line.Substring(1));
                        break;
                    case 'P':
                        trx.Vendor = line.Substring(1);
                        break;
                    case 'L':
                        var category = line.Substring(1)
                            .Replace("Everyday Expenses:", "")
                            .Replace("Rainy Day Funds:", "")
                            .Replace("Monthly Bills:", "");
                        trx.CategoryId = context.GetOrCreateCategory(category).GetAwaiter().GetResult().CategoryId;
                        break;
                    case 'M':
                        trx.Description = line.Substring(1);
                        break;
                    case 'C':
                        trx.IsReconciled = line.Substring(1) == "c";
                        break;
                }
            }
        }
        return transactions;
    }


    // For certain banks that don't offer file downloads so we have to copy and paste the transactions from their
    // crappy online banking site
    public IEnumerable<TransactionForDisplay> Parse(string transactionList, Guid accountId, DateTime? fromDate)
    {
        var lines = transactionList.Split('\n');
        // And wouldn't you know it, the same bank uses different formats for banks accounts vs. credit cards
        // Luckily, an easy way to differentiate them is that the date includes a comma in one and not the other
        var transactions = lines[0].Contains(',') 
            ? ParseBankAccount(lines, accountId) 
            : ParseCreditCard(lines, accountId);
        fromDate ??= DateTime.MinValue;
        return transactions
            .Where(t => t.TransactionDate >= fromDate);
    }

    private IEnumerable<TransactionForDisplay> ParseBankAccount(string[] lines, Guid accountId) {

        var transactions = new List<TransactionForDisplay>();
        for (var i = 0; i < lines.Length; i += 3) {
            var date = lines[i].TrimEnd();
            var yearAndDescription = lines[i+1];
            var amount = decimal.Parse(lines[i+2].Replace("$", "").Split(' ')[0]);
            var year = int.Parse(yearAndDescription.Split('\t')[0].Trim());
            date += year;
            var transactionDate = DateTime.ParseExact(date, "MMM d,yyyy", CultureInfo.InvariantCulture);
            var description = yearAndDescription.Split('\t')[1].Trim();
            var transaction = new TransactionForDisplay{
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                TransactionDate = transactionDate,
                EnteredDate = DateTime.Now,
                TransactionType = TRANSACTION_TYPE.REGULAR,
                Amount = amount,
                Description = description
            };
            transaction.SetDebitAndCredit();
            transactions.Add(transaction);
        }

        return transactions;
    }

    private IEnumerable<TransactionForDisplay> ParseCreditCard(string[] lines, Guid accountId) {

        var transactions = new List<TransactionForDisplay>();
        for (var i = 0; i < lines.Length; i += 4) {
            var date = lines[i];
            var yearAndDescription = lines[i+1];
            // This is a credit card transaction; negate it
            var amount = -decimal.Parse(lines[i+3].Replace("$", "").Replace(" ", "").Trim());
            var year = int.Parse(yearAndDescription.Split('\t')[0].Trim());
            date += "," + year;
            var transactionDate = DateTime.ParseExact(date, "MMM d,yyyy", CultureInfo.InvariantCulture);
            var description = yearAndDescription.Split('\t')[1].Trim();
            var transaction = new TransactionForDisplay{
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                TransactionDate = transactionDate,
                EnteredDate = DateTime.Now,
                TransactionType = TRANSACTION_TYPE.REGULAR,
                Amount = amount,
                Description = description
            };
            transaction.SetDebitAndCredit();
            transactions.Add(transaction);
        }

        return transactions;
    }

}