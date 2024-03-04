<#
    Restores packages for solution

#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $SolutionFile
)

dotnet restore $SolutionFile