using System;
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
            //Unit1Lesson6.Run();
            Unit2Lesson1.Run();


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

   
}
