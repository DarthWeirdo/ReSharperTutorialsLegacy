using System;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Intentions.Bulbs;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.UI.Application;

namespace pluginTestW04
{       
    [SolutionComponent]
    public class TutorialRunner
    {
        private Narrator _narrator;

        public TutorialRunner([NotNull] Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  [NotNull] ISolutionStateTracker solutionStateTracker,
                                  [NotNull] GlobalOptions globalOptions, TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment)
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
//                _narrator?.SaveAndClose();
            });
                            
            // TODO: replace with foreach; make List<> globalOptions.TutorialPaths
            if (VsCommunication.GetCurrentSolutionPath() == globalOptions.Tutorial1Path)
                    solutionStateTracker.AfterSolutionOpened.Advise(lifetime, sol => RunTutorial(globalOptions.Tutorial1ContentPath, lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, documentManager, environment));            
        }

        private void RunTutorial(string contentPath, Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,                                 
                                  TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment)
        {            
            _narrator = new Narrator(contentPath, lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager,                             documentManager, environment);
            _narrator.Start();
        }   
    }
}
