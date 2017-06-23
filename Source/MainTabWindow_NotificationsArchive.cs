using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class MainTabWindow_NotificationsArchive : MainTabWindow
	{
		//Private members used to reference values, can be easily changed in the future
		private const float listItemHeight = Letter.DrawHeight;

		private const float dateWidth = 100f;

		private float listItemTotalSize = listItemHeight * 1.5f;

		private float edgeTextSpace = Text.LineHeight * 1.5f;

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
			outRect.yMin += TabDrawer.TabHeight + this.edgeTextSpace;
			outRect.yMax -= this.edgeTextSpace;

			//Virtual Rect for scrolling list
			Rect viewRect = new Rect(outRect);
			viewRect.xMax -= Letter.DrawWidth;

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

			//Draw edge text (translated)
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = new Color32(255, 165, 0, 255); //Orange
			Widgets.Label(new Rect(outRect.x, outRect.yMin - this.edgeTextSpace, outRect.width, this.edgeTextSpace), this.mostRecentLabel); //Drawn above the scrollable list
			Widgets.Label(new Rect(outRect.x, outRect.yMax, outRect.width, this.edgeTextSpace), this.oldestLabel); //Drawn below the scrollable list
			Text.Anchor = TextAnchor.UpperLeft; //Reset
			GUI.color = Color.white; //Reset

			//Draw Letters list for Letters tab
			if (curTab == Archive_Tab.Letters && !letters.NullOrEmpty())
			{
				//Adjust virtual scrollable height
				viewRect.height = (float)letters.Count * listItemTotalSize;

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int i = letters.Count - 1; i >= 0; i--)
				{
					var let = letters[i].letter;

					DrawLetter(viewRect, let, curY);

					curY += listItemTotalSize;
				}

				Widgets.EndScrollView();
			}

			//Draw Messages list for Messages tab
			else if (curTab == Archive_Tab.Messages && !messages.NullOrEmpty())
			{
				//Adjust virtual scrollable height
				viewRect.height = (float)messages.Count * listItemTotalSize;

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int j = messages.Count - 1; j >= 0; j--)
				{
					var msg = messages[j].message;

					DrawMessage(viewRect, msg, curY);

					curY += listItemTotalSize;
				}

				Widgets.EndScrollView();
			}
		}

		private void DrawLetter(Rect originalRect, Letter curLetter, float topY)
		{
			//Draw date box
			Rect dateRect = new Rect(originalRect.x, topY, dateWidth, listItemHeight);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, LetterDateReadout(curLetter));

			//Draw letter box
			Rect letRect = new Rect(dateRect.x + dateRect.width + 5f, topY, Letter.DrawWidth, listItemHeight);

			//Draw letter icon on letter box
			GUI.color = curLetter.def.color;
			GUI.DrawTexture(letRect, curLetter.def.Icon);

			//Draw letter info
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Rect infoRect = new Rect(letRect.x + letRect.width + 5f, topY, originalRect.width - dateRect.width - letRect.width - 10f, listItemHeight);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(infoRect, curLetter.label);
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Highlight and button
			Rect buttonRect = new Rect(letRect.x, letRect.y, letRect.width + infoRect.width + 5f, listItemHeight);
			Widgets.DrawHighlightIfMouseover(buttonRect);
			var curChoiceLetter = curLetter as ChoiceLetter;
			if (curChoiceLetter != null) //Tooltip with some of the notification text for quality of life
			{
				string tooltipText = curChoiceLetter.text;
				if (tooltipText.Length > 100)
				{
					tooltipText = tooltipText.TrimmedToLength(100) + "...";
				}
				TooltipHandler.TipRegion(buttonRect, tooltipText);
			}

			if (Widgets.ButtonInvisible(buttonRect, false))
			{
				curLetter.OpenLetter();
			}
		}

		private void DrawMessage(Rect originalRect, ArchivedMessage message, float topY)
		{
			//Draw date box
			Rect dateRect = new Rect(originalRect.x, topY, dateWidth, listItemHeight);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, MessageDateReadout(message));

			//Draw message rect
			Rect msgRect = new Rect(dateRect.x + dateRect.width + 5f, topY, originalRect.width - dateRect.width - 5f, listItemHeight);

			//Draw message content
			Widgets.Label(msgRect, message.text);
			Text.Font = GameFont.Small; //Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Thing target button and highlight if lookTarget exists
			if (message.lookTarget.IsValid)
			{
				Widgets.DrawHighlightIfMouseover(msgRect);
				TooltipHandler.TipRegion(msgRect, this.targetedMessageTooltipText);

				if (Widgets.ButtonInvisible(msgRect, false))
				{
					CameraJumper.TryJumpAndSelect(message.lookTarget);
				}
			}
		}
	}
}
