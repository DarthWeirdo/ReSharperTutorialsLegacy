using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using JetBrains.Application.DataContext;
using JetBrains.Util;
using Process = System.Diagnostics.Process;

namespace pluginTestW04
{    

    public static class Utils
    {        
        public static void OpenVsSolution(IDataContext context, string path, TutorialId id)
        {           
            var globalOptions = context.GetComponent<GlobalOptions>();
            globalOptions.Id = id;
            globalOptions.Path = path;

            var vsInstance = GetCurrentVsInstance();
                        
            MessageBox.ShowMessageBox(globalOptions.Id.ToString() + " | " + 
                globalOptions.Path, MbButton.MB_OK, MbIcon.MB_ICONASTERISK);

            vsInstance.ExecuteCommand("File.OpenProject", path);

        }

        public static void UnloadTutorial(GlobalOptions globalOptions)
        {
            globalOptions.Id = TutorialId.None;
            globalOptions.Path = "noPath";
        }

        private static IEnumerable<DTE> EnumVsInstances()
        {            
            IRunningObjectTable rot;
            var retVal = GetRunningObjectTable(0, out rot);                
            if (retVal == 0)
            {
                IEnumMoniker enumMoniker;
                rot.EnumRunning(out enumMoniker);

                var fetched = IntPtr.Zero;
                var moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);                    
                    var isVisualStudio = displayName.StartsWith("!VisualStudio");
                    if (isVisualStudio)
                    {
                        object obj;
                        rot.GetObject(moniker[0], out obj);
                        var dte = obj as DTE;
                        yield return dte;
                    }
                }
            }
        }

        public static DTE GetCurrentVsInstance()
        {            
            IRunningObjectTable rot;
            GetRunningObjectTable(0, out rot);
            IEnumMoniker enumMoniker;
            rot.EnumRunning(out enumMoniker);
            enumMoniker.Reset();
            var fetched = IntPtr.Zero;
            var moniker = new IMoniker[1];
            while (enumMoniker.Next(1, moniker, fetched) == 0)
            {
                IBindCtx bindCtx;
                CreateBindCtx(0, out bindCtx);
                string displayName;
                moniker[0].GetDisplayName(bindCtx, null, out displayName);
                var isCurrentVsInstance = displayName.StartsWith("!VisualStudio") && displayName.Contains(Process.GetCurrentProcess().Id.ToString());
                if (isCurrentVsInstance)
                {
                    object obj;
                    rot.GetObject(moniker[0], out obj);
                    return (DTE)obj;
                }
            }
            return null;

        }

        public static string GetCurrentSolutionPath()
        {
            var dte = GetCurrentVsInstance();
            var solutionPath = Path.GetFullPath(dte.Solution.FullName);
            return solutionPath;
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

    }
}