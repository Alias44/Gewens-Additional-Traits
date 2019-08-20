using System;
using System.Reflection;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;

namespace Gewens_AdditionalTraits
{
	[StaticConstructorOnStartup]
	public class HarmonyPatches
	{
		static readonly Type patchType = typeof(HarmonyPatches);

		static HarmonyPatches ()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("Gewens_AdditionalTraits.main");
			harmony.Patch(AccessTools.Method(typeof(Corpse), "GiveObservedThought"), null, new HarmonyMethod(patchType, nameof(MorbidCorpse)));

			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		[HarmonyPostfix]
		public static void MorbidCorpse (ref Thought_Memory __result)
		{
			//Fix later
			/*if (__result.pawn.story.traits.HasTrait(TraitDef.Named("Morbid")))
			{
				if (__result.def.defName.Equals("ObservedLayingCorpse"))
				{
					__result = (Thought_MemoryObservation) ThoughtMaker.MakeThought(ThoughtDef.Named("ObservedLayingCorpseMorbid"));
				}
				else if (__result.def.defName.Equals("ObservedLayingRottingCorpse"))
				{
					__result = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDef.Named("ObservedLayingRottingCorpseMorbid"));
				}
			}*/
		}
	}
}
