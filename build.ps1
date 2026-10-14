param([string]$Editor = 'C:\Program Files\Unity 6000.3.12f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $Editor)) { throw "Unity editor not found: $Editor" }
$taskBuildArgs = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $PSScriptRoot + '"'), '-executeMethod', 'AccordingToJames.Editor.ProjectSetup.BuildWindows', '-logFile', ('"' + (Join-Path $PSScriptRoot 'build-windows.log') + '"'))
$taskBuildProcess = Start-Process -FilePath $Editor -ArgumentList $taskBuildArgs -WindowStyle Hidden -PassThru
$taskBuildProcess.WaitForExit()
if ($taskBuildProcess.ExitCode -ne 0) { throw 'Unity build failed. See build-windows.log.' }
Write-Output (Join-Path $PSScriptRoot 'Builds\Windows\AccordingToJames.exe')
