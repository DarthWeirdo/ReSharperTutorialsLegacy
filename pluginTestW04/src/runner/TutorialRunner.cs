using System;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.UI.Application;
using pluginTestW04.utils;

namespace pluginTestW04.runner
{       
    [SolutionComponent]
    public class TutorialRunner
    {
        private Narrator _narrator;

        public TutorialRunner([NotNull] Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  [NotNull] ISolutionStateTracker solutionStateTracker,
                                  [NotNull] GlobalSettings globalSettings, TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment, IActionManager actionManager)
        {
            if (lifetime == null)
                throw new ArgumentNullException("lifetime");
            if (solutionStateTracker == null)
                throw new ArgumentNullException("solutionStateTracker");
            if (globalSettings == null)
                throw new ArgumentNullException("globalSettings");
                        
            lifetime.AddAction(() =>
            {
                var args = new TutorialRunnerEventArgs(VsCommunication.IsSolutionSaved());
                _narrator.SaveAndClose(this, args);                
            });

            foreach (var tutorial in globalSettings.AvailableTutorials)
            {
                if (VsCommunication.GetCurrentSolutionPath() == tutorial.Value)
                {
                    solutionStateTracker.AfterPsiLoaded.Advise(lifetime, 
                    sol => RunTutorial(globalSettings.GetPath(tutorial.Key, PathType.WorkCopyContentFile), lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, documentManager, environment, actionManager));                    
                }
            }                                              
        }

        private void RunTutorial(string contentPath, Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,                                 
                                  TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment, IActionManager actionManager)
        {            
            _narrator = new Narrator(contentPath, lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager,                             documentManager, environment, actionManager);
            _narrator.Start();
        }   
    }
}
