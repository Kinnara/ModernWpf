# PowerShell sample

This sample loads ModernWpf from a PowerShell-hosted WPF window. Run
`.\run.ps1` from Windows PowerShell 5.1 or from PowerShell 7 on .NET 8 or
later. The script builds the sample first and selects the compatible supported
target automatically:

- Windows PowerShell uses `net462`.
- PowerShell 7 on .NET 8 or 9 uses `net8.0-windows7.0`.
- PowerShell 7 on .NET 10 or later uses `net10.0-windows7.0`.

The WPF thread must use the STA apartment state. If the current host is not
STA, restart it with the `-STA` option.

## Runner options

```powershell
.\run.ps1 -Configuration Release
.\run.ps1 -NoBuild
.\run.ps1 -ValidateOnly
```

`-NoBuild` reuses the selected configuration's existing output.
`-ValidateOnly` loads the assemblies and parses the window without displaying
it, which is useful for automated checks.

## Finding named controls

For one control, give it an `x:Name` in XAML and call `FindName`:

```xaml
<ui:NavigationView x:Name="Navigation" />
```

```powershell
$navigation = $window.FindName('Navigation')
```

To collect every `x:Name`, query the attribute in the XAML namespace and read
the attribute value. `XmlElement.Name` is the element name (for example,
`NavigationView`), not the value of `x:Name`.

```powershell
[xml] $document = Get-Content -LiteralPath '.\MainWindow.xaml' -Raw
$xamlNamespace = 'http://schemas.microsoft.com/winfx/2006/xaml'
$namespaceManager = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
$namespaceManager.AddNamespace('x', $xamlNamespace)

$document.SelectNodes('//*[@x:Name]', $namespaceManager) | ForEach-Object {
    $name = $_.GetAttribute('Name', $xamlNamespace)
    Set-Variable -Scope Script -Name $name -Value $window.FindName($name)
}
```

Keep all WPF control access on the window's dispatcher. Background runspaces
must marshal UI work through `$window.Dispatcher`.
