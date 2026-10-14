using UnityEditor;
using UnityEngine;
namespace AccordingToJames.Editor {
 public sealed class MusicImporter:AssetPostprocessor {
  void OnPreprocessAudio(){if(!assetPath.StartsWith("Assets/Resources/Audio/"))return;var importer=(AudioImporter)assetImporter;importer.forceToMono=false;importer.loadInBackground=false;var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.85f;importer.defaultSampleSettings=settings;}
 }
}

