Param(
    [string]$directoryPath = "C:\usr\bin\cmd-tools" # Directory where the executable, cmd-tools.exe, resides. 
                                                    # Specify another directory on the commandline. Ex: .\run-it -directoryPath:"C:\Program Files\cmd-tools"
)
$ErrorActionPreference = "Stop"

if( ! (test-path $directoryPath -PathType Container) ) {
    write-host "The directory, ${directoryPath}, does not exist."
    return
}

$cmd = "${directoryPath}\cmd-tools.exe"

if( ! (test-path $cmd -PathType Leaf) ) {
    write-host "The file, ${cmd}, does not exist."
    return
}

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
