using DapperUtils.ToreAurstadIT;
using DapperUtils.ToreAurstadIT.Tests;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DapperExtensions.Client
{
    class Program
    {
        private static IConfigurationRoot Config { get; set; }

        private static IDbConnection Connection { get; set; }

        static void Main(string[] args)
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

            Connection.Close();
            Connection.Dispose();
            Connection = null;

            Assert.AreEqual(firstRow.EmployeeID + firstRow.Title + firstRow.OrderID + firstRow.ShipName + firstRow.ProductID.ToString() + firstRow.ProductName + firstRow.CategoryID + firstRow.CategoryName + firstRow.SupplierID + firstRow.CompanyName, "5Sales Manager10248Vins et alcools Chevalier11Queso Cabrales4Dairy Products5Cooperativa de Quesos 'Las Cabras'");
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
