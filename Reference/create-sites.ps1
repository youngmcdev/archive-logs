Param(
    [bool]$deleteStuff = $false # When this parameter is specified on the command line, "-deleteStuff:$true", then the sites and app pools are removed from IIS. Nothing else is done.
)
$ErrorActionPreference = "Stop"
Import-Module IISAdministration

# Code snipits for testing.
# $mgr = Get-IISServerManager
# $mgr.ApplicationPools.Remove($mgr.ApplicationPools["itsme247.com"]);$mgr.ApplicationPools.Remove($mgr.ApplicationPools["beta.itsme247.com"]);$mgr.ApplicationPools.Remove($mgr.ApplicationPools["iPay"]);$mgr.ApplicationPools.Remove($mgr.ApplicationPools["Splashpagemanager"]);$mgr.ApplicationPools.Remove($mgr.ApplicationPools["Version Dispatcher"]);$mgr.CommitChanges();
# $mgr.Sites.Remove($mgr.Sites["itsme247.com"]);$mgr.Sites.Remove($mgr.Sites["beta.itsme247.com"]);$mgr.CommitChanges();
# C:\Windows\System32\inetsrv\appcmd.exe list sites /config /xml
# C:\Windows\System32\inetsrv\appcmd.exe list apppool /config /xml

# Add website IDs as needed. The assigned value is the ID of the site in IIS.
# These are the sites that may be created.
Enum SiteIds{
    None = 0
    Monitoring = 1
    ApiCdn = 2
    BizLinkAggregator = 3
    Plaid = 4
    AuthItsMe = 5
    ItsMe = 1000
    # ItsMeMobile = 1010 # Obsolete
    ApiItsMe = 1020
    GoItsMe = 1040
    BizLink = 1050
    LoansItsMe = 1060
    PibItsMe = 1070
    SmsItsMe = 1090
    StaticItsMe = 1100
    #StaticBizLink = 1110 # Obsolete
    #BizLinkMobile = 1120 # Obsolete
    PibBizLink = 1130
    PibServices = 1200
    Iris = 2000
}

# Add app pools as needed. These are the app pools that may be created.
Enum AppPoolTypes{
    None = 0
    ApiDaonOnboarding
    ApiIrisOldVersions
    ApiMoneyDesktop
    ApiV1
    ApiV2
    ApiV3
    ApiV3ConnPool
    ApiVersions
    ApiCdn
    ApiItsMe
    AuthItsMe
    Iris
    GoItsMe
    IPay
    IPayEnrollment
    ItsMe
    BizLink
    LoansItsMe
    #ItsMeMobile # Obsolete
    #BizLinkMobile # Obsolete
    BizLinkAggregator
    Monitoring
    PibItsMe
    PibBizLink
    PibServices
    Plaid
    SmsItsMe
    SplashPageManager
    StaticItsMe
    #StaticBizLink # Obsolete
    VersionDispatcher
    VersionDispatcherBizlink
}

# Map the enum values to the site names in IIS.
function GetSiteName{
    param([SiteIds]$siteId)
    $result = switch($siteId){
        None {''}
        Monitoring {'Monitoring'}
        ApiCdn {'api.cdn'}
        BizLinkAggregator {'min.bizlink247.com'}
        Plaid {'plaid.itsme247.com'}
        AuthItsMe {'auth.itsme247.com'}
        ItsMe {'itsme247.com'}
        #ItsMeMobile {'m.itsme247.com'} # Obsolete
        ApiItsMe {'api.itsme247.com'}
        GoItsMe {'go.itsme247.com'}
        BizLink {'itsmybiz247.com'}
        LoansItsMe {'loans.itsme247.com'}
        PibItsMe {'pib.itsme247.com'}
        SmsItsMe {'sms.itsme247.com'}
        StaticItsMe {'static.itsme247.com'}
        #StaticBizLink {'static.itsmybiz247.com'} # Obsolete
        #BizLinkMobile {'m.itsmybiz247.com'} # Obsolete
        PibBizLink {'pib.itsmybiz247.com'}
        PibServices {'pibservices.local'}
        Iris {'beta.itsme247.com'}
    }

    return $result
}

# Map the enum values to the app pool names in IIS.
function GetAppPoolName{
    param([AppPoolTypes]$appPoolType)
    $result = switch($appPoolType){
        None {''}
        ApiDaonOnboarding {'API Daon Onboarding'}
        ApiIrisOldVersions {'API Iris Old Versions'}
        ApiMoneyDesktop {'API MoneyDesktop'}
        ApiV1 {'API V1'}
        ApiV2 {'API V2'}
        ApiV3 {'API V3'}
        ApiV3ConnPool {'API v3 Conn Pool'}
        ApiVersions {'API Versions'}
        ApiCdn {'api.cdn'}
        ApiItsMe {'api.itsme247.com'}
        AuthItsMe {'auth.itsme247.com'}
        Iris {'beta.itsme247.com'}
        GoItsMe {'go.itsme247.com'}
        IPay {'iPay'}
        IPayEnrollment {'iPayEnrollment'}
        ItsMe {'itsme247.com'}
        BizLink {'itsmybiz247.com'}
        LoansItsMe {'loans.itsme247.com'}
        #ItsMeMobile {'m.itsme247.com'} # Obsolete
        #BizLinkMobile {'m.itsmybiz247.com'} # Obsolete
        BizLinkAggregator {'min.bizlink247.com'}
        Monitoring {'Monitoring'}
        PibItsMe {'pib.itsme247.com'}
        PibBizLink {'pib.itsmybiz247.com'}
        PibServices {'pibservices.local'}
        Plaid {'plaid.itsme2347.com'}
        SmsItsMe {'sms.itsme247.com'}
        SplashPageManager {'Splashpagemanager'}
        StaticItsMe {'static.itsme247.com'}
        #StaticBizLink {'static.itsmybiz247.com'} # Obsolete
        VersionDispatcher {'Version Dispatcher'}
        VersionDispatcherBizlink {'Version Dispatcher - BizLink'}
    }

    return $result
}

class BaseBindingInfo{
    # The following three properties will be turned into a colon-delimited set of data that indicates the IP address, port, and host header that the site listener should be bound to.
    [string] $BindingIpAddress = ""
    [int] $BindingPort = 80
    [string] $BindingHeader = ""

    # A comma-delimited list of protocols that the new site should use.
    [string] $BindingProtocal = "http"

    [string] GetBindingInformation(){
        return "{0}:{1}:{2}" -f $this.BindingIpAddress, $this.BindingPort, $this.BindingHeader
    }
}

# Represents a website binding
class BindingRecord : BaseBindingInfo{

}

# Represents an IIS application
class WebApplicationRecord{
    [string] $Name = ""
    [string] $PhysicalPath = ""
    [AppPoolTypes] $AppPoolType = [AppPoolTypes]::None

    [string] GetPath(){
        return "/$($this.Name)"
    }
}

# Represents an IIS website
class WebsiteRecord : BaseBindingInfo {
    [string] $Name = ""
    # Physical location on the file system
    [string] $PhysicalPath = ""
    [string] $DefaultApplicationSubdirectory = ""
    # Application pool to which this site will be assigned.
    [AppPoolTypes] $AppPoolType = [AppPoolTypes]::None
    [AppPoolTypes] $DefaultApplicationAppPoolType = [AppPoolTypes]::None
    # Web site ID
    [int] $Id
    [bool] $TraceFailedRequestsLogging = $false
    # Bindings to be configured for this website. This is for sites that have more than one binding. It will be empty for sites that have only one binding.
    [BindingRecord[]] $ExtraBindings = [BindingRecord[]]::new(0)
    # Web applications to be configured for this website
    [WebApplicationRecord[]] $WebApplications = [WebApplicationRecord[]]::new(0)

    WebsiteRecord() {}
}

# Represents an IIS application pool with default values - [2023.11.07 Using beta.itsme247.com as the template.]
class AppPoolRecord{
    [string] $Name = ""
    # What version of the .NET runtime to use. Valid options are "v2.0" and
    # "v4.0". IIS Manager often presents them as ".NET 4.5", but these still
    # use the .NET 4.0 runtime so should use "v4.0". For a "No Managed Code"
    # equivalent, pass an empty string.
    [string] $RuntimeVersion = ""
    # As opposed to "Classic"
    [string] $PipelineMode = "Integrated"
    # If your ASP.NET app must run as a 32-bit process even on 64-bit machines
    # set this to $true. This is usually only important if your app depends
    # on some unmanaged (non-.NET) DLL's.
    [bool] $Enable32Bit = $false
    [bool] $AutoStart = $true
    # Another start mode is "OnDemand"
    [string] $StartMode = "AlwaysRunning"
    [int] $QueueLength = 9000
    [bool] $RapidFailProtection = $true
    [TimeSpan] $IdleTimeout = [TimeSpan]::FromMinutes(0) # 2023.11.07 - We decided to go with zero as the default value.
    [TimeSpan] $PeriodicRestartTime = [TimeSpan]::FromMinutes(0) # Now called "Regular Time Interval (minutes)"
    # What account does the application pool run as?
    # "ApplicationPoolIdentity" = best
    # "LocalSysten" = bad idea!
    # "NetworkService" = not so bad
    # "SpecificUser" = useful if the user needs special rights
    [string] $IdentityType = "ApplicationPoolIdentity"

    AppPoolRecord() {}

    AppPoolRecord([string]$name){
        $this.Name = $name
    }
}

#--- Some global values

# IP address for which this script will be executed, or server on which it's executed.
$serverIpAddress = "192.168.128.37"

# Drive for physical paths. This is the directory in which the website folders will be created. 
# Currently "d:\" on our servers.
$driveLetter = "c:\inetpub\wwwroot"

$date = get-date
# Directory where the log for this script will be written
$logDir = "c:\logs"
$logFileName = "create-iis-sites-log-$($date.tostring("yyyyMMddhhmmss")).txt"
$logFile = "$logDir\$logFileName"

# Reference to IIS Manager - All IIS configurations are completed with this.
$iisManager = Get-IISServerManager

# For .NET Core applications
$noRuntimeVersion = ""
# For .Net applicatinos
$v40Runtime = "v4.0"

$endl = [Environment]::NewLine


# Write to the log file and optionally to the console.
function Write-Log{
    param([string]$message, [bool]$toConsole = $true)
    if(!(test-path $logFile)){
       new-item -path $logDir -name $logFileName -type File -Value "" -ErrorAction Stop | Out-Null
    }
    $message | Out-File -FilePath $logFile -Append -ErrorAction Stop

    if($toConsole){
        Write-Host $message
    }
    return
}


#--- Start app pool configurations - Collection of AppPoolRecord objects for the app pools to be created

$appPoolsToBeConfigured = [ordered]@{}

$appPoolType = [AppPoolTypes]::ApiDaonOnboarding
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::ApiIrisOldVersions
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiMoneyDesktop
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiV1
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiV2
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiV3 # PrivaetMemoryLimit:2000000; MaxWorkerProcesses:3
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiV3ConnPool # MaxWorkerProcesses:2
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiVersions
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiCdn
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::ApiItsMe # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::AuthItsMe
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::Iris
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::GoItsMe # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::IPay
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true}
$appPoolType = [AppPoolTypes]::IPayEnrollment
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::ItsMe # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::BizLink # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::LoansItsMe # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::BizLinkAggregator
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::Monitoring
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; }
$appPoolType = [AppPoolTypes]::PibItsMe # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::PibBizLink # IdleTimeOut:0
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::PibServices
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); PipelineMode = "Classic"; RuntimeVersion = $v40Runtime; Enable32Bit = $true}
$appPoolType = [AppPoolTypes]::Plaid
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::SmsItsMe # IdleTimeOut:0 
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::SplashPageManager # IdleTimeOut:0 
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true}
$appPoolType = [AppPoolTypes]::StaticItsMe # IdleTimeOut:0 
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]@{Name = GetAppPoolName($appPoolType); RuntimeVersion = $v40Runtime; Enable32Bit = $true; RapidFailProtection = $false}
$appPoolType = [AppPoolTypes]::VersionDispatcher
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))
$appPoolType = [AppPoolTypes]::VersionDispatcherBizlink
$appPoolsToBeConfigured[$appPoolType] = [AppPoolRecord]::New((GetAppPoolName($appPoolType)))

#--- End app pool configurations


#--- Start website configurations - Collection of WebsiteRecord objects for the sites to be created; including bindings and applications.

function GetWebsiteRecord{
    param([SiteIds]$siteId, [AppPoolTypes]$appPoolType, [string[]]$bindingHeaders, [string]$defaultApplicationSubdirectory = "", [AppPoolTypes]$defaultApplicationAppPoolType = [AppPoolTypes]::None)

    $siteName = GetSiteName($siteId)
    $newSiteRecord = [WebsiteRecord]@{Name = $siteName; Id = $siteId; PhysicalPath = (Join-Path -Path $driveLetter -ChildPath $siteName); AppPoolType = $appPoolType; BindingIpAddress = $serverIpAddress; BindingHeader = $siteName; DefaultApplicationSubdirectory = $defaultApplicationSubdirectory; DefaultApplicationAppPoolType = $defaultApplicationAppPoolType }
    
    if($null -ne $bindingHeaders -and $bindingHeaders.count -gt 0){
        # For some reason writing to log/output seems to change the type of object returned from this method. I don't understand...
        $null = Write-Log "Number of headers is greater than zero!"
        $bindingsArray = [BindingRecord[]]::new($bindingHeaders.Count)
        for($i = 0; $i -lt $bindingHeaders.Count; $i++){
            $bindingsArray[$i] = [BindingRecord]@{BindingIpAddress = $serverIpAddress; BindingHeader = $bindingHeaders[$i]}
            $null = Write-Log ("    BindingHeader: {0}" -f $bindingsArray[$i].GetBindingInformation())
        }

        $newSiteRecord.ExtraBindings = $bindingsArray
    }
    else{
        $null = Write-Log "There are NO headers to process for ${siteName}!"
    }

    return $newSiteRecord
}

$websitesToBeConfigured = [ordered]@{}

$websiteId = [SiteIds]::Monitoring
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::Monitoring) @()
$websiteToBeConfigured.BindingIpAddress = "*"
$websiteToBeConfigured.BindingHeader = ""
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::ApiCdn # api.cdn
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::ApiCdn) @()
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::BizLinkAggregator # min.bizlink247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::BizLinkAggregator) @()
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::Plaid # plaid.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::Plaid) @()
$websiteToBeConfigured.BindingIpAddress = "*"
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::AuthItsMe # auth.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::AuthItsMe) @()
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::ItsMe # itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::ItsMe) @() "StaticRoot"
$websiteToBeConfigured.BindingHeader = "legacy.itsme247.com"
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::ApiItsMe # api.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::ApiItsMe) @("www.$(GetSiteName($websiteId))")
# Applications for {versions, v2, v3, PlatformSettings, MoneyDesktop, GeoLocation, daon}
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "onboarding"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'onboarding'); AppPoolType = [AppPoolTypes]::ApiDaonOnboarding},
    [WebApplicationRecord]@{Name = "GeoLocation"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'GeoLocation\1.0.0.0'); AppPoolType = $websiteToBeConfigured.AppPoolType},
    [WebApplicationRecord]@{Name = "MoneyDesktop"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'MoneyDesktop\20.10.01.00'); AppPoolType = [AppPoolTypes]::ApiMoneyDesktop},
    [WebApplicationRecord]@{Name = "PlatformSettings"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'PlatformSettings'); AppPoolType = $websiteToBeConfigured.AppPoolType},
    [WebApplicationRecord]@{Name = "v1"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'v1'); AppPoolType = [AppPoolTypes]::ApiV1},
    [WebApplicationRecord]@{Name = "v2"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'v2'); AppPoolType = [AppPoolTypes]::ApiV2},
    [WebApplicationRecord]@{Name = "v3"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'v3'); AppPoolType = [AppPoolTypes]::ApiV3},
    [WebApplicationRecord]@{Name = "versions"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'versions'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::GoItsMe # go.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::GoItsMe) @("www.$(GetSiteName($websiteId))") "20.09.00.00"
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::BizLink # itsmybiz247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::BizLink) @("www.bizlink247.com") "" ([AppPoolTypes]::VersionDispatcherBizlink)
$websiteToBeConfigured.BindingHeader = "bizlink247.com"
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "Help"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'Help'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::LoansItsMe # loans.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::LoansItsMe) @("www.$(GetSiteName($websiteId))") "StaticRoot"
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::PibItsMe # pib.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::PibItsMe) @("www.$(GetSiteName($websiteId))") "v.1.5.0"
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "v.1.5.0"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'v.1.5.0'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::SmsItsMe # sms.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::SmsItsMe) @("www.$(GetSiteName($websiteId))")
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "services"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath '20.07.01.00'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::StaticItsMe # static.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::StaticItsMe) @()
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "docs"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'docs'); AppPoolType = $websiteToBeConfigured.AppPoolType},
    [WebApplicationRecord]@{Name = "iris"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'iris'); AppPoolType = $websiteToBeConfigured.AppPoolType},
    [WebApplicationRecord]@{Name = "logos"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'logos'); AppPoolType = $websiteToBeConfigured.AppPoolType},
    [WebApplicationRecord]@{Name = "obc"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'obc'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::PibBizLink #pib.itsmybiz247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::PibBizLink) @("www.$(GetSiteName($websiteId))", "pib.bizlink247.com", "www.pib.bizlink247.com") "StaticRoot"
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "21.02.01.00"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath '21.02.01.00'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured
# MCY - Pick up here
$websiteId = [SiteIds]::PibServices # pibservices.local
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::PibServices) @()
$websiteToBeConfigured.BindingHeader = "PIBServices"
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "services"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'services'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

$websiteId = [SiteIds]::Iris # beta.itsme247.com
$websiteToBeConfigured = GetWebsiteRecord $websiteId ([AppPoolTypes]::Iris) @("itsme2347.com", "www.itsme247.com") "" ([AppPoolTypes]::VersionDispatcher)
$websiteToBeConfigured.WebApplications = @(
    [WebApplicationRecord]@{Name = "SplashPageManager"; PhysicalPath = (Join-Path -Path $driveLetter -ChildPath 'itsme247.com\SplashPageManager'); AppPoolType = [AppPoolTypes]::SplashPageManager},
    [WebApplicationRecord]@{Name = "iPay"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'IPayEnrollment'); AppPoolType = [AppPoolTypes]::IPayEnrollment},
    [WebApplicationRecord]@{Name = "help"; PhysicalPath = (Join-Path -Path $websiteToBeConfigured.PhysicalPath -ChildPath 'help'); AppPoolType = $websiteToBeConfigured.AppPoolType}
)
$websitesToBeConfigured[$websiteId] = $websiteToBeConfigured

#--- End website configurations





#--- Start functions to create stuff

function CreatePhysicalPath{
    # Create a directory on the file system.
    param($path)

    if(!$path){
        Write-Log "Cannot create this path. It is null."
        return
    }

    if((Test-Path $path) -eq $true){
        Write-Log "The path, $path, already exists"
        return
    }

    Write-Log "Creating the path, $path."
    new-item -itemtype Directory -path $path -ErrorAction Stop | Out-Null
}

# Add the folder to the IIS_IUSRS group. In testing so far, any subfolders inherit this as well. 
# Question: Are threre negative ramifications to running this multiple times for a folder? Can we check to see if this has already been done.
function AddFolderToIisUsersGroup(){
    param($path)

    if(!$path){
        Write-Log "Cannot add this path to IIS_IUSRS. It is null."
        return
    }

    if((Test-Path $path) -ne $true){
        Write-Log "The path, $path, does not exist. It cannot be added to IIS_IUSRS."
        return
    }

    Write-Log "The path, $path, will be added to IIS_IUSRS."
    try{
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl = Get-ACL $path
        $acl.AddAccessRule($accessRule)
        Set-ACL -Path $path -ACLObject $acl
    }
    catch{
        Write-Log "ERROR ***${endl}There was a problem adding path, $path, to IIS_IUSRS.${endl}$($_.Exception.Message)${endl}$($_.ScriptStackTrace)${endl}$($_.Exception.StackTrace)"
    }
}

function CreateAppPools{    
    Write-Log "CREATE APP POOLS"
    ForEach($key in $appPoolsToBeConfigured.keys){
        $pool = $appPoolsToBeConfigured[$key]
        if($null -eq $pool){
            Write-Log "Could not find the app pool for $key"
            return
        }
        $null = Write-Log "Type of app pool object: $($pool.GetType())"
        CreateAppPool $pool
    }
}

function CreateWebsites{
    Write-Log "CREATE WEBSITES"
    foreach($key in $websitesToBeConfigured.keys){
        $site = $websitesToBeConfigured[$key]
        if($null -eq $site){
            Write-Log "Could not find the site for $key"
            return
        }
        $null = Write-Log "Type of website object: $($site.GetType())"
        CreateWebsiteAndBindings $site
    }
}

function CreateWebApplications{
    # Iterate over the array of websites. Create each application for each site.
    Write-Log "CREATE WEB APPLICATIONS"
    foreach($website in $websitesToBeConfigured.values){
        Write-Log "Create the applications for website, $($website.Name)."
        $webApps = $website.WebApplications
        foreach($webApp in $webApps){
            $null = Write-Log "Type of web application object: $($webApp.GetType()) - Type of website object: $($website.GetType())"
            CreateWebApplication $webApp $website
        }
    }
}

function CreateAppPool{
    param([AppPoolRecord]$poolInfo)

    if(!($null -eq $iisManager.ApplicationPools[$poolInfo.Name])){
        Write-Log "This app pool already exists. $($poolInfo.Name)"
        return
    }

    Write-Log "This app pool will be created. $($poolInfo.Name)"
    $newPool = $iisManager.ApplicationPools.Add($poolInfo.Name)
    $newPool.ManagedPipelineMode = $poolInfo.PipelineMode
    $newPool.ManagedRuntimeVersion = $poolInfo.RuntimeVersion
    $newPool.enable32BitAppOnWin64 = $poolInfo.Enable32Bit
    $newPool.AutoStart = $poolInfo.AutoStart
    $newPool.StartMode = $poolInfo.StartMode
    $newPool.queuelength = $poolInfo.QueueLength
    $newPool.ProcessModel.IdentityType = $poolInfo.IdentityType
    $newPool.Recycling.PeriodicRestart.Time = $poolInfo.PeriodicRestartTime
    $newPool.Recycling.PeriodicRestart.Schedule.Clear()
    $newPool.ProcessModel.IdleTimeout = $poolInfo.IdleTimeout
    $newPool.Failure.RapidFailProtection = $poolInfo.RapidFailProtection

    $iisManager.CommitChanges()
    
    # This seems to cause an error, and the app pool is starting automatically anyway.
    #$newPool.Start()
}

function CreateWebsiteAndBindings{
    param([WebsiteRecord]$siteInfo)

    if(!($null -eq $iisManager.Sites[$siteInfo.Name])){
        Write-Log "This website already exists. $($siteInfo.Name)"
        return
    }
    
    Write-Log "This website will be created - $($siteInfo.Name)"
    CreatePhysicalPath $siteInfo.PhysicalPath
    AddFolderToIisUsersGroup $siteInfo.PhysicalPath
    $newSite = $iisManager.Sites.Add($siteInfo.Name, $siteInfo.BindingProtocal, $siteInfo.GetBindingInformation(), $siteInfo.PhysicalPath)
    $newSite.Id = $siteInfo.Id

    # Set the app pool for the default application
    $defaultApplication = $newSite.Applications["/"]
    if($defaultApplication){
        if($siteInfo.DefaultApplicationAppPoolType -eq [AppPoolTypes]::None){
            Write-Log "    Assign app pool name: $(GetAppPoolName($siteInfo.AppPoolType))"
            $defaultApplication.ApplicationPoolName = GetAppPoolName($siteInfo.AppPoolType)
        }
        else{
            Write-Log "    Assign app pool name: $(GetAppPoolName($siteInfo.DefaultApplicationAppPoolType))"
            $defaultApplication.ApplicationPoolName = GetAppPoolName($siteInfo.DefaultApplicationAppPoolType)
        }
    
        # Set the physical path of the default application and ensure it exists on the file system.
        $virtualDirectory = $defaultApplication.VirtualDirectories["/"]
        if($virtualDirectory){
            if([string]::IsNullOrWhitespace($siteInfo.DefaultApplicationSubdirectory)){
                $virtualDirectory.PhysicalPath = $siteInfo.PhysicalPath
            }
            else{
                $virtualDirectory.PhysicalPath = Join-Path -Path $siteInfo.PhysicalPath -ChildPath $siteInfo.DefaultApplicationSubdirectory
            }
            CreatePhysicalPath $virtualDirectory.PhysicalPath
        }
        else{
            Write-Log "Colud not find the virtual directory for the default application for website $($siteInfo.Name)"
        }
    }
    else{
        Write-Log "Could not find the default application for website $($siteInfo.Name). Its application pool and physical path have not been set."
    }
    
    # Add bindings
    foreach($curBinding in $siteInfo.ExtraBindings){
        Write-Log "    Binding for site - $($curBinding.GetBindingInformation())"
        $newSite.Bindings.Add($curBinding.GetBindingInformation(), $curBinding.BindingProtocal)
    }
    $iisManager.CommitChanges()
}

function CreateWebApplication{
    param([WebApplicationRecord]$webAppInfo, [WebsiteRecord]$siteInfo)

    if(!($null -eq $iisManager.Sites[$siteInfo.Name].Applications[$webAppInfo.GetPath()])){
        Write-Log "The web application, $($webAppInfo.Name), already exists for website $($siteInfo.Name). "
        return
    }

    Write-Log "The web application, $($webAppInfo.Name), will be created for website $($siteInfo.Name). "
    CreatePhysicalPath $webAppInfo.PhysicalPath
    $siteForApp = $iisManager.Sites[$siteInfo.Name]
    if($siteForApp){
        $newApp = $siteForApp.Applications.Add($webAppInfo.GetPath(), $webAppInfo.PhysicalPath)
        $newApp.ApplicationPoolName = GetAppPoolName($webAppInfo.AppPoolType)
        $iisManager.CommitChanges()
    }
    else{
        Write-Log "Could not find website, $($siteInfo.Name). Application, $($webAppInfo.Name), was not created."
    }
}

function RemoveWebsite{
    param([string]$websiteName)

    $deleted = $false
    if(!($null -eq $iisManager.Sites[$websiteName])){
        Write-Log "Deleting web site, $websiteName."
        $iisManager.Sites.Remove($iisManager.Sites[$websiteName])
        $deleted = $true;
    }
    else{
        Write-Log "Did not find web site, $websiteName, to delete."
    }
    $deleted
}

function RemoveAppPool{
    param([string]$appPoolName)

    $deleted = $false
    if(!($null -eq $iisManager.ApplicationPools[$appPoolName])){
        Write-Log "Deleting app pool, $appPoolName."
        $iisManager.ApplicationPools.Remove($iisManager.ApplicationPools[$appPoolName])
        $deleted = $true
    }
    else{
        Write-Log "Did not find app pool, $appPoolName, to delete."
    }
    $deleted
}

function DeleteDirectory{
    param([string]$directoryName)

    $deleted = $false
    if(test-path $directoryName){
        Write-Log "Deleting directory, $directoryName, and its subdirectories."
        remove-item -Path $directoryName -Force -Recurse
        $deleted = $true
    }else{
        Write-Log "Did not find directory, $directoryName, to delete."
    }
    $deleted
}

function DeleteDirectories{
    param([string[]]$directoryNames)

    foreach($directory in $directoryNames){
        DeleteDirectory $directory
    }
}

function RemoveSitesAndAppPools{
    param([string[]]$webSiteNames, [string[]]$appPoolNames)

    $sitesDeleted = $false
    foreach($siteToDelete in $webSiteNames){
        $deleted = RemoveWebsite($siteToDelete)
        $sitesDeleted = ($sitesDeleted -or $deleted)
    }
    Write-Log "Websites removed? $sitesDeleted"

    $poolsDeleted = $false
    foreach($poolToDelete in $appPoolNames){
        $deleted = RemoveAppPool($poolToDelete)
        $poolsDeleted = ($poolsDeleted -or $deleted)
    }
    Write-Log "App pools removed? $poolsDeleted"

    if($sitesDeleted -or $poolsDeleted) {$iisManager.CommitChanges()} else {Write-Log "No changes, nothing committed."}
}

function RemoveDefaultSitesAndAppPools{
    Write-Log "Remove default app pools and websites."
    $defaultSites = @("Default Web Site")
    $defaultPools = @("DefaultAppPool", "ASP.NET v4.0", "ASP.NET v4.0 Classic", "Classic .NET AppPool", ".NET v2.0", ".NET v2.0 Classic", ".NET v4.5", ".NET v4.5 Classic")
    RemoveSitesAndAppPools $defaultSites $defaultPools
}

function CreateAll{
    # This method makes everything happen.
    write-log "Run all the things..."
    RemoveDefaultSitesAndAppPools
    CreateAppPools
    CreateWebsites
    CreateWebApplications
}

#--- End functions to create stuff




#--- Execution starts here

# Ensure the directory for the log file exists.
if(!(test-path -path $logDir))
{
    Write-Log "Creating log directory, $logDir"
    CreatePhysicalPath $logDir
}

Write-Log $(get-date).tostring("yyyy.MM.dd hh:mm:ss")
if($deleteStuff -eq $true){
    # The deleteStuff option was specified on the command line. Sites and App Pools will be deleted.

    # web sites and app pools to delete
    $sitesToDelete = [System.Enum]::GetNames( [SiteIds] )
    $poolsToDelete = [System.Enum]::GetNames( [AppPoolTypes] )    
    # we need their names
    $siteNamesToDelete = $sitesToDelete | ForEach-Object {if($_ -NotMatch '^None$') {GetSiteName($_)}}
    $poolNamesToDelete = $poolsToDelete | ForEach-Object {if($_ -NotMatch '^None$') {GetAppPoolName($_)}}
    # delete them
    RemoveSitesAndAppPools $siteNamesToDelete $poolNamesToDelete
    # delete website directories
    $directoryNames = $siteNamesToDelete | ForEach-Object { (Join-Path -Path $driveLetter -ChildPath $_ ) }
    DeleteDirectories $directoryNames
}
else{
    # Create App Pools, Web Sites, and Applications
    CreateAll
}

Write-Log $(get-date).tostring("yyyy.MM.dd hh:mm:ss")
Write-Log "See the log at $logFile"

