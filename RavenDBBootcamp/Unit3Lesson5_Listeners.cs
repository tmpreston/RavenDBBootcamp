using System;
using System.Linq;
using System.Security.Principal;
using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace RavenDBBootcamp
{
	public class Unit3Lesson5_Listeners
	{
		public static void Run()
		{
			//DisallowChangesByKeyExample();
			//PreventDeleteExample();
			AddAuditExample();
		}

		/// <summary>
		/// </summary>
		/// <remarks>Only effects this client.  Studio still allowed addition.</remarks>
		private static void DisallowChangesByKeyExample()
		{
			Utility.DocumentStoreHolder.Store.Listeners.RegisterListener(new MyDocumentStoreListener());
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var document = new Category()
				{
					Id = "categories/99",
					Name = "Forbidden",
					Description = "Forbidden"
				};

				session.Store(document);
				session.SaveChanges();
			}
		}

		public class MyDocumentStoreListener : IDocumentStoreListener
		{
			public bool BeforeStore(
				string key,
				object entityInstance,
				RavenJObject metadata,
				RavenJObject original)
			{
				Console.WriteLine($"Before storing {key}.");
				var allow = key != "categories/99";
				if (!allow)
					throw new InvalidOperationException($"'{key}' is not an acceptable id.");

				return false;
			}

			public void AfterStore(
				string key, object entityInstance, RavenJObject metadata)
			{
				Console.WriteLine($"After storing {key}.");
			}
		}

		private static void PreventDeleteExample()
		{
			Utility.DocumentStoreHolder.Store.Listeners.RegisterListener(new PreventDeleteListener());
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var anyCategory = session.Query<Category>().First();
				session.Delete(anyCategory);
				session.SaveChanges();
			}
		}

		public class PreventDeleteListener : IDocumentDeleteListener
		{
			public void BeforeDelete(
				string key,
				object entityInstance,
				RavenJObject metadata)
			{
				throw new NotSupportedException();
			}
		}

		private static void AddAuditExample()
		{
			Utility.DocumentStoreHolder.Store.Listeners.RegisterListener(new PreventDeleteListener());
			using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
			{
				var anyCategory = session.Load<Category>("categories/99");
				anyCategory.Description = "New description: " + Guid.NewGuid().ToString("N");
				session.SaveChanges();
			}
		}

		public class AuditStoreListener : IDocumentStoreListener
		{
			public bool BeforeStore(
				string key,
				object entityInstance,
				RavenJObject metadata,
				RavenJObject original)
			{
				var windowsUser = WindowsIdentity.GetCurrent().Name;
				metadata["Last-Modified-By"] = windowsUser;
				metadata["This-Guy-Is-Awesome"] = windowsUser;

				return false;
			}

			public void AfterStore(
				string key,
				object entityInstance,
				RavenJObject metadata)
			{
			}
		}
	}
}