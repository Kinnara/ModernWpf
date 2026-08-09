// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class LinedFlowLayoutItemsInfoRequestedEventArgs
    {
        private readonly int _requestedStartIndex;
        private readonly int _requestedLength;
        private int _itemsRangeStartIndex;
        private int _establishedLength;
        private double _minWidth = -1.0;
        private double _maxWidth = -1.0;

        internal LinedFlowLayoutItemsInfoRequestedEventArgs(int itemsRangeStartIndex, int itemsRangeRequestedLength)
        {
            if (itemsRangeStartIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsRangeStartIndex));
            }

            if (itemsRangeRequestedLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsRangeRequestedLength));
            }

            _requestedStartIndex = itemsRangeStartIndex;
            _requestedLength = itemsRangeRequestedLength;
            _itemsRangeStartIndex = itemsRangeStartIndex;
        }

        public int ItemsRangeStartIndex
        {
            get => _itemsRangeStartIndex;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ItemsRangeStartIndex must be non-negative.");
                }

                if (value > _itemsRangeStartIndex)
                {
                    throw new ArgumentException("ItemsRangeStartIndex cannot be increased.", nameof(value));
                }

                if (_establishedLength != 0 && value + _establishedLength < _requestedStartIndex + _requestedLength)
                {
                    throw new ArgumentException("The supplied range no longer covers the requested items.", nameof(value));
                }

                _itemsRangeStartIndex = value;
            }
        }

        public int ItemsRangeRequestedLength => _requestedLength;

        public double MinWidth
        {
            get => _minWidth;
            set => _minWidth = value < 0.0 ? -1.0 : value;
        }

        public double MaxWidth
        {
            get => _maxWidth;
            set => _maxWidth = value < 0.0 ? -1.0 : value;
        }

        public void SetDesiredAspectRatios(double[] values)
        {
            DesiredAspectRatios = SetValues(values);
        }

        public void SetMinWidths(double[] values)
        {
            MinWidths = SetValues(values);
        }

        public void SetMaxWidths(double[] values)
        {
            MaxWidths = SetValues(values);
        }

        internal double[] DesiredAspectRatios { get; private set; }

        internal double[] MinWidths { get; private set; }

        internal double[] MaxWidths { get; private set; }

        internal int EstablishedLength => _establishedLength;

        private double[] SetValues(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            SetItemsRangeEstablishedLength(values.Length);
            return (double[])values.Clone();
        }

        private void SetItemsRangeEstablishedLength(int value)
        {
            if (value == _establishedLength)
            {
                return;
            }

            if (value < _requestedLength && _itemsRangeStartIndex == _requestedStartIndex)
            {
                throw new ArgumentException("The provided array must cover the requested range.", nameof(value));
            }

            if (_itemsRangeStartIndex + value < _requestedStartIndex + _requestedLength)
            {
                throw new ArgumentException("The provided array does not cover the requested range.", nameof(value));
            }

            if (_establishedLength > 0)
            {
                throw new ArgumentException("All provided arrays must have the same length.", nameof(value));
            }

            _establishedLength = value;
        }
    }
}
