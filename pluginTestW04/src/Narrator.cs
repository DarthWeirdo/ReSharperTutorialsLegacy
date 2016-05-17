using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.TextControl;
using JetBrains.UI.Application;
using JetBrains.VsIntegration.Menu;
using tutorialUI;

namespace pluginTestW04
{    

    public class Narrator
    {
        private readonly IDataContext _context; // probably we don't need context here as it is actual only on the moment of creation        
        private SourceCodeNavigator _codeNavigator;
        private readonly TutorialWindow _tutorialWindow;

        private Dictionary<int, TutorialStep> _steps;        
        private int _currentStepId;  
//        private TextBlock _textPresenter;   // this is a reference to TextBlock on TutorialWindow

        public TutorialStep CurrentStep;  

        public Narrator(string contentPath, Lifetime lifetime, IDataContext context, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks, IEditorManager editorManager, 
                                  DocumentManager documentManager, IUIApplication environment)
        {
            _context = context;
            
            _codeNavigator = new SourceCodeNavigator(lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, 
                documentManager, environment);

            _tutorialWindow = new TutorialWindow();
            _steps = new Dictionary<int, TutorialStep>();            
            LoadTutorialContent(contentPath);

            // TODO: here must be some logic on checking current step (if a user've already proceeded to some step during previous sessions)
            _currentStepId = 1;  
            CurrentStep = _steps[_currentStepId];


            // TODO: Implement subscribing through the TutorialWindow.Subscribe()
            var btnNext =  (Button) _tutorialWindow.FindName("BtnNext");
            if (btnNext != null) btnNext.Click += delegate { GoNext(); };

            var btnSaveClose = (Button)_tutorialWindow.FindName("BtnSaveClose");
            if (btnSaveClose != null) btnSaveClose.Click += delegate { SaveAndClose(); };           


            /*
            _textPresenter = (TextBlock) _tutorialWindow.FindName("TextContent");
            _textPresenter?.SetBinding(TextBlock.TextProperty, new Binding("Text")
            {
                Source = CurrentStep,
                Mode = BindingMode.OneWay
            });
            */

            //            _tutorialWindow.SetBinding(_tutorialWindow.TextProperty, new Binding("Text")
            //            {
            //                Source = CurrentStep,
            //                Mode = BindingMode.OneWay
            //            });


        }


        private void LoadTutorialContent(string contentPath)
        {
            // TODO: Probably we should do this async if content loading is too long + display progress
            _steps = TutorialXmlReader.ReadTutorialSteps(contentPath);
        }

        public void Start()
        {
            // TODO: Implement restoring previous tutorial state

            ShowWindow();
            ProcessStep(_currentStepId);
        }

        public void SaveAndClose()
        {
            // TODO: Implement saving current tutorial state
            CloseWindow();
            // TODO: close solution, unsubscribe

        }

        private void ProcessStep(int stepId)
        {
            // TODO: Preparation - Open required cs file, move cursor to required line, prepare to post-step activities (subscrive to particular event, etc.)
            ShowStepText(stepId);

            _codeNavigator.TestPsi();
            
        }

        private void ShowStepText(int stepId)
        {
            _tutorialWindow.TutorialText = _steps[stepId].Text;
        }
            
        public void ShowWindow()
        {
            _tutorialWindow.Show();
        }

        private void CloseWindow()
        {
            _tutorialWindow.Close();
        }

        public void GoNext()
        {
            if (_currentStepId < _steps.Count)
            {
                _currentStepId++;
                CurrentStep = _steps[_currentStepId];
                ProcessStep(_currentStepId);
            }

            // TODO: raise event that tutorial is over, buttons in _tutorialWindow must be subscribed to this event.
            
        }

        public void GoPrevious()
        {
            // TODO: Probably, remove this at all. This action makes the step inconsistent with the code state!
            _currentStepId--;
            CurrentStep = _steps[_currentStepId];
        }
    }
}
