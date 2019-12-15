// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
namespace ModernWpfTestApp
{
    public class TopLevelTestPageAttribute : Attribute
    {
        public string Name { get; set; } = "NoName";

        public string Icon { get; set; } = "DefaultIcon.png";
    }
}