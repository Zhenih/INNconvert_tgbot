using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace INNconvert_tgbot
{
    class Program
    {
        static public bool inn_request = false;

        static public Update lastupdate;

        private static async Task Start(ITelegramBotClient botClient, Message message)
        {
            inn_request = false;
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
            inn_request = false;
            //сообщение выводимое при вызове /help
            string help_message = "Список доступных команд:\n/start – начать общение с ботом.\n/help – вывести справку о доступных командах.\n/hello – вывести имя и фамилию, email и ссылку на github автора бота.\n/inn – получить наименования и адреса компаний по ИНН.\n/last – повторить последнее действие бота.";
            await botClient.SendTextMessageAsync(message.Chat.Id, help_message);
        }

        private static async Task Hello(ITelegramBotClient botClient, Message message)
        {
            inn_request = false;
            await botClient.SendTextMessageAsync(message.Chat.Id, "Женюх Александр\nazhenuh@mail.ru\nhttps://github.com/Zhenuh");
        }

        private static async Task Inns(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ИНН\n(если собираетесь ввести несколько, вводите через пробел)");
            inn_request = true;
        }

        private static async Task Inn(ITelegramBotClient botClient, Message message)
        {
            string[] subs = message.Text.Split(" ");

            foreach (var sub in subs)
            {
                inn_request = true;
                Message part_message = message;
                part_message.Text = sub;
                await FNS(botClient, part_message);
            }

        }
        private static async Task FNS(ITelegramBotClient botClient, Message message)
        {
            inn_request = false;
            // Создание HTTP-клиента
            using (HttpClient client = new HttpClient())
            {
                string answer = "";
                if (int.TryParse(message.Text, out int num) && (message.Text.Length == 10))
                {
                    try
                    {
                        // URL API ФНС
                        string url = "https://api-fns.ru/api/search";

                        // Добавление параметров запроса (ИНН и API-ключ)
                        string apiKey = GetKeyFromXML("C:\\Users\\zhenu\\source\\repos\\INNconvert_tgbot\\configFNS.xml");
                        string inn = message.Text;
                        url += $"?q={inn}&key={apiKey}";

                        // Отправка GET-запроса
                        HttpResponseMessage response = await client.GetAsync(url);

                        // Получение и обработка ответа
                        if (response.IsSuccessStatusCode)
                        {
                            
                            string responseBody = await response.Content.ReadAsStringAsync();
                            using JsonDocument doc = JsonDocument.Parse(responseBody);
                            JsonElement root = doc.RootElement;
                            JsonElement items = root.GetProperty("items");
                            JsonElement firstItem = items[0];
                            JsonElement entity = firstItem.GetProperty("ЮЛ");


                            answer = "Наименование: " +"\n"+ entity.GetProperty("НаимСокрЮЛ").GetString() + "\n" + "Адрес: " + "\n" + entity.GetProperty("АдресПолн").GetString();
                        }
                        else
                        {
                            answer = $"Ошибка подключения";
                        }
                        await botClient.SendTextMessageAsync(message.Chat.Id, answer);
                    }
                    catch (Exception ex)
                    {
                        answer = $"Ошибка при отправке запроса: не найдено организации с указанным ИНН";
                        await botClient.SendTextMessageAsync(message.Chat.Id, answer);
                    }
                }
                else
                {
                    answer = "Некорректный формат ИНН\nИНН юридического лица представляет собой последовательность из 10 арабских цифр";
                    await botClient.SendTextMessageAsync(message.Chat.Id, answer);
                }
            }

        }

        private static string GetKeyFromXML(string path)
        {
            string api = "";
            var documetn = new XmlDocument();
            documetn.Load(@path);
            XmlElement element = documetn.DocumentElement;
            foreach (XmlNode xnode in element)
            {
                if (xnode.Attributes.Count > 0)
                {
                    XmlNode attr = xnode.Attributes.GetNamedItem("API");
                    if (attr != null)
                        api = attr.Value;
                }
            }
            return api;
        }

        private static async Task Last(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "/last");
        }


        static void Main(string[] args)
        {
            //чтение ключа из xml
            string api = "";
            api = GetKeyFromXML("C:\\Users\\zhenu\\source\\repos\\INNconvert_tgbot\\config.xml");
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
                                lastupdate = update;
                                return;
                            }
                        case "/help":
                            {
                                Help(botClient, message);
                                lastupdate = update;
                                return;
                            }
                        case "/hello":
                            {
                                Hello(botClient, message);
                                lastupdate = update;
                                return;
                            }
                        case "/inn":
                            {
                                Inns(botClient, message);
                                lastupdate = update;
                                return;
                            }
                        case "/last":
                            {
                                if (lastupdate is not null)
                                {
                                    Update(botClient, lastupdate, token);
                                }
                                else
                                {
                                    botClient.SendTextMessageAsync(message.Chat.Id, "Предыдущая команда не распознана");
                                }
                                return;
                            }
                        default:
                            {
                                if (inn_request)
                                {
                                    await Inn(botClient, message);
                                    lastupdate = update;
                                    Start(botClient, message);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Неккоректный ввод, воспользуйтесь /help:");
                                    lastupdate = update;
                                    Start(botClient, message);
                                }
                                return;
                            }
                    
                    }

                }
            }
        }
    }
}