using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Notifications_Archiver
{
	[StaticConstructorOnStartup]
	public static class Utilities
	{
		public static readonly Texture2D TargetedMessageIcon = LetterDefOf.ItemStashFeeDemand.Icon;

		public static readonly Texture2D PinTexture = DingoUtils.GetHQTexture("Pin"); //Must be attributed to Freepik (freepik.com)

		public static readonly SoundDef TaskCompleted = MessageTypeDefOf.PositiveEvent.sound;

		public static readonly SoundDef TaskRejected = SoundDefOf.ClickReject;

		public static void TooltipRegion(Rect rect, Vector2 mousePosition, string tooltip)
		{
			if (rect.Contains(mousePosition))
			{
				Widgets.DrawHighlight(rect);

				TooltipHandler.TipRegion(rect, tooltip);
			}
		}

		public static void UserFeedbackChain(ModSound sound, string message = "")
		{
			if (Controller.UseSounds)
			{
				SoundDef audio = null;

				switch (sound)
				{
					case ModSound.TaskCompleted:
						audio = TaskCompleted;
						break;
					case ModSound.TaskRejected:
						audio = TaskRejected;
						break;
				}

				audio.PlayOneShotOnCamera(null);
			}

			if (message != "")
			{
				Messages.Message(message, MessageTypeDefOf.SilentInput);
			}
		}

		public static void DoFloatMenu(MasterArchive master)
		{
			List<MasterArchive> allArchives = Controller.GetArchiver.MasterArchives;
			string pinLabel = !master.pinned ? "Archiver_Pin".CachedTranslation() : "Archiver_Unpin".CachedTranslation();

			Action removeCurrent = delegate
			{
				if (!master.pinned)
				{
					allArchives.Remove(master);

					MainTabWindow_Archive.mustRecacheList = true;
				}

				else
				{
					UserFeedbackChain(ModSound.TaskRejected, "Archiver_Error_TriedToDeletePinned".CachedTranslation());
				}
			};

			Action removeOlder = delegate
			{
				for (int j = allArchives.IndexOf(master) - 1; j >= 0; j--)
				{
					if (!allArchives[j].pinned)
					{
						allArchives.RemoveAt(j);
					}
				}

				MainTabWindow_Archive.mustRecacheList = true;
			};

			FloatMenuOption togglePinned = new FloatMenuOption(pinLabel, delegate
			{
				master.pinned = !master.pinned;

				UserFeedbackChain(ModSound.TaskCompleted);
			});

			FloatMenuOption deleteArchive = new FloatMenuOption("Archiver_DeleteCurrent".CachedTranslation(), delegate
			{
				Prompt(removeCurrent, "Archiver_Prompt_DeleteCurrent".CachedTranslation(), "Archiver_Title_DeleteCurrent".CachedTranslation());
			});

			FloatMenuOption deleteOlder = new FloatMenuOption("Archiver_DeleteOlder".CachedTranslation(), delegate
			{
				Prompt(removeOlder, "Archiver_Prompt_DeleteOlder".CachedTranslation(), "Archiver_Title_DeleteOlder".CachedTranslation());
			});

			Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption> { togglePinned, deleteArchive, deleteOlder }));
		}

		public static void Prompt(Action onAccept, string content, string title, bool forcePrompt = false)
		{
			if (forcePrompt || Controller.ShowPrompts)
			{
				DiaOption accept = new DiaOption("Archiver_Accept".CachedTranslation())
				{
					action = onAccept,
					resolveTree = true
				};

				DiaOption reject = new DiaOption("Archiver_Cancel".CachedTranslation())
				{
					resolveTree = true
				};

				DiaNode prompt = new DiaNode(content)
				{
					options = new List<DiaOption> { accept, reject }
				};

				Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
			}

			else
			{
				onAccept();
			}
		}
	}
}
