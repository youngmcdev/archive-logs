$7zipPath = 'D:\Programs\7zip\7z.exe'
$endl = [Environment]::NewLine
$homeDirectory = get-location
$now = get-date
$today = $now.Date
$archivingLogFileName = "archive-iis-log-files-$($now.tostring("yyyyMMddhhmmss")).log" # Logging for this script.
$archivingLogFile = Join-Path -Path $homeDirectory -ChildPath $archivingLogFileName
$dateOffset = -14 # How many days of logs should remain unarchived?
$cutoffDate = $today.AddDays($dateOffset)
$numberOfFilesArchived = 0
$totalBytesArchived = 0
$archiveFileNameMonthOffset = 0 

$baseLogFilePath = "D:\logs" # Root of all log files to be achived.
$logFileType = "W3SVC"
# IIS website IDs we use.
$iisWebsiteIds = @(1,2,3,4,5,6,1010,1020,1040,1050,1055,1060,1070,1090,1100,1110,1120,1130,1200,2000,2050,3000)
#$iisWebsiteIds = @(2050,2112,2000) # Scaled down for testing
$iisLogDirectories = $iisWebsiteIds | ForEach-Object { Join-Path -Path $baseLogFilePath -ChildPath "$logFileType$_" }

$iisLogFileNamePattern = '(u_ex)([0-9]{6})\.(log)' # What do the log file names look like?

# Write to a log file and/or the terminal.
function Write-Log{
    param([string]$message, [bool]$toConsole = $false)
    if(!(test-path $archivingLogFile)){
       new-item -path $homeDirectory -name $archivingLogFileName -type File -Value "" -ErrorAction Stop | Out-Null
    }
    $message | Out-File -FilePath $archivingLogFile -Append -ErrorAction Stop

    if($toConsole){
        Write-Host $message
    }
    return
}

class FileArchiveSource{
    [long] $Bytes = 0
    [System.Collections.ArrayList] $Files = [System.Collections.ArrayList]::new()

    FileArchiveSource(){}
}

# Return the list of files within a directory that should be archived.
function GetFilesToArchive{
    param([string]$directoryPath, [string]$fileNamePattern = "file\.log")

    $fileList = get-childitem -path $directoryPath # files within the directory
    $archiveSource = [FileArchiveSource]::new()

    foreach($currentFile in $fileList){
        $fileName = $currentFile.Name
        if (-not($fileName -match $fileNamePattern)) {
            $null = write-log "  * File, $fileName, does not match /${fileNamePattern}/ and will be excluded from the archive."
            continue 
        }
        
        $fileNameDate = $Matches[2]
        $dateCharacters = $fileNameDate.ToCharArray()
        $year = -join ($dateCharacters | select-object -first 2)
        $month = $dateCharacters[2] + $dateCharacters[3]
        $day = -join ($dateCharacters | select-object -last 2)
        $fileDate = get-date -Date "20$year-$month-$day"
        if($fileDate -le $cutoffDate){
            $null = write-log "  ${fileName}: $($fileDate.ToString("yyyy-MM-dd")) is less than or equal to $($cutoffdate.ToString("yyyy-MM-dd")). Archive this file ($([math]::round($currentFile.Length / 1KB, 2)) KB)."
            [void]$archiveSource.Files.Add($currentFile)
            $archiveSource.Bytes = $archiveSource.Bytes + $currentFile.Length
        }
        else{
            $null = write-log "  ${fileName}: $($fileDate.ToString("yyyy-MM-dd")) is greater than $($cutoffdate.ToString("yyyy-MM-dd")). Do not archive this file."
        }
    }
    return $archiveSource
}

# ---------- Begin Execution ---------- #

write-log "** Start processing $($now.tostring("yyyy.MM.dd hh:mm:ss zzz")) **" $true

if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
    write-log "${endl}The path to 7-zip, $7zipPath, was not found."
    throw "7 zip file '$7zipPath' not found"
}
Set-Alias Start-SevenZip $7zipPath # Create an alias for the 7-zip command.

$iisLogDirectoryList = $iisLogDirectories -join ","
write-log "${endl}List of IIS log directories where log files may be archived: ${iisLogDirectoryList}"

$iisLogDirectories | foreach-object{
    $iisLogDirectory = $_
    $iisLogDirectoryExists = test-path -path $iisLogDirectory -pathtype Container
    if(!($iisLogDirectoryExists)){
        write-log "${endl}* $iisLogDirectory is NOT a directory"
        return
    }

    write-log "${endl}Get list of log files in directory, ${iisLogDirectory}."
    Set-Location -Path $iisLogDirectory
    $curPath = Get-Location
    $curDirectoryName = Split-Path -Path $curPath -Leaf
    write-log "  Moved to $curPath and the directoy name is ${curDirectoryName}."

    $filesToBeArchived = GetFilesToArchive $curPath $iisLogFileNamePattern
    if($filesToBeArchived.Files.Count -le 0) 
    { 
        write-log "$endl  * There are no files to archive."
        return
    }
    $totalBytesArchived = $totalBytesArchived + $filesToBeArchived.Bytes
    $numberOfFilesArchived = $numberOfFilesArchived + $filesToBeArchived.Files.Count
    $sourceFiles = ($filesToBeArchived.Files | select-object -ExpandProperty Name) -join ' '
    write-log "${endl}  $($filesToBeArchived.Files.Count) files ($([math]::round($filesToBeArchived.Bytes / 1MB, 2)) MB) will be archived."

    # There will be one archive per month.
    $archiveFileName = join-path -path $curPath -ChildPath "${curDirectoryName}-log-archive-$($today.AddMonths($archiveFileNameMonthOffset).ToString("yyyyMM")).7z" 
    write-log "${endl}  Executing the following command to archive files"
    
    # Only one of the "Start-SevenZip" commands below should be uncommented. One deletes the archived files after archiving, and the other does not delete archived files.

    # Don't delete files after archiving.
    #write-log "$7zipPath a -t7z -m0=lzma -mx=5 -y $archiveFileName $sourceFiles"
    #Start-SevenZip a -t7z -m0=lzma -mx=5 -y $archiveFileName $filesToBeArchived.Files
    
    # Delete files after archiving.
    write-log "$7zipPath a -t7z -m0=lzma -mx=5 -y -sdel $archiveFileName $sourceFiles"
    #Start-SevenZip a -t7z -m0=lzma -mx=5 -y -sdel $archiveFileName $filesToBeArchived.Files

    write-log "${endl}  Done processing log files in ${iisLogDirectory}."
    Set-Location $homeDirectory
}

$now = get-date
write-log "${endl}** Processing complete $($now.tostring("yyyy.MM.dd hh:mm:ss zzz")) **" $true
write-log "   Number of Files Archived: ${numberOfFilesArchived} ($([math]::round($totalBytesArchived / 1MB, 2)) MB)" $true
write-log "${endl}Review the log at ${archivingLogFile}${endl}" $true