using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CountLinesOfCodeInDirectory
{
    internal class MainClass
    {
        private readonly char dirSep = '\\';
        private readonly string dirSepRegex = @"\\";
        private readonly OperatingSystem operatingSystem = Environment.OSVersion;
        private readonly Regex regex;

        public MainClass()
        {
            if ((operatingSystem.Platform == PlatformID.MacOSX) || (operatingSystem.Platform == PlatformID.Unix))
            {
                dirSep = '/';
                dirSepRegex = "/";
            }

            var dirPattern = $@"{dirSepRegex}(\.git|\$tf[0-9]*|\.vs|packages|bin|obj){dirSepRegex}";
            const string extPattern = @"\.((.*-arc)|dsk|7z|zip|exe|com|obj|dll|msi|gif|ico|png|jpg|pdb|bak|snk|diagram|(res|rtf|doc|xls|ppt)x?)$";
            var regexPattern = $"{dirPattern}|{extPattern}";

            regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

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

        private void Run(string dirPath)
        {
            var watch = DateTime.Now;

            List<FileInfoWithLinesOfCode> files = GetFileInfosWithLineOfCodes(dirPath).Take(100).ToList();

            files.OrderBy(f => f.ProjectName).Dump($"Files: {DateTime.Now - watch}", true);

            files.GroupBy(f => new {f.ProjectName, f.FileType})
                 .Select(g => new {g.Key.ProjectName, g.Key.FileType, LinesOfCode = g.Sum(l => l.LinesOfCode)})
                 .OrderBy(f => f.ProjectName)
                 .ThenBy(f => f.FileType)
                 .Dump($"Projects with type: {DateTime.Now - watch}", true);

            files.GroupBy(f => f.ProjectName)
                 .Select(g => new {ProjectName = g.Key, LinesOfCode = g.Sum(l => l.LinesOfCode)})
                 .OrderBy(f => f.ProjectName)
                 .Dump($"Projects: {DateTime.Now - watch}", true);
        }

        private IEnumerable<FileInfoWithLinesOfCode> GetFileInfosWithLineOfCodes(string dirPath)
        {
            if (dirPath[dirPath.Length - 1] != dirSep)
            {
                dirPath = dirPath + dirSep;
            }

            return
                Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
                         .Where(fileName => !regex.IsMatch(fileName))
                         .AsParallel()
                         .Select(
                             fileName =>
                             {
                                 var firstNewPos = fileName.IndexOf(dirSep, dirPath.Length) - dirPath.Length;
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

                                 var extension = Path.GetExtension(fileName);
                                 extension = extension.Length > 1 ? extension.Substring(1).ToUpper() : "";

                                 return new FileInfoWithLinesOfCode
                                        {
                                            ProjectName = projectName,
                                            FileType = extension,
                                            FileName = fileName,
                                            LinesOfCode = linesOfCode
                                        };
                             });
        }
    }
}
