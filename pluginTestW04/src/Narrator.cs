using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.UI.Application;
using tutorialUI;

namespace pluginTestW04
{    

    public class Narrator
    {
//        private readonly Checker _checker;
        private readonly SourceCodeNavigator _codeNavigator;
        private readonly TutorialWindow _tutorialWindow;
        private readonly string _contentPath;        
        private Dictionary<int, TutorialStep> _steps;        
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

        public bool SolutionSaved => VsCommunication.GetSolutionSaved();


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
//            _checker = new Checker(lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, documentManager,
//                actionManager, environment);

            LoadTutorialContent(contentPath);
            
            _currentStepId = TutorialXmlReader.ReadCurrentStep(contentPath);
            CurrentStep = _steps[_currentStepId];            

            _tutorialWindow.Subscribe(UiActionType.Next, GoNext);
            _tutorialWindow.Subscribe(UiActionType.SaveClose, SaveAndClose);
        }
            
        public void SaveAndClose(object sender, RoutedEventArgs args)
        {            
            TutorialXmlReader.WriteCurrentStep(_contentPath, _currentStepId.ToString());
            _tutorialWindow.Unsubscribe();
            _tutorialWindow.Close();

            VsCommunication.CloseVsSolution(sender.GetType() != typeof (TutorialRunner));
        }

        private void GoNext(object sender, RoutedEventArgs args)
        {
            if (_currentStepId == _steps.Count - 1)
            {
                _tutorialWindow.HideNextButton();
                return;
            }

            _currentStepId++;
            CurrentStep = _steps[_currentStepId];
            ProcessStep();            
        }


        private void LoadTutorialContent(string contentPath)
        {
            // TODO: Probably we should do this async if content loading is too long + display progress
            _steps = TutorialXmlReader.ReadTutorialSteps(contentPath);
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
//                _checker.PerformStepChecks(CurrentStep);
            }
            else if (_currentStepId < _steps.Count - 1)                     
                _tutorialWindow.ShowNextButton();            
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