. "$PSScriptRoot\_shared-functions.ps1"

prog '<publish> source --> staging ...'
pwsh -WorkingDirectory "$PSScriptRoot\..\src\GitInfo-cli" -Command "dotnet publish -p:PublishProfile=FolderProfile-portable --nologo"
$exitcode = $LastExitCode
# Write-Host -ForegroundColor DarkGray "ex:$exitcode"
if ( $exitcode -ne 0 ) { exit $exitcode }

prog '<build msi> staging --> package bin...'
pwsh -WorkingDirectory "$PSScriptRoot\WixTools-MsiPackager" -Command "dotnet build"
$exitcode = $LastExitCode
# Write-Host -ForegroundColor DarkGray "ex:$exitcode"
if ( $exitcode -ne 0 ) { exit $exitcode }
