﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.platforms;
using JetBrains.DocumentManagers;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Paths;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace pluginTestW04
{
    public static class PsiNavigationHelper
    {        
        public static IEnumerable<ProjectFileTextRange> GetProjectFileTextRangesByDeclaredElement(ISolution solution,
          IDeclaredElement declaredElement)
        {
            IEnumerable<IDeclaration> declarations = declaredElement.GetDeclarations();
            var navigationRanges =
              declarations.Select(declaration => declaration.GetNavigationRange()).Where(range => range.IsValid());
            var psiModule = declarations.Select(decl => decl.GetPsiModule()).FirstOrDefault();
            var documentManager = solution.GetComponent<DocumentManager>();

            return navigationRanges.Select(range =>
            {
                var projectFile = documentManager.TryGetProjectFile(range.Document);

                return projectFile == null
                  ? ProjectFileTextRange.Invalid
                  : new ProjectFileTextRange(projectFile, range.TextRange,
                    psiModule != null ? psiModule.TargetFrameworkId : null);
            }).Where(range => range.IsValid);
        }

        public static IEnumerable<ITypeElement> GetTypeElementsByClrName(ISolution solution, string clrName)
        {            
            IPsiServices psiServices = solution.GetComponent<IPsiServices>();
            psiServices.Files.CommitAllDocuments();

            ISymbolCache symbolCache = psiServices.Symbols;
            ISymbolScope symbolScope = symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);

            IEnumerable<ITypeElement> validTypeElements = symbolScope.GetTypeElementsByCLRName(clrName)
              .Where(element => element.IsValid());

            return SkipDefaultProfileIfRuntimeExist(validTypeElements);
        }

        private static IEnumerable<ITypeElement> SkipDefaultProfileIfRuntimeExist(IEnumerable<ITypeElement> validTypeElements)
        {
            return validTypeElements
              // Merge default and runtime profiles into one group to select runtime from there
              .GroupBy(typeElement => typeElement.GetPlatformId(), DefaultPlatformUtil.IgnoreRuntimeAndDefaultProfilesComparer)
              .Select(TypeFromRuntimeProfilePlatformIfExist);
        }

        private static ITypeElement TypeFromRuntimeProfilePlatformIfExist(IGrouping<PlatformID, ITypeElement> @group)
        {            
            return @group.OrderByDescending(typeElement => typeElement.GetPlatformId(), DefaultPlatformUtil.DefaultPlatformIDComparer).First();
        }

        [CanBeNull]
        public static IProject GetProjectByName(ISolution solution, string projectName)
        {
            var projects = solution.GetTopLevelProjects();
            return projects.FirstOrDefault(project => project.Name == projectName);
        }

        [CanBeNull]
        public static IProject GetOpenedProject(ISolution solution)
        {
            var projects = solution.GetTopLevelProjects();            
            return projects.FirstOrDefault(project => project.IsOpened);            
        }

        [CanBeNull]
        public static ICSharpFile GetCSharpFile(IProject project, string filename)
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

        /// <summary>
        /// Navigate to a method
        /// </summary>
        /// <param name="file"></param>
        /// <param name="typeName">Type FQN</param>
        /// <param name="methodName">Method short name</param>
        public static void NavigateToMethod(ICSharpFile file, string typeName, string methodName)
        {
            var treeNodeList = file.EnumerateTo(file.LastChild);            

            var methods = (from treeNode in treeNodeList
                let element = GetDeclaredElement(treeNode)
                let typeElement = element as ITypeElement
                where typeElement != null
                where typeElement.GetFullClrName() == typeName
                select typeElement.GetAllMethods()).FirstOrDefault();

            if (methods == null) return;

            var targetMethod = methods.FirstOrDefault(method => method.ShortName == methodName);            
            targetMethod.Navigate(true);
        }

        public static void NavigateToType(ICSharpFile file, string typeName)
        {
            var treeNodeList = file.EnumerateTo(file.LastChild);

            var targetType = (from treeNode in treeNodeList
                let element = GetDeclaredElement(treeNode)
                let typeElement = element as ITypeElement
                where typeElement != null                           
                where typeElement.GetFullClrName() == typeName
                select typeElement).FirstOrDefault();
            
            targetType.Navigate(true);
        }


        [CanBeNull]
        public static ITreeNode GetTypeNodeByFullClrName(ICSharpFile file, string name)
        {
            var treeNodeList = file.EnumerateTo(file.LastChild);

            return (from treeNode in treeNodeList
                let element = GetDeclaredElement(treeNode)
                let typeElement = element as ITypeElement
                where typeElement != null
                where typeElement.GetFullClrName() == name
                select treeNode).FirstOrDefault();
        }

    }
}
