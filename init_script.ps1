if($isWindows)
{
	$env:NAME="Aragas.TupleEventSystem"
	
	$env:NUGET_NAME="$env:NAME"
	$env:NUGET_TOKEN=""
	$env:NUGET_DEVEL_NAME="$env:NAME-devel"
	$env:NUGET_DEVEL_TOKEN=""
	
	$env:COVERALLS_TOKEN=""
	
	$env:GITHUB_PAGES_TOKEN=""
	
	if ($env:APPVEYOR_REPO_TAG -eq "true")
	{
		Update-AppveyorBuild -Version "$($env:APPVEYOR_REPO_TAG_NAME)"
	}
}
#if($isLinux)
#{
#	$env:NAME="Solution.Linux"
#}

$env:SOLUTION_NAME="$env:NAME.sln"