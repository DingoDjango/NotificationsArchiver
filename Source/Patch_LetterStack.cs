using System;
using Harmony;
using Verse;

namespace Notifications_Archiver
{
	[HarmonyPatch(typeof(LetterStack))]
	[HarmonyPatch("ReceiveLetter")]
	[HarmonyPatch(new Type[] { typeof(Letter), typeof(string) })]
	public class Notifications_Archiver_LetterStack_Patch
	{
		public static void Postfix(Letter let, string debugInfo)
		{
			var logger = Current.Game.GetComponent<Logger>();

			if (logger != null)
			{
				logger.NotifyNewLetter(let);
			}
		}
	}
}
