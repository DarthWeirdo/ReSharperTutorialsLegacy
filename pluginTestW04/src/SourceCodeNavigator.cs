using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.platforms;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.DocumentModel;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.Navigation.Goto.ProvidersAPI;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationProviders;
using JetBrains.ReSharper.Features.Internal.PsiBrowser;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Paths;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.TreeModels;
using JetBrains.UI.Application;
using JetBrains.UI.Avalon.TreeListView;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.Resources;
using JetBrains.Util;

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

        /// <summary>
        /// By default, navigates to type definition. if methodName is provided, navigates to a method declaration (first occurrence) 
        /// within the specified class. If textToFind is provided, navigates to the first text occurrence after class or method declaration.       
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="fileName"></param>
        /// <param name="typeName">Required</param>
        /// <param name="methodName"></param>
        /// <param name="textToFind"></param>
        public void Navigate([CanBeNull] string projectName, [NotNull] string fileName, [NotNull] string typeName, [CanBeNull] string methodName,
            [CanBeNull] string textToFind)
        {
            var project = PsiNavigationHelper.GetOpenedProject(Solution);

            if (projectName != null)
            {
                project = PsiNavigationHelper.GetProjectByName(Solution, projectName);
            }

            var file = PsiNavigationHelper.GetCSharpFile(project, fileName);

            if (methodName != null)            
                PsiNavigationHelper.NavigateToMethod(file, typeName, methodName);            
            else            
                PsiNavigationHelper.NavigateToType(file, typeName);

            if (textToFind != null)
            {
                VsCommunication.FindTextInCurrentDocument(textToFind);
            }            
        }

        public void Navigate(TutorialStep step)
        {
            var project = PsiNavigationHelper.GetOpenedProject(Solution);

            if (step.ProjectName != null)
            {
                project = PsiNavigationHelper.GetProjectByName(Solution, step.ProjectName);
            }

            var file = PsiNavigationHelper.GetCSharpFile(project, step.FileName);

            if (step.MethodName != null)
                PsiNavigationHelper.NavigateToMethod(file, step.TypeName, step.MethodName);
            else
                PsiNavigationHelper.NavigateToType(file, step.TypeName);

            if (step.TextToFind != null)
            {
                VsCommunication.FindTextInCurrentDocument(step.TextToFind);
            }
        }

        // TODO: delete this
        public void TestPsi()
        {            
            ICSharpFile csFile = null;
            var projects = Solution.GetTopLevelProjects();

            foreach (var project in projects)
            {
                if (project.Name == "Tutorial1_EssentialShortcuts")
                {
                    csFile = PsiNavigationHelper.GetCSharpFile(project, "1-AltEnter.cs");                    
                }                
            }

            //            var node = GetTypeNodeByFullClrName(csFile, "Tutorial1_EssentialShortcuts.ContextAction");         
            //            Navigate(node, true);

//            NavigateToMethod(csFile, "Tutorial1_EssentialShortcuts.ContextAction", "FormatString");
            PsiNavigationHelper.NavigateToType(csFile, "Tutorial1_EssentialShortcuts.ContextAction");
            VsCommunication.FindTextInCurrentDocument("Hello");
        }


        /// <summary>
        /// Navigate to any node. Not needed right now, though maybe needed in future.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="activate"></param>
        private void NavigateToNode(ITreeNode treeNode, bool activate)
        {
            //            if (!IsUpToDate()) return;
            _shellLocks.ExecuteOrQueueReadLock(_lifetime, "Navigate", () =>
            {
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
            });
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
