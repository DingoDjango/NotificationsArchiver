using System.Collections.Generic;
using Verse;

namespace Notifications_Archiver
{
	public class Logger : GameComponent
	{
		private List<Letter> archivedLetters = new List<Letter>();

		private List<ArchivedMessage> archivedMessages = new List<ArchivedMessage>();

		public List<Letter> LoggedLetters
		{
			get
			{
				return this.archivedLetters;
			}
		}

		public List<ArchivedMessage> LoggedMessages
		{
			get
			{
				return this.archivedMessages;
			}
		}

		public Logger()
		{
		}

		public Logger(Game game)
		{
		}

		public void NotifyNewLetter(Letter letter)
		{
			if (!archivedLetters.Contains(letter))
			{
				archivedLetters.Add(letter);
			}
		}

		public void NotifyNewArchivedMessage(ArchivedMessage message)
		{
			if (!archivedMessages.Contains(message))
			{
				archivedMessages.Add(message);
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<Letter>(ref this.archivedLetters, "archivedLetters", LookMode.Deep);
			Scribe_Collections.Look<ArchivedMessage>(ref this.archivedMessages, "archivedMessages", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (archivedLetters.RemoveAll((Letter x) => x == null) != 0)
				{
					Log.Error("Notification Log :: Some Letters were null.");
				}

				if (archivedMessages.RemoveAll((ArchivedMessage m) => m == null) != 0)
				{
					Log.Error("Notification Log :: Some Messages were null.");
				}
			}
		}
	}
}
