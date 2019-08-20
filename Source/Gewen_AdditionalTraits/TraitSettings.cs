using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

using Harmony;


namespace Gewen_AdditionalTraits
{
	public class TraitSettings : ModSettings
	{
		//public static List<string> disabledTraitList = new List<string>();
		private List<DefPackage> packages = new List<DefPackage>();
		private Dictionary<string, bool> fileEnabledDict = new Dictionary<string, bool>();
		private List<Dictionary<string, bool>> defEnabledDict = new List<Dictionary<string, bool>>();
		private static Vector2 scrollVector2;

		private int defCount = 0;

		//Fetch mod defs, initialize dictionaries, and count entries
		public void init()
		{
			packages = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.GetDefPackagesInFolder("TraitDefs\\").ToList();

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				if (fileEnabledDict.TryGetValue(pack.fileName) == false) //If the file hasn't been 
				{
					fileEnabledDict.Add(pack.fileName, true);

					defEnabledDict.Add(new Dictionary<string, bool>());
				}

				foreach (TraitDef item in pack)
				{
					if (defEnabledDict[i].TryGetValue(item.defName) == false)
					{
						defEnabledDict[i].Add(item.defName, fileEnabledDict[pack.fileName]);
					}
				}
				defCount += defEnabledDict[i].Count;
			}
		}

		public override void ExposeData()
		{
			//Scribe_Collections.Look<string>(ref disabledTraitList, "disabledTraitList", LookMode.Value);
		}

		//GUI Window
		public void DoSettingsWindowContents(Rect inRect)
		{
			if (packages.Any() == false)
			{
				init();
			}

			Listing_Standard options = new Listing_Standard();
			Rect r = inRect.ExpandedBy(.9f);
			options.Begin(r);

			Color defaultColor = GUI.color;
			Text.Font = GameFont.Medium;
			GUI.color = Color.red;
			options.Label("All changes require a restart to take effect");
			Text.Font = GameFont.Small;
			GUI.color = defaultColor;

			options.Gap();
			float gapHeight = 12;
			Rect scroller = new Rect(r.x, r.y, r.width * 0.9f, (defCount + fileEnabledDict.Count) * (Text.LineHeight + (options.verticalSpacing / 2)) + (fileEnabledDict.Count * gapHeight) + 36);
			options.BeginScrollView(r.TopPart(.9f), ref scrollVector2, ref scroller);
			options.Gap(36); //Not sure why the scroller thinks it needs to start 36 before when it actually does...

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				bool fileStatus = fileEnabledDict[pack.fileName];
				options.CheckboxLabeled(pack.fileName, ref fileStatus, "enable/disable all defs in file");

				foreach (TraitDef def in pack.defs)
				{
					bool defStatus = fileStatus;
					string desc = "";

					if (fileStatus == fileEnabledDict[pack.fileName])
					{
						defStatus = defEnabledDict[i].TryGetValue(def.defName);
					}
					foreach (var item in def.degreeDatas)
					{
						desc += item.label + ": " + item.description + "\n\n";
					}
					options.CheckboxLabeled("\t" + def.defName, ref defStatus, desc);
					defEnabledDict[i][def.defName] = defStatus;
				}
				fileEnabledDict[pack.fileName] = fileStatus;

				options.GapLine(gapHeight);
			}

			options.EndScrollView(ref scroller);
			options.End();
			this.Write();
		}
	}

	public class GewensAddTraits_Mod : Mod
	{
		public TraitSettings settings = new TraitSettings();

		public GewensAddTraits_Mod(ModContentPack content) : base(content)
		{
			GetSettings<TraitSettings>();
		}

		public override string SettingsCategory() => "Additional Traits";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			GetSettings<TraitSettings>().DoSettingsWindowContents(inRect);
		}
	}
}