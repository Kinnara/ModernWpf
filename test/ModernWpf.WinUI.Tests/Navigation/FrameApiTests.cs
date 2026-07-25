using System.Reflection;
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
}
