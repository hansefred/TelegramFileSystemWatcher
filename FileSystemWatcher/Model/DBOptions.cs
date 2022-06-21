using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Model
{
    public class DBOptions
    {
        public const string Position = "DBOptions";
        public string DefaultConnection { get; init; } = String.Empty;
        public string ChatConnection { get; init; } = String.Empty;
        public string SystemDataConnection { get; init; } = String.Empty;

    }
}
