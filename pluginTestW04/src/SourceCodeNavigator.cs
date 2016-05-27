using System;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.Application;

namespace pluginTestW04
{
    public class SourceCodeNavigator
    {
        private readonly Lifetime _lifetime;
        private readonly ISolution _solution;
        private readonly IPsiFiles _psiFiles;
        private readonly TextControlManager _textControlManager;
        private readonly IShellLocks _shellLocks;
        private readonly IEditorManager _editorManager;
        private readonly DocumentManager _documentManager;
        private readonly IUIApplication _environment;
        private int _psiTimestamp;

        public SourceCodeNavigator(Lifetime lifetime, ISolution solution, IPsiFiles psiFiles,
                                  TextControlManager textControlManager, IShellLocks shellLocks,
                                  IEditorManager editorManager, DocumentManager documentManager, IUIApplication environment)
        {            
            _lifetime = lifetime;
            _solution = solution;
            _psiFiles = psiFiles;
            _textControlManager = textControlManager;
            _shellLocks = shellLocks;
            _documentManager = documentManager;
            _environment = environment;
            _editorManager = editorManager;            

            Action<ITreeNode, PsiChangedElementType> psiChanged =
                (_, __) => OnPsiChanged();

            _lifetime.AddBracket(
              () => psiFiles.AfterPsiChanged += psiChanged,
              () => psiFiles.AfterPsiChanged -= psiChanged);                                                                
        }


//        public void NavigateOld(TutorialStep step)
//        {
//            var project = PsiNavigationHelper.GetProjectByName(Solution, step.ProjectName);
//
//            var file = PsiNavigationHelper.GetCSharpFile(project, step.FileName);
//
//            if (step.MethodName != null)
//                PsiNavigationHelper.NavigateToMethod(file, step.TypeName, step.MethodName, _shellLocks, _lifetime);
//            else
//                PsiNavigationHelper.NavigateToType(file, step.TypeName, _shellLocks, _lifetime);
//            
//            if (step.TextToFind != null)
//                VsCommunication.FindTextInCurrentDocument(step.TextToFind, step.TextToFindOccurrence);            
//        }

        public void Navigate(TutorialStep step)
        {
            _shellLocks.ExecuteOrQueueReadLock(_lifetime, "Navigate", () =>
            {
                var project = PsiNavigationHelper.GetProjectByName(Solution, step.ProjectName);

                var file = PsiNavigationHelper.GetCSharpFile(project, step.FileName);

                var node = PsiNavigationHelper.GetTreeNodeForStep(file, step.TypeName, step.MethodName, step.TextToFind,
                    step.TextToFindOccurrence);

                NavigateToNode(node, true);
            });
        }
    
        
        private void NavigateToNode(ITreeNode treeNode, bool activate)
        {
            //            if (!IsUpToDate()) return;
//            _shellLocks.ExecuteOrQueueReadLock(_lifetime, "Navigate", () =>
//            {
                var range = treeNode.GetDocumentRange();
                if (!range.IsValid()) return;                

                var projectFile = _documentManager.TryGetProjectFile(range.Document);
                if (projectFile == null) return;

                var textControl = _editorManager.OpenProjectFile(projectFile, activate);
                if (textControl == null) return;

                textControl.Caret.MoveTo(
                    range.TextRange.StartOffset, CaretVisualPlacement.DontScrollIfVisible);

//                if (range.TextRange.Length < 30) // select if small enough
                    textControl.Selection.SetRange(range.TextRange);
//            });
        }



        public bool IsUpToDate()
        {
            // PsiTimestamp is a time stamp for PsiFiles - use it to check the current PSI is up to date
            return _psiTimestamp == Solution.GetPsiServices().Files.PsiTimestamp;
        }

        public ISolution Solution
        {
            get { return _solution; }
        }

        private void OnPsiChanged()
        {            
            _shellLocks.QueueReadLock("SourceCodeNavigator.CheckOnPsiChanged",
                  () => _psiFiles.CommitAllDocumentsAsync(() => CheckCode()));
            
        }

        private void CheckCode()
        {
            // TODO: check whether the user updated the code in the way we asked him
        }
    }
}
