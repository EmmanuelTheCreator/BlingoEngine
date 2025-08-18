using System;
using System.Threading.Tasks;

namespace LingoEngine.Tools
{
    /// <summary>
    /// Helper utilities for conditional parallel execution based on workload size.
    /// </summary>
    public static class ParallelHelper
    {
        /// <summary>
        /// Minimum number of pixels required before parallel execution is used.
        /// </summary>
        public const int MinParallelPixels = 640 * 480;

        /// <summary>
        /// Executes <see cref="body"/> either sequentially or in parallel depending on
        /// <paramref name="totalPixels"/>. If the work is smaller than
        /// <see cref="MinParallelPixels"/>, a simple loop is used to avoid the overhead of
        /// <c>Parallel.For</c>.
        /// </summary>
        /// <param name="fromInclusive">Start index (inclusive).</param>
        /// <param name="toExclusive">End index (exclusive).</param>
        /// <param name="totalPixels">Approximate total work size in pixels.</param>
        /// <param name="body">Delegate to execute for each index.</param>
        public static void For(int fromInclusive, int toExclusive, int totalPixels, Action<int> body)
        {
            if (totalPixels >= MinParallelPixels)
            {
                Parallel.For(fromInclusive, toExclusive, body);
            }
            else
            {
                for (int i = fromInclusive; i < toExclusive; i++)
                {
                    body(i);
                }
            }
        }
    }
}

