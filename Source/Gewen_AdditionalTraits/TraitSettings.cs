using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

using Harmony;


namespace Gewen_AdditionalTraits
{
	public class FileInfo : IExposable
	{
		public class DefInfo : IExposable
		{
			public bool enabled = true;
				
			public string description = "";

			public DefInfo()
			{
				this.enabled = true;
				this.description = "";
			}

			public DefInfo(bool enabled = true, string description="")
			{
				this.enabled = enabled;
				this.description = description;
			}

			public void ExposeData()
			{
				Scribe_Values.Look(ref enabled, "defEnabled", true);
				Scribe_Values.Look(ref description, "defDescription", "");
			}
		}

		public bool enabled = true;
		public Dictionary<string, DefInfo> defInfo = new Dictionary<string, DefInfo>();

		public FileInfo()
		{
			this.enabled = true;
		}

		public FileInfo(bool enabled = true)
		{
			this.enabled = enabled;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref enabled, "fileEnabled", true);
			Scribe_Collections.Look<string, DefInfo>(ref defInfo, "defInfo", LookMode.Value, LookMode.Deep);

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (defInfo == null)
				{
					defInfo = new Dictionary<string, DefInfo>();
				}
			}
		}
	}

	public class TraitSettings : ModSettings
	{

		private Dictionary<string, FileInfo> fileDict = new Dictionary<string, FileInfo>();

		//public static List<string> disabledTraitList = new List<string>();
		private List<DefPackage> packages = new List<DefPackage>();
		//private Dictionary<string, bool> fileEnabledDict = new Dictionary<string, bool>();
		//private List<Dictionary<string, bool>> defEnabledDict = new List<Dictionary<string, bool>>();
		private static Vector2 scrollVector2;

		private int defCount = 0;

		//Fetch mod defs, initialize dictionaries, and count entries
		public void init()
		{
			packages = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.GetDefPackagesInFolder("TraitDefs\\").ToList();

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				if (fileDict.ContainsKey(pack.fileName) == false)
				{
					fileDict.Add(pack.fileName, new FileInfo(true));
				}

				foreach (TraitDef item in pack)
				{
					if (fileDict[pack.fileName].defInfo.ContainsKey(item.defName) == false)
					{
						fileDict[pack.fileName].defInfo.Add(item.defName, new FileInfo.DefInfo(fileDict[pack.fileName].enabled));
					}

					string desc = "";
					foreach (var degree in item.degreeDatas)
					{
						desc += degree.label + ": " + degree.description + "\n\n";
					}
					fileDict[pack.fileName].defInfo[item.defName].description = desc;
				}
				defCount += fileDict[pack.fileName].defInfo.Count;
			}
		}

		public override void ExposeData()
		{
			//Scribe_Collections.Look<string>(ref disabledTraitList, "disabledTraitList", LookMode.Value);
			Scribe_Collections.Look<string, FileInfo>(ref fileDict, "fileDict", LookMode.Value, LookMode.Deep);

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (fileDict == null)
				{
					fileDict = new Dictionary<string, FileInfo>();
				}
			}
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
			Rect scroller = new Rect(r.x, r.y, r.width * 0.9f, (defCount + fileDict.Count) * (Text.LineHeight + (options.verticalSpacing / 2)) + (fileDict.Count * gapHeight) + 36);
			options.BeginScrollView(r.TopPart(.9f), ref scrollVector2, ref scroller);
			options.Gap(36); //Not sure why the scroller thinks it needs to start 36 before when it actually does...

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				//bool fileStatus = fileEnabledDict[pack.fileName];
				bool fileStatus = fileDict[pack.fileName].enabled;
				options.CheckboxLabeled(pack.fileName, ref fileStatus, "enable/disable all defs in file");

				foreach (TraitDef def in pack.defs)
				{
					bool defStatus = fileStatus;
					string desc = "";

					if (fileStatus == fileDict[pack.fileName].enabled)
					{
						defStatus = fileDict[pack.fileName].defInfo[def.defName].enabled;
					}
					foreach (var item in def.degreeDatas)
					{
						desc += item.label + ": " + item.description + "\n\n";
					}
					options.CheckboxLabeled("\t" + def.defName, ref defStatus, desc);
					fileDict[pack.fileName].defInfo[def.defName].enabled = defStatus;
				}
				fileDict[pack.fileName].enabled = fileStatus;

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