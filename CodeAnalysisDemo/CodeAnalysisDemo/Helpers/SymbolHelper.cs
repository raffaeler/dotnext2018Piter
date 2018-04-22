using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace CodeAnalysisDemo.Helpers
{
    public static class SymbolHelper
    {
        public static ImmutableArray<SyntaxReference> GetSyntaxReferences(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            if (semanticModel == null)
            {
                return ImmutableArray<SyntaxReference>.Empty;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
            if (symbolInfo.Symbol == null)
            {
                return ImmutableArray<SyntaxReference>.Empty;
            }

            return symbolInfo.Symbol.OriginalDefinition.DeclaringSyntaxReferences;
        }

        public static ITypeSymbol GetTypeSymbol(ISymbol symbol, ITypeSymbol defaultSymbol)
        {
            switch (symbol)
            {
                case ITypeSymbol typeSymbol:
                    return typeSymbol;

                case ILocalSymbol localSymbol:
                    return localSymbol.Type;

                case IEventSymbol eventSymbol:
                    return eventSymbol.Type;

                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.Type;

                case IParameterSymbol parameterSymbol:
                    return parameterSymbol.Type;

                case IPropertySymbol propertySymbol:
                    return propertySymbol.Type;

                case IMethodSymbol methodSymbol:
                    return methodSymbol.ReturnType;

                default:
                    return defaultSymbol;
            }
        }
    }
}
