param(
    [Parameter(Mandatory = $true)]
    [string[]]$ResultsPath
)

$ErrorActionPreference = "Stop"

$files = @(
    foreach ($path in $ResultsPath) {
        $resolved = Resolve-Path $path
        foreach ($item in $resolved) {
            if ((Get-Item $item).PSIsContainer) {
                Get-ChildItem $item -Filter "*.trx" -File -Recurse
            }
            else {
                Get-Item $item
            }
        }
    }
) | Sort-Object FullName -Unique

if ($files.Count -eq 0) {
    throw "No TRX result files were found."
}

$total = 0
$passed = 0
$skipped = 0

foreach ($file in $files) {
    [xml]$document = Get-Content $file.FullName
    $namespace = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespace.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")

    $summary = $document.SelectSingleNode("//t:ResultSummary", $namespace)
    if ($null -eq $summary -or $summary.GetAttribute("outcome") -ne "Completed") {
        throw "Test run '$($file.FullName)' did not complete successfully."
    }

    $results = @($document.SelectNodes("//t:UnitTestResult", $namespace))
    if ($results.Count -eq 0) {
        throw "Test run '$($file.FullName)' contains no tests."
    }

    $failedResults = @($results | Where-Object { $_.GetAttribute("outcome") -eq "Failed" })
    if ($failedResults.Count -ne 0) {
        $failedNames = @($failedResults | ForEach-Object { $_.GetAttribute("testName") })
        throw "Test run '$($file.FullName)' has failures: $($failedNames -join ', ')"
    }

    $skippedResults = @($results | Where-Object { $_.GetAttribute("outcome") -eq "NotExecuted" })
    foreach ($result in $skippedResults) {
        $reasonNode = $result.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $namespace)
        if ($null -eq $reasonNode -or [string]::IsNullOrWhiteSpace($reasonNode.InnerText)) {
            throw "Skipped test '$($result.GetAttribute("testName"))' in '$($file.FullName)' has no recorded reason."
        }
    }

    $unexpectedOutcomes = @(
        $results |
            Where-Object {
                $_.GetAttribute("outcome") -notin @("Passed", "NotExecuted")
            } |
            ForEach-Object {
                "$($_.GetAttribute('testName'))=$($_.GetAttribute('outcome'))"
            }
    )
    if ($unexpectedOutcomes.Count -ne 0) {
        throw "Test run '$($file.FullName)' has unexpected outcomes: $($unexpectedOutcomes -join ', ')"
    }

    $total += $results.Count
    $passed += @($results | Where-Object { $_.GetAttribute("outcome") -eq "Passed" }).Count
    $skipped += $skippedResults.Count
}

Write-Host "Verified $($files.Count) TRX file(s): $passed passed, $skipped skipped with reasons, $total total."
