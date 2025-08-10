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
        private readonly DBOptions _options;
        public SystemDataContext(IOptions<DBOptions> options)
        {
            _options = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(_options.SystemDataConnection);
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
                .UseMySQL(_dBOptions.SystemDataConnection).Options;


            return new SystemDataContext(options);
        }
    }

}
