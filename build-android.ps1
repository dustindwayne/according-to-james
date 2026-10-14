param(
 [string]$Editor='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe',
 [string]$Sdk=(Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Android\Sdk'),
 [string]$Ndk=(Join-Path (Split-Path $PSScriptRoot) 'android-tools\android-ndk-r27c'),
 [string]$Jdk=(Join-Path (Split-Path $PSScriptRoot) 'android-tools\OpenJDK')
)
$ErrorActionPreference='Stop'
foreach($taskPath in @($Editor,$Sdk,$Ndk,$Jdk)){if(!(Test-Path -LiteralPath $taskPath)){throw "Missing Android build dependency: $taskPath"}}
$taskStage=Join-Path (Split-Path $PSScriptRoot) 'android-build-project'
New-Item -ItemType Directory -Force -Path $taskStage | Out-Null
foreach($taskFolder in @('Assets','Packages','ProjectSettings')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $taskFolder) -Destination $taskStage -Recurse -Force}
$env:JAMES_ANDROID_SDK=$Sdk;$env:JAMES_ANDROID_NDK=$Ndk;$env:JAMES_ANDROID_JDK=$Jdk
$taskLog=Join-Path (Split-Path $PSScriptRoot) 'build-android.log'
$taskArgs=@('-batchmode','-nographics','-quit','-buildTarget','Android','-projectPath',('"'+$taskStage+'"'),'-executeMethod','AccordingToJames.Editor.AndroidBuild.Build','-logFile',('"'+$taskLog+'"'))
$taskProcess=Start-Process -FilePath $Editor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
if($taskProcess.ExitCode -ne 0){throw "Android build failed: $taskLog"}
$taskOutput=Join-Path (Split-Path $PSScriptRoot) 'According-to-James-Android.apk'
Copy-Item -LiteralPath (Join-Path $taskStage 'Builds\Android\AccordingToJames.apk') -Destination $taskOutput -Force
Write-Output $taskOutput
