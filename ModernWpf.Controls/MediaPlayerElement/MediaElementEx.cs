using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    public class MediaElementEx : MediaElement, INotifyPropertyChanged
    {
        private DispatcherTimer timer;

        public event RoutedEventHandler MediaPlay;
        public event RoutedEventHandler MediaPause;
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MediaElementEx()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
            timer.Tick += timer_Tick;

            Loaded += (ss, ee) =>
            {
                Play();
                if (!AutoPlay)
                {
                    Pause();
                    Position = TimeSpan.FromMilliseconds(1);
                }
            };

            Unloaded += (ss, ee) => timer.Stop();

            MediaOpened += (ss, ee) =>
            {
                //触发PropertyChanged DurationTime
                RaisePropertyChangedEvent(nameof(DurationTime));
                RaisePropertyChangedEvent(nameof(DurationTimeString));
                timer.Start();
                IsOpening = false;
            };

            //发生错误和视频播放完毕 停止计时器
            MediaEnded += async (ss, ee) =>
            {
                await Task.Delay(1000 / 60);
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
                CurrentState = MediaState.Pause;
            };

            MediaFailed += async (ss, ee) =>
            {
                await Task.Delay(1000 / 60);
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
                CurrentState = MediaState.Error;
            };
        }

        #region AutoPlay

        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register(
                nameof(AutoPlay),
                typeof(bool),
                typeof(MediaElementEx),
                new PropertyMetadata(false));

        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        #endregion

        #region IsOpening

        public static readonly DependencyProperty IsOpeningProperty =
            DependencyProperty.Register(
                nameof(IsOpening),
                typeof(bool),
                typeof(MediaElementEx),
                new PropertyMetadata(true));

        public bool IsOpening
        {
            get => (bool)GetValue(IsOpeningProperty);
            private set => SetValue(IsOpeningProperty, value);
        }

        #endregion

        #region IsRepeat

        public static readonly DependencyProperty IsRepeatProperty =
            DependencyProperty.Register(
                nameof(IsRepeat),
                typeof(bool),
                typeof(MediaElementEx),
                new PropertyMetadata(false));

        public bool IsRepeat
        {
            get => (bool)GetValue(IsRepeatProperty);
            set => SetValue(IsRepeatProperty, value);
        }

        #endregion

        #region CurrentState

        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register(
                nameof(CurrentState),
                typeof(MediaState),
                typeof(MediaElementEx),
                new PropertyMetadata(MediaState.Stop));

        public MediaState CurrentState
        {
            get => (MediaState)GetValue(CurrentStateProperty);
            private set => SetValue(CurrentStateProperty, value);
        }

        #endregion

        #region CurrentTime

        /// <summary>
        /// 当前播放进度
        /// </summary>
        public double CurrentTime
        {
            get => Position.TotalMilliseconds;
            set
            {
                //进度条拖动太频繁太久，性能跟不上，所以设置一个时间阀，跳过某些时段
                if ((DateTime.Now - _lastChangedTime).TotalMilliseconds > 50)
                {
                    Position = TimeSpan.FromMilliseconds(value);
                    _lastChangedTime = DateTime.Now;
                    if (!timer.IsEnabled)
                    {
                        timer.Start();
                    }
                }
            }
        }

        /// <summary>
        /// 当前播放时间
        /// </summary>
        public string CurrentTimeString
        {
            get
            {
                var ts = Position;
                return string.Format("{0:0}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        #endregion

        #region DurationTime

        /// <summary>
        /// 记录最后修改进度的时间，
        /// </summary>
        private DateTime _lastChangedTime = DateTime.Now;

        /// <summary>
        /// 当前视频时长
        /// </summary>
        public double DurationTime => NaturalDuration.HasTimeSpan ? NaturalDuration.TimeSpan.TotalMilliseconds : double.MinValue;

        /// <summary>
        /// 视频时长时间
        /// </summary>
        public string DurationTimeString
        {
            get
            {
                if (NaturalDuration.HasTimeSpan)
                {
                    var ts = NaturalDuration.TimeSpan;
                    return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                }
                return string.Format("--:--:--");
            }
        }

        #endregion

        #region LeftTime

        /// <summary>
        /// 当前剩余时间
        /// </summary>
        public double LeftTime => DurationTime - CurrentTime;

        /// <summary>
        /// 当前剩余时间
        /// </summary>
        public string LeftTimeString
        {
            get
            {
                if (NaturalDuration.HasTimeSpan)
                {
                    if (LeftTime == 0)
                    {
                        CurrentState = MediaState.Pause;
                        MediaPause?.Invoke(this, new RoutedEventArgs());
                        if (IsRepeat)
                        {
                            Task.Run(async () =>
                            {
                                await Task.Delay(1000 / 60);
                                this.RunOnUIThread(async () =>
                                {
                                    Position = TimeSpan.FromMilliseconds(0);
                                    await Task.Delay(1000 / 60);
                                    Play();
                                });
                            });
                        }
                    }
                    var ts = TimeSpan.FromMilliseconds(LeftTime);
                    return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                }
                return string.Format("--:--:--");
            }
        }

        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            //定时触发PropertyChanged CurrentTime
            RaisePropertyChangedEvent(nameof(CurrentTime));
            RaisePropertyChangedEvent(nameof(CurrentTimeString));
            //定时触发PropertyChanged LeftTime
            RaisePropertyChangedEvent(nameof(LeftTime));
            RaisePropertyChangedEvent(nameof(LeftTimeString));
        }

        public void StartTimer()
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            UpdateAllProperty();
        }

        public new void Play()
        {
            base.Play();
            CurrentState = MediaState.Play;
            MediaPlay?.Invoke(this, new RoutedEventArgs());
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            UpdateAllProperty();
        }

        public new void Pause()
        {
            base.Pause();
            CurrentState = MediaState.Pause;
            MediaPause?.Invoke(this, new RoutedEventArgs());
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            UpdateAllProperty();
        }

        public new void Stop()
        {
            base.Stop();
            CurrentState = MediaState.Pause;
            MediaPause?.Invoke(this, new RoutedEventArgs());
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            UpdateAllProperty();
        }

        public new void Close()
        {
            base.Close();
            CurrentState = MediaState.Pause;
            MediaPause?.Invoke(this, new RoutedEventArgs());
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            UpdateAllProperty();
        }

        public void UpdateAllProperty()
        {
            //触发PropertyChanged DurationTime
            RaisePropertyChangedEvent(nameof(DurationTime));
            RaisePropertyChangedEvent(nameof(DurationTimeString));
            //定时触发PropertyChanged CurrentTime
            RaisePropertyChangedEvent(nameof(CurrentTime));
            RaisePropertyChangedEvent(nameof(CurrentTimeString));
            //定时触发PropertyChanged LeftTime
            RaisePropertyChangedEvent(nameof(LeftTime));
            RaisePropertyChangedEvent(nameof(LeftTimeString));
        }
    }

    public enum MediaState
    {
        Stop,
        Error,
        Play,
        Pause
    }
}
