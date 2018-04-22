using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace CodeAnalysisDemo.Visitors
{
    /// <summary>
    /// This class visits a member declaration, analyzes the graph of the calls
    /// and finds the top (known) declarations in the graph
    /// </summary>
    public class DeclarationUpVisitor
    {
        private InspectorContext _context;
        private IDictionary<string, MemberDeclarationSyntax> _topDeclarations;
        private HashSet<string> _alreadyVisited;

        private DeclarationUpVisitor(InspectorContext context)
        {
            _context = context;
            _topDeclarations = new Dictionary<string, MemberDeclarationSyntax>();
            _alreadyVisited = new HashSet<string>();
        }

        public static IList<MemberDeclarationSyntax> Start(InspectorContext context, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var operationUpVisitor = new DeclarationUpVisitor(context);
            operationUpVisitor.Visit(memberDeclarationSyntax);
            return operationUpVisitor._topDeclarations.Values.ToList();
        }

        private void Visit(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var semanticModel = _context.GetSemanticModelFor(memberDeclarationSyntax);
            var memberSymbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);
            var memberType = memberSymbol.ContainingType;
            var fullName = $"{memberSymbol.ContainingNamespace}.{memberType.Name}.{memberSymbol.Name}";

            // avoid circular references
            if (_alreadyVisited.Contains(fullName)) return;
            _alreadyVisited.Add(fullName);

            // find all callers
            var references = SymbolFinder.FindCallersAsync(memberSymbol, _context.Solution).Result;
            if (!references.Any())
            {
                // a top declaration was found
                // Warning: this demo does not take in account overloads
                _topDeclarations[fullName] = memberDeclarationSyntax;
                return;
            }

            // cycle the callers
            foreach (var referenced in references)
            {
                var caller = referenced.CallingSymbol;

                foreach (var definition in caller.DeclaringSyntaxReferences)
                {
                    var callerMemberDeclarationSyntax = (MemberDeclarationSyntax)definition.GetSyntax();

                    // recurse on the available declarations
                    Visit(callerMemberDeclarationSyntax);
                }
            }
        }
    }
}
