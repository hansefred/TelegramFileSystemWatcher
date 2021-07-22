using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FileSystemWatcher.Model
{
    public class TelegramMessage
    {

        public int id { get; set; }

        public long Chat_id { get; set; }

        public DateTime Created { get; set; }
        [StringLength(250)]
        public string Message { get; set; }

        [StringLength(150)]
        public string User_FirstName { get; set; }
        [StringLength(150)]
        public string User_LastName { get; set; }
        [StringLength(150)]
        public string User_UserName  { get; set; }



    }
}
