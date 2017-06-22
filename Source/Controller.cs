using System.Reflection;
using Harmony;
using Verse;

namespace Notifications_Archiver
{
	public class Controller : Mod
	{
		public Controller(ModContentPack content) : base(content)
		{
			var harmony = HarmonyInstance.Create("dingo.rimworld.notifications_archiver");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
