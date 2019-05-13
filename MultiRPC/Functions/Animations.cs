using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MultiRPC.Functions
{
    class Animations
    {
        public static void ImageFadeAnimation(Image image, double to, Storyboard storyboard = null, Duration duration = new Duration())
        {
            if (storyboard == null)
                storyboard = new Storyboard();
            DoubleAnimation winOpacityAnimation = new DoubleAnimation
            {
                From = image.Opacity,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration,
                EasingFunction = new QuinticEase()
            };

            storyboard.Children.Add(winOpacityAnimation);
            Storyboard.SetTargetName(winOpacityAnimation, image.Name);
            Storyboard.SetTargetProperty(winOpacityAnimation, new PropertyPath(Image.OpacityProperty));
            storyboard.Begin(image);
        }
    }
}
