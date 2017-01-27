using System;
using System.Collections;
using System.Collections.Generic;

namespace CountLinesOfCodeInDirectory
{
    public static class DumpExtensions
    {
        public static void Dump<T>(this IEnumerable<T> col, string header = null, bool inGrid = false) 
        {
            var oldColor = Console.ForegroundColor;
            if (!string.IsNullOrWhiteSpace(header))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(header);
                Console.ForegroundColor = oldColor;
            }

            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var item in col)
            {
                foreach (var prop in properties)
                {
                    var val = prop.GetValue(item);
                    Console.Write($"{val}\t");
                }
                Console.WriteLine();
            }
        }
    }
}