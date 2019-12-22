using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ConvertForm
{
    /// <summary>
    /// Credits to https://stackoverflow.com/questions/14485818/how-to-update-a-progress-bar-so-it-increases-smoothly
    /// </summary>
    public static class ProgressBarExtensions
    {
        private static TimeSpan duration = TimeSpan.FromSeconds(2);

        public static void SetPercent(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
