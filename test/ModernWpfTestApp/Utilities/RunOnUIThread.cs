// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MUXControlsTestApp.Utilities
{
    public class RunOnUIThread
    {
        public static void Execute(Action action)
        {
            Execute(Application.Current.Dispatcher, action);
        }

        public static void Execute(Dispatcher dispatcher, Action action)
        {
            Exception exception = null;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                // We're not on the UI thread, queue the work. Make sure that the action is not run until
                // the splash screen is dismissed (i.e. that the window content is present).
                var workComplete = new AutoResetEvent(false);
                App.RunAfterSplashScreenDismissed(() =>
                {
                    // If the Splash screen dismissal happens on the UI thread, run the action right now.
                    if (dispatcher.CheckAccess())
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            exception = e;
                            throw;
                        }
                        finally // Unblock calling thread even if action() throws
                        {
                            workComplete.Set();
                        }
                    }
                    else
                    {
                        // Otherwise queue the work to the UI thread and then set the completion event on that thread.
                        var ignore = dispatcher.InvokeAsync(
                            () =>
                            {
                                try
                                {
                                    action();
                                }
                                catch (Exception e)
                                {
                                    exception = e;
                                    throw;
                                }
                                finally // Unblock calling thread even if action() throws
                                {
                                    workComplete.Set();
                                }
                            });
                    }
                });

                workComplete.WaitOne();
                if (exception != null)
                {
                    Verify.Fail("Exception thrown by action on the UI thread: " + exception.ToString());
                }
            }
        }

        public static void WaitForTick()
        {
            var renderingEvent = new ManualResetEvent(false);

            EventHandler renderingHandler = (object sender, EventArgs args) =>
            {
                renderingEvent.Set();
            };

            RunOnUIThread.Execute(() =>
            {
                CompositionTarget.Rendering += renderingHandler;
            });

            renderingEvent.WaitOne();

            RunOnUIThread.Execute(() =>
            {
                CompositionTarget.Rendering -= renderingHandler;
            });
        }

    }
}
