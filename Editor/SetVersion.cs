using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace DevTools.Editor
{
    // Will run the script whenever the editor loads
    [InitializeOnLoad]
    public class SetVersion: IPreprocessBuildWithReport
    {
        public int callbackOrder => -5000;

        public void OnPreprocessBuild(BuildReport report)
        {
            Version();
        }

        static SetVersion() 
        {
            Version();
        }

        /// <summary>
        /// Version is selected from last tag following the "v0" format. 
        /// Itll take the number of commits and tag it on the end so that "v1.5" with 26 commits will become "v1.5.26"
        /// </summary>
        [MenuItem("PreBuild/Set Version From Git #b")]
        public static void Version()
        {
            try
            {
                // Get Root Folder for Git
                string gitRepository = GetRootFolder();

                // Get Git Version Number
                string newVersion = GitTagVersion(gitRepository);

                if (Application.version == newVersion)
                {
                    Debug.Log($"Version: {Application.version}");
                    return;
                }

                // Get Project Version File
                string filePath = GetProjectFileName();

                // Replace project version without messing up the rest of the file
                ReplaceVersionInEditor(filePath, newVersion);

                Debug.Log($"New Version: v{newVersion}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not get new version. Version will not be updated. \n" + e);
            }
        }

        private static void ReplaceVersionInEditor(string filePath, string newVersion)
        {
            string yaml = File.ReadAllText(filePath);
            yaml = yaml.Replace($"bundleVersion: {Application.version}", $"bundleVersion: {newVersion}");
            File.WriteAllText(filePath, yaml);
        }

        private static string GitTagVersion(string repository) 
        {
            string version = RunGit(@"describe --tags --long --match ""v[0-9]*""", repository);
            version = version.Replace('-', '.');
            version = version.Substring(1, version.LastIndexOf('.') - 1);
            return version;
        }

        public static string RunGit(string arguments, string path) 
        {
            using (var process = new System.Diagnostics.Process()) 
            {
                int exitCode = process.Run("git", arguments, path,
                    out string output, out string errors);
                if (exitCode == 0)
                    return output;
                else
                    throw new System.Exception($"Git Exit Code: {exitCode} - {errors}");
            }
        }

        private static string GetRootFolder() 
        {
            string path = SanitizePath(Application.dataPath);
            path = Path.Combine(Application.dataPath, SanitizePath("../"));
            path = Path.GetFullPath(path);
            return path;
        }

        private static string GetProjectFileName() 
        {
            string path = GetRootFolder();
            path = Path.Combine(path, SanitizePath("ProjectSettings/ProjectSettings.asset"));
            path = Path.GetFullPath(path);
            return path;
        }

        private static string SanitizePath(string s)
        {
            return s.Replace('/', '\\');
        }
    }
}