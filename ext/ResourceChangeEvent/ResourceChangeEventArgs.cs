using System;

namespace Itschwabing.Libraries.ResourceChangeEvent
{
    public class ResourceChangeEventArgs : EventArgs
    {
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }

        public ResourceChangeEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}