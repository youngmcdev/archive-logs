$7zipPath = 'D:\Programs\7zip\7z.exe'
$endl = [Environment]::NewLine
$homeDirectory = get-location
$now = get-date
$today = $now.Date
$archivingLogFileName = "archive-olb-log-files-$($now.tostring("yyyyMMddhhmmss")).log" # Stores the logging for this script.
$archivingLogFile = Join-Path -Path $homeDirectory -ChildPath $archivingLogFileName # Full path to the log file.
$dateOffset = -14 # How many days of logs should remain unarchived?
$cutoffDate = $today.AddDays($dateOffset)
$archiveFileNameMonthOffset = 0 # Used in the naming of the archive files.

# Used to collect stats.
$numberOfFilesArchived = 0
$totalBytesArchived = 0


$baseLogFilePath = "D:\logs" # Root of all log files to be achived.
#$logFileType = "W3SVC"
# IIS website IDs we use.
#$iisWebsiteIds = @(1,2,3,4,5,6,1010,1020,1040,1050,1055,1060,1070,1090,1100,1110,1120,1130,1200,2000,2050,3000)
#$iisWebsiteIds = @(2050,2112,2000)
#$iisLogDirectories = $iisWebsiteIds | ForEach-Object { Join-Path -Path $baseLogFilePath -ChildPath "$logFileType$_" }

#$iisLogFileNamePattern = '(u_ex)([0-9]{6})\.(log)' 

Enum SiteTypes{
    None = 0
    ApiMoneyDesktop # Date in Matches[2]. No date parsing requiered - already in yyyy-MM-DD
    ApiVersions # Same as ApiPlatformSettings
    ApiPlatformSettings # Date in Matches[2]. Date in yyyyMMDD format and parsed out into yyyy-MM-DD.
    GoItsMe # Date in Matches[2]. Date in yyyyMM format.
    SplashPageManager # Same as GoItsMe
    PibItsMe # Same as GoItsMe
    SmsItsMe # Same as ApiPlatformSettings
    PibBizLink # Can be the same as ApiPlatformSettings
}

class LogFileProperties{
    [string] $Location = 'cua-service' # Location under $baseLogFilePath where the log files exist.
    [string] $Pattern = 'log.txt' # Regular expression that will be used to identify the log files within this.$Location
    [SiteTypes] $SiteType = [SiteTypes]::None # A category for how the log file should be processed.

    LogFileProperties(){}
}

# These are the locations where log files will be archived.
$logFileLocations = @(
    [LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "api.itsme247.com\moneydesktop"; Pattern = "(moneydesktop\.log\.)(20[0-9-]{8})(\.[0-9]+)?"; SiteType = [SiteTypes]::ApiMoneyDesktop},
    [LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "api.itsme247.com\platformsettings"; Pattern = "(platformsettingsApi\.log\.)(20[0-9]{6})(\.log)"; SiteType = [SiteTypes]::ApiPlatformSettings},
    [LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "api.itsme247.com\versions"; Pattern = "(verisonsApi\.log\.)(20[0-9]{6})(\.log)"; SiteType = [SiteTypes]::ApiVersions},
    #[LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "go.itsme247.com"; Pattern = "(goservice[0-9_]+ - CU0Base [0-9_]+\.log)(20[0-9]{4})"; SiteType = [SiteTypes]::GoItsMe},
    #[LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "itsme247.com"; Pattern = "(splash[0-9_]+\.log)(20[0-9]{4})"; SiteType = [SiteTypes]::SplashPageManager},
    #[LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "pib.itsme247.com"; Pattern = "(pib[0-9_]+\.log)(20[0-9]{4})"; SiteType = [SiteTypes]::PibItsMe},
    #[LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "pib.itsmybiz247.com"; Pattern = "(pib_mlo\.log\.)(20[0-9]{6})\.(log)"; SiteType = [SiteTypes]::PibBizLink},
    [LogFileProperties]@{Location =  Join-Path -Path $baseLogFilePath -ChildPath "sms.itsme247.com"; Pattern = "(TextBanking[0-9_]+\.log\.)(20[0-9]{6})\.(log)"; SiteType = [SiteTypes]::SmsItsMe}
)

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

function GetDateFromFileName{
    param([string]$fileName, [LogFileProperties]$fileProperties)

    if (-not($fileName -match $fileProperties.Pattern)) {
        $null = write-log "  * File, $fileName, does not match /$($fileProperties.Pattern)/ and will be excluded from the archive."
        return [DateTime]::MinValue
    }

    $fileDate = [DateTime]::MinValue

    switch($fileProperties.SiteType){
        ([SiteTypes]::ApiMoneyDesktop){
            $fileNameDate = $Matches[2]
            $fileDate = get-date -Date $fileNameDate
        }
        {($_ -eq [SiteTypes]::ApiPlatformSettings) -or ($_ -eq [SiteTypes]::PibBizLink) -or ($_ -eq [SiteTypes]::SmsItsMe) -or ($_ -eq [SiteTypes]::ApiVersions)}{
            $fileNameDate = $Matches[2]
            $dateCharacters = $fileNameDate.ToCharArray()
            $year = -join ($dateCharacters | select-object -first 4)
            $month = $dateCharacters[4] + $dateCharacters[5]
            $day = -join ($dateCharacters | select-object -last 2)
            $fileDate = get-date -Date "$year-$month-$day"
        }
        {($_ -eq [SiteTypes]::GoItsMe) -or ($_ -eq [SiteTypes]::PibItsMe) -or ($_ -eq [SiteTypes]::SplashPageManager)}{

        }
    }

    return $fileDate
}

# Return the list of files within a directory that should be archived.
function GetFilesToArchive{
    param([string]$directoryPath, [LogFileProperties]$fileProperties)

    $fileList = get-childitem -path $directoryPath # files within the directory
    $archiveSource = [FileArchiveSource]::new()

    foreach($currentFile in $fileList){
        $fileName = $currentFile.Name
        if (-not($fileName -match $fileProperties.Pattern)) {
            $null = write-log "  * File, $fileName, does not match /$($fileProperties.Pattern)/ and will be excluded from the archive."
            continue 
        }
        # TODO: Determine how to process the file name based on $fileProperties.SiteType
        $fileDate = GetDateFromFileName $fileName $fileProperties
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

$logFileLocationList = $logFileLocations.Location -join ","
write-log "${endl}List of directories where log files may be archived: ${logFileLocationList}"

$logFileLocations | foreach-object{
    $logFileLocationProperties = $_
    $logDirectoryExists = test-path -path $logFileLocationProperties.Location -pathtype Container
    if(!($logDirectoryExists)){
        write-log "${endl}* $($logFileLocationProperties.Location) is NOT a directory"
        return
    }

    write-log "${endl}Get list of log files in directory, $($logFileLocationProperties.Location)."
    Set-Location -Path $logFileLocationProperties.Location
    $curPath = Get-Location
    $curDirectoryName = Split-Path -Path $curPath -Leaf
    write-log "  Moved to $curPath and the directoy name is ${curDirectoryName}."

    $filesToBeArchived = GetFilesToArchive $curPath $logFileLocationProperties
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

    write-log "${endl}  Done processing log files in $($logFileLocationProperties.Location)."
    Set-Location $homeDirectory
}

$now = get-date
write-log "${endl}** Processing complete $($now.tostring("yyyy.MM.dd hh:mm:ss zzz")) **" $true
write-log "   Number of Files Archived: ${numberOfFilesArchived} ($([math]::round($totalBytesArchived / 1MB, 2)) MB)" $true
write-log "${endl}Review the log at ${archivingLogFile}${endl}" $true