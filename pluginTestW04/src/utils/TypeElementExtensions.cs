using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Platform.VisualStudio.SinceVs14.ProjectModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Psi;
using PlatformID = JetBrains.Application.platforms.PlatformID;

namespace pluginTestW04
{
    public static class TypeElementExtensions
    {
        [CanBeNull]
        public static PlatformID GetPlatformId([NotNull] this ITypeElement typeElement)
        {
            if (typeElement == null) throw new ArgumentNullException("typeElement");
            IModule containingProjectModule = typeElement.Module.ContainingProjectModule;
            return containingProjectModule == null ? null : containingProjectModule.PlatformID;
        }

        public static string GetFullClrName([NotNull] this ITypeElement typeElement)
        {
            if (typeElement == null) throw new ArgumentNullException("typeElement");
            return typeElement.GetClrName().FullName;
        }

        [NotNull]
        public static IEnumerable<IMethod> GetAllMethods([NotNull] this ITypeElement typeElement)
        {
            if (typeElement == null) throw new ArgumentNullException("typeElement");
            return typeElement.GetMembers().OfType<IMethod>();
        }
       

        [NotNull]
        public static void NavigateToFirstMember([NotNull] this ITypeElement typeElement)
        {
            if (typeElement == null) throw new ArgumentNullException("typeElement");
            var member = typeElement.GetMembers().FirstOrDefault();
            member.Navigate(true);
        }

    }
}