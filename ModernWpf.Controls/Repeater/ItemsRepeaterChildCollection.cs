// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class ItemsRepeaterChildCollection : IList
    {
        public ItemsRepeaterChildCollection(UIElement visualParent, ItemsRepeaterPanel logicalParent)
        {
            if (visualParent == null)
            {
                throw new ArgumentNullException(string.Format("'{0}' must be provided when instantiating '{1}'.", "visualParent", GetType()));
            }

            _visualChildren = new VisualCollection(visualParent);
            _visualParent = visualParent;
            _logicalParent = logicalParent;
        }

        public virtual int Count
        {
            get { return _visualChildren.Count; }
        }

        public virtual bool IsSynchronized
        {
            get { return _visualChildren.IsSynchronized; }
        }

        public virtual object SyncRoot
        {
            get { return _visualChildren.SyncRoot; }
        }

        public virtual void CopyTo(Array array, int index)
        {
            _visualChildren.CopyTo(array, index);
        }

        public virtual void CopyTo(UIElement[] array, int index)
        {
            _visualChildren.CopyTo(array, index);
        }

        public virtual int Capacity
        {
            get { return _visualChildren.Capacity; }
            set
            {
                _visualChildren.Capacity = value;
            }
        }

        public virtual UIElement this[int index]
        {
            get { return _visualChildren[index] as UIElement; }
            set
            {
                ValidateElement(value);

                VisualCollection vc = _visualChildren;

                //if setting new element into slot or assigning null, 
                //remove previously hooked element from the logical tree
                if (vc[index] != value)
                {
                    UIElement e = vc[index] as UIElement;
                    if (e != null)
                        ClearLogicalParent(e);

                    vc[index] = value;

                    SetLogicalParent(value);

                    //_visualParent.InvalidateMeasure();
                }
            }
        }

        internal void SetInternal(int index, UIElement item)
        {
            ValidateElement(item);

            VisualCollection vc = _visualChildren;

            if (vc[index] != item)
            {
                vc[index] = null; // explicitly disconnect the existing visual;
                vc[index] = item;

                //_visualParent.InvalidateMeasure();
            }
        }

        public virtual int Add(UIElement element)
        {
            return AddInternal(element);
        }

        internal int AddInternal(UIElement element)
        {
            ValidateElement(element);

            SetLogicalParent(element);
            int retVal = _visualChildren.Add(element);

            // invalidate measure on visual parent
            //_visualParent.InvalidateMeasure();

            return retVal;
        }

        public virtual int IndexOf(UIElement element)
        {
            return _visualChildren.IndexOf(element);
        }

        public bool IndexOf(UIElement element, out int index)
        {
            int i = IndexOf(element);
            if (i >= 0)
            {
                index = i;
                return true;
            }
            else
            {
                index = 0;
                return false;
            }
        }

        public virtual void Remove(UIElement element)
        {
            RemoveInternal(element);
        }

        internal void RemoveInternal(UIElement element)
        {
            _visualChildren.Remove(element);
            ClearLogicalParent(element);
            //_visualParent.InvalidateMeasure();
        }

        internal virtual void RemoveNoVerify(UIElement element)
        {
            _visualChildren.Remove(element);
        }

        public virtual bool Contains(UIElement element)
        {
            return _visualChildren.Contains(element);
        }

        public virtual void Clear()
        {
            ClearInternal();
        }

        internal void ClearInternal()
        {
            VisualCollection vc = _visualChildren;
            int cnt = vc.Count;

            if (cnt > 0)
            {
                // copy children in VisualCollection so that we can clear the visual link first, 
                // followed by the logical link
                Visual[] visuals = new Visual[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    visuals[i] = vc[i];
                }

                vc.Clear();

                //disconnect from logical tree
                for (int i = 0; i < cnt; i++)
                {
                    UIElement e = visuals[i] as UIElement;
                    if (e != null)
                    {
                        ClearLogicalParent(e);
                    }
                }

                //_visualParent.InvalidateMeasure();
            }
        }

        public virtual void Insert(int index, UIElement element)
        {
            InsertInternal(index, element);
        }

        internal void InsertInternal(int index, UIElement element)
        {
            ValidateElement(element);

            SetLogicalParent(element);
            _visualChildren.Insert(index, element);
            //_visualParent.InvalidateMeasure();
        }

        public virtual void RemoveAt(int index)
        {
            VisualCollection vc = _visualChildren;

            //disconnect from logical tree
            UIElement e = vc[index] as UIElement;

            vc.RemoveAt(index);

            if (e != null)
                ClearLogicalParent(e);

            //_visualParent.InvalidateMeasure();
        }

        public virtual void RemoveRange(int index, int count)
        {
            RemoveRangeInternal(index, count);
        }

        internal void RemoveRangeInternal(int index, int count)
        {
            VisualCollection vc = _visualChildren;
            int cnt = vc.Count;
            if (count > cnt - index)
            {
                count = cnt - index;
            }

            if (count > 0)
            {
                // copy children in VisualCollection so that we can clear the visual link first, 
                // followed by the logical link
                Visual[] visuals = new Visual[count];
                int i = index;
                for (int loop = 0; loop < count; i++, loop++)
                {
                    visuals[loop] = vc[i];
                }

                vc.RemoveRange(index, count);

                //disconnect from logical tree
                for (i = 0; i < count; i++)
                {
                    UIElement e = visuals[i] as UIElement;
                    if (e != null)
                    {
                        ClearLogicalParent(e);
                    }
                }

                //_visualParent.InvalidateMeasure();
            }
        }

        private UIElement Cast(object value)
        {
            if (value == null)
                throw new ArgumentException(string.Format("Cannot add null to a collection of type '{0}'.", "ItemsRepeaterChildCollection"));

            UIElement element = value as UIElement;

            if (element == null)
                throw new ArgumentException(string.Format("Cannot add instance of type '{1}' to a collection of type '{0}'. Only items of type '{2}' are allowed.", "ItemsRepeaterChildCollection", value.GetType().Name, "UIElement"));

            return element;
        }

        #region IList Members

        int IList.Add(object value)
        {
            return Add(Cast(value));
        }

        bool IList.Contains(object value)
        {
            return Contains(value as UIElement);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf(value as UIElement);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, Cast(value));
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            Remove(value as UIElement);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = Cast(value);
            }
        }

        #endregion

        public virtual IEnumerator GetEnumerator()
        {
            return _visualChildren.GetEnumerator();
        }

        protected void SetLogicalParent(UIElement element)
        {
            if (_logicalParent != null)
            {
                _logicalParent.AddLogicalChildInternal(element);
            }
        }

        protected void ClearLogicalParent(UIElement element)
        {
            if (_logicalParent != null)
            {
                _logicalParent.RemoveLogicalChildInternal(element);
            }
        }

        internal UIElement VisualParent
        {
            get { return _visualParent; }
        }

        private void ValidateElement(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(string.Format("Children of '{0}' cannot be null. Object derived from UIElement expected.", GetType()));
            }
        }

        internal FrameworkElement LogicalParent
        {
            get { return _logicalParent; }
        }

        private readonly VisualCollection _visualChildren;
        private readonly UIElement _visualParent;
        private readonly ItemsRepeaterPanel _logicalParent;
    }
}