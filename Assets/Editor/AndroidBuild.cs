using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System;
using System.IO;
namespace AccordingToJames.Editor {
 public static class AndroidBuild {
  public static void Build(){
#if UNITY_ANDROID
   ProjectSetup.EnsureScene();AtlasCompiler.CompileAll();
   UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath=Environment.GetEnvironmentVariable("JAMES_ANDROID_SDK");
   UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath=Environment.GetEnvironmentVariable("JAMES_ANDROID_NDK");
   UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath=Environment.GetEnvironmentVariable("JAMES_ANDROID_JDK");
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.accordingtojames.dungeon");
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
   PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel36;
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.Android.bundleVersionCode=AccordingToJames.BuildVersion.AndroidCode;PlayerSettings.bundleVersion=AccordingToJames.BuildVersion.Current;
   PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});EditorUserBuildSettings.buildAppBundle=false;
   Directory.CreateDirectory("Builds/Android");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Dungeon.unity"},"Builds/Android/AccordingToJames.apk",BuildTarget.Android,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Android build failed");
#else
   throw new Exception("Run with -buildTarget Android using an editor with Android support installed");
#endif
  }
 }
}

