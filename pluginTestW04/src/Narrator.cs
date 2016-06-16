using System;
using System.Collections.Generic;
using System.Windows;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.UI.Application;
using pluginTestW04.utils;
using tutorialUI;

namespace pluginTestW04
{    

    public class Narrator
    {
        private readonly SourceCodeNavigator _codeNavigator;
        private readonly TutorialWindow _tutorialWindow;
        private readonly string _contentPath;        
        private readonly Dictionary<int, TutorialStep> _steps;        
        private int _currentStepId;

        public readonly Lifetime Lifetime;
        public readonly ISolution Solution;
        public readonly IPsiFiles PsiFiles;
        public readonly TextControlManager TextControlManager;
        public readonly IShellLocks ShellLocks;
        public readonly IEditorManager EditorManager;
        public readonly DocumentManager DocumentManager;
        public readonly IUIApplication Environment;
        public readonly IActionManager ActionManager;
        public TutorialStep CurrentStep { get; set; }        


        public Narrator(string contentPath, Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks, IEditorManager editorManager, 
                                  DocumentManager documentManager, IUIApplication environment, IActionManager actionManager)
        {            
            Lifetime = lifetime;
            Solution = solution;
            PsiFiles = psiFiles;
            TextControlManager = textControlManager;
            ShellLocks = shellLocks;
            EditorManager = editorManager;
            DocumentManager = documentManager;
            Environment = environment;
            ActionManager = actionManager;
            _contentPath = contentPath;            
            _codeNavigator = new SourceCodeNavigator(lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, 
                documentManager, environment);
            _tutorialWindow = new TutorialWindow();
            _steps = new Dictionary<int, TutorialStep>();
            _steps = TutorialXmlReader.ReadTutorialSteps(contentPath);

            _currentStepId = TutorialXmlReader.ReadCurrentStep(contentPath);
            CurrentStep = _steps[_currentStepId];            

            _tutorialWindow.Subscribe(UiActionType.Next, GoNext);
            _tutorialWindow.Subscribe(UiActionType.SaveClose, SaveAndClose);
        }
            
        public void SaveAndClose(object sender, RoutedEventArgs args)
        {                        
            _tutorialWindow.Unsubscribe();
            _tutorialWindow.Close();

            // TODO: The suggestion is to remove saving functionality at all
            VsCommunication.CloseVsSolution(true);
            /* if (sender.GetType() != typeof(TutorialRunner))
            {
                TutorialXmlReader.WriteCurrentStep(_contentPath, _currentStepId.ToString());
                VsCommunication.CloseVsSolution(true);
            }
            else
            {
                var trArgs = (TutorialRunnerEventArgs) args;
                if (trArgs.SolutionSaved)               
                    TutorialXmlReader.WriteCurrentStep(_contentPath, _currentStepId.ToString());                
            }*/
        }

        private void GoNext(object sender, RoutedEventArgs args)
        {
            if (_currentStepId == _steps.Count) return;            

            _currentStepId++;
            CurrentStep = _steps[_currentStepId];
            ProcessStep();            
        }


        public void Start()
        {                 
            ShowWindow();
            ProcessStep();
        }        


        private void ProcessStep()
        {            
            ShowText(CurrentStep);            
            _codeNavigator.Navigate(CurrentStep);

            if (CurrentStep.NextStep == NextStep.Auto) 
            {
                _tutorialWindow.HideNextButton();
                CurrentStep.StepIsDone += StepOnStepIsDone;
                CurrentStep.PerformChecks(this);
            }
            else
                _tutorialWindow.ShowNextButton();

            if (_currentStepId == _steps.Count)            
                _tutorialWindow.HideNextButton();
                              
        }


        private void StepOnStepIsDone(object sender, EventArgs eventArgs)
        {
            CurrentStep.StepIsDone -= StepOnStepIsDone;
            GoNext(this, null);
        }
        

        private void ShowText(TutorialStep step)
        {
            _tutorialWindow.TutorialText = step.Text;
        }

        private void ShowWindow()
        {
            _tutorialWindow.Show();

        }
          
    }
}