using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace MultiRPC
{
    //ToDo: Find a way for this to not use a load of ram for short amount of time

    /// <summary>
    /// Helper for animating UI elements
    /// </summary>
    internal static class Animations
    {
        /// <summary>
        /// Animates the <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">element to animate</param>
        /// <param name="to">What the elements <see cref="PropertyPath"/> should be at the end of the animation</param>
        /// <param name="from">What the elements <see cref="PropertyPath"/> should be at the start of the animation</param>
        /// <param name="storyboard">Storyborad to use for the animation</param>
        /// <param name="duration">How long the animation should last for</param>
        /// <param name="propertyPath">The property to animate</param>
        /// <param name="ease">Any easing that should be used</param>
        /// <param name="startAnimation">If to start the animation</param>
        public static async Task DoubleAnimation(
            this FrameworkElement element,
            double to,
            double? from = default,
            Duration duration = new Duration(),
            DependencyProperty propertyDependency = null,
            IEasingFunction ease = null,
            FillBehavior fillBehavior = FillBehavior.Stop,
            bool waitForAnimation = true)
        {
            propertyDependency ??= UIElement.OpacityProperty;

            var fadeAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration,
                EasingFunction = ease ?? new QuadraticEase(),
                FillBehavior = fillBehavior
            };

            element.BeginAnimation(propertyDependency, fadeAnimation);

            if (waitForAnimation)
            {
                await Task.Delay(fadeAnimation.Duration.TimeSpan.Subtract(TimeSpan.FromMilliseconds(1)));
            }
        }

        /// <summary>
        /// Animates the <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">element to animate</param>
        /// <param name="to">What the elements <see cref="PropertyPath"/> should be at the end of the animation</param>
        /// <param name="from">What the elements <see cref="PropertyPath"/> should be at the start of the animation</param>
        /// <param name="storyboard">Storyborad to use for the animation</param>
        /// <param name="duration">How long the animation should last for</param>
        /// <param name="propertyPath">The property to animate</param>
        /// <param name="ease">Any easing that should be used</param>
        /// <param name="startAnimation">If to start the animation</param>
        public static async Task ThicknessAnimation(
            this FrameworkElement element,
            Thickness to,
            Thickness from,
            Duration duration = new Duration(),
            DependencyProperty propertyDependency = null,
            IEasingFunction ease = null,
            FillBehavior fillBehavior = FillBehavior.Stop,
            bool waitForAnimation = true)
        {
            propertyDependency ??= FrameworkElement.MarginProperty;

            var thicknessAnimation = new ThicknessAnimation
            {
                From = from,
                To = to,
                Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.3)) : duration,
                EasingFunction = ease ?? new CircleEase(),
                FillBehavior = fillBehavior,
            };

            element.BeginAnimation(propertyDependency, thicknessAnimation);

            if (waitForAnimation)
            {
                await Task.Delay(thicknessAnimation.Duration.TimeSpan);
            }
        }
    }
}
