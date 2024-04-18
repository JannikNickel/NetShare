using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetShare.Models
{
    public class FileCollection
    {
        private static readonly char[] dirSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private readonly string rootDir;
        private readonly List<string> inputFiles;
        private readonly List<FileSystemInfo> fileEntries = new List<FileSystemInfo>();
        private long totalSize = 0;

        public string RootPath => rootDir;
        public IEnumerable<FileSystemInfo> Entries => fileEntries;
        public int EntryCount => fileEntries.Count;
        public long TotalSize => totalSize;

        public FileCollection(string[] files)
        {
            inputFiles = FilteredInput(files);
            rootDir = GetRootDir(inputFiles);
        }

        private static List<string> FilteredInput(string[] input)
        {
            string[] sorted = input.Select(NormalizePath).Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(n => n.Length).ToArray();
            List<string> res = new List<string>();
            foreach(string path in sorted)
            {
                if(!res.Any(n => IsSubdirectoryOf(n, path)))
                {
                    res.Add(path);
                }
            }
            return res;
        }

        private static string GetRootDir(IEnumerable<string> files)
        {
            string? shortest = null;
            foreach(string file in files)
            {
                string? dir = Path.GetDirectoryName(file);
                if(dir != null)
                {
                    shortest ??= dir;
                    if(dir.Length < shortest.Length && files.All(n => n.StartsWith(dir)))
                    {
                        shortest = dir;
                    }
                }
            }
            return shortest ?? "";
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(dirSeparators);
        }

        private static bool IsSubdirectoryOf(string parent, string child)
        {
            string[] parentParts = parent.Split(dirSeparators, StringSplitOptions.RemoveEmptyEntries);
            string[] childParts = child.Split(dirSeparators, StringSplitOptions.RemoveEmptyEntries);

            if(parentParts.Length >= childParts.Length)
            {
                return false;
            }

            for(int i = 0;i < parentParts.Length;i++)
            {
                if(!parentParts[i].Equals(childParts[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task LoadFilesAsync(IProgress<(int files, double size)> progress)
        {
            await Task.Run(() =>
            {
                totalSize = 0;
                fileEntries.Clear();
                foreach(string input in inputFiles)
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(input);
                        if(attributes.HasFlag(FileAttributes.Directory))
                        {
                            AddEntry(input, progress);

                            foreach(string file in Directory.EnumerateFileSystemEntries(input, "", SearchOption.AllDirectories))
                            {
                                AddEntry(file, progress);
                            }
                        }
                        else
                        {
                            AddEntry(input, progress);
                        }
                    }
                    catch { }
                }
            });
        }

        private void AddEntry(string entry, IProgress<(int files, double size)> progress)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(entry);
                FileSystemInfo info;
                if(attributes.HasFlag(FileAttributes.Directory))
                {
                    info = new DirectoryInfo(entry);
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(entry);
                    totalSize += fileInfo.Length;
                    info = fileInfo;
                }
                fileEntries.Add(info);
                progress?.Report((fileEntries.Count, totalSize / 1024d / 1024d));
            }
            catch { }
        }
    }
}
