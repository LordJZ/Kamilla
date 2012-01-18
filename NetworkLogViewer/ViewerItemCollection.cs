//#define IMPLEMENT_INPC

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Kamilla.Network.Viewing;

namespace NetworkLogViewer
{
    public class ViewerItemCollection : IList, IList<ViewerItem>, INotifyCollectionChanged
#if IMPLEMENT_INPC
        , INotifyPropertyChanged
#endif
    {
        List<ViewerItem> m_list;

        #region .ctor
        /// <summary>
        /// Initializes a new instance of <see cref="NetworkLogViewer.ViewerItemCollection"/> class.
        /// </summary>
        public ViewerItemCollection()
        {
            m_list = new List<ViewerItem>();
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

        /// <summary>
        /// Updates the specified <see cref="Kamilla.Network.Viewing.ViewerItem"/>
        /// even if the collection is in suspended state.
        /// </summary>
        /// <param name="item">
        /// The <see cref="Kamilla.Network.Viewing.ViewerItem"/> that should be updated.
        /// </param>
        public void Update(ViewerItem item)
        {
            int index = item.Index;

            if (m_list[index] != item)
                throw new InvalidOperationException();

            // Any better solution?
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
#if IMPLEMENT_INPC
            this.OnPropertyChanged(Binding.IndexerName);
#endif
        }

//        public void SetData(IList<ViewerItem> items)
//        {
//            if (m_list.Capacity < items.Count)
//                m_list.Capacity = items.Count;

//            m_list.AddRange(items);

//            if (!m_suspended)
//            {
//#if IMPLEMENT_INPC
//                this.OnPropertyChanged("Count");
//                this.OnPropertyChanged(Binding.IndexerName);
//#endif
//                this.OnCollectionReset();
//            }
//        }

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
//            if (index < 0 || index >= m_list.Count)
//                throw new ArgumentOutOfRangeException();

//            var item = m_list[index];
//            m_list.RemoveAt(index);

//            if (!m_suspended)
//            {
//#if IMPLEMENT_INPC
//                this.OnPropertyChanged("Count");
//                this.OnPropertyChanged(Binding.IndexerName);
//#endif
//                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
//            }
        }

        public void Add(ViewerItem value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

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
            return m_list.Contains(value);
        }

        public bool Remove(ViewerItem value)
        {
            throw new NotSupportedException();
//            int idx = this.IndexOf(value);

//            if (idx >= 0)
//            {
//                this.RemoveAt(idx);
//#if IMPLEMENT_INPC
//                this.OnPropertyChanged("Count");
//                this.OnPropertyChanged(Binding.IndexerName);
//#endif
//                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, idx);
//                return true;
//            }

//            return false;
        }

        public int IndexOf(ViewerItem value)
        {
            return m_list.IndexOf(value);
        }

        public void Insert(int index, ViewerItem value)
        {
            throw new NotSupportedException();
            //m_list.Insert(index, value);

            //if (!m_suspended)
            //{
            //    this.OnPropertyChanged("Count");
            //    this.OnPropertyChanged(Binding.IndexerName);
            //    this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
            //}
        }

        public ViewerItem this[int index]
        {
            get
            {
                var result = m_list[index];

                if (result != null && this.ItemQueried != null)
                    this.ItemQueried(this, new ViewerItemEventArgs(result));

                return result;
            }
            set
            {
                if (index < 0 || index >= m_list.Count)
                    throw new ArgumentOutOfRangeException();

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
            if (value == null)
                throw new ArgumentNullException();

            var val = value as ViewerItem;
            if (val == null)
                throw new ArgumentException();

            return ((IList<ViewerItem>)this).IndexOf(val);
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
