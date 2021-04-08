using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace YoutubeToMP3.BusinessLogic
{
    /// <summary>
    /// Credits to https://stackoverflow.com/questions/14485818/how-to-update-a-progress-bar-so-it-increases-smoothly
    /// </summary>
    public static class ProgressBarExtensions
    {
        private static TimeSpan durationLong = TimeSpan.FromSeconds(2);

        private static TimeSpan durationFast = TimeSpan.FromMilliseconds(250);

        public static void SetPercentDefault(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, durationLong);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        public static void SetPercentFast(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, durationFast);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
