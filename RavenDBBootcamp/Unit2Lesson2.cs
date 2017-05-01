using System;
using System.Linq;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit2Lesson2 : AbstractIndexCreationTask<Region>
	{
		public Unit2Lesson2()
		{
			Map = regions =>
				from singleRegion in regions
				select new
				{
					singleRegion.Name
				};
		}

		public static void Run()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var results = session
					.Query<Region, Unit2Lesson2>()
					.Where(x => x.Name.StartsWith("S", StringComparison.InvariantCultureIgnoreCase) || x.Name.StartsWith("N"))
					.ToList();

				foreach (var res in results)
					res.WriteProperties();
			}
		}
	}
}