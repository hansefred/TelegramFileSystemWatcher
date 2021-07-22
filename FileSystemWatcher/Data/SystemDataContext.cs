using FileSystemWatcher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWatcher.Data
{
    public class SystemDataContext : DbContext
    {
        public SystemDataContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=DB;Port=3306;Database=SystemDataDB;Uid=root;Pwd=root;", ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=SystemDataDB;Uid=root;Pwd=root;"));
            }
            base.OnConfiguring(optionsBuilder);
        }
        public SystemDataContext(DbContextOptions<SystemDataContext> options)
            : base(options)
        {
        }

        public DbSet<SystemData> SystemDatas { get; set; }


    }
    public class SystemDataFactory
    {

        private readonly DBOptions _dBOptions;
        public SystemDataFactory(IOptions<DBOptions> options)
        {
            _dBOptions = options.Value;
        }
        public SystemDataContext Create()
        {
            var options = new DbContextOptionsBuilder<SystemDataContext>()
                .UseMySql(_dBOptions.SystemDataConnection, ServerVersion.AutoDetect(_dBOptions.SystemDataConnection)).Options;


            return new SystemDataContext(options);
        }
    }

}
