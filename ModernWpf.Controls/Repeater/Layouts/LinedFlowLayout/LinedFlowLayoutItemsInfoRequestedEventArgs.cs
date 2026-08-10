// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class LinedFlowLayoutItemsInfoRequestedEventArgs
    {
        internal LinedFlowLayoutItemsInfoRequestedEventArgs(int itemsRangeStartIndex, int itemsRangeRequestedLength)
        {
            _itemsRangeStartIndex = itemsRangeStartIndex;
            _itemsRangeRequestedStartIndex = itemsRangeStartIndex;
            ItemsRangeRequestedLength = itemsRangeRequestedLength;
        }

        public int ItemsRangeStartIndex
        {
            get => _itemsRangeStartIndex;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ItemsRangeStartIndex must be positive.");
                }

                if (value > _itemsRangeStartIndex)
                {
                    throw new ArgumentException("ItemsRangeStartIndex cannot be increased.", nameof(value));
                }

                if (_itemsRangeEstablishedLength != 0 &&
                    value + _itemsRangeEstablishedLength < _itemsRangeRequestedStartIndex + ItemsRangeRequestedLength)
                {
                    throw new ArgumentException("The value is too small for the array length already provided.", nameof(value));
                }

                _itemsRangeStartIndex = value;
            }
        }

        public int ItemsRangeRequestedLength { get; }

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
            SetItemsRangeEstablishedLength(GetLength(values));
            DesiredAspectRatios = (double[])values.Clone();
        }

        public void SetMinWidths(double[] values)
        {
            SetItemsRangeEstablishedLength(GetLength(values));
            MinWidths = (double[])values.Clone();
        }

        public void SetMaxWidths(double[] values)
        {
            SetItemsRangeEstablishedLength(GetLength(values));
            MaxWidths = (double[])values.Clone();
        }

        internal double[] DesiredAspectRatios { get; private set; }

        internal double[] MinWidths { get; private set; }

        internal double[] MaxWidths { get; private set; }

        internal int EstablishedLength => _itemsRangeEstablishedLength;

        private static int GetLength(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return values.Length;
        }

        private void SetItemsRangeEstablishedLength(int value)
        {
            if (value == _itemsRangeEstablishedLength)
            {
                return;
            }

            if (value < ItemsRangeRequestedLength && _itemsRangeStartIndex == _itemsRangeRequestedStartIndex)
            {
                throw new ArgumentException("The provided array length must cover ItemsRangeRequestedLength.");
            }

            if (_itemsRangeStartIndex + value < _itemsRangeRequestedStartIndex + ItemsRangeRequestedLength &&
                _itemsRangeStartIndex < _itemsRangeRequestedStartIndex)
            {
                throw new ArgumentException("The provided array is too small for the decreased ItemsRangeStartIndex.");
            }

            if (_itemsRangeEstablishedLength > 0)
            {
                throw new ArgumentException("All provided arrays must have the same length.");
            }

            _itemsRangeEstablishedLength = value;
        }

        private readonly int _itemsRangeRequestedStartIndex;
        private int _itemsRangeStartIndex;
        private int _itemsRangeEstablishedLength;
        private double _minWidth = -1.0;
        private double _maxWidth = -1.0;
    }
}
