using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisDemo.Visitors
{
    public class InspectorContext
    {
        private InspectorContext(string solutionFullPath)
        {
            SolutionFullPath = solutionFullPath;
            SolutionFolder = System.IO.Path.GetDirectoryName(solutionFullPath);

            VisitedMemberNames = new HashSet<string>();
            CallDescriptors = new List<CallDescriptor>();
        }

        public string SolutionFolder { get; private set; }
        public string SolutionFullPath { get; private set; }
        public Solution Solution { get; private set; }

        public HashSet<string> VisitedMemberNames { get; private set; }

        public IList<SyntaxTreeInfo> SyntaxTreeInfos { get; private set; }

        public IList<CallDescriptor> CallDescriptors { get; private set; }

        public static async Task<InspectorContext> Create(Solution solution)
        {
            var instance = new InspectorContext(solution.FilePath);
            await instance.CompileAsync(solution);
            return instance;
        }

        public SemanticModel GetSemanticModelFor(SyntaxNode node)
        {
            var filePath = node.SyntaxTree.FilePath;
            var semanticModel = SyntaxTreeInfos
                .Where(s => s.SyntaxTree.FilePath == filePath)
                .Select(s => s.SemanticModel)
                .FirstOrDefault();

            return semanticModel;
        }

        public INamedTypeSymbol GetTypeByMetadataName(string name)
        {
            foreach (var semanticModel in SyntaxTreeInfos.Select(sti => sti.SemanticModel))
            {
                var namedTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(name);
                if (namedTypeSymbol != null)
                {
                    return namedTypeSymbol;
                }
            }

            return null;
        }

        private async Task CompileAsync(Solution solution)
        {
            this.Solution = solution;
            SyntaxTreeInfos = new List<SyntaxTreeInfo>();

            var compilationTasks = solution.Projects
                .Select(s => s.GetCompilationAsync())
                .ToArray();

            await Task.WhenAll(compilationTasks);

            foreach (var compilation in compilationTasks.Select(t => t.Result))
            {
                using (var ms = new MemoryStream())
                {
                    var res = compilation.Emit(ms);
                    if (!res.Success)
                    {
                        throw new Exception("Compilation failed in AnalysisContext");
                    }
                }

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxTree, false);
                    SyntaxTreeInfos.Add(new SyntaxTreeInfo(syntaxTree, semanticModel));
                }
            }
        }
    }
}
