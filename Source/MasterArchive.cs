using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Notifications_Archiver
{
	public class MasterArchive : IExposable
	{
		public int dateDayofSeason;

		public string dateQuadrum;

		public int dateYear;

		public Letter letter;

		public ArchivedMessage message;

		//Null constructor
		public MasterArchive()
		{
			this.dateDayofSeason = -1;
			this.dateQuadrum = null;
			this.dateYear = -1;
			this.letter = null;
			this.message = null;
		}

		//Letter constructor
		public MasterArchive(Letter let) : this()
		{
			this.letter = let;

			SetDateInfo();
		}

		//ArchivedMessage constructor
		public MasterArchive(ArchivedMessage msg) : this()
		{
			this.message = msg;

			SetDateInfo();
		}

		private void SetDateInfo()
		{
			//Try to get a location for date calculations (RimWorld.DateReadout.DateOnGUI)
			Vector2 location;
			if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.selectedTile >= 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
			}
			else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.NumSelectedObjects > 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
			}
			else
			{
				if (Find.VisibleMap == null)
				{
					return;
				}
				location = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile);
			}

			//Find.TickManager.TicksAbs errors if gameStartAbsTick is 0
			if (Find.TickManager.gameStartAbsTick == 0)
			{
				this.dateDayofSeason = GenDate.DayOfSeason((long)Find.TickManager.TicksGame, location.x) + 1;
				this.dateQuadrum = GenDate.Quadrum((long)Find.TickManager.TicksGame, location.x).Label();
				this.dateYear = GenDate.Year((long)Find.TickManager.TicksGame, location.x);
			}

			else
			{
				this.dateDayofSeason = GenDate.DayOfSeason((long)Find.TickManager.TicksAbs, location.x) + 1;
				this.dateQuadrum = GenDate.Quadrum((long)Find.TickManager.TicksAbs, location.x).Label();
				this.dateYear = GenDate.Year((long)Find.TickManager.TicksAbs, location.x);
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.dateDayofSeason, "dateDayofSeason", -1);
			Scribe_Values.Look<string>(ref this.dateQuadrum, "dateQuadrum", null);
			Scribe_Values.Look<int>(ref this.dateYear, "dateYear", -1);
			Scribe_Deep.Look<Letter>(ref this.letter, "letter", new object[0]);
			Scribe_Deep.Look<ArchivedMessage>(ref this.message, "message", new object[0]);
		}
	}
}
