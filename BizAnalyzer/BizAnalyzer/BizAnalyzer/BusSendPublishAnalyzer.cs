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
    public class BusSendPublishAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BusSendPublishAnalyzer";
        internal static readonly LocalizableString Title = "BusSendPublishAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "BusSendPublishAnalyzer '{0}'";
        internal const string Category = "BusSendPublishAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        private static readonly string _busInterfaceIdentifier = "SampleLibrary.IBusManager";


        private static readonly LocalizableString InvalidSendArgument = new LocalizableResourceString(
            nameof(Resources.InvalidSendArgument), Resources.ResourceManager, typeof(Resources));
        public static readonly string SendMethod = "Send";
        public static int SendParameterIndex = 0;
        public static readonly string SendParameter1Identifier = "SampleLibrary.ICommand";

        private static readonly LocalizableString InvalidPublishArgument = new LocalizableResourceString(
            nameof(Resources.InvalidPublishArgument), Resources.ResourceManager, typeof(Resources));
        public static readonly string PublishMethod = "Publish";
        public static readonly string PublishParameter1Identifier = "SampleLibrary.IEvent";
        public static int PublishParameterIndex = 0;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(c =>
            {
                c.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            });
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            if (context.Operation.Kind == OperationKind.Invocation)
            {
                var operation = (IInvocationOperation)context.Operation;
                var method = operation.TargetMethod;
                var type = method.ContainingType;
                var busInterfaceType = context.Compilation.GetTypeByMetadataName(_busInterfaceIdentifier);

                if (type == busInterfaceType || type.AllInterfaces.Contains(busInterfaceType))
                {
                    if (method.Name == SendMethod && operation.Arguments.Length > SendParameterIndex)
                    {
                        var sendParameter1Type = context.Compilation.GetTypeByMetadataName(SendParameter1Identifier);
                        var argument = operation.Arguments[SendParameterIndex];
                        var semanticModel = context.Compilation.GetSemanticModel(argument.Syntax.SyntaxTree);
                        var argumentSymbolType = semanticModel.GetTypeInfo(((ArgumentSyntax)argument.Syntax).Expression).Type;
                        if (argumentSymbolType != null &&
                            argumentSymbolType != sendParameter1Type &&
                            !argumentSymbolType.AllInterfaces.Contains(sendParameter1Type))
                        {
                            var location = operation.Syntax.GetLocation();
                            var diagnostic = Diagnostic.Create(Rule, location, InvalidSendArgument);
                            context.ReportDiagnostic(diagnostic);
                        }

                        return;
                    }

                    if (method.Name == PublishMethod && operation.Arguments.Length > PublishParameterIndex)
                    {
                        var publishParameter1Type = context.Compilation.GetTypeByMetadataName(PublishParameter1Identifier);
                        var argument = operation.Arguments[PublishParameterIndex];
                        var semanticModel = context.Compilation.GetSemanticModel(argument.Syntax.SyntaxTree);
                        var argumentSymbolType = semanticModel.GetTypeInfo(((ArgumentSyntax)argument.Syntax).Expression).Type;
                        if (argumentSymbolType != null && 
                            argumentSymbolType != publishParameter1Type &&
                            !argumentSymbolType.AllInterfaces.Contains(publishParameter1Type))
                        {
                            var location = operation.Syntax.GetLocation();
                            var diagnostic = Diagnostic.Create(Rule, location, InvalidPublishArgument);
                            context.ReportDiagnostic(diagnostic);
                        }

                        return;
                    }

                }

            }
        }
    }
}
