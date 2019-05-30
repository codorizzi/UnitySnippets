#if (UNITY_EDITOR) 
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics;

public static class ScriptBatch {
    [MenuItem("BuildTools/Butler WebGL")]
    public static void BuildGame() {

        UnityEngine.Debug.Log("Building WebGL");

        File.Delete("./Builds/webgl.zip");
        BuildPipeline.BuildPlayer(getScenes(), "./Builds/webgl", BuildTarget.WebGL, BuildOptions.None);

        UnityEngine.Debug.Log("Compressing build");
        executeProcess("C:\\Program Files\\7-Zip\\7z.exe", "a -tzip -r ./Builds/webgl ./Builds/webgl");

        UnityEngine.Debug.Log("Pushing to Itch");
        executeProcess("C:\\Program Files\\butler-windows-amd64\\butler.exe", "push C:\\Users\\Searle\\Source\\Repos\\Platformer2\\Builds\\webgl.zip searle/platformer1:webgl");

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