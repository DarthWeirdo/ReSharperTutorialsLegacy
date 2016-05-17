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

        // TODO: delete this
        public void TestPsi()
        {
            ICSharpFile csFile = null;
            var projects = Solution.GetTopLevelProjects();

            foreach (var project in projects)
            {
                if (project.Name == "Tutorial1_EssentialShortcuts")
                {
                    csFile = GetCSharpFile(project, "1-AltEnter.cs");                    
                }                
            }

            var node = GetNodeByFullClrName(csFile, "Tutorial1_EssentialShortcuts.ContextAction");
            Navigate(node, true);
        }

        [CanBeNull]
        private static IProject GetProjectByName(ISolution solution, string projectName)
        {
            var projects = solution.GetTopLevelProjects();
            return projects.FirstOrDefault(project => project.Name == projectName);
        }

        [CanBeNull]
        private static IProject GetOpenedProject(ISolution solution)
        {
            var projects = solution.GetTopLevelProjects();
            return projects.FirstOrDefault(project => project.IsOpened);            
        }

        [CanBeNull]
        private static ICSharpFile GetCSharpFile(IProject project, string filename)
        {
            IPsiSourceFile file = project.GetPsiSourceFileInProject(FileSystemPath.Parse(filename));
            return file.GetPsiFiles<CSharpLanguage>().SafeOfType<ICSharpFile>().SingleOrDefault();
        }

        [CanBeNull]
        public static IDeclaration GetDeclaration(ITreeNode node)
        {
            while (null != node)
            {
                var declaration = node as IDeclaration;
                if (null != declaration)
                    return declaration;
                node = node.Parent;
            }
            return null;
        }

        [CanBeNull]
        public static IDeclaredElement GetDeclaredElement(ITreeNode node)
        {
            var declaration = GetDeclaration(node);            
            return declaration?.DeclaredElement;
        }

        [CanBeNull]
        public ITreeNode GetNodeByFullClrName(ICSharpFile file, string name)
        {
            var treeNodeList = file.EnumerateTo(file.LastChild);

            return (from treeNode in treeNodeList
                    let element = GetDeclaredElement(treeNode)
                    let typeElement = element as ITypeElement
                    where typeElement != null
                    where typeElement.GetFullClrName() == name
                    select treeNode).FirstOrDefault();
        }                  


        public void Navigate(ITreeNode treeNode, bool activate)
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

        // This was the double-click handler in PsiBrowser
        public void OnPreviewNavigate(TreeModelNode modelNode)
        {
            if (!IsUpToDate()) return;

            var dataValue = modelNode.DataValue;
            _shellLocks.ExecuteOrQueueReadLock(_lifetime, "Navigate", () =>
            {
                var range = DocumentRange.InvalidRange;
                var reference = dataValue as IReference;
                if (reference != null)
                {
                    range = reference.GetDocumentRange();
                }
                else
                {
                    var node = dataValue as ITreeNode;
                    if (node != null) range = node.GetDocumentRange();
                }

                if (range.IsValid())
                {
                    var projectFile = _documentManager.TryGetProjectFile(range.Document);
                    if (projectFile != null)
                    {
                        var textControl = _editorManager.OpenProjectFile(projectFile, true);
                        if (textControl != null)
                            textControl.Selection.SetRange(range.TextRange);
                    }
                }

                var windowContext = Shell.Instance.GetComponent<MainWindowPopupWindowContext>();
                var options = NavigationOptions.FromWindowContext(windowContext.Source, "Navigate", true);
                var manager = NavigationManager.GetInstance(Solution);

                manager.Navigate<INavigationProvider<object>, object>(dataValue, options);
                
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
