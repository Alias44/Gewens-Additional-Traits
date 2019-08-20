using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

using Harmony;


namespace Gewen_AdditionalTraits
{
	public class GAT_FileInfo : IExposable
	{
		public class GAT_DefInfo : IExposable
		{
			public bool enabled = true;
				
			public string description = "";

			public GAT_DefInfo()
			{
				this.enabled = true;
				this.description = "";
			}

			public GAT_DefInfo(bool enabled = true, string description="")
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
		public Dictionary<string, GAT_DefInfo> defInfo = new Dictionary<string, GAT_DefInfo>();

		public GAT_FileInfo()
		{
			this.enabled = true;
		}

		public GAT_FileInfo(bool enabled = true)
		{
			this.enabled = enabled;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref enabled, "fileEnabled", true);
			Scribe_Collections.Look<string, GAT_DefInfo>(ref defInfo, "defInfo", LookMode.Value, LookMode.Deep);

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (defInfo == null)
				{
					defInfo = new Dictionary<string, GAT_DefInfo>();
				}
			}
		}
	}

	public class TraitSettings : ModSettings
	{
		private int defCount = 0;
		private static Vector2 scrollVector2;
		private List<DefPackage> packages = new List<DefPackage>();
		public static Dictionary<string, GAT_FileInfo> fileInfoDict = new Dictionary<string, GAT_FileInfo>();

		public void init()
		{
			packages = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.GetDefPackagesInFolder("TraitDefs\\").ToList();

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				if (fileInfoDict.ContainsKey(pack.fileName) == false)
				{
					fileInfoDict.Add(pack.fileName, new GAT_FileInfo(true));
				}

				foreach (TraitDef item in pack)
				{
					if (fileInfoDict[pack.fileName].defInfo.ContainsKey(item.defName) == false)
					{
						fileInfoDict[pack.fileName].defInfo.Add(item.defName, new GAT_FileInfo.GAT_DefInfo(fileInfoDict[pack.fileName].enabled));
					}

					string desc = "";
					foreach (var degree in item.degreeDatas)
					{
						desc += degree.label + ": " + degree.description + "\n\n";
					}
					fileInfoDict[pack.fileName].defInfo[item.defName].description = desc;
				}
				defCount += fileInfoDict[pack.fileName].defInfo.Count;
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<string, GAT_FileInfo>(ref fileInfoDict, "fileDict", LookMode.Value, LookMode.Deep);

			if (Scribe.mode != LoadSaveMode.Saving)
			{
				if (fileInfoDict == null)
				{
					fileInfoDict = new Dictionary<string, GAT_FileInfo>();
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
			Rect scroller = r.GetInnerRect();
			scroller.width -= 20f; //20 is width of scroll bar
			scroller.height = (defCount + fileInfoDict.Count) * (Text.LineHeight + (options.verticalSpacing)) + (fileInfoDict.Count * gapHeight);
			Widgets.BeginScrollView(r.GetInnerRect().TopPart(.9f), ref scrollVector2, scroller);
			options.Begin(scroller);

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				bool fileStatus = fileInfoDict[pack.fileName].enabled;

				options.CheckboxLabeled(pack.fileName, ref fileStatus, "enable/disable all defs in file");
				foreach (TraitDef def in pack.defs)
				{
					bool defStatus = fileStatus;

					if (fileStatus == fileInfoDict[pack.fileName].enabled)
					{
						defStatus = fileInfoDict[pack.fileName].defInfo[def.defName].enabled;
					}

					options.CheckboxLabeled("\t" + def.defName, ref defStatus, fileInfoDict[pack.fileName].defInfo[def.defName].description);
					fileInfoDict[pack.fileName].defInfo[def.defName].enabled = defStatus;
				}
				fileInfoDict[pack.fileName].enabled = fileStatus;

				options.GapLine(gapHeight);
			}
			options.End();
			Widgets.EndScrollView();
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