using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
namespace AccordingToJames.Editor
{
    public class ArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture(){if(!assetPath.Contains("Resources/Art/"))return;var t=(TextureImporter)assetImporter;if(!assetPath.Contains("/Compiled/"))t.textureType=TextureImporterType.Default;t.isReadable=true;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=4096;t.npotScale=TextureImporterNPOTScale.None;}
    }
    [InitializeOnLoad] public static class ProjectSetup
    {
        static ProjectSetup(){EditorApplication.delayCall+=EnsureScene;}
        [MenuItem("Tools/According to James/Open Game Scene")]
        public static void EnsureScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;const string path="Assets/Scenes/Dungeon.unity";
            if(!File.Exists(path)){Directory.CreateDirectory("Assets/Scenes");var s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(s,path);}
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
            if(EditorSceneManager.GetActiveScene().path!=path)EditorSceneManager.OpenScene(path);
            PlayerSettings.companyName="According to James";PlayerSettings.productName="According to James";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;
            AssetDatabase.SaveAssets();
        }
        public static void BuildWindows(){EnsureScene();Directory.CreateDirectory("Assets/Resources/Materials");if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/DungeonSurface.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")),"Assets/Resources/Materials/DungeonSurface.mat");AssetDatabase.SaveAssets();AtlasCompiler.CompileAll();Directory.CreateDirectory("Builds/Windows");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Dungeon.unity"},"Builds/Windows/AccordingToJames.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Unity player build failed");}
        public static void VerifyImport(){EnsureScene();if(!Resources.Load<Texture2D>("Art/HeroWalk"))throw new System.Exception("Missing hero animation");Debug.Log("ACCORDING_TO_JAMES_IMPORT_OK");}
    }
}
