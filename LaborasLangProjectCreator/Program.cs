using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangProjectCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: LaborasLangProjectCreator.exe <LLFilePath> <OutputDir>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Input file doesn't exist!");
                return;
            }

            var solutionDir = args[1];
            var projectName = Path.GetFileNameWithoutExtension(args[0]);
            var projectDir = Path.Combine(solutionDir, projectName);
            
            if (!Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }

            File.Copy(args[0], Path.Combine(projectDir, Path.GetFileName(args[0])), true);

            // Solution file
            File.WriteAllText(Path.Combine(solutionDir, projectName + ".sln"), string.Format(Templates.SLNTemplate, projectName));

            // Project file
            File.WriteAllText(Path.Combine(projectDir, projectName + ".vcxproj"), 
                string.Format(Templates.VCXProjTemplate, projectName + ".ll", projectName + ".exe"));

            // Filters file
            File.WriteAllText(Path.Combine(projectDir, projectName + ".vcxproj.filters"),
                string.Format(Templates.FiltersTemplate, projectName + ".ll"));

            // Users file
            File.WriteAllText(Path.Combine(projectDir, projectName + ".vcxproj.user"), Templates.UsersTemplate);
        }
    }
}
