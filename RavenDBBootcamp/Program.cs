﻿using System;
using static System.Console;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;

namespace RavenDBBootcamp
{
	class Program
	{
		static void Main(string[] args)
		{
		    Task.Run(() =>
		    {
		        Utility.DocumentStoreHolder.Store.GetLastWrittenEtag();
		    });
		    Console.Title = "RavenDB Bootcamp";
			Console.WriteLine("Starting.");

			//Unit1Lesson2.Run();
			//Unit1Lesson3.Run();
			//Unit1Lesson4.Run();
			//Unit1Lesson5.Run();
            Unit1Lesson6.Run();

			Console.WriteLine("Enter to continue.");
			Console.ReadLine();
		}
	}

	public static class Utility
	{
		public static void WriteProperties<T>(this T objectToExpand)
		{
			var prev = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			var sbReturn = new StringBuilder();
			GetSubObjectPropertiesString(objectToExpand, sbReturn, "\t");
			Console.WriteLine(sbReturn.ToString());
			Console.ForegroundColor = prev;
		}

		private static void GetSubObjectPropertiesString<TObjectType>(TObjectType subobject, StringBuilder sbReturn, string tabIndent)
		{
			if (subobject.GetType().Assembly.FullName.StartsWith("System")) return;
			tabIndent = tabIndent ?? String.Empty;
			foreach (PropertyInfo property in typeof(TObjectType).GetProperties())
			{
				try
				{
					if (property.CanRead)
					{
						var propValue = property.GetValue(subobject, null);
						sbReturn.AppendFormat("{2}| {0} - '{1}'\r\n", property.Name, propValue, tabIndent);
						GetSubObjectPropertiesString(propValue, sbReturn, tabIndent + "\t");
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public static class DocumentStoreHolder
		{
			private static readonly Lazy<IDocumentStore> LazyStore =
				new Lazy<IDocumentStore>(() =>
				{
					var store = new DocumentStore
					{
						ConnectionStringName = "RavenDB",
					};

					return store.Initialize();
				});

			public static IDocumentStore Store =>
				LazyStore.Value;
		}
	}

	public static class Unit1Lesson2
	{

		public static void Run()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var p = session.Load<dynamic>("products/1");
				System.Console.WriteLine(p.Name);
			}
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var p = session.Load<Product>("products/70");
				p.WriteProperties();
			}
		}
	}

	public static class Unit1Lesson3
	{
		public static void Run()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var p = session.Load<Product>("products/1");
				System.Console.WriteLine(p.Name);
			}
		}
	}

	public static class Unit1Lesson4
	{
		public static void Run()
		{
			//using (var session = Unit1Lesson3.DocumentStoreHolder.Store.OpenSession())
			//{
			//	var p1 = session.Load<Product>("products/1");
			//	var p2 = session.Load<Product>("products/1");
			//	Debug.Assert(ReferenceEquals(p1, p2));
			//	Console.WriteLine("Assert worked!");
			//}
			//using (var session = Unit1Lesson3.DocumentStoreHolder.Store.OpenSession())
			//{
			//	//Product[] products = session.Load<Product>(new[]
			//	//{
			//	//	"products/1",
			//	//	"products/2",
			//	//	"products/3"
			//	//});
			//	Product[] products = session.Load<Product>(1, 2, 3);
			//	foreach (var product in products)
			//	{
			//		product.WriteProperties();
			//	}
			//}
			//using (var session = Unit1Lesson3.DocumentStoreHolder.Store.OpenSession())
			//{
			//	var items = session.Load<dynamic>(new[] {"products/1", "categories/2"});

			//	Product p = (Product)items[0];
			//	Category c = (Category)items[1];

			//	p.WriteProperties();
			//	c.WriteProperties();
			//}
			//using (var session = Unit1Lesson3.DocumentStoreHolder.Store.OpenSession())
			//{
			//	Console.WriteLine("Requests: {0}", session.Advanced.NumberOfRequests);
			//	var p = session
			//		.Include<Product>(x => x.Category)
			//		.Load(1);

			//	var c = session.Load<Category>(p.Category);
			//	Console.WriteLine("Requests: {0}", session.Advanced.NumberOfRequests);
			//	p.WriteProperties();
			//	c.WriteProperties();
			//	Console.WriteLine("Requests: {0}", session.Advanced.NumberOfRequests);
			//}
		}

		public static void ComplexExample()
		{
			while (true)
			{
				WriteLine("Please, enter an order # (0 to exit): ");

				int orderNumber;
				if (!int.TryParse(ReadLine(), out orderNumber))
				{
					WriteLine("Order # is invalid.");
					continue;
				}

				if (orderNumber == 0) break;

				PrintOrder(orderNumber);
			}

			WriteLine("Goodbye!");
		}

		private static void PrintOrder(int orderNumber)
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var order = session
					.Include<Order>(o => o.Company)
					.Include(o => o.Employee)
					.Include(o => o.Lines.Select(l => l.Product))
					.Load(orderNumber);

				if (order == null)
				{
					WriteLine($"Order #{orderNumber} not found.");
					return;
				}

				WriteLine($"Order #{orderNumber}");

				var c = session.Load<Company>(order.Company);
				WriteLine($"Company : {c.Id} - {c.Name}");

				var e = session.Load<Employee>(order.Employee);
				WriteLine($"Employee: {e.Id} - {e.LastName}, {e.FirstName}");

				foreach (var orderLine in order.Lines)
				{
					var p = session.Load<Product>(orderLine.Product);
					WriteLine($"   - {orderLine.ProductName}," +
								 $" {orderLine.Quantity} x {p.QuantityPerUnit}");
				}
			}
		}
	}

	public static class Unit1Lesson5
	{
		public static void Run()
		{
			while (true)
			{
				WriteLine("Please, enter a company id (0 to exit): ");

				int companyId;
				if (!int.TryParse(ReadLine(), out companyId))
				{
					WriteLine("Order # is invalid.");
					continue;
				}

				if (companyId == 0) break;

				QueryCompanyOrders(companyId);
			}

			WriteLine("Goodbye!");
		}

		private static void QueryCompanyOrders(int companyId)
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var orders = (
					from order in session.Query<Order>()
						.Include(o => o.Company)
					where order.Company == $"companies/{companyId}"
					select order
				).ToList();

				var company = session.Load<Company>(companyId);

				if (company == null)
				{
					WriteLine("Company not found.");
					return;
				}

				WriteLine($"Orders for {company.Name}");

				foreach (var order in orders)
				{
					WriteLine($"{order.Id} - {order.OrderedAt}");
				}
			}
		}
	}

    public static class Unit1Lesson6
    {
        public static void Run()
        {
            // storing a new document
            string categoryId;
            using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
            {
                var newCategory = new Category
                {
                    Name = "My New Category",
                    Description = "Description of the new category"
                };

                session.Store(newCategory);
                categoryId = newCategory.Id;
                session.SaveChanges();
            }

            // loading and modifying
            using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
            {
                var storedCategory = session
                    .Load<Category>(categoryId);

                storedCategory.Name = "abcd";

                session.SaveChanges();
            }

            // deleting
            using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
            {
                session.Delete(categoryId);
                session.SaveChanges();
            }
        }
    }

}
