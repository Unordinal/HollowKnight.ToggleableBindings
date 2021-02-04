using System;

namespace UnoHKUtils.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Copies the array and adds the item to the end of the new array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="source">The array to add to.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>
        /// A new array of type <typeparamref name="T"/> that is longer by one and
        /// has <paramref name="item"/> as the last element.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static T[] Add<T>(this T[] source, T item)
        {
            return source.Insert(source.Length, item);
        }

        /// <summary>
        /// Copies the array and adds an array of items to the end of the new array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="source">The array to add to.</param>
        /// <param name="items">The items to add.</param>
        /// <returns>
        /// A new array of type <typeparamref name="T"/> that is longer by the length of
        /// <paramref name="items"/> and has the contents of <paramref name="items"/> as its last elements.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static T[] AddRange<T>(this T[] source, params T[] items)
        {
            return source.InsertRange(source.Length, items);
        }

        /// <summary>
        /// Copies the array and inserts an item into the new array at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="source">The array to insert into.</param>
        /// <param name="index">The index to insert at.</param>
        /// <param name="item">The item to insert.</param>
        /// <returns>
        /// A new array of type <typeparamref name="T"/> that is longer by one and contains the inserted
        /// <paramref name="item"/> at <paramref name="index"/>, with existing items shifted forward to make room.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static T[] Insert<T>(this T[] source, int index, T item)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (index < 0 || index > source.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            T[] output = new T[source.Length + 1];
            output[index] = item;

            if (index > 0)
                Array.Copy(source, output, index);
            if (index < source.Length)
                Array.Copy(source, index, output, index + 1, source.Length - index);

            return output;
        }

        /// <summary>
        /// Copies the array and inserts an array of items into the new array at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="source">The array to insert into.</param>
        /// <param name="index">The index to insert at.</param>
        /// <param name="items">The items to insert.</param>
        /// <returns>
        /// A new array of type <typeparamref name="T"/> that is longer by the length of <paramref name="items"/> and contains the inserted
        /// <paramref name="items"/> at <paramref name="index"/>, with existing items shifted forward to make room.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static T[] InsertRange<T>(this T[] source, int index, params T[] items)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (index < 0 || index > source.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            T[] output = new T[source.Length + items.Length];
            Array.Copy(items, 0, output, index, items.Length);

            if (index > 0)
                Array.Copy(source, output, index);
            if (index < source.Length)
                Array.Copy(source, index, output, index + items.Length, source.Length - index);

            return output;
        }

        /// <summary>
        /// Copies the array and removes the item at the specified index of the new array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="source">The array to remove from.</param>
        /// <param name="index">The index of the item to remove.</param>
        /// <returns>A new array of type <typeparamref name="T"/> that is shorter by one and does not contain the element previously at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (index < 0 || index >= source.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            T[] output = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, output, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, output, index, output.Length - index);

            return output;
        }
    }
}