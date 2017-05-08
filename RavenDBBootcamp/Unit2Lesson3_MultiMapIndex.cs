using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit2Lesson3_MultiMapIndex
	{
		public static void Run()
		{
			Console.Title = Console.Title + " - Multi-map sample";
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				while (true)
				{
					Console.Write("\nSearch terms (or 0/empty to exit) : ");
					var searchTerms = Console.ReadLine();
					if (searchTerms.Trim() == "0" || searchTerms.Trim() == string.Empty)
					{
						break;
					}

					foreach (var result in People_Search.Search(session, searchTerms))
					{
						Console.WriteLine($"{result.SourceId}\t{result.Type}\t{result.Name}");
					}
				}
			}
		}

		public class People_Search : AbstractMultiMapIndexCreationTask<People_Search.Result>
		{
			public class Result
			{
				public string SourceId { get; set; }
				public string Name { get; set; }
				public string Type { get; set; }
			}

			public People_Search()
			{
				AddMap<Company>(companies =>
					from company in companies
					select new Result
					{
						SourceId = company.Id,
						Name = company.Contact.Name,
						Type = "Company's contact"
					}
				);

				AddMap<Supplier>(suppliers =>
					from supplier in suppliers
					select new Result
					{
						SourceId = supplier.Id,
						Name = supplier.Contact.Name,
						Type = "Supplier's contact"
					}
				);

				AddMap<Employee>(employees =>
					from employee in employees
					select new Result
					{
						SourceId = employee.Id,
						Name = $"{employee.FirstName} {employee.LastName}",
						Type = "Employee"
					}
				);

				Index(entry => entry.Name, FieldIndexing.Analyzed);

				Store(entry => entry.SourceId, FieldStorage.Yes);
				Store(entry => entry.Name, FieldStorage.Yes);
				Store(entry => entry.Type, FieldStorage.Yes);
			}

			public static IEnumerable<Result> Search(IDocumentSession session,string searchTerms)
			{
				var results = session.Query<Result, People_Search>()
					.Search(
						r => r.Name,
						searchTerms,
						escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
					.ProjectFromIndexFieldsInto<Result>();

				return results;
			}
		}
	}
}