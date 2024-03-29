﻿using System;
using JetBrains.DataFlow;
using pluginTestW04.checker;

namespace pluginTestW04.tutorialStep
{

    public enum GoToNextStep { Auto, Manual }

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
        public string Check { get; }
        public bool StrikeOnDone { get; }
        public string Text { get; set; }
        /// <summary>
        /// If GoToNextStep is specified as Manual or not specified, 
        /// a user can proceed to the next step ONLY by clicking the Next button. 
        /// </summary>
        public GoToNextStep GoToNextStep { get; }               
        
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
            string textToFind, int textToFindOccurrence, string action, string check, string goToNextStep, bool strkieOnDone)
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
            StrikeOnDone = strkieOnDone;
            _processingLifetime = null;

            if (goToNextStep != null && goToNextStep.ToLower() == "auto") GoToNextStep = GoToNextStep.Auto;
            else GoToNextStep = GoToNextStep.Manual;                                   
        }
       

        protected virtual void OnStepIsDone()
        {
            _processingLifetime.Terminate();
            StepIsDone?.Invoke(this, EventArgs.Empty);
        }


        public void PerformChecks(TutorialStepPresenter stepPresenter)
        {
            _processingLifetime = Lifetimes.Define(stepPresenter.Lifetime);

            var checker = new Checker(_processingLifetime.Lifetime, this, stepPresenter.Solution, stepPresenter.PsiFiles, stepPresenter.TextControlManager, stepPresenter.ShellLocks, stepPresenter.EditorManager, stepPresenter.DocumentManager, stepPresenter.ActionManager, stepPresenter.Environment);

            checker.PerformStepChecks();
        }
    }
}
