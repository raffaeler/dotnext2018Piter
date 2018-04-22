using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CodeAnalysisDemo.Helpers
{
    /// <summary>
    /// This is an helper class registering a specific version of the MSBuild tools
    /// that are needed to compile a solution or a project
    /// The goal is to obtain either a 'Solution' or 'Project' object that can be
    /// used by Roslyn APIs to analyze the code.
    /// Currently MSBuild can only work as a desktop standard and not 'netstandard'.
    /// </summary>
    public class BuildHelper
    {
        public MSBuildWorkspace Workspace { get; private set; }

        public BuildHelper(string versionHint = null)
        {
            VisualStudioInstance instance = null;
            if (!string.IsNullOrEmpty(versionHint))
            {
                var instances = MSBuildLocator.QueryVisualStudioInstances();
                instance = instances.FirstOrDefault(i => i.Version.ToString().Contains(versionHint));
                if (instance != null)
                {
                    // register the instance whose version matches with the hinted one
                    MSBuildLocator.RegisterInstance(instance);
                    return;
                }
            }

            if (instance == null)
            {
                // register the default instead
                instance = MSBuildLocator.RegisterDefaults();
            }

            Debug.WriteLine($"BuildHelper: registered instance: {GetVisualStudioString(instance)}");
            Workspace = MSBuildWorkspace.Create();
        }

        public async Task<Solution> OpenSolutionAsync(string solutionFullPath)
        {
            var solution = await Workspace.OpenSolutionAsync(solutionFullPath);
            return solution;
        }

        public async Task<Project> OpenProjectAsync(string projectFullPath)
        {
            var project = await Workspace.OpenProjectAsync(projectFullPath);
            return project;
        }

        private string GetVisualStudioString(VisualStudioInstance visualStudio)
        {
            if (visualStudio == null) return "(not found)";

            return $"{visualStudio.Name} {visualStudio.Version}";
        }
    }
}
