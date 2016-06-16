using System;
using JetBrains.DataFlow;
using pluginTestW04.checker;

namespace pluginTestW04.narrator
{

    public enum NextStep { Auto, Manual }

    public delegate void StepIsDoneHandler(object sender, EventArgs e);

    public class TutorialStep 
    {
        public int Id { get; }
        public string ProjectName { get;}
        public string FileName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public string TextToFind { get; }
        public int TextToFindOccurrence { get; }
        public string Action { get; }
        public string Check;
        public string Text { get; set; }
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
            Text = text;
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
