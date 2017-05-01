using System.Linq;
using System.Runtime.CompilerServices;
using Raven.Client.Indexes;

namespace RavenDBBootcamp
{
    public class Unit2Lesson2 : AbstractIndexCreationTask<Region>
    {
        public Unit2Lesson2()
        {
            Map = (regions) =>
           from singleRegion in regions
           select new
           {
               TheName = singleRegion.Name,
               HowMany = singleRegion.Territories.Count,
               BestOrFirst = singleRegion.Territories.FirstOrDefault(),
           };
        }
    }
}