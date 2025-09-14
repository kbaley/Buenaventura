namespace Buenaventura.MCP.Services;

using Microsoft.Extensions.Options;
using Buenaventura.MCP.Configuration;
using Buenaventura.MCP.Models;
using Npgsql;
using Dapper;

public class CustomerService
{
    private readonly string _connectionString;
    public CustomerService(IOptions<DatabaseConfiguration> dbConfig)
    {
        _connectionString = dbConfig.Value.Buenaventura;
    }

    public IEnumerable<Customer> GetCustomers()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var customers = connection.Query<Customer>("SELECT customer_id AS CustomerId, name AS Name, street_address AS StreetAddress, city AS City, region AS Region, email AS Email, contact_name AS ContactName FROM customers");
            return customers;
        }
    }

    public void AddCustomer(Customer customer)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var sql = "INSERT INTO customers (customer_id, name, street_address, city, region, email, contact_name) VALUES (@CustomerId, @Name, @StreetAddress, @City, @Region, @Email, @ContactName)";
            connection.Execute(sql, customer);
        }
    }
}