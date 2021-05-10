using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: InternalsVisibleTo("MUXControlsTestApp, publicKey=0024000004800000940000000602000000240000525341310004000001000100f12b3bd55e2dd0b7a4e71e4784150b3f37147c331e9a0837f869018b7e50e8d35090898d64d225e2deda6f26bf7ee675b523c0ede37134cbe20d97e61b25de151e61b529e2987bc9032fcef6983326c56d98c53249881ed638430a0a7d1e5ff11197bab328d822b51f69f7b31520ee03b32b38867f0ff1ca9874ff83f0f086bc")]


[assembly: XmlnsDefinition("http://schemas.modernwpf.com/2019", "ModernWpf.Controls")]
[assembly: XmlnsDefinition("http://schemas.modernwpf.com/2019", "ModernWpf.Controls.Primitives")]
