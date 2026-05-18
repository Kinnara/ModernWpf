using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ContentDialogs;

[TestClass]
public class ContentDialogApiTests
{
    [TestMethod]
    public void VerifyContentDialogDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var dialog = new ContentDialog();

            Assert.IsNull(dialog.Title);
            Assert.IsNull(dialog.TitleTemplate);
            Assert.AreEqual(string.Empty, dialog.PrimaryButtonText);
            Assert.IsNull(dialog.PrimaryButtonCommand);
            Assert.IsNull(dialog.PrimaryButtonCommandParameter);
            Assert.IsNull(dialog.PrimaryButtonStyle);
            Assert.IsTrue(dialog.IsPrimaryButtonEnabled);
            Assert.AreEqual(string.Empty, dialog.SecondaryButtonText);
            Assert.IsNull(dialog.SecondaryButtonCommand);
            Assert.IsNull(dialog.SecondaryButtonCommandParameter);
            Assert.IsNull(dialog.SecondaryButtonStyle);
            Assert.IsTrue(dialog.IsSecondaryButtonEnabled);
            Assert.AreEqual(string.Empty, dialog.CloseButtonText);
            Assert.IsNull(dialog.CloseButtonCommand);
            Assert.IsNull(dialog.CloseButtonCommandParameter);
            Assert.IsNull(dialog.CloseButtonStyle);
            Assert.AreEqual(ContentDialogButton.None, dialog.DefaultButton);
            Assert.IsFalse(dialog.FullSizeDesired);
            Assert.AreEqual(new CornerRadius(), dialog.CornerRadius);
            Assert.IsFalse(dialog.IsShadowEnabled);
            Assert.IsNull(dialog.Owner);
        });
    }

    [TestMethod]
    public void VerifyContentDialogPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var owner = new Window();
            try
            {
                var titleTemplate = new DataTemplate();
                var primaryCommand = new RecordingCommand();
                var secondaryCommand = new RecordingCommand();
                var closeCommand = new RecordingCommand();
                var primaryStyle = new Style(typeof(Button));
                var secondaryStyle = new Style(typeof(Button));
                var closeStyle = new Style(typeof(Button));
                var dialog = new ContentDialog
                {
                    Title = "Delete file?",
                    TitleTemplate = titleTemplate,
                    Content = "This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    PrimaryButtonCommand = primaryCommand,
                    PrimaryButtonCommandParameter = "primary",
                    PrimaryButtonStyle = primaryStyle,
                    IsPrimaryButtonEnabled = false,
                    SecondaryButtonText = "Archive",
                    SecondaryButtonCommand = secondaryCommand,
                    SecondaryButtonCommandParameter = "secondary",
                    SecondaryButtonStyle = secondaryStyle,
                    IsSecondaryButtonEnabled = false,
                    CloseButtonText = "Cancel",
                    CloseButtonCommand = closeCommand,
                    CloseButtonCommandParameter = "close",
                    CloseButtonStyle = closeStyle,
                    DefaultButton = ContentDialogButton.Close,
                    FullSizeDesired = true,
                    CornerRadius = new CornerRadius(6),
                    IsShadowEnabled = true,
                    Owner = owner
                };

                Assert.AreEqual("Delete file?", dialog.Title);
                Assert.AreSame(titleTemplate, dialog.TitleTemplate);
                Assert.AreEqual("This action cannot be undone.", dialog.Content);
                Assert.AreEqual("Delete", dialog.PrimaryButtonText);
                Assert.AreSame(primaryCommand, dialog.PrimaryButtonCommand);
                Assert.AreEqual("primary", dialog.PrimaryButtonCommandParameter);
                Assert.AreSame(primaryStyle, dialog.PrimaryButtonStyle);
                Assert.IsFalse(dialog.IsPrimaryButtonEnabled);
                Assert.AreEqual("Archive", dialog.SecondaryButtonText);
                Assert.AreSame(secondaryCommand, dialog.SecondaryButtonCommand);
                Assert.AreEqual("secondary", dialog.SecondaryButtonCommandParameter);
                Assert.AreSame(secondaryStyle, dialog.SecondaryButtonStyle);
                Assert.IsFalse(dialog.IsSecondaryButtonEnabled);
                Assert.AreEqual("Cancel", dialog.CloseButtonText);
                Assert.AreSame(closeCommand, dialog.CloseButtonCommand);
                Assert.AreEqual("close", dialog.CloseButtonCommandParameter);
                Assert.AreSame(closeStyle, dialog.CloseButtonStyle);
                Assert.AreEqual(ContentDialogButton.Close, dialog.DefaultButton);
                Assert.IsTrue(dialog.FullSizeDesired);
                Assert.AreEqual(new CornerRadius(6), dialog.CornerRadius);
                Assert.IsTrue(dialog.IsShadowEnabled);
                Assert.AreSame(owner, dialog.Owner);
            }
            finally
            {
                owner.Close();
            }
        });
    }

    [TestMethod]
    public void TemplateAppliesButtonTextEnablementAndWinUI2ResourceConstants()
    {
        WpfTestHost.Run(() =>
        {
            var dialog = CreateDialog();

            using var host = CreateInPlaceHost(dialog);
            var primaryButton = GetTemplateButton(dialog, "PrimaryButton");
            var secondaryButton = GetTemplateButton(dialog, "SecondaryButton");
            var closeButton = GetTemplateButton(dialog, "CloseButton");

            Assert.AreEqual("Primary", primaryButton.Content);
            Assert.AreEqual("Secondary", secondaryButton.Content);
            Assert.AreEqual("Close", closeButton.Content);
            Assert.IsTrue(primaryButton.IsEnabled);
            Assert.IsTrue(secondaryButton.IsEnabled);

            dialog.IsPrimaryButtonEnabled = false;
            dialog.IsSecondaryButtonEnabled = false;
            host.UpdateLayout();

            Assert.IsFalse(primaryButton.IsEnabled);
            Assert.IsFalse(secondaryButton.IsEnabled);
            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/ContentDialog/ContentDialog.xaml", UriKind.Relative)
            };
            AssertResource(resources, "ContentDialogMinWidth", 320d);
            AssertResource(resources, "ContentDialogMaxWidth", 548d);
            AssertResource(resources, "ContentDialogMinHeight", 184d);
            AssertResource(resources, "ContentDialogMaxHeight", 756d);
            AssertResource(resources, "ContentDialogButtonSpacing", new GridLength(8));
            AssertResource(resources, "ContentDialogTitleMargin", new Thickness(0, 0, 0, 12));
            AssertResource(resources, "ContentDialogPadding", new Thickness(24));
            AssertResource(resources, "ContentDialogSeparatorThickness", new Thickness(0, 0, 0, 1));
        });
    }

    [TestMethod]
    public void TemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var dialog = CreateDialog();
            using var host = CreateInPlaceHost(dialog);
            host.UpdateLayout();

            var root = GetTemplateChild<FrameworkElement>(dialog, "Container");

            AssertStateSetter(root, "DialogShowingStates", "DialogShowing", "PrimaryButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowing", "SecondaryButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowing", "CloseButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowing", "LayoutRoot.Visibility");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowing", "BackgroundElement.(KeyboardNavigation.TabNavigation)");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowingWithoutSmokeLayer", "PrimaryButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowingWithoutSmokeLayer", "SecondaryButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowingWithoutSmokeLayer", "CloseButton.IsTabStop");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowingWithoutSmokeLayer", "LayoutRoot.Visibility");
            AssertStateSetter(root, "DialogShowingStates", "DialogShowingWithoutSmokeLayer", "LayoutRoot.Background");
            AssertStateSetter(root, "DialogSizingStates", "FullDialogSizing", "BackgroundElement.VerticalAlignment");
            AssertStateSetter(root, "ButtonsVisibilityStates", "AllVisible", "FirstSpacer.Width");
            AssertStateSetter(root, "ButtonsVisibilityStates", "AllVisible", "SecondaryColumn.Width");
            AssertStateSetter(root, "ButtonsVisibilityStates", "AllVisible", "SecondaryButton.(Grid.Column)");
            AssertStateSetter(root, "ButtonsVisibilityStates", "NoneVisible", "CommandSpace.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryVisible", "PrimaryButton.(Grid.Column)");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryVisible", "SecondaryButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryVisible", "CloseButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "SecondaryVisible", "SecondaryButton.(Grid.Column)");
            AssertStateSetter(root, "ButtonsVisibilityStates", "SecondaryVisible", "PrimaryButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "SecondaryVisible", "CloseButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "CloseVisible", "PrimaryButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "CloseVisible", "SecondaryButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryAndSecondaryVisible", "SecondaryButton.(Grid.Column)");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryAndSecondaryVisible", "CloseButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "PrimaryAndCloseVisible", "SecondaryButton.Visibility");
            AssertStateSetter(root, "ButtonsVisibilityStates", "SecondaryAndCloseVisible", "PrimaryButton.Visibility");
            AssertStateSetter(root, "DefaultButtonStates", "PrimaryAsDefaultButton", "PrimaryButton.Style");
            AssertStateSetter(root, "DefaultButtonStates", "SecondaryAsDefaultButton", "SecondaryButton.Style");
            AssertStateSetter(root, "DefaultButtonStates", "CloseAsDefaultButton", "CloseButton.Style");
            AssertStateSetter(root, "DialogBorderStates", "AccentColorBorder", "BackgroundElement.BorderBrush");
        });
    }

    [TestMethod]
    public void TemplateUsesSourceContentDialogShadowDepth()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var dialog = CreateDialog();
            using var host = CreateInPlaceHost(dialog);
            host.UpdateLayout();

            var shadow = GetTemplateChild<ThemeShadowChrome>(dialog, "Shdw");

            Assert.AreEqual(128.0, shadow.Depth);
            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, shadow.WindowedPopupInsetMode);
            Assert.AreEqual(new Thickness(64, 32, 64, 96), shadow.ShadowPadding);
        });
    }

    [TestMethod]
    public void DefaultButtonVisualStatesApplyAccentStyle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var dialog = CreateDialog();
            using var host = CreateInPlaceHost(dialog);
            host.UpdateLayout();

            var primaryButton = GetTemplateButton(dialog, "PrimaryButton");
            var secondaryButton = GetTemplateButton(dialog, "SecondaryButton");
            var normalPrimaryStyle = primaryButton.Style;
            var accentButtonStyle = dialog.TryFindResource("AccentButtonStyle") as Style
                ?? throw new AssertFailedException("Expected AccentButtonStyle resource.");

            dialog.DefaultButton = ContentDialogButton.Primary;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreSame(accentButtonStyle, primaryButton.Style);

            dialog.DefaultButton = ContentDialogButton.Secondary;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreSame(normalPrimaryStyle, primaryButton.Style);
            Assert.AreSame(accentButtonStyle, secondaryButton.Style);
        });
    }

    [TestMethod]
    public void DefaultButtonVisualStateFollowsSourceCommandAreaFocus()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var dialog = CreateDialog();
            using var host = CreateInPlaceHost(dialog);
            var showTask = ShowInPlace(dialog);

            var primaryButton = GetTemplateButton(dialog, "PrimaryButton");
            var secondaryButton = GetTemplateButton(dialog, "SecondaryButton");
            var normalPrimaryStyle = primaryButton.Style;
            var normalSecondaryStyle = secondaryButton.Style;
            var accentButtonStyle = dialog.TryFindResource("AccentButtonStyle") as Style
                ?? throw new AssertFailedException("Expected AccentButtonStyle resource.");

            dialog.DefaultButton = ContentDialogButton.Primary;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreSame(accentButtonStyle, primaryButton.Style);

            Assert.IsTrue(secondaryButton.Focus());
            WpfTestHost.DoEvents();

            Assert.AreSame(normalPrimaryStyle, primaryButton.Style);
            Assert.AreSame(normalSecondaryStyle, secondaryButton.Style);

            Assert.IsTrue(primaryButton.Focus());
            WpfTestHost.DoEvents();

            Assert.AreSame(accentButtonStyle, primaryButton.Style);

            dialog.Hide();
            Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
        });
    }

    [TestMethod]
    public void InPlaceShowAndHideRaiseEventsAndReturnNone()
    {
        WpfTestHost.Run(() =>
        {
            var dialog = CreateDialog();
            using var host = CreateInPlaceHost(dialog);
            var openedCount = 0;
            var closingCount = 0;
            var closedCount = 0;
            var closedResult = ContentDialogResult.Primary;

            dialog.Opened += (_, _) => openedCount++;
            dialog.Closing += (_, args) =>
            {
                closingCount++;
                Assert.AreEqual(ContentDialogResult.None, args.Result);
            };
            dialog.Closed += (_, args) =>
            {
                closedCount++;
                closedResult = args.Result;
            };

            var showTask = ShowInPlace(dialog);
            PumpUntil(() => openedCount == 1);
            Assert.IsFalse(showTask.IsCompleted);

            dialog.Hide();
            Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(1, closingCount);
            Assert.AreEqual(1, closedCount);
            Assert.AreEqual(ContentDialogResult.None, closedResult);
        });
    }

    [TestMethod]
    public void ButtonClicksReturnExpectedResultsAndExecuteCommands()
    {
        WpfTestHost.Run(() =>
        {
            var primaryCommand = new RecordingCommand();
            Assert.AreEqual(
                ContentDialogResult.Primary,
                ClickDialogButton(ContentDialogButton.Primary, dialog =>
                {
                    dialog.PrimaryButtonCommand = primaryCommand;
                    dialog.PrimaryButtonCommandParameter = "primary";
                }));
            Assert.AreEqual(1, primaryCommand.ExecuteCount);
            Assert.AreEqual("primary", primaryCommand.LastParameter);

            var secondaryCommand = new RecordingCommand();
            Assert.AreEqual(
                ContentDialogResult.Secondary,
                ClickDialogButton(ContentDialogButton.Secondary, dialog =>
                {
                    dialog.SecondaryButtonCommand = secondaryCommand;
                    dialog.SecondaryButtonCommandParameter = "secondary";
                }));
            Assert.AreEqual(1, secondaryCommand.ExecuteCount);
            Assert.AreEqual("secondary", secondaryCommand.LastParameter);

            var closeCommand = new RecordingCommand();
            Assert.AreEqual(
                ContentDialogResult.None,
                ClickDialogButton(ContentDialogButton.Close, dialog =>
                {
                    dialog.CloseButtonCommand = closeCommand;
                    dialog.CloseButtonCommandParameter = "close";
                }));
            Assert.AreEqual(1, closeCommand.ExecuteCount);
            Assert.AreEqual("close", closeCommand.LastParameter);
        });
    }

    [TestMethod]
    public void EnterOnlyInvokesExplicitDefaultButtonLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var dialog = CreateDialog();
            dialog.DefaultButton = ContentDialogButton.None;

            var primaryClickCount = 0;
            dialog.PrimaryButtonClick += (_, _) => primaryClickCount++;

            using (CreateInPlaceHost(dialog))
            {
                var showTask = ShowInPlace(dialog);

                PressKey(dialog, Key.Enter);
                Assert.IsFalse(showTask.IsCompleted);
                Assert.AreEqual(0, primaryClickCount);

                dialog.DefaultButton = ContentDialogButton.Primary;
                PressKey(dialog, Key.Enter);

                Assert.AreEqual(ContentDialogResult.Primary, WaitForResult(showTask));
                Assert.AreEqual(1, primaryClickCount);
            }
        });
    }

    [TestMethod]
    public void EscapeUsesSourceCloseActionAndCloseButtonClickCancellation()
    {
        WpfTestHost.Run(() =>
        {
            var closeCommand = new RecordingCommand();
            var dialog = CreateDialog();
            dialog.CloseButtonCommand = closeCommand;
            dialog.CloseButtonCommandParameter = "close";

            var closeClickCount = 0;
            var cancelClose = true;
            dialog.CloseButtonClick += (_, args) =>
            {
                closeClickCount++;
                args.Cancel = cancelClose;
            };

            using (CreateInPlaceHost(dialog))
            {
                var showTask = ShowInPlace(dialog);

                PressKey(dialog, Key.Escape);
                Assert.IsFalse(showTask.IsCompleted);
                Assert.AreEqual(1, closeClickCount);
                Assert.AreEqual(0, closeCommand.ExecuteCount);

                cancelClose = false;
                PressKey(dialog, Key.Escape);

                Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
                Assert.AreEqual(2, closeClickCount);
                Assert.AreEqual(1, closeCommand.ExecuteCount);
                Assert.AreEqual("close", closeCommand.LastParameter);
            }
        });
    }

    [TestMethod]
    public void ButtonClickCanBeCanceledAndDeferred()
    {
        WpfTestHost.Run(() =>
        {
            var cancelCommand = new RecordingCommand();
            var cancelDialog = CreateDialog();
            cancelDialog.PrimaryButtonCommand = cancelCommand;
            cancelDialog.PrimaryButtonClick += (_, args) => args.Cancel = true;

            using (CreateInPlaceHost(cancelDialog))
            {
                var showTask = ShowInPlace(cancelDialog);
                ClickButton(cancelDialog, ContentDialogButton.Primary);
                WpfTestHost.DoEvents();

                Assert.IsFalse(showTask.IsCompleted);
                Assert.AreEqual(0, cancelCommand.ExecuteCount);

                cancelDialog.Hide();
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
            }

            ContentDialogButtonClickDeferral? deferral = null;
            var deferDialog = CreateDialog();
            deferDialog.SecondaryButtonClick += (_, args) => deferral = args.GetDeferral();

            using (CreateInPlaceHost(deferDialog))
            {
                var showTask = ShowInPlace(deferDialog);
                ClickButton(deferDialog, ContentDialogButton.Secondary);
                WpfTestHost.DoEvents();

                Assert.IsFalse(showTask.IsCompleted);
                Assert.IsNotNull(deferral);

                deferral!.Complete();
                Assert.AreEqual(ContentDialogResult.Secondary, WaitForResult(showTask));
            }
        });
    }

    [TestMethod]
    public void ClosingCanBeCanceledAndDeferred()
    {
        WpfTestHost.Run(() =>
        {
            var cancelDialog = CreateDialog();
            var cancelClosing = true;
            cancelDialog.Closing += (_, args) =>
            {
                if (cancelClosing)
                {
                    args.Cancel = true;
                }
            };

            using (CreateInPlaceHost(cancelDialog))
            {
                var showTask = ShowInPlace(cancelDialog);
                cancelDialog.Hide();
                WpfTestHost.DoEvents();

                Assert.IsFalse(showTask.IsCompleted);

                cancelClosing = false;
                cancelDialog.Hide();
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
            }

            ContentDialogClosingDeferral? deferral = null;
            var deferDialog = CreateDialog();
            deferDialog.Closing += (_, args) => deferral = args.GetDeferral();

            using (CreateInPlaceHost(deferDialog))
            {
                var showTask = ShowInPlace(deferDialog);
                deferDialog.Hide();
                WpfTestHost.DoEvents();

                Assert.IsFalse(showTask.IsCompleted);
                Assert.IsNotNull(deferral);

                deferral!.Complete();
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(showTask));
            }
        });
    }

    [TestMethod]
    public void InPlaceDialogOpenRulesMatchSiblingBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var siblingRoot = new Grid();
            var firstSibling = CreateDialog();
            var secondSibling = CreateDialog();
            siblingRoot.Children.Add(firstSibling);
            siblingRoot.Children.Add(secondSibling);

            using (new TestWindowHost(siblingRoot, width: 640, height: 480))
            {
                var firstTask = ShowInPlace(firstSibling);
                Assert.ThrowsException<InvalidOperationException>(() => secondSibling.ShowAsync(ContentDialogPlacement.InPlace));

                firstSibling.Hide();
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(firstTask));
            }

            var nonSiblingRoot = new StackPanel();
            var firstHost = new Grid();
            var secondHost = new Grid();
            var firstNonSibling = CreateDialog();
            var secondNonSibling = CreateDialog();
            firstHost.Children.Add(firstNonSibling);
            secondHost.Children.Add(secondNonSibling);
            nonSiblingRoot.Children.Add(firstHost);
            nonSiblingRoot.Children.Add(secondHost);

            using (new TestWindowHost(nonSiblingRoot, width: 640, height: 640))
            {
                var firstTask = ShowInPlace(firstNonSibling);
                var secondTask = ShowInPlace(secondNonSibling);

                Assert.IsFalse(firstTask.IsCompleted);
                Assert.IsFalse(secondTask.IsCompleted);

                firstNonSibling.Hide();
                secondNonSibling.Hide();
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(firstTask));
                Assert.AreEqual(ContentDialogResult.None, WaitForResult(secondTask));
            }
        });
    }

    private static ContentDialog CreateDialog()
    {
        return new ContentDialog
        {
            Title = "ContentDialog Title",
            Content = "ContentDialog Content",
            PrimaryButtonText = "Primary",
            SecondaryButtonText = "Secondary",
            CloseButtonText = "Close"
        };
    }

    private static TestWindowHost CreateInPlaceHost(ContentDialog dialog)
    {
        var root = new Grid();
        root.Children.Add(dialog);
        return new TestWindowHost(root, width: 640, height: 480);
    }

    private static T GetTemplateChild<T>(ContentDialog dialog, string name)
        where T : FrameworkElement
    {
        dialog.ApplyTemplate();
        WpfTestHost.DoEvents();

        return dialog.Template?.FindName(name, dialog) as T
            ?? throw new AssertFailedException($"Expected ContentDialog template child '{name}'.");
    }

    private static Task<ContentDialogResult> ShowInPlace(ContentDialog dialog)
    {
        var task = dialog.ShowAsync(ContentDialogPlacement.InPlace);
        WpfTestHost.DoEvents();
        return task;
    }

    private static ContentDialogResult ClickDialogButton(ContentDialogButton button, Action<ContentDialog>? configure = null)
    {
        var dialog = CreateDialog();
        configure?.Invoke(dialog);

        using var host = CreateInPlaceHost(dialog);
        var showTask = ShowInPlace(dialog);
        ClickButton(dialog, button);
        return WaitForResult(showTask);
    }

    private static void ClickButton(ContentDialog dialog, ContentDialogButton button)
    {
        GetTemplateButton(dialog, GetButtonName(button)).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        WpfTestHost.DoEvents();
    }

    private static void PressKey(ContentDialog dialog, Key key)
    {
        var source = PresentationSource.FromVisual(dialog)
            ?? throw new AssertFailedException("Expected ContentDialog to be connected to a presentation source.");
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        dialog.RaiseEvent(args);
        WpfTestHost.DoEvents();
    }

    private static Button GetTemplateButton(ContentDialog dialog, string name)
    {
        dialog.ApplyTemplate();
        WpfTestHost.DoEvents();

        var button = VisualTreeTestHelper
            .EnumerateDescendants(dialog)
            .OfType<Button>()
            .FirstOrDefault(candidate => candidate.Name == name);

        Assert.IsNotNull(button, $"Expected ContentDialog template button named '{name}'.");
        return button!;
    }

    private static string GetButtonName(ContentDialogButton button)
    {
        return button switch
        {
            ContentDialogButton.Primary => "PrimaryButton",
            ContentDialogButton.Secondary => "SecondaryButton",
            ContentDialogButton.Close => "CloseButton",
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    private static T WaitForResult<T>(Task<T> task)
    {
        PumpUntil(() => task.IsCompleted);
        return task.GetAwaiter().GetResult();
    }

    private static void PumpUntil(Func<bool> condition, int timeoutMilliseconds = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);

        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                Assert.Fail("Timed out waiting for ContentDialog dispatcher work.");
            }

            Thread.Sleep(10);
            WpfTestHost.DoEvents();
        }
    }

    private static void AssertResource<T>(ResourceDictionary resources, object key, T expected)
    {
        Assert.AreEqual(expected, resources[key]);
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"{groupName}.{stateName} should set {setterTarget}.");
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
