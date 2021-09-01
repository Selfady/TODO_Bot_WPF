using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using System.Configuration;


namespace TODO_Bot
{
    /// <summary>
    /// Heap of untestable code to automatically save files on your PC the bot receives.
    /// The bot allows you to download stored files.
    /// </summary>
    internal class TodoBot
    {
        /// a variable TelegramBotClient
        static TelegramBotClient _bot;

        static void Main(string[] args)
        {
            //token path
            var tokenPath = ConfigurationManager.AppSettings["tokenlocation"];

            //The path to download folder
            string downloadPath = @".\Download\";

                                  
            //Creating a directory to store files if it doesn't exist.
            if (!Directory.Exists(downloadPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(downloadPath);
            }
            
            //token to access the bot
            string token = File.ReadAllText(tokenPath);

            //Initialization of a new TelegramBotClient based on the token
            _bot = new TelegramBotClient(token);

            //On event OnMessage we call Message Listener method
            _bot.OnMessage += MessageListener;
            _bot.StartReceiving();
            Console.ReadKey();
        }

        /// <summary>
        /// Method that listens to the messages and proccesses them (answers)
        /// </summary>
        /// <param name="sender">The sender of the object</param>
        /// <param name="e">Message.Event.Args that contains message data</param>
        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            //Message text: time, Name, Id, Text
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            //Printing TypeMessage to the standard output
            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");

            //The path to download folder
            string downloadPath = @".\Download\";

            //Handler for Document message type
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                //Console.WriteLine(e.Message.Document.FileId);
                //Console.WriteLine(e.Message.Document.FileName);
                //Console.WriteLine(e.Message.Document.FileSize);

                //Download method call
                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName, downloadPath);
            }

            //A variable to store message text
            var messageText = e.Message.Text;

            //Logic to ignore messages without text
            if (e.Message.Text == null) return;

            #region ParseText

            //Handler for not null messages
            if (!String.IsNullOrEmpty(e.Message.Text))
            {
                //Handler for /start command
                if (e.Message.Text == "/start")
                {
                    _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        "Недобот забивает жесткий диск компьютера файлами, что втыкают в чатик и знает две команды:");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        "/list - должен вернуть список всех файлов, что уже накидали.");
                    _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        "/gimme n - n это имя файла в списке");
                }

                //Handler for /list command
                if (e.Message.Text == "/list")
                {
                    //Getting all files from the download directory
                    string[] fileEntries = Directory.GetFiles(downloadPath);

                    //Sending file names to the user
                    foreach (var fileName in fileEntries)
                    {
                        _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        $"{Path.GetFileName(fileName)}");
                    }
                    
                }

                //Handler for /gimme command
                if (e.Message.Text.Contains("/gimme") && e.Message.Text.Length > "/gimme".Length)
                {
                    //Meh code to get filenames without download path
                    var fileName = e.Message.Text.Substring("/gimme".Length+1);
                    var whatToGive = downloadPath + fileName;
                    //Sending file names
                    Upload(whatToGive, e.Message.Chat.Id);
                }
            }

            #endregion ParseText
        }

        /// <summary>
        /// Method to download files from Telegram messages
        /// </summary>
        /// <param name="fileId">The Id of a file</param>
        /// <param name="fileName">The name of a file</param>
        /// <param name="path">Path to download folder</param>
        static async void DownLoad(string fileId, string fileName, string path)
        {  

            //Initializing a variable to store file data
            var file = await _bot.GetFileAsync(fileId);

            //Initializing a new file-stream to create a file
            FileStream fs = new FileStream(path + fileName, FileMode.Create);

            //Async downloading of the file
            await _bot.DownloadFileAsync(file.FilePath, fs);

            //Closing the file-stream
            fs.Close();

            //Releasing resources of the file-stream
            fs.Dispose();
        }

        /// <summary>
        /// A Method to upload files from the PC to the bot and end-user
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="id">The id of the message</param>
        static async void Upload(string fileName, long id)
        {
            Console.WriteLine(fileName);
            //Initialization of using to open file-stream and release resources automatically
            using(FileStream fs = System.IO.File.OpenRead(fileName))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, fileName);
                await _bot.SendDocumentAsync(id, inputOnlineFile);
            }
        }
    }
}
