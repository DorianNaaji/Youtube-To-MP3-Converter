using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace YoutubeToMP3.BusinessLogic
{
    public static class ProgressBarExtensions
    {
        private static readonly TimeSpan DurationLong = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DurationFast = TimeSpan.FromMilliseconds(250);

        public static void SetPercentDefault(this ProgressBar progressBar, double percentage)
        {
            var animation = new DoubleAnimation(percentage, DurationLong);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        public static void SetPercentFast(this ProgressBar progressBar, double percentage)
        {
            var animation = new DoubleAnimation(percentage, DurationFast);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
