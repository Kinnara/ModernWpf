// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public partial class RatingItemImageInfo : RatingItemInfo
    {
        public RatingItemImageInfo()
        {
        }

        protected override Freezable CreateInstanceCore()
        {
            return new RatingItemImageInfo();
        }
    }
}
