using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;

namespace pluginTestW04
{
    public enum TutorialId { None, Tutorial1, Tutorial2, Tutorial3, Tutorial4, Tutorial5 }
    
    [ShellComponent]
    public class GlobalOptions
    {
        public TutorialId? Id { get; set; }
        public string Path { get; set; }

        public GlobalOptions([NotNull]Lifetime lifetime)
        {
            Id = TutorialId.None;
            Path = "";
        }
    }

    public static class Constants
    {
        public static string Tutorial1Path = "e:\\myproject\\MassFileProcessing.sln";
    }
}