﻿using RimWorld;
using Verse;

namespace Gewen_AdditionalTraits
{
	public class ThoughtWorker_Perv : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal (Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
				return (ThoughtState)false;
			if (pawn.story.traits.HasTrait(GATDefOf.GAT_Pervert) || !other.story.traits.HasTrait(GATDefOf.GAT_Pervert)) // crosscheck trait existance?!
				return (ThoughtState)false;
			return (ThoughtState)true;
		}
	}
}
