using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiRPC.Functions
{
    internal class Animations
    {
        public static async Task DoubleAnimation(FrameworkElement element, double to, Storyboard storyboard = null,
            Duration duration = new Duration())
        {
            if (storyboard == null)
                storyboard = new Storyboard();
            var fadeAnimation = new DoubleAnimation
            {
                From = element.Opacity,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration,
                EasingFunction = new QuinticEase()
            };

            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Begin(element);
            await Task.Delay(fadeAnimation.Duration.TimeSpan);
        }

        public static async Task ThicknessAnimation(FrameworkElement element, Thickness to,
            Thickness from, Duration duration = new Duration(), PropertyPath propertyPath = null)
        {
            if (propertyPath == null)
                propertyPath = new PropertyPath(FrameworkElement.MarginProperty);
            var storyboard = new Storyboard();
            var fadeAnimation = new ThicknessAnimation
            {
                From = from,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.3)) : duration,
                EasingFunction = new CircleEase()
            };

            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeAnimation, propertyPath);
            storyboard.Begin(element);
            await Task.Delay(fadeAnimation.Duration.TimeSpan);
        }
    }
}