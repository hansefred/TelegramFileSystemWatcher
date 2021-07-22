using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Model
{
    public class ProgramOptions
    {
        public string APIKey { get; set; }
        public TimeSpan PoolingInverval { get; set; }

        public string WatchingDir { get; set; }
    }
}
