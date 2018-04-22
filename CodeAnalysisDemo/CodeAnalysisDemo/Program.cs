using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisDemo
{
    class Program
    {
        public static readonly string Sample =
            @"..\..\..\..\SampleSolution2\SampleSolution2.sln";
        static void Main(string[] args)
        {
            FinalScenario.Run(Sample).Wait();
        }
    }
}
