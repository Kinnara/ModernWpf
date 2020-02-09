// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf;

namespace Microsoft.UI.Private.Controls
{
    partial class RepeaterTestHooks
    {
        private static RepeaterTestHooks s_testHooks;

        static void EnsureHooks()
        {
            if (s_testHooks == null)
            {
                s_testHooks = new RepeaterTestHooks();
            }
        }

        public static event TypedEventHandler<object, object> BuildTreeCompleted
        {
            add
            {
                EnsureHooks();
                s_testHooks.m_buildTreeCompleted += value;
            }
            remove
            {
                if (s_testHooks != null)
                {
                    s_testHooks.m_buildTreeCompleted -= value;
                }
            }
        }

        static void NotifyBuildTreeCompleted()
        {
            if (s_testHooks != null)
            {
                s_testHooks.NotifyBuildTreeCompletedImpl();
            }
        }
    }
}
