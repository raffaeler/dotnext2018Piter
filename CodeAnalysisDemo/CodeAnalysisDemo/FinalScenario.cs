using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAnalysisDemo.Helpers;
using CodeAnalysisDemo.Visitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisDemo
{
    public class FinalScenario
    {
        private static readonly string _ruleInterfaceIdentifier = "SampleLibrary.IBizRule";

        public static async Task Run(string solutionPath)
        {
            var buildHelper = new BuildHelper();
            var solution = await buildHelper.OpenSolutionAsync(solutionPath);
            var context = await InspectorContext.Create(solution);


            var members = await MemberExtractorVisitor.GetMembers(context, solution);
            var filter = "ProcessOrder";

            Console.WriteLine($"Find the sequence of methods calling '{filter}'\r\n");

            var processOrderMethod = members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.ToString().Contains(filter))
                .FirstOrDefault();

            // walk the call graph upwards and find the "top" (not referenced) method(s)
            var topMembers = DeclarationUpVisitor.Start(context, processOrderMethod);
            var topMember = topMembers
                .OfType<MethodDeclarationSyntax>()
                .First();

            Console.WriteLine($"The top method we walk down is '{topMember.Identifier.ToString()}'");

            var sequence = CallSequenceVisitor2.Start(context, topMember, _ruleInterfaceIdentifier);
            var str = string.Join("\r\n", sequence.Select(s => "[" + string.Join(";", s) + "]"));
            Console.WriteLine(str);


            Console.WriteLine("\r\nPress any key to exit");
            Console.ReadKey();
        }
    }
}
