﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Util;

namespace pluginTestW04
{
    public static class TutorialXmlReader
    {

        public static string ReadCurrentStep(string path)
        {
            using (var reader = XmlReader.Create(new StreamReader(path)))
            {
                while (reader.ReadToFollowing("currentStep"))
                {
                    return reader.ReadElementContentAsString();                    
                }
            }

            return "Missing tutorial content. Please reinstall the plugin!";
        }

        public static void WriteCurrentStep(string path, string value)
        {            
            var doc = new XmlDocument();
            doc.Load(path);
            var node = doc.SelectSingleNode("/tutorial/" + "currentStep");
            if (node != null)
            {
                node.InnerText = value;
            }
            else
            {
                XmlNode root = doc.DocumentElement;
                var elem = doc.CreateElement("currentStep");
                elem.InnerText = value;
                root.AppendChild(elem);
            }
            doc.Save(path);
            doc = null;          
        }

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
                    var typeName = reader.GetAttribute("type");
                    var methodName = reader.GetAttribute("method");
                    var projectName = reader.GetAttribute("project");
                    var textToFind = reader.GetAttribute("textToFind");
                    var buttons = reader.GetAttribute("buttons");
                    reader.ReadToFollowing("text");
                    var text = reader.ReadInnerXml();
                    text = Regex.Replace(text, @"\s+", " ");

                    if ((file == null) || (projectName == null))
                    {
                        throw new Exception("Tutorial content file is corrupted. Please reinstall the plugin.");
                    }
                    var step = new TutorialStep(li, text, file, projectName, typeName, methodName, textToFind, buttons);                    
                    result.Add(li, step);
                }
            }
            return result;
        }
    }

        
    }
