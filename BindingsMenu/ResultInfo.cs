#nullable enable

namespace TogglableBindings
{
    /// <summary>
    /// Provides a way to return a value along with extra information.
    /// </summary>
    public class ResultInfo<T>
    {
        public T Value { get; set; }

        public string? Information { get; set; }

        public ResultInfo(T value, string? information = null)
        {
            Value = value;
            Information = information;
        }

        public static implicit operator ResultInfo<T>(T value) => new(value);

        public static implicit operator ResultInfo<T>((T Value, string Info) value) => new(value.Value, value.Info);
    }
}