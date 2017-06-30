using System.Collections.Generic;
using Verse;

namespace Notifications_Archiver
{
	public class DummyStandardLetter : ChoiceLetter
	{
		protected override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return DiaOption.DefaultOK; //Using DefaultOK as base.OK would remove the Letter if it's still active

				if (this.lookTarget.IsValid)
				{
					yield return base.JumpToLocation;
				}
			}
		}
	}
}
