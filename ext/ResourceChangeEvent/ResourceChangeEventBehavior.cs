using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Itschwabing.Libraries.ResourceChangeEvent
{
    public class ResourceChangeEventBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ResourceProperty = DependencyProperty.Register(
            "Resource", typeof(object), typeof(ResourceChangeEventBehavior), new PropertyMetadata(default(object), ResourceChangedCallback));

        public event EventHandler<ResourceChangeEventArgs> ResourceChanged;

        public object Resource
        {
            get { return GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value); }
        }

        private static void ResourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (!(dependencyObject is ResourceChangeEventBehavior resourceChangeNotifier))
            {
                return;
            }

            resourceChangeNotifier.OnResourceChanged(new ResourceChangeEventArgs(args.OldValue, args.NewValue));
        }

        private void OnResourceChanged(ResourceChangeEventArgs args)
        {
            ResourceChanged?.Invoke(this, args);
        }
    }
}
