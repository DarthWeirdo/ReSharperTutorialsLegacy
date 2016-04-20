﻿using System.IO;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;

namespace pluginTestW04
{
    public enum TutorialId { None, Tutorial1, Tutorial2, Tutorial3, Tutorial4, Tutorial5 }
    
    [ShellComponent]
    public class GlobalOptions
    {        
        public string Tutorial1Path;

        public GlobalOptions([NotNull]Lifetime lifetime)
        {        
            var commonTutorialPath = Utils.GetTutorialsPath();
            if (commonTutorialPath == null)
                throw new DirectoryNotFoundException("Unable to find the folder with sample solutions. Please reinstall the plugin");

            Tutorial1Path = commonTutorialPath + "\\Tutorial1_EssentialShortcuts\\Tutorial1_EssentialShortcuts.sln";
        }
    }    
}