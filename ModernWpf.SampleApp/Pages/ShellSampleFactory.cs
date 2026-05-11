using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.SampleApp.Pages
{
    internal static class ShellSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppNotification":
                    return CreateAppNotificationSample();
                case "BadgeNotificationManager":
                    return CreateBadgeNotificationSample();
                case "JumpList":
                    return CreateJumpListSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateAppNotificationSample()
        {
            var panel = CreateSamplePanel("App notifications are represented with a WPF toast-style surface and system notification cue.");
            var title = new TextBox
            {
                Width = 420,
                Text = "ModernWpf Gallery"
            };
            ControlHelper.SetHeader(title, "Title");
            var message = new TextBox
            {
                Width = 420,
                Text = "A sample notification was sent from the WPF Gallery port.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(message, "Message");

            var toastTitle = new TextBlock
            {
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                Foreground = Brushes.White
            };
            var toastMessage = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0),
                Opacity = 0.82,
                Foreground = Brushes.White
            };
            var toast = CreateToastSurface(toastTitle, toastMessage);
            toast.Visibility = Visibility.Collapsed;
            var output = CreateOutput("Ready.");

            var dismissTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            dismissTimer.Tick += delegate
            {
                dismissTimer.Stop();
                toast.Visibility = Visibility.Collapsed;
                output.Text = "Notification expired.";
            };

            var commands = CreateCommandRow();
            var send = CreateButton("Send notification");
            var dismiss = CreateButton("Dismiss");
            send.Click += delegate
            {
                toastTitle.Text = string.IsNullOrWhiteSpace(title.Text) ? "Notification" : title.Text;
                toastMessage.Text = string.IsNullOrWhiteSpace(message.Text) ? "No message." : message.Text;
                toast.Visibility = Visibility.Visible;
                SystemSounds.Asterisk.Play();
                dismissTimer.Stop();
                dismissTimer.Start();
                output.Text = "Notification shown in the sample surface.";
            };
            dismiss.Click += delegate
            {
                dismissTimer.Stop();
                toast.Visibility = Visibility.Collapsed;
                output.Text = "Notification dismissed.";
            };
            commands.Children.Add(send);
            commands.Children.Add(dismiss);

            panel.Children.Add(title);
            panel.Children.Add(message);
            panel.Children.Add(commands);
            panel.Children.Add(toast);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateBadgeNotificationSample()
        {
            var panel = CreateSamplePanel("Badge notifications map to WPF taskbar overlay and progress state for unpackaged desktop apps.");
            var number = new Slider
            {
                Width = 260,
                Minimum = 1,
                Maximum = 99,
                Value = 7,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(number, "Badge number");
            var badgeText = new TextBlock
            {
                Text = "7",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var badge = new Border
            {
                Width = 58,
                Height = 58,
                CornerRadius = new CornerRadius(29),
                Background = CreateBrush("#D13438"),
                Child = badgeText,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var output = CreateOutput("Ready.");
            number.ValueChanged += delegate
            {
                badgeText.Text = ((int)number.Value).ToString();
            };

            var commands = CreateCommandRow();
            var set = CreateButton("Set badge");
            var attention = CreateButton("Set attention");
            var clear = CreateButton("Clear");
            set.Click += delegate
            {
                var window = Window.GetWindow((FrameworkElement)set);
                var info = EnsureTaskbarInfo(window);
                if (info != null)
                {
                    info.Overlay = CreateBitmap(ResourceUri("Assets/Tiles/BadgeLogo.scale-100.png"));
                    info.ProgressState = TaskbarItemProgressState.Normal;
                    info.ProgressValue = number.Value / number.Maximum;
                    output.Text = "Taskbar overlay and progress set.";
                }
                else
                {
                    output.Text = "No owning window was available.";
                }
            };
            attention.Click += delegate
            {
                var window = Window.GetWindow((FrameworkElement)attention);
                var info = EnsureTaskbarInfo(window);
                if (info != null)
                {
                    info.ProgressState = TaskbarItemProgressState.Indeterminate;
                    SystemSounds.Exclamation.Play();
                    output.Text = "Taskbar progress set to attention state.";
                }
                else
                {
                    output.Text = "No owning window was available.";
                }
            };
            clear.Click += delegate
            {
                var window = Window.GetWindow((FrameworkElement)clear);
                var info = EnsureTaskbarInfo(window);
                if (info != null)
                {
                    info.Overlay = null;
                    info.ProgressState = TaskbarItemProgressState.None;
                    info.ProgressValue = 0;
                    output.Text = "Taskbar badge state cleared.";
                }
                else
                {
                    output.Text = "No owning window was available.";
                }
            };
            commands.Children.Add(set);
            commands.Children.Add(attention);
            commands.Children.Add(clear);

            panel.Children.Add(number);
            panel.Children.Add(badge);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateJumpListSample()
        {
            var panel = CreateSamplePanel("JumpList maps to WPF JumpList and JumpTask entries for taskbar quick actions.");
            var preview = new ListBox
            {
                Width = 420,
                Height = 142,
                ItemsSource = new[]
                {
                    "Open Gallery",
                    "Show All Controls",
                    "Open WinUI Gallery reference"
                }
            };
            ControlHelper.SetHeader(preview, "Jump list tasks");
            var output = CreateOutput("Ready.");

            var commands = CreateCommandRow();
            var apply = CreateButton("Apply JumpList");
            var clear = CreateButton("Clear JumpList");
            apply.Click += delegate
            {
                var app = Application.Current;
                var path = GetExecutablePath();
                if (app == null || string.IsNullOrEmpty(path))
                {
                    output.Text = "JumpList could not find the running application path.";
                    return;
                }

                var jumpList = new JumpList
                {
                    ShowFrequentCategory = false,
                    ShowRecentCategory = true
                };
                jumpList.JumpItems.Add(CreateJumpTask("Open Gallery", "Open the ModernWpf Gallery sample app.", path, "--gallery"));
                jumpList.JumpItems.Add(CreateJumpTask("Show All Controls", "Open the all-controls surface.", path, "--all-controls"));
                jumpList.JumpItems.Add(CreateJumpTask("Open WinUI Gallery reference", "Open documentation reference entry point.", path, "--winui-gallery-reference"));
                JumpList.SetJumpList(app, jumpList);
                jumpList.Apply();
                output.Text = "Sample JumpList applied to the running app.";
            };
            clear.Click += delegate
            {
                var app = Application.Current;
                if (app == null)
                {
                    output.Text = "No WPF application was available.";
                    return;
                }

                var jumpList = new JumpList();
                JumpList.SetJumpList(app, jumpList);
                jumpList.Apply();
                output.Text = "Sample JumpList cleared.";
            };
            commands.Children.Add(apply);
            commands.Children.Add(clear);

            panel.Children.Add(preview);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static Border CreateToastSurface(TextBlock title, TextBlock message)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var icon = new Image
            {
                Width = 42,
                Height = 42,
                Source = CreateBitmap(ResourceUri("Assets/ControlImages/AppNotification.png")),
                Margin = new Thickness(0, 0, 14, 0)
            };
            var copy = new StackPanel();
            copy.Children.Add(title);
            copy.Children.Add(message);
            Grid.SetColumn(icon, 0);
            Grid.SetColumn(copy, 1);
            layout.Children.Add(icon);
            layout.Children.Add(copy);

            return new Border
            {
                Width = 460,
                Margin = new Thickness(0, 14, 0, 0),
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#202020"),
                BorderBrush = CreateBrush("#707070"),
                BorderThickness = new Thickness(1),
                Child = layout
            };
        }

        private static TaskbarItemInfo EnsureTaskbarInfo(Window window)
        {
            if (window == null)
            {
                return null;
            }

            if (window.TaskbarItemInfo == null)
            {
                window.TaskbarItemInfo = new TaskbarItemInfo();
            }

            return window.TaskbarItemInfo;
        }

        private static JumpTask CreateJumpTask(string title, string description, string applicationPath, string arguments)
        {
            return new JumpTask
            {
                Title = title,
                Description = description,
                ApplicationPath = applicationPath,
                Arguments = arguments,
                IconResourcePath = applicationPath,
                WorkingDirectory = Path.GetDirectoryName(applicationPath)
            };
        }

        private static string GetExecutablePath()
        {
            try
            {
                var module = Process.GetCurrentProcess().MainModule;
                return module == null ? null : module.FileName;
            }
            catch
            {
                return null;
            }
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static BitmapImage CreateBitmap(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/" + path;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}
