using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit2Lesson4_MapReduce
	{
		public static void Run()
		{
			//Example1();
			Example2();
		}

		private static void Example1()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var results = session
					.Query<Products_ByCategory.Result, Products_ByCategory>()
					.Include(x => x.Category)
					.ToList();

				foreach (var result in results)
				{
					var category = session.Load<Category>(result.Category);
					Console.WriteLine($"{category.Name} has {result.Count} items.");
				}
			}
		}

		private static void Example2()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var query = session
					.Query<Employees_SalesPerMonth.Result, Employees_SalesPerMonth>()
					.Include(x => x.Employee)
					.Customize(zz => zz.WaitForNonStaleResults());

				var results = (
					from result in query
					where result.Month == "1998-03"
					orderby result.TotalSales descending
					select result
				).ToList();

				foreach (var result in results)
				{
					var employee = session.Load<Employee>(result.Employee);
					Console.WriteLine(
						$"{employee.FirstName} {employee.LastName}"
						+ $" made {result.TotalSales} sales.");
				}
			}
		}

		public class Products_ByCategory :
			AbstractIndexCreationTask<Product, Products_ByCategory.Result>
		{
			public class Result
			{
				public string Category { get; set; }
				public int Count { get; set; }
			}

			public Products_ByCategory()
			{
				Map = products =>
					from product in products
					select new
					{
						Category = product.Category,
						Count = 1
					};

				Reduce = results =>
					from result in results
					group result by result.Category into g
					select new
					{
						Category = g.Key,
						Count = g.Sum(x => x.Count)
					};
			}
		}

		public class Category
		{
			public string Name { get; set; }
		}

		public class Product
		{
			public string Category { get; set; }
		}

		public class Employees_SalesPerMonth :
			AbstractIndexCreationTask<Order, Employees_SalesPerMonth.Result>
		{
			public class Result
			{
				public string Employee { get; set; }
				public string Month { get; set; }
				public int TotalSales { get; set; }
			}

			public Employees_SalesPerMonth()
			{
				Map = orders =>
					from order in orders
					select new
					{
						order.Employee,
						Month = order.OrderedAt.ToString("yyyy-MM"),
						TotalSales = 1
					};

				Reduce = results =>
					from result in results
					group result by new
					{
						result.Employee,
						result.Month
					}
					into g
					select new
					{
						g.Key.Employee,
						g.Key.Month,
						TotalSales = g.Sum(x => x.TotalSales)
					};
			}
		}

		public class Order
		{
			public string Employee { get; }
			public DateTime OrderedAt { get; }
		}

		public class Employee
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
		}
	}
}