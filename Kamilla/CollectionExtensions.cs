using System;
using System.Collections.Generic;
using System.Linq;

namespace Kamilla
{
    /// <summary>
    /// Contains collection extension methods.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Concatenates the string representations of members of a collection,
        /// using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">The type of the members of values.</typeparam>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <param name="separator">The separator to use between each member.</param>
        /// <returns>
        /// A string that consists of the members of values delimited by the separator string.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// values is null.
        /// </exception>
        public static string ToStringJoin<T>(this IEnumerable<T> values, string separator)
        {
            return string.Join<T>(separator, values);
        }

        /// <summary>
        /// Concatenates the string representations of members of a collection,
        /// using a comma as a separator.
        /// </summary>
        /// <typeparam name="T">The type of the members of values.</typeparam>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <returns>
        /// A string that consists of the members of values delimited by a comma.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// values is null.
        /// </exception>
        public static string ToStringJoin<T>(this IEnumerable<T> values)
        {
            return ToStringJoin(values, ", ");
        }

        /// <summary>
        /// Adds the specified number of elements of the specified collection
        /// to the end of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of elements of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </typeparam>
        /// <param name="list">
        /// The <see cref="System.Collections.Generic.List&lt;T&gt;"/> to which
        /// the elements should be added.
        /// </param>
        /// <param name="collection">
        /// The collection whose elements should be added to the end
        /// of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </param>
        /// <param name="count">
        /// Number of elements of the collection to add to the end
        /// of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </param>
        public static void AddRange<T>(this List<T> list, IEnumerable<T> collection, int count)
        {
            AddRange(list, collection, 0, count);
        }

        /// <summary>
        /// Adds the specified number of elements starting from the specified index of the specified
        /// collection to the end of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of elements of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </typeparam>
        /// <param name="list">
        /// The <see cref="System.Collections.Generic.List&lt;T&gt;"/> to which
        /// the elements should be added.
        /// </param>
        /// <param name="startIndex">
        /// Index of the first item in the collection whose elements should be added to the end
        /// of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </param>
        /// <param name="collection">
        /// The collection whose elements should be added to the end
        /// of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </param>
        /// <param name="count">
        /// Number of elements of the collection to add to the end
        /// of the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.
        /// </param>
        public static void AddRange<T>(this List<T> list, IEnumerable<T> collection, int startIndex, int count)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (collection == null)
                throw new ArgumentNullException("collection");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (list.Capacity - list.Count < count)
                list.Capacity = list.Count + count;

            int end = startIndex + count;
            var enumer = collection.GetEnumerator();
            for (int i = 0; i < end; i++)
            {
                if (!enumer.MoveNext())
                    throw new ArgumentOutOfRangeException();

                if (i >= startIndex)
                    list.Add(enumer.Current);
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key if one exists.
        /// </summary>
        /// <typeparam name="TKey">
        /// Type of the key of the dictionary.
        /// </typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary">
        /// The dictionary to look for value in.
        /// </param>
        /// <param name="key">
        /// Key of the value.
        /// </param>
        /// <returns>
        /// Value associated with the specified key if one exists; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// dictionary is null.
        /// </exception>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null)
                throw new ArgumentNullException();

            TValue val;
            dictionary.TryGetValue(key, out val);
            return val;
        }

        /// <summary>
        /// Determines if the elements of two collections are equal using the specified equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="comparer">
        /// Equality comparer for the elements of the collections.
        /// </param>
        /// <returns>
        /// true if the elements of the collections are equal according to
        /// the specified equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null, or comparer is null.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2, IEqualityComparer<T> comparer)
        {
            return InternalSequenceEqual(collection, collection2, 0, 0, -1, comparer, false);
        }

        ///// <summary>
        ///// Determines if the elements of two collections are equal using the default equality comparer.
        ///// </summary>
        ///// <typeparam name="T">
        ///// Element type of the collections.
        ///// </typeparam>
        ///// <param name="collection">
        ///// First collection to compare elements in.
        ///// </param>
        ///// <param name="collection2">
        ///// Second collection to compare elements in.
        ///// </param>
        ///// <returns>
        ///// true if the elements of the collections are equal according to
        ///// the default equality comparer; otherwise, false.
        ///// </returns>
        ///// <exception cref="System.ArgumentNullException">
        ///// collection is null, or collection2 is null.
        ///// </exception>
        //public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2)
        //{
        //    return InternalSequenceEqual(collection, collection2, 0, 0, -1, EqualityComparer<T>.Default, false);
        //}

        /// <summary>
        /// Determines if the specified number of elements of two collections
        /// starting from the specified indices are equal using the specified equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="index">
        /// Index of the first element to compare in the first collection.
        /// </param>
        /// <param name="index2">
        /// Index of the second element to compare in the first collection.
        /// </param>
        /// <param name="count">
        /// Number of elements to compare in the collections.
        /// </param>
        /// <param name="comparer">
        /// Equality comparer for the elements of the collections.
        /// </param>
        /// <returns>
        /// true if the specified number of elements of the collections starting from the specified
        /// indices are equal according to the specified equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null, or comparer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index, index2, and count represent invalid sequences for the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int index, int index2, int count, IEqualityComparer<T> comparer)
        {
            return InternalSequenceEqual(collection, collection2, index, index2, count, comparer, true);
        }

        /// <summary>
        /// Determines if the elements of two collections
        /// starting from the specified indices are equal using the specified equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="index">
        /// Index of the first element to compare in the first collection.
        /// </param>
        /// <param name="index2">
        /// Index of the second element to compare in the first collection.
        /// </param>
        /// <param name="comparer">
        /// Equality comparer for the elements of the collections.
        /// </param>
        /// <returns>
        /// true if the elements of the collections starting from the specified
        /// indices are equal according to the specified equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null, or comparer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index and index2 represent invalid sequences for the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int index, int index2, IEqualityComparer<T> comparer)
        {
            return InternalSequenceEqual(collection, collection2, index, index2, -1, comparer, false);
        }

        /// <summary>
        /// Determines if the elements of two collections
        /// starting from the specified indices are equal using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="index">
        /// Index of the first element to compare in the first collection.
        /// </param>
        /// <param name="index2">
        /// Index of the second element to compare in the first collection.
        /// </param>
        /// <returns>
        /// true if the elements of the collections starting from the specified
        /// indices are equal according to the default equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index and index2 represent invalid sequences for the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int index, int index2)
        {
            return InternalSequenceEqual(collection, collection2, index, index2, -1, EqualityComparer<T>.Default, false);
        }

        /// <summary>
        /// Determines if the specified number of elements of two collections
        /// starting from the specified indices are equal using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="index">
        /// Index of the first element to compare in the first collection.
        /// </param>
        /// <param name="index2">
        /// Index of the second element to compare in the first collection.
        /// </param>
        /// <param name="count">
        /// Number of elements to compare in the collections.
        /// </param>
        /// <returns>
        /// true if the specified number of elements of the collections starting from the specified
        /// indices are equal according to the default equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index, index2, and count represent invalid sequences for the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int index, int index2, int count)
        {
            return InternalSequenceEqual(collection, collection2, index, index2, count, EqualityComparer<T>.Default, true);
        }

        /// <summary>
        /// Determines if the specified number of elements of two collections
        /// are equal using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="count">
        /// Number of elements to compare in the collections.
        /// </param>
        /// <returns>
        /// true if the specified number of elements of the collections are equal according
        /// to the default equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// There are less than count elements in one of the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2, int count)
        {
            return InternalSequenceEqual(collection, collection2, 0, 0, count, EqualityComparer<T>.Default, true);
        }

        /// <summary>
        /// Determines if the specified number of elements of two collections
        /// are equal using the specified equality comparer.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collections.
        /// </typeparam>
        /// <param name="collection">
        /// First collection to compare elements in.
        /// </param>
        /// <param name="collection2">
        /// Second collection to compare elements in.
        /// </param>
        /// <param name="count">
        /// Number of elements to compare in the collections.
        /// </param>
        /// <param name="comparer">
        /// Equality comparer for the elements of the collections.
        /// </param>
        /// <returns>
        /// true if the specified number of elements of the collections are equal according
        /// to the specified equality comparer; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// collection is null, or collection2 is null, or comparer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// There are less than count elements in one of the collections.
        /// </exception>
        public static bool SequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int count, IEqualityComparer<T> comparer)
        {
            return InternalSequenceEqual(collection, collection2, 0, 0, count, comparer, true);
        }

        static bool InternalSequenceEqual<T>(this IEnumerable<T> collection, IEnumerable<T> collection2,
            int index, int index2, int count, IEqualityComparer<T> comparer, bool countKnown)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection2 == null)
                throw new ArgumentNullException("collection2");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (index2 < 0)
                throw new ArgumentOutOfRangeException("index2");

            if (countKnown && count < 0)
                throw new ArgumentOutOfRangeException("count");

            var enumer1 = collection.GetEnumerator();
            for (int i = 0; i < index; ++i)
            {
                if (!enumer1.MoveNext())
                    throw new ArgumentOutOfRangeException("index");
            }

            var enumer2 = collection2.GetEnumerator();
            for (int i = 0; i < index2; ++i)
            {
                if (!enumer2.MoveNext())
                    throw new ArgumentOutOfRangeException("index2");
            }

            if (countKnown)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (!enumer1.MoveNext() || !enumer2.MoveNext())
                        throw new ArgumentOutOfRangeException("count");

                    if (!comparer.Equals(enumer1.Current, enumer2.Current))
                        return false;
                }

                return true;
            }
            else
            {
                int i = 0;
                while (true)
                {
                    bool first = enumer1.MoveNext();
                    bool second = enumer2.MoveNext();

                    if (!first && !second)
                        return true;
                    else if (!first && second)
                        return false;
                    else if (first && !second)
                        return false;

                    if (!comparer.Equals(enumer1.Current, enumer2.Current))
                        return false;

                    ++i;
                }
            }

            // Unreachable
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collection.
        /// </typeparam>
        /// <param name="collection">
        /// The collection to find the equal item in.
        /// </param>
        /// <param name="value">
        /// The object to locate in the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/>.
        /// The value can be null for reference types.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the entire
        /// <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/>, if found; otherwise, –1.
        /// </returns>
        public static int IndexOf<T>(this IEnumerable<T> collection, T value)
        {
            var eqcmp = EqualityComparer<T>.Default;
            int i = 0;

            foreach (var item in collection)
            {
                if (eqcmp.Equals(value, item))
                    return i;

                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Reports the index of the first matching item in a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collection.
        /// </typeparam>
        /// <param name="collection">
        /// The collection to find matching items in.
        /// </param>
        /// <param name="pred">
        /// Predicate which must be matched by the item.
        /// </param>
        /// <returns>
        /// Index of the first matching item; if none found, -1.
        /// </returns>
        public static int IndexOf<T>(this IEnumerable<T> collection, Predicate<T> pred)
        {
            int i = 0;

            foreach (var item in collection)
            {
                if (pred(item))
                    return i;

                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Reports the index of the first matching item in a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Element type of the collection.
        /// </typeparam>
        /// <param name="collection">
        /// The collection to find matching items in.
        /// </param>
        /// <param name="pred">
        /// Predicate which must be matched by the item.
        /// </param>
        /// <param name="start">
        /// The starting index for the search.
        /// </param>
        /// <returns>
        /// Index of the first matching item; if none found, -1.
        /// </returns>
        public static int IndexOf<T>(this IEnumerable<T> collection, Predicate<T> pred, int start)
        {
            int i = 0;

            foreach (var item in collection.Skip(start))
            {
                if (pred(item))
                    return i + start;

                ++i;
            }

            return -1;
        }

        public static int BinaryIndexOf<T>(this IList<T> list, T item, Comparison<T> comparison)
        {
            return BinaryIndexOf(list, 0, list.Count, item, comparison);
        }

        public static int BinaryIndexOf<T>(this IList<T> list, T item, IComparer<T> comparer)
        {
            return BinaryIndexOf(list, 0, list.Count, item, comparer);
        }

        public static int BinaryIndexOf<T>(this IList<T> list, Func<T, int> f)
        {
            return BinaryIndexOf(list, 0, list.Count, f);
        }

        public static int BinaryIndexOf<T>(this IList<T> list, int index, int length, T item, Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            return BinaryIndexOf(list, index, length, _ => comparison(_, item));
        }

        public static int BinaryIndexOf<T>(this IList<T> list, int index, int length, T item, IComparer<T> comparer)
        {
            return BinaryIndexOf(list, index, length, item, comparer.Compare);
        }

        public static int BinaryIndexOf<T>(this IList<T> list, int index, int length, Func<T, int> f)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (index < 0 || index + length > list.Count || length < 0)
                throw new ArgumentOutOfRangeException();

            if (f == null)
                throw new ArgumentNullException("f");

            int num = index;
            int num2 = (index + length) - 1;
            while (num <= num2)
            {
                int num3 = num + ((num2 - num) >> 1);
                int num4 = f(list[num3]);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return -1;
        }

        public static int IndexOfAny<T>(this IEnumerable<T> array, T[] values)
        {
            var eqcmp = EqualityComparer<T>.Default;
            int i = 0;
            int valuesLength = values.Length;

            foreach (var item in array)
            {
                for (int j = 0; j < valuesLength; ++j)
                {
                    if (eqcmp.Equals(values[j], item))
                        return i;
                }

                ++i;
            }

            return -1;
        }

        public static int IndexOfSequence<T>(this T[] array, T[] sequence)
        {
            var eqcmp = EqualityComparer<T>.Default;

            for (int i = 0; i < array.Length - sequence.Length + 1; ++i)
            {
                for (int j = 0; j < sequence.Length; ++j)
                {
                    if (!eqcmp.Equals(array[i + j], sequence[j]))
                        goto _cont;
                }

                return i;
            _cont:
                continue;
            }

            return -1;
        }

        public static int IndexOfSequence<T>(this T[] array, T?[] sequence) where T : struct
        {
            var eqcmp = EqualityComparer<T>.Default;

            for (int i = 0; i < array.Length - sequence.Length + 1; ++i)
            {
                for (int j = 0; j < sequence.Length; ++j)
                {
                    if (sequence[j].HasValue)
                    {
                        if (!eqcmp.Equals(array[i + j], sequence[j].Value))
                            goto _cont;
                    }
                }

                return i;
            _cont:
                continue;
            }

            return -1;
        }

        public static void CombineItems<T>(ref List<T> items, int first, int count, Func<T> insertedItem)
        {
            if (insertedItem == null)
                throw new ArgumentNullException("insertedItem");

            InternalCheckCombineItems(items.Count, first, count);
            items = InternalCombineItems(items.ToArray(), first, count, insertedItem());
        }

        public static void CombineItems<T>(ref T[] items, int first, int count, Func<T> insertedItem)
        {
            if (insertedItem == null)
                throw new ArgumentNullException("insertedItem");

            InternalCheckCombineItems(items.Length, first, count);
            items = InternalCombineItems(items, first, count, insertedItem()).ToArray();
        }

        public static void CombineItems<T>(ref List<T> items, int first, int count, T insertedItem)
        {
            InternalCheckCombineItems(items.Count, first, count);

            items = InternalCombineItems(items.ToArray(), first, count, insertedItem);
        }

        public static void CombineItems<T>(ref T[] items, int first, int count, T insertedItem)
        {
            InternalCheckCombineItems(items.Length, first, count);

            items = InternalCombineItems(items, first, count, insertedItem).ToArray();
        }

        static void InternalCheckCombineItems(int total, int first, int count)
        {
            if (first < 0 || first >= total)
                throw new ArgumentOutOfRangeException("first");

            int last = first + count - 1;

            if (count == 0 || count == 1)
                return;
            else if (count < 0 || first + count - 1 >= total)
                throw new ArgumentOutOfRangeException("count");
        }

        static List<T> InternalCombineItems<T>(T[] items, int first, int count, T insertedItem)
        {
            int last = first + count - 1;
            var newItems = new List<T>(items.Length - count + 1);
            newItems.AddRange(items, 0, first);
            newItems.Add(insertedItem);
            if (items.Length > last)
                newItems.AddRange(items, first + count, items.Length - first - count);
            return newItems;
        }
    }
}
