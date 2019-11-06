if($env:CONFIGURATION -eq "Release" -and $env:APPVEYOR_REPO_TAG -eq "true" -and $isWindows)
{
	cd $env:APPVEYOR_BUILD_FOLDER
	cd common
	nuget pack "$env:NUGET_NAME.nuspec" -Version $env:APPVEYOR_BUILD_VERSION
	$nupkg = (Get-ChildItem "$env:NUGET_NAME*.nupkg")[0];
	Push-AppveyorArtifact $nupkg.FullName -FileName $nupkg.Name -DeploymentName "$env:NUGET_NAME.nupkg";
}