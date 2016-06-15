using System;
using JetBrains.ActionManagement;
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
                                  [NotNull] GlobalSettings globalSettings, TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment, IActionManager actionManager)
        {
            if (lifetime == null)
                throw new ArgumentNullException("lifetime");
            if (solutionStateTracker == null)
                throw new ArgumentNullException("solutionStateTracker");
            if (globalSettings == null)
                throw new ArgumentNullException("globalSettings");
            
            // solutionStateTracker.BeforeSolutionClosed.Advise(lifetime, () =>{...});
            lifetime.AddAction(() =>
            {
                if (_narrator.SolutionSaved)
                {
                    _narrator.SaveAndClose(this, null);
                }
            });
                            
            // TODO: replace with foreach; make List<> GlobalSettings.TutorialPaths
            if (VsCommunication.GetCurrentSolutionPath() ==
                globalSettings.GetPath(TutorialId.Tutorial1, PathType.WorkCopySolutionFile))
            {
//                solutionStateTracker.AfterSolutionOpened.Advise(lifetime, sol => RunTutorial(GlobalSettings.GetPath(TutorialId.Tutorial1, PathType.WorkCopyContentFile), lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, documentManager, environment));

                solutionStateTracker.AfterPsiLoaded.Advise(lifetime, sol => RunTutorial(globalSettings.GetPath(TutorialId.Tutorial1, PathType.WorkCopyContentFile), lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, documentManager, environment, actionManager));
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
