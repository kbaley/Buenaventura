using Microsoft.EntityFrameworkCore;
using Buenaventura.Domain;

namespace Buenaventura.Data;
    public class BuenaventuraDbContext(DbContextOptions<BuenaventuraDbContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
        public DbSet<Vendor> Vendors { get;set; }
        public DbSet<Investment> Investments { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<InvestmentCategory> InvestmentCategories { get; set; }
        public DbSet<ReimbursementSettlement> ReimbursementSettlements { get; set; }
        public DbSet<ReimbursementMatch> ReimbursementMatches { get; set; }
        public DbSet<ReimbursementMatchTransaction> ReimbursementMatchTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Transaction>()
                .HasOne(a => a.LeftTransfer)
                .WithOne(t => t.LeftTransaction);
            builder.Entity<Transaction>()
                .HasOne(a => a.RightTransfer)
                .WithOne(t => t.RightTransaction);
            builder.Entity<Transaction>()
                .HasOne(t => t.ReimbursementSettlement)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.ReimbursementSettlementId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<ReimbursementMatchTransaction>()
                .HasKey(t => new { t.ReimbursementMatchId, t.TransactionId });
            builder.Entity<ReimbursementMatchTransaction>()
                .HasOne(t => t.Match)
                .WithMany(m => m.MatchTransactions)
                .HasForeignKey(t => t.ReimbursementMatchId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ReimbursementMatchTransaction>()
                .HasOne(t => t.Transaction)
                .WithMany(t => t.ReimbursementMatchTransactions)
                .HasForeignKey(t => t.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
