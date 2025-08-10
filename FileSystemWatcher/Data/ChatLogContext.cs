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

        private readonly DBOptions _options;

        public ChatLogContext(IOptions<DBOptions> options)
        {
            _options = options.Value;
        }
  
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseMySQL();
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
                .UseMySQL(_dBOptions.ChatConnection).Options;


            return new ChatLogContext(options);
        }
    }
}

