using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemWatcher.Services
{
    public class HostedTelegramService : IHostedService
    {
        private readonly TelegramBotService _telegramBotService;
        private readonly SystemDataService _systemDataService;
        private readonly Stopwatch _stopwatch;

        public HostedTelegramService(TelegramBotService TelegramBotService, SystemDataService systemDataService)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _systemDataService = systemDataService;
            _telegramBotService = TelegramBotService;
            _telegramBotService.MessageRecived += _telegramBotService_MessageRecived;
        }

        private void _telegramBotService_MessageRecived(object sender, Telegram.Bot.Types.Update e)
        {
            Log.Logger.Information($"New Message {e.Message.Text} from {e.Message.From.FirstName}");

            
            if (_telegramBotService.GetSubscription().Result.Where(o => o.Chat_ID == e.Message.From.Id).FirstOrDefault() != null)
            {
                Log.Logger.Information($"User {e.Message.From.FirstName} {e.Message.From.LastName} ({e.Message.From.Username}) is known !");

                if (e.Message.Text.ToLower() == "status")
                {
                    Log.Logger.Information("Status was requested");
                    _telegramBotService.SendMessage(e.Message.From.Id, $"{_systemDataService.GetSystemData().ToString()} Uptime: {_stopwatch.Elapsed.ToString()}");
                }
                else
                {
                    Log.Logger.Information("Command not found sending help");
                    _telegramBotService.SendMessage(e.Message.From.Id, $"Command: status (Get System Infos)");
                }
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
    }


}
