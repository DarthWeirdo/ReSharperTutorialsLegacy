using System;
using System.Reflection;
using System.Windows;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.Actions.ActionManager;
using JetBrains.UI.Application;

namespace pluginTestW04
{
    class Checker
    {
        private readonly Lifetime _lifetime;
        private readonly ISolution _solution;
        private readonly IPsiFiles _psiFiles;
        private readonly TextControlManager _textControlManager;
        private readonly IShellLocks _shellLocks;
        private readonly IEditorManager _editorManager;
        private readonly DocumentManager _documentManager;
        private readonly IUIApplication _environment;
        private readonly IActionManager _actionManager;
        private int _psiTimestamp;
        
        /// <summary>
        /// Used to identify whether a user applied an action specified in the step
        /// </summary>
        private readonly StepActionChecker _stepActionChecker;

        /// <summary>
        /// Use it to perform checks that use PSI tree
        /// </summary>
        private readonly StepPsiChecker _stepPsiChecker;

        private TutorialStep _currentStep = null;
                

        public Checker(Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IActionManager actionManager,  
                                  IUIApplication environment)
        {
            _lifetime = lifetime;
            _solution = solution;
            _psiFiles = psiFiles;
            _textControlManager = textControlManager;
            _shellLocks = shellLocks;
            _documentManager = documentManager;
            _environment = environment;
            _editorManager = editorManager;
            _actionManager = actionManager;

            _stepActionChecker = new StepActionChecker(lifetime, actionManager);
            _stepPsiChecker = new StepPsiChecker(lifetime, solution, psiFiles, textControlManager, shellLocks, editorManager, 
                documentManager, environment);
        }


        public void PerformStepChecks(TutorialStep step)
        {
            _currentStep = step;
                        
            if (_currentStep.Action != null)
            {
                _stepActionChecker.StepActionName = _currentStep.Action;

                _stepActionChecker.AfterActionApplied.Advise(_lifetime, () =>
                {
                    _currentStep.IsActionDone = true;                    
                });
            }

            
            if (_currentStep.Check != null)
            {                
                MethodInfo mInfo = typeof (Checker).GetMethod(_currentStep.Check);
                if (mInfo != null)
                {                    
                    var checkMethod = (Func<bool>) Delegate.CreateDelegate(typeof (Func<bool>), this, mInfo);
                    _stepPsiChecker.Check = checkMethod;

                    _stepPsiChecker.AfterPsiChangesDone.Advise(_lifetime, () =>
                    {
                        _currentStep.IsCheckDone = true;
                    });
                }
                else                
                    throw new Exception($"Unable to find the checker {_currentStep.Check}. Please reinstall the plugin.");                
            }            
        }

        #region Typical Checks
        /// <summary>
        /// Finds type declaration in scope specified in the current step
        /// </summary>
        /// <returns>Returns true if $typeName$ is found</returns>
        private bool FindTypeDeclaration(string typeName)
        {
            ITreeNode node = null;
            _shellLocks.TryExecuteWithReadLock(() =>
            {
                var project = PsiNavigationHelper.GetProjectByName(_solution, _currentStep.ProjectName);
                var file = PsiNavigationHelper.GetCSharpFile(project, _currentStep.FileName);
                node = PsiNavigationHelper.GetTypeNodeByFullClrName(file, typeName);
            });

            return node != null;
        }


        /// <summary>
        /// Finds type declaration in scope specified in the current step
        /// </summary>
        /// <returns>Returns true if $typeName$ is found</returns>
        private bool FindMethodDeclaration(string typeName, string methodName)
        {
            ITreeNode node = null;
            _shellLocks.TryExecuteWithReadLock(() =>
            {
                var project = PsiNavigationHelper.GetProjectByName(_solution, _currentStep.ProjectName);
                var file = PsiNavigationHelper.GetCSharpFile(project, _currentStep.FileName);
                node = PsiNavigationHelper.GetMethodNodeByFullClrName(file, typeName, methodName);
            });

            return node != null;
        }


        /// <summary>
        /// Finds text of a tree node in the scope specified in the current step
        /// </summary>        
        /// <returns>Returns true if specific $occurrence$ of $text$ is found</returns>        
        private bool FindText(string text, int occurrence)
        {
            ITreeNode node = null;
            _shellLocks.TryExecuteWithReadLock(() =>
            {
                var project = PsiNavigationHelper.GetProjectByName(_solution, _currentStep.ProjectName);
                var file = PsiNavigationHelper.GetCSharpFile(project, _currentStep.FileName);
                node = PsiNavigationHelper.GetTreeNodeForStep(file, _currentStep.TypeName, _currentStep.MethodName,
                    text, occurrence);
            });

            return node != null;
        }


        #endregion

        #region Custom step checks (This must be "public bool" method that returns true if the check passes)

        /// <summary>
        /// Example of a PSI check
        /// </summary>
        /// <returns>Returns true if BadlyNamedClass is found</returns>
        public bool CheckTutorial1Step2()
        {            
            return FindTypeDeclaration("Tutorial1_EssentialShortcuts.BadlyNamedClass");
        }


        public bool CheckTutorial1Step3()
        {
            return FindText("Format", 1);
        }


        public bool CheckTutorial1Step4()
        {
            return FindTypeDeclaration("Tutorial1_EssentialShortcuts.Renamed");
        }

        #endregion
    }
}
