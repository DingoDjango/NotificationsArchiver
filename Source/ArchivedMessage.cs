using RimWorld.Planet;
using Verse;

namespace Notifications_Archiver
{
	public class ArchivedMessage : IExposable
	{
		public string text;

		public GlobalTargetInfo lookTarget = GlobalTargetInfo.Invalid;

		public ArchivedMessage()
		{
			this.text = null;
			this.lookTarget = GlobalTargetInfo.Invalid;
		}

		public ArchivedMessage(string txt, GlobalTargetInfo targetInfo)
		{
			this.text = txt;
			this.lookTarget = targetInfo;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref this.text, "text", null);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (this.lookTarget.ThingDestroyed)
				{
					this.lookTarget = GlobalTargetInfo.Invalid;
				}

				if (this.lookTarget.HasThing && this.lookTarget.Thing.MapHeld == null)
				{
					this.lookTarget = GlobalTargetInfo.Invalid;
				}
			}
			Scribe_TargetInfo.Look(ref this.lookTarget, "lookTarget");
		}
	}
}
