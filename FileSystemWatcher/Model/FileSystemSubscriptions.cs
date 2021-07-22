using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Model
{
    public class FileSystemSubscription
    {

        public int id { get; set; }

        public long Chat_ID { get; set; }
        [StringLength(150)]
        public string User_FirstName { get; set; }
        [StringLength(150)]
        public string User_LastName { get; set; }
        [StringLength(150)]
        public string User_UserName { get; set; }
    }
}
