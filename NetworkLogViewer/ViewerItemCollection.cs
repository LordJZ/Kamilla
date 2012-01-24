//#define IMPLEMENT_INPC

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Kamilla.Network.Viewing;

namespace NetworkLogViewer
{
    class ViewerItemCollection : IList, IList<ViewerItem>, INotifyCollectionChanged
#if IMPLEMENT_INPC
        , INotifyPropertyChanged
#endif
    {
        List<ViewerItem> m_list;
        ViewerImplementation m_viewer;

        #region .ctor
        /// <summary>
        /// Initializes a new instance of <see cref="NetworkLogViewer.ViewerItemCollection"/> class.
        /// </summary>
        public ViewerItemCollection(ViewerImplementation viewer)
        {
            m_list = new List<ViewerItem>();
            m_viewer = viewer;
        }
        #endregion

        #region Properties

        public bool IsFixedSize { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public int Count { get { return m_list.Count; } }
        public int Capacity
        {
            get { return m_list.Capacity; }
            set { m_list.Capacity = value; }
        }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return null; } }
        bool m_suspended;

        #endregion

        #region Methods
        public void SuspendUpdating()
        {
            m_suspended = true;
        }

        public void ResumeUpdating()
        {
            m_suspended = false;
        }

        /// <summary>
        /// Updates the collection via events if it is in suspended state.
        /// </summary>
        public void Update()
        {
#if IMPLEMENT_INPC
            this.OnPropertyChanged(Binding.IndexerName);
#endif
            this.OnCollectionReset();
        }

        public void Clear()
        {
            if (m_list.Count != 0)
            {
                m_list.Clear();

                if (!m_suspended)
                {
#if IMPLEMENT_INPC
                    this.OnPropertyChanged("Count");
                    this.OnPropertyChanged(Binding.IndexerName);
#endif
                    this.OnCollectionReset();
                }
            }
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(ViewerItem value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Viewer != m_viewer)
                throw new ArgumentException("value");

            if (value.Index != m_list.Count)
                throw new ArgumentException("value.Index");

            m_list.Add(value);

            if (!m_suspended)
            {
#if IMPLEMENT_INPC
                this.OnPropertyChanged("Count");
                this.OnPropertyChanged(Binding.IndexerName);
#endif
                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, m_list.Count - 1);
            }
        }

        public bool Contains(ViewerItem value)
        {
            if (value == null)
                return false;

            if (value.Viewer != m_viewer)
                return false;

            int index = value.Index;
            if (index < 0 || index >= m_list.Count)
                return false;

            if (m_list[index] != value)
                throw new InvalidOperationException();

            return true;
        }

        public bool Remove(ViewerItem value)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(ViewerItem value)
        {
            if (value == null)
                return -1;

            if (value.Viewer != m_viewer)
                return -1;

            int index = value.Index;
            if (index < 0 || index >= m_list.Count)
                return -1;

            if (m_list[index] == value)
                return index;

            return -1;
        }

        public void Insert(int index, ViewerItem value)
        {
            throw new NotSupportedException();
        }

        public ViewerItem this[int index]
        {
            get
            {
                if (index < 0 || index >= m_list.Count)
                    throw new ArgumentOutOfRangeException("index");

                var result = m_list[index];

                if (result != null && this.ItemQueried != null)
                    this.ItemQueried(this, new ViewerItemEventArgs(result));

                return result;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Viewer != m_viewer)
                    throw new ArgumentException("value");

                if (index != value.Index)
                    throw new ArgumentException("index");

                if (index < 0 || index >= m_list.Count)
                    throw new ArgumentOutOfRangeException("index");

                var item = m_list[index];
                m_list[index] = value;

                if (!m_suspended)
                {
#if IMPLEMENT_INPC
                    this.OnPropertyChanged(Binding.IndexerName);
#endif
                    this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, value, index);
                }
            }
        }

        public IEnumerator<ViewerItem> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        #endregion

        protected void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
            object item, int index)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
            object item, int index, int oldIndex)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
            object oldItem, object newItem, int index)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        protected void OnCollectionReset()
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event ViewerItemEventHandler ItemQueried;

        #region Hidden Interface Methods

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ViewerItem>)this).GetEnumerator();
        }
        #endregion

        #region ICollection implementation
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)m_list).CopyTo(array, index);
        }
        #endregion

        #region ICollection<ViewerItem> implementation
        void ICollection<ViewerItem>.CopyTo(ViewerItem[] array, int index)
        {
            ((ICollection<ViewerItem>)m_list).CopyTo(array, index);
        }
        #endregion

        #region IList implementation
        object IList.this[int index]
        {
            get
            {
                return ((IList<ViewerItem>)this)[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                var val = value as ViewerItem;
                if (val == null)
                    throw new ArgumentException();

                ((IList<ViewerItem>)this)[index] = val;
            }
        }

        int IList.Add(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as ViewerItem;
            if (val == null)
                throw new ArgumentException();

            ((IList<ViewerItem>)this).Add(val);

            return this.Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as ViewerItem;
            if (val == null)
                throw new ArgumentException();

            return ((IList<ViewerItem>)this).Contains(val);
        }

        int IList.IndexOf(object value)
        {
            return ((IList<ViewerItem>)this).IndexOf(value as ViewerItem);
        }

        void IList.Insert(int index, object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as ViewerItem;
            if (val == null)
                throw new ArgumentException();

            ((IList<ViewerItem>)this).Insert(index, val);
        }

        void IList.Remove(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as ViewerItem;
            if (val == null)
                throw new ArgumentException();

            ((IList<ViewerItem>)this).Remove(val);
        }
        #endregion

        #endregion
    }
}
