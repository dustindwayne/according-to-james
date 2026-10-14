using System;
namespace AccordingToJames {
 public static class BuildVersion {public const string Current="0.7.1";public const int AndroidCode=9;}
 public static class ReleasePolicy {
  public static bool TryVersion(string tag,out Version version){version=null;return !string.IsNullOrEmpty(tag)&&Version.TryParse(tag.TrimStart('v','V'),out version);}
  public static bool IsNewer(string tag,string installed){return TryVersion(tag,out var latest)&&TryVersion(installed,out var current)&&latest>current;}
  public static bool IsDownloadUrl(string url){return Uri.TryCreate(url,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&uri.Host=="github.com"&&uri.AbsolutePath.StartsWith("/dustindwayne/according-to-james/releases/download/",StringComparison.Ordinal);}
 }
}
