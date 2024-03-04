<#
    Builds whole Chat solution

#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("Debug","Release")]
    [string]
    $Configuration = "Release",

    [Parameter(Mandatory=$true)]
    [string]
    $SolutionFile
)

dotnet build $SolutionFile --configuration $Configuration --no-restore