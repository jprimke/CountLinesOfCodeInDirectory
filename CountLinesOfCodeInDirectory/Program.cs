using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CalcLinesOfCode
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            new MainClass().Run();
        }

        void Run()
        {
            var dirPath = @"/users/jprimke/Projekte/";
            var regex = new Regex(@"/(\.git|\$tf|\.vs|packages|bin|obj)/|\.((.*-arc)|dsk|zip|exe|com|obj|dll|msi|gif|ico|png|jpg|pdb|bak|snk|diagram|(res|rtf|doc|xls|ppt)x?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var watch = DateTime.Now;

            var files = Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories).Where(fileName => !regex.IsMatch(fileName)).Select(fileName =>
                {
                    var firstNewPos = fileName.IndexOf('/', dirPath.Length) - dirPath.Length;
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

                    return new { ProjectName = projectName, FileName = fileName, LinesOfCode = linesOfCode };
                }).ToList();

            files.OrderBy(f => f.ProjectName).Dump($"Files: {DateTime.Now - watch}", true);

            files.GroupBy(f => f.ProjectName).Select(g => new { Project = g.Key, LinesOfCode = g.Sum(l => l.LinesOfCode) }).OrderBy(f => f.Project).Dump($"Projects: {DateTime.Now - watch}", true);

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
    }
}
