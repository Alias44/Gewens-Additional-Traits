using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Gewen_AdditionalTraits;
public static class Util
{
	public static string DegreeLabels(this TraitDef def, string delimiter) => string.Join(delimiter, def.degreeDatas.Select(degree => degree.label + ": " + degree.description));
}
