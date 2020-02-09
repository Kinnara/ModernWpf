// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf;
using ModernWpf.Controls;

namespace Microsoft.UI.Private.Controls
{
    internal partial class RepeaterTestHooks
    {
        internal void NotifyBuildTreeCompletedImpl()
        {
            m_buildTreeCompleted?.Invoke(null, null);
        }

        // We removed index parameter from the GetElement call, which we used extensively for 
        // validation in tests. In order to avoid rewriting the tests, we keep the index internally and have 
        // a test hook to get it for validation in tests.
        public static int GetElementFactoryElementIndex(object getArgs)
        {
            var args = (ElementFactoryGetArgs)getArgs;
            return args.Index;
        }

        public static object CreateRepeaterElementFactoryGetArgs()
        {
            var instance = new ElementFactoryGetArgs();
            return instance;
        }

        public static object CreateRepeaterElementFactoryRecycleArgs()
        {
            var instance = new ElementFactoryRecycleArgs();
            return instance;
        }

        public static string GetLayoutId(object layout)
        {
            if (layout is Layout instance)
            {
                return instance.LayoutId;
            }

            return string.Empty;
        }

        public static void SetLayoutId(object layout, string id)
        {
            if (layout is Layout instance)
            {
                instance.LayoutId = id;
            }
        }

        private event TypedEventHandler<object, object> m_buildTreeCompleted;
    }
}
