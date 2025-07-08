using RimWorld;
using System;
using System.Xml;
using Verse;

namespace Gewen_AdditionalTraits;

class BackCompatibilityConverter_GAT : BackCompatibilityConverter
{
	public override bool AppliesToVersion(int majorVer, int minorVer) => majorVer == 0 || (majorVer == 1 && minorVer <= 1); // applies to <= 1.1

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (GenDefDatabase.GetDefSilentFail(defType, defName, false) == null)
		{
			if (defType == typeof(TraitDef) || defType == typeof(ThoughtDef))
			{
				var def = GenDefDatabase.GetDefSilentFail(defType, "GAT_" + defName, false);

				if (def != null)
				{
					return def.defName;
				}
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
