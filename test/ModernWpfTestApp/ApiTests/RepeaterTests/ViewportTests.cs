// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MUXControlsTestApp.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Collections.Generic;
using ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests.Common.Mocks;
using System.Numerics;
using Common;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Markup;
using ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests.Common;

using Task = System.Threading.Tasks.Task;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

using VirtualizingLayout = ModernWpf.Controls.VirtualizingLayout;
using ItemsRepeater = ModernWpf.Controls.ItemsRepeater;
using VirtualizingLayoutContext = ModernWpf.Controls.VirtualizingLayoutContext;
using LayoutContext = ModernWpf.Controls.LayoutContext;
using RecyclingElementFactory = ModernWpf.Controls.RecyclingElementFactory;
using StackLayout = ModernWpf.Controls.StackLayout;
using UniformGridLayout = ModernWpf.Controls.UniformGridLayout;
using ItemsRepeaterScrollHost = ModernWpf.Controls.ItemsRepeaterScrollHost;
//using Scroller = ModernWpf.Controls.Primitives.Scroller;
//using ScrollCompletedEventArgs = ModernWpf.Controls.ScrollCompletedEventArgs;
//using ZoomCompletedEventArgs = ModernWpf.Controls.ZoomCompletedEventArgs;
//using AnimationMode = ModernWpf.Controls.AnimationMode;
//using SnapPointsMode = ModernWpf.Controls.SnapPointsMode;
//using ScrollOptions = ModernWpf.Controls.ScrollOptions;
//using ZoomOptions = ModernWpf.Controls.ZoomOptions;
//using ContentOrientation = ModernWpf.Controls.ContentOrientation;
using IRepeaterScrollingSurface = ModernWpf.Controls.IRepeaterScrollingSurface;
using ConfigurationChangedEventHandler = ModernWpf.Controls.ConfigurationChangedEventHandler;
using PostArrangeEventHandler = ModernWpf.Controls.PostArrangeEventHandler;
using ViewportChangedEventHandler = ModernWpf.Controls.ViewportChangedEventHandler;
using ScrollViewer = System.Windows.Controls.ScrollViewerEx;


namespace ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests
{
    [TestClass]
    public class ViewportTests : ApiTestBase
    {
        [TestMethod]
        public void ValidateNoScrollingSurfaceScenario()
        {
            RunOnUIThread.Execute(() =>
            {
                var realizationRects = new List<Rect>();

                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 500), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                Content = repeater;
                Content.UpdateLayout();

                Verify.AreEqual(2, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);

                if (true /*!PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5)*/)
                {
                    Verify.AreEqual(new Rect(0, 0, float.MaxValue, float.MaxValue), realizationRects[1]);
                }
                else
                {
                    // Using Effective Viewport
                    Verify.AreEqual(0, realizationRects[1].X);
                    // 32 pixel title bar and some tolerance for borders
                    Verify.IsLessThan(2.0, Math.Abs(realizationRects[1].Y - 32));
                    // Width/Height depends on the window size, so just
                    // validating something reasonable here to avoid flakiness.
                    Verify.IsLessThan(500.0, realizationRects[1].Width);
                    Verify.IsLessThan(500.0, realizationRects[1].Height);
                }

                realizationRects.Clear();
            });
        }

        // [TestMethod] Temporarily disabled for bug 18866003
        public void ValidateItemsRepeaterScrollHostScenario()
        {
            var realizationRects = new List<Rect>();
            var scrollViewer = (ScrollViewer)null;
            var viewChangedEvent = new ManualResetEvent(false);
            int waitTime = 2000; // 2 seconds 

            RunOnUIThread.Execute(() =>
            {
                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 600), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                scrollViewer = new ScrollViewer
                {
                    Content = repeater,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                };

                scrollViewer.ViewChanged += (sender, args) =>
                {
                    if (!args.IsIntermediate)
                    {
                        viewChangedEvent.Set();
                    }
                };

                var tracker = new ItemsRepeaterScrollHost()
                {
                    Width = 200,
                    Height = 300,
                    ScrollViewer = scrollViewer
                };

                Content = tracker;
                Content.UpdateLayout();

                // First layout pass will invalidate measure during the first arrange
                // so that we can get a viewport during the second measure/arrange pass.
                // That's why Measure gets invoked twice.
                // After that, ScrollViewer.SizeChanged is raised and it also invalidates
                // layout (third pass).
                Verify.AreEqual(3, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);
                Verify.AreEqual(new Rect(0, 0, 200, 300), realizationRects[1]);
                Verify.AreEqual(new Rect(0, 0, 200, 300), realizationRects[2]);
                realizationRects.Clear();

                viewChangedEvent.Reset();
                scrollViewer.ChangeView(null, 100.0, 1.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(0, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangedEvent.Reset();
                // Max viewport offset is (300, 400). Horizontal viewport offset
                // is expected to get coerced from 400 to 300.
                scrollViewer.ChangeView(400, 100.0, 1.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangedEvent.Reset();
                scrollViewer.ChangeView(null, null, 2.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 100, 150), realizationRects.Last());
                realizationRects.Clear();
            });
        }

        [TestMethod]
        public void CanRegisterElementsWithScrollingSurfaces()
        {
            /*
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            {
                Log.Warning("Skipping since RS5+ we use effective viewport instead of IRepeaterScrollingSurface");
                return;
            }
            */

            // In this test, we validate that ItemsRepeater can register and unregister its children
            // with one or two scrollers.
            // The initial setup is 4 nested scrollers with a root repeater under which
            // we have two groups (and two other inner repeaters).
            RunElementTrackingTestRoutine((data, scrollers, rootRepeater) =>
            {
                var resetScrollers = (Action)(() =>
                {
                    foreach (var scroller in scrollers)
                    {
                        scroller.IsHorizontallyScrollable = false;
                        scroller.IsVerticallyScrollable = false;
                        scroller.RegisterAnchorCandidateFunc = null;
                        scroller.UnregisterAnchorCandidateFunc = null;
                        scroller.GetRelativeViewportFunc = null;
                    }
                });

                var actualActionSequence = new List<string>();
                var expectedActionSequence = new List<string>()
                {
                    "S2: GetRelativeViewport ItemsRepeater #0",
                    "S2: Register Item #0.0",
                    "S2: Register Item #0.1",
                    "S2: Register Item #0.2",
                    "S2: GetRelativeViewport ItemsRepeater #1",
                    "S2: Register Item #1.0",
                    "S2: Register Item #1.1",
                    "S2: Register Item #1.2",
                    "S2: GetRelativeViewport Root ItemsRepeater",
                    "S2: Register Group #0",
                    "S2: Register Group #1"
                };

                var registerAnchorCandidateFunc = (Action<TestScrollingSurface, UIElement, bool>)((scroller, element, expectedInPostArrange) =>
                {
                    actualActionSequence.Add(scroller.Tag + ": Register " + ((FrameworkElement)element).Tag);
                    Log.Comment(actualActionSequence.Last());
                    Verify.AreEqual(expectedInPostArrange, scroller.InPostArrange);
                });

                var unregisterAnchorCandidateFunc = (Action<TestScrollingSurface, UIElement>)((scroller, element) =>
                {
                    actualActionSequence.Add(scroller.Tag + ": Unregister " + ((FrameworkElement)element).Tag);
                    Log.Comment(actualActionSequence.Last());
                    Verify.IsFalse(scroller.InArrange);
                    Verify.IsFalse(scroller.InPostArrange);
                });

                var getRelativeViewportFunc = (Func<TestScrollingSurface, UIElement, bool, Rect>)((scroller, element, expectedInPostArrange) =>
                {
                    actualActionSequence.Add(scroller.Tag + ": GetRelativeViewport " + ((FrameworkElement)element).Tag);
                    Log.Comment(actualActionSequence.Last());
                    Verify.AreEqual(expectedInPostArrange, scroller.InPostArrange);
                    var outerScroller = scrollers.Last();
                    return new Rect(0, 0, outerScroller.Width, outerScroller.Height);
                });

                // Step 1.0:
                // - Make scroller 2 scrollable in both directions.
                // - Validate that the correct methods are called on it from repeater at the right moment.
                scrollers[2].IsHorizontallyScrollable = true;
                scrollers[2].IsVerticallyScrollable = true;
                scrollers[2].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[2], element, true);
                scrollers[2].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[2], element);
                scrollers[2].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[2], element, true);

                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ", expectedActionSequence.Concat(expectedActionSequence)), string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 1.1:
                // - Remove an item from the data.
                // - Validate that the recycled element is no longer a candidate for tracking.
                data[0].RemoveAt(1);
                Content.UpdateLayout();
                Verify.AreEqual(string.Join( ", ",
                    new List<string>()
                    {
                        "S2: Unregister Item #0.1",
                        "S2: GetRelativeViewport ItemsRepeater #0",
                        "S2: Register Item #0.0",
                        "S2: Register Item #0.2",
                        "S2: GetRelativeViewport ItemsRepeater #1",
                        "S2: Register Item #1.0",
                        "S2: Register Item #1.1",
                        "S2: Register Item #1.2",
                        "S2: GetRelativeViewport Root ItemsRepeater",
                        "S2: Register Group #0",
                        "S2: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 2.0:
                // - Reset the scrollers configuration.
                // - Configure scroller 1 and 3 to be, respectively, vertically and horizontally scrollable.
                // - Validate that the correct methods are called on scroller 1 and 3 from repeater at the right moment.
                resetScrollers();
                scrollers[1].IsVerticallyScrollable = true;
                scrollers[3].IsHorizontallyScrollable = true;

                scrollers[1].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[1], element, false);
                scrollers[1].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[1], element);
                scrollers[1].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[1], element, false);

                scrollers[3].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[3], element, true);
                scrollers[3].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[3], element);
                scrollers[3].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[3], element, true);

                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ",
                    new List<string>()
                    {
                        "S3: GetRelativeViewport ItemsRepeater #0",
                        "S1: GetRelativeViewport ItemsRepeater #0",
                        "S3: Register Item #0.0",
                        "S1: Register Item #0.0",
                        "S3: Register Item #0.2",
                        "S1: Register Item #0.2",
                        "S3: GetRelativeViewport ItemsRepeater #1",
                        "S1: GetRelativeViewport ItemsRepeater #1",
                        "S3: Register Item #1.0",
                        "S1: Register Item #1.0",
                        "S3: Register Item #1.1",
                        "S1: Register Item #1.1",
                        "S3: Register Item #1.2",
                        "S1: Register Item #1.2",
                        "S3: GetRelativeViewport Root ItemsRepeater",
                        "S1: GetRelativeViewport Root ItemsRepeater",
                        "S3: Register Group #0",
                        "S1: Register Group #0",
                        "S3: Register Group #1",
                        "S1: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 2.1:
                // - Remove an item from the data.
                // - Validate that scroller 1 and 3 are no longer tracking the recycled element because it's not registered anymore.
                data[1].RemoveAt(1);
                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ",
                    new List<string>()
                    {
                        "S3: Unregister Item #1.1",
                        "S1: Unregister Item #1.1",
                        "S3: GetRelativeViewport ItemsRepeater #0",
                        "S1: GetRelativeViewport ItemsRepeater #0",
                        "S3: Register Item #0.0",
                        "S1: Register Item #0.0",
                        "S3: Register Item #0.2",
                        "S1: Register Item #0.2",
                        "S3: GetRelativeViewport ItemsRepeater #1",
                        "S1: GetRelativeViewport ItemsRepeater #1",
                        "S3: Register Item #1.0",
                        "S1: Register Item #1.0",
                        "S3: Register Item #1.2",
                        "S1: Register Item #1.2",
                        "S3: GetRelativeViewport Root ItemsRepeater",
                        "S1: GetRelativeViewport Root ItemsRepeater",
                        "S3: Register Group #0",
                        "S1: Register Group #0",
                        "S3: Register Group #1",
                        "S1: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();
                //Log.Comment(">> " + string.Join(", ", actualActionSequence.Select(i => "\"" + i + "\"")));
            });
        }

        [TestMethod]
        public void ValidateSuggestedElement()
        {
            /*
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            {
                Log.Warning("Skipping since RS5+ we use effective viewport instead of IRepeaterScrollingSurface");
                return;
            }
            */

            // In this test, we validate that ItemsRepeater can suggested the correct anchor element
            // to its layout.
            // The initial setup is 4 nested scrollers with a root repeater under which
            // we have two groups (and two other inner repeaters).
            RunElementTrackingTestRoutine((data, scrollers, rootRepeater) =>
            {
                var outerScroller = scrollers[3];
                scrollers[3].IsVerticallyScrollable = true;
                scrollers[1].IsHorizontallyScrollable = true;

                foreach(var scrollableScroller in new [] { scrollers[1], scrollers[3] })
                {
                    scrollableScroller.RegisterAnchorCandidateFunc = (element) => { Log.Comment("Register {0}", ((FrameworkElement)element).Tag); };
                    scrollableScroller.UnregisterAnchorCandidateFunc = (element) => { Log.Comment("Unregister {0}", ((FrameworkElement)element).Tag); };
                    scrollableScroller.GetRelativeViewportFunc = (element) => { Log.Comment("GetRelativeViewport {0}", ((FrameworkElement)element).Tag); return new Rect(0, 0, outerScroller.Width, outerScroller.Height); };
                }                

                var groups = Enumerable.Range(0, data.Count).Select(i =>
                {
                    var group = (StackPanel)rootRepeater.TryGetElement(i);
                    return new
                    {
                        Group = group,
                        Header = (TextBlock)group.Children[0],
                        ItemsRepeater = (ItemsRepeater)group.Children[1]
                    };
                }).ToArray();

                // Scroller 3 will be tracking the first group header while scroller 1 will
                // be tracking the second group header. Our repeaters (root repeater and the two inner
                // "group" repeaters) should only care about what scroller 1 is tracking because
                // it's the inner most scrollable scroller.
                scrollers[3].AnchorElement = groups[0].Header;
                scrollers[1].AnchorElement = groups[1].Header;
                Content.UpdateLayout();

                var stackLayout = (TestStackLayout)rootRepeater.Layout;
                var gridLayout1 = (TestGridLayout)groups[0].ItemsRepeater.Layout;
                var gridLayout2 = (TestGridLayout)groups[1].ItemsRepeater.Layout;

                // Root repeater should suggest the second group (because the group header is
                // an indirect child of it).
                // The other repeaters should not suggest anything because the anchor element
                // is not one of their children (either directly or indirectly).
                Verify.AreEqual(groups[1].Group, stackLayout.SuggestedAnchor);
                Verify.IsNull(gridLayout1.SuggestedAnchor);
                Verify.IsNull(gridLayout2.SuggestedAnchor);

                // Now let's make the second element in the second inner repeater the anchor element.
                scrollers[1].AnchorElement = groups[1].ItemsRepeater.TryGetElement(1);
                rootRepeater.InvalidateMeasure();
                groups[0].ItemsRepeater.InvalidateMeasure();
                groups[1].ItemsRepeater.InvalidateMeasure();
                Content.UpdateLayout();

                // Root repeater should suggest the second group like before.
                // The second inner repeater will suggest the anchor element because it's a direct child.
                Verify.AreEqual(groups[1].Group, stackLayout.SuggestedAnchor);
                Verify.IsNull(gridLayout1.SuggestedAnchor);
                Verify.AreEqual(scrollers[1].AnchorElement, gridLayout2.SuggestedAnchor);
            });
        }

        // [TestMethod] Temporarily disabled for bug 18866003
        public void ValidateLoadUnload()
        {
            /*
            if (!PlatformConfiguration.IsOsVersionGreaterThan(OSVersion.Redstone2))
            {
                Log.Warning("Skipping: Test has instability on rs1 and rs2. Tracked by bug 18919096");
                return;
            }
            */

            // In this test, we will repeatedly put a repeater in and out
            // of the visual tree, under the same or a different parent.
            // And we will validate that the subscriptions and unsubscriptions to
            // the IRepeaterScrollingSurface events is done in sync.

            TestScrollingSurface scroller1 = null;
            TestScrollingSurface scroller2 = null;
            ItemsRepeater repeater = null;
            WeakReference repeaterWeakRef = null;
            var renderingEvent = new ManualResetEvent(false);

            var unorderedLoadEvent = false;
            var loadCounter = 0;
            var unloadCounter = 0;

            int scroller1SubscriberCount = 0;
            int scroller2SubscriberCount = 0;

            RunOnUIThread.Execute(() =>
            {
                CompositionTarget.Rendering += (sender, args) =>
                {
                    renderingEvent.Set();
                };

                var host = new Grid();
                scroller1 = new TestScrollingSurface() { Tag = "Scroller 1" };
                scroller2 = new TestScrollingSurface() { Tag = "Scroller 2" };
                repeater = new ItemsRepeater();
                repeaterWeakRef = new WeakReference(repeater);

                repeater.Loaded += delegate
                {
                    Log.Comment("ItemsRepeater Loaded in " + ((FrameworkElement)repeater.Parent).Tag);
                    unorderedLoadEvent |= (++loadCounter > unloadCounter + 1);
                };
                repeater.Unloaded += delegate
                {
                    Log.Comment("ItemsRepeater Unloaded");
                    unorderedLoadEvent |= (++unloadCounter > loadCounter);
                };

                // Subscribers count should never go above 1 or under 0.
                var validateSubscriberCount = new Action(() =>
                {
                    Verify.IsLessThanOrEqual(scroller1SubscriberCount, 1);
                    Verify.IsGreaterThanOrEqual(scroller1SubscriberCount, 0);

                    Verify.IsLessThanOrEqual(scroller2SubscriberCount, 1);
                    Verify.IsGreaterThanOrEqual(scroller2SubscriberCount, 0);
                });

                scroller1.ConfigurationChangedAddFunc = () => { ++scroller1SubscriberCount; validateSubscriberCount(); };
                scroller2.ConfigurationChangedAddFunc = () => { ++scroller2SubscriberCount; validateSubscriberCount(); };
                scroller1.ConfigurationChangedRemoveFunc = () => { --scroller1SubscriberCount; validateSubscriberCount(); };
                scroller2.ConfigurationChangedRemoveFunc = () => { --scroller2SubscriberCount; validateSubscriberCount(); };

                scroller1.Content = repeater;
                host.Children.Add(scroller1);
                host.Children.Add(scroller2);

                Content = host;
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            renderingEvent.Reset();
            Log.Comment("Putting repeater in and out of scroller 1 until we observe two out-of-sync loaded/unloaded events.");
            for (int i = 0; i < 2; ++i)
            {
                while (!unorderedLoadEvent)
                {
                    RunOnUIThread.Execute(() =>
                    {
                        // Validate subscription count for events + reset.
                        scroller1.Content = null;
                        scroller1.Content = repeater;
                    });
                    // For this issue to repro, we need to wait in such a way
                    // that we don't tick the UI thread. We can't use IdleSynchronizer here.
                    Task.Delay(16 * 3).Wait();
                }
                unorderedLoadEvent = false;
                Log.Comment("Detected an unordered load/unload event.");
            }

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            renderingEvent.Reset();
            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(1, scroller1SubscriberCount);
                Verify.AreEqual(0, scroller2SubscriberCount);

                Log.Comment("Moving repeater from scroller 1 to scroller 2.");
                scroller1.Content = null;
                scroller2.Content = repeater;
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(0, scroller1SubscriberCount);
                Verify.AreEqual(1, scroller2SubscriberCount);

                Log.Comment("Moving repeater out of scroller 2.");
                scroller2.Content = null;
                repeater = null;
            });

            Log.Comment("Waiting for repeater to get GCed.");
            for (int i = 0; i < 5 && repeaterWeakRef.IsAlive; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                IdleSynchronizer.Wait();
            }
            Verify.IsFalse(repeaterWeakRef.IsAlive);

            renderingEvent.Reset();
            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(0, scroller1SubscriberCount);
                Verify.AreEqual(0, scroller2SubscriberCount);

                Log.Comment("Scroller raise IRepeaterScrollSurface.PostArrange. Make sure no one is subscribing to it.");
                scroller1.InvalidateArrange();
                scroller2.InvalidateArrange();
                Content.UpdateLayout();
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");
        }

        private void ValidateRealizedRange(
            ItemsRepeater repeater,
            int expectedFirstGroupIndex,
            int expectedLastGroupIndex,
            int expectedFirstItemGroupIndex,
            int expectedFirstItemIndex,
            int expectedLastItemGroupIndex,
            int expectedLastItemIndex)
        {
            int actualFirstGroupIndex = -1;
            int actualLastGroupIndex = -1;
            int actualFirstItemGroupIndex = -1;
            int actualFirstItemIndex = -1;
            int actualLastItemGroupIndex = -1;
            int actualLastItemIndex = -1;

            RunOnUIThread.Execute(() =>
            {
                int groupIndex = 0;
                int itemIndex = 0;

                var groups = (IEnumerable<NamedGroup<string>>)repeater.ItemsSource;

                foreach (var group in groups)
                {
                    var groupElement = repeater.TryGetElement(groupIndex);

                    if (groupElement != null)
                    {
                        actualFirstGroupIndex =
                            actualFirstGroupIndex == -1 ?
                            groupIndex :
                            actualFirstGroupIndex;

                        actualLastGroupIndex = groupIndex;

                        var innerRepeater = (ItemsRepeater)((FrameworkElement)groupElement).FindName("InnerRepeater");

                        foreach (var item in group)
                        {
                            var itemElement = innerRepeater.TryGetElement(itemIndex);

                            if(itemElement != null)
                            {
                                actualFirstItemGroupIndex =
                                    actualFirstItemGroupIndex == -1 ?
                                    groupIndex :
                                    actualFirstItemGroupIndex;

                                actualFirstItemIndex =
                                    actualFirstItemIndex == -1 ?
                                    itemIndex :
                                    actualFirstItemIndex;

                                actualLastItemGroupIndex = groupIndex;
                                actualLastItemIndex = itemIndex;
                            }

                            ++itemIndex;
                        }
                    }

                    itemIndex = 0;
                    ++groupIndex;
                }
            });

            Verify.AreEqual(expectedFirstGroupIndex, actualFirstGroupIndex);
            Verify.AreEqual(expectedLastGroupIndex, actualLastGroupIndex);
            Verify.AreEqual(expectedFirstItemGroupIndex, actualFirstItemGroupIndex);
            Verify.AreEqual(expectedFirstItemIndex, actualFirstItemIndex);
            Verify.AreEqual(expectedLastItemGroupIndex, actualLastItemGroupIndex);
            Verify.AreEqual(expectedLastItemIndex, actualLastItemIndex);
        }

        private void RunElementTrackingTestRoutine(Action<
            ObservableCollection<ObservableCollection<string>> /* data */,
            TestScrollingSurface[] /* scrollers */,
            ItemsRepeater /* rootRepeater */> testRoutine)
        {
            // Base setup for our element tracking tests.
            // We have 4 fake scrollers in series, initially in a non scrollable configuration.
            // Under them we have a group repeater tree (2 groups with 3 items each).
            // The group UI is a StackPanel with a TextBlock "header" and an inner ItemsRepeater.
            RunOnUIThread.Execute(() =>
            {
                int groupCount = 2;
                int itemsPerGroup = 3;

                var data = new ObservableCollection<ObservableCollection<string>>(
                    Enumerable.Range(0, groupCount).Select(i => new ObservableCollection<string>(
                        Enumerable.Range(0, itemsPerGroup).Select(j => string.Format("Item #{0}.{1}", i, j)))));

                var itemElements = Enumerable.Range(0, groupCount).Select(i =>
                    Enumerable.Range(0, itemsPerGroup).Select(j => new Border { Tag = data[i][j], Width = 50, Height = 50, Background = new SolidColorBrush(Colors.Red) }).ToList()).ToArray();

                var headerElements = Enumerable.Range(0, groupCount).Select(i => new TextBlock { Text = "Header #" + i }).ToList();
                var groupRepeaters = Enumerable.Range(0, groupCount).Select(i => new ItemsRepeater
                {
                    Tag = "ItemsRepeater #" + i,
                    ItemsSource = data[i],
                    ItemTemplate = MockElementFactory.CreateElementFactory(itemElements[i]),
                    Layout = new TestGridLayout { Orientation = Orientation.Horizontal, MinItemWidth = 50, MinItemHeight = 50, MinRowSpacing = 10, MinColumnSpacing = 10 }
                }).ToList();
                var groupElement = Enumerable.Range(0, groupCount).Select(i =>
                {
                    var panel = new StackPanel();
                    panel.Tag = "Group #" + i;
                    panel.Children.Add(headerElements[i]);
                    panel.Children.Add(groupRepeaters[i]);
                    return panel;
                }).ToList();

                var rootRepeater = new ItemsRepeater
                {
                    Tag = "Root ItemsRepeater",
                    ItemsSource = data,
                    ItemTemplate = MockElementFactory.CreateElementFactory(groupElement),
                    Layout = new TestStackLayout { Orientation = Orientation.Vertical }
                };

                var scrollers = new TestScrollingSurface[4];
                for (int i = 0; i < scrollers.Length; ++i)
                {
                    scrollers[i] = new TestScrollingSurface()
                    {
                        Tag = "S" + i,
                        Content = i > 0 ? (UIElement)scrollers[i - 1] : rootRepeater
                    };
                }

                var resetScrollers = (Action)(() =>
                {
                    foreach (var scroller in scrollers)
                    {
                        scroller.IsHorizontallyScrollable = false;
                        scroller.IsVerticallyScrollable = false;
                        scroller.RegisterAnchorCandidateFunc = null;
                        scroller.UnregisterAnchorCandidateFunc = null;
                        scroller.GetRelativeViewportFunc = null;
                    }
                });

                var outerScroller = scrollers.Last();
                outerScroller.Width = 200.0;
                outerScroller.Height = 2000.0;

                Content = outerScroller;
                Content.UpdateLayout();

                testRoutine(data, scrollers, rootRepeater);
            });
        }

        private static VirtualizingLayout GetMonitoringLayout(Size desiredSize, List<Rect> realizationRects)
        {
            return new MockVirtualizingLayout
            {
                MeasureLayoutFunc = (availableSize, context) =>
                {
                    realizationRects.Add(context.RealizationRect);
                    return desiredSize;
                },

                ArrangeLayoutFunc = (finalSize, context) => finalSize
            };
        }

        private class TestScrollingSurface : ContentControl, IRepeaterScrollingSurface
        {
            private bool _isHorizontallyScrollable;
            private bool _isVerticallyScrollable;
            private EventRegistrationTokenTable<ConfigurationChangedEventHandler> _configurationChangedTokenTable;

            public bool InMeasure { get; set; }
            public bool InArrange { get; set; }
            public bool InPostArrange { get; private set; }

            public Action ConfigurationChangedAddFunc { get; set; }
            public Action ConfigurationChangedRemoveFunc { get; set; }

            public Action<UIElement> RegisterAnchorCandidateFunc { get; set; }
            public Action<UIElement> UnregisterAnchorCandidateFunc { get; set; }
            public Func<UIElement, Rect> GetRelativeViewportFunc { get; set; }

            public UIElement AnchorElement { get; set; }

            public bool IsHorizontallyScrollable
            {
                get { return _isHorizontallyScrollable; }
                set
                {
                    _isHorizontallyScrollable = value;
                    RaiseConfigurationChanged();
                    InvalidateMeasure();
                }
            }

            public bool IsVerticallyScrollable
            {
                get { return _isVerticallyScrollable; }
                set
                {
                    _isVerticallyScrollable = value;
                    RaiseConfigurationChanged();
                    InvalidateMeasure();
                }
            }

            public event ConfigurationChangedEventHandler ConfigurationChanged
            {
                add
                {
                    if (ConfigurationChangedAddFunc != null)
                    {
                        ConfigurationChangedAddFunc();
                    }

                    /*return*/ EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                        .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                        .AddEventHandler(value);
                }
                remove
                {
                    if (ConfigurationChangedRemoveFunc != null)
                    {
                        ConfigurationChangedRemoveFunc();
                    }

                    EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                        .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                        .RemoveEventHandler(value);
                }
            }
            public event PostArrangeEventHandler PostArrange;
#pragma warning disable CS0067
            // Warning CS0067: The event 'ViewportTests.TestScrollingSurface.ViewportChanged' is never used.
            public event ViewportChangedEventHandler ViewportChanged;
#pragma warning restore CS0067

            public void RegisterAnchorCandidate(UIElement element)
            {
                RegisterAnchorCandidateFunc(element);
            }

            public void UnregisterAnchorCandidate(UIElement element)
            {
                UnregisterAnchorCandidateFunc(element);
            }

            public Rect GetRelativeViewport(UIElement child)
            {
                return GetRelativeViewportFunc(child);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                InMeasure = true;
                var result = base.MeasureOverride(availableSize);
                InMeasure = false;
                return result;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                InArrange = true;

                var result = base.ArrangeOverride(finalSize);

                InArrange = false;
                InPostArrange = true;

                if (PostArrange != null)
                {
                    PostArrange(this);
                }

                InPostArrange = false;

                return result;
            }

            private void RaiseConfigurationChanged()
            {
                var temp =
                    EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                    .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                    .InvocationList;
                if (temp != null)
                {
                    temp(this);
                }
            }
        }

        private class TestStackLayout : StackLayout
        {
            public UIElement SuggestedAnchor { get; private set; }

            protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
            {
                var anchorIndex = context.RecommendedAnchorIndex;
                SuggestedAnchor = anchorIndex < 0 ? null : context.GetOrCreateElementAt(anchorIndex);
                return base.MeasureOverride(context, availableSize);
            }
        }

        private class TestGridLayout : UniformGridLayout
        {
            public UIElement SuggestedAnchor { get; private set; }

            protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
            {
                var anchorIndex = context.RecommendedAnchorIndex;
                SuggestedAnchor = anchorIndex < 0 ? null : context.GetOrCreateElementAt(anchorIndex);
                return base.MeasureOverride(context, availableSize);
            }
        }
    }
}