# TODO: perform this via a ~make-like tool

. "$PSScriptRoot\_shared-functions.ps1"

prog '<publish> source --> staging ...'
pwsh -WorkingDirectory "$PSScriptRoot\..\src\GitInfo-cli" -Command "dotnet publish -p:PublishProfile=FolderProfile-portable --nologo"
$exitcode = $LastExitCode
# Write-Host -ForegroundColor DarkGray "ex:$exitcode"
if ( $exitcode -ne 0 ) { exit $exitcode }


# msi-wix\publish.ps1

