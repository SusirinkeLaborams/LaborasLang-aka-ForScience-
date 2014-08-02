using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;

namespace LaborasLangPackage.MSBuildTasks
{
    public class CleanTask : Task
    {
        public string OutputPath { get; set; }

        public override bool Execute()
        {
            try
            {
                var files = Directory.GetFiles(OutputPath, "*.*");

                foreach (var file in files)
                {
                    try
                    {
                        Log.LogCommandLine(MessageImportance.Normal, string.Format("Clean: \"{0}\"", file));
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Failed to delete \"{0}\": {1}", file, ex.Message));
                    }
                }
                
            }
            catch (Exception e)
            {
                var error = new BuildErrorEventArgs(null, null, "", 0, 0, 0, 0, e.Message, null, "Clean", DateTime.UtcNow);
                BuildEngine.LogErrorEvent(error);
                return false;
            }

            return true;
        }
    }
}
