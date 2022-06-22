using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Model
{
    public class ProgramOptions
    {
        public const string Position = "APISettings";
        public string APIKey { get; set; }
        public TimeSpan APIPoolingInverval { get; set; }
        public TimeSpan FilePoolingInverval { get; set; }

        public string WatchingDir { get; set; }
    }
}
