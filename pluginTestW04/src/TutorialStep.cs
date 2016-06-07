using System;
using System.ComponentModel;
using System.Windows;
using JetBrains.Annotations;

namespace pluginTestW04
{
    public delegate void StepIsDoneHandler(object sender, EventArgs e);

    public class TutorialStep : INotifyPropertyChanged
    {        
        // TODO: some event? or property subscribed to event that indicates the step is completed.        

        private string _text;
        private bool _isDone;
//        private readonly StepActionCheckerEventStyle _stepActionChecker;

        public int Id { get; }
        public string ProjectName { get;}
        public string FileName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public string TextToFind { get; }
        public int TextToFindOccurrence { get; }
        public string Action { get; }
        
        // TODO: this is the check that must be run after the Action is performed (if it exists).
        // TODO: all checks can be placed in a separate static class
        public Func<bool> Check;
        private bool _isActionDone = false;
        public event StepIsDoneHandler StepIsDone;


        public bool IsActionDone
        {
            get { return _isActionDone; }
            set
            {
                if (value == _isActionDone) return;                
                _isActionDone = value;

                if (Check == null)
                {
                    OnStepIsDone();
                }
            }
        }

        public bool IsCheckDone { get; set; } = false;

        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (value == _isDone)
                    return;

                _isDone = value;
                OnPropertyChanged("IsDone");
            }
        }

        public TutorialStep(int li, string text, string file, string projectName, string typeName, string methodName, 
            string textToFind, int textToFindOccurrence, string action)
        {
            Id = li;
            _text = text;
            FileName = file;
            TypeName = typeName;
            Action = action;
            TextToFindOccurrence = textToFindOccurrence;
            ProjectName = projectName;
            MethodName = methodName;
            TextToFind = textToFind;
            _isDone = false;

            // trying to implement logic checks right inside step class
//            if (action != null)
//            {
//                _stepActionChecker = new StepActionCheckerEventStyle(action);
//                _stepActionChecker.ActionApplied += StepActionCheckerOnActionApplied;
//            }                                    
        }

//        public void Unsubscribe()
//        {
//            if (Action != null)            
//                _stepActionChecker.ActionApplied -= StepActionCheckerOnActionApplied;                        
//        }
//
//        private void StepActionCheckerOnActionApplied(object sender, EventArgs eventArgs)
//        {
//            IsActionDone = true;
//            MessageBox.Show(Action, " DONE!!!");
//        }


        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text)
                    return;

                _text = value;
                OnPropertyChanged("Text");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

        protected virtual void OnStepIsDone()
        {
            StepIsDone?.Invoke(this, EventArgs.Empty);
        }
    }
}
