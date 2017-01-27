using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CountLinesOfCodeInDirectory
{
    class MainClass
    {
        private readonly OperatingSystem operatingSystem = Environment.OSVersion;
        private readonly char dirSep = '\\';
        private readonly string dirSepRegex = @"\\";
        private readonly Regex regex;

        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                new MainClass().Run(args[0]);
            }
            else
            {
                Console.WriteLine($"usage: {Assembly.GetEntryAssembly().GetName().Name} dirpath");
            }
#if DEBUG
            Console.ReadLine();
#endif
        }

        public MainClass()
        {
            if (operatingSystem.Platform == PlatformID.MacOSX || operatingSystem.Platform == PlatformID.Unix)
            {
                dirSep = '/';
                dirSepRegex = "/";
            }

            var regexPattern = $@"{dirSepRegex}(\.git|\$tf[0-9]*|\.vs|packages|bin|obj){dirSepRegex}|\.((.*-arc)|dsk|7z|zip|exe|com|obj|dll|msi|gif|ico|png|jpg|pdb|bak|snk|diagram|(res|rtf|doc|xls|ppt)x?)$";
            regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        void Run(string dirPath)
        {
            if (dirPath[dirPath.Length - 1] != dirSep)
                dirPath = dirPath + dirSep;

            var watch = DateTime.Now;

            var files = Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories).Where(fileName => !regex.IsMatch(fileName)).Select(fileName =>
                {
                    var firstNewPos = fileName.IndexOf(dirSep, dirPath.Length) - (dirPath.Length);
                    var projectName = "";
                    if (firstNewPos > 0)
                    {
                        projectName = fileName.Substring(dirPath.Length, firstNewPos);
                    }
                    var linesOfCode = 0;
                    try
                    {
                        linesOfCode = File.ReadAllLines(fileName).Length;
                    }
                    catch
                    {
                        linesOfCode = 0;
                    }

                    var extension = Path.GetExtension(fileName).Substring(1);

                    return new { ProjectName = projectName, FileType = extension, FileName = fileName, LinesOfCode = linesOfCode };
                }).ToList();

            files.OrderBy(f => f.ProjectName).Dump($"Files: {DateTime.Now - watch}", true);

            files.GroupBy(f => new { f.ProjectName, f.FileType }).Select(g => new
            {
                ProjectName = g.Key.ProjectName,
                FileType = g.Key.FileType,
                LinesOfCode = g.Sum(l => l.LinesOfCode)
            })
                 .OrderBy(f => f.ProjectName).ThenBy(f => f.FileType)
                 .Dump($"Projects with type: {DateTime.Now - watch}", true);

            files.GroupBy(f => f.ProjectName).Select(g => new
            {
                ProjectName = g.Key,
                LinesOfCode = g.Sum(l => l.LinesOfCode)
            })
                 .OrderBy(f => f.ProjectName)
                 .Dump($"Projects: {DateTime.Now - watch}", true);
        }
    }

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

        public static void Dump<T>(this T item, string header = null, bool inGrid = false)
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
            foreach (var prop in properties)
            {
                var val = prop.GetValue(item);
                Console.WriteLine($"{prop.Name}: {val}");
            }
            Console.WriteLine();
        }
    }
}
