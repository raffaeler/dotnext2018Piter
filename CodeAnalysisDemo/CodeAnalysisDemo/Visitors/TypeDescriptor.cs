using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace CodeAnalysisDemo.Visitors
{
    public class TypeDescriptor
    {
        private static readonly SymbolDisplayFormat NamespaceFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        private static readonly SymbolDisplayFormat NameFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.None,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.Default,
            SymbolDisplayParameterOptions.None,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.None,
            SymbolDisplayKindOptions.None,
            SymbolDisplayMiscellaneousOptions.ExpandNullable);


        public TypeDescriptor(ITypeSymbol typeSymbol)
        {
            this.AssemblyName = typeSymbol.ContainingAssembly.Name;
            this.Namespace = typeSymbol.ContainingNamespace.ToDisplayString(NamespaceFormat);
            this.Name = typeSymbol.ToDisplayString(NameFormat);
        }

        public string AssemblyName { get; internal set; }
        public string Namespace { get; internal set; }
        public string Name { get; internal set; }

        public string ToFullQualifiedAssemblyName()
        {
            return $"{Namespace}.{Name},{AssemblyName}";
        }

        public string ToFullName()
        {
            return $"{Namespace}.{Name}";
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return ToFullName().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is TypeDescriptor other)
            {
                if (other.ToFullName() == this.ToFullName())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
