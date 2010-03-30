using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace BrunoCaimar.Pop2Trac
{
    /// <summary>
    /// Pop2Trac settings read/saved as Xml
    /// </summary>
    [XmlRoot("Pop2TracSettings")]
    public class Settings
    {
        #region Readonly

        // seemed like a good enough name. you could change it... if you like ;)
        private static readonly String CONFIG_FILENAME = "Pop2Trac.config";

        #endregion

        #region Privates
        
        private Int32 _interval;

        private String _pop3Username;
        private String _pop3Password;
        private String _pop3Server;
        private Int32 _pop3Port;
        private Boolean _pop3UseSsl;
        private Boolean _pop3DeleteMessageAfterRead;

        private String _pythonPath;
        private String _email2tracPath;

        #endregion

        #region Static Properties

        /// <summary>
        /// Get config file based off of the config directory
        /// </summary>
        public static FileInfo ConfigFile
        {
            get
            {
                return new FileInfo(String.Format("{0}\\{1}",
                    Settings.ConfigDirectory.FullName, Settings.CONFIG_FILENAME));
            }
        }

        /// <summary>
        /// Get config directory based on where the service is executing
        /// </summary>
        public static DirectoryInfo ConfigDirectory
        {
            get
            {
                return (new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)).Directory;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Interval in milliseconds to check for new Pop3 messages
        /// </summary>
        public Int32 Interval
        {
            get { return this._interval; }
            set { this._interval = value; }
        }

        /// <summary>
        /// Pop3 username
        /// </summary>
        public String Pop3Username
        {
            get { return this._pop3Username; }
            set { this._pop3Username = value; }
        }

        /// <summary>
        /// Pop3 password
        /// </summary>
        public String Pop3Password
        {
            get { return this._pop3Password; }
            set { this._pop3Password = value; }
        }

        /// <summary>
        /// Pop3 server
        /// </summary>
        public String Pop3Server
        {
            get { return this._pop3Server; }
            set { this._pop3Server = value; }
        }

        /// <summary>
        /// Pop3 port
        /// </summary>
        public Int32 Pop3Port
        {
            get { return this._pop3Port; }
            set { this._pop3Port = value; }
        }

        /// <summary>
        /// Should use Ssl over Pop3
        /// </summary>
        public Boolean Pop3UseSsl
        {
            get { return this._pop3UseSsl; }
            set { this._pop3UseSsl = value; }
        }

        /// <summary>
        /// Should delete Pop3 messages after they're read
        /// </summary>
        public Boolean Pop3DeleteMessageAfterRead
        {
            get { return this._pop3DeleteMessageAfterRead; }
            set { this._pop3DeleteMessageAfterRead = value; }
        }

        /// <summary>
        /// Python Path 
        /// </summary>
        public String PythonPath
        {
            get { return this._pythonPath; }
            set { this._pythonPath = value; }
        }

        /// <summary>
        /// email2trac.py script Path
        /// </summary>
        public String Email2tracPath
        {
            get { return this._email2tracPath; }
            set { this._email2tracPath = value; }
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Save settings as Xml
        /// </summary>
        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter writer = null;

            using (writer = new StreamWriter(Settings.ConfigFile.FullName, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, this);
                writer.Flush();
                writer.Close();
            }
        }

        #endregion

        #region Public Static Methods
        
        /// <summary>
        /// Load a saved Xml config file
        /// </summary>
        /// <returns>Populated config information or null if config was malformed or missing</returns>
        public static Settings Load()
        {
            Settings settings = null;

            try
            {
                if (Settings.Exists())
                {
                    XmlSerializer serializier = new XmlSerializer(typeof(Settings));
                    TextReader reader = null;

                    using (reader = new StreamReader(Settings.ConfigFile.FullName, Encoding.UTF8))
                    {
                        settings = serializier.Deserialize(reader) as Settings;
                        reader.Close();
                    }
                }
            }

            catch (Exception)
            {
                // no config or malformed? then return null!
                settings = null;
            }

            return settings;
        }

        /// <summary>
        /// Checks if config file exists on disk
        /// </summary>
        /// <returns>Whether the config file exists or not (not whether it is valid)</returns>
        public static Boolean Exists()
        {
            return Settings.ConfigFile.Exists;
        }

        #endregion
    }
}
