// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace ModernWpf.Controls
{
    public sealed class IndexPath
    {
        public IndexPath(int index)
        {
            m_path.Add(index);
        }

        public IndexPath(int groupIndex, int itemIndex)
        {
            m_path.Add(groupIndex);
            m_path.Add(itemIndex);
        }

        public IndexPath(IList<int> indices)
        {
            if (indices != null)
            {
                for (var i = 0; i < indices.Count; i++)
                {
                    m_path.Add(indices[i]);
                }
            }
        }

        public static IndexPath CreateFrom(int index)
        {
            return new IndexPath(index);
        }

        public static IndexPath CreateFrom(int groupIndex, int itemIndex)
        {
            return new IndexPath(groupIndex, itemIndex);
        }

        public static IndexPath CreateFromIndices(IList<int> indices)
        {
            return new IndexPath(indices);
        }

        public int GetSize()
        {
            return m_path.Count;
        }

        public int GetAt(int index)
        {
            return m_path[index];
        }

        public int CompareTo(IndexPath other)
        {
            var rhsPath = other;
            int compareResult = 0;
            int lhsCount = m_path.Count;
            int rhsCount = rhsPath.m_path.Count;

            if (lhsCount == 0 || rhsCount == 0)
            {
                // one of the paths are empty, compare based on size
                compareResult = (lhsCount - rhsCount);
            }
            else
            {
                // both paths are non-empty, but can be of different size
                for (int i = 0; i < Math.Min(lhsCount, rhsCount); i++)
                {
                    if (m_path[i] < rhsPath.m_path[i])
                    {
                        compareResult = -1;
                        break;
                    }
                    else if (m_path[i] > rhsPath.m_path[i])
                    {
                        compareResult = 1;
                        break;
                    }
                }

                // if both match upto min(lhsCount, rhsCount), compare based on size
                compareResult = compareResult == 0 ? (lhsCount - rhsCount) : compareResult;
            }

            if (compareResult != 0)
            {
                compareResult = compareResult > 0 ? 1 : -1;
            }

            return compareResult;
        }

        public override string ToString()
        {
            string result = "R";
            foreach (int index in m_path)
            {
                result += ".";
                result += index;
            }

            return result;
        }

        internal bool IsValid()
        {
            bool isValid = true;
            for (int i = 0; i < m_path.Count; i++)
            {
                if (m_path[i] < 0)
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        internal IndexPath CloneWithChildIndex(int childIndex)
        {
            var newPath = new List<int>(m_path);
            newPath.Add(childIndex);
            return new IndexPath(newPath);
        }

        private readonly List<int> m_path = new List<int>();
    }
}
