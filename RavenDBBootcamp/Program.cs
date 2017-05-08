using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using static System.Console;

namespace RavenDBBootcamp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Task.Run(() => { Utility.DocumentStoreHolder.Store.GetLastWrittenEtag(); });
			Title = "RavenDB Bootcamp";
			WriteLine("Starting.");
			var sw = new Stopwatch();
			sw.Start();

			//Unit1Lesson2.Run();
			//Unit1Lesson3.Run();
			//Unit1Lesson4.Run();
			//Unit1Lesson5.Run();
			//Unit1Lesson6.Run();
			//Unit2Lesson1.Run();
			//new Unit2Lesson2().Execute(Utility.DocumentStoreHolder.Store);
			//Unit2Lesson2.Run();
			//Unit2Lesson3_MultiMapIndex.Run();
			Unit2Lesson4_MapReduce.Run();


			//WriteLine("Total time: {0:0.00}s", sw.ElapsedMilliseconds / 1000);
			WriteLine("Enter to continue.");
			ReadLine();
		}
	}

	public static class Utility
	{
		public static void WriteProperties<T>(this T objectToExpand)
		{
			var prev = ForegroundColor;
			ForegroundColor = ConsoleColor.DarkGreen;
			var sbReturn = new StringBuilder();
			GetSubObjectPropertiesString(objectToExpand, sbReturn, "\t");
			WriteLine(sbReturn.ToString());
			ForegroundColor = prev;
		}

		private static void GetSubObjectPropertiesString<TObjectType>(TObjectType subobject, StringBuilder sbReturn, string tabIndent)
		{
			if (subobject.GetType().Assembly.FullName.StartsWith("System")) return;
			tabIndent = tabIndent ?? string.Empty;
			foreach (var property in typeof(TObjectType).GetProperties())
				try
				{
					if (property.CanRead)
					{
						var propValue = property.GetValue(subobject, null);
						sbReturn.AppendFormat("{2}| {0} - '{1}'\r\n", property.Name, propValue, tabIndent);
						var propType = propValue.GetType();
						if (propValue is IEnumerable && propType != typeof(string))
						{
							var listType = propType.GetTypeInfo().GetGenericArguments()[0];
							var t = typeof(Utility);
							var m = t.GetMethod("GetSubObjectPropertiesStringForList", BindingFlags.Static | BindingFlags.NonPublic);
							var g = m.MakeGenericMethod(listType);
							g.Invoke(null, new[] {propValue, sbReturn, tabIndent});
						}
						else
						{
							GetSubObjectPropertiesString(propValue, sbReturn, tabIndent);
						}
					}
				}
				catch (Exception ex)
				{
				}
		}

		private static void GetSubObjectPropertiesStringForList<TObjectType>(List<TObjectType> subobjectList, StringBuilder sbReturn, string tabIndent)
		{
			if (subobjectList.GetType().Assembly.FullName.StartsWith("System")) return;
			tabIndent = tabIndent ?? string.Empty;
			foreach (var singleItem in subobjectList)
				GetSubObjectPropertiesString(singleItem, sbReturn, tabIndent + "\t..>");
		}

		public static class DocumentStoreHolder
		{
			private static readonly Lazy<IDocumentStore> LazyStore =
				new Lazy<IDocumentStore>(() =>
				{
					var store = new DocumentStore
					{
						ConnectionStringName = "RavenDB"
					};
					var sw = new Stopwatch();
					sw.Start();
					store.Initialize();
					Console.WriteLine("Store.Initialize: {0:0.00}ms", sw.ElapsedMilliseconds);

					var asm = Assembly.GetExecutingAssembly();
					sw.Reset();
					sw.Start();
					IndexCreation.CreateIndexes(asm, store);
					Console.WriteLine("IndexCreation.CreateIndexes: {0:0.00}ms", sw.ElapsedMilliseconds);

					return store;
				});

			public static IDocumentStore Store =>
				LazyStore.Value;
		}

	}
}