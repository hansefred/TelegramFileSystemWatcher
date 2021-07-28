using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Model
{
    public class SystemData
    {

        public int id { get; set; }
        public long ProcessedFiles { get; set; }
        public long SendPhotos { get; set; }
        public long SendVideos { get; set; }

        public DateTime CreatedDate { get; set; }


        public override string ToString()
        {
            return $"ProcessedFiles: {ProcessedFiles}, SendedPhotos: {SendPhotos}, Sended Videos: {SendVideos} last change(UTC Time): {CreatedDate.ToLongDateString()}";
        }
    }
}
