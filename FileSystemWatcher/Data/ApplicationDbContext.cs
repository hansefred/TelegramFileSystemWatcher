using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileSystemWatcher.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

   

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=DB;Port=3306;Database=APPDB;Uid=root;Pwd=root;",ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=APPDB;Uid=root;Pwd=root;"));
            }
            base.OnConfiguring(optionsBuilder);
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
