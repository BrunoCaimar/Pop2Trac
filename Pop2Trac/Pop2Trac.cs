using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using EricWillis.Pop2Ticket.Pop3;

namespace BrunoCaimar.Pop2Trac
{
    public class Pop2Trac
    {
        private Settings _config = null;
        private Pop3Client _pop3 = null;
        private bool _failedStartup = false;

        public Pop2Trac()
        {
            this._config = Settings.Load();

            if (this._failedStartup = this._config == null)
            { 
                this._config = new Settings();
                this._config.Interval = 60000;
                this._config.Pop3Password = "your pop3 password";
                this._config.Pop3Port = 110;
                this._config.Pop3Server = "your pop3 server";
                this._config.Pop3Username = "your pop3 username";
                this._config.Pop3UseSsl = false;
                this._config.PythonPath = "your python path";
                this._config.Email2tracPath = "your email2trac.py script path";

                this._config.Save();
            }

            this._pop3 = new Pop3Client(
                this._config.Pop3Username,
                this._config.Pop3Password,
                this._config.Pop3Server,
                this._config.Pop3Port,
                this._config.Pop3UseSsl);
        }

        public void Run()
        {
            this._pop3.Connect();

            foreach (Message message in this._pop3.GetMessages())
            {
                Console.Out.WriteLine(message.ID + "--" + message.From + "--");
                string pathMessage = string.Format("Message_{0}.eml", message.ID);
                try
                {
                    File.WriteAllText(pathMessage, message.FullTextMessage);
                    //// ToDo: Verify if the script run OK
                    this.ProcessMessage(pathMessage);
                    
                    if (this._config.Pop3DeleteMessageAfterRead)
                    {
                        this._pop3.DeleteMessage(message.ID);
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                    Console.Out.WriteLine(ex.StackTrace);
                }
            }

            this._pop3.Disconnect();
        }

        private void ProcessMessage(string pathMessage)
        {
            if (!File.Exists(pathMessage))
            {
                throw new ArgumentException(string.Format("Message file not found! FileName: {0}", pathMessage));
            }

            string batchName = "RunEmail2Trac.Bat";
            string batchContent = string.Format("{0} {1} <%1", this._config.PythonPath, this._config.Email2tracPath) +
                                  Environment.NewLine + "DEL %1" +
                                  Environment.NewLine + "REM pause";
            // If batch doesn't exist, generate it
            if (!File.Exists(batchName))
            {
                File.WriteAllText(batchName, batchContent);
            }
            this.Trace(string.Format("Processing message... {0}", pathMessage));
            Process.Start(batchName, pathMessage);
        }
        private void Trace(string msg) {
            Console.Out.WriteLine(msg);
        }
    }
}
