using FileSystemWatcher.Areas.Identity;
using FileSystemWatcher.Data;
using FileSystemWatcher.Model;
using FileSystemWatcher.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemWatcher
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .CreateLogger();


            services.Configure<DBOptions>(o =>
            {
                if (Environment.GetEnvironmentVariable ("APPDBConnectingString") != null || Environment.GetEnvironmentVariable("ChatLogConnectingString") != null ||  Environment.GetEnvironmentVariable("SystemDataConnectingString") != null)
                {
                    o.ChatConnection = Environment.GetEnvironmentVariable("ChatLogConnectingString");
                    o.DefaultConnection = Environment.GetEnvironmentVariable("APPDBConnectingString");
                    o.SystemDataConnection = Environment.GetEnvironmentVariable("SystemDataConnectingString");

                }
                else
                {
                    Log.Error("Not all ENV Variable for DB connect set, Exit!");
                    Environment.Exit(-1);
                }


            });


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Environment.GetEnvironmentVariable("APPDBConnectingString"), ServerVersion.AutoDetect(
                    Environment.GetEnvironmentVariable("APPDBConnectingString"))));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();


           



            services.Configure<ProgramOptions>(options =>
            {
                if (Environment.GetEnvironmentVariable("APIKey") != null || Environment.GetEnvironmentVariable("WatchDir") != null || Environment.GetEnvironmentVariable("PoolingInverval") != null)
                {
                    options.PoolingInverval = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("PoolingInverval")));
                    options.APIKey = Environment.GetEnvironmentVariable("APIKey");
                    options.WatchingDir = Environment.GetEnvironmentVariable("WatchDir");
                }
                else
                {
                    Log.Error("PoolingInterval, WatchingDir or APIKey not set ! Exit");
                    Environment.Exit(-1);
                }

            });


            services.AddSingleton<ChatLogContextFactory>();
            services.AddSingleton<SystemDataFactory>();
            services.AddSingleton<TelegramBotService>();
            services.AddSingleton<SystemDataService>();


            services.AddHostedService<HostedTelegramService>();
            services.AddHostedService<HostedFileSystemWatcher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<DBOptions> options)
        {

            WaitforDB(options.Value.DefaultConnection, Log.Logger);
    

            var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();



            Log.Logger.Information("Migrate ApplicationContext...");
            var context1 = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context1.Database.Migrate();

            Log.Logger.Information("Migrate ChatLogContext...");
            var Chatfactory = serviceScope.ServiceProvider.GetService<ChatLogContextFactory>();
            using (var context2 = Chatfactory.Create())
            {
                context2.Database.Migrate();
            }

            Log.Logger.Information("Migrate SystemDataContext...");
            var factory = serviceScope.ServiceProvider.GetService<SystemDataFactory>();
            using (var context3 = factory.Create())
            {
                context3.Database.Migrate();
            }


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }




        public static void WaitforDB(string ConnectingString, ILogger logger, int Port = 3306, int Retry = 100)
        {
            int Start = ConnectingString.IndexOf("Server=") + 7;
            int End = ConnectingString.IndexOf(";", ConnectingString.IndexOf("Server=")) - (ConnectingString.IndexOf("Server=") + 7);

            string Server = ConnectingString.Substring(Start, End);

            int i = 1;

            while (i < Retry)
            {
                logger.Information($"Try to connect to Server: {Server}... ({i} - {Retry})");
                Ping PingSender = new Ping();
                try
                {
                    var Reply = PingSender.Send(Server);

                    if (Reply.Status == IPStatus.Success)
                    {
                        logger.Information($"Ping Sucess, try to connect to Port: {Port}... ({i})");
                        using (TcpClient tcpClient = new TcpClient())
                        {
                            try
                            {
                                tcpClient.Connect(Reply.Address.ToString(), Port);
                                logger.Information("Port open");
                                return;
                            }
                            catch
                            {
                                logger.Information("Port closed Wait and Try again... ");
                                Thread.Sleep(4000);
                                i++;
                            }
                        }
                    }
                }
                catch
                {
                    logger.Information("Wait and Try again... ");
                    Thread.Sleep(500);
                    i++;
                }


            }
        }
    }
}