using System;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace Notifications_Archiver
{
	[HarmonyPatch(typeof(Messages))]
	[HarmonyPatch("Message")]
	[HarmonyPatch(new Type[] { typeof(string), typeof(GlobalTargetInfo), typeof(MessageSound) })]
	public static class Notifications_Archiver_Messages_Patch_textAndTarget
	{
		public static void Postfix(string text, GlobalTargetInfo lookTarget, MessageSound sound)
		{
			var msg = new ArchivedMessage(text, lookTarget);

			var logger = Current.Game.GetComponent<Logger>();

			if (logger != null)
			{
				logger.NotifyNewArchivedMessage(msg);
			}
		}
	}

	[HarmonyPatch(typeof(Messages))]
	[HarmonyPatch("Message")]
	[HarmonyPatch(new Type[] { typeof(string), typeof(MessageSound) })]
	public static class Notifications_Archiver_Messages_Patch_textOnly
	{
		public static void Postfix(string text, MessageSound sound)
		{
			var plainMsg = new ArchivedMessage(text, GlobalTargetInfo.Invalid);

			var logger = Current.Game.GetComponent<Logger>();

			if (logger != null)
			{
				logger.NotifyNewArchivedMessage(plainMsg);
			}
		}
	}
}
