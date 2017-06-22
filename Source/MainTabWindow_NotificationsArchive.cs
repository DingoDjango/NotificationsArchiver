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

		private float messageHeight = Text.LineHeight * 2f;

		private List<Letter> recordedLetters = Current.Game.GetComponent<Logger>().LoggedLetters;

		private List<ArchivedMessage> recordedMessages = Current.Game.GetComponent<Logger>().LoggedMessages;

		private enum Archive_Tab : byte
		{
			Letters,
			Messages
		}

		private Archive_Tab curTab;

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

			//Tabs rect
			Rect tabsRect = new Rect(inRect);
			tabsRect.y += TabDrawer.TabHeight;

			//Outer Rect for scrolling list
			Rect outRect = new Rect(inRect);
			outRect.yMin += TabDrawer.TabHeight + (Text.LineHeight * 1.5f);
			outRect.yMax -= (Text.LineHeight * 1.5f);

			//Virtual Rect for scrolling list
			Rect viewRect = new Rect(outRect);
			viewRect.xMax -= Letter.DrawWidth;

			//Tab switcher taken from MainTabWindow_History
			List<TabRecord> tabsList = new List<TabRecord>();
			tabsList.Add(new TabRecord("Notifications_Archiver_LetterTab".Translate(), delegate
			{
				this.curTab = Archive_Tab.Letters;
			}, this.curTab == Archive_Tab.Letters));
			tabsList.Add(new TabRecord("Notifications_Archiver_MessageTab".Translate(), delegate
			{
				this.curTab = Archive_Tab.Messages;
			}, this.curTab == Archive_Tab.Messages));

			//Draw tabs
			TabDrawer.DrawTabs(tabsRect, tabsList);

			//Guide text
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(new Rect(outRect.x, outRect.yMin - (Text.LineHeight * 1.25f), outRect.width, Text.LineHeight), "Notifications_Archiver_Message_MostRecent".Translate()); //Latest, drawn at the top
			Widgets.Label(new Rect(outRect.x, outRect.yMax + (Text.LineHeight * 0.5f), outRect.width, Text.LineHeight), "Notifications_Archiver_Message_Oldest".Translate()); //Oldest, drawn at the bottom
			Text.Anchor = TextAnchor.UpperLeft; //Return to RimWorld default

			if (curTab == Archive_Tab.Letters && !recordedLetters.NullOrEmpty())
			{
				this.contentHeight = (float)recordedLetters.Count * listItemSize;
				viewRect.height = this.contentHeight; //Adjust virtual scrollable height

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int i = recordedLetters.Count - 1; i >= 0; i--)
				{
					var let = recordedLetters[i];

					DrawLetter(viewRect, let, curY);

					curY += listItemSize;
				}

				Widgets.EndScrollView();
			}

			else if (curTab == Archive_Tab.Messages && !recordedMessages.NullOrEmpty())
			{
				this.contentHeight = (float)recordedMessages.Count * messageHeight;
				viewRect.height = this.contentHeight; //Adjust virtual scrollable height

				Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

				float curY = viewRect.y;

				for (int j = recordedMessages.Count - 1; j >= 0; j--)
				{
					var msg = recordedMessages[j];

					DrawMessage(viewRect, msg, curY);

					curY += messageHeight;
				}

				Widgets.EndScrollView();
			}
		}

		private void DrawLetter(Rect originalRect, Letter curLetter, float topY)
		{
			//Draw letter box
			Rect letRect = new Rect(originalRect.x, topY, Letter.DrawWidth, Letter.DrawHeight);

			//Draw letter icon on letter box
			GUI.color = curLetter.def.color;
			GUI.DrawTexture(letRect, curLetter.def.Icon);

			//Draw letter info
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			string text = curLetter.label;
			Rect infoRect = new Rect(letRect.x + Letter.DrawWidth * 1.5f, topY, originalRect.width - Letter.DrawWidth * 1.5f, letRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(infoRect, text);
			Text.Anchor = TextAnchor.UpperLeft; //Reset TextAnchor as per RimWorld standard

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
			//Draw message rect
			Rect msgRect = new Rect(originalRect.x, topY, originalRect.width, Text.LineHeight * 2f);

			//Draw message content
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(msgRect, message.text);
			Text.Anchor = TextAnchor.UpperLeft; //Reset to RimWorld default

			//Thing target button and highlight if lookTarget exists
			if (message.lookTarget.IsValid)
			{
				Widgets.DrawHighlightIfMouseover(msgRect);
				TooltipHandler.TipRegion(msgRect, "Notifications_Archiver_TargetedMessage_Tooltip".Translate());

				if (Widgets.ButtonInvisible(msgRect, false))
				{
					CameraJumper.TryJumpAndSelect(message.lookTarget);
				}
			}
		}
	}
}
