using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.AnnotatedScrollBar;

[TestClass]
public class AnnotatedScrollBarApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();

            Assert.IsNotNull(annotatedScrollBar);
            Assert.IsNotNull(annotatedScrollBar.Labels);
            Assert.IsNull(annotatedScrollBar.LabelTemplate);
            Assert.IsNull(annotatedScrollBar.DetailLabelTemplate);
            Assert.AreEqual(0d, annotatedScrollBar.SmallChange);

            var scrollController = annotatedScrollBar.ScrollController;
            Assert.AreSame(annotatedScrollBar, scrollController);
            Assert.IsFalse(scrollController.CanScroll);
            Assert.IsFalse(scrollController.IsScrollingWithMouse);

            Assert.IsNotNull(scrollController.PanningInfo);
            Assert.IsTrue(scrollController.PanningInfo.IsRailEnabled);
            Assert.AreEqual(Orientation.Vertical, scrollController.PanningInfo.PanOrientation);
            Assert.IsNull(scrollController.PanningInfo.PanningElementAncestor);
        });
    }

    [TestMethod]
    public void VerifyLabelCollectionIsInstanceScoped()
    {
        WpfTestHost.Run(() =>
        {
            var first = new ModernWpf.Controls.AnnotatedScrollBar();
            var second = new ModernWpf.Controls.AnnotatedScrollBar();

            first.Labels.Add(new AnnotatedScrollBarLabel("A", 10));

            Assert.AreEqual(1, first.Labels.Count);
            Assert.AreEqual(0, second.Labels.Count);
            Assert.AreNotSame(first.Labels, second.Labels);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var labels = new ObservableCollection<AnnotatedScrollBarLabel>
            {
                new AnnotatedScrollBarLabel("First", 0),
                new AnnotatedScrollBarLabel("Second", 100)
            };
            var labelTemplate = new DataTemplate();
            var detailLabelTemplate = new DataTemplate();
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels = labels,
                LabelTemplate = labelTemplate,
                DetailLabelTemplate = detailLabelTemplate,
                SmallChange = 24
            };

            Assert.AreSame(labels, annotatedScrollBar.Labels);
            Assert.AreSame(labelTemplate, annotatedScrollBar.LabelTemplate);
            Assert.AreSame(detailLabelTemplate, annotatedScrollBar.DetailLabelTemplate);
            Assert.AreEqual(24d, annotatedScrollBar.SmallChange);
        });
    }

    [TestMethod]
    public void VerifyLabelProperties()
    {
        WpfTestHost.Run(() =>
        {
            var label = new AnnotatedScrollBarLabel("Important section", 42);

            Assert.AreEqual("Important section", label.Content);
            Assert.AreEqual(42d, label.ScrollOffset);
        });
    }

    [TestMethod]
    public void VerifyTemplateUsesWinUISourceParts()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels =
                {
                    new AnnotatedScrollBarLabel("A", 0),
                    new AnnotatedScrollBarLabel("B", 50)
                }
            };

            using var host = new TestWindowHost(annotatedScrollBar, width: 160, height: 220);
            WpfTestHost.DoEvents();

            Assert.IsNotNull(FindTemplatePart<Border>(annotatedScrollBar, "PART_VerticalThumb"));
            Assert.IsNotNull(FindTemplatePart<Border>(annotatedScrollBar, "PART_VerticalThumbGhost"));
            Assert.IsNotNull(FindTemplatePart<RepeatButton>(annotatedScrollBar, "PART_VerticalIncrementRepeatButton"));
            Assert.IsNotNull(FindTemplatePart<RepeatButton>(annotatedScrollBar, "PART_VerticalDecrementRepeatButton"));
            Assert.IsNotNull(FindTemplatePart<Grid>(annotatedScrollBar, "PART_VerticalGrid"));
            Assert.IsNotNull(FindTemplatePart<Grid>(annotatedScrollBar, "PART_LabelsGrid"));
            Assert.IsNotNull(FindTemplatePart<ToolTip>(annotatedScrollBar, "PART_DetailLabelToolTip"));
            Assert.IsNull(FindTemplatePart<FrameworkElement>(annotatedScrollBar, "PART_Rail"));
            Assert.IsNull(FindTemplatePart<FrameworkElement>(annotatedScrollBar, "PART_LabelsHost"));
            Assert.IsFalse(VisualTreeTestHelper.EnumerateDescendants(annotatedScrollBar).OfType<ItemsControl>().Any());

            var panningInfo = annotatedScrollBar.ScrollController.PanningInfo;
            Assert.AreSame(FindTemplatePart<Grid>(annotatedScrollBar, "PART_VerticalGrid"), panningInfo.PanningElementAncestor);
        });
    }

    [TestMethod]
    public void VerifyLabelsAreLaidOutAsContentPresenters()
    {
        WpfTestHost.Run(() =>
        {
            var labels = new ObservableCollection<AnnotatedScrollBarLabel>
            {
                new AnnotatedScrollBarLabel("A", 0),
                new AnnotatedScrollBarLabel("B", 50)
            };
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels = labels
            };

            using var host = new TestWindowHost(annotatedScrollBar, width: 160, height: 220);
            WpfTestHost.DoEvents();

            var labelsGrid = FindTemplatePart<Grid>(annotatedScrollBar, "PART_LabelsGrid");
            Assert.IsNotNull(labelsGrid);
            Assert.AreEqual(2, labelsGrid!.Children.Count);
            CollectionAssert.AreEqual(
                labels.Cast<object>().ToArray(),
                labelsGrid.Children.OfType<ContentPresenter>().Select(presenter => presenter.Content).ToArray());

            labels.Add(new AnnotatedScrollBarLabel("C", 100));
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreEqual(3, labelsGrid.Children.Count);
            CollectionAssert.AreEqual(
                labels.Cast<object>().ToArray(),
                labelsGrid.Children.OfType<ContentPresenter>().Select(presenter => presenter.Content).ToArray());
        });
    }

    [TestMethod]
    public void VerifySetValuesValidation()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();
            var controller = annotatedScrollBar.ScrollController;

            Assert.ThrowsException<ArgumentException>(() => controller.SetValues(100, 0, 0, 0));
            Assert.ThrowsException<ArgumentException>(() => controller.SetValues(0, 100, 0, -1));
        });
    }

    [TestMethod]
    public void VerifyCanScrollTracksScrollabilityAndEnabledState()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();
            var controller = annotatedScrollBar.ScrollController;
            var canScrollChangedCount = 0;
            controller.CanScrollChanged += (_, _) => canScrollChangedCount++;

            controller.SetIsScrollable(true);

            Assert.IsTrue(controller.CanScroll);
            Assert.AreEqual(1, canScrollChangedCount);

            annotatedScrollBar.IsEnabled = false;

            Assert.IsFalse(controller.CanScroll);
            Assert.AreEqual(2, canScrollChangedCount);
        });
    }

    [TestMethod]
    public void VerifyClickRaisesScrollToRequested()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();
            ScrollControllerScrollToRequestedEventArgs? requestedArgs = null;
            AnnotatedScrollBarScrollingEventArgs? scrollingArgs = null;
            annotatedScrollBar.ScrollController.SetValues(0, 100, 0, 0);
            annotatedScrollBar.Scrolling += (_, args) => scrollingArgs = args;
            annotatedScrollBar.ScrollController.ScrollToRequested += (_, args) => requestedArgs = args;

            annotatedScrollBar.ScrollToRatioForTesting(0.5, AnnotatedScrollBarScrollingEventKind.Click);

            Assert.IsNotNull(scrollingArgs);
            Assert.IsNotNull(requestedArgs);
            Assert.AreEqual(scrollingArgs!.ScrollOffset, requestedArgs!.Offset);
            Assert.AreEqual(ScrollingAnimationMode.Disabled, requestedArgs.Options.AnimationMode);
            Assert.AreEqual(ScrollingSnapPointsMode.Ignore, requestedArgs.Options.SnapPointsMode);
        });
    }

    [TestMethod]
    public void VerifyCanceledScrollingSuppressesScrollRequest()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();
            var scrollToRequested = false;
            annotatedScrollBar.Scrolling += (_, args) => args.Cancel = true;
            annotatedScrollBar.ScrollController.ScrollToRequested += (_, _) => scrollToRequested = true;

            var scrollingArgs = annotatedScrollBar.ScrollToRatioForTesting(0.5, AnnotatedScrollBarScrollingEventKind.Click);

            Assert.IsNotNull(scrollingArgs);
            Assert.IsTrue(scrollingArgs!.Cancel);
            Assert.IsFalse(scrollToRequested);
        });
    }

    [TestMethod]
    public void VerifySmallChangeButtonDirectionMatchesWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                SmallChange = 10
            };
            double? scrollByDelta = null;
            float? scrollVelocity = null;
            annotatedScrollBar.ScrollController.SetValues(0, 100, 50, 80);
            annotatedScrollBar.ScrollController.ScrollByRequested += (_, args) => scrollByDelta = args.OffsetDelta;
            annotatedScrollBar.ScrollController.AddScrollVelocityRequested += (_, args) => scrollVelocity = args.OffsetVelocity;

            var incrementArgs = annotatedScrollBar.ScrollToRatioForTesting(0.5, AnnotatedScrollBarScrollingEventKind.IncrementButton);

            Assert.AreEqual(40d, incrementArgs.ScrollOffset);
            Assert.IsTrue(scrollByDelta == -10d || scrollVelocity < 0);

            scrollByDelta = null;
            scrollVelocity = null;
            annotatedScrollBar.ScrollController.SetValues(0, 100, 50, 80);

            var decrementArgs = annotatedScrollBar.ScrollToRatioForTesting(0.5, AnnotatedScrollBarScrollingEventKind.DecrementButton);

            Assert.AreEqual(60d, decrementArgs.ScrollOffset);
            Assert.IsTrue(scrollByDelta == 10d || scrollVelocity > 0);
        });
    }

    [TestMethod]
    public void VerifyDetailLabelHasNoNearestLabelFallback()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels =
                {
                    new AnnotatedScrollBarLabel("Start", 0),
                    new AnnotatedScrollBarLabel("Middle", 50),
                    new AnnotatedScrollBarLabel("End", 100)
                }
            };

            var defaultArgs = annotatedScrollBar.RequestDetailLabelForRatioForTesting(0.5);
            Assert.IsNull(defaultArgs.Content);

            annotatedScrollBar.DetailLabelRequested += (_, args) => args.Content = "Offset " + args.ScrollOffset;

            var args = annotatedScrollBar.RequestDetailLabelForRatioForTesting(0.5);

            Assert.AreEqual("Offset " + args.ScrollOffset, args.Content);
        });
    }

    private static T? FindTemplatePart<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T ??
            VisualTreeTestHelper
                .EnumerateDescendants(control)
                .OfType<T>()
                .FirstOrDefault(element => element.Name == name);
    }
}
