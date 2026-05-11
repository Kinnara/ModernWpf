using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class SwipeItem : DependencyObject
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(SwipeItem),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(SwipeItem),
                new PropertyMetadata(null));

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(
                nameof(Background),
                typeof(Brush),
                typeof(SwipeItem),
                new PropertyMetadata(null));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(
                nameof(Foreground),
                typeof(Brush),
                typeof(SwipeItem),
                new PropertyMetadata(null));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(SwipeItem),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(SwipeItem),
                new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty BehaviorOnInvokedProperty =
            DependencyProperty.Register(
                nameof(BehaviorOnInvoked),
                typeof(SwipeBehaviorOnInvoked),
                typeof(SwipeItem),
                new PropertyMetadata(SwipeBehaviorOnInvoked.Auto));

        public SwipeBehaviorOnInvoked BehaviorOnInvoked
        {
            get => (SwipeBehaviorOnInvoked)GetValue(BehaviorOnInvokedProperty);
            set => SetValue(BehaviorOnInvokedProperty, value);
        }

        public event TypedEventHandler<SwipeItem, SwipeItemInvokedEventArgs> Invoked;

        internal void Invoke(SwipeControl swipeControl)
        {
            Invoked?.Invoke(this, new SwipeItemInvokedEventArgs(swipeControl));

            var command = Command;
            var parameter = CommandParameter;
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
            }

            if (BehaviorOnInvoked == SwipeBehaviorOnInvoked.Auto ||
                BehaviorOnInvoked == SwipeBehaviorOnInvoked.Close)
            {
                swipeControl.Close();
            }
        }
    }
}
