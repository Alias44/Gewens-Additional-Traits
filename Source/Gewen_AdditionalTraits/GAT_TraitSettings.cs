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
			public bool exists = false;
			public string fileChanged = "";
			public string description = "";

			public GAT_DefInfo()
			{
				this.enabled = true;

				this.description = "";
			}

			public GAT_DefInfo(bool enabled = true, bool exists=true, string description="")
			{
				this.enabled = enabled;
				this.exists = exists;
				this.description = description;
			}

			public void ExposeData()
			{
				Scribe_Values.Look(ref enabled, "defEnabled", true);
				Scribe_Values.Look(ref description, "defDescription", "");
			}
		}

		public bool enabled = true;
		public bool exists = true;
		public Dictionary<string, GAT_DefInfo> defInfo = new Dictionary<string, GAT_DefInfo>();

		public GAT_FileInfo()
		{
			this.enabled = true;
			this.exists = false;
		}

		public GAT_FileInfo(bool enabled = true, bool exists = true)
		{
			this.enabled = enabled;
			this.exists = exists;
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

	public class GAT_TraitSettings : ModSettings
	{
		public static bool defsChanged = false;
		private static int defCount = 0;
		private static Vector2 scrollVector2;
		private static List<DefPackage> packages = new List<DefPackage>();
		public static Dictionary<string, GAT_FileInfo> fileInfoDict = new Dictionary<string, GAT_FileInfo>();

		public static void Init()
		{
			defCount = 0;
			packages = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.GetDefPackagesInFolder("TraitDefs\\").ToList();

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				if (fileInfoDict.ContainsKey(pack.fileName) == false)
				{
					fileInfoDict.Add(pack.fileName, new GAT_FileInfo(true, true));
				}
				else
				{
					fileInfoDict[pack.fileName].exists = true;
				}

				foreach (TraitDef item in pack)
				{
					if (fileInfoDict[pack.fileName].defInfo.ContainsKey(item.defName) == false)
					{
						fileInfoDict[pack.fileName].defInfo.Add(item.defName, new GAT_FileInfo.GAT_DefInfo(fileInfoDict[pack.fileName].enabled, true));
					}
					else
					{
						fileInfoDict[pack.fileName].defInfo[item.defName].exists = true;
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

			if (defsChanged == true)
			{
				HandleChanges();
			}
		}

		private static int RecountDefs()
		{
			int count = 0;

			foreach (var pack in packages)
			{
				foreach (TraitDef item in pack)
				{
					count++;
				}
			}

			return count;
		}

		public static void HandleChanges()
		{
			List<string> fileHitList = new List<string>();

			foreach (KeyValuePair<string, GAT_FileInfo> file in fileInfoDict)
			{
				List<string> defHitList = new List<string>();

				foreach (KeyValuePair<string, GAT_FileInfo.GAT_DefInfo> def in file.Value.defInfo)
				{
					if (def.Value.fileChanged.NullOrEmpty() == false) //def moved, set new def's enabled
					{
						string newLocation = def.Value.fileChanged;
						def.Value.fileChanged = "";
						fileInfoDict[newLocation].defInfo[def.Key] = def.Value;

						defHitList.Add(def.Key);
					}
					else if (def.Value.exists == false)
					{
						defHitList.Add(def.Key);
					}
				}

				foreach (string hit in defHitList)
				{
					fileInfoDict[file.Key].defInfo.Remove(hit);
				}
				defHitList.Clear();

				if (fileInfoDict[file.Key].defInfo.Count == 0)
				{
					fileHitList.Add(file.Key);
				}
			}

			foreach (string hit in fileHitList)
			{
				fileInfoDict.Remove(hit);
			}

			defsChanged = false;
			//defCount = RecountDefs();
			Init();
			//toStringDebug();
		}

		public static void toStringDebug()
		{
			string msg = "Packages:\n";

			foreach (var item in packages)
			{
				msg += item.fileName + "\n";

				foreach (var item2 in item.defs)
				{
					msg += "\t" + item2.defName + "\n";
				}
			}

			msg += "Dict:\n";
			foreach (var item in fileInfoDict)
			{
				msg += item.Key + ":\n";

				foreach (var item2 in item.Value.defInfo)
				{
					msg += "\t" + item2.Key + "\n";
				}
			}

			Log.Message(msg);
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
			if (defCount == 0)
			{
				Init();
			}

			Listing_Standard options = new Listing_Standard();
			Rect r = inRect.ExpandedBy(.9f);
			options.Begin(r);

			Color defaultColor = GUI.color;
			Text.Font = GameFont.Medium;
			GUI.color = Color.red;
			options.Label("All changes require a restart to take effect");
			Text.Font = GameFont.Small;
			GUI.color = Color.yellow;
			string warningMsg = "Please note that disabling any traits currently in use by your save will cause a \"Could not load reference to RimWorld.TraitDef named\" error on reload for each trait disabled. " +
				"These errors can safely be dismissed and will not reoccur after saving again. " +
				"Anyone that previously had a disabled trait will be randomly assigned a new one in its place.";
			float warnMsgHeight = Text.CalcHeight(warningMsg, r.width);
			options.Label(warningMsg);
			GUI.color = defaultColor;
			options.Gap();
			
			float gapHeight = 12;
			Rect scroller = r.GetInnerRect();
			Rect listRect = r.GetInnerRect().TopPart(.9f);
			listRect.y += warnMsgHeight;
			listRect.height -= warnMsgHeight;

			scroller.width -= 20f; //20 is width of scroll bar
			scroller.height = (defCount + fileInfoDict.Count) * (Text.LineHeight + (options.verticalSpacing)) + (fileInfoDict.Count * gapHeight);
			Widgets.BeginScrollView(listRect, ref scrollVector2, scroller);
			options.Begin(scroller);

			for (int i = 0; i < packages.Count; i++)
			{
				var pack = packages[i];

				bool fileStatus = fileInfoDict[pack.fileName].enabled;

				options.CheckboxLabeled(pack.fileName, ref fileStatus, (fileStatus==true ? "Disable" : "Enable")+" all defs in file");
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
		public GAT_TraitSettings settings = new GAT_TraitSettings();

		public GewensAddTraits_Mod(ModContentPack content) : base(content)
		{
			GetSettings<GAT_TraitSettings>();
		}

		public override string SettingsCategory() => "Additional Traits";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			GetSettings<GAT_TraitSettings>().DoSettingsWindowContents(inRect);
		}
	}
}