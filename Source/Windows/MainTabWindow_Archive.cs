using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class MainTabWindow_Archive : MainTabWindow
	{
		private const float WindowWidth = 600f;

		private const float ShortSpacing = 10f;

		private const float SpaceForScrollbar = 22f;

		private const float ListItemHeight = 30f;

		private const float IconHeight = 16f;

		private const float IconWidth = 20f;

		private const float DateWidth = 100f; //Relatively wide to account for some languages' date strings

		private const int MaxArchiveTextLength = 66;

		private float textFilterLabelLength = Text.CalcSize("Archiver_TextFilter".CachedTranslation()).x;

		private Archiver archiver = Controller.GetArchiver;

		private List<MasterArchive> cachedArchives = new List<MasterArchive>();

		private string listFilter = string.Empty;

		private Vector2 scrollPosition = Vector2.zero;

		public static bool mustRecacheList = true;

		private string MasterDate(MasterArchive master)
		{
			if (master.dateDayofSeason != -1 && master.dateQuadrum != Quadrum.Undefined && master.dateYear != -1)
			{
				return "Archiver_FormattedDate".CachedTranslation(new object[]
				{
					master.dateQuadrum.Label(),
					master.dateDayofSeason,
					master.dateYear
				});
			}

			return "Archiver_UnknownDate".CachedTranslation();
		}

		private List<MasterArchive> GetCachedArchives()
		{
			Predicate<string> MatchesFilter = text => text.IndexOf(this.listFilter, StringComparison.OrdinalIgnoreCase) >= 0;

			return this.archiver.MasterArchives.FindAll(archive =>
			(this.archiver.ShowLetters && archive.type == ArchiveType.Letter || this.archiver.ShowMessages && archive.type == ArchiveType.Message)
			&& (this.listFilter == string.Empty ||
				MatchesFilter(this.MasterDate(archive)) ||
				MatchesFilter(archive.Label) ||
				MatchesFilter(archive.Text))
			);
		}

		private void DrawListItem(Rect rect, MasterArchive master)
		{
			float iconSpacing = (rect.height - IconHeight) / 2f;

			Rect pinRect = new Rect(rect.x + ShortSpacing / 2f, rect.y + iconSpacing, IconHeight, IconHeight);
			Rect iconRect = new Rect(pinRect.xMax + ShortSpacing / 2f, rect.y + iconSpacing, IconWidth, IconHeight);
			Rect dateRect = new Rect(iconRect.xMax + ShortSpacing, rect.y, DateWidth, rect.height);
			Rect labelRect = new Rect(dateRect.xMax + ShortSpacing, rect.y, rect.width - dateRect.width - iconRect.width - pinRect.width - 3f * ShortSpacing, rect.height);

			if (master.pinned)
			{
				GUI.DrawTexture(pinRect, Utilities.PinTexture);
			}

			//Draw icon
			if (master.type == ArchiveType.Letter)
			{
				GUI.color = master.letter.def.color;
				GUI.DrawTexture(iconRect, master.letter.def.Icon);
				GUI.color = Color.white; //Reset
			}

			else if (master.type == ArchiveType.Message && master.lookTarget.IsValid)
			{
				GUI.DrawTexture(iconRect, Utilities.TargetedMessageIcon);
			}

			//Draw date and label
			string masterLabel = master.Label;
			string drawLabel = masterLabel.Length <= MaxArchiveTextLength ? masterLabel : masterLabel.Substring(0, MaxArchiveTextLength - 3) + "...";

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(dateRect, this.MasterDate(master));
			Widgets.Label(labelRect, drawLabel);
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		public override bool CausesMessageBackground() => true;

		public override Vector2 RequestedTabSize => new Vector2(WindowWidth, UI.screenHeight * 0.85f);

		public override void WindowOnGUI()
		{
			if (mustRecacheList)
			{
				this.cachedArchives = this.GetCachedArchives();

				mustRecacheList = false;
			}

			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			Rect enableArchivingRect = new Rect(inRect.x, inRect.y, inRect.width / 3f - ShortSpacing, Text.LineHeight * 1.2f);
			Rect showLettersRect = new Rect(enableArchivingRect.xMax + ShortSpacing, inRect.y, enableArchivingRect.width, enableArchivingRect.height);
			Rect showMessagesRect = new Rect(showLettersRect.xMax + ShortSpacing, inRect.y, enableArchivingRect.width, enableArchivingRect.height);

			Rect filterLabelRect = new Rect(inRect.x, inRect.y + enableArchivingRect.height + ShortSpacing, this.textFilterLabelLength, enableArchivingRect.height);
			Rect filterInputRect = new Rect(filterLabelRect.xMax, filterLabelRect.y, inRect.width - filterLabelRect.width - ShortSpacing, enableArchivingRect.height);

			Rect scrollviewOutRect = new Rect(inRect.x, filterLabelRect.yMax + ShortSpacing, inRect.width, inRect.height - 2f * (enableArchivingRect.height + ShortSpacing));
			Rect scrollviewInRect = new Rect(inRect.x, scrollviewOutRect.y, inRect.width - SpaceForScrollbar, this.cachedArchives.Count * ListItemHeight);

			//Cache current options values
			bool tempShowLetters = this.archiver.ShowLetters;
			bool tempShowMessages = this.archiver.ShowMessages;
			string tempListFilter = this.listFilter;

			//Draw options with user input
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.CheckboxLabeled(enableArchivingRect, "Archiver_EnableArchiving".CachedTranslation(), ref this.archiver.EnableArchiving);
			Widgets.CheckboxLabeled(showLettersRect, "Archiver_ShowLetters".CachedTranslation(), ref this.archiver.ShowLetters);
			Widgets.CheckboxLabeled(showMessagesRect, "Archiver_ShowMessages".CachedTranslation(), ref this.archiver.ShowMessages);
			this.listFilter = Widgets.TextField(filterInputRect, this.listFilter);
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Check if options were changed
			if (tempShowLetters != this.archiver.ShowLetters || tempShowMessages != this.archiver.ShowMessages || tempListFilter != this.listFilter)
			{
				mustRecacheList = true;
			}

			//Draw other UI elements
			if (Event.current.type == EventType.Repaint)
			{
				Vector2 mousePosition = Event.current.mousePosition;

				Widgets.Label(filterLabelRect, "Archiver_TextFilter".CachedTranslation());
				Utilities.TooltipRegion(enableArchivingRect, mousePosition, "Archiver_Tooltip_EnableArchiving".CachedTranslation());
				Utilities.TooltipRegion(showLettersRect, mousePosition, "Archiver_Tooltip_ShowLetters".CachedTranslation());
				Utilities.TooltipRegion(showMessagesRect, mousePosition, "Archiver_Tooltip_ShowMessages".CachedTranslation());
				Utilities.TooltipRegion(filterLabelRect, mousePosition, "Archiver_Tooltip_TextFilter".CachedTranslation());
			}

			//Draw list
			Widgets.BeginScrollView(scrollviewOutRect, ref this.scrollPosition, scrollviewInRect, true);

			Text.Font = GameFont.Tiny;
			Vector2 listMousePosition = Event.current.mousePosition;
			float dynamicVerticalY = scrollviewInRect.yMin;

			//Determine which archives to render
			DingoUtils.CacheScrollview(true, this.scrollPosition.y, scrollviewOutRect.height, ListItemHeight, this.cachedArchives.Count, ref dynamicVerticalY, out int FirstRenderedIndex, out int LastRenderedIndex);

			for (int i = FirstRenderedIndex; i > LastRenderedIndex; i--)
			{
				MasterArchive currentMaster = this.cachedArchives[i];

				Rect currentRect = new Rect(scrollviewInRect.x, dynamicVerticalY, scrollviewInRect.width, ListItemHeight);

				if (Event.current.type == EventType.Repaint)
				{
					//Differentiate list background
					if (i % 2 == 0)
					{
						Widgets.DrawAltRect(currentRect);
					}

					//Draw labels and icon
					this.DrawListItem(currentRect, currentMaster);
				}

				//Detect user interaction
				if (currentRect.Contains(listMousePosition))
				{
					if (Event.current.type == EventType.Repaint)
					{
						Widgets.DrawHighlight(currentRect);

						TooltipHandler.TipRegion(currentRect, "Archiver_Tooltip_ListItem".CachedTranslation(new string[] { currentMaster.Label }));
					}

					if (Event.current.type == EventType.MouseDown)
					{
						if (Event.current.button == 0)
						{
							currentMaster.ClickAction();
						}

						else if (Event.current.button == 1)
						{
							Utilities.DoFloatMenu(currentMaster);
						}
					}
				}

				dynamicVerticalY += ListItemHeight;
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			this.listFilter = string.Empty;
		}
	}
}
