using System;
using System.IO;
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
    [Action("ActionOpenTutorial1", "Start Tutorial 1 - Essential Shortcuts", Id = 100)]
    public class ActionOpenTutorial1: ActionOpenTutorial
    {        
        protected override void OpenTutorial(IDataContext context, DelegateExecute nextExecute)
        {                                   
            MessageBox.ShowMessageBox("This will close your current solution and open a tutorial solution", MbButton.MB_OK, MbIcon.MB_ICONASTERISK);

            var globalOptions = context.GetComponent<GlobalOptions>();        
            Utils.OpenVsSolution(context, globalOptions.Tutorial1Path);            
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

            lifetime.AddAction(() => { Utils.UnloadTutorial(globalOptions);}); // this can be replaced with:  
                                                                               // solutionStateTracker.BeforeSolutionClosed.Advise(lifetime, () =>{...});
                                                                                         
            if (Utils.GetCurrentSolutionPath() == globalOptions.Tutorial1Path)
                    solutionStateTracker.AfterSolutionOpened.Advise(lifetime, solution => RunTutorial());            
        }

        private static void RunTutorial()
        {            
            var wnd = new TutorialWindow();
            wnd.Show();
        }       
    }    
}
