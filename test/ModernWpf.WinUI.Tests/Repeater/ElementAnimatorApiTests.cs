using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class ElementAnimatorApiTests
{
    [TestMethod]
    public void ValidateElementAnimator()
    {
        WpfTestHost.Run(() =>
        {
            var root = new Grid();
            using var host = new TestWindowHost(root);
            var element = new Button();
            root.Children.Add(element);

            var animator = new ElementAnimatorDerived
            {
                HasShowAnimationValue = true,
                HasHideAnimationValue = true,
                HasBoundsChangeAnimationValue = true
            };

            var completed = new List<string>();
            animator.ShowAnimationCompleted += (sender, completedElement) =>
            {
                Assert.AreSame(element, completedElement);
                completed.Add("show");
            };
            animator.HideAnimationCompleted += (sender, completedElement) =>
            {
                Assert.AreSame(element, completedElement);
                completed.Add("hide");
            };
            animator.BoundsChangeAnimationCompleted += (sender, completedElement) =>
            {
                Assert.AreSame(element, completedElement);
                completed.Add("bounds");
            };

            var oldBounds = new Rect(0, 0, 10, 20);
            var newBounds = new Rect(0, 50, 10, 20);
            animator.OnElementShown(element, AnimationContext.CollectionChangeAdd);
            animator.OnElementHidden(element, AnimationContext.CollectionChangeRemove);
            animator.OnElementBoundsChanged(
                element,
                AnimationContext.CollectionChangeAdd | AnimationContext.CollectionChangeRemove,
                oldBounds,
                newBounds);

            Assert.IsTrue(animator.HasShowPending);
            Assert.IsTrue(animator.HasHidePending);
            Assert.IsTrue(animator.HasBoundsChangePending);
            Assert.AreEqual(
                AnimationContext.CollectionChangeAdd | AnimationContext.CollectionChangeRemove,
                animator.SharedAnimationContext);

            FlushRendering();

            Assert.AreEqual(1, animator.ShowCalls.Count);
            Assert.AreSame(element, animator.ShowCalls[0].Element);
            Assert.AreEqual(AnimationContext.CollectionChangeAdd, animator.ShowCalls[0].Context);

            Assert.AreEqual(1, animator.HideCalls.Count);
            Assert.AreSame(element, animator.HideCalls[0].Element);
            Assert.AreEqual(AnimationContext.CollectionChangeRemove, animator.HideCalls[0].Context);

            Assert.AreEqual(1, animator.BoundsChangeCalls.Count);
            Assert.AreSame(element, animator.BoundsChangeCalls[0].Element);
            Assert.AreEqual(AnimationContext.CollectionChangeAdd | AnimationContext.CollectionChangeRemove, animator.BoundsChangeCalls[0].Context);
            Assert.AreEqual(oldBounds, animator.BoundsChangeCalls[0].OldBounds);
            Assert.AreEqual(newBounds, animator.BoundsChangeCalls[0].NewBounds);

            CollectionAssert.AreEqual(new[] { "show", "hide", "bounds" }, completed);
            Assert.IsFalse(animator.HasShowPending);
            Assert.IsFalse(animator.HasHidePending);
            Assert.IsFalse(animator.HasBoundsChangePending);
            Assert.AreEqual(AnimationContext.None, animator.SharedAnimationContext);

            animator.ShowCalls.Clear();
            animator.HideCalls.Clear();
            animator.BoundsChangeCalls.Clear();
            completed.Clear();
            animator.HasShowAnimationValue = true;
            animator.HasHideAnimationValue = false;
            animator.HasBoundsChangeAnimationValue = false;

            animator.OnElementShown(element, AnimationContext.CollectionChangeAdd);
            animator.OnElementHidden(element, AnimationContext.CollectionChangeRemove);
            animator.OnElementBoundsChanged(element, AnimationContext.LayoutTransition, oldBounds, newBounds);

            Assert.IsTrue(animator.HasShowPending);
            Assert.IsFalse(animator.HasHidePending);
            Assert.IsFalse(animator.HasBoundsChangePending);

            FlushRendering();

            Assert.AreEqual(1, animator.ShowCalls.Count);
            Assert.AreEqual(0, animator.HideCalls.Count);
            Assert.AreEqual(0, animator.BoundsChangeCalls.Count);
            CollectionAssert.AreEqual(new[] { "show" }, completed);
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

    private sealed class ElementAnimatorDerived : ElementAnimator
    {
        public bool HasShowAnimationValue { get; set; }

        public bool HasHideAnimationValue { get; set; }

        public bool HasBoundsChangeAnimationValue { get; set; }

        public List<AnimationCallInfo> ShowCalls { get; } = new();

        public List<AnimationCallInfo> HideCalls { get; } = new();

        public List<AnimationCallInfo> BoundsChangeCalls { get; } = new();

        public bool HasShowPending => HasShowAnimationsPending;

        public bool HasHidePending => HasHideAnimationsPending;

        public bool HasBoundsChangePending => HasBoundsChangeAnimationsPending;

        public AnimationContext SharedAnimationContext => SharedContext;

        protected override bool HasShowAnimationCore(UIElement element, AnimationContext context)
        {
            return HasShowAnimationValue;
        }

        protected override bool HasHideAnimationCore(UIElement element, AnimationContext context)
        {
            return HasHideAnimationValue;
        }

        protected override bool HasBoundsChangeAnimationCore(UIElement element, AnimationContext context, Rect oldBounds, Rect newBounds)
        {
            return HasBoundsChangeAnimationValue;
        }

        protected override void StartShowAnimation(UIElement element, AnimationContext context)
        {
            ShowCalls.Add(new AnimationCallInfo(element, context));
            OnShowAnimationCompleted(element);
        }

        protected override void StartHideAnimation(UIElement element, AnimationContext context)
        {
            HideCalls.Add(new AnimationCallInfo(element, context));
            OnHideAnimationCompleted(element);
        }

        protected override void StartBoundsChangeAnimation(UIElement element, AnimationContext context, Rect oldBounds, Rect newBounds)
        {
            BoundsChangeCalls.Add(new AnimationCallInfo(element, context, oldBounds, newBounds));
            OnBoundsChangeAnimationCompleted(element);
        }
    }

    private readonly struct AnimationCallInfo
    {
        public AnimationCallInfo(UIElement element, AnimationContext context)
        {
            Element = element;
            Context = context;
            OldBounds = default;
            NewBounds = default;
        }

        public AnimationCallInfo(UIElement element, AnimationContext context, Rect oldBounds, Rect newBounds)
        {
            Element = element;
            Context = context;
            OldBounds = oldBounds;
            NewBounds = newBounds;
        }

        public UIElement Element { get; }

        public AnimationContext Context { get; }

        public Rect OldBounds { get; }

        public Rect NewBounds { get; }
    }
}
