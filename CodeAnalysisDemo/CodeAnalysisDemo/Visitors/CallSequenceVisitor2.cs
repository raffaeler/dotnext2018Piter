using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace CodeAnalysisDemo.Visitors
{
    public class CallSequenceVisitor2 : CSharpSyntaxWalker
    {
        private InspectorContext _context;
        private IList<MethodOnVariable> _methodSequence;
        private INamedTypeSymbol[] _namedTypes;
        private Stack<Dictionary<string, VariableIdentity>> _variables;

        private CallSequenceVisitor2(InspectorContext context, INamedTypeSymbol[] namedTypes)
        {
            _context = context;
            _namedTypes = namedTypes;
            _methodSequence = new List<MethodOnVariable>();

            _variables = new Stack<Dictionary<string, VariableIdentity>>();
        }

        public static IList<IList<string>> Start(InspectorContext context, MemberDeclarationSyntax memberDeclarationSyntax,
            params string[] typeIdentifiers)
        {
            var semanticModel = context.GetSemanticModelFor(memberDeclarationSyntax);
            var namedTypes = typeIdentifiers.Select(t => GetTypeByMetadataName(semanticModel, t)).ToArray();

            var visitor = new CallSequenceVisitor2(context, namedTypes);
            visitor.StartInternal(memberDeclarationSyntax, new List<ISymbol>());
            return visitor.GetResults();
        }

        private IList<IList<string>> GetResults()
        {
            var res = new List<IList<string>>();

            var previous = -1;
            IList<string> currentList = null;
            foreach (var element in _methodSequence)
            {
                if (element.VariableId != previous)
                {
                    currentList = new List<string>();
                    currentList.Add(element.MethodName);
                    res.Add(currentList);
                    previous = element.VariableId;
                }
                else
                {
                    currentList.Add(element.MethodName);
                }
            }

            return res;
        }

        private static INamedTypeSymbol GetTypeByMetadataName(SemanticModel semanticModel, string name)
        {
            return semanticModel.Compilation.GetTypeByMetadataName(name);
        }

        private void StartInternal(MemberDeclarationSyntax memberDeclarationSyntax,
            IList<ISymbol> arguments)
        {
            var semanticModel = _context.GetSemanticModelFor(memberDeclarationSyntax);

            var variableFrame = new Dictionary<string, VariableIdentity>();
            if (arguments.Any() &&
                memberDeclarationSyntax is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                var vars = _variables.Peek();

                var parameters = methodDeclarationSyntax.ParameterList.Parameters;

                for (int i = 0; i < Math.Min(arguments.Count, parameters.Count); i++)
                {
                    var argument = arguments[i];
                    var parameter = parameters[i];

                    if (vars.TryGetValue(argument.Name, out VariableIdentity variableIdentity))
                    {
                        variableFrame[parameter.Identifier.ToString()] = variableIdentity;
                    }
                }
            }

            _variables.Push(variableFrame);
            Visit(memberDeclarationSyntax);
            _variables.Pop();
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            base.VisitInvocationExpression(invocationExpressionSyntax);
            var semanticModel = _context.GetSemanticModelFor(invocationExpressionSyntax);
            var method = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

            var containingType = method.ContainingType;

            if (IsTaggedType(containingType))
            {
                var fullName = $"{method.ContainingNamespace}.{containingType.Name}.{method.Name}";
                VariableIdentity varid = null;

                var argument = invocationExpressionSyntax.ArgumentList.Arguments.FirstOrDefault();
                if (argument != null)
                {
                    var argumentSymbol = semanticModel.GetSymbolInfo(argument.Expression).Symbol;
                    var variableName = argumentSymbol.Name;
                    var vars = _variables.Peek();
                    vars.TryGetValue(variableName, out varid);
                }

                _methodSequence.Add(new MethodOnVariable(fullName, varid?.Id ?? 0));
            }

            foreach (var declaringSyntax in method.DeclaringSyntaxReferences)
            {
                var declarationSyntax = (MethodDeclarationSyntax)declaringSyntax.GetSyntax();

                var argumentSymbols = new List<ISymbol>();
                foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
                {
                    var argumentSymbol = semanticModel.GetSymbolInfo(argument.Expression).Symbol;
                    if (argumentSymbol != null)
                    {
                        argumentSymbols.Add(argumentSymbol);
                    }
                }

                StartInternal(declarationSyntax, argumentSymbols.ToList());
            }
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentExpressionSyntax)
        {
            base.VisitAssignmentExpression(assignmentExpressionSyntax);

            var semanticModel = _context.GetSemanticModelFor(assignmentExpressionSyntax);

            var vars = _variables.Peek();

            var symbol = semanticModel.GetSymbolInfo(assignmentExpressionSyntax.Left).Symbol;
            VariableIdentity variableIdentity = null;

            if (assignmentExpressionSyntax.Right != null && assignmentExpressionSyntax.Right is IdentifierNameSyntax identifierNameSyntax)
            {
                var rightVariable = semanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;

                if (rightVariable != null && vars.TryGetValue(rightVariable.Name, out VariableIdentity value))
                {
                    variableIdentity = VariableIdentity.FromAlias(value, symbol);
                }
            }

            if (variableIdentity == null)
            {
                variableIdentity = new VariableIdentity(symbol);
            }

            vars[symbol.Name] = variableIdentity;

        }

        // just the declaration with the initialization
        public override void VisitVariableDeclarator(VariableDeclaratorSyntax variableDeclaratorSyntax)
        {
            base.VisitVariableDeclarator(variableDeclaratorSyntax);
            var identifier = variableDeclaratorSyntax.Identifier;
            var semanticModel = _context.GetSemanticModelFor(variableDeclaratorSyntax);
            var symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax);

            var vars = _variables.Peek();
            VariableIdentity variableIdentity = null;

            if (variableDeclaratorSyntax.Initializer != null &&
                variableDeclaratorSyntax.Initializer is EqualsValueClauseSyntax equalsValueClauseSyntax)
            {
                var right = equalsValueClauseSyntax.Value;
                var rightVariable = semanticModel.GetSymbolInfo(right).Symbol;

                if (rightVariable != null &&
                    vars.TryGetValue(rightVariable.Name, out VariableIdentity value))
                {
                    variableIdentity = VariableIdentity.FromAlias(value, symbol);
                }
            }

            if (variableIdentity == null)
            {
                variableIdentity = new VariableIdentity(symbol);
            }

            vars[symbol.Name] = variableIdentity;
        }

        private bool IsTaggedType(INamedTypeSymbol type)
        {
            if (_namedTypes.Length == 0)
            {
                // when no types has been passed to the visitor
                // all the calls will be captured
                return true;
            }

            foreach (var tagType in _namedTypes)
            {
                if (type == tagType || type.AllInterfaces.Contains(tagType) || type.BaseType == tagType)
                {
                    return true;
                }
            }

            return false;
        }

        private class VariableIdentity
        {
            private static int _id = 0;

            private VariableIdentity()
            {
            }

            public VariableIdentity(ISymbol symbol)
            {
                this.Symbol = symbol;
                this.Id = ++_id;
            }

            public static VariableIdentity FromAlias(VariableIdentity other, ISymbol symbol)
            {
                var instance = new VariableIdentity();
                instance.Id = other.Id;
                instance.Symbol = symbol;
                return instance;
            }

            public ISymbol Symbol { get; set; }
            public int Id { get; set; }
        }

        private class MethodOnVariable
        {
            public MethodOnVariable(string methodName, int variableId)
            {
                this.MethodName = methodName;
                this.VariableId = variableId;
            }

            public string MethodName { get; set; }
            public int VariableId { get; set; }
        }
    }
}
