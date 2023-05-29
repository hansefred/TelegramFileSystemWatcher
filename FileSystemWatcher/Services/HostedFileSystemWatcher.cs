using FileSystemWatcher.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FileSystemWatcher.Services
{
    public class HostedFileSystemWatcher : IHostedService
    {


        private readonly ProgramOptions _options;
        private readonly ILogger<HostedFileSystemWatcher> _logger;
        private readonly TelegramBotService _telegramBotService;
        private readonly SystemDataService _systemDataService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public HostedFileSystemWatcher(IOptions<ProgramOptions> options, TelegramBotService telegramBotService, SystemDataService systemDataService, ILogger<HostedFileSystemWatcher> logger)
        {
            _systemDataService = systemDataService;
            _options = options.Value;
            _logger = logger;
            _telegramBotService = telegramBotService;

            _logger.LogInformation("Watch for new Files in {FilePath} every {PoolTime} sec", _options.WatchingDir, _options.FilePoolingInverval.TotalSeconds);
            _fileSystemWatcher = new FileSystemWatcher(_options.WatchingDir, _options.FilePoolingInverval);
            _fileSystemWatcher.NewFileCreated += _fileSystemWatcher_NewFileCreated;


        }

        private void _fileSystemWatcher_NewFileCreated(object sender, NewFileEventArgs e)
        {

            // if file no directory 
            if (File.Exists(e.FileInfo.FullName))
            {
                _logger.LogInformation($"Neue Datei {e.FileInfo.Name}");
                _systemDataService.IncProcessedFiles();

                Task.Run(async () =>
                {




                    try
                    {


                        FileInfo file = e.FileInfo;



                        if (file.Extension.ToLower() == ".mp4")
                        {
                            WaitForFileComplete(file);

                            var Users = await _telegramBotService.GetSubscription();
                            if (Users.Count > 0)
                            {
                                await _telegramBotService.SendVideo(Users.Select(o => o.Chat_ID).ToList(), e.FileInfo.FullName, e.FileInfo.Name);
                                try
                                {
                                    File.Delete(e.FileInfo.FullName);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error deleting File: {e.FileInfo.FullName}, Error: {ex.Message}");
                                }

                            }
                            else
                            {
                                _logger.LogInformation("No Subscription !");
                            }



                        }
                        if (file.Extension.ToLower() == ".jpg" || file.Extension.ToLower() == ".png")
                        {
                            WaitForFileComplete(file);
                            var Users = await _telegramBotService.GetSubscription();
                            if (Users.Count > 0)
                            {
                                await _telegramBotService.SendPhoto(Users.Select(o => o.Chat_ID).ToList(), "", e.FileInfo.FullName, e.FileInfo.Name);
                                try
                                {
                                    File.Delete(e.FileInfo.FullName);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error deleting File: {e.FileInfo.FullName}, Error: {ex.Message}");
                                }

                            }
                            else
                            {
                                _logger.LogInformation("No Subscription !");
                            }

                        }


                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Fehler beim verarbeiten der Datei: {ex.Message}");

                    }


                });
            }

        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        static void WaitForFileComplete(FileInfo file)
        {
            Log.Logger.Information($"Wait for changing Extension: {file.Extension}");
            while (file.Extension.Contains("_"))
            {
                file = new FileInfo(file.FullName);
                Thread.Sleep(100);
            }
            long FileLengh = 0;
            do
            {
                Log.Logger.Information($"Wait for Filesize Extension: New: {file.Length} Old: {FileLengh.ToString()}");
                FileLengh = file.Length;
                Thread.Sleep(100);
            }
            while (file.Length != FileLengh);
        }




    }
}
