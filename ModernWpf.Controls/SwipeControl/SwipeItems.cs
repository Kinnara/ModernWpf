using System;
using System.Collections.ObjectModel;

namespace ModernWpf.Controls
{
    public class SwipeItems : ObservableCollection<SwipeItem>
    {
        private SwipeMode _mode = SwipeMode.Reveal;
        private SwipeControl _owner;
        private SwipeItemsPlacement _placement;

        public SwipeMode Mode
        {
            get => _mode;
            set
            {
                if (value == SwipeMode.Execute && Count > 1)
                {
                    throw new ArgumentException("Execute items should only have one item.");
                }

                _mode = value;
            }
        }

        protected override void InsertItem(int index, SwipeItem item)
        {
            ValidateCanAdd();
            base.InsertItem(index, item);
            _owner?.OnSwipeItemsChanged();
        }

        protected override void SetItem(int index, SwipeItem item)
        {
            base.SetItem(index, item);
            _owner?.OnSwipeItemsChanged();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _owner?.OnSwipeItemsChanged();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _owner?.OnSwipeItemsChanged();
        }

        internal void AttachOwner(SwipeControl owner, SwipeItemsPlacement placement)
        {
            _owner = owner;
            _placement = placement;
        }

        internal void DetachOwner(SwipeControl owner)
        {
            if (ReferenceEquals(_owner, owner))
            {
                _owner = null;
                _placement = SwipeItemsPlacement.None;
            }
        }

        private void ValidateCanAdd()
        {
            if (Mode == SwipeMode.Execute && Count > 0)
            {
                throw new ArgumentException("Execute items should only have one item.");
            }

            _owner?.ValidateSwipeItemsCanAdd(_placement);
        }
    }
}
