using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.platforms;
using JetBrains.DocumentManagers;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;

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
    }
}
