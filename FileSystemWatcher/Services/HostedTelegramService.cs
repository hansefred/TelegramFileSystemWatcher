using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemWatcher.Services
{
    public class HostedTelegramService : IHostedService
    {
        private readonly TelegramBotService _telegramBotService;


        public HostedTelegramService(TelegramBotService TelegramBotService)
        {
            _telegramBotService = TelegramBotService;
            _telegramBotService.MessageRecived += _telegramBotService_MessageRecived;
        }

        private void _telegramBotService_MessageRecived(object sender, Telegram.Bot.Types.Update e)
        {
            Log.Logger.Information($"New Message {e.Message.Text} from {e.Message.From.FirstName}");
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
