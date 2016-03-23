using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.UI.ActionsRevised;

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
}