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
    [SolutionComponent]
    public class TutorialRunner
    {
        private Narrator _narrator;

        public TutorialRunner([NotNull] Lifetime lifetime,
                                  [NotNull] ISolutionStateTracker solutionStateTracker,
                                  [NotNull] GlobalOptions globalOptions,
                                  IDataContext context)
        {
            if (lifetime == null)
                throw new ArgumentNullException("lifetime");
            if (solutionStateTracker == null)
                throw new ArgumentNullException("solutionStateTracker");
            if (globalOptions == null)
                throw new ArgumentNullException("globalOptions");

            //            lifetime.AddAction(() => { VSCommunication.UnloadTutorial(globalOptions);}); // this can be replaced with:  
            // solutionStateTracker.BeforeSolutionClosed.Advise(lifetime, () =>{...});
            lifetime.AddAction(() =>
            {
                _narrator?.SaveAndClose();
            });

            // TODO: replace with foreach; make List<> globalOptions.TutorialPaths
            if (VsCommunication.GetCurrentSolutionPath() == globalOptions.Tutorial1Path)
                    solutionStateTracker.AfterSolutionOpened.Advise(lifetime, solution => RunTutorial(globalOptions.Tutorial1ContentPath, context));            
        }

        private void RunTutorial(string contentPath, IDataContext context)
        {
            _narrator = new Narrator(contentPath, context);
            _narrator.Start();
        }   
    }
}
