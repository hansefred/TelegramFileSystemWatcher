using FileSystemWatcher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Data
{
    public class ChatLogContext : DbContext
    {
        public ChatLogContext()
        {

        }
  
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseMySql("Server=DB;Port=3306;Database=ChatDB;Uid=root;Pwd=root;", ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=ChatDB;Uid=root;Pwd=root;"));
                }
                base.OnConfiguring(optionsBuilder);
            }
            public ChatLogContext(DbContextOptions<ChatLogContext> options)
                : base(options)
            {
            }

        public DbSet<TelegramMessage> Messages { get; set; }

        public DbSet<FileSystemSubscription> FileSystemSubscriptions { get; set; }



    }
    public class ChatLogContextFactory
    {

        private readonly DBOptions _dBOptions;
        public ChatLogContextFactory(IOptions<DBOptions> options)
        {
            _dBOptions = options.Value;
        }
        public ChatLogContext Create()
        {
            var options = new DbContextOptionsBuilder<ChatLogContext>()
                .UseMySql(_dBOptions.ChatConnection, ServerVersion.AutoDetect(_dBOptions.ChatConnection)).Options;


            return new ChatLogContext(options);
        }
    }
}

