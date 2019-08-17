#if (UNITY_EDITOR)
using System;
using File = UnityEngine.Windows.File;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;

// Requirements: Butler, 7Zip

public static class ScriptBatch {

    // application paths
    private static string _zipPath = "C:\\Program Files\\7-Zip\\7z.exe";
    private static string _butlerPath = "C:\\Program Files\\butler-windows-amd64\\butler.exe";
    
    // itch paths
    private static string _itchLogin = "searle";
    private static string _projectName = Application.productName.ToLower();

    [MenuItem("BuildTools/Butler WebGL")]
    public static void BuildWebGL() {

        BuildGame(BuildTarget.WebGL);
    }

    [MenuItem("BuildTools/Butler Windows")]
    public static void BuildWindows() {
        BuildGame(BuildTarget.StandaloneWindows);
    }

    public static void BuildGame(BuildTarget buildTarget) {
        
        DirectoryInfo di = new DirectoryInfo(Application.dataPath);
        string cwd = di.Parent.FullName;

        string buildExtension;
        string buildPath;
        switch (buildTarget) {
            case BuildTarget.WebGL:
                buildExtension = "webgl";
                buildPath = $"{buildExtension}";
                break;
            case BuildTarget.StandaloneWindows:
                buildExtension = "win";
                buildPath = $"{buildExtension}/{_projectName}_{buildExtension}.exe";
                break;
            default:
                throw new Exception("Invalid Target Type");
        }
        
        UnityEngine.Debug.Log($"Building {buildExtension}: " + cwd);

        string basename = $"{_projectName}_{buildExtension}";
        string zipPath = "./Builds/" + basename + ".zip";

        Directory.Delete($"./Builds", true);
        
        BuildPipeline.BuildPlayer(getScenes(), $"./Builds/{buildPath}", buildTarget, BuildOptions.None);

        UnityEngine.Debug.Log("Compressing build");
        executeProcess(_zipPath, $"a -tzip -r ./Builds/{basename} ./Builds/{buildExtension}");

        UnityEngine.Debug.Log("Pushing to Itch: " + $"{_butlerPath} push {cwd}\\Builds\\{basename}.zip {_itchLogin}/{_projectName}:{buildExtension}");
        executeProcess(_butlerPath, $"push {cwd}\\Builds\\{basename}.zip {_itchLogin}/{_projectName}:{buildExtension}");

        UnityEngine.Debug.Log($"{buildExtension} deploy complete");
        
    }

    public static string[] getScenes() {

        int count = SceneManager.sceneCountInBuildSettings;

        string[] scenes = new string[count];

        for(int i = 0; i < count; i++) { 
            scenes[i] = "Assets/Scenes/" + System.IO.Path.GetFileName(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));            
        }

        return scenes;

    }    

    public static void executeProcess(string executable, string args) {
        Process p = new Process();
        p.StartInfo.FileName = executable;
        p.StartInfo.Arguments = args;
        p.Start();
        p.WaitForExit();
    }


}
#endif