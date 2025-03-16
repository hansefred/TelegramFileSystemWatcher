using FileSystemWatcher.Areas.Identity;
using FileSystemWatcher.Data;
using FileSystemWatcher.Model;
using FileSystemWatcher.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

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



            services.Configure<DBOptions>(Configuration.GetSection(DBOptions.Position));

            services.AddDbContext<ApplicationDbContext>();


            
                    
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();


           

            

            services.Configure<ProgramOptions> (Configuration.GetSection(ProgramOptions.Position));


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

            Log.Logger.Information("App is in Dev: {DevState}", env.IsDevelopment());


            Log.Logger.Information("Waiting DB Connection...");
            WaitforDB(options.Value.DefaultConnection, Log.Logger);


            Thread.Sleep(TimeSpan.FromSeconds(5));

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

           // app.UseHttpsRedirection();
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
