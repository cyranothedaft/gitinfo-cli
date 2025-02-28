# Param(
#     [string]$srcprojdir
# )
# 
# . "$PSScriptRoot\_shared-functions.ps1"
# 
# Set-Location $srcprojdir

$exitcode = $LastExitCode
# Write-Host -ForegroundColor DarkGray "dotnet publish exit: $exitcode"
if ( $exitcode -ne 0 ) { exit $exitcode }
