Set-Location $PSScriptRoot
[IO.Directory]::SetCurrentDirectory($PSScriptRoot)

Add-Type -AssemblyName PresentationFramework
[Reflection.Assembly]::LoadFrom('bin\Debug\net452\System.ValueTuple.dll') | Out-Null
[Reflection.Assembly]::LoadFrom('bin\Debug\net452\ModernWpf.dll') | Out-Null
[Reflection.Assembly]::LoadFrom('bin\Debug\net452\ModernWpf.Controls.dll') | Out-Null

$xaml = Get-Content "MainWindow.xaml" -Raw
$window = [Windows.Markup.XamlReader]::Parse($xaml)
$window.ShowDialog()