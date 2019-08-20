using Harmony;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Gewen_AdditionalTraits
{
	[StaticConstructorOnStartup]
	static class AdditionalTraits
	{
		static AdditionalTraits()
		{
			/*foreach (string trait in TraitSettings.disabledTraitList)
			{
				if(RemoveTrait(trait) == false)
				{
					TraitSettings.disabledTraitList.Remove(trait);
				}
			}*/
		}

		public static void AddTrait(TraitDef trait)
		{
			DefDatabase<TraitDef>.Add(trait);
		}

		public static bool RemoveTrait(string traitDefName)
		{
			TraitDef td = DefDatabase<TraitDef>.GetNamed(traitDefName);
			if (td == null)
			{
				return false;
			}

			Traverse.Create(typeof(DefDatabase<TraitDef>)).Method("Remove", td);
			return true;
		}

		/*public static void RemoveTrait(string trait)
		{
			Traverse.Create(typeof(DefDatabase<TraitDef>)).Method("Remove", new System.Type[1]
			{
			  typeof (TraitDef)
			}, (object[])null).GetValue((object)TraitDef.Named(trait));
		}*/

		public static void RemoveTrait(TraitDef trait)
		{
			RemoveTrait(trait.defName);
		}
	}
}
