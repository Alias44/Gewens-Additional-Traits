using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;


namespace Gewen_AdditionalTraits;

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

			//Scribe_Values.Look(ref description, "defDescription", ""); //Saving these is kind of pointless because they get rebuilt on load anyway (it needs to do that so that changes to the description are loaded)
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

		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (defInfo == null)
			{
				defInfo = [];
			}
		}
	}

	public void Add(TraitDef def)
	{
		defInfo.Add(def.defName, new GAT_DefInfo(this.enabled, true, def.DegreeLabels("\n\n")));
	}
}

public class GAT_TraitSettings : ModSettings
{
	public static bool defsChanged = false;
	private static int defCount = 0;
	private static int fileCount = 0;
	private static Vector2 scrollVector2;
	//private static List<DefPackage> packages = new List<DefPackage>();
	private static List<TraitDef> gatTraitDefs = new List<TraitDef>();

	public static Dictionary<string, GAT_FileInfo> fileInfoDict = new Dictionary<string, GAT_FileInfo>();

	public static void Init()
	{
		//packages = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.GetDefPackagesInFolder("TraitDefs\\").ToList();
		gatTraitDefs = Verse.LoadedModManager.GetMod<GewensAddTraits_Mod>().Content.AllDefs.OfType<TraitDef>().ToList();
		foreach (TraitDef traitDef in gatTraitDefs)
		{
			string tdFileName = traitDef.fileName;
			string tdDefName = traitDef.defName;

			if (fileInfoDict.ContainsKey(tdFileName) == false)
			{
				fileInfoDict.Add(tdFileName, new GAT_FileInfo(true, true)); //Newly found files should be enabled by default and obviously exist
			}
			else
			{
				fileInfoDict[tdFileName].exists = true; //confirm the existance of files referenced by defs (they get set to false on load)
			}

			if (fileInfoDict[tdFileName].defInfo.ContainsKey(tdDefName) == false) //Same as for files, but for each def
			{
				fileInfoDict[tdFileName].Add(traitDef);
			}
			else
			{
				fileInfoDict[tdFileName].defInfo[tdDefName].exists = true;
			}
		}

		recountValid();

		if (defsChanged == true)
		{
			HandleChanges();
		}
	}

	private static void recountValid()
	{
		var part = fileInfoDict.Values.Where(fileInfo => fileInfo.exists);

		defCount = part.Count();
		fileCount = part.Sum(fileInfo => fileInfo.defInfo.Values.Where(defInfo => defInfo.exists).Count());
	}


	public static void HandleChanges()
	{
		List<string> fileHitList = new List<string>();

		foreach (KeyValuePair<string, GAT_FileInfo> file in fileInfoDict)
		{
			List<string> defHitList = new List<string>();

			foreach (KeyValuePair<string, GAT_FileInfo.GAT_DefInfo> def in file.Value.defInfo)
			{
				try
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
				catch (System.Collections.Generic.KeyNotFoundException)
				{
					defHitList.Add(def.Key);
				}
			}

			defHitList.ForEach(hit => fileInfoDict[file.Key].defInfo.Remove(hit));
			defHitList.Clear();

			if (fileInfoDict[file.Key].defInfo.Count == 0)
			{
				fileHitList.Add(file.Key);
			}
		}

		fileHitList.ForEach(hit => fileInfoDict.Remove(hit));

		defsChanged = false;
		recountValid();
		Init();
		//toStringDebug();
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
		recountValid();
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
		
		float gapHeight = Listing.DefaultGap;
		Rect scroller = r.GetInnerRect();
		Rect listRect = r.GetInnerRect().TopPart(.9f);
		listRect.y += warnMsgHeight;
		listRect.height -= warnMsgHeight;

		scroller.width -= 20f; //20 is width of scroll bar
		scroller.height = (defCount + fileCount) * (Text.LineHeight + (options.verticalSpacing)) + (fileCount * gapHeight);
		Widgets.BeginScrollView(listRect, ref scrollVector2, scroller);
		options.Begin(scroller);

		foreach (string fileName in fileInfoDict.Where(kv => kv.Value.exists).Select(kv => kv.Key).OrderBy(k => k))
		{
			bool fileStatus = fileInfoDict[fileName].enabled;

			options.CheckboxLabeled(fileName, ref fileStatus, (fileStatus == true ? "Disable" : "Enable") + " all defs in file");

			bool allDefStstus = false;
			foreach (string defName in fileInfoDict[fileName].defInfo.Where(kv => kv.Value.exists).Select(kv => kv.Key))
			{

				bool defStatus = fileInfoDict[fileName].defInfo[defName].enabled;

				if (fileStatus != fileInfoDict[fileName].enabled) //If the file status has changed, set the defs to be the new file status
				{
					defStatus = fileStatus;
				}
				allDefStstus = allDefStstus || defStatus; //the file status should be the result of all of the defs or'd together

				options.CheckboxLabeled("\t" + defName, ref defStatus, fileInfoDict[fileName].defInfo[defName].description);
				fileInfoDict[fileName].defInfo[defName].enabled = defStatus;

			}
			fileStatus = allDefStstus;
			fileInfoDict[fileName].enabled = fileStatus;

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
	public static GAT_TraitSettings settings;

	public GewensAddTraits_Mod(ModContentPack content) : base(content)
	{
		settings = GetSettings<GAT_TraitSettings>();
	}

	public override string SettingsCategory() => "Additional Traits";

	public override void DoSettingsWindowContents(Rect inRect)
	{
		settings.DoSettingsWindowContents(inRect);
	}
}