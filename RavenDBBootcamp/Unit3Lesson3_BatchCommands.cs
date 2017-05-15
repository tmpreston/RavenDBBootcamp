using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit3Lesson3_BatchCommands
	{
		public static void Run()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				DisplayProducts("Before batch update.", session);

				var commands = Utility.DocumentStoreHolder.Store.DatabaseCommands;
				var operation = commands.UpdateByIndex(
					"Products/ByDiscontinued",
					new IndexQuery { Query = "Discontinued:false" },
					new ScriptedPatchRequest
					{
						Script =
							@"this.PricePerUnit = this.PricePerUnit * 1.1"
					});

				operation.WaitForCompletion();
				Console.WriteLine("All active products had Price per unit increased in 10%");
				DisplayProducts("After batch update.", session);
			}
		}

		public static void DisplayProducts(string info, IDocumentSession session)
		{
			Console.WriteLine(info);
			var products = session.Query<Product, Products_ByDiscontinued>()
				.Customize(zz => zz.WaitForNonStaleResults(TimeSpan.FromMinutes(1)))
				.Where(zz => zz.Discontinued == false)
				.Take(5)
				.ToList();
			foreach (var product in products)
			{
				Utility.WriteProperties(product);
			}
			Console.WriteLine("---");
		}

		public class Products_ByDiscontinued : AbstractIndexCreationTask<Product>
		{
			public Products_ByDiscontinued()
			{
				Map = (products) =>
					from p in products
					select new { p.Discontinued };
			}
		}
	}
}