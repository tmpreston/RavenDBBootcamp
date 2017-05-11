using System;
using System.Linq;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit2Lesson5_LoadDocument
	{
		public static void Run()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var query = session
					.Query<Products_ByCategory2.Result, Products_ByCategory2>()
					.Customize(zz => zz.WaitForNonStaleResults());
				//.Include(x => x.Category);

				var results = (
					from result in query
					select result
				).ToList();

				foreach (var result in results)
				{
					//var category = session.Load<Category>(result.Category);
					//Console.WriteLine($"{category.Name} has {result.Count} items.");
					Console.WriteLine($"{result.Category} has {result.Count} items.");
				}
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

		public class Products_ByCategory2 :
			AbstractIndexCreationTask<Product, Products_ByCategory2.Result>
		{
			public class Result
			{
				public string Category { get; set; }
				public int Count { get; set; }
			}

			public Products_ByCategory2()
			{
				Map = products =>
					from product in products
					let categoryName = LoadDocument<Category>(product.Category).Name
					select new
					{
						Category = categoryName,
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
	}
}