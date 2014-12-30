using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace LFNet.Common.Extensions
{
    /// <summary>
    /// Helper class to print out performance related data like number of runs, elapsed time and frequency
    /// </summary>
    public static class ProfileExtensions
    {
        private static NumberFormatInfo _myNumberFormat;
        private const string DefaultFormat = "Executed {runs} in {time} ({frequency}/s).";

        /// <summary>
        /// Execute the given function n-times and print the timing values (number of runs, elapsed time, call frequency)
        /// to the console window.
        /// </summary>
        /// <param name="func">Function to call in a for loop.</param>
        /// <param name="runs">Number of iterations.</param>
        /// <param name="format">Format string which can contain {runs} or {0},{time} or {1} and {frequency} or {2}.</param>
        public static void Profile(this Action func, int runs, string format = DefaultFormat)
        {
            Func<int> func2 = delegate {
                for (int j = 0; j < runs; j++)
                {
                    func();
                }
                return runs;
            };
            ProfileInternal(func2, format);
        }

        /// <summary>
        /// Execute the given function n-times and print the timing values (number of runs, elapsed time, call frequency)
        /// to the console window.
        /// </summary>
        /// <param name="func">Function to call in a for loop.</param>
        /// <param name="runs">Number of iterations.</param>
        /// <param name="format">Format string which can contain {runs} or {0},{time} or {1} and {frequency} or {2}.</param>
        public static void ProfileConcurrently(this Action func, int runs, string format = DefaultFormat)
        {
            Func<int> func2 = delegate {
                Parallel.For(0, runs, (Action<int>) (i => func()));
                return runs;
            };
            ProfileInternal(func2, format);
        }

        /// <summary>
        /// Call a function in a for loop n-times. The first function call will be measured independently to measure
        /// first call effects.
        /// </summary>
        /// <param name="func">Function to call in a loop.</param>
        /// <param name="runs">Number of iterations.</param>
        /// <param name="format">Format string for first function call performance.</param>
        /// <remarks>
        /// The format string can contain {runs} or {0},{time} or {1} and {frequency} or {2}.
        /// </remarks>
        public static void ProfileConcurrentlyWithWarmup(this Action func, int runs, string format = DefaultFormat)
        {
            func();
            func.ProfileConcurrently(runs, format);
        }

        private static void ProfileInternal(Func<int> func, string format = DefaultFormat)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int num = func();
            stopwatch.Stop();
            string str = format.Replace("{runs}", "{3}").Replace("{time}", "{4}").Replace("{frequency}", "{5}");
            string str2 = stopwatch.Elapsed.ToWords(true);
            float num2 = ((float) stopwatch.ElapsedMilliseconds) / 1000f;
            float num3 = ((float) num) / num2;
            try
            {
                Console.WriteLine(str, new object[] { num, str2, num3, num.ToString("N0", NumberFormat), str2, num3.ToString("N0", NumberFormat) });
            }
            catch (FormatException exception)
            {
                throw new FormatException(string.Format("The input string format string did contain not an expected token like {{runs}}/{{0}}, {{time}}/{{1}} or {{frequency}}/{{2}} or the format string itself was invalid: \"{0}\"", format), exception);
            }
        }

        /// <summary>
        /// Call a function in a for loop n-times. The first function call will be measured independently to measure
        /// first call effects.
        /// </summary>
        /// <param name="func">Function to call in a loop.</param>
        /// <param name="runs">Number of iterations.</param>
        /// <param name="format">Format string for first function call performance.</param>
        /// <remarks>
        /// The format string can contain {runs} or {0},{time} or {1} and {frequency} or {2}.
        /// </remarks>
        public static void ProfileWithWarmup(this Action func, int runs, string format = DefaultFormat)
        {
            func();
            func.Profile(runs, format);
        }

        private static NumberFormatInfo NumberFormat
        {
            get
            {
                if (_myNumberFormat == null)
                {
                    _myNumberFormat = CultureInfo.CurrentCulture.NumberFormat;
                }
                return _myNumberFormat;
            }
        }
    }
}

