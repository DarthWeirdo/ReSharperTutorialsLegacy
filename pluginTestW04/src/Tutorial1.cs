using System;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.UI.ActionsRevised;
using JetBrains.Util;
using tutorialUI;

namespace pluginTestW04
{    
    [Action("ActionOpenTutorial1", "Start Tutorial 1 - First Steps", Id = 100)]
    public class ActionOpenTutorial1: ActionOpenTutorial
    {
        private static string _tutorialPath = "e:\\myproject\\MassFileProcessing.sln"; 
        private static TutorialId _tutorialId = TutorialId.Tutorial1;

        protected override void OpenTutorial(IDataContext context, DelegateExecute nextExecute)
        {                       
            MessageBox.ShowMessageBox("This will close your current solution and open a tutorial solution", MbButton.MB_OK, MbIcon.MB_ICONASTERISK);
//            Utils.OpenVsSolution(context, _tutorialPath, _tutorialId);            
            Utils.OpenVsSolution(context, Constants.Tutorial1Path, _tutorialId);            
        }        
    }

    [SolutionComponent]
    public class Tutorial1
    {        
        public Tutorial1([NotNull] Lifetime lifetime,
                                  [NotNull] ISolutionStateTracker solutionStateTracker,
                                  [NotNull] GlobalOptions globalOptions)
        {
            if (lifetime == null)
                throw new ArgumentNullException("lifetime");
            if (solutionStateTracker == null)
                throw new ArgumentNullException("solutionStateTracker");
            

            //            lifetime.AddAction(() => { Utils.UnloadTutorial(globalOptions);});            

            //            var isTutorial1 = (globalOptions.Id == TutorialId.Tutorial1) &&
            //                (Utils.GetCurrentSolutionPath() == globalOptions.Path);
            //
            //            MessageBox.ShowMessageBox(globalOptions.Id.ToString() + '=' + TutorialId.Tutorial1 + " | " +
            //                                        Utils.GetCurrentSolutionPath() + "=" + globalOptions.Path + " | " + isTutorial1.ToString(),
            //                                        MbButton.MB_OK, MbIcon.MB_ICONASTERISK);



            //            if (isTutorial1)


            MessageBox.ShowMessageBox(Utils.GetCurrentSolutionPath(), MbButton.MB_OK, MbIcon.MB_ICONASTERISK);

            if (Utils.GetCurrentSolutionPath() == Constants.Tutorial1Path)
                    solutionStateTracker.AfterSolutionOpened.Advise(lifetime, solution => RunTutorial());            

            solutionStateTracker.BeforeSolutionClosed.Advise(lifetime, () =>
            {
                MessageBox.ShowMessageBox("I'm dead!!! SolutionStateTracker", MbButton.MB_OK,
                    MbIcon.MB_ICONASTERISK);
                Utils.UnloadTutorial(globalOptions);
            });

        }

        private static void RunTutorial()
        {
            MessageBox.ShowMessageBox("The solution is opened. We are ready to start!", MbButton.MB_OK, MbIcon.MB_ICONASTERISK);

            var wnd = new TutorialWindow();
            wnd.Show();
        }       
    }    
}
