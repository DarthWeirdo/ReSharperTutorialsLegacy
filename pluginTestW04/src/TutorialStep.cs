using System;
using System.ComponentModel;
using JetBrains.Annotations;
using JetBrains.DataFlow;
using NUnit.Framework;

namespace pluginTestW04
{

    public enum NextStep { Auto, Manual }

    public delegate void StepIsDoneHandler(object sender, EventArgs e);

    public class TutorialStep : INotifyPropertyChanged
    {                
        private string _text;        
//        private readonly StepActionCheckerEventStyle _stepActionChecker;

        public int Id { get; }
        public string ProjectName { get;}
        public string FileName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public string TextToFind { get; }
        public int TextToFindOccurrence { get; }
        public string Action { get; }
        public string Check;
        /// <summary>
        /// If NextStep is specified as Manual or not specified, 
        /// a user can proceed to the next step ONLY by clicking the Next button. 
        /// </summary>
        public NextStep NextStep { get; }               
        
        private bool _isActionDone;
        private bool _isCheckDone;
        public event StepIsDoneHandler StepIsDone;

        /// <summary>
        /// Lifetime created for the duration of performing checks
        /// </summary>
        private LifetimeDefinition _processingLifetime;        


        public bool IsActionDone
        {
            get { return _isActionDone; }
            set
            {
                if (value == _isActionDone) return;                
                _isActionDone = value;

                if (Check == null || IsCheckDone)                
                    OnStepIsDone();                
            }
        }

        public bool IsCheckDone
        {
            get
            {
                return _isCheckDone;
            }
            set
            {
                if (value == _isCheckDone) return;
                _isCheckDone = value;

                if (Action != null && IsActionDone)                
                    OnStepIsDone();                
                else if (Action == null)
                    OnStepIsDone();                
            }
        } 
        

        public TutorialStep(int li, string text, string file, string projectName, string typeName, string methodName, 
            string textToFind, int textToFindOccurrence, string action, string check, string nextStep)
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
            Check = check;
            _processingLifetime = null;

            if (nextStep != null && nextStep.ToLower() == "auto") NextStep = NextStep.Auto;
            else NextStep = NextStep.Manual;
            

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
            _processingLifetime.Terminate();
            StepIsDone?.Invoke(this, EventArgs.Empty);
        }


        public void PerformChecks(Narrator ownerNarrator)
        {            
            _processingLifetime = Lifetimes.Define(ownerNarrator.Lifetime);
            var checker = new Checker(_processingLifetime.Lifetime, this, ownerNarrator.Solution, ownerNarrator.PsiFiles, ownerNarrator.TextControlManager, ownerNarrator.ShellLocks, ownerNarrator.EditorManager, ownerNarrator.DocumentManager, ownerNarrator.ActionManager, ownerNarrator.Environment);
            checker.PerformStepChecks();
        }
    }
}
