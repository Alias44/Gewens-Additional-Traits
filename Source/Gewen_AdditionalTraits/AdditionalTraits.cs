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
			foreach (KeyValuePair<string, GAT_FileInfo> file in TraitSettings.fileInfoDict)
			{
				foreach (KeyValuePair<string, GAT_FileInfo.GAT_DefInfo> item in file.Value.defInfo)
				{
					if (item.Value.enabled == false)
					{
						if (RemoveTrait(item.Key) == false)
						{
							//TraitSettings.fileInfoDict[file.Key].defInfo.Remove(item.Key);
						}
					}
				}

				if (TraitSettings.fileInfoDict[file.Key] == null)
				{
					TraitSettings.fileInfoDict.Remove(file.Key);
				}
			}
		}

		public static bool RemoveTrait(string traitDefName)
		{
			TraitDef td = DefDatabase<TraitDef>.GetNamed(traitDefName);
			if (td == null)
			{
				return false;
			}

			Traverse.Create(typeof(DefDatabase<TraitDef>)).Method("Remove", new Type[]
			{
				typeof (TraitDef)
			}).GetValue(TraitDef.Named(traitDefName));

			return true;
		}
	}
}
