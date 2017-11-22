using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Notifications_Archiver
{
	public class Archiver : GameComponent
	{
		private List<MasterArchive> archives = new List<MasterArchive>();

		private Queue<Action> queuedCleanup = new Queue<Action>();

		private Action currentCleanup = null;

		public int ticksSinceArchiveValidation = 0;

		public bool EnableArchiving = true;

		public bool ShowLetters = true;

		public bool ShowMessages = true;

		public List<MasterArchive> MasterArchives => this.archives;

		private void ValidateArchiveTarget(MasterArchive archive)
		{
			Predicate<GlobalTargetInfo> invalidTarget = target => !target.IsValid || target.ThingDestroyed || (target.HasThing && target.Thing.MapHeld == null);

			Letter letter = archive.letter;

			if (letter != null)
			{
				if (invalidTarget(letter.lookTarget))
				{
					letter.lookTarget = GlobalTargetInfo.Invalid;
				}
			}

			else if (invalidTarget(archive.lookTarget))
			{
				archive.lookTarget = GlobalTargetInfo.Invalid;
			}

			this.ticksSinceArchiveValidation = 0;
			this.currentCleanup = null;
		}

		public void NewArchive(Letter letter, string text, GlobalTargetInfo target)
		{
			if (this.EnableArchiving)
			{
				MasterArchive newArchive;

				if (letter != null)
				{
					newArchive = new MasterArchive(letter);

					//Dummify complex letters to avoid players exploiting the archiver		
					if (letter is ChoiceLetter && letter.GetType() != typeof(StandardLetter))
					{
						ChoiceLetter choiceLet = newArchive.letter as ChoiceLetter;

						newArchive.letter = new DummyStandardLetter
						{
							def = choiceLet.def,
							label = choiceLet.label,
							lookTarget = choiceLet.lookTarget,
							disappearAtTick = -1,
							title = choiceLet.title,
							text = choiceLet.text
						};
					}
				}

				else
				{
					newArchive = new MasterArchive(text, target);
				}

				this.archives.Add(newArchive);

				MainTabWindow_Archive.mustRecacheList = true;
			}
		}

		public void QueueArchiveCleanup(MasterArchive master)
		{
			this.queuedCleanup.Enqueue(delegate
			{
				this.ValidateArchiveTarget(master);
			});
		}

		public override void GameComponentUpdate()
		{
			base.GameComponentUpdate();

			this.currentCleanup?.Invoke();

			if (this.currentCleanup == null && this.queuedCleanup.Count > 0)
			{
				this.currentCleanup = this.queuedCleanup.Dequeue();
			}
		}

		public override void GameComponentTick()
		{
			base.GameComponentTick();

			if (++this.ticksSinceArchiveValidation == GenDate.TicksPerDay)
			{
				this.ticksSinceArchiveValidation = 0;

				for (int i = 0; i < this.archives.Count; i++)
				{
					this.QueueArchiveCleanup(this.archives[i]);
				}
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref this.archives, "archives", LookMode.Deep);
			Scribe_Values.Look(ref this.EnableArchiving, "EnableArchiving", true);
			Scribe_Values.Look(ref this.ShowLetters, "ShowLetters", true);
			Scribe_Values.Look(ref this.ShowMessages, "ShowMessages", true);
		}

		public Archiver()
		{
		}

		public Archiver(Game game)
		{
		}
	}
}
