using System;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace INNconvert_tgbot
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TelegramBotClient("6867781306:AAGctctGa9rdso2hA0YVZQ-wLjt0AsE0esQ");
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
                if (message.Text != null)
                {
                    if (message.Text == "start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "прив");
                    }    
                }
        }
    }
}