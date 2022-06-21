using FileSystemWatcher.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileSystemWatcher.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        private readonly DBOptions _dBOptions;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_dBOptions.DefaultConnection, ServerVersion.AutoDetect(_dBOptions.DefaultConnection));
            }
            base.OnConfiguring(optionsBuilder);
        }
        public ApplicationDbContext(IOptions<DBOptions> DBOption)
        {
            _dBOptions = DBOption.Value;
        }
    }
}
