using System.Windows;

namespace pluginTestW04.runner
{
    internal class TutorialRunnerEventArgs: RoutedEventArgs
    {
        public readonly bool SolutionSaved;

        public TutorialRunnerEventArgs(bool solutionSaved)
        {
            SolutionSaved = solutionSaved;
        }
    }
}
