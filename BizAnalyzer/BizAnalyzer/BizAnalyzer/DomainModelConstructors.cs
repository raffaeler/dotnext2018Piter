using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace BizAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DomainModelConstructors : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DomainModelConstructors";
        internal static readonly LocalizableString Title = "DomainModelConstructors Title";
        internal static readonly LocalizableString MessageFormat = "DomainModelConstructors '{0}'";
        internal const string Category = "DomainModelConstructors Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);


        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(Resources.DomainModel_AvoidDefaultConstructor), Resources.ResourceManager, typeof(Resources));
        private static readonly string _interfaceIdentifier = "SampleLibrary.IDomainIdentifier";


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(c =>
            {
                c.RegisterOperationAction(AnalyzeOperation, OperationKind.ObjectCreation);
            });

            //context.RegisterSyntaxNodeAction(AnalyzeAccessMember, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (context.Operation.Kind == OperationKind.ObjectCreation)
            {
                var interfaceType = context.Compilation.GetTypeByMetadataName(_interfaceIdentifier);
                var operation = (IObjectCreationOperation)context.Operation;
                var type = operation.Type;

                if (type.AllInterfaces.Contains(interfaceType))
                {
                    // we found the construction of a type implementing our interface!

                    if (operation.Constructor.Parameters.Length == 0)
                    {
                        var location = operation.Syntax.GetLocation();
                        var diagnostic = Diagnostic.Create(Rule, location, Message);
                        context.ReportDiagnostic(diagnostic);
                    }
                }

            }
        }

        private void AnalyzeAccessMember(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.Kind() == SyntaxKind.ObjectCreationExpression)
            {
                var model = context.SemanticModel;
                var syntax = (ObjectCreationExpressionSyntax)context.Node;
                var objectCreationSymbol = model.GetSymbolInfo(syntax).Symbol;
                if (objectCreationSymbol != null)
                {
                    var interfaceType = context.Compilation.GetTypeByMetadataName(_interfaceIdentifier);

                    var type = objectCreationSymbol.ContainingType;
                    if (type.AllInterfaces.Contains(interfaceType))
                    {
                        if (syntax.ArgumentList.Arguments.Count == 0)
                        {
                            var location = syntax.GetLocation();
                            var diagnostic = Diagnostic.Create(Rule, location, Message);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
