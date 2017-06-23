using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class MainTabWindow_NotificationsArchive : MainTabWindow
	{
		private float contentHeight;

		private Vector2 scrollPosition;

		private float listItemSize = Letter.DrawHeight * 1.5f;

		private float edgeTextSpace = Text.LineHeight * 1.5f;

		private List<MasterArchive> masterArchives = Current.Game.GetComponent<Logger>().MasterArchives;

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
			if (master.dateDayofSeason != -1 && master.dateQuadrum != null && master.dateYear != -1)
			{
				return "Notifications_Archiver_Date_Readout".Translate(new object[]
			{
				master.dateQuadrum,
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
			this.contentHeight = 0f;

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

			//Tab switcher taken from MainTabWindow_History
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

			//Guide text
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(new Rect(outRect.x, outRect.yMin - this.edgeTextSpace, outRect.width, this.edgeTextSpace), this.mostRecentLabel); //Latest, drawn at the top
			Widgets.Label(new Rect(outRect.x, outRect.yMax, outRect.width, this.edgeTextSpace), this.oldestLabel); //Oldest, drawn at the bottom
			Text.Anchor = TextAnchor.UpperLeft; //Return to RimWorld default

			if (curTab == Archive_Tab.Letters && !letters.NullOrEmpty())
			{
				this.contentHeight = (float)letters.Count * listItemSize;
				viewRect.height = this.contentHeight; //Adjust virtual scrollable height

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int i = letters.Count - 1; i >= 0; i--)
				{
					var let = letters[i].letter;

					DrawLetter(viewRect, let, curY);

					curY += listItemSize;
				}

				Widgets.EndScrollView();
			}

			else if (curTab == Archive_Tab.Messages && !messages.NullOrEmpty())
			{
				this.contentHeight = (float)messages.Count * listItemSize;
				viewRect.height = this.contentHeight; //Adjust virtual scrollable height

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int j = messages.Count - 1; j >= 0; j--)
				{
					var msg = messages[j].message;

					DrawMessage(viewRect, msg, curY);

					curY += listItemSize;
				}

				Widgets.EndScrollView();
			}
		}

		private void DrawLetter(Rect originalRect, Letter curLetter, float topY)
		{
			string dateReadout = LetterDateReadout(curLetter);

			//Draw date box
			Rect dateRect = new Rect(originalRect.x, topY, 60f, Letter.DrawHeight);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, dateReadout);

			//Draw letter box
			Rect letRect = new Rect(originalRect.x + dateRect.width + 5f, topY, Letter.DrawWidth, Letter.DrawHeight);

			//Draw letter icon on letter box
			GUI.color = curLetter.def.color;
			GUI.DrawTexture(letRect, curLetter.def.Icon);

			//Draw letter info
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			string text = curLetter.label;
			Rect infoRect = new Rect(letRect.x + (letRect.width * 1.5f), topY, originalRect.width - (letRect.width * 1.5f) - dateRect.width - 5f, letRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Small;
			Widgets.Label(infoRect, text);
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//Highlight and button
			Rect buttonRect = new Rect(letRect.x, letRect.y, originalRect.width, letRect.height);
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
			string dateReadout = MessageDateReadout(message);

			//Draw date box
			Rect dateRect = new Rect(originalRect.x, topY, 60f, Letter.DrawHeight);

			//Draw date info
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(dateRect, dateReadout);

			//Draw message rect
			Rect msgRect = new Rect(originalRect.x + dateRect.width + 5f, topY, originalRect.width - dateRect.width - 5f, Letter.DrawHeight);

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
