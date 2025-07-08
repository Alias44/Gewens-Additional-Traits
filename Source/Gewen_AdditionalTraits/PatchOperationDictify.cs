using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace Gewen_AdditionalTraits;

#if RELEASE_1_4 || RELEASE_1_3 || RELEASE_1_2 || RELEASE_1_1
/// <summary>
/// Patch class to convert list definitions to legacy dictionary system.
/// Intetnded for use with skillGains.
/// </summary>
/// <remarks>Defs/TraitDef[starts-with(defName,"GAT_")]/degreeDatas/li/skillGains</remarks>
public class PatchOperationDictify : PatchOperationPathed
{
	protected override bool ApplyWorker(XmlDocument xml)
	{
		bool flag = false;

		foreach (XmlNode skillGains in xml.SelectNodes(this.xpath).Cast<XmlNode>())
		{
			var newNodes = new List<XmlNode>();

			foreach (XmlNode skill in skillGains.ChildNodes)
			{
				var li = xml.CreateNode(XmlNodeType.Element, "li","");

				var key = xml.CreateNode(XmlNodeType.Element, "key", "");
				key.InnerText = skill.Name;
				li.AppendChild(key);
				
				var value = xml.CreateNode(XmlNodeType.Element, "value", "");
				value.InnerText = skill.InnerText;
				li.AppendChild(value);

				newNodes.Add(li);
			}

			skillGains.RemoveAll();

			newNodes.ForEach(li => skillGains.AppendChild(li));

			flag = true;
		}
		return flag;
	}
}
#endif
