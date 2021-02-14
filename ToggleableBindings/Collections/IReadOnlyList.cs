namespace ToggleableBindings.Collections
{
    /// <summary>
    /// Provides a read-only view of a generic list.
    /// </summary>
    /// <typeparam name="T">The type of the items within the list.</typeparam>
    public interface IReadOnlyList<T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }
}