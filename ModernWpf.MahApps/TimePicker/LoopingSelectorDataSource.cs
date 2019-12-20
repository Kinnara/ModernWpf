using System;
using System.Collections;
using System.Collections.Generic;

namespace ModernWpf.MahApps
{
    internal class LoopingSelectorDataSource : IList
    {
        private const int RepeatCount = 1000;

        private readonly List<int> _source;

        public LoopingSelectorDataSource(IEnumerable<int> source)
        {
            _source = new List<int>(source);
        }

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _source[index % _source.Count];
            }
            set => throw new NotImplementedException();
        }

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public int Count => _source.Count * RepeatCount;

        public int SourceCount => _source.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            if (value is int item)
            {
                return _source.Contains(item);
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _source[i % _source.Count];
            }
        }

        public int IndexOf(object value)
        {
            int index = -1;

            if (value is int item)
            {
                index = _source.IndexOf(item);
            }

            if (index > -1)
            {
                index += _source.Count * (RepeatCount / 2);
            }

            return index;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
