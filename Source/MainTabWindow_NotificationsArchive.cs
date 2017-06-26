using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class MainTabWindow_NotificationsArchive : MainTabWindow
	{
		private const float listItemSize = 45f; //Verse.Letter.DrawHeight * 1.5

		private const float adjustedLetterWidth = 57f; //Verse.Letter.DrawWidth * 1.5

		private const float dateWidth = 100f; //Relatively wide to account for some languages' date strings

		private List<MasterArchive> masterArchives = Current.Game.GetComponent<Logger>().MasterArchives;

		private Vector2 scrollPosition;

		private enum Archive_Tab : byte
		{
			Letters,
			Messages
		}

		private Archive_Tab curTab;

		//Translation strings
		private string letterTabLabel = "Notifications_Archiver_LetterTab".Translate();
		private string messageTabLabel = "Notifications_Archiver_MessageTab".Translate();
		private string mostRecentLabel = "Notifications_Archiver_Message_MostRecent".Translate();
		private string oldestLabel = "Notifications_Archiver_Message_Oldest".Translate();
		private string targetedMessageTooltipText = "Notifications_Archiver_TargetedMessage_Tooltip".Translate();
		private string dateUnknown = "Notifications_Archiver_Date_None".Translate();

		private string LetterDateReadout(Letter letter)
		{
			var letMaster = this.masterArchives.Find(m => m.letter == letter);

			if (letMaster != null)
			{
				return this.MasterDate(letMaster);
			}

			return this.dateUnknown;
		}

		private string MessageDateReadout(ArchivedMessage message)
		{
			var msgMaster = this.masterArchives.Find(m => m.message == message);

			if (msgMaster != null)
			{
				return this.MasterDate(msgMaster);
			}

			return this.dateUnknown;
		}

		private string MasterDate(MasterArchive master)
		{
			if (master.dateDayofSeason != -1 && master.dateQuadrum != Quadrum.Undefined && master.dateYear != -1)
			{
				return "Notifications_Archiver_Date_Readout".Translate(new object[]
			{
				master.dateQuadrum.Label(),
				master.dateDayofSeason,
				master.dateYear
			});
			}

			return this.dateUnknown;
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(600f, (float)UI.screenHeight * 0.75f);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			//Filtered MasterArchive according to desired type
			List<MasterArchive> letters = this.masterArchives.FindAll(archive => archive.letter != null);
			List<MasterArchive> messages = this.masterArchives.FindAll(archive => archive.message != null);

			//Tabs rect
			Rect tabsRect = new Rect(inRect);
			tabsRect.y += TabDrawer.TabHeight;

			//Outer Rect for scrolling list
			Rect outRect = new Rect(inRect);
			outRect.yMin += TabDrawer.TabHeight + 10f;

			//Virtual Rect for scrolling list
			Rect viewRect = new Rect(outRect);
			viewRect.xMax -= 40f;

			//Tab switcher (RimWorld.MainTabWindow_History)
			List<TabRecord> tabsList = new List<TabRecord>();
			tabsList.Add(new TabRecord(this.letterTabLabel, delegate
			{
				this.curTab = Archive_Tab.Letters;
			}, this.curTab == Archive_Tab.Letters));
			tabsList.Add(new TabRecord(this.messageTabLabel, delegate
			{
				this.curTab = Archive_Tab.Messages;
			}, this.curTab == Archive_Tab.Messages));

			//Draw tabs
			TabDrawer.DrawTabs(tabsRect, tabsList);

			//Draw Letters list for Letters tab
			if (curTab == Archive_Tab.Letters && !letters.NullOrEmpty())
			{
				//Adjust virtual scrollable height
				viewRect.height = (float)letters.Count * listItemSize;

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int i = letters.Count - 1; i >= 0; i--)
				{
					var let = letters[i].letter;

					Rect listRect = new Rect(viewRect.x, curY, viewRect.width, listItemSize);

					if (i % 2 == 0)
					{
						Widgets.DrawAltRect(listRect);
					}

					Rect letRect = listRect.ContractedBy(1f);

					DrawLetter(letRect, let);

					curY += listItemSize;
				}

				Widgets.EndScrollView();
			}

			//Draw Messages list for Messages tab
			else if (curTab == Archive_Tab.Messages && !messages.NullOrEmpty())
			{
				//Adjust virtual scrollable height
				viewRect.height = (float)messages.Count * listItemSize;

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int j = messages.Count - 1; j >= 0; j--)
				{
					var msg = messages[j].message;

					Rect listRect = new Rect(viewRect.x, curY, viewRect.width, listItemSize);

					if (j % 2 == 0)
					{
						Widgets.DrawAltRect(listRect);
					}

					Rect msgRect = listRect.ContractedBy(1f);

					DrawMessage(msgRect, msg);

					curY += listItemSize;
				}

				Widgets.EndScrollView();
			}
		}

		private void DrawLetter(Rect rect, Letter letter)
		{
			//Draw date rect
			Rect dateRect = new Rect(rect.x, rect.y, dateWidth, rect.height);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, LetterDateReadout(letter));

			//Draw letter rect
			Rect letRect = new Rect(rect.x + dateRect.width + 5f, rect.y, adjustedLetterWidth, rect.height);

			//Draw letter icon on letter rect
			GUI.color = letter.def.color;
			GUI.DrawTexture(letRect, letter.def.Icon);

			//Draw letter info
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Rect infoRect = new Rect(letRect.x + letRect.width + 5f, rect.y, rect.width - dateRect.width - letRect.width - 10f, rect.height);
			Widgets.Label(infoRect, letter.label);
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Highlight and button
			Widgets.DrawHighlightIfMouseover(rect);
			var curChoiceLetter = letter as ChoiceLetter;
			if (curChoiceLetter != null) //Tooltip with some of the notification text for quality of life
			{
				string tooltipText = curChoiceLetter.text;
				if (tooltipText.Length > 100)
				{
					tooltipText = tooltipText.TrimmedToLength(100) + "...";
				}
				TooltipHandler.TipRegion(rect, tooltipText);
			}

			if (Widgets.ButtonInvisible(rect, false))
			{
				letter.OpenLetter();
			}
		}

		private void DrawMessage(Rect rect, ArchivedMessage message)
		{
			//Draw date box
			Rect dateRect = new Rect(rect.x, rect.y, dateWidth, rect.height);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, MessageDateReadout(message));

			//Draw message rect
			Rect msgRect = new Rect(rect.x + dateRect.width + 5f, rect.y, rect.width - dateRect.width - 5f, rect.height);

			//Draw message content
			Widgets.Label(msgRect, message.text);
			Text.Font = GameFont.Small; //Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Thing target button and highlight if lookTarget exists
			if (message.lookTarget.IsValid)
			{
				Widgets.DrawHighlightIfMouseover(rect);
				TooltipHandler.TipRegion(rect, this.targetedMessageTooltipText);

				if (Widgets.ButtonInvisible(rect, false))
				{
					CameraJumper.TryJumpAndSelect(message.lookTarget);
				}
			}
		}
	}
}
