using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace System.Windows
{
    [MarkupExtensionReturnType(typeof(object))]
    public class BindExtension : MarkupExtension
    {
        public BindExtension()
        {
        }

        public BindExtension(string path)
        {
            if (path != null)
            {
                Path = new PropertyPath(path, null);
            }
        }

        public PropertyPath Path { get; set; }

        [DefaultValue(BindingMode.OneTime)]
        public BindingMode Mode { get; set; } = BindingMode.OneWay;

        [DefaultValue(null)]
        public IValueConverter Converter { get; set; }

        [DefaultValue(UpdateSourceTrigger.PropertyChanged)]
        public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.PropertyChanged;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootObject = (serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject;
            if (rootObject == null)
                throw new InvalidOperationException("Cannot find RootObject");

            PropertyPath effectivePath = Path;
            string elementName = null;

            if (rootObject is FrameworkElement rootElement && rootElement.TemplatedParent == null)
            {
                if (Path != null && Path.PathParameters.Count == 0)
                {
                    string path = Path.Path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        var names = path.Split('.');
                        if (names.Length >= 2)
                        {
                            if (rootElement.GetType().GetProperty(names[0]) == null)
                            {
                                elementName = names[0];

                                if (names.Length == 3 && names[1] == "IsChecked" && names[2] == "Value")
                                {
                                    effectivePath = new PropertyPath(names[1]);
                                }
                                else
                                {
                                    effectivePath = new PropertyPath(path.Substring(names[0].Length + 1));
                                }
                            }
                        }
                    }
                }
            }

            var binding = new Binding
            {
                Path = effectivePath,
                Mode = Mode,
                UpdateSourceTrigger = UpdateSourceTrigger,
                Converter = Converter
            };

            if (elementName != null)
            {
                binding.ElementName = elementName;
            }
            else
            {
                binding.Source = rootObject;
            }

            return binding.ProvideValue(serviceProvider);
        }
    }
}
