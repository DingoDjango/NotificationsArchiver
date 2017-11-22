using System;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace Notifications_Archiver
{
	[HarmonyPatch(typeof(LetterStack))]
	[HarmonyPatch(nameof(LetterStack.ReceiveLetter))]
	[HarmonyPatch(new Type[] { typeof(Letter), typeof(string) })]
	public class Patch_LetterStack
	{
		public static void Postfix(Letter let)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Current.Game.GetComponent<Archiver>()?.NewArchive(let, "", GlobalTargetInfo.Invalid);
			}
		}
	}

	[HarmonyPatch(typeof(Messages))]
	[HarmonyPatch(nameof(Messages.Message))]
	[HarmonyPatch(new Type[] { typeof(string), typeof(GlobalTargetInfo), typeof(MessageTypeDef) })]
	public static class Patch_Messages_TargetedMessage
	{
		public static void Postfix(string text, GlobalTargetInfo lookTarget)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Current.Game.GetComponent<Archiver>()?.NewArchive(null, text, lookTarget);
			}
		}
	}

	[HarmonyPatch(typeof(Messages))]
	[HarmonyPatch(nameof(Messages.Message))]
	[HarmonyPatch(new Type[] { typeof(string), typeof(MessageTypeDef) })]
	public static class Patch_Messages_PlainMessage
	{
		public static void Postfix(string text)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Current.Game.GetComponent<Archiver>()?.NewArchive(null, text, GlobalTargetInfo.Invalid);
			}
		}
	}
}
