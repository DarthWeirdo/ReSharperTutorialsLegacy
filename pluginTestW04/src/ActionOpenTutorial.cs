using System.Windows;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.UI.ActionsRevised;
using JetBrains.Util;
using tutorialUI;

namespace pluginTestW04
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

    }

    [Action("ActionOpenTutorial1", "Start Tutorial 1 - Essential Shortcuts", Id = 100)]
    public class ActionOpenTutorial1 : ActionOpenTutorial
    {
        protected override void OpenTutorial(IDataContext context, DelegateExecute nextExecute)
        {
            var globalOptions = context.GetComponent<GlobalOptions>();            
            var titleString = TutorialXmlReader.ReadIntro(globalOptions.Tutorial1ContentPath);
            var titleWnd = new TitleWindow(titleString);

            // TODO: Check result of the dialog
            titleWnd.ShowDialog();
            
            VsCommunication.OpenVsSolution(context, globalOptions.Tutorial1Path);
        }
    }
}