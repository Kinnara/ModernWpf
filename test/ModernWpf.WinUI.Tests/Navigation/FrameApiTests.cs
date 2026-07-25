using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Media.Animation;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Navigation;

[TestClass]
public class FrameApiTests
{
    [TestMethod]
    public void PageInstanceNavigationTransitionOverloadsAreAvailable()
    {
        var frameType = typeof(ModernWpf.Controls.Frame);

        Assert.IsNotNull(
            frameType.GetMethod(
                nameof(ModernWpf.Controls.Frame.Navigate),
                new[] { typeof(object), typeof(NavigationTransitionInfo) }));
        Assert.IsNotNull(
            frameType.GetMethod(
                nameof(ModernWpf.Controls.Frame.Navigate),
                new[] { typeof(object), typeof(object), typeof(NavigationTransitionInfo) }));
    }

    [TestMethod]
    public void PageInstanceNavigationTransitionOverloadsApplyOverrides()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var frame = new ModernWpf.Controls.Frame();
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            var firstPage = new ModernWpf.Controls.Page();
            Assert.IsTrue(frame.Navigate(firstPage));
            host.UpdateLayout();
            Assert.AreSame(firstPage, frame.Content);

            var transitionOverrideField = typeof(ModernWpf.Controls.Frame).GetField(
                "_transitionInfoOverride",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertFailedException("Expected Frame to retain the active navigation transition override.");

            NavigationTransitionInfo? observedTransition = null;
            object? observedExtraData = null;
            frame.Navigating += (sender, args) =>
            {
                observedTransition = transitionOverrideField.GetValue(frame) as NavigationTransitionInfo;
                observedExtraData = args.ExtraData;
            };

            var secondPage = new ModernWpf.Controls.Page();
            var secondTransition = new SuppressNavigationTransitionInfo();
            Assert.IsTrue(frame.Navigate((object)secondPage, secondTransition));
            host.UpdateLayout();

            Assert.AreSame(secondPage, frame.Content);
            Assert.AreSame(secondTransition, observedTransition);
            Assert.IsNull(observedExtraData);
            Assert.IsNull(transitionOverrideField.GetValue(frame));

            observedTransition = null;
            observedExtraData = null;
            var thirdPage = new ModernWpf.Controls.Page();
            var extraData = new object();
            var thirdTransition = new DrillInNavigationTransitionInfo();
            Assert.IsTrue(frame.Navigate((object)thirdPage, extraData, thirdTransition));
            host.UpdateLayout();

            Assert.AreSame(thirdPage, frame.Content);
            Assert.AreSame(thirdTransition, observedTransition);
            Assert.AreSame(extraData, observedExtraData);
            Assert.IsNull(transitionOverrideField.GetValue(frame));
        });
    }

    [TestMethod]
    public void SourcePageTypeBindingRemainsActiveAcrossNavigation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var viewModel = new FrameSourcePageTypeViewModel();
            var frame = new ModernWpf.Controls.Frame
            {
                DataContext = viewModel
            };
            frame.SetBinding(
                ModernWpf.Controls.Frame.SourcePageTypeProperty,
                new Binding(nameof(FrameSourcePageTypeViewModel.SourcePageType)));

            using var host = new TestWindowHost(frame, width: 320, height: 240);
            host.UpdateLayout();

            Assert.IsInstanceOfType(frame.Content, typeof(FrameBindingFirstPage));
            Assert.AreEqual(typeof(FrameBindingFirstPage), frame.CurrentSourcePageType);
            Assert.IsTrue(BindingOperations.IsDataBound(
                frame,
                ModernWpf.Controls.Frame.SourcePageTypeProperty));

            viewModel.SourcePageType = typeof(FrameBindingSecondPage);
            host.UpdateLayout();

            Assert.IsInstanceOfType(frame.Content, typeof(FrameBindingSecondPage));
            Assert.AreEqual(typeof(FrameBindingSecondPage), frame.CurrentSourcePageType);
            Assert.IsTrue(BindingOperations.IsDataBound(
                frame,
                ModernWpf.Controls.Frame.SourcePageTypeProperty));

            viewModel.SourcePageType = typeof(FrameBindingFirstPage);
            host.UpdateLayout();

            Assert.IsInstanceOfType(frame.Content, typeof(FrameBindingFirstPage));
            Assert.AreEqual(typeof(FrameBindingFirstPage), frame.CurrentSourcePageType);
        });
    }

    public sealed class FrameBindingFirstPage : ModernWpf.Controls.Page
    {
    }

    public sealed class FrameBindingSecondPage : ModernWpf.Controls.Page
    {
    }

    private sealed class FrameSourcePageTypeViewModel : INotifyPropertyChanged
    {
        private Type _sourcePageType = typeof(FrameBindingFirstPage);

        public event PropertyChangedEventHandler? PropertyChanged;

        public Type SourcePageType
        {
            get => _sourcePageType;
            set
            {
                if (_sourcePageType != value)
                {
                    _sourcePageType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourcePageType)));
                }
            }
        }
    }
}
