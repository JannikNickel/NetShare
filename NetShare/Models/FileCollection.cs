﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetShare.Models
{
    public class FileCollection
    {
        private static readonly char[] dirSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private readonly List<string> inputFiles;
        private readonly List<FileInfo> fileEntries = new List<FileInfo>();

        public FileCollection(string[] files)
        {
            inputFiles = FilteredInput(files);
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

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(dirSeparators).ToUpperInvariant();
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
                long size = 0;
                fileEntries.Clear();
                foreach(string input in inputFiles)
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(input);
                        if(attributes.HasFlag(FileAttributes.Directory))
                        {
                            foreach(string file in Directory.EnumerateFiles(input, "", SearchOption.AllDirectories))
                            {
                                AddFile(file, ref size);
                            }
                        }
                        else
                        {
                            AddFile(input, ref size);
                        }
                        progress?.Report((fileEntries.Count, size / 1024d / 1024d));
                    }
                    catch { }
                }
            });
        }

        private void AddFile(string file, ref long totalSize)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
                fileEntries.Add(fileInfo);
            }
            catch { }
        }
    }
}
