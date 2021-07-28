using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Gewen_AdditionalTraits
{
	[StaticConstructorOnStartup]
	static class AdditionalTraits
	{
		static AdditionalTraits()
		{
			GAT_TraitSettings.Init();
			RemoveTraits();
		}

		public static void RemoveTraits()
		{
			foreach (KeyValuePair<string, GAT_FileInfo> file in GAT_TraitSettings.fileInfoDict)
			{
				foreach (KeyValuePair<string, GAT_FileInfo.GAT_DefInfo> item in file.Value.defInfo)
				{
					TraitDef td = DefDatabase<TraitDef>.GetNamedSilentFail(item.Key);

					if (td == null) //Def does not exist (it was deleted)
					{
						GAT_TraitSettings.defsChanged = true;
						GAT_TraitSettings.fileInfoDict[file.Key].defInfo[item.Key].exists = false;
					}
					else //Def exists
					{
						GAT_TraitSettings.fileInfoDict[file.Key].defInfo[item.Key].exists = true;

						if (file.Value.exists == false || td.fileName.Equals(file.Key) == false) //def exists, but the file does not (def moved to new file / file has been renamed)
						{
							GAT_TraitSettings.defsChanged = true;
							GAT_TraitSettings.fileInfoDict[file.Key].defInfo[item.Key].fileChanged = td.fileName;
						}

						if (item.Value.enabled == false) //def exists and needs to be removed
						{
							RemoveTraitDef(item.Key);
						}
					}
				}
			}

			if (GAT_TraitSettings.defsChanged)
			{
				GAT_TraitSettings.HandleChanges();
			}
		}

		static MethodInfo removeMethod = AccessTools.Method(typeof(DefDatabase<TraitDef>), "Remove");
		public static bool RemoveTraitDef(string traitDefName)
		{
			removeMethod.Invoke(null, new object[] {TraitDef.Named(traitDefName)});

			return true;
		}
	}
}
