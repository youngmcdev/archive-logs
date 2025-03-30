Param(
    [string]$publishTarget = "C:\usr\bin\cmd-tools" # Specify another directory on the commandline. Ex: .\publish-it -publishTarget:D:\programs\cmd-tools
)
$ErrorActionPreference = "Stop"

if( ! (test-path $publishTarget -PathType Container) ) {
    write-host "The directory, ${publishTarget}, does not exist."
    return
}

$cmd = "${publishTarget}\cmd-tools.exe"

# For Web Servers
#$param = 'ark', '--directories-from-config', '--dry-run' # Logging will be generated, but no files will be archived.
#$param = 'ark', '--directories-from-config', '--delete-files' # The log file directories are specified in appsettings.json (see Archive.ArchiveCommandsToInvoke).

# For Local Use
$logdir = "c:\logs\testing"
$param = 'ark', '-t', 'IIS', '-d', "${logdir}\w3svc2001"
#$param = 'ark', '--directories-from-config'

Write-Host "Executing $cmd $param"
Write-Host ''
& $cmd $param
