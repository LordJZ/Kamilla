using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla
{
    public class CircularCollection<T> : IList<T>, IList
    {
        #region Enumerator
        struct Enumerator : IEnumerator<T>
        {
            CircularCollection<T> m_collection;
            int m_version;
            int m_index;

            internal Enumerator(CircularCollection<T> collection)
            {
                m_collection = collection;
                m_version = collection.m_version;
                m_index = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (m_index < 0 || m_index >= m_collection.Count || m_collection.m_version != m_version)
                        throw new InvalidOperationException();

                    return m_collection[m_index];
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    if (m_index < 0 || m_index >= m_collection.Count || m_collection.m_version != m_version)
                        throw new InvalidOperationException();

                    return m_collection[m_index];
                }
            }

            bool IEnumerator.MoveNext()
            {
                if (m_collection.m_version != m_version)
                    throw new InvalidOperationException();

                if (m_index >= m_collection.Count)
                    return false;

                ++m_index;
                return true;
            }

            void IEnumerator.Reset()
            {
                if (m_collection.m_version != m_version)
                    throw new InvalidOperationException();

                m_index = -1;
            }

            void IDisposable.Dispose()
            {
            }
        }
        #endregion

        T[] m_data;
        int m_head;
        int m_version;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.CircularCollection&lt;T&gt;"/>
        /// class with the specified capacity.
        /// </summary>
        /// <param name="capacity">
        /// The number of elements this instance can hold.
        /// </param>
        public CircularCollection(int capacity)
        {
            m_data = new T[capacity];
        }

        /// <summary>
        /// Gets the number of elements contained in the current
        /// instance of <see cref="Kamilla.CircularCollection&lt;T&gt;"/> class.
        /// 
        /// This value is constant.
        /// </summary>
        public int Count { get { return m_data.Length; } }

        /// <summary>
        /// Rolls the elements of the <see cref="Kamilla.CircularCollection&lt;T&gt;"/> to the left;
        /// that is, to the front of the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </summary>
        /// <param name="count">
        /// Number of roll rounds to perform.
        /// </param>
        public void RollLeft(int count)
        {
            this.InternalAlterHead(count);

            ++m_version;
        }

        /// <summary>
        /// Rolls the elements of the <see cref="Kamilla.CircularCollection&lt;T&gt;"/> to the right;
        /// that is, to the back of the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </summary>
        /// <param name="count">
        /// Number of roll rounds to perform.
        /// </param>
        public void RollRight(int count)
        {
            this.InternalAlterHead(-count);

            ++m_version;
        }

        /// <summary>
        /// Removes all items from the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_data, 0, m_data.Length);
            m_head = 0;

            ++m_version;
        }

        /// <summary>
        /// Determines whether the <see cref="Kamilla.CircularCollection&lt;T&gt;"/> contains a specific item.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// true if item is found in the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return m_data.Contains(item);
        }

        public void PushFront(T item)
        {
            this.RollRight(1);

            this.Front = item;
        }

        public void PushBack(T item)
        {
            this.RollLeft(1);

            this.Back = item;
        }

        public T Front
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }

        public T Back
        {
            get
            {
                return this[this.Count - 1];
            }
            set
            {
                this[this.Count - 1] = value;
            }
        }

        public T PopFront()
        {
            var front = this.Front;
            this.Front = default(T);

            this.RollLeft(1);

            return front;
        }

        public T PopBack()
        {
            var back = this.Back;
            this.Back = default(T);

            this.RollRight(1);

            return back;
        }

        /// <summary>
        /// Gets or sets the item at the specified index in the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </summary>
        /// <param name="index">
        /// Index of the item to get or set.
        /// </param>
        /// <returns>
        /// Item at the specified index in the <see cref="Kamilla.CircularCollection&lt;T&gt;"/>.
        /// </returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= m_data.Length)
                    throw new ArgumentOutOfRangeException("index");

                index += m_head;
                this.InternalFixIndex(ref index);
                return m_data[index];
            }
            set
            {
                if (index < 0 || index >= m_data.Length)
                    throw new ArgumentOutOfRangeException("index");

                index += m_head;
                this.InternalFixIndex(ref index);
                m_data[index] = value;

                ++m_version;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="Kamilla.CircularCollection&lt;T&gt;"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public IEnumerable<T> Reverse()
        {
            int version = m_version;
            int count = m_data.Length;

            for (int i = count - 1; i >= 0; i--)
            {
                if (version != m_version)
                    throw new InvalidOperationException();

                yield return this[i];
            }
        }

        #region Internals
        void InternalAlterHead(int count)
        {
            m_head += count;

            this.InternalFixIndex(ref m_head);
        }

        void InternalFixIndex(ref int index)
        {
            int local_index = index;
            int count = m_data.Length;

            local_index %= count;
            if (local_index < 0)
                local_index += count;

            index = local_index;
        }
        #endregion

        #region Implicit Interface Implementations
        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
        #endregion

        #region ICollection
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return null; } }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Array.Copy(m_data, m_head, array, arrayIndex, m_data.Length - m_head);
            Array.Copy(m_data, 0, array, arrayIndex + m_data.Length - m_head, m_head);
        }
        #endregion

        #region IList
        bool IList.IsFixedSize { get { return true; } }
        bool IList.IsReadOnly { get { return false; } }

        object IList.this[int index]
        {
            get
            {
                return ((IList<T>)this)[index];
            }
            set
            {
                if (!(value is T))
                    throw new ArgumentException("value");

                ((IList<T>)this)[index] = (T)value;
            }
        }

        int IList.Add(object item)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object item)
        {
            if (!(item is T))
                throw new ArgumentException("item");

            return ((ICollection<T>)this).Contains((T)item);
        }

        void IList.Remove(object item)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object item)
        {
            int index = Array.IndexOf(m_data, item);
            if (index < 0)
                return index;

            index += m_head;
            this.InternalFixIndex(ref index);
            return index;
        }
        #endregion

        #region ICollection<T>
        bool ICollection<T>.IsReadOnly { get { return false; } }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection)this).CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IList<T>
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList<T>.Insert(int index, T value)
        {
            throw new NotSupportedException();
        }

        int IList<T>.IndexOf(T item)
        {
            int index = Array.IndexOf(m_data, item);
            if (index < 0)
                return -1;

            index += m_head;
            this.InternalFixIndex(ref index);
            return index;
        }
        #endregion
        #endregion
    }
}
