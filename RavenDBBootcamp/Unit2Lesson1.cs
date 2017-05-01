using System;
using System.Linq;
using Raven.Client.Linq;

namespace RavenDBBootcamp
{
    public class Unit2Lesson1
    {
        public static void Run()
        {
            using (var session = Utility.DocumentStoreHolder.Store.OpenSession())
            {
                var ordersIds = (
                    from order in session.Query<Order>()
                    where order.Company == "companies/1"
                    orderby order.OrderedAt
                    select order.Id
                ).ToList();

                foreach (var id in ordersIds)
                    Console.WriteLine(id);
            }
        }
    }
}