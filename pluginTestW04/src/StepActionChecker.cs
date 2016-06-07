using System;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.UI.Avalon.TreeListView;

namespace pluginTestW04
{    

    /// <summary>
    /// Used to check whether a user performed an action required by the step.
    /// Action is specified as an argument in XML
    /// </summary>
    public class StepActionChecker
    {
        private static DTE _vsInstance;
        private static CommandEvents _commandEvents;
        public string StepActionName;
        public ISignal<bool> AfterActionApplied { get; private set; }
                    

        public StepActionChecker(Lifetime lifetime, IActionManager actionManager)
        {
            _vsInstance = VsCommunication.GetCurrentVsInstance();
            var events2 = _vsInstance?.Events as Events2;
            if (events2 == null) return;

            _commandEvents = events2.CommandEvents;

            lifetime.AddBracket(
                () => _commandEvents.AfterExecute += CommandEventsOnAfterExecute, 
                () => _commandEvents.AfterExecute -= CommandEventsOnAfterExecute);

            AfterActionApplied = new Signal<bool>(lifetime, "StepActionChecker.AfterActionApplied");
        }        

        private void CommandEventsOnAfterExecute(string guid, int id1, object customIn, object customOut)
        {            
            if (_vsInstance == null) return;

            var command = _vsInstance.Commands.Item(guid, id1);

            if (command.Name == StepActionName)
            {
                AfterActionApplied.Fire(true);                
            }
        }
        
    }



    public delegate void ActionAppliedHandler(object sender, EventArgs e);

    public class StepActionCheckerEventStyle
    {
        private static DTE _vsInstance;
        private static CommandEvents _commandEvents;
        private readonly string _stepActionName;        

        public event ActionAppliedHandler ActionApplied;


        public StepActionCheckerEventStyle(string stepActionName)
        {
            _stepActionName = stepActionName;
            _vsInstance = VsCommunication.GetCurrentVsInstance();
            var events2 = _vsInstance?.Events as Events2;
            if (events2 == null) return;

            _commandEvents = events2.CommandEvents;
            _commandEvents.AfterExecute += CommandEventsOnAfterExecute;            
        }

        private void CommandEventsOnAfterExecute(string guid, int id1, object customIn, object customOut)
        {
            if (_vsInstance == null) return;

            var command = _vsInstance.Commands.Item(guid, id1);

            if (command.Name != _stepActionName) return;
            OnActionApplied();
            Unsubscribe();
        }

        protected virtual void OnActionApplied()
        {
            ActionApplied?.Invoke(this, EventArgs.Empty);
        }

        private void Unsubscribe()
        {
            _commandEvents.AfterExecute -= CommandEventsOnAfterExecute;
        }

//        public void Dispose()
//        {
//            _commandEvents.AfterExecute -= CommandEventsOnAfterExecute;
//        }
    }
}
