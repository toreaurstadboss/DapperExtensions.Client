using Dapper;
using DapperUtils.ToreAurstadIT;
using DapperUtils.ToreAurstadIT.Tests;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToreAurstadIT.DapperUtils;

namespace DapperExtensions.Client
{
    class Program
    {
        private static IConfigurationRoot Config { get; set; }

        private static IDbConnection Connection { get; set; }

        static async Task Main(string[] args)
        {
            Config = SetupConfigurationFile();
            Connection = new SqlConnection(Config.GetConnectionString("Northwind"));
            Connection.Open();

            Console.WriteLine("Testing out ");
            var joinedproductsandcategory = Connection.InnerJoin(
             (Order o, OrderDetail od) => o.OrderID == od.OrderID,
             (Order o, Employee e) => o.EmployeeID == e.EmployeeID,
             (OrderDetail od, Product p) => od.ProductID == p.ProductID,
             (Product p, Category c) => p.CategoryID == c.CategoryID,
             (Product p, Supplier s) => p.SupplierID == s.SupplierID);
            dynamic firstRow = joinedproductsandcategory.ElementAt(0);
       
            Assert.AreEqual(firstRow.EmployeeID + firstRow.Title + firstRow.OrderID + firstRow.ShipName + firstRow.ProductID.ToString() + firstRow.ProductName + firstRow.CategoryID + firstRow.CategoryName + firstRow.SupplierID + firstRow.CompanyName, "5Sales Manager10248Vins et alcools Chevalier11Queso Cabrales4Dairy Products5Cooperativa de Quesos 'Las Cabras'");
      
            var filteredJoindProductsAndCategory = Connection.InnerJoin((Product p, Category c) => p.CategoryID == c.CategoryID,
                new Tuple<string, Type>[] { new Tuple<string, Type>("CategoryID=5", typeof(Category)) });

            dynamic firstRowFiltered = filteredJoindProductsAndCategory;
            Assert.AreEqual(firstRow.EmployeeID + firstRow.Title + firstRow.OrderID + firstRow.ShipName + firstRow.ProductID.ToString() + firstRow.ProductName + firstRow.CategoryID + firstRow.CategoryName + firstRow.SupplierID + firstRow.CompanyName, "5Sales Manager10248Vins et alcools Chevalier11Queso Cabrales4Dairy Products5Cooperativa de Quesos 'Las Cabras'");

            await TestOutCrud();

            Connection.Close();
            Connection.Dispose();
            Connection = null;
        }

        private async static Task TestOutCrud()
        {
            var product = new Product
            {
                ProductName = "Misvaerost",
                SupplierID = 15,
                CategoryID = 4,
                QuantityPerUnit = "300 g",
                UnitPrice = 2.70M,
                UnitsInStock = 130,
                UnitsOnOrder = 0,
                ReorderLevel = 20,
                Discontinued = false
            };
            var anotherProduct = new Product
            {
                ProductName = "Jarslbergost",
                SupplierID = 15,
                CategoryID = 4,
                QuantityPerUnit = "170 g",
                UnitPrice = 2.80M,
                UnitsInStock = 70,
                UnitsOnOrder = 0,
                ReorderLevel = 10,
                Discontinued = false
            };

            var products = new List<Product> { product, anotherProduct };
            var productIds = await Connection.InsertMany(products);
            productIds.Cast<int>().Count().Should().Be(2, "Expected to insert two rows into the DB.");
            productIds.Cast<int>().All(p => p > 0).Should().Be(true, "Expected to insert two rows into the DB with non-zero ids");

            var updatePropertyBag = new Dictionary<string, object>
            {
                { "UnitPrice", 133 },
                { "UnitsInStock", 192 }
            };

            products[0].ProductID = productIds.Cast<int>().ElementAt(0);
            products[1].ProductID = productIds.Cast<int>().ElementAt(1);

            var updatedProductsIds = await Connection.UpdateMany(products, updatePropertyBag);

            foreach (var productId in productIds.Cast<int>())
            {
                var productAfterUpdateToDelete = Connection.Query<Product>($"select * from Products where ProductID = {productId}").First();
                productAfterUpdateToDelete.UnitPrice.Should().Be(133);
                productAfterUpdateToDelete.UnitsInStock.Should().Be(192);
                await Connection.Delete(productAfterUpdateToDelete);
            }
        }

        private static IConfigurationRoot SetupConfigurationFile()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            return configuration;
        }

    }
}
