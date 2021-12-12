using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyFirstNewTempSoloBot2
{
    class Program
    {
        private static string Token { get; } = "1409731192:AAGz7CaeRBMLDZ2bYEDBYGIRhMM4rjynZXY";

        private static bool BotRunStatus { get; set; } = false;
        private static string startUrl { get; } = $@"https://api.telegram.org/bot{Token}/";
        private static string getFiletUrl { get; } = $@"https://api.telegram.org/file/bot{Token}/";

        private static WebClient wc = new WebClient() { Encoding = Encoding.UTF8 };

        private static int MessageId = 0;
        private static int Date = 0;

        private static string userId = null;
        private static string userFirstrName = null;

        private static string userMessageText = null;

        private static string DocumentId = null;
        private static string DocumentName = null;

        private static string AudioId = null;
        private static string AudioName = null;

        private static string VideoId = null;
        private static string VideoName = null;


        private static string PictureId = null;
        private static string PhotoName = null;

        private static string userMessageType = "";


        static void Main(string[] args)
        {
            Thread task = new Thread(BackgrundBot);
            task.Start();
        }

        static void BackgrundBot()
        {
            try
            {
                int update_id = -1;
                int lastMessageId = 0;
                MessageId = 0;
                while (true)
                {
                    string url = $"{startUrl}getUpdates?offset={update_id}";
                    var r = wc.DownloadString(url);

                    JToken[] msgs = JObject.Parse(r)["result"].ToArray();
                    JToken msg = null;
                    JToken isBotMtessage = null;

                    if (msgs.Length > 0)
                    {
                        msg = msgs.Last();
                        isBotMtessage = msg.SelectToken("message.from.is_bot");
                        MessageId = (int)msg.SelectToken("message.message_id");
                        Date = (int)msg.SelectToken("message.date");
                    }
                    if(isBotMtessage != null)
                    if ((bool)isBotMtessage == false && lastMessageId != MessageId)
                    {
                        userMessageText = (string)msg.SelectToken("message.text");
                        userId = (string)msg.SelectToken("message.from.id");
                        userFirstrName = (string)msg.SelectToken("message.from.first_name");
                       
                        PictureId = (string)msg.SelectToken("message.photo[2].file_id");
                        DocumentId = (string)msg.SelectToken("message.document.file_id");
                        AudioId = (string)msg.SelectToken("message.voice.file_id");
                        VideoId = (string)msg.SelectToken("message.video_note.file_id");

                        DocumentName = (string)msg.SelectToken("message.document.file_name");


                        userMessageType = "";

                        if (userMessageText == "/start")
                        {
                            userMessageType = "command";
                            printMessage();
                            BotRunStatus = true;
                            string responseText = $"Hi, {userFirstrName}! I glad to see you! You can send to me any file and i will save it. Also in \"Menu\" You can see my commands: ";
                            sendMessage(userId, responseText);
                        }
                        else if (userMessageText == "/stop")
                        {
                            userMessageType = "command";
                            printMessage();

                            BotRunStatus = false;
                            string responseText = "Bye! I will here. If you will need my help just activate me! /start ";
                            sendMessage(userId, responseText);
                        }
                        else if (PictureId != null)
                        {
                            userMessageType = "picture";
                            printMessage();
                            PhotoName = $"photo_{Date}.jpg";
                            reciveFile(userId, PhotoName, PictureId);
                        }
                        else if (AudioId != null)
                        {
                            userMessageType = "voice";
                            printMessage();

                            AudioName = $"voice_{Date}.ogg";

                            reciveFile(userId, AudioName, AudioId);
                        }
                        else if (VideoId != null)
                        {
                            userMessageType = "video_note";
                            printMessage();

                            VideoName = $"video_note_{Date}.mp4";

                            reciveFile(userId, VideoName, VideoId);
                        }
                        else if (DocumentId != null)
                        {
                            userMessageType = "document";
                            printMessage();

                            reciveFile(userId, DocumentName, DocumentId);

                        }
                        else if (userMessageText == "/files_list")
                        {
                            userMessageType = "command";
                            printMessage();

                            sendFilesList(userId);
                        }
                        else if (userMessageText != null)
                        {
                            userMessageType = "string";
                            printMessage();

                            sendFile(userId, userMessageText);
                        }
                        else
                        {
                            userMessageType = "unknown";
                            printMessage();
                        }

                        lastMessageId = (int)MessageId;
                        update_id = -(int)MessageId;

                    }

                    Thread.Sleep(200);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            static void printMessage()
            {
                string text = $"User: {userFirstrName}, UserID: {userId}, MessageType: {userMessageType}, MessageText: {userMessageText}, MessageID: {MessageId}";
                Console.WriteLine(text);
            }

            static void sendMessage(string userId, string responseText) 
            {
                string url = $"{startUrl}sendMessage?chat_id={userId}&text={responseText}";
                wc.DownloadString(url);
            }

            static void reciveFile(string userId, string file_name, string file_id)
            {

                try
                {
                    string url = $"{startUrl}getFile?file_id={file_id}";
                    string r = wc.DownloadString(url);

                    JToken msg = JObject.Parse(r)["result"];
                    Uri myUri = null;

                    string Directory = checkFolderExist(userId);

                    if (msg != null)
                    {
                        string file_path = (string)msg.SelectToken("file_path");

                        url = $"{getFiletUrl}{file_path}";
                        myUri = new Uri(url, UriKind.Absolute);
                        Console.WriteLine("Downloading");
                        
                        string actual_file_path = $"{Directory}/{file_name}"; 

                        wc.DownloadFileAsync(myUri, actual_file_path);
                        while (wc.IsBusy)
                        {
                            Console.Write(".");
                            Thread.Sleep(50);
                        }

                        Console.WriteLine("\nDone");
                        sendMessage(userId, $"Saved: {file_name}");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"EXEPTION: {e.Message}");
                }
            }

            static string checkFolderExist(string userId)
            {
                string Directory = $@"Files{userId}";
                if (!System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.CreateDirectory(Directory);
                }
                return Directory;
            }

            static void sendFilesList(string userId)
            {

                string[] files = checkFilesList(userId);

                string msg = new string("");


                if (files != null && files.Length>0)
                {
                    for (var i = 1; i <= files.Length; i++)
                    {
                        msg += $"/download_{i} {files[i-1].Substring(files[i - 1].IndexOf('\\') + 1)}\n"; //}";
                    }
                }
                else
                {
                    msg = "Folder is Empty";
                }
                sendMessage(userId, msg);
            }

            static string[] checkFilesList(string userId)
            {
                string Directory = checkFolderExist(userId);

                string[] files = System.IO.Directory.GetFiles(Directory);

                return files;
            }

            static async void sendFile(string userId, string fileNumberinList)
            {
                try
                {
                    string[] files = checkFilesList(userId);

                    var i = Convert.ToInt32(fileNumberinList.Remove(0, 10));
                    string file = files[i-1];

                    if (File.Exists(file))
                    {
                        using (FileStream fs = File.OpenRead(file))
                        {
                            var client = new Telegram.Bot.TelegramBotClient(Token);

                            //   Telegram.InputOnlineFile Telegramfile = new Telegram.Bot.Types.FileToSend("log.txt", fs);

                            var sendtask = await client.SendDocumentAsync(
                                chatId: userId,
                                document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, file.Substring(file.IndexOf('\\') + 1)),
                                caption: file
                            ) ;

                        }
                    }
                }
                catch
                {
                    sendMessage(userId, $"Send file Failed!");
                }
            }

        }
    }
}
