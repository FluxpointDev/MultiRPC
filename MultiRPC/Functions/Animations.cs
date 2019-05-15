using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MultiRPC.Functions
{
    internal class Animations
    {
        public static async Task ImageFadeAnimation(Image image, double to, Storyboard storyboard = null,
            Duration duration = new Duration())
        {
            if (storyboard == null)
                storyboard = new Storyboard();
            var fadeAnimation = new DoubleAnimation
            {
                From = image.Opacity,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration,
                EasingFunction = new QuinticEase()
            };

            storyboard.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, image.Name);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Begin(image);
            await Task.Delay(fadeAnimation.Duration.TimeSpan);
        }
    }
}