using Raven.Abstractions.Data;

namespace RavenDBBootcamp
{
	public class Unit3Lesson2_Commands
	{
		public static void Run()
		{
			var commands = Utility.DocumentStoreHolder.Store.DatabaseCommands;

			commands.Patch(
				"orders/816",
				new ScriptedPatchRequest
				{
					Script = @"this.Lines.push({
                        'Product': 'products/1',
                        'ProductName': 'Chai',
                        'PricePerUnit': 18,
                        'Quantity': 1,
                        'Discount': 0
                        });"
				});
		}
	}
}