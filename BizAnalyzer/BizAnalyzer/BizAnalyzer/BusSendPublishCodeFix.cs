using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Editing;

// Note 1:
// If something screw up without any apparent reason, just delete the
// data for the Roslyn instance (which is not the one the dev is working on)
// C:\Users\(user)\AppData\Local\Microsoft\VisualStudio\15.0_4cb75e46Roslyn
// where "15.0_4cb75e46" is:
// - 15.0 ==> the vs version
// - the instance being used in development retrieved with "vswhere" utility on MS Roslyn github repo
// - the "Roslyn" suffix (which is the one identifying the 'experimental' instance)
// This 'Roslyn' instance is a debugging only instance. The first time is created it takes a long time
// and debugging can fail. From the second time on, it works pretty fast.
//
// Note 2:
// when an analyzer is added from the references/analyzer node in Visual Studio,
// VS caches the analyzers in this temporary folder:
// C:\Users\(user)\AppData\Local\Temp\VS\AnalyzerAssemblyLoader\96bf94369c1746edacfb5afa3cbcf4a0\0\BizAnalyzer.dll
// To clear the cache just close and reopen VS

namespace BizAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BusSendPublishCodeFix)), Shared]
    public class BusSendPublishCodeFix : CodeFixProvider
    {
        public const string DiagnosticId = BusSendPublishAnalyzer.DiagnosticId;
        private const string title = "Fix correct bus method";
    
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;
            context.RegisterCodeFix(
                CodeAction.Create(title, c => ChangeBusMethod(context.Document, span, c)),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> ChangeBusMethod(Document document, TextSpan span,
            CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync(c);
            var invocationNode = root.FindNode(span);
            var semanticModel = await document.GetSemanticModelAsync(c);

            // current pieces of the syntax
            var invocationOperation = (IInvocationOperation)semanticModel.GetOperation(invocationNode, c);
            var invocationSyntax = (InvocationExpressionSyntax)invocationOperation.Syntax;
            var memberAccess = (MemberAccessExpressionSyntax)invocationSyntax.Expression;
            var leftSide = memberAccess.Expression;
            var memberName = memberAccess.Name.ToString();
            var arguments = invocationSyntax.ArgumentList.Arguments;

            var generator = SyntaxGenerator.GetGenerator(document);

            // building the new syntax
            var newMemberName = GetNewBusMethod(semanticModel, memberName, arguments.ToList());
            if (newMemberName == null) return document;

            var newMember = generator.IdentifierName(newMemberName);
            var newMemberAccess = generator.MemberAccessExpression(leftSide, newMember);
            var newInvocation = generator.InvocationExpression(newMemberAccess, arguments);
            var newRoot = root.ReplaceNode(invocationNode, newInvocation);

            // replace the old node with the new one
            return document.WithSyntaxRoot(newRoot);
        }

        private string GetNewBusMethod(SemanticModel semanticModel,
            string currentMethodName, IList<ArgumentSyntax> arguments)
        {
            var sendParameter1Type = semanticModel.Compilation.GetTypeByMetadataName(
                BusSendPublishAnalyzer.SendParameter1Identifier);

            var publishParameter1Type = semanticModel.Compilation.GetTypeByMetadataName(
                BusSendPublishAnalyzer.PublishParameter1Identifier);

            // current method is Send
            if (currentMethodName == BusSendPublishAnalyzer.SendMethod &&
                arguments.Count > BusSendPublishAnalyzer.SendParameterIndex)
            {
                // we now need the send argument
                var argument = arguments[BusSendPublishAnalyzer.SendParameterIndex];
                var argumentSymbolType = semanticModel.GetTypeInfo(argument.Expression).Type;

                // if the argument needs publish, return publish
                if (argumentSymbolType != null && (
                    argumentSymbolType == publishParameter1Type ||
                    argumentSymbolType.AllInterfaces.Contains(publishParameter1Type)))
                {
                    return BusSendPublishAnalyzer.PublishMethod;
                }
            }

            // current method is Publish
            if (currentMethodName == BusSendPublishAnalyzer.PublishMethod &&
                arguments.Count > BusSendPublishAnalyzer.PublishParameterIndex)
            {
                // we now need the publish argument
                var argument = arguments[BusSendPublishAnalyzer.PublishParameterIndex];
                var argumentSymbolType = semanticModel.GetTypeInfo(argument.Expression).Type;

                // if the argument needs send, return send
                if (argumentSymbolType != null && (
                    argumentSymbolType == sendParameter1Type ||
                    argumentSymbolType.AllInterfaces.Contains(sendParameter1Type)))
                {
                    return BusSendPublishAnalyzer.SendMethod;
                }
            }

            // found no replacement
            return null;
        }
    }
}
