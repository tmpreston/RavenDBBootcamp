using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Linq;

namespace RavenDBBootcamp
{
	public class Unit2Lesson7_StatsAndStaleIndexes
	{
		public static void Run()
		{
			//InitTesting();
			Exercise();
		}

		private static void Exercise()
		{
			new Unit2Lesson6_Transformers.Products_ProductAndSupplierName().Execute(Utility.DocumentStoreHolder.Store);

			Console.Title = "Timings demo";
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				RavenQueryStatistics stats;

				var query = session.Query<Order>()
					.Statistics(out stats)
					.Customize(q => q.ShowTimings());

				var orders = (
						from order in query
						orderby order.OrderedAt
						select order
					)
					.ToList();

				var detailedInfo = stats.TimingsInMilliseconds;

				Console.WriteLine($"Orders count : {orders.Count}");
				Console.WriteLine($"Total results: {stats.TotalResults}");
				Console.WriteLine("");

				Console.WriteLine($"Time (ms)  \t Element");
				foreach (var entry in detailedInfo)
				{
					Console.WriteLine($"{entry.Value} \t\t {entry.Key} ");
				}
			}
		}

		private static void InitTesting()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				RavenQueryStatistics stats;

				var orders = (from order in session.Query<Order>()
						.Statistics(out stats)
						//.Customize(zz=>zz.WaitForNonStaleResultsAsOfNow(TimeSpan.FromSeconds(5)))
						.Customize(zz => zz.ShowTimings())
					where order.Company == "companies/1"
					orderby order.OrderedAt
					select order
				).ToList();

				Console.WriteLine($"Index used was: {stats.IndexName}");
				if (stats.IsStale)
				{
					Console.WriteLine("Index was stale.");
				}
				var detailInfo = stats.TimingsInMilliseconds;
				detailInfo.WriteContents();
			}
		}
	}
}