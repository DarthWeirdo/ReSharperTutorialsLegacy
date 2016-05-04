using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Util;

namespace pluginTestW04
{
    public static class TutorialXmlReader
    {

        public static string ReadIntro(string path)
        {
            using (var reader = XmlReader.Create(new StreamReader(path)))
            {
                while (reader.ReadToFollowing("intro"))
                {
                    return reader.ReadInnerXml();
                }
            }

            return "Missing tutorial content. Please reinstall the plugin!";

            //var document = XDocument.Parse(path);
            //var xElement = document.Element("tutorial");
            //return xElement?.Element("intro").Value ?? "Missing tutorial content. Please reinstall the plugin";
        }

        
        public static Dictionary<int, TutorialStep> ReadTutorialSteps(string path)
        {
            var result = new Dictionary<int, TutorialStep>();

            using (var reader = XmlReader.Create(new StreamReader(path)))
            {
                while (reader.ReadToFollowing("step"))
                {
                    var li = Convert.ToInt32(reader.GetAttribute("li"));
                    var file = reader.GetAttribute("file");
                    var treenode = reader.GetAttribute("treenode");
                    var buttons = reader.GetAttribute("buttons");
                    reader.ReadToFollowing("text");
                    var text = reader.ReadInnerXml();
                    text = Regex.Replace(text, @"\s+", " ");

                    var step = new TutorialStep(li, text, file, treenode, buttons);
                    result.Add(li, step);
                }
            }
            return result;
        }
    }
}
