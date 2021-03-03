#nullable enable

using ToggleableBindings;

namespace ToggleableBindings.Utility
{
    /// <summary>
    /// Provides a way to return a value along with extra information.
    /// </summary>
    public class ResultInfo<T>
    {
        /// <summary>
        /// The held value of this <see cref="ResultInfo{T}"/>.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The information provided with this <see cref="ResultInfo{T}"/>.
        /// </summary>
        public string? Information { get; set; }

        /// <summary>
        /// Creates a new <see cref="ResultInfo{T}"/> with the given value and information string.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        /// <param name="information">The information provided with the result.</param>
        public ResultInfo(T value, string? information = null)
        {
            Value = value;
            Information = information;
        }

        /// <summary>
        /// Implicitly converts a value into a new <see cref="ResultInfo{T}"/>.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public static implicit operator ResultInfo<T>(T value) => new(value);

        /// <summary>
        /// Implicitly converts a tuple of a value and a string into a new <see cref="ResultInfo{T}"/>.
        /// </summary>
        /// <param name="value">The tuple to use.</param>
        public static implicit operator ResultInfo<T>((T Value, string Info) value) => new(value.Value, value.Info);
    }
}