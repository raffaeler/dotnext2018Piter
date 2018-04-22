using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAnalysisDemo.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisDemo.Visitors
{
    public class CallDescriptor
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

        public int Depth { get; private set; }
        public string InvocationString { get; private set; }
        public string OwnerStatementString { get; private set; }
        public bool IsMethod { get; private set; }
        public bool IsProperty { get; private set; }
        public bool IsPropertySetter { get; private set; }

        public string MemberName { get; private set; }
        public TypeDescriptor ReturnType { get; private set; }
        public TypeDescriptor ContainingType { get; private set; }
        public IList<TypeDescriptor> ParameterTypes { get; private set; }
        public IList<TypeDescriptor> GenericTypesForMethod { get; private set; }
        public IList<TypeDescriptor> GenericTypesForContainedType { get; private set; }
        public MethodDeclarationSyntax MethodDeclarationSyntax { get; private set; }
        public PropertyDeclarationSyntax PropertyDeclarationSyntax { get; private set; }

        public TypeDescriptor MemberOwnerType { get; private set; }


        private CallDescriptor()
        {
        }

        public static CallDescriptor Create(int depth, InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                // methodSymbol is null when the invocationExpressionSyntax is "nameof(...)"
                return null;
            }

            var callDescriptor = new CallDescriptor();
            callDescriptor.Initialize(depth, invocationExpressionSyntax, methodSymbol, semanticModel);
            return callDescriptor;
        }

        public static CallDescriptor Create(int depth, MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
        {
            var isMethod = false;
            var isProperty = false;
            switch (memberDeclarationSyntax.Kind())
            {
                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration:
                    isMethod = true;
                    break;
                case Microsoft.CodeAnalysis.CSharp.SyntaxKind.PropertyDeclaration:
                    isProperty = true;
                    break;
                default:
                    return null;
            }

            var callDescriptor = new CallDescriptor();
            callDescriptor.Initialize(depth, memberDeclarationSyntax, semanticModel, isMethod, isProperty);
            return callDescriptor;
        }

        private void Initialize(int depth, MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel, bool isMethod, bool isProperty)
        {
            Depth = depth;
            var memberSymbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

            IsMethod = isMethod;
            IsProperty = isProperty;
            InvocationString = string.Empty;
            OwnerStatementString = string.Empty;
            MemberOwnerType = new TypeDescriptor(memberSymbol.ContainingType);
            MemberName = memberSymbol.Name;
            if (isMethod)
            {
                ReturnType = new TypeDescriptor((memberSymbol as IMethodSymbol).ReturnType);
                MethodDeclarationSyntax = memberDeclarationSyntax as MethodDeclarationSyntax;
            }
            else
            {
                ReturnType = new TypeDescriptor((memberSymbol as IPropertySymbol).Type);
                PropertyDeclarationSyntax = memberDeclarationSyntax as PropertyDeclarationSyntax;
            }

            ContainingType = new TypeDescriptor(memberSymbol.ContainingType);
        }

        public static CallDescriptor Create(int depth, MemberAccessExpressionSyntax memberAccessExpressionSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax);
            if (symbolInfo.Symbol == null)
            {
                return null;
            }

            var symbolKind = symbolInfo.Symbol.Kind;
            if (symbolKind != SymbolKind.Property)
            {
                return null;
            }

            var propertySymbol = symbolInfo.Symbol as IPropertySymbol;

            var callDescriptor = new CallDescriptor();
            callDescriptor.Initialize(depth, memberAccessExpressionSyntax, propertySymbol, semanticModel);
            return callDescriptor;
        }

        private void Initialize(int depth, MemberAccessExpressionSyntax memberAccessExpressionSyntax,
            IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            Depth = depth;
            IsProperty = true;
            InvocationString = memberAccessExpressionSyntax.ToString();
            var containingNode = memberAccessExpressionSyntax.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
            OwnerStatementString = containingNode?.ToString();

            var objectAccessedSyntax = memberAccessExpressionSyntax.Expression; // .ToString() => var name
            var objectAccessedSymbol = semanticModel.GetSymbolInfo(objectAccessedSyntax).Symbol;
            var objectAccessedTypeSymbol = semanticModel.GetTypeInfo(objectAccessedSyntax).Type;
            MemberOwnerType = new TypeDescriptor(objectAccessedTypeSymbol);

            MemberName = propertySymbol.Name;
            ReturnType = new TypeDescriptor(propertySymbol.Type);
            var containingTypeSymbol = propertySymbol.ContainingType;
            ContainingType = new TypeDescriptor(containingTypeSymbol);

            PropertyDeclarationSyntax = SymbolHelper.GetSyntaxReferences(memberAccessExpressionSyntax, semanticModel)
                .FirstOrDefault()
                ?.GetSyntax() as PropertyDeclarationSyntax;
            var assignment = memberAccessExpressionSyntax.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null && assignment.Left.DescendantNodesAndSelf().Contains(memberAccessExpressionSyntax))
            {
                // is a set property
                IsPropertySetter = true;
            }
            else
            {
                // is a get property
            }
        }

        private void Initialize(int depth, InvocationExpressionSyntax invocationExpressionSyntax,
            IMethodSymbol methodSymbol, SemanticModel semanticModel)
        {
            Depth = depth;
            IsMethod = true;
            InvocationString = invocationExpressionSyntax.ToString();
            var containingNode = invocationExpressionSyntax.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
            OwnerStatementString = containingNode?.ToString();


            MemberName = methodSymbol.Name;
            ReturnType = new TypeDescriptor(methodSymbol.ReturnType);
            var containingTypeSymbol = methodSymbol.ContainingType;
            ContainingType = new TypeDescriptor(containingTypeSymbol);

            ParameterTypes = invocationExpressionSyntax.ArgumentList.Arguments
                .Select(a => new TypeDescriptor(semanticModel.GetTypeInfo(a.Expression).Type))
                .ToList();

            GenericTypesForMethod = methodSymbol.IsGenericMethod ? methodSymbol.TypeArguments
                .Select(t => new TypeDescriptor(t))
                .ToList() : null;

            GenericTypesForContainedType = containingTypeSymbol.IsGenericType ? containingTypeSymbol.TypeArguments
                .Select(t => new TypeDescriptor(t))
                .ToList() : null;

            MethodDeclarationSyntax = SymbolHelper.GetSyntaxReferences(invocationExpressionSyntax, semanticModel)
                .FirstOrDefault()
                ?.GetSyntax() as MethodDeclarationSyntax;

            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                var memberAccessSymbolInfo = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax);

                // info on the variable being accessed
                var objectAccessedSyntax = memberAccessExpressionSyntax.Expression;
                var objectAccessedSymbol = semanticModel.GetSymbolInfo(objectAccessedSyntax).Symbol;
                var objectAccessedTypeSymbol = semanticModel.GetTypeInfo(objectAccessedSyntax).Type;
                MemberOwnerType = new TypeDescriptor(objectAccessedTypeSymbol);
            }
            else
            {
                // a method that is part of the current class (containingType)
                MemberOwnerType = ContainingType;
            }
        }

        public override string ToString()
        {
            return $"[{Depth}] {MemberName};{InvocationString}";
        }
    }
}
