using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private readonly SourceCodeNavigator _codeNavigator;
        private readonly TutorialWindow _tutorialWindow;
        private readonly string _contentPath;
        private readonly Button _btnNext;
        private readonly Button _btnSaveClose;

        private Dictionary<int, TutorialStep> _steps;        
        private int _currentStepId;  
//        private TextBlock _textPresenter;   // this is a reference to TextBlock on TutorialWindow

        public TutorialStep CurrentStep => _steps[_currentStepId];


        public Narrator(string contentPath, Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks, IEditorManager editorManager, 
                                  DocumentManager documentManager, IUIApplication environment)
        {            
            _contentPath = contentPath;            
            _codeNavigator = new SourceCodeNavigator(lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, 
                documentManager, environment);
            _tutorialWindow = new TutorialWindow();
            _steps = new Dictionary<int, TutorialStep>();            
            LoadTutorialContent(contentPath);

            // TODO: here must be some logic on checking current step (if a user've already proceeded to some step during previous sessions)     
            _currentStepId = TutorialXmlReader.ReadCurrentStep(contentPath);              

            // TODO: Implement subscribing through the TutorialWindow.Subscribe()
//            var btnNext =  (Button) _tutorialWindow.FindName("BtnNext");
//            if (btnNext != null) btnNext.Click += delegate { GoNext(); };
//
//            var btnSaveClose = (Button)_tutorialWindow.FindName("BtnSaveClose");
//            if (btnSaveClose != null) btnSaveClose.Click += delegate { SaveAndClose(); };

            _tutorialWindow.Subscribe(UiActionType.Next, GoNext);
            _tutorialWindow.Subscribe(UiActionType.SaveClose, SaveAndClose);

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
            
        private void SaveAndClose(object sender, RoutedEventArgs args)
        {            
            TutorialXmlReader.WriteCurrentStep(_contentPath, _currentStepId.ToString());
            _tutorialWindow.Unsubscribe();
            CloseWindow();
            
            VsCommunication.SaveVsSolution();
            VsCommunication.CloseVsSolution();
        }

        private void GoNext(object sender, RoutedEventArgs args)
        {
            if (_currentStepId >= _steps.Count) return;
            _currentStepId++;
            ProcessStep(CurrentStep);

            // TODO: raise event that tutorial is over, buttons in _tutorialWindow must be subscribed to this event.            
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

            // False Navigate
            // For some reason, after a user opens the tutorial solution for the first time (right after installation),
            // there are no opened files in it (just a blank screen). The second navigate opens the file automatically.
            // The first one for some reason does NOT. That's why we add a false navigate operation which navigates
            // to the file from the first step before the tutorial starts.
            _codeNavigator.Navigate(_steps[1]);

            ProcessStep(CurrentStep);
        }        


        private void ProcessStep(TutorialStep step)
        {
            // TODO: Preparation - Open required cs file, move cursor to required line, prepare to post-step activities (subscrive to particular event, etc.)
            ShowStepText(step);

            _codeNavigator.Navigate(step);
//            _codeNavigator.NavigateOld(step);

        }


        private void ShowStepText(TutorialStep step)
        {
            _tutorialWindow.TutorialText = step.Text;
        }
          
          
        public void ShowWindow()
        {
            _tutorialWindow.Show();
        }


        private void CloseWindow()
        {
            _tutorialWindow.Close();
        }       


        public void GoPrevious()
        {
            // TODO: Probably, remove this at all. This action makes the step inconsistent with the code state!
            _currentStepId--;            
        }
    }
}
