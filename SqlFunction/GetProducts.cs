using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SqlFunction
{
    public static class GetProducts
    {
        [FunctionName("GetProduct")]
        public static async Task<IActionResult> RunProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var productId = int.Parse(req.Query["id"]);

            var statement = $"SELECT ProductID, ProductName, Quantity FROM Products WHERE ProductID = {productId}";

            var connection = GetConnection();
            connection.Open();

            var command = new SqlCommand(statement, connection);

            Product product;

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    product = new Product()
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Quantity = reader.GetInt32(2)
                    };
                }

                connection.Close();
                return new OkObjectResult(product);
            }
            catch (Exception)
            {
                var response = "No records found";
                connection.Close();
                return new OkObjectResult(response);
            }
        }

        [FunctionName("GetProducts")]
        public static async Task<IActionResult> RunProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var connection = GetConnection();
            var productList = new List<Product>();
            var statement = "SELECT ProductID, ProductName, Quantity FROM Products";

            connection.Open();

            var command = new SqlCommand(statement, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var product = new Product()
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Quantity = reader.GetInt32(2)
                    };

                    productList.Add(product);
                }
            }

            connection.Close();

            return new OkObjectResult(productList);
        }

        private static SqlConnection GetConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_SQLConnectionString");
            return new SqlConnection(connectionString);
        }
    }
}
