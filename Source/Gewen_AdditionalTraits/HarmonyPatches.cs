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
#if RELEASE_1_5 || RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1
			harmony.Patch(AccessTools.Method(typeof(QualityUtility), "GenerateQualityCreatedByPawn", [typeof(Pawn), typeof(SkillDef)]), null, new HarmonyMethod(patchType, nameof(ApolloQuality)));
#else
			harmony.Patch(AccessTools.Method(typeof(QualityUtility), "GenerateQualityCreatedByPawn", [typeof(Pawn), typeof(SkillDef), typeof(bool)]), null, new HarmonyMethod(patchType, nameof(ApolloQuality)));
#endif

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Add a custom back compatibility to the conversion chain
			List<BackCompatibilityConverter> compatibilityConverters =
				AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility),
					"conversionChain");

			compatibilityConverters.Add(new BackCompatibilityConverter_GAT());
			compatibilityConverters.Add(new BackCompatibilityConverter_Rename());
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

		[HarmonyPostfix]
		public static void ApolloQuality(Pawn pawn, ref QualityCategory __result)
		{
			try
			{
				if(pawn.story.traits.HasTrait(GATDefOf.GAT_Apollo))
				{
					MethodInfo addLevels = AccessTools.Method(typeof(QualityUtility), "AddLevels", [typeof(QualityCategory), typeof(int)]);
					__result = (QualityCategory) addLevels.Invoke(null, [__result, 1]);
				}
			} catch (Exception) {}
		}
	}
}