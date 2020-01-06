// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Resources;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides
    /// <see cref="T:ModernWpf.Controls.ITransition"/>s
    /// for transition families and modes.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    internal static class Transitions
    {
        /// <summary>
        /// The cached XAML read from the Storyboard resources.
        /// </summary>
        private static Dictionary<string, string> _storyboardXamlCache;

        /// <summary>
        /// Creates a
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// for a transition family, transition mode, and
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        /// <typeparam name="T">The type of the transition mode.</typeparam>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="name">The transition family.</param>
        /// <param name="mode">The transition mode.</param>
        /// <returns>The <see cref="T:ModernWpf.Controls.ITransition"/>.</returns>
        private static ITransition GetEnumStoryboard<T>(UIElement element, string name, T mode)
        {
            string key = name + Enum.GetName(typeof(T), mode);
            Storyboard storyboard = GetStoryboard(key);
            if (storyboard == null)
            {
                return null;
            }
            Storyboard.SetTarget(storyboard, element);
            return new Transition(element, storyboard);
        }

        /// <summary>
        /// Creates a
        /// <see cref="T:System.Windows.Media.Storyboard"/>
        /// for a particular transition family and transition mode.
        /// </summary>
        /// <param name="name">The transition family and transition mode.</param>
        /// <returns>The <see cref="T:System.Windows.Media.Storyboard"/>.</returns>
        private static Storyboard GetStoryboard(string name)
        {
            if (_storyboardXamlCache == null)
            {
                _storyboardXamlCache = new Dictionary<string, string>();
            }
            string xaml = null;
            if (_storyboardXamlCache.ContainsKey(name))
            {
                xaml = _storyboardXamlCache[name];
            }
            else
            {
                string path = "/ModernWpf;component/Transitions/Storyboards/" + name + ".xaml";
                Uri uri = new Uri(path, UriKind.Relative);
                StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
                using (StreamReader streamReader = new StreamReader(streamResourceInfo.Stream))
                {
                    xaml = streamReader.ReadToEnd();
                    _storyboardXamlCache[name] = xaml;
                }
            }
            return XamlReader.Parse(xaml) as Storyboard;
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the slide transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="slideTransitionMode">The transition mode.</param>
        /// <returns>The <see cref="T:ModernWpf.Controls.ITransition"/>.</returns>
        public static ITransition Slide(UIElement element, SlideTransitionMode slideTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(SlideTransitionMode), slideTransitionMode))
            {
                throw new ArgumentOutOfRangeException("slideTransitionMode");
            }
            element.RenderTransform = new TranslateTransform();
            return GetEnumStoryboard(element, string.Empty, slideTransitionMode);
        }

        public static ITransition Fade(UIElement element, FadeTransitionMode fadeTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!Enum.IsDefined(typeof(FadeTransitionMode), fadeTransitionMode))
            {
                throw new ArgumentOutOfRangeException(nameof(fadeTransitionMode));
            }
            return GetEnumStoryboard(element, string.Empty, fadeTransitionMode);
        }

        public static ITransition Drill(UIElement element, DrillTransitionMode drillTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!Enum.IsDefined(typeof(DrillTransitionMode), drillTransitionMode))
            {
                throw new ArgumentOutOfRangeException(nameof(drillTransitionMode));
            }
            element.RenderTransform = new ScaleTransform();
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            return GetEnumStoryboard(element, string.Empty, drillTransitionMode);
        }
    }
}