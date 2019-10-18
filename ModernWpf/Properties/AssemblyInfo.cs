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

[assembly: InternalsVisibleTo("ModernWpf.Controls")]

[assembly: XmlnsPrefix("http://www.modernwpf.com", "ui")]
[assembly: XmlnsDefinition("http://www.modernwpf.com", "ModernWpf")]
[assembly: XmlnsDefinition("http://www.modernwpf.com", "ModernWpf.Controls")]
[assembly: XmlnsDefinition("http://www.modernwpf.com", "ModernWpf.Controls.Primitives")]
[assembly: XmlnsDefinition("http://www.modernwpf.com", "ModernWpf.Markup")]
[assembly: XmlnsDefinition("http://www.modernwpf.com", "ModernWpf.Media.Animation")]
