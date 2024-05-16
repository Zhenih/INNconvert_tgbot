using System;
using System.Collections.Generic;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace INNconvert_tgbot
{
    class Program
    {
        private static async Task Start(ITelegramBotClient botClient, Message message)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("/help"),
                                            new KeyboardButton("/hello"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("/inn"),
                                            new KeyboardButton("/last"),
                                        }
                })
            {
                ResizeKeyboard = true,
            };

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Выберите действие",
                replyMarkup: replyKeyboard);
        }

        private static async Task Help(ITelegramBotClient botClient, Message message)
        {
            //сообщение выводимое при вызове /help
            string help_message = "Спмсок доступных команд:\n/start – начать общение с ботом.\n/help – вывести справку о доступных командах.\n/hello – вывести имя и фамилию, email и ссылку на github автора бота.\n/inn – получить наименования и адреса компаний по ИНН.\n/last – повторить последнее действие бота.";
            await botClient.SendTextMessageAsync(message.Chat.Id, help_message);
        }

        private static async Task Hello(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "/hello");
        }

        private static async Task Inn(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "/Inn");
        }

        private static async Task Last(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "/last");
        }


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
                    switch (message.Text)
                    {
                        case "/start":
                            {
                                Start(botClient, message);
                                return;
                            }
                        case "/help":
                            {
                                Help(botClient, message);
                                return;
                            }
                        case "/hello":
                            {
                                
                                return;
                            }
                        case "/inn":
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "/inn");
                                return;
                            }
                        case "/last":
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "/last");
                                return;
                            }
                    }
                }
            }
        }
    }
}