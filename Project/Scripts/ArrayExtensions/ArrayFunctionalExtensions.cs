/* This class provides a clean, expressive API for working with grids (2D arrays), making extensive use of C#'s first-class
 * treatment of functions. */

using System;

namespace AKSaigyouji.ArrayExtensions
{
    public static class ArrayFunctionalExtensions
    {
        public static void SetAll<T>(this T[,] grid, T value)
        {
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = value;
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEach<T>(this T[,] grid, Action<int, int> action)
        {
            ThrowIfNull(action);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    action(x, y);
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEachBoundary<T>(this T[,] grid, Action<int, int> action)
        {
            ThrowIfNull(action);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int x = 0; x < length; x++)
            {
                action(x, 0);
                action(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++)
            {
                action(0, y);
                action(length - 1, y);
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEachInterior<T>(this T[,] grid, Action<int, int> action)
        {
            ThrowIfNull(action);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    action(x, y);
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEach<T>(this T[,] grid, Action<int, int> action, Func<int, int, bool> predicate)
        {
            ThrowIfNull(action);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (predicate(x, y))
                    {
                        action(x, y);
                    }
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEachBoundary<T>(this T[,] grid, Action<int, int> action, Func<int, int, bool> predicate)
        {
            ThrowIfNull(action);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int x = 0; x < length; x++)
            {
                if (predicate(x, 0))
                    action(x, 0);
                if (predicate(x, width - 1))
                    action(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++) // adjust boundaries so we don't double-visit 0,0 and length-1,width-1
            {
                if (predicate(0, y))
                    action(0, y);
                if (predicate(length - 1, y))
                    action(length - 1, y);
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEachInterior<T>(this T[,] grid, Action<int, int> action, Func<int, int, bool> predicate)
        {
            ThrowIfNull(action);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    if (predicate(x, y))
                    {
                        action(x, y);
                    }
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void Transform<T>(this T[,] grid, Func<int, int, T> transformation)
        {
            ThrowIfNull(transformation);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = transformation(x, y);
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void TransformBoundary<T>(this T[,] grid, Func<int, int, T> transformation)
        {
            ThrowIfNull(transformation);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int x = 0; x < length; x++)
            {
                grid[x, 0] = transformation(x, 0);
                grid[x, width - 1] = transformation(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++)
            {
                grid[0, y] = transformation(0, y);
                grid[length - 1, y] = transformation(length - 1, y);
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void TransformInterior<T>(this T[,] grid, Func<int, int, T> transformation)
        {
            ThrowIfNull(transformation);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    grid[x, y] = transformation(x, y);
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void Transform<T>(this T[,] grid, Func<int, int, T> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (predicate(x, y))
                    {
                        grid[x, y] = transformation(x, y);
                    }
                }
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void TransformBoundary<T>(this T[,] grid, Func<int, int, T> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int x = 0; x < length; x++)
            {
                if (predicate(x, 0))
                    grid[x, 0] = transformation(x, 0);
                if (predicate(x, width - 1))
                    grid[x, width - 1] = transformation(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++) // adjust boundaries so we don't double-visit 0,0 and length-1,width-1
            {
                if (predicate(0, y))
                    grid[0, y] = transformation(0, y);
                if (predicate(length - 1, y))
                    grid[length - 1, y] = transformation(length - 1, y);
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void TransformInterior<T>(this T[,] grid, Func<int, int, T> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);
            int length = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    if (predicate(x, y))
                    {
                        grid[x, y] = transformation(x, y);
                    }
                }
            }
        }

        static void ThrowIfNull<T>(Func<int, int, T> transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
        }

        static void ThrowIfNull(Func<int, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
        }

        static void ThrowIfNull(Action<int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
        }
    } 
}