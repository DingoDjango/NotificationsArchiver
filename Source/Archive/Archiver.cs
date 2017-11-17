using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Notifications_Archiver
{
	public class Archiver : GameComponent
	{
		private List<MasterArchive> archives = new List<MasterArchive>();

		public bool EnableArchiving = true;

		public bool ShowLetters = true;

		public bool ShowMessages = true;

		public List<MasterArchive> MasterArchives => this.archives;

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
