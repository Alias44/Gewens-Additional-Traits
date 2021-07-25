using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Gewen_AdditionalTraits
{
	[StaticConstructorOnStartup]
	public class HarmonyPatches
	{
		static readonly Type patchType = typeof(HarmonyPatches);

		static HarmonyPatches()
		{
			var harmony = new Harmony("Gewen_AdditionalTraits.main");
			//harmony.Patch(AccessTools.Method(typeof(Corpse), "GiveObservedThought"), null, new HarmonyMethod(patchType, nameof(MorbidCorpse)));

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Add a custom back compatibility to the conversion chain
			List<BackCompatibilityConverter> compatibilityConverters =
				AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility),
					"conversionChain");

			compatibilityConverters.Add(new BackCompatibilityConverter_GAT());
		}

		/*[HarmonyPostfix]
		public static void MorbidCorpse (Corpse __instance, ref Thought_Memory __result)
		{
			//Log.Message(__result.def.ToString());
			
			//Fix later
			if (__result.pawn.story.traits.HasTrait(TraitDef.Named("Morbid")))
			{
				Thought_MemoryObservation memoryObservation = !__instance.IsNotFresh() ? (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDef.Named("ObservedLayingCorpseMorbid")) : (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDef.Named("ObservedLayingRottingCorpseMorbid"));
				memoryObservation.Target = (Thing)__instance;
				__result = (Thought_Memory)memoryObservation;
			}
		}*/
	}
}