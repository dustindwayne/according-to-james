using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
namespace AccordingToJames {
 public sealed class ReleaseUpdater:MonoBehaviour {
  [Serializable] public class Asset {public string name,browser_download_url,digest;public long size;}
  [Serializable] public class Release {public string tag_name;public bool draft,prerelease;public Asset[] assets;}
  public string State {get;private set;}="idle";public string Message {get;private set;}="Version "+BuildVersion.Current+" · Check GitHub for updates.";
  public string DownloadPath {get;private set;} public bool Busy=>State=="checking"||State=="downloading";public bool CanDownload=>State=="available";public bool Downloaded=>State=="downloaded";
  Asset selected;string releaseTag;public string TestDirectory;UnityWebRequest active;
  public void Check(){Check(BuildVersion.Current);}public void Check(string installed){if(Busy)return;StartCoroutine(CheckRoutine(installed));}
  IEnumerator CheckRoutine(string installed){selected=null;State="checking";Message="Checking GitHub…";
   using(var request=UnityWebRequest.Get("https://api.github.com/repos/dustindwayne/according-to-james/releases/latest")){active=request;request.timeout=20;request.SetRequestHeader("Accept","application/vnd.github+json");request.SetRequestHeader("User-Agent","AccordingToJames/"+BuildVersion.Current);yield return request.SendWebRequest();active=null;
    if(request.result!=UnityWebRequest.Result.Success){Fail(request.responseCode==403||request.responseCode==429?"GitHub is busy. Please try again later.":"Could not check updates. Check your connection and retry.");yield break;}
    Release release=null;try{release=JsonUtility.FromJson<Release>(request.downloadHandler.text);}catch(Exception){}if(release==null||release.draft||release.prerelease||!ReleasePolicy.TryVersion(release.tag_name,out _)){Fail("GitHub returned an invalid release.");yield break;}
    if(!ReleasePolicy.IsNewer(release.tag_name,installed)){State="current";Message="You’re up to date · Version "+BuildVersion.Current;yield break;}
    string assetName=Application.platform==RuntimePlatform.Android?"According-to-James-Android.apk":"According-to-James-Windows.zip";if(release.assets!=null)foreach(var asset in release.assets)if(asset.name==assetName&&asset.size>0&&ReleasePolicy.IsDownloadUrl(asset.browser_download_url)){selected=asset;break;}
    if(selected==null){Fail("The latest release has no download for this platform yet.");yield break;}releaseTag=release.tag_name;State="available";Message="Update "+releaseTag+" available · "+Mathf.CeilToInt(selected.size/1048576f)+" MB";
   }
  }
  public void Download(){if(!CanDownload)return;StartCoroutine(DownloadRoutine());}
  IEnumerator DownloadRoutine(){State="downloading";string directory=string.IsNullOrEmpty(TestDirectory)?Path.Combine(Application.persistentDataPath,"Updates"):TestDirectory;string temporary=null;string target=null;
   try{Directory.CreateDirectory(directory);target=Path.Combine(directory,releaseTag+"-"+selected.name);temporary=target+"."+Guid.NewGuid().ToString("N")+".part";}catch(Exception){Fail("Cannot create the download folder. Check available storage.");yield break;}
   using(var request=UnityWebRequest.Get(selected.browser_download_url)){active=request;request.timeout=180;request.downloadHandler=new DownloadHandlerFile(temporary){removeFileOnAbort=true};var operation=request.SendWebRequest();while(!operation.isDone){Message="Downloading "+releaseTag+" · "+Mathf.RoundToInt(Mathf.Clamp01(request.downloadProgress)*100)+"%";yield return null;}active=null;
    if(request.result!=UnityWebRequest.Result.Success){TryRemove(temporary);Fail("Download failed. Check your connection or storage, then retry.");yield break;}
   }
   try{if(new FileInfo(temporary).Length!=selected.size)throw new IOException("Incomplete download");if(!string.IsNullOrEmpty(selected.digest)&&selected.digest.StartsWith("sha256:")){using(var stream=File.OpenRead(temporary))using(var hash=SHA256.Create()){string actual=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();if(actual!=selected.digest.Substring(7).ToLowerInvariant())throw new IOException("Checksum mismatch");}}
    if(File.Exists(target))target=Path.Combine(directory,releaseTag+"-"+Guid.NewGuid().ToString("N").Substring(0,8)+"-"+selected.name);File.Move(temporary,target);DownloadPath=target;State="downloaded";Message="Downloaded "+releaseTag+". Open the download to install it.";
   }catch(Exception){TryRemove(temporary);Fail("Could not verify or save the download. Please retry.");}
  }
  static void TryRemove(string path){try{if(File.Exists(path))File.Delete(path);}catch(Exception){}}
  void Fail(string message){State="error";Message=message;}
  public void OpenDownload(){if(!Downloaded)return;Application.OpenURL(new Uri(Path.GetDirectoryName(DownloadPath)).AbsoluteUri);}
  void OnDestroy(){if(active!=null)active.Abort();}
 }
}
