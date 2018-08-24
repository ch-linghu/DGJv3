﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGJv3
{
    internal static class Utilities
    {
        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal static readonly string ConfigPath = Path.Combine(AssemblyDirectory, "点歌姬");

        internal static readonly string SongsCachePath = Path.Combine(ConfigPath, "歌曲缓存");



    }
}