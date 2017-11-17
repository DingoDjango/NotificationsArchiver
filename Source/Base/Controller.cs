using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class Controller : ModBase
	{
		private SettingHandle<bool> ClearMastersButton;

		public static SettingHandle<bool> UseSounds;

		public static SettingHandle<bool> ShowPrompts;

		public static Archiver GetArchiver;

		private bool DrawClearArchivesButton(Rect rect)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(rect, this.ClearMastersButton.Title, true, false, true))
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					Utilities.UserFeedbackChain(ModSound.TaskRejected, "Archiver_Error_NoGame".CachedTranslation());

					return false;
				}

				List<MasterArchive> archiveList = GetArchiver.MasterArchives;

				if (archiveList.NullOrEmpty())
				{
					Utilities.UserFeedbackChain(ModSound.TaskRejected, "Archiver_Error_NoArchives".CachedTranslation());

					return false;
				}

				Action action = delegate
				{
					archiveList.RemoveAll(archive => !archive.pinned);

					MainTabWindow_Archive.mustRecacheList = true;
				};

				Utilities.Prompt(action, "Archiver_Prompt_EraseAll".CachedTranslation(), "Archiver_Title_EraseAll".CachedTranslation(), true);
			}

			return false;
		}

		public override string ModIdentifier => "Notifications_Archiver";

		public override void DefsLoaded()
		{
			base.DefsLoaded();

			this.ClearMastersButton = this.Settings.GetHandle("ClearMastersButton", "Archiver_ClearArchives".CachedTranslation(), "Archiver_Tooltip_ClearArchives".CachedTranslation(), false);
			this.ClearMastersButton.Unsaved = true;
			this.ClearMastersButton.CustomDrawer = rect => this.DrawClearArchivesButton(rect);

			UseSounds = this.Settings.GetHandle("UseSounds", "Archiver_UseSounds".CachedTranslation(), "Archiver_Tooltip_UseSounds".CachedTranslation(), true);

			ShowPrompts = this.Settings.GetHandle("ShowPrompts", "Archiver_ShowPrompts".CachedTranslation(), "Archiver_Tooltip_ShowPrompts".CachedTranslation(), true);
		}

		public override void WorldLoaded()
		{
			base.WorldLoaded();

			GetArchiver = Current.Game.GetComponent<Archiver>();
		}
	}
}
