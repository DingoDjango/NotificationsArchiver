using System.Collections.Generic;
using Verse;

namespace Notifications_Archiver
{
	public class Logger : GameComponent
	{
		private List<MasterArchive> masterArchiveMembers = new List<MasterArchive>();

		public List<MasterArchive> MasterArchives
		{
			get
			{
				return this.masterArchiveMembers;
			}
		}

		public void NotifyNewLetter(Letter letter)
		{
			//Assign to master archive
			var letterArchive = new MasterArchive(letter);

			if (!masterArchiveMembers.Contains(letterArchive))
			{
				masterArchiveMembers.Add(letterArchive);
			}
		}

		public void NotifyNewArchivedMessage(ArchivedMessage message)
		{
			//Assign to master archive
			var messageArchive = new MasterArchive(message);

			if (!masterArchiveMembers.Contains(messageArchive))
			{
				masterArchiveMembers.Add(messageArchive);
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<MasterArchive>(ref this.masterArchiveMembers, "masterArchiveMembers", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (masterArchiveMembers.RemoveAll((MasterArchive m) => m == null) != 0)
				{
					Log.Error("Notification Archiver :: Some MasterArchives were null.");
				}
			}
		}

		//Empty constructors due to A17 bug
		public Logger()
		{
		}

		public Logger(Game game)
		{
		}

		public override void StartedNewGame()
		{
			NullListCheck();
		}

		public override void LoadedGame()
		{
			NullListCheck();
		}

		private void NullListCheck()
		{
			if (this.masterArchiveMembers == null)
			{
				this.masterArchiveMembers = new List<MasterArchive>();
			}
		}
	}
}
