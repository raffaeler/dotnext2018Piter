using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace CodeAnalysisDemo.Visitors
{
    public class SyntaxTreeInfo
    {
        public SyntaxTreeInfo(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            this.SyntaxTree = syntaxTree;
            this.SemanticModel = semanticModel;
        }

        public SyntaxTree SyntaxTree { get; private set; }

        public SemanticModel SemanticModel { get; private set; }
    }
}
