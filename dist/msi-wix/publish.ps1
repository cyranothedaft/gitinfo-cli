. "$PSScriptRoot\_shared-functions.ps1"

prog '<build msi> staging --> package bin...'
pwsh -WorkingDirectory "$PSScriptRoot\WixTools-MsiPackager" -Command "dotnet build"
$exitcode = $LastExitCode
# Write-Host -ForegroundColor DarkGray "ex:$exitcode"
if ( $exitcode -ne 0 ) { exit $exitcode }
