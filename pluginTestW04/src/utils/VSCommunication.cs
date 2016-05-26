using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using JetBrains.Application.DataContext;
using JetBrains.Util;
using Process = System.Diagnostics.Process;

namespace pluginTestW04
{    

    public static class VsCommunication
    {

        public const string PluginName = "huy.pluginTestW04";

        public static void FindTextInCurrentDocument(string text, int occurrence)
        {
            var vsInstance = GetCurrentVsInstance();
            var selection = vsInstance.ActiveDocument.Selection as TextSelection;
            if (occurrence == 0) occurrence = 1;
            for (int i = 1; i <= occurrence; i++)            
                selection?.FindText(text);                        
            selection?.MoveToPoint(selection.BottomPoint);
        }

        public static void DropSelection()
        {
            var vsInstance = GetCurrentVsInstance();
            var selection = vsInstance.ActiveDocument.Selection as TextSelection;
            selection?.MoveToPoint(selection.BottomPoint);
        }


        public static string GetTutorialsPath()
        {
            var pluginsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\JetBrains\\plugins";
            var dirs = Directory.GetDirectories(pluginsPath);
            string result = null;

            foreach (var dir in dirs.Where(dir => dir.Contains(PluginName)))
            {
                result = dir + "\\Tutorials";
            }

            return result;
        }

        public static void OpenVsSolution(string path)
        {                       
            var vsInstance = GetCurrentVsInstance();

            vsInstance.ExecuteCommand("File.OpenProject", path);
        }

        public static void SaveVsSolution()
        {
            var vsInstance = GetCurrentVsInstance();

            vsInstance.ExecuteCommand("File.SaveAll");
        }

        public static void CloseVsSolution()
        {
            var vsInstance = GetCurrentVsInstance();

            vsInstance.ExecuteCommand("File.CloseSolution");
        }

        public static void UnloadTutorial(GlobalOptions globalOptions)
        {            
        }

        private static IEnumerable<DTE> EnumVsInstances()
        {            
            IRunningObjectTable rot;
            int retVal = GetRunningObjectTable(0, out rot);
            if (retVal != 0) yield break;
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
                if (!isVisualStudio) continue;
                object obj;
                rot.GetObject(moniker[0], out obj);
                var dte = obj as DTE;
                yield return dte;
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
                if (!isCurrentVsInstance) continue;
                object obj;
                rot.GetObject(moniker[0], out obj);
                return (DTE)obj;
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