//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ListBoxPage
    {
        private List<Tuple<string, FontFamily>> _fonts = new List<Tuple<string, FontFamily>>()
        {
            new Tuple<string, FontFamily>("Arial", new FontFamily("Arial")),
            new Tuple<string, FontFamily>("Comic Sans MS", new FontFamily("Comic Sans MS")),
            new Tuple<string, FontFamily>("Courier New", new FontFamily("Courier New")),
            new Tuple<string, FontFamily>("Segoe UI", new FontFamily("Segoe UI")),
            new Tuple<string, FontFamily>("Times New Roman", new FontFamily("Times New Roman"))
        };

        public List<Tuple<string, FontFamily>> Fonts
        {
            get { return _fonts; }
        }

        public ListBoxPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
