using System;
using Raven.Json.Linq;

namespace RavenDBBootcamp
{
    public class Unit3Lesson1_Metadata
    {
        public static void Run()
        {
            using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
            {
                var product = session.Load<Product>("products/1");
                RavenJObject metadata = session.Advanced.GetMetadataFor(product);

                metadata["Last-Modified-By"] = "Tim @ EZIO10";
                session.SaveChanges();

                foreach (var info in metadata)
                {
                    Console.WriteLine($"{info.Key}: {info.Value}");
                }

		 	   var commands = Utility.DocumentStoreHolder.Store.DatabaseCommands;
			   var metadataOnly = commands.Head("suppliers/1").Metadata;
                foreach (var info in metadataOnly)
                    Console.WriteLine($"{info.Key}: {info.Value}");
            }
        }
    }
}