using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisDemo.Visitors
{
    public class MemberExtractorVisitor : CSharpSyntaxWalker
    {
        private List<MemberDeclarationSyntax> _members;
        private SemanticModel _semanticModel;
        private bool _collectMethods;
        private bool _collectProperties;

        private MemberExtractorVisitor(bool collectMethods, bool collectProperties)
        {
            _members = new List<MemberDeclarationSyntax>();
            _collectMethods = collectMethods;
            _collectProperties = collectProperties;
        }

        public static async Task<IList<MemberDeclarationSyntax>> GetMembers(
            InspectorContext context, Solution solution,
            bool collectMethods = true, bool collectProperties = false)
        {
            var visitor = new MemberExtractorVisitor(collectMethods, collectProperties);

            foreach (var project in solution.Projects)
            {
                visitor._members.AddRange(
                    await GetMembers(context, project, collectMethods, collectProperties));
            }

            return visitor._members;
        }

        public static async Task<IList<MemberDeclarationSyntax>> GetMembers(
            InspectorContext context, Project project,
            bool collectMethods = true, bool collectProperties = false)
        {
            var visitor = new MemberExtractorVisitor(collectMethods, collectProperties);
            foreach (var document in project.Documents)
            {
                visitor._members.AddRange(
                    await GetMembers(context, document, collectMethods, collectProperties));
            }

            return visitor._members;
        }

        public static async Task<IList<MemberDeclarationSyntax>> GetMembers(
            InspectorContext context, Document document,
            bool collectMethods = true, bool collectProperties = false)
        {
            var visitor = new MemberExtractorVisitor(collectMethods, collectProperties);
            var root = await document.GetSyntaxRootAsync();
            visitor._semanticModel = context.GetSemanticModelFor(root);

            visitor.Visit(root);
            return visitor._members;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_collectMethods)
            {
                _members.Add(node);
            }
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (_collectProperties)
            {
                _members.Add(node);
            }
        }
    }
}
