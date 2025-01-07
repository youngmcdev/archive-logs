Param(
    [string]$publishTarget = "C:\usr\bin\cmd-tools" # Specify another directory on the commandline. Ex: .\publish-it -publishTarget:C:\temp\cmd-tools
)
$ErrorActionPreference = "Stop"

if( ! (test-path $publishTarget -PathType Container) ) {
    write-host "The directory, ${publishTarget}, does not exist."
    return
}

$logdir = "c:\logs\testing"

$cmd = "${publishTarget}\cmd-tools.exe"
$param = 'ark', '-t', 'IIS', '-d', "${logdir}\w3svc2001"
#$param = 'ark', '--directories-from-config'
Write-Host "Executing $cmd $param"
Write-Host ''
& $cmd $param
