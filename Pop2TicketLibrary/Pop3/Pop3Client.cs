using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;

namespace EricWillis.Pop2Ticket.Pop3
{
    // I am by no means an expert on Pop3 interaction. I looked all of this
    // stuff up in specs and online. Double check documentation if you're using
    // any of this code for something mission critical!
    // http://www.ietf.org/rfc/rfc1939.txt

    /// <summary>
    /// A simple Pop3 client
    /// </summary>
    public class Pop3Client
    {
        #region Privates

        private String _server;
        private Int32 _port;
        private Boolean _useSsl;
        private String _username;
        private String _password;

        private TcpClient _connection;
        private Stream _stream;
        private StreamReader _reader;

        #endregion

        #region Properties

        /// <summary>
        /// Pop3 server
        /// </summary>
        public String Server
        {
            get { return this._server; }
            set { this._server = value; }
        }

        /// <summary>
        /// Pop3 server port (default is 110)
        /// </summary>
        public Int32 Port
        {
            get { return this._port; }
            set { this._port = value; }
        }

        /// <summary>
        /// Should use Ssl to connect to Pop3 server (default is false)
        /// </summary>
        public Boolean UseSsl
        {
            get { return this._useSsl; }
            set { this._useSsl = value; }
        }

        /// <summary>
        /// Pop3 username
        /// </summary>
        public String Username
        {
            get { return this._username; }
            set { this._username = value; }
        }

        /// <summary>
        /// Pop3 password
        /// </summary>
        public String Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        public Pop3Client()
            : this(String.Empty, String.Empty)
        { }

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        /// <param name="username">Pop3 username</param>
        /// <param name="password">Pop3 password</param>
        public Pop3Client(String username, String password)
            : this(username, password, String.Empty, 110)
        { }

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        /// <param name="server">Pop3 server</param>
        /// <param name="port">Pop3 server port</param>
        public Pop3Client(String server, Int32 port)
            : this(String.Empty, String.Empty, server, port)
        { }

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        /// <param name="username">Pop3 username</param>
        /// <param name="password">Pop3 password</param>
        /// <param name="server">Pop3 server</param>
        public Pop3Client(String username, String password, String server)
            : this(username, password, server, 110)
        { }

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        /// <param name="username">Pop3 username</param>
        /// <param name="password">Pop3 password</param>
        /// <param name="server">Pop3 server</param>
        /// <param name="port">Pop3 server port</param>
        public Pop3Client(String username, String password, String server, Int32 port)
            : this(username, password, server, port, false)
        { }

        /// <summary>
        /// Create a new Pop3 client
        /// </summary>
        /// <param name="username">Pop3 username</param>
        /// <param name="password">Pop3 password</param>
        /// <param name="server">Pop3 server</param>
        /// <param name="port">Pop3 server port</param>
        /// <param name="useSsl">Should use Ssl to connect to Pop3 server</param>
        public Pop3Client(String username, String password, String server, Int32 port, Boolean useSsl)
        {
            this._connection = null;
            this._username = username;
            this._password = password;
            this._server = server;
            this._port = port;
            this._useSsl = useSsl;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Create connection to Pop3 server
        /// </summary>
        public void Connect()
        {
            this.Connect(this._username, this._password);
        }

        /// <summary>
        /// Create connection to Pop3 server
        /// </summary>
        /// <param name="username">Pop3 username</param>
        /// <param name="password">Pop3 password</param>
        public void Connect(String username, String password)
        {
            this._username = username;
            this._password = password;
            this._connection = new TcpClient(this._server, this._port);

            if (this._useSsl)
            {
                this._stream = new SslStream(this._connection.GetStream(), false);
                this._stream.ReadTimeout = 60000;
                ((SslStream)this._stream).AuthenticateAsClient(this._server);
            }
            else
            {
                this._stream = this._connection.GetStream();
                this._stream.ReadTimeout = 60000;
            }

            this._reader = new StreamReader(this._stream, Encoding.ASCII);
            this._reader.ReadLine();

            this.Execute("USER " + username);
            this.Execute("PASS " + password);
        }

        /// <summary>
        /// Disconnect from Pop3 server
        /// </summary>
        public void Disconnect()
        {
            this.Execute("QUIT");
        }

        /// <summary>
        /// Get a list of unread messages on the Pop3 server
        /// </summary>
        /// <returns>Collection of unread messages</returns>
        public List<Message> GetMessages()
        {
            List<Message> messages = new List<Message>();
            List<Int32> messageIDs = new List<Int32>();
            String messageIDSignature = String.Empty;

            this.Execute("LIST");

            while (ReadResponseLines(out messageIDSignature))
                messageIDs.Add(Convert.ToInt32(messageIDSignature.Split(' ')[0]));

            foreach (Int32 messageID in messageIDs)
            {
                String messageLine = String.Empty;
                StringBuilder unparsedMessageText = new StringBuilder();

                this.Execute("RETR " + messageID.ToString());

                while (ReadResponseLines(out messageLine))
                    unparsedMessageText.AppendLine(messageLine);

                messages.Add(Message.Parse(messageID, unparsedMessageText.ToString()));
            }

            return messages;
        }

        /// <summary>
        /// Delete a message by message ID
        /// </summary>
        /// <param name="messageID">Message ID</param>
        public void DeleteMessage(Int32 messageID)
        {
            Console.WriteLine(this.Execute("DELE " + messageID.ToString()));
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Executes commands on the Pop3 server
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>String response from the Pop3 server</returns>
        private String Execute(String command)
        {
            Char[] commandChars = (command + "\r\n").ToCharArray();
            Byte[] commandBytes = Encoding.ASCII.GetBytes(commandChars); // from what I read, must be ASCII (not UTF-8)

            this._stream.Write(commandBytes, 0, commandBytes.Length);
            this._stream.Flush();

            return this._reader.ReadLine();
        }

        /// <summary>
        /// Reads string responses from Pop3 server; returns false if there is nothing to read
        /// </summary>
        /// <param name="line">Line to read</param>
        /// <returns>Whether or not there is more to read from the Pop3 server response</returns>
        private Boolean ReadResponseLines(out String line)
        {
            line = this._reader.ReadLine();

            if (line.Equals(".")) // if the line is a single period, that means it's the end of the server response
                return false;

            if (line.Length > 0 && line.StartsWith("."))
                line = line.Substring(1, line.Length - 1);

            return true;
        }

        #endregion
    }
}
