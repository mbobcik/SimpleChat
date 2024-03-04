<#
    Starts Full build process of Chat solution.

#>
param (
    [Parameter()]
    [ValidateSet("Debug","Release")]
    [string]
    $Configuration = "Release"
)

$sourceRoot = Resolve-Path "$PSScriptRoot/../src"
$solutionFile = Resolve-Path "$sourceRoot/Chat.sln"

. $PSScriptRoot/Restore.ps1 -SolutionFile $solutionFile
. $PSScriptRoot/Build.ps1 -SolutionFile $solutionFile -Configuration $Configuration

# TODO: 
# - Test
# - Copy binaries to root folder
# - Pack
# - Clean before build