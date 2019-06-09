#if (UNITY_EDITOR)
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
    private static string _projectName = "platformer2";

    [MenuItem("BuildTools/Butler WebGL")]
    public static void BuildGame() {

        DirectoryInfo di = new DirectoryInfo(Application.dataPath);
        string cwd = di.Parent.FullName;

        UnityEngine.Debug.Log("Building WebGL: " + cwd);

        File.Delete("./Builds/webgl.zip");
        BuildPipeline.BuildPlayer(getScenes(), "./Builds/webgl", BuildTarget.WebGL, BuildOptions.None);

        UnityEngine.Debug.Log("Compressing build");
        executeProcess(_zipPath, "a -tzip -r ./Builds/webgl ./Builds/webgl");

        UnityEngine.Debug.Log("Pushing to Itch: " + $"{_butlerPath} push {cwd}\\Builds\\webgl.zip {_itchLogin}/{_projectName}:webgl");
        executeProcess(_butlerPath, $"push {cwd}\\Builds\\webgl.zip {_itchLogin}/{_projectName}:webgl");

        UnityEngine.Debug.Log("WebGL deploy complete");
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