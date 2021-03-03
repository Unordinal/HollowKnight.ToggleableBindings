#nullable enable

namespace ToggleableBindings
{
    /// <summary>
    /// Represents a delegate for handing events dealing with <see cref="Binding"/> objects.
    /// </summary>
    /// <param name="binding">The binding.</param>
    public delegate void BindingEventHandler(Binding binding);
}