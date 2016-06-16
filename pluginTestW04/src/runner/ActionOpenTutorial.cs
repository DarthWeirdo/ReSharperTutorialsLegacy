using System;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.UI.ActionsRevised;
using pluginTestW04.utils;
using tutorialUI;

namespace pluginTestW04.runner
{    
    public abstract class ActionOpenTutorial : IExecutableAction
    {

        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            OpenTutorial(context, nextExecute);
        }

        protected abstract void OpenTutorial(IDataContext context, DelegateExecute nextExecute);

        public void OpenOrRestart(IDataContext context, TutorialId id)
        {
            var globalOptions = context.GetComponent<GlobalSettings>();
            var titleString = TutorialXmlReader.ReadIntro(globalOptions.GetPath(id, PathType.WorkCopyContentFile));
            var step = TutorialXmlReader.ReadCurrentStep(globalOptions.GetPath(id, PathType.WorkCopyContentFile));
            var firstTime = step == 1;

            var titleWnd = new TitleWindow(titleString, firstTime);

            if (titleWnd.ShowDialog() != true) return;
            if (titleWnd.Restart)
            {
                SolutionCopyHelper.CopySolution(globalOptions.GetPath(id, PathType.BaseSolutionFolder),
                    globalOptions.GetPath(id, PathType.WorkCopySolutionFolder));

                GC.Collect();
                TutorialXmlReader.WriteCurrentStep(globalOptions.GetPath(id, PathType.WorkCopyContentFile), "1");

                VsCommunication.OpenVsSolution(globalOptions.GetPath(id, PathType.WorkCopySolutionFile));
            }
            else
                VsCommunication.OpenVsSolution(globalOptions.GetPath(id, PathType.WorkCopySolutionFile));
        }

    }

    [Action("ActionOpenTutorial1", "Start Tutorial 1 - Essential Shortcuts", Id = 100)]
    public class ActionOpenTutorial1 : ActionOpenTutorial
    {
        protected override void OpenTutorial(IDataContext context, DelegateExecute nextExecute)
        {
            OpenOrRestart(context, TutorialId.Tutorial1);
        }
    }
}