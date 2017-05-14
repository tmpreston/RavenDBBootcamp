using System;
using System.Linq;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
	public class Unit2Lesson6_Transformers
	{
		public static void Run()
		{
			//LoadByCompany();
			//LoadByTransformer();
			LoadByTransformerReferenceMultipleDocumets();
		}

		private static void LoadByCompany()
		{
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var company = session.Load<Company>("companies/89");
				Console.WriteLine(company.Name);
			}
		}

		private static void LoadByTransformer()
		{
			new Company_JustName().Execute(Utility.DocumentStoreHolder.Store);
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var company = session.Load<Company_JustName, Company>("companies/89");
				Console.WriteLine(company.Name);
			}
		}

		private static void LoadByTransformerReferenceMultipleDocumets()
		{
			new Products_ProductAndSupplierName().Execute(Utility.DocumentStoreHolder.Store);
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var product = session.Load<
					Products_ProductAndSupplierName,
					Products_ProductAndSupplierName.Result
				>("products/1");

				Console.WriteLine($"{product.ProductName} from {product.SupplierName}");
			}
		}

		public class Company_JustName : AbstractTransformerCreationTask<Company>
		{
			public Company_JustName()
			{
				TransformResults = companies =>
					from company in companies
					select new { company.Name };
			}
		}

		public class Products_ProductAndSupplierName :
			AbstractTransformerCreationTask<Product>
		{
			public class Result
			{
				public string ProductName { get; set; }
				public string SupplierName { get; set; }
			}

			public Products_ProductAndSupplierName()
			{
				TransformResults = products =>
					from product in products
					let category = LoadDocument<Supplier>(product.Supplier)
					select new
					{
						ProductName = product.Name,
						SupplierName = category.Name
					};
			}
		}
	}
}