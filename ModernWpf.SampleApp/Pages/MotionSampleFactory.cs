using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.SampleApp.Pages
{
    internal static class MotionSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "XamlCompInterop":
                    return CreateAnimationInteropSample();
                case "ConnectedAnimation":
                    return CreateConnectedAnimationSample();
                case "EasingFunction":
                    return CreateEasingFunctionSample();
                case "ImplicitTransition":
                    return CreateImplicitTransitionSample();
                case "PageTransition":
                    return CreatePageTransitionSample();
                case "ThemeTransition":
                    return CreateThemeTransitionSample();
                case "ParallaxView":
                    return CreateParallaxViewSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateAnimationInteropSample()
        {
            var panel = CreateSamplePanel("Animation interop maps to WPF property animations driven by XAML elements and transforms.");
            var targetTransform = new TranslateTransform();
            var followerTransform = new TranslateTransform();
            var output = CreateOutput("Target X: 0, follower animates to match.");

            var canvas = new Canvas
            {
                Width = 500,
                Height = 240,
                Background = CreateBrush("#F3F3F3"),
                ClipToBounds = true
            };
            canvas.Children.Add(new Rectangle
            {
                Width = 410,
                Height = 2,
                Fill = CreateBrush("#D0D0D0")
            });
            Canvas.SetLeft(canvas.Children[0], 45);
            Canvas.SetTop(canvas.Children[0], 120);

            var target = new Border
            {
                Width = 96,
                Height = 64,
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#0078D4"),
                RenderTransform = targetTransform,
                Child = new TextBlock
                {
                    Text = "XAML",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Canvas.SetLeft(target, 36);
            Canvas.SetTop(target, 72);

            var follower = new Border
            {
                Width = 72,
                Height = 72,
                CornerRadius = new CornerRadius(36),
                Background = CreateBrush("#8764B8"),
                Opacity = 0.86,
                RenderTransform = followerTransform,
                Child = new TextBlock
                {
                    Text = "FX",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Canvas.SetLeft(follower, 48);
            Canvas.SetTop(follower, 130);

            canvas.Children.Add(target);
            canvas.Children.Add(follower);

            var position = new Slider
            {
                Width = 360,
                Minimum = 0,
                Maximum = 330,
                Value = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(position, "Target offset");
            position.ValueChanged += delegate
            {
                var value = Math.Round(position.Value);
                targetTransform.X = value;
                followerTransform.BeginAnimation(
                    TranslateTransform.XProperty,
                    CreateAnimation(value, 500, new CubicEase { EasingMode = EasingMode.EaseOut }));
                output.Text = "Target X: " + value + ", follower animates to match.";
            };

            var commands = CreateCommandRow();
            var run = CreateButton("Run expression");
            run.Click += delegate { position.Value = position.Value < 160 ? 330 : 0; };
            commands.Children.Add(run);

            panel.Children.Add(canvas);
            panel.Children.Add(commands);
            panel.Children.Add(position);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateConnectedAnimationSample()
        {
            var panel = CreateSamplePanel("Connected animations preserve visual context by moving a shared element between list and detail surfaces.");
            var canvas = new Canvas
            {
                Width = 540,
                Height = 300,
                Background = CreateBrush("#F3F3F3"),
                ClipToBounds = true
            };
            var heroBrush = new ImageBrush(CreateBitmap(ResourceUri("Assets/SampleMedia/LandscapeImage1.jpg")))
            {
                Stretch = Stretch.UniformToFill
            };
            var hero = new Border
            {
                Width = 104,
                Height = 72,
                CornerRadius = new CornerRadius(8),
                Background = heroBrush,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 16,
                    ShadowDepth = 2,
                    Opacity = 0.22
                }
            };
            Canvas.SetLeft(hero, 24);
            Canvas.SetTop(hero, 56);
            Panel.SetZIndex(hero, 10);

            var detail = new Border
            {
                Width = 290,
                Height = 190,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Background = Brushes.White,
                Padding = new Thickness(18),
                Child = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Detail view",
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = "Click a thumbnail to animate it into this surface.",
                            Margin = new Thickness(0, 6, 0, 0),
                            TextWrapping = TextWrapping.Wrap,
                            Opacity = 0.72
                        }
                    }
                }
            };
            Canvas.SetLeft(detail, 220);
            Canvas.SetTop(detail, 56);
            canvas.Children.Add(detail);

            var images = new[]
            {
                "LandscapeImage1.jpg",
                "LandscapeImage5.jpg",
                "LandscapeImage9.jpg"
            };
            for (var i = 0; i < images.Length; i++)
            {
                var thumbnail = CreateThumbnail(images[i], i);
                var left = 24.0;
                var top = 24.0 + i * 86;
                Canvas.SetLeft(thumbnail, left);
                Canvas.SetTop(thumbnail, top);
                var imageName = images[i];
                var sourceLeft = left;
                var sourceTop = top;
                thumbnail.MouseLeftButtonUp += delegate
                {
                    heroBrush.ImageSource = CreateBitmap(ResourceUri("Assets/SampleMedia/" + imageName));
                    hero.BeginAnimation(Canvas.LeftProperty, null);
                    hero.BeginAnimation(Canvas.TopProperty, null);
                    hero.BeginAnimation(FrameworkElement.WidthProperty, null);
                    hero.BeginAnimation(FrameworkElement.HeightProperty, null);
                    Canvas.SetLeft(hero, sourceLeft);
                    Canvas.SetTop(hero, sourceTop);
                    hero.Width = 104;
                    hero.Height = 72;
                    hero.BeginAnimation(Canvas.LeftProperty, CreateAnimation(236, 620, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    hero.BeginAnimation(Canvas.TopProperty, CreateAnimation(76, 620, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    hero.BeginAnimation(FrameworkElement.WidthProperty, CreateAnimation(258, 620, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    hero.BeginAnimation(FrameworkElement.HeightProperty, CreateAnimation(142, 620, new CubicEase { EasingMode = EasingMode.EaseOut }));
                };
                canvas.Children.Add(thumbnail);
            }

            canvas.Children.Add(hero);
            panel.Children.Add(canvas);
            return panel;
        }

        private static UIElement CreateEasingFunctionSample()
        {
            var panel = CreateSamplePanel("Easing functions shape animation velocity while the target property remains the same.");
            var canvas = new Canvas
            {
                Width = 520,
                Height = 260,
                Background = CreateBrush("#F3F3F3")
            };
            var transforms = new List<TranslateTransform>();
            AddEasingRow(canvas, "Linear", CreateBrush("#0078D4"), 30, null, transforms);
            AddEasingRow(canvas, "Cubic ease", CreateBrush("#8764B8"), 82, new CubicEase { EasingMode = EasingMode.EaseInOut }, transforms);
            AddEasingRow(canvas, "Back ease", CreateBrush("#D13438"), 134, new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.55 }, transforms);
            AddEasingRow(canvas, "Bounce ease", CreateBrush("#107C10"), 186, new BounceEase { EasingMode = EasingMode.EaseOut, Bounces = 2, Bounciness = 2.2 }, transforms);

            var commands = CreateCommandRow();
            var run = CreateButton("Run");
            var reset = CreateButton("Reset");
            run.Click += delegate
            {
                foreach (var transform in transforms)
                {
                    transform.X = 0;
                    transform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(320, 900, transform.GetValue(EasingFunctionProperty) as IEasingFunction));
                }
            };
            reset.Click += delegate
            {
                foreach (var transform in transforms)
                {
                    transform.BeginAnimation(TranslateTransform.XProperty, null);
                    transform.X = 0;
                }
            };
            commands.Children.Add(run);
            commands.Children.Add(reset);

            panel.Children.Add(canvas);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreateImplicitTransitionSample()
        {
            var panel = CreateSamplePanel("Implicit transitions map to WPF animations that run when layout-affecting properties change.");
            var canvas = new Canvas
            {
                Width = 520,
                Height = 260,
                Background = CreateBrush("#F3F3F3"),
                ClipToBounds = true
            };
            var cards = new List<Border>();
            var nextCard = 1;
            for (var i = 0; i < 4; i++)
            {
                AddImplicitCard(canvas, cards, nextCard++);
            }
            LayoutImplicitCards(cards, false);

            var commands = CreateCommandRow();
            var add = CreateButton("Add");
            var remove = CreateButton("Remove");
            var shuffle = CreateButton("Shuffle");
            add.Click += delegate
            {
                if (cards.Count >= 6)
                {
                    return;
                }

                var card = AddImplicitCard(canvas, cards, nextCard++);
                var scale = card.RenderTransform as ScaleTransform;
                card.Opacity = 0;
                if (scale != null)
                {
                    scale.ScaleX = 0.86;
                    scale.ScaleY = 0.86;
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, CreateAnimation(1, 260, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, CreateAnimation(1, 260, new CubicEase { EasingMode = EasingMode.EaseOut }));
                }
                card.BeginAnimation(UIElement.OpacityProperty, CreateAnimation(1, 260, null));
                LayoutImplicitCards(cards, true);
            };
            remove.Click += delegate
            {
                if (cards.Count <= 1)
                {
                    return;
                }

                var card = cards[cards.Count - 1];
                cards.RemoveAt(cards.Count - 1);
                var fade = CreateAnimation(0, 220, null);
                fade.Completed += delegate { canvas.Children.Remove(card); };
                card.BeginAnimation(UIElement.OpacityProperty, fade);
                LayoutImplicitCards(cards, true);
            };
            shuffle.Click += delegate
            {
                if (cards.Count <= 1)
                {
                    return;
                }

                var first = cards[0];
                cards.RemoveAt(0);
                cards.Add(first);
                LayoutImplicitCards(cards, true);
            };
            commands.Children.Add(add);
            commands.Children.Add(remove);
            commands.Children.Add(shuffle);

            panel.Children.Add(canvas);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreatePageTransitionSample()
        {
            var panel = CreateSamplePanel("Page transitions animate content as navigation changes the current page.");
            var host = new Border
            {
                Width = 460,
                Height = 250,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Background = Brushes.White,
                ClipToBounds = true
            };
            var grid = new Grid();
            host.Child = grid;
            var pageIndex = 0;
            ShowTransitionPage(grid, pageIndex, 0);

            var output = CreateOutput("Page 1 of 3");
            var commands = CreateCommandRow();
            var previous = CreateButton("Previous");
            var next = CreateButton("Next");
            previous.Click += delegate
            {
                pageIndex = pageIndex == 0 ? 2 : pageIndex - 1;
                ShowTransitionPage(grid, pageIndex, -1);
                output.Text = "Page " + (pageIndex + 1) + " of 3";
            };
            next.Click += delegate
            {
                pageIndex = pageIndex == 2 ? 0 : pageIndex + 1;
                ShowTransitionPage(grid, pageIndex, 1);
                output.Text = "Page " + (pageIndex + 1) + " of 3";
            };
            commands.Children.Add(previous);
            commands.Children.Add(next);

            panel.Children.Add(host);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateThemeTransitionSample()
        {
            var panel = CreateSamplePanel("Theme transitions are represented by reusable entrance, reposition, and add-delete animations.");
            var canvas = new Canvas
            {
                Width = 520,
                Height = 260,
                Background = CreateBrush("#F3F3F3"),
                ClipToBounds = true
            };
            var tiles = new List<Border>();
            var positions = new[]
            {
                new Point(32, 36),
                new Point(166, 36),
                new Point(300, 36),
                new Point(32, 144),
                new Point(166, 144)
            };
            var colors = new[] { "#0078D4", "#8764B8", "#107C10", "#D83B01", "#5C2D91" };
            for (var i = 0; i < 4; i++)
            {
                var tile = CreateThemeTile("Tile " + (i + 1), colors[i]);
                Canvas.SetLeft(tile, positions[i].X);
                Canvas.SetTop(tile, positions[i].Y);
                canvas.Children.Add(tile);
                tiles.Add(tile);
            }

            var extraTile = CreateThemeTile("New tile", colors[4]);
            Canvas.SetLeft(extraTile, positions[4].X);
            Canvas.SetTop(extraTile, positions[4].Y);
            extraTile.Opacity = 0;
            var extraScale = extraTile.RenderTransform as ScaleTransform;
            if (extraScale != null)
            {
                extraScale.ScaleX = 0.86;
                extraScale.ScaleY = 0.86;
            }

            var reversed = false;
            var extraVisible = false;
            var commands = CreateCommandRow();
            var entrance = CreateButton("Entrance");
            var reposition = CreateButton("Reposition");
            var addDelete = CreateButton("Add/delete");
            entrance.Click += delegate
            {
                for (var i = 0; i < tiles.Count; i++)
                {
                    RunEntranceAnimation(tiles[i], i * 80);
                }
                if (extraVisible)
                {
                    RunEntranceAnimation(extraTile, tiles.Count * 80);
                }
            };
            reposition.Click += delegate
            {
                reversed = !reversed;
                for (var i = 0; i < tiles.Count; i++)
                {
                    var targetIndex = reversed ? tiles.Count - 1 - i : i;
                    tiles[i].BeginAnimation(Canvas.LeftProperty, CreateAnimation(positions[targetIndex].X, 360, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    tiles[i].BeginAnimation(Canvas.TopProperty, CreateAnimation(positions[targetIndex].Y, 360, new CubicEase { EasingMode = EasingMode.EaseOut }));
                }
            };
            addDelete.Click += delegate
            {
                if (!extraVisible)
                {
                    if (!canvas.Children.Contains(extraTile))
                    {
                        canvas.Children.Add(extraTile);
                    }
                    extraVisible = true;
                    extraTile.BeginAnimation(UIElement.OpacityProperty, CreateAnimation(1, 260, null));
                    var scale = extraTile.RenderTransform as ScaleTransform;
                    if (scale != null)
                    {
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, CreateAnimation(1, 260, new CubicEase { EasingMode = EasingMode.EaseOut }));
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, CreateAnimation(1, 260, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    }
                }
                else
                {
                    extraVisible = false;
                    var fade = CreateAnimation(0, 220, null);
                    fade.Completed += delegate { canvas.Children.Remove(extraTile); };
                    extraTile.BeginAnimation(UIElement.OpacityProperty, fade);
                }
            };
            commands.Children.Add(entrance);
            commands.Children.Add(reposition);
            commands.Children.Add(addDelete);

            panel.Children.Add(canvas);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreateParallaxViewSample()
        {
            var panel = CreateSamplePanel("ParallaxView maps to a scroll-linked transform where background media moves at a different rate than content.");
            var backgroundTransform = new TranslateTransform();
            var background = new Image
            {
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/rainier.jpg")),
                Stretch = Stretch.UniformToFill,
                Width = 500,
                Height = 390,
                VerticalAlignment = VerticalAlignment.Top,
                RenderTransform = backgroundTransform
            };
            var content = new Grid
            {
                Width = 500,
                Height = 760
            };
            content.Children.Add(background);

            var cards = new StackPanel
            {
                Margin = new Thickness(28, 190, 28, 28)
            };
            cards.Children.Add(CreateParallaxCard("Header content", "The hero image travels more slowly than the foreground content."));
            cards.Children.Add(CreateParallaxCard("Scroll-linked offset", "WPF ScrollChanged updates a TranslateTransform on the background layer."));
            cards.Children.Add(CreateParallaxCard("Foreground list", "Cards remain readable while the media layer provides depth."));
            cards.Children.Add(CreateParallaxCard("Composition mapping", "The WinUI ParallaxView behavior is represented with standard WPF scrolling primitives."));
            content.Children.Add(cards);

            var scrollViewer = new ScrollViewer
            {
                Width = 500,
                Height = 340,
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            var progress = new ProgressBar
            {
                Width = 300,
                Height = 8,
                Minimum = 0,
                Maximum = 1,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(progress, "Scroll progress");
            scrollViewer.ScrollChanged += delegate
            {
                backgroundTransform.Y = scrollViewer.VerticalOffset * 0.36;
                var max = Math.Max(1, scrollViewer.ScrollableHeight);
                progress.Value = scrollViewer.VerticalOffset / max;
            };

            panel.Children.Add(scrollViewer);
            panel.Children.Add(progress);
            return panel;
        }

        private static Border CreateThumbnail(string imageName, int index)
        {
            var border = new Border
            {
                Width = 104,
                Height = 72,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.White,
                Background = new ImageBrush(CreateBitmap(ResourceUri("Assets/SampleMedia/" + imageName)))
                {
                    Stretch = Stretch.UniformToFill
                },
                ToolTip = "Animate image " + (index + 1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            border.Child = new TextBlock
            {
                Text = (index + 1).ToString(),
                Foreground = Brushes.White,
                Background = CreateBrush("#99000000"),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            return border;
        }

        private static void AddEasingRow(Canvas canvas, string name, Brush brush, double top, IEasingFunction easing, ICollection<TranslateTransform> transforms)
        {
            var label = new TextBlock
            {
                Text = name,
                Width = 92,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(label, 24);
            Canvas.SetTop(label, top + 6);
            canvas.Children.Add(label);

            var line = new Rectangle
            {
                Width = 330,
                Height = 2,
                Fill = CreateBrush("#D0D0D0")
            };
            Canvas.SetLeft(line, 130);
            Canvas.SetTop(line, top + 17);
            canvas.Children.Add(line);

            var transform = new TranslateTransform();
            transform.SetValue(EasingFunctionProperty, easing);
            var dot = new Ellipse
            {
                Width = 28,
                Height = 28,
                Fill = brush,
                RenderTransform = transform
            };
            Canvas.SetLeft(dot, 124);
            Canvas.SetTop(dot, top + 4);
            canvas.Children.Add(dot);
            transforms.Add(transform);
        }

        private static Border AddImplicitCard(Canvas canvas, IList<Border> cards, int number)
        {
            var card = new Border
            {
                Width = 128,
                Height = 68,
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#FFFFFF"),
                BorderBrush = CreateBrush("#D8D8D8"),
                BorderThickness = new Thickness(1),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1),
                Child = new TextBlock
                {
                    Text = "Card " + number,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            cards.Add(card);
            canvas.Children.Add(card);
            return card;
        }

        private static void LayoutImplicitCards(IList<Border> cards, bool animate)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var left = 26 + (i % 3) * 156;
                var top = 32 + (i / 3) * 96;
                if (animate)
                {
                    cards[i].BeginAnimation(Canvas.LeftProperty, CreateAnimation(left, 340, new CubicEase { EasingMode = EasingMode.EaseOut }));
                    cards[i].BeginAnimation(Canvas.TopProperty, CreateAnimation(top, 340, new CubicEase { EasingMode = EasingMode.EaseOut }));
                }
                else
                {
                    Canvas.SetLeft(cards[i], left);
                    Canvas.SetTop(cards[i], top);
                }
            }
        }

        private static void ShowTransitionPage(Grid host, int index, int direction)
        {
            host.Children.Clear();
            var page = CreateTransitionPage(index);
            var transform = new TranslateTransform(direction * 54, 0);
            page.RenderTransform = transform;
            page.Opacity = direction == 0 ? 1 : 0;
            host.Children.Add(page);

            if (direction != 0)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(0, 360, new CubicEase { EasingMode = EasingMode.EaseOut }));
                page.BeginAnimation(UIElement.OpacityProperty, CreateAnimation(1, 260, null));
            }
        }

        private static Grid CreateTransitionPage(int index)
        {
            var imageNames = new[] { "LandscapeImage2.jpg", "LandscapeImage6.jpg", "LandscapeImage12.jpg" };
            var titles = new[] { "Overview", "Details", "Confirmation" };
            var colors = new[] { "#0078D4", "#8764B8", "#107C10" };
            var grid = new Grid
            {
                Background = Brushes.White
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var image = new Border
            {
                Margin = new Thickness(18),
                CornerRadius = new CornerRadius(8),
                Background = new ImageBrush(CreateBitmap(ResourceUri("Assets/SampleMedia/" + imageNames[index])))
                {
                    Stretch = Stretch.UniformToFill
                }
            };
            Grid.SetColumn(image, 0);

            var copy = new StackPanel
            {
                Margin = new Thickness(8, 34, 30, 28)
            };
            copy.Children.Add(new TextBlock
            {
                Text = titles[index],
                FontSize = 28,
                FontWeight = FontWeights.SemiBold
            });
            copy.Children.Add(new Rectangle
            {
                Width = 52,
                Height = 4,
                RadiusX = 2,
                RadiusY = 2,
                Fill = CreateBrush(colors[index]),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 16)
            });
            copy.Children.Add(new TextBlock
            {
                Text = "NavigationThemeTransition-style motion helps communicate where the user is moving in the workflow.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });
            Grid.SetColumn(copy, 1);
            grid.Children.Add(image);
            grid.Children.Add(copy);
            return grid;
        }

        private static Border CreateThemeTile(string text, string color)
        {
            return new Border
            {
                Width = 106,
                Height = 78,
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush(color),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static void RunEntranceAnimation(UIElement element, int delay)
        {
            element.Opacity = 0;
            var transform = element.RenderTransform as ScaleTransform;
            if (transform != null)
            {
                transform.ScaleX = 0.92;
                transform.ScaleY = 0.92;
                var scaleX = CreateAnimation(1, 320, new CubicEase { EasingMode = EasingMode.EaseOut });
                var scaleY = CreateAnimation(1, 320, new CubicEase { EasingMode = EasingMode.EaseOut });
                scaleX.BeginTime = TimeSpan.FromMilliseconds(delay);
                scaleY.BeginTime = TimeSpan.FromMilliseconds(delay);
                transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
                transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            }

            var fade = CreateAnimation(1, 320, null);
            fade.BeginTime = TimeSpan.FromMilliseconds(delay);
            element.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private static Border CreateParallaxCard(string title, string body)
        {
            var card = new Border
            {
                Margin = new Thickness(0, 0, 0, 14),
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#F8FFFFFF"),
                BorderBrush = CreateBrush("#D8D8D8"),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 18,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = body,
                            Margin = new Thickness(0, 6, 0, 0),
                            TextWrapping = TextWrapping.Wrap,
                            Opacity = 0.72
                        }
                    }
                }
            };
            return card;
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

        private static DoubleAnimation CreateAnimation(double to, int milliseconds, IEasingFunction easing)
        {
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(milliseconds)
            };
            if (easing != null)
            {
                animation.EasingFunction = easing;
            }
            return animation;
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

        private static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.RegisterAttached(
                "EasingFunction",
                typeof(IEasingFunction),
                typeof(MotionSampleFactory),
                new PropertyMetadata(null));
    }
}
