using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class ItemCollectionTransitionProviderTests
{
    [TestMethod]
    public void TransitionProgressStartsAndCompletesExactlyOnce()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new RecordingTransitionProvider { ShouldAnimateValue = true };
            var element = new Border();
            var transition = new ItemCollectionTransition(
                provider,
                element,
                ItemCollectionTransitionOperation.Add,
                ItemCollectionTransitionTriggers.CollectionChangeAdd);
            var completed = new List<ItemCollectionTransition>();
            provider.TransitionCompleted += (sender, args) => completed.Add(args.Transition);

            Assert.IsFalse(transition.HasStarted);
            Assert.AreEqual(ItemCollectionTransitionOperation.Add, transition.Operation);
            Assert.AreEqual(ItemCollectionTransitionTriggers.CollectionChangeAdd, transition.Triggers);
            Assert.AreEqual(Rect.Empty, transition.OldBounds);
            Assert.AreEqual(Rect.Empty, transition.NewBounds);

            var progress = transition.Start();
            Assert.AreSame(progress, transition.Start());
            Assert.AreSame(transition, progress.Transition);
            Assert.AreSame(element, progress.Element);
            Assert.IsTrue(transition.HasStarted);

            progress.Complete();
            progress.Complete();

            Assert.AreEqual(1, completed.Count);
            Assert.AreSame(transition, completed[0]);
        });
    }

    [TestMethod]
    public void QueueAutoCompletesTransitionsThatAreNotAnimated()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new RecordingTransitionProvider { ShouldAnimateValue = false };
            var transition = new ItemCollectionTransition(
                provider,
                new Border(),
                ItemCollectionTransitionOperation.Remove,
                ItemCollectionTransitionTriggers.CollectionChangeRemove);
            var completedCount = 0;
            provider.TransitionCompleted += (sender, args) =>
            {
                Assert.AreSame(transition, args.Transition);
                completedCount++;
            };

            provider.QueueTransition(transition);
            FlushRendering();

            Assert.AreEqual(SystemParameters.ClientAreaAnimation ? 1 : 0, provider.ShouldAnimateCalls);
            Assert.AreEqual(0, provider.StartTransitionsCalls);
            Assert.IsFalse(transition.HasStarted);
            Assert.AreEqual(1, completedCount);
        });
    }

    [TestMethod]
    public void QueueUsesTheSystemAnimationSettingAndCompletes()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new RecordingTransitionProvider
            {
                ShouldAnimateValue = true,
                CompleteStartedTransitions = true
            };
            var transition = new ItemCollectionTransition(
                provider,
                new Border(),
                ItemCollectionTransitionTriggers.LayoutTransition,
                new Rect(0, 0, 10, 10),
                new Rect(20, 30, 10, 10));
            var completedCount = 0;
            provider.TransitionCompleted += (sender, args) => completedCount++;

            provider.QueueTransition(transition);
            FlushRendering();

            Assert.AreEqual(SystemParameters.ClientAreaAnimation ? 1 : 0, provider.ShouldAnimateCalls);
            Assert.AreEqual(SystemParameters.ClientAreaAnimation ? 1 : 0, provider.StartTransitionsCalls);
            Assert.AreEqual(SystemParameters.ClientAreaAnimation, transition.HasStarted);
            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(ItemCollectionTransitionOperation.Move, transition.Operation);
            Assert.AreEqual(new Rect(0, 0, 10, 10), transition.OldBounds);
            Assert.AreEqual(new Rect(20, 30, 10, 10), transition.NewBounds);
        });
    }

    [TestMethod]
    public void ItemsRepeaterAcceptsAnExplicitProviderAndLinedFlowLayoutSuppliesADefault()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new RecordingTransitionProvider();
            var explicitLayout = new ExposedLinedFlowLayout();
            var repeater = new ItemsRepeater
            {
                ItemTransitionProvider = provider,
                Layout = explicitLayout
            };

            Assert.AreSame(provider, repeater.ItemTransitionProvider);
            Assert.AreSame(provider, repeater.GetValue(ItemsRepeater.ItemTransitionProviderProperty));
            Assert.AreEqual(0, explicitLayout.CreateDefaultProviderCalls);

            var defaultLayout = new ExposedLinedFlowLayout();
            var defaultRepeater = new ItemsRepeater { Layout = defaultLayout };

            Assert.AreEqual(1, defaultLayout.CreateDefaultProviderCalls);
            Assert.IsNotNull(defaultLayout.Provider);
            Assert.AreEqual(
                "LinedFlowLayoutItemCollectionTransitionProvider",
                defaultLayout.Provider!.GetType().Name);
        });
    }

    [TestMethod]
    public void ItemsRepeaterCompletesCollectionAddAndRemoveTransitions()
    {
        WpfTestHost.Run(() =>
        {
            var items = new ObservableCollection<int> { 0, 1, 2 };
            var provider = new RecordingTransitionProvider
            {
                ShouldAnimateValue = true,
                CompleteStartedTransitions = true
            };
            var repeater = new ItemsRepeater
            {
                Width = 240,
                Layout = new StackLayout(),
                ItemTransitionProvider = provider,
                ItemsSource = items,
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                    "<Border Height='28'><TextBlock Text='{Binding}' /></Border>" +
                    "</DataTemplate>")
            };

            using var host = new TestWindowHost(repeater, width: 300, height: 180);
            FlushRendering();
            provider.ResetObservations();

            items.Add(3);
            host.UpdateLayout();
            FlushRendering();

            Assert.IsTrue(provider.CompletedTransitions.Any(transition =>
                transition.Operation == ItemCollectionTransitionOperation.Add &&
                (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeAdd) != 0));

            provider.ResetObservations();
            var removedElement = repeater.TryGetElement(1);
            Assert.IsNotNull(removedElement);

            items.RemoveAt(1);
            host.UpdateLayout();
            FlushRendering();
            host.UpdateLayout();

            Assert.IsTrue(provider.CompletedTransitions.Any(transition =>
                transition.Operation == ItemCollectionTransitionOperation.Remove &&
                (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeRemove) != 0));
            Assert.AreEqual(-1, repeater.GetElementIndex(removedElement!));
        });
    }

    [TestMethod]
    public void LinedFlowDefaultProviderSkipsInitialMovesAndResetRemovalsWithAdds()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new ExposedLinedFlowTransitionProvider();
            var initialMove = new ItemCollectionTransition(
                provider,
                new Border(),
                ItemCollectionTransitionTriggers.LayoutTransition,
                new Rect(0, 0, 10, 10),
                new Rect(20, 0, 10, 10));
            var resetRemove = new ItemCollectionTransition(
                provider,
                new Border(),
                ItemCollectionTransitionOperation.Remove,
                ItemCollectionTransitionTriggers.CollectionChangeReset);
            var resetAdd = new ItemCollectionTransition(
                provider,
                new Border(),
                ItemCollectionTransitionOperation.Add,
                ItemCollectionTransitionTriggers.CollectionChangeReset);

            provider.StartNow(new[] { initialMove, resetRemove, resetAdd });

            Assert.IsFalse(initialMove.HasStarted);
            Assert.IsFalse(resetRemove.HasStarted);
            Assert.IsTrue(resetAdd.HasStarted);
            resetAdd.Start().Complete();
        });
    }

    [TestMethod]
    public void LinedFlowDefaultProviderCompletesInterruptedTransitionsAndRestoresTransforms()
    {
        WpfTestHost.Run(() =>
        {
            var provider = new ExposedLinedFlowTransitionProvider();
            var originalTransform = new TranslateTransform(3.0, 4.0);
            var originalTransformOrigin = new Point(0.25, 0.75);
            var element = new Border
            {
                Width = 40,
                Height = 40,
                RenderTransform = originalTransform,
                RenderTransformOrigin = originalTransformOrigin
            };
            using var host = new TestWindowHost(element, width: 120, height: 120);
            FlushRendering();

            var completed = new List<ItemCollectionTransition>();
            provider.TransitionCompleted += (sender, args) => completed.Add(args.Transition);
            var first = new ItemCollectionTransition(
                provider,
                element,
                ItemCollectionTransitionOperation.Add,
                ItemCollectionTransitionTriggers.CollectionChangeAdd);
            var second = new ItemCollectionTransition(
                provider,
                element,
                ItemCollectionTransitionOperation.Remove,
                ItemCollectionTransitionTriggers.CollectionChangeRemove);

            provider.StartNow(new[] { first });
            Assert.IsTrue(first.HasStarted);
            Assert.IsInstanceOfType<TransformGroup>(element.RenderTransform);

            provider.StartNow(new[] { second });

            Assert.AreEqual(1, completed.Count);
            Assert.AreSame(first, completed[0]);
            Assert.IsTrue(second.HasStarted);
            Assert.IsInstanceOfType<TransformGroup>(element.RenderTransform);

            WaitFor(
                () => completed.Count == 2,
                "The replacement LinedFlowLayout transition did not complete.",
                timeoutMilliseconds: 2500);

            Assert.AreSame(second, completed[1]);
            Assert.AreEqual(1, completed.Count(item => ReferenceEquals(item, first)));
            Assert.AreEqual(1, completed.Count(item => ReferenceEquals(item, second)));
            Assert.AreSame(originalTransform, element.RenderTransform);
            Assert.AreEqual(originalTransformOrigin, element.RenderTransformOrigin);
        });
    }

    private static void FlushRendering()
    {
        var frame = new DispatcherFrame();
        var timedOut = false;
        EventHandler? renderingHandler = null;
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        renderingHandler = (sender, args) =>
        {
            CompositionTarget.Rendering -= renderingHandler;
            frame.Continue = false;
        };
        timer.Tick += (sender, args) =>
        {
            timedOut = true;
            CompositionTarget.Rendering -= renderingHandler;
            frame.Continue = false;
        };

        CompositionTarget.Rendering += renderingHandler;
        timer.Start();
        Dispatcher.PushFrame(frame);
        timer.Stop();

        if (timedOut)
        {
            Assert.Fail("Timed out waiting for CompositionTarget.Rendering.");
        }
    }

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (!predicate() && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
            WpfTestHost.DoEvents();
        }

        Assert.IsTrue(predicate(), failureMessage);
    }

    private sealed class RecordingTransitionProvider : ItemCollectionTransitionProvider
    {
        public RecordingTransitionProvider()
        {
            TransitionCompleted += (sender, args) => CompletedTransitions.Add(args.Transition);
        }

        public bool ShouldAnimateValue { get; set; }

        public bool CompleteStartedTransitions { get; set; }

        public int ShouldAnimateCalls { get; private set; }

        public int StartTransitionsCalls { get; private set; }

        public List<ItemCollectionTransition> ObservedTransitions { get; } =
            new List<ItemCollectionTransition>();

        public List<ItemCollectionTransition> CompletedTransitions { get; } =
            new List<ItemCollectionTransition>();

        public void ResetObservations()
        {
            ShouldAnimateCalls = 0;
            StartTransitionsCalls = 0;
            ObservedTransitions.Clear();
            CompletedTransitions.Clear();
        }

        protected override bool ShouldAnimateCore(ItemCollectionTransition transition)
        {
            ShouldAnimateCalls++;
            ObservedTransitions.Add(transition);
            return ShouldAnimateValue;
        }

        protected override void StartTransitions(IList<ItemCollectionTransition> transitions)
        {
            StartTransitionsCalls++;
            foreach (var transition in transitions)
            {
                var progress = transition.Start();
                if (CompleteStartedTransitions)
                {
                    progress.Complete();
                }
            }
        }
    }

    private sealed class ExposedLinedFlowLayout : LinedFlowLayout
    {
        public int CreateDefaultProviderCalls { get; private set; }

        public ItemCollectionTransitionProvider? Provider { get; private set; }

        protected override ItemCollectionTransitionProvider CreateDefaultItemTransitionProvider()
        {
            CreateDefaultProviderCalls++;
            Provider = base.CreateDefaultItemTransitionProvider();
            return Provider;
        }
    }

    private sealed class ExposedLinedFlowTransitionProvider : LinedFlowLayoutItemCollectionTransitionProvider
    {
        public void StartNow(IList<ItemCollectionTransition> transitions)
        {
            StartTransitions(transitions);
        }
    }
}
