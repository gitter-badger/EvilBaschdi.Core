﻿using System;
using System.IO;
using System.Linq;

namespace EvilBaschdi.Core.Extensions
{
    /// <summary>
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        ///     Extension to get size of a directory.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="dir" /> is <see langword="null" />.</exception>
        public static double GetDirectorySize(this DirectoryInfo dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }

            var sum = dir.GetFiles().Aggregate<FileInfo, double>(0, (current, file) => current + file.Length);
            return dir.GetDirectories().Aggregate(sum, (current, dir1) => current + GetDirectorySize(dir1));
        }

        /// <summary>
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        public static string GetProperDirectoryCapitalization(this DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;

            return null == parentDirInfo
                ? dirInfo.Name
                : Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                    parentDirInfo.GetDirectories(dirInfo.Name)[0].Name).Trim();
        }

        /// <summary>
        ///     Extension to rename a directory.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"><paramref name="dir" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">New name cannot be null or blank</exception>
        public static void RenameTo(this DirectoryInfo dir, string name)
        {
            if (dir?.Parent == null)
            {
                throw new ArgumentNullException(nameof(dir), "Directory info to rename cannot be null");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("New name cannot be null or blank", nameof(name));
            }

            dir.MoveTo(Path.Combine(dir.Parent.FullName, name));
        }
    }
}