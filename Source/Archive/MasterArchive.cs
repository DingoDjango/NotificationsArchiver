using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Notifications_Archiver
{
	public class MasterArchive : IExposable
	{
		public ArchiveType type;

		public bool pinned;

		public int dateDayofSeason = -1;

		public Quadrum dateQuadrum = Quadrum.Undefined;

		public int dateYear = -1;

		public Letter letter;

		public GlobalTargetInfo lookTarget;

		private string text;

		private void SetDateInfo()
		{
			//RimWorld.DateReadout.DateOnGUI
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

			int gameTicks = Find.TickManager.gameStartAbsTick == 0 ? Find.TickManager.TicksGame : Find.TickManager.TicksAbs; //Find.TickManager.TicksAbs errors if gameStartAbsTick is 0

			//RimWorld.GenDate.DateReadoutStringAt
			this.dateDayofSeason = GenDate.DayOfSeason(gameTicks, location.x) + 1;
			this.dateQuadrum = GenDate.Quadrum(gameTicks, location.x);
			this.dateYear = GenDate.Year(gameTicks, location.x);
		}

		public string Label
		{
			get
			{
				if (this.letter != null)
				{
					return this.letter.label;
				}

				return this.text;
			}
		}

		public string Text
		{
			get
			{
				if (this.letter is ChoiceLetter)
				{
					ChoiceLetter choiceLet = this.letter as ChoiceLetter;

					return choiceLet.text;
				}

				return this.text;
			}
		}

		public void ClickAction()
		{
			if (this.letter != null)
			{
				this.letter.OpenLetter();
			}

			if (this.lookTarget != GlobalTargetInfo.Invalid)
			{
				CameraJumper.TryJumpAndSelect(this.lookTarget);
			}
		}

		public MasterArchive()
		{
			this.SetDateInfo();
		}

		//Letter constructor
		public MasterArchive(Letter let) : this()
		{
			this.type = ArchiveType.Letter;
			this.letter = let;
		}

		//Message constructor
		public MasterArchive(string content, GlobalTargetInfo target) : this()
		{
			this.type = ArchiveType.Message;
			this.text = content;
			this.lookTarget = target;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!this.lookTarget.IsValid || this.lookTarget.ThingDestroyed || (this.lookTarget.HasThing && this.lookTarget.Thing.MapHeld == null))
				{
					this.lookTarget = GlobalTargetInfo.Invalid;
				}
			}

			Scribe_Values.Look(ref this.type, "type");
			Scribe_Values.Look(ref this.pinned, "pinned", false);

			Scribe_Values.Look(ref this.dateDayofSeason, "dateDayofSeason", -1);
			Scribe_Values.Look(ref this.dateQuadrum, "dateQuadrum", Quadrum.Undefined);
			Scribe_Values.Look(ref this.dateYear, "dateYear", -1);

			Scribe_Deep.Look(ref this.letter, "letter", new object[0]);
			Scribe_TargetInfo.Look(ref this.lookTarget, "lookTarget");
			Scribe_Values.Look(ref this.text, "text");
		}
	}
}
