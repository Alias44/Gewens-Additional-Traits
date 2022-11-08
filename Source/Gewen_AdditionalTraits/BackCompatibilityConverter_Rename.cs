using RimWorld;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace Gewen_AdditionalTraits
{
	class BackCompatibilityConverter_Rename : BackCompatibilityConverter
	{
		Dictionary<string, string> renames = new Dictionary<string, string>()
		{
			{ "GAT_Mercury", "GAT_Hermes" },
			{ "GAT_Sylvanus", "GAT_Pan" },
			{ "GAT_Vulcan", "GAT_Hephaestus" },
			// pre 1.1 non-prefixed defnames
			{ "Mercury", "GAT_Hermes" },
			{ "Sylvanus", "GAT_Pan" },
			{ "Vulcan", "GAT_Hephaestus" }
		};

		public override bool AppliesToVersion(int majorVer, int minorVer) => true;

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (GenDefDatabase.GetDefSilentFail(defType, defName, false) == null)
			{
				if (defType == typeof(TraitDef))
				{
					renames.TryGetValue(defName, out string newName);

					return newName;
				}
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			return;
		}
	}
}
