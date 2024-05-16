using System;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace INNconvert_tgbot
{
    class Program
    {
        static void Main(string[] args)
        {
            //чтение ключа из xml
            string api = "";
            var documetn = new XmlDocument();
            documetn.Load(@"C:\\Users\\zhenu\\source\\repos\\INNconvert_tgbot\\config.xml"); 
            XmlElement element = documetn.DocumentElement;
            foreach (XmlNode xnode in element)
            {
                if (xnode.Attributes.Count>0)
                {
                    XmlNode attr = xnode.Attributes.GetNamedItem("API");
                    if (attr != null)
                        api = attr.Value;
                }
            }

            //подключение телеграм бота
            var client = new TelegramBotClient(api);
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
            if (message != null)
            {
                if (message.Text != null)
                {
                    if (message.Text == "прив")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "хеллоу ворлд");
                    }
                }
            }
        }
    }
}