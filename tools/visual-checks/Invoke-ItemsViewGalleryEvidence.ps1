param(
    [string]$RepositoryRoot = "",
    [Parameter(Mandatory = $true)]
    [string]$ExpectedCommit,
    [Parameter(Mandatory = $true)]
    [string]$OutputRoot,
    [ValidateSet("net462", "net8.0-windows7.0", "net10.0-windows7.0")]
    [string[]]$Frameworks = @("net462", "net8.0-windows7.0", "net10.0-windows7.0"),
    [ValidateSet("Light", "Dark", "HighContrast")]
    [string[]]$Themes = @("Light", "Dark")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName System.Drawing

function Write-Json([string]$Path, $Value) {
    $parent = Split-Path -Parent $Path
    if (![string]::IsNullOrWhiteSpace($parent)) {
        [void](New-Item -ItemType Directory -Force -Path $parent)
    }

    $Value | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
}

function Invoke-WinApp([string[]]$Arguments, [switch]$AllowFailure) {
    $lines = & $script:WinApp @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    $text = ($lines | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
    $json = $null
    if (![string]::IsNullOrWhiteSpace($text)) {
        try {
            $json = $text | ConvertFrom-Json -Depth 64
        }
        catch {
            $json = $null
        }
    }

    $script:Actions.Add([ordered]@{
        utc = [DateTime]::UtcNow.ToString("o")
        tfm = $script:CurrentTfm
        theme = $script:CurrentTheme
        arguments = $Arguments
        exitCode = $exitCode
        output = $(if ($text.Length -le 1600 -or $exitCode -ne 0) { $text } else { $null })
    })
    if ($exitCode -ne 0 -and !$AllowFailure) {
        throw "WinApp failed ($exitCode): winapp $($Arguments -join ' ')`n$text"
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Text = $text
        Json = $json
    }
}

function Get-AppWindows([int]$ProcessId) {
    $result = Invoke-WinApp @("ui", "list-windows", "-a", $ProcessId.ToString(), "--json") -AllowFailure
    if ($result.ExitCode -ne 0 -or $null -eq $result.Json) {
        return @()
    }

    return @($result.Json)
}

function Wait-MainWindow([int]$ProcessId, [int]$TimeoutSeconds = 20) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $match = @(Get-AppWindows $ProcessId | Where-Object {
            [int]$_.processId -eq $ProcessId -and
            [string]$_.title -eq "ModernWPF" -and
            [int64]$_.hwnd -ne 0
        } | Select-Object -First 1)
        if ($match.Count -eq 1) {
            return $match[0]
        }

        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out waiting for ModernWPF PID $ProcessId."
}

function Search-Elements([int64]$Hwnd, [string]$Query) {
    $result = Invoke-WinApp @("ui", "search", $Query, "-w", $Hwnd.ToString(), "--json") -AllowFailure
    if ($result.ExitCode -ne 0 -or $null -eq $result.Json) {
        return @()
    }

    return @($result.Json.matches)
}

function Get-ElementById([int64]$Hwnd, [string]$AutomationId) {
    $matches = @(Search-Elements $Hwnd $AutomationId | Where-Object {
        [string]$_.automationId -eq $AutomationId -and
        ![string]::IsNullOrWhiteSpace([string]$_.selector) -and
        ![bool]$_.isOffscreen -and
        [double]$_.width -gt 0 -and
        [double]$_.height -gt 0
    } | Sort-Object y, x)
    if ($matches.Count -eq 0) {
        throw "Automation element '$AutomationId' was not visible."
    }

    return $matches[0]
}

function Get-Nodes($Value) {
    $nodes = [Collections.Generic.List[object]]::new()

    function Visit($Item) {
        if ($null -eq $Item) {
            return
        }

        if ($Item -is [Collections.IEnumerable] -and $Item -isnot [string] -and $Item -isnot [pscustomobject]) {
            foreach ($child in $Item) {
                Visit $child
            }
            return
        }

        if ($Item.PSObject.Properties.Name -contains "type") {
            $nodes.Add($Item)
        }
        foreach ($property in @("windows", "elements", "children", "matches")) {
            if ($Item.PSObject.Properties.Name -contains $property) {
                Visit $Item.$property
            }
        }
    }

    Visit $Value
    return $nodes.ToArray()
}

function Inspect-Element([int64]$Hwnd, [string]$AutomationId, [int]$Depth = 7) {
    $result = Invoke-WinApp @(
        "ui", "inspect", $AutomationId,
        "-w", $Hwnd.ToString(),
        "--json", "--depth", $Depth.ToString())
    return @(Get-Nodes $result.Json)
}

function Get-VisibleNamedNode([object[]]$Nodes, [string]$Name) {
    $matches = @($Nodes | Where-Object {
        [string]$_.name -eq $Name -and
        ![bool]$_.isOffscreen -and
        ![string]::IsNullOrWhiteSpace([string]$_.selector) -and
        [double]$_.width -gt 0 -and
        [double]$_.height -gt 0
    } | Sort-Object y, x)
    if ($matches.Count -eq 0) {
        throw "Visible element '$Name' was not found in the inspected ItemsView."
    }

    return $matches[0]
}

function Get-Value([int64]$Hwnd, [string]$Selector, [switch]$AllowFailure) {
    $result = Invoke-WinApp @("ui", "get-value", $Selector, "-w", $Hwnd.ToString(), "--json") -AllowFailure:$AllowFailure
    if ($result.ExitCode -ne 0 -or $null -eq $result.Json) {
        return $null
    }
    if ($result.Json.PSObject.Properties.Name -notcontains "text") {
        return ""
    }

    return [string]$result.Json.text
}

function Wait-Value([int64]$Hwnd, [string]$Selector, [string]$Expected, [int]$TimeoutSeconds = 12) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    $value = $null
    do {
        $value = Get-Value $Hwnd $Selector -AllowFailure
        if ($value -eq $Expected) {
            return $value
        }

        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out waiting for '$Selector' to equal '$Expected'. Last value '$value'."
}

function Focus-Id([int64]$Hwnd, [string]$AutomationId) {
    [void](Invoke-WinApp @("ui", "focus", $AutomationId, "-w", $Hwnd.ToString(), "--json"))
    Start-Sleep -Milliseconds 300
}

function Click-Element([int]$ProcessId, $Element, [string]$Purpose, [switch]$Double) {
    [void](Invoke-WinApp @("ui", "focus", [string]$Element.selector, "-a", $ProcessId.ToString(), "--json"))
    $arguments = @("ui", "click", [string]$Element.selector, "-a", $ProcessId.ToString())
    if ($Double) {
        $arguments += "--double"
    }
    $arguments += "--json"
    [void](Invoke-WinApp $arguments)
    Start-Sleep -Milliseconds 350
    $script:Actions.Add([ordered]@{
        utc = [DateTime]::UtcNow.ToString("o")
        tfm = $script:CurrentTfm
        theme = $script:CurrentTheme
        action = $(if ($Double) { "physical-double-click" } else { "physical-click" })
        purpose = $Purpose
        selector = [string]$Element.selector
    })
}

function Click-Id([int]$ProcessId, [int64]$Hwnd, [string]$AutomationId, [string]$Purpose) {
    Focus-Id $Hwnd $AutomationId
    Click-Element $ProcessId (Get-ElementById $Hwnd $AutomationId) $Purpose
}

function Send-PhysicalKeys([int64]$Hwnd, [string]$Keys, [string]$Purpose, [string]$Target = "") {
    $arguments = @("ui", "send-keys", $Keys, "-w", $Hwnd.ToString(), "--via", "send-input")
    if (![string]::IsNullOrWhiteSpace($Target)) {
        $arguments += @("--target", $Target)
    }
    $arguments += "--json"
    [void](Invoke-WinApp $arguments)
    Start-Sleep -Milliseconds 400
    $script:Actions.Add([ordered]@{
        utc = [DateTime]::UtcNow.ToString("o")
        tfm = $script:CurrentTfm
        theme = $script:CurrentTheme
        action = "physical-keys"
        purpose = $Purpose
        keys = $Keys
        target = $Target
    })
}

function Get-ImageStats([string]$Path) {
    $bitmap = [Drawing.Bitmap]::new($Path)
    try {
        $colors = [Collections.Generic.HashSet[int]]::new()
        $visible = 0
        $nonBlack = 0
        $stepX = [Math]::Max(1, [int][Math]::Floor($bitmap.Width / 160.0))
        $stepY = [Math]::Max(1, [int][Math]::Floor($bitmap.Height / 120.0))
        for ($y = 0; $y -lt $bitmap.Height; $y += $stepY) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $stepX) {
                $color = $bitmap.GetPixel($x, $y)
                if ($color.A -eq 0) {
                    continue
                }

                $visible++
                [void]$colors.Add($color.ToArgb())
                if ($color.R -gt 8 -or $color.G -gt 8 -or $color.B -gt 8) {
                    $nonBlack++
                }
            }
        }

        $ratio = if ($visible -eq 0) { 0.0 } else { $nonBlack / [double]$visible }
        return [ordered]@{
            width = $bitmap.Width
            height = $bitmap.Height
            visible = $visible
            distinctColors = $colors.Count
            nonBlackRatio = [Math]::Round($ratio, 6)
            nonBlank = $visible -gt 0 -and $colors.Count -gt 8 -and $ratio -gt 0.005
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Capture([int64]$Hwnd, [string]$Name, [string]$Purpose) {
    $path = Join-Path $script:RunRoot ("screenshots\" + $Name)
    [void](New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path))
    [void](Invoke-WinApp @("ui", "screenshot", "-w", $Hwnd.ToString(), "-o", $path, "--json"))
    if (!(Test-Path -LiteralPath $path)) {
        throw "Screenshot was not created: $path"
    }

    $stats = Get-ImageStats $path
    if (!$stats.nonBlank) {
        throw "Screenshot was blank: $path"
    }

    $entry = [ordered]@{
        utc = [DateTime]::UtcNow.ToString("o")
        purpose = $Purpose
        path = [IO.Path]::GetRelativePath($script:OutputRoot, $path)
        provider = "WinApp Windows.Graphics.Capture"
        processId = $script:CurrentProcessId
        hwnd = $Hwnd
        sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        stats = $stats
    }
    $script:Screenshots.Add($entry)
    return $entry
}

function Wait-RouteReady([int64]$Hwnd, [int]$TimeoutSeconds = 20) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $ready = Get-Value $Hwnd "GalleryVisualTestReadyState" -AllowFailure
        if ($ready -eq "Ready:item/ItemsView") {
            return
        }

        $anchor = @(Search-Elements $Hwnd "ItemsView Page" | Where-Object {
            [string]$_.name -eq "ItemsView Page" -and ![bool]$_.isOffscreen
        })
        if ($anchor.Count -gt 0) {
            return
        }

        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out waiting for the ItemsView Gallery route. Last ready value '$ready'."
}

function Start-Gallery([hashtable]$Framework, [string]$Theme) {
    $arguments = @("--route", "item/ItemsView")
    if ($Theme -ne "HighContrast") {
        $arguments += @("--theme", $Theme)
    }

    $process = Start-Process -FilePath $Framework.Exe -ArgumentList $arguments -PassThru
    try {
        $window = Wait-MainWindow $process.Id
        $hwnd = [int64]$window.hwnd
        Wait-RouteReady $hwnd
        $lastException = Get-Value $hwnd "GalleryVisualTestLastException" -AllowFailure
        if (![string]::IsNullOrWhiteSpace($lastException)) {
            throw "Gallery reported an ItemsView exception: $lastException"
        }

        return [pscustomobject]@{
            Process = $process
            Hwnd = $hwnd
            Arguments = $arguments
        }
    }
    catch {
        $process.Refresh()
        if (!$process.HasExited) {
            Stop-Process -Id $process.Id -Force
            [void]$process.WaitForExit(5000)
        }
        throw
    }
}

function Stop-Gallery($Session) {
    if ($null -eq $Session) {
        return
    }

    $process = $Session.Process
    $process.Refresh()
    if (!$process.HasExited) {
        [void](Invoke-WinApp @("ui", "invoke", "CloseButton", "-w", $Session.Hwnd.ToString(), "--json") -AllowFailure)
        [void]$process.WaitForExit(5000)
    }
    $process.Refresh()
    if (!$process.HasExited) {
        Stop-Process -Id $process.Id -Force
        [void]$process.WaitForExit(5000)
    }
}

function Invoke-ItemsViewScenario([hashtable]$Framework, [string]$Theme) {
    $session = $null
    try {
        $session = Start-Gallery $Framework $Theme
        $script:CurrentProcessId = $session.Process.Id
        $initialWindow = @(Get-AppWindows $session.Process.Id | Where-Object {
            [int64]$_.hwnd -eq $session.Hwnd
        })[0]
        [void](Capture $session.Hwnd "01-itemsview-initial.png" "ItemsView initial realization")

        Click-Id $session.Process.Id $session.Hwnd "MaximizeButton" "maximize ItemsView Gallery"
        $maximizedWindow = @(Get-AppWindows $session.Process.Id | Where-Object {
            [int64]$_.hwnd -eq $session.Hwnd
        })[0]
        if ([int]$maximizedWindow.width -le [int]$initialWindow.width) {
            throw "Maximize did not enlarge the ItemsView Gallery window."
        }
        Click-Id $session.Process.Id $session.Hwnd "MaximizeButton" "restore ItemsView Gallery"

        $primaryNodes = Inspect-Element $session.Hwnd "GallerySample_ItemsView_PrimaryItemsView"
        $item2 = Get-VisibleNamedNode $primaryNodes "Item 2"
        Click-Element $session.Process.Id $item2 "double-click Item 2" -Double
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_InvocationResult" "Invoked: Item 2")

        Send-PhysicalKeys $session.Hwnd "down enter" "move to and invoke Item 3" ([string]$item2.selector)
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_InvocationResult" "Invoked: Item 3")
        [void](Capture $session.Hwnd "02-itemsview-pointer-keyboard.png" "ItemsView pointer and keyboard invocation")

        Focus-Id $session.Hwnd "GallerySample_ItemsView_LayoutSelector"
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_LayoutSelector" "open ItemsView layout selector"
        Send-PhysicalKeys $session.Hwnd "end enter" "select LinedFlowLayout"
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_LayoutSelector" "LinedFlowLayout")
        [void](Capture $session.Hwnd "03-itemsview-lined-flow.png" "ItemsView with LinedFlowLayout")
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_AddItem" "add an ItemsView item"
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_RemoveItem" "remove the last ItemsView item"

        Focus-Id $session.Hwnd "GallerySample_ItemsView_SelectionMode"
        [void](Capture $session.Hwnd "04-itemsview-selection-options.png" "ItemsView selection and invocation options")
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_ClearSelection" "clear ItemsView selection"
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_SelectionStatus" "No items selected.")

        $selectionNodes = Inspect-Element $session.Hwnd "GallerySample_ItemsView_SelectionItemsView"
        $selectionItem1 = Get-VisibleNamedNode $selectionNodes "Item 1"
        Send-PhysicalKeys $session.Hwnd "ctrl+a" "select every ItemsView item" ([string]$selectionItem1.selector)
        $selectedText = "Selected: " + ((1..18 | ForEach-Object { "Item $_" }) -join ", ")
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_SelectionStatus" $selectedText)
        [void](Capture $session.Hwnd "05-itemsview-select-all.png" "ItemsView Ctrl+A multi-selection")

        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_InvertSelection" "invert ItemsView selection"
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_SelectionStatus" "No items selected.")
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_SelectAll" "select all ItemsView items"
        [void](Wait-Value $session.Hwnd "GallerySample_ItemsView_SelectionStatus" $selectedText)
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_InvocationEnabled" "disable ItemsView invocation"
        Click-Id $session.Process.Id $session.Hwnd "GallerySample_ItemsView_InvocationEnabled" "restore ItemsView invocation"
        [void](Capture $session.Hwnd "06-itemsview-final.png" "ItemsView final selected state")

        return [ordered]@{
            status = "Passed"
            processId = $session.Process.Id
            hwnd = $session.Hwnd
            pointerInvocation = "Invoked: Item 2"
            keyboardInvocation = "Invoked: Item 3"
            layout = "LinedFlowLayout"
            ctrlASelectedItems = 18
            invertClearedSelection = $true
            selectAllSelectedItems = 18
            maximizeRestore = $true
        }
    }
    finally {
        Stop-Gallery $session
    }
}

$script:StartedUtc = [DateTime]::UtcNow
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
} else {
    $RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
}
$script:OutputRoot = [IO.Path]::GetFullPath($OutputRoot, $RepositoryRoot)
if (Test-Path -LiteralPath $script:OutputRoot) {
    throw "Evidence output already exists: $($script:OutputRoot)"
}

$actualCommit = (& git -C $RepositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne $ExpectedCommit) {
    throw "Expected HEAD $ExpectedCommit but found $actualCommit."
}
$status = @(& git -C $RepositoryRoot status --porcelain)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) {
    throw "The worktree is not clean."
}

$osHighContrast = [System.Windows.SystemParameters]::HighContrast
if (($Themes -contains "HighContrast") -and $Themes.Count -ne 1) {
    throw "Run HighContrast separately from Light/Dark."
}
if (($Themes -contains "HighContrast") -ne $osHighContrast) {
    throw "Requested themes do not match real OS High Contrast state ($osHighContrast)."
}

$script:WinApp = (Get-Command winapp -ErrorAction Stop).Source
$winAppVersion = ((& $script:WinApp --version 2>&1) | Out-String).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Unable to determine the WinApp version."
}
$environment = [ordered]@{
    osVersion = [Environment]::OSVersion.Version.ToString()
    windowsBuild = [Environment]::OSVersion.Version.Build
    processArchitecture = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    sessionId = [Diagnostics.Process]::GetCurrentProcess().SessionId
    sessionName = [Environment]::GetEnvironmentVariable("SESSIONNAME")
    rdpSession = [Environment]::GetEnvironmentVariable("SESSIONNAME") -like "RDP-*"
    primaryScreenWidth = [System.Windows.SystemParameters]::PrimaryScreenWidth
    primaryScreenHeight = [System.Windows.SystemParameters]::PrimaryScreenHeight
    winAppVersion = $winAppVersion
}
$frameworkMap = @{
    "net462" = Join-Path $RepositoryRoot "ModernWpf.Gallery\bin\Release\net462\ModernWpf.Gallery.exe"
    "net8.0-windows7.0" = Join-Path $RepositoryRoot "ModernWpf.Gallery\bin\Release\net8.0-windows7.0\ModernWpf.Gallery.exe"
    "net10.0-windows7.0" = Join-Path $RepositoryRoot "ModernWpf.Gallery\bin\Release\net10.0-windows7.0\ModernWpf.Gallery.exe"
}
$frameworkRecords = @{}
foreach ($tfm in $Frameworks) {
    $exe = $frameworkMap[$tfm]
    if (!(Test-Path -LiteralPath $exe)) {
        throw "Gallery executable not found: $exe"
    }

    $versionInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
    $frameworkRecords[$tfm] = @{
        Tfm = $tfm
        Exe = $exe
        Sha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
        FileVersion = $versionInfo.FileVersion
        ProductVersion = $versionInfo.ProductVersion
    }
}

[void](New-Item -ItemType Directory -Force -Path $script:OutputRoot)
$script:Actions = [Collections.Generic.List[object]]::new()
$runs = [Collections.Generic.List[object]]::new()
$failures = [Collections.Generic.List[object]]::new()
foreach ($tfm in $Frameworks) {
    foreach ($theme in $Themes) {
        $script:CurrentTfm = $tfm
        $script:CurrentTheme = $theme
        $script:RunRoot = Join-Path $script:OutputRoot "$tfm\$theme"
        [void](New-Item -ItemType Directory -Force -Path $script:RunRoot)
        $script:Screenshots = [Collections.Generic.List[object]]::new()
        $started = [DateTime]::UtcNow
        try {
            $scenario = Invoke-ItemsViewScenario $frameworkRecords[$tfm] $theme
            $run = [ordered]@{
                tfm = $tfm
                theme = $theme
                status = "Passed"
                osHighContrast = $osHighContrast
                executable = [IO.Path]::GetRelativePath($RepositoryRoot, $frameworkRecords[$tfm].Exe)
                executableSha256 = $frameworkRecords[$tfm].Sha256
                fileVersion = $frameworkRecords[$tfm].FileVersion
                productVersion = $frameworkRecords[$tfm].ProductVersion
                resourceEntry = "ThemeResources + FluentControlsResources"
                useCompactResources = $false
                startedUtc = $started.ToString("o")
                completedUtc = [DateTime]::UtcNow.ToString("o")
                itemsView = $scenario
                screenshots = $script:Screenshots
            }
            $runs.Add($run)
            Write-Json (Join-Path $script:RunRoot "run.json") $run
        }
        catch {
            $failure = [ordered]@{
                tfm = $tfm
                theme = $theme
                message = $_.Exception.Message
            }
            $failures.Add($failure)
            $failedRun = [ordered]@{
                tfm = $tfm
                theme = $theme
                status = "Failed"
                osHighContrast = $osHighContrast
                executable = [IO.Path]::GetRelativePath($RepositoryRoot, $frameworkRecords[$tfm].Exe)
                executableSha256 = $frameworkRecords[$tfm].Sha256
                startedUtc = $started.ToString("o")
                completedUtc = [DateTime]::UtcNow.ToString("o")
                failure = $_.Exception.Message
                screenshots = $script:Screenshots
            }
            $runs.Add($failedRun)
            Write-Json (Join-Path $script:RunRoot "run.json") $failedRun
        }
    }
}

$manifest = [ordered]@{
    schema = "modernwpf-itemsview-gallery-evidence-v1"
    version = "1.0.0-preview.7"
    expectedCommit = $ExpectedCommit
    actualCommit = $actualCommit
    startedUtc = $script:StartedUtc.ToString("o")
    completedUtc = [DateTime]::UtcNow.ToString("o")
    provider = "WinApp Windows.Graphics.Capture + physical pointer + SendInput keyboard"
    environment = $environment
    resourceEntry = "ThemeResources + FluentControlsResources"
    useCompactResources = $false
    osHighContrast = $osHighContrast
    frameworks = $Frameworks
    themes = $Themes
    runs = $runs
    failures = $failures
    gateDecision = $(if ($failures.Count -eq 0 -and $runs.Count -eq ($Frameworks.Count * $Themes.Count)) {
        "Passed"
    } else {
        "Failed"
    })
}
Write-Json (Join-Path $script:OutputRoot "manifest.json") $manifest
Write-Json (Join-Path $script:OutputRoot "actions.json") $script:Actions

$reportLines = [Collections.Generic.List[string]]::new()
$reportLines.Add("# ModernWPF Preview 7 ItemsView Gallery evidence")
$reportLines.Add("")
$reportLines.Add("- Commit: ``$actualCommit``")
$reportLines.Add("- Windows build: ``$($environment.osVersion)``")
$reportLines.Add("- Session: ``$($environment.sessionName)`` (ID $($environment.sessionId), RDP=$($environment.rdpSession))")
$reportLines.Add("- Capture/input: ``$($manifest.provider)``")
$reportLines.Add("- Resource entry: ``$($manifest.resourceEntry)``; compact resources: ``$($manifest.useCompactResources)``")
$reportLines.Add("")
$reportLines.Add("| Framework | Theme | Result | Screenshots |")
$reportLines.Add("| --- | --- | --- | ---: |")
foreach ($run in $runs) {
    $reportLines.Add("| $($run.tfm) | $($run.theme) | $($run.status) | $(@($run.screenshots).Count) |")
}
$reportLines.Add("")
$reportLines.Add("Gate decision: **$($manifest.gateDecision)**")
if ($failures.Count -ne 0) {
    $reportLines.Add("")
    $reportLines.Add("## Failures")
    $reportLines.Add("")
    foreach ($failure in $failures) {
        $reportLines.Add("- $($failure.tfm) / $($failure.theme): $($failure.message)")
    }
}
$reportLines | Set-Content -LiteralPath (Join-Path $script:OutputRoot "report.md") -Encoding utf8NoBOM

$files = @(Get-ChildItem -LiteralPath $script:OutputRoot -Recurse -File | Where-Object {
    $_.Name -notin @("files.json", "SHA256SUMS")
} | Sort-Object FullName | ForEach-Object {
    [ordered]@{
        path = [IO.Path]::GetRelativePath($script:OutputRoot, $_.FullName).Replace("\", "/")
        length = $_.Length
        sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
})
Write-Json (Join-Path $script:OutputRoot "files.json") $files
$sumLines = @(Get-ChildItem -LiteralPath $script:OutputRoot -Recurse -File | Where-Object {
    $_.Name -ne "SHA256SUMS"
} | Sort-Object FullName | ForEach-Object {
    $relativePath = [IO.Path]::GetRelativePath($script:OutputRoot, $_.FullName).Replace("\", "/")
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $relativePath"
})
$sumLines | Set-Content -LiteralPath (Join-Path $script:OutputRoot "SHA256SUMS") -Encoding ascii

Write-Output "EvidenceRoot=$($script:OutputRoot)"
Write-Output "GateDecision=$($manifest.gateDecision)"
Write-Output "Runs=$($runs.Count)"
Write-Output "Failures=$($failures.Count)"
if ($failures.Count -ne 0) {
    exit 1
}
