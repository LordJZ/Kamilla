using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnderlyingArrayItem = System.String;

namespace NetworkLogViewer
{
    class VisualItemCollection : IList, IList<UnderlyingArrayItem[]>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        List<UnderlyingArrayItem[]> m_list;
        int m_expectedArrayLength;

        #region .ctor
        /// <summary>
        /// Initializes a new instance of <see cref="NetworkLogViewer.VisualItemCollection"/> class.
        /// </summary>
        public VisualItemCollection()
        {
            m_list = new List<UnderlyingArrayItem[]>();
            m_expectedArrayLength = -1;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of expected elements in the underlying string arrays.
        /// </summary>
        public int ExpectedArrayLength
        {
            get
            {
                return m_expectedArrayLength;
            }
            set
            {
                if (this.Count != 0)
                    throw new InvalidOperationException("Cannot modify expected array length when there are elements in the collection.");

                m_expectedArrayLength = value;
            }
        }

        public bool IsFixedSize { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public int Count { get { return m_list.Count; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return null; } }

        #endregion

        #region Methods
        public void Clear()
        {
            if (m_list.Count != 0)
            {
                m_list.Clear();

                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                this.OnCollectionReset();
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= m_list.Count)
                throw new ArgumentOutOfRangeException();

            var item = m_list[index];
            m_list.RemoveAt(index);

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public void Add(UnderlyingArrayItem[] value)
        {
            if (value.Length != m_expectedArrayLength)
                throw new ArgumentException();

            m_list.Add(value);

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, m_list.Count - 1);
        }

        public bool Contains(UnderlyingArrayItem[] value)
        {
            return m_list.Contains(value);
        }

        public bool Remove(UnderlyingArrayItem[] value)
        {
            int idx = this.IndexOf(value);

            if (idx >= 0)
            {
                this.RemoveAt(idx);
                return true;
            }

            return false;
        }

        public int IndexOf(UnderlyingArrayItem[] value)
        {
            return m_list.IndexOf(value);
        }

        public void Insert(int index, UnderlyingArrayItem[] value)
        {
            if (value.Length != m_expectedArrayLength)
                throw new ArgumentException();

            m_list.Insert(index, value);

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
        }

        public UnderlyingArrayItem[] this[int index]
        {
            get
            {
                // here item is displayed

                return m_list[index];
            }
            set
            {
                if (index < 0 || index >= m_list.Count)
                    throw new ArgumentOutOfRangeException();

                if (value.Length != m_expectedArrayLength)
                    throw new ArgumentException();

                var item = m_list[index];
                m_list[index] = value;

                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, value, index);
            }
        }

        public IEnumerator<UnderlyingArrayItem[]> GetEnumerator()
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
            if (this.CollectionChanged!=null)
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

        #region Hidden Interface Methods

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<UnderlyingArrayItem[]>)this).GetEnumerator();
        }
        #endregion

        #region ICollection implementation
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)m_list).CopyTo(array, index);
        }
        #endregion

        #region ICollection<UnderlyingArrayItem[]> implementation
        void ICollection<UnderlyingArrayItem[]>.CopyTo(UnderlyingArrayItem[][] array, int index)
        {
            ((ICollection<UnderlyingArrayItem[]>)m_list).CopyTo(array, index);
        }
        #endregion

        #region IList implementation
        object IList.this[int index]
        {
            get
            {
                return ((IList<UnderlyingArrayItem[]>)this)[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                var val = value as UnderlyingArrayItem[];
                if (val == null)
                    throw new ArgumentException();

                ((IList<UnderlyingArrayItem[]>)this)[index] = val;
            }
        }

        int IList.Add(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as UnderlyingArrayItem[];
            if (val == null)
                throw new ArgumentException();

            ((IList<UnderlyingArrayItem[]>)this).Add(val);

            return this.Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as UnderlyingArrayItem[];
            if (val == null)
                throw new ArgumentException();

            return ((IList<UnderlyingArrayItem[]>)this).Contains(val);
        }

        int IList.IndexOf(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as UnderlyingArrayItem[];
            if (val == null)
                throw new ArgumentException();

            return ((IList<UnderlyingArrayItem[]>)this).IndexOf(val);
        }

        void IList.Insert(int index, object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as UnderlyingArrayItem[];
            if (val == null)
                throw new ArgumentException();

            ((IList<UnderlyingArrayItem[]>)this).Insert(index, val);
        }

        void IList.Remove(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var val = value as UnderlyingArrayItem[];
            if (val == null)
                throw new ArgumentException();

            ((IList<UnderlyingArrayItem[]>)this).Remove(val);
        }
        #endregion

        #endregion
    }
}
