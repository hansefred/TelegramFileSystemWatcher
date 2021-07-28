using FileSystemWatcher.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Options;
using G2Development.FileWatcher;
using System.IO;
using System.Linq;

namespace FileSystemWatcher.Services
{
    public class HostedFileSystemWatcher : IHostedService
    {


        private readonly ProgramOptions _options;
        private readonly FileWatcher _fileSystemWatcher;
        private readonly ILogger _logger;
        private readonly TelegramBotService _telegramBotService;
        private readonly SystemDataService _systemDataService;

        public HostedFileSystemWatcher(IOptions<ProgramOptions> options, TelegramBotService telegramBotService, SystemDataService systemDataService)
        {
            _systemDataService = systemDataService;
            _options = options.Value;
            _logger = Log.Logger;
            _telegramBotService = telegramBotService;
            _fileSystemWatcher = new FileWatcher(_options.WatchingDir, "*.*");
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.AccessSense = AccessSensitivity.Restraint;
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }


        
        private void _fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            // if file no directory 
            if (File.Exists(e.FullPath))
            {
                _logger.Information($"Neue Datei {e.Name}");
                _systemDataService.IncProcessedFiles();

                Task.Run(async () =>
                {




                    try
                    {

                        if (e.ChangeType == System.IO.WatcherChangeTypes.Created)
                        {
                            FileInfo file = new FileInfo(e.FullPath);



                            if (file.Extension.ToLower().Contains(".mp4"))
                            {
                                WaitForFileComplete(file);

                                var Users = await _telegramBotService.GetSubscription();
                                await _telegramBotService.SendVideo(Users.Select(o => o.Chat_ID).ToList(), e.FullPath, e.Name);


                            }
                            if (file.Extension.ToLower().Contains(".jpg") || file.Extension.ToLower().Contains(".png"))
                            {
                                WaitForFileComplete(file);
                                var Users = await _telegramBotService.GetSubscription();
                                await _telegramBotService.SendPhoto(Users.Select(o => o.Chat_ID).ToList(), "", e.FullPath, e.Name);
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Fehler beim verarbeiten der Datei: {ex.Message}");

                    }
                    try
                    {

                        //File.Delete(e.FullPath);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error deleting File: {e.FullPath}, Error: {ex.Message}");

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
                file = new FileInfo (file.FullName);
                Thread.Sleep (100);
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
