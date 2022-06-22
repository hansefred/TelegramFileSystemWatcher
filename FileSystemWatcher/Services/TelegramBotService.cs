using FileSystemWatcher.Data;
using FileSystemWatcher.Model;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace FileSystemWatcher.Services
{
    public class TelegramBotService
    {
        protected virtual void OnMessageRecived(Update e)
        {

            if (_handler != null)
            {
                _handler(this, e);
            }
        }

        private EventHandler<Update> _handler;
        private Task PoolingTask;
        public event EventHandler<Update> MessageRecived
        {
            add
            {
                PoolingTask = Task.Run(() =>
                {
                    var Timer = new System.Timers.Timer(_telegramOptions.APIPoolingInverval.TotalMilliseconds);
                    Timer.Start();

                    Timer.Elapsed += ((object sender, System.Timers.ElapsedEventArgs e) =>
                    {
                        var Updates = _botClient.GetUpdatesAsync().Result;

                        foreach (Update item in Updates)
                        {
                            try
                            {
                                using (var context = _logContextFactory.Create())
                                {
                                    context.Add(new TelegramMessage() { Created = item.Message.Date, User_FirstName = item.Message.From.FirstName, User_LastName = item.Message.From.LastName, User_UserName = item.Message.From.Username, Chat_id = item.Message.From.Id, Message = item.Message.Text });
                                    context.SaveChanges();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"Failed to Save ChatMessage {ex.Message}");
                            }
                            OnMessageRecived(item);
                            _botClient.GetUpdatesAsync(item.Id + 1).Wait();
                        }

                            (sender as System.Timers.Timer).Start();





                    });

                    while (true)
                    {

                        Thread.Sleep(500);
                    }

                });

                _handler += value;


            }


            remove
            {
                PoolingTask.Dispose();
                _handler -= value;
            }
        }



        private readonly ILogger _logger;
        private readonly TelegramBotClient _botClient;
        private readonly ChatLogContextFactory _logContextFactory;
        private readonly ProgramOptions _telegramOptions;
        private readonly SystemDataService _systemDataService;

        public TelegramBotService(IOptions<ProgramOptions> options, ChatLogContextFactory chatLogContextFactory, SystemDataService systemDataService)
        {

            _logContextFactory = chatLogContextFactory;
            _systemDataService = systemDataService;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClientHandler _httpClientHandler = new HttpClientHandler
            {
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (HttpRequestMessage arg1, X509Certificate2 certificate, X509Chain arg3, SslPolicyErrors sslPolicyErrors) => { return true; }
            };
            var _httpClient = new HttpClient(_httpClientHandler, false);

            _logger = Log.Logger;
            _telegramOptions = options.Value;
            try
            {
                _botClient = new TelegramBotClient(_telegramOptions.APIKey, _httpClient);
                var me = _botClient.GetMeAsync().Result;
                _logger.Information($"Connected to Bot: {me.FirstName}");


            }
            catch (Exception ex)
            {
                _logger.Error($"Error Connecting to Telegram Bot: {ex.Message}");
            }



        }


        public Task<Message> SendMessage(long ChatID, string Text, bool DisableNotification = false, int replytoid = 0)
        {
            return Task.Run(async () =>
            {
                return await _botClient.SendTextMessageAsync(ChatID, Text, disableNotification: DisableNotification, replyToMessageId: replytoid);

            });
        }

        public Task<Message> SendPhoto(long ChatID, string Text, string FilePath, string FileName = "", bool DisableNotification = false, int replytoid = 0)
        {
            return Task.Run(async () =>
            {
                if (System.IO.File.Exists(FilePath))
                {
                    _logger.Information($"Uploading File: {FilePath}");

                    using (FileStream fs = System.IO.File.OpenRead(FilePath))
                    {
                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, FileName);
                        _systemDataService.IncSendPhotos();
                        return await _botClient.SendPhotoAsync(ChatID, inputOnlineFile, Text);
                        

                    }
                }
                else
                {
                    _logger.Error($"File: {FilePath} does not exists!");
                    return null;
                }
            });

        }

        public Task SendPhoto(List<long> ChatIDs, string Text, string FilePath, string FileName = "", bool DisableNotification = false, int replytoid = 0)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (System.IO.File.Exists(FilePath))
                    {
                        _logger.Information($"Uploading File: {FilePath}");

                        using (FileStream fs = System.IO.File.OpenRead(FilePath))
                        {

                            InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, FileName);
                            _systemDataService.IncSendPhotos();
                            var Result = await _botClient.SendPhotoAsync(ChatIDs[0], inputOnlineFile, Text);

                            if (Result.Photo.First().FileId != null)
                            {
                                _logger.Information($"First sending was sucess forward File via FileID");
                            }
                            else
                            {
                                _logger.Error("Cannot recive FileID");

                            }

                            var UploadedFile = new InputOnlineFile(Result.Photo.First().FileId);
                            foreach (var ChatID in ChatIDs.GetRange(1, ChatIDs.Count - 1))
                            {
                                _systemDataService.IncSendPhotos();
                                await _botClient.SendPhotoAsync(ChatID, UploadedFile, Text);
                            }

                        }


                    }
                    else
                    {
                        _logger.Error($"File: {FilePath} does not exists!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Eror sending Photo: {ex.Message}");
                }


              

            });
        }

        public Task SendVideo(List<long> ChatIDs, string FilePath, string FileName = "", bool DisableNotification = false, int replytoid = 0)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (System.IO.File.Exists(FilePath))
                    {
                        _logger.Information($"Uploading File: {FilePath}");

                        using (FileStream fs = System.IO.File.OpenRead(FilePath))
                        {

                            InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, FileName);
                            _systemDataService.IncSendVideos();
                            var Result = await _botClient.SendVideoAsync(ChatIDs[0], inputOnlineFile);

                            if (Result.Video.FileId != null)
                            {
                                _logger.Information($"First sending was sucess forward File via FileID");
                            }
                            else
                            {
                                _logger.Error("Cannot recive FileID");

                            }

                            var UploadedFile = new InputOnlineFile(Result.Video.FileId);
                            foreach (var ChatID in ChatIDs.GetRange(1, ChatIDs.Count - 1))
                            {
                                _systemDataService.IncSendVideos();
                                await _botClient.SendPhotoAsync(ChatID, UploadedFile);
                            }

                        }


                    }
                    else
                    {
                        _logger.Error($"File: {FilePath} does not exists!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Eror sending Photo: {ex.ToString()}");
                }




            });
        }

        public Task<List<TelegramMessage>> GetChatHistoryAsync()
        {
            return Task.Run(() =>
            {
                using (var context = _logContextFactory.Create())
                {
                    return context.Messages.ToList();
                }
            });
        }

        public Task Subscribe(TelegramMessage telegramMessage)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var context = _logContextFactory.Create())
                    {
                        if (context.FileSystemSubscriptions.Where(o => o.Chat_ID == telegramMessage.Chat_id).FirstOrDefault() == null)
                        {
                            context.FileSystemSubscriptions.Add(new FileSystemSubscription() { User_FirstName = telegramMessage.User_FirstName, User_LastName = telegramMessage.User_LastName, Chat_ID = telegramMessage.Chat_id, User_UserName = telegramMessage.User_UserName });
                            context.SaveChanges();
                        }
                        else
                        {
                            _logger.Information($"User {telegramMessage.User_FirstName} already exists!, do nothing");
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while subscripe User: {ex.Message}");
                }
            });
        }

        public Task<List<FileSystemSubscription>> GetSubscription()
        {
            return Task.Run(() =>
            {
                using (var context = _logContextFactory.Create())
                {
                    return context.FileSystemSubscriptions.ToList();
                }
            });
        }

        public Task DelteSubscription(FileSystemSubscription fileSystemSubscription)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var context = _logContextFactory.Create())
                    {
                        context.Remove(fileSystemSubscription);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error delete User {fileSystemSubscription.Chat_ID.ToString()} Error: {ex.Message}");
                }
            });
        }
    }
}
