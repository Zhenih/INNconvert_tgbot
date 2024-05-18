using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace INNconvert_tgbot
{
    class Program
    {
        //отвечает, ожидается ли в данный момент ввод ИНН пользователем
        static public bool inn_request = false; 
        //хранит последнее изменение для работы функции /last
        static public Update lastupdate;

        /// <summary>
        /// Запуск бота, вывод пользователю клавиатуры с основными функциями
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
        private static async Task Start(ITelegramBotClient botClient, Message message)
        {
            //Ввод ИНН не ожидается
            inn_request = false;
            //Клавиатура с перечислением основных функций бота
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
            //Отправка сообщения-инструкции пользователю
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Выберите действие",
                replyMarkup: replyKeyboard);
        }
        /// <summary>
        /// Один из основных методов, вывод навигации по боту пользователю
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
        private static async Task Help(ITelegramBotClient botClient, Message message)
        {
            //Ввод ИНН не ожидается
            inn_request = false;
            string helpMessage = "Список доступных команд:\n" +
                                 "/start – начать общение с ботом.\n" +
                                 "/help – вывести справку о доступных командах.\n" +
                                 "/hello – вывести имя и фамилию, email и ссылку на github автора бота.\n" +
                                 "/inn – получить наименования и адреса компаний по ИНН.\n" +
                                 "/last – повторить последнее действие бота.";

            await botClient.SendTextMessageAsync(message.Chat.Id, helpMessage);
        }
        /// <summary>
        /// Один из основных методов, вывод информации и разработчике бота
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
        private static async Task Hello(ITelegramBotClient botClient, Message message)
        {
            //Ввод ИНН не ожидается
            inn_request = false;
            string helloMessage = "Женюх Александр\n" +
                                  "azhenuh@mail.ru\n" +
                                  "https://github.com/Zhenuh";

            await botClient.SendTextMessageAsync(message.Chat.Id, helloMessage);
        }

        /// <summary>
        /// Вспомогательный метод, сообщает о том, что следующее сообщение будет содержать ИНН для обработки
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
        private static async Task Inns(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ИНН\n(если собираетесь ввести несколько, вводите через пробел)");
            //ожидается Ввод ИНН в следующем сообщении пользователя
            inn_request = true;
        }
        /// <summary>
        /// Вспомогательный метод, принимает на вход сообщение пользователя с ИНН,
        /// делит, при необходимости, его на отдельные части (В случае, если введено больше одного ИНН)
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
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
        /// <summary>
        /// Один из основных методов, реализация Get-запроса на https://api-fns.ru/api/search
        /// и последующий возврат обработанного ответа пользователю
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="message">Обрабатываемое соообщение</param>
        private static async Task FNS(ITelegramBotClient botClient, Message message)
        {
            //Ввод ИНН не ожидается после выхода из метода
            inn_request = false;
            // Создание HTTP-клиента
            using (HttpClient client = new HttpClient())
            {
                string answer = "";
                //Проверка корректности ввода, чтобы не отправлять заведомо некорректный Get-запрос
                if (long.TryParse(message.Text, out long num) && (message.Text.Length == 10))
                {
                    try
                    {
                        // URL API ФНС метода Search для получения основной информации об юр. лице по ИНН
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
        /// <summary>
        /// Вспомогательный метод, считывает API-ключ из XML файла 
        /// </summary>
        /// <param name="path"></param>
        /// <returns>API ключ в виде строки</returns>
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
        /// <summary>
        /// Обработка непредусмотренных в остальном коде ошибок при запросе к боту
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="exception">Информация об исключении</param>
        /// <param name="token">Токен для отмены операции асинхронного метода.</param>
        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Произошла ошибка: {exception.Message}");

            try
            {
                var message = exception.Message;
                // Формируем сообщение об ошибке
                string errorMessage = $"Произошла ошибка: {exception.Message}";
                // Отправляем сообщение пользователю об ошибке
                await botClient.SendTextMessageAsync(message, errorMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения об ошибке: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="botClient">Клиент Telegram Bot API для взаимодействия с ботом.</param>
        /// <param name="update">Информация об изменении</param>
        /// <param name="token">Токен для отмены операции асинхронного метода.</param>
        /// <returns></returns>
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
                                //обновление информации о последнем запросе к боту
                                //для корректной реализации обработки команды /last
                                lastupdate = update;
                                return;
                            }
                        case "/help":
                            {
                                Help(botClient, message);
                                //обновление информации о последнем запросе к боту
                                //для корректной реализации обработки команды /last
                                lastupdate = update;
                                return;
                            }
                        case "/hello":
                            {
                                Hello(botClient, message);
                                //обновление информации о последнем запросе к боту
                                //для корректной реализации обработки команды /last
                                lastupdate = update;
                                return;
                            }
                        case "/inn":
                            {
                                Inns(botClient, message);
                                //обновление информации о последнем запросе к боту
                                //для корректной реализации обработки команды /last
                                lastupdate = update;
                                return;
                            }
                        case "/last":
                            {
                                //реализация обработки команды /last
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
                                //кроме команд боту могут поступать 2 вида сообщений:
                                //1) ИНН, который обрабатывается только следом за командой /inn
                                //2) Некорректный ввод
                                if (inn_request)
                                {
                                    await Inn(botClient, message);
                                    //обновление информации о последнем запросе к боту
                                    //для корректной реализации обработки команды /last
                                    lastupdate = update;
                                    //После ввода ИНН с клавиатуры
                                    //возвращаем пользователю клавиатуру с основными функциями
                                    Start(botClient, message);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Некорректный ввод, воспользуйтесь /help:");
                                    //обновление информации о последнем запросе к боту
                                    //для корректной реализации обработки команды /last
                                    lastupdate = update;
                                    //После некорректного ввода с клавиатуры
                                    //возвращаем пользователю клавиатуру с основными функциями
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