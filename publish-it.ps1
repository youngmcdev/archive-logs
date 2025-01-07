Param(
    [string]$publishTarget = "C:\usr\bin\cmd-tools" # Specify another directory on the commandline. Ex: .\publish-it -publishTarget:C:\temp\cmd-tools
)
$ErrorActionPreference = "Stop"

if( test-path $publishTarget -PathType Container) {
    get-childitem -path $publishTarget -include *.* -file -recurse | foreach-object { $_.Delete() } 
} else {

    new-item -path $publishTarget -ItemType Directory -ErrorAction Stop | Out-Null
}

write-host "Cmd-Tools will be published to $publishTarget"


dotnet publish -r win-x64 -c Release --self-contained true -property:PublishDir=${publishTarget}
