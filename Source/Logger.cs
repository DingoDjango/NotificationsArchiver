using System.Collections.Generic;
using Verse;

namespace Notifications_Archiver
{
	public class Logger : GameComponent
	{
		private List<MasterArchive> archives = new List<MasterArchive>();

		public List<MasterArchive> MasterArchives
		{
			get
			{
				return this.archives;
			}
		}

		//MasterArchive assignment, used by patches
		public void NotifyNewLetter(Letter letter)
		{
			var letterArchive = new MasterArchive(letter);
			this.archives.Add(letterArchive);
		}

		public void NotifyNewArchivedMessage(ArchivedMessage message)
		{
			var messageArchive = new MasterArchive(message);
			this.archives.Add(messageArchive);
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<MasterArchive>(ref this.archives, "masterArchiveMembers", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (archives.RemoveAll((MasterArchive m) => m == null) != 0)
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
			if (this.archives == null)
			{
				this.archives = new List<MasterArchive>();
			}
		}
	}
}
