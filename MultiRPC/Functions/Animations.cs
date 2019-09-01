using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace MultiRPC.Functions
{
    internal class Animations
    {
        public static async Task DoubleAnimation(
            FrameworkElement element, 
            double to, 
            double from,
            Storyboard storyboard = null,
            Duration duration = new Duration(), 
            PropertyPath propertyPath = null,
            IEasingFunction ease = null)
        {
            if (storyboard == null)
            {
                storyboard = new Storyboard();
            }

            if (propertyPath == null)
            {
                propertyPath = new PropertyPath(UIElement.OpacityProperty);
            }

            //storyboard ??= new Storyboard();
            //propertyPath ??= new PropertyPath(UIElement.OpacityProperty);

            var fadeAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration,
                EasingFunction = ease ?? new QuadraticEase()
            };

            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeAnimation, propertyPath);
            storyboard.Begin(element);
            await Task.Delay(fadeAnimation.Duration.TimeSpan);
        }

        public static async Task ThicknessAnimation(
            FrameworkElement element, 
            Thickness to,
            Thickness from, 
            Duration duration = new Duration(),
            PropertyPath propertyPath = null,
            IEasingFunction ease = null)
        {
            if (propertyPath == null)
            {
                propertyPath = new PropertyPath(FrameworkElement.MarginProperty);
            }

            //propertyPath ??= new PropertyPath(FrameworkElement.MarginProperty);
    
            var storyboard = new Storyboard();
            var fadeAnimation = new ThicknessAnimation
            {
                From = from,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.3)) : duration,
                EasingFunction = ease ?? new CircleEase()
            };

            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeAnimation, propertyPath);
            storyboard.Begin(element);
            await Task.Delay(fadeAnimation.Duration.TimeSpan);
        }
    }
}