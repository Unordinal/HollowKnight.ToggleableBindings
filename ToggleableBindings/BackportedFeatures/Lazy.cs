using System;

namespace ToggleableBindings.BackportedFeatures
{
    public sealed class Lazy<T>
    {
        private readonly Func<T> _initializer;
        private bool _isValueInitialized;
        private T _value;

        public T Value
        {
            get
            {
                if (!_isValueInitialized)
                {
                    _value = _initializer();
                    _isValueInitialized = true;
                }

                return _value;
            }
        }

        public Lazy(Func<T> initializer)
        {
            if (initializer is null)
                throw new ArgumentNullException(nameof(initializer));

            _initializer = initializer;
        }
    }
}