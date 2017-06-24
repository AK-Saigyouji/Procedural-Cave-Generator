/* This is a growing collection of simple, general-purpose extension methods. When enough methods for a given type
 are written, they get refactored into a separate class.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.ArrayExtensions
{
    public static class ArrayFunctionalExtensions
    {
        public static void SetAll<T>(this T[,] arr, T value)
        {
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                for (int x = 0; x < arr.GetLength(0); x++)
                {
                    arr[x, y] = value;
                }
            }
        }

        public static void ForEach<T>(this T[,] arr, Action<int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            for (int y = 0; y < arr.GetLength(1); y++)
            {
                for (int x = 0; x < arr.GetLength(0); x++)
                {
                    action(x, y);
                }
            }
        }

        public static K[,] Transform<T, K>(this T[,] arr, Func<int, int, K> transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");

            K[,] result = new K[arr.GetLength(0), arr.GetLength(1)];
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                for (int x = 0; x < arr.GetLength(0); x++)
                {
                    result[x, y] = transformation(x, y);
                }
            }
            return result;
        }
    } 
}