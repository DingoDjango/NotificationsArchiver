using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Notifications_Archiver
{
	public class MasterArchive : IExposable
	{
		public int dateDayofSeason = -1;

		public Quadrum dateQuadrum = Quadrum.Undefined;

		public int dateYear = -1;

		public Letter letter = null;

		public ArchivedMessage message = null;

		public MasterArchive()
		{
		}

		//Letter constructor
		public MasterArchive(Letter let)
		{
			this.letter = let;

			this.SetDateInfo();
		}

		//ArchivedMessage constructor
		public MasterArchive(ArchivedMessage msg)
		{
			this.message = msg;

			this.SetDateInfo();
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
				this.dateQuadrum = GenDate.Quadrum((long)Find.TickManager.TicksGame, location.x);
				this.dateYear = GenDate.Year((long)Find.TickManager.TicksGame, location.x);
			}

			else
			{
				this.dateDayofSeason = GenDate.DayOfSeason((long)Find.TickManager.TicksAbs, location.x) + 1;
				this.dateQuadrum = GenDate.Quadrum((long)Find.TickManager.TicksAbs, location.x);
				this.dateYear = GenDate.Year((long)Find.TickManager.TicksAbs, location.x);
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref this.dateDayofSeason, "dateDayofSeason", -1);
			Scribe_Values.Look(ref this.dateQuadrum, "dateQuadrum", Quadrum.Undefined);
			Scribe_Values.Look(ref this.dateYear, "dateYear", -1);
			Scribe_Deep.Look(ref this.letter, "letter", new object[0]);
			Scribe_Deep.Look(ref this.message, "message", new object[0]);
		}
	}
}
