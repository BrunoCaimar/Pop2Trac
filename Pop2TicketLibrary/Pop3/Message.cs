using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EricWillis.Pop2Ticket.Pop3
{
    public class Message
    {
        #region Privates

        private Int32 _id;
        private String _from;
        private String _subject;
        private String _body;
        private String _fulltextMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Message ID
        /// </summary>
        public Int32 ID
        {
            get { return this._id; }
        }

        /// <summary>
        /// Sender's email address
        /// </summary>
        public String From
        {
            get { return this._from; }
        }

        /// <summary>
        /// Subject of email message
        /// </summary>
        public String Subject
        {
            get { return this._subject; }
        }

        /// <summary>
        /// Body of email message
        /// </summary>
        public String Body
        {
            get { return this._body; }
        }
        /// <summary>
        /// Full text of email message
        /// </summary>
        public String FullTextMessage
        {
            get { return this._fulltextMessage; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new email message
        /// </summary>
        public Message()
            : this(0, String.Empty, String.Empty, String.Empty, string.Empty)
        { }

        /// <summary>
        /// Creates a new email message
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param name="from">Sender's email address</param>
        /// <param name="subject">Subject of email message</param>
        /// <param name="body">Body of email message</param>
        /// <param name="fullText">Full text of email message</param>
        public Message(Int32 id, String from, String subject, String body, String fullText)
        {
            this._id = id;
            this._from = from;
            this._subject = subject;
            this._body = body;
            this._fulltextMessage = fullText;
        }

        #endregion

        #region Public Static

        /// <summary>
        /// Parse an email from the Pop3 client (relies heavily on SharpMimeTools)
        /// </summary>
        /// <param name="unparsedMessage">Unparsed message text</param>
        /// <returns>A populated email message</returns>
        public static Message Parse(String unparsedMessage)
        {
            return Message.Parse(0, unparsedMessage);
        }
        
        /// <summary>
        /// Parse an email from the Pop3 client (relies heavily on SharpMimeTools)
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param name="unparsedMessage">Unparsed message text</param>
        /// <returns>A populated email message with provided ID</returns>
        public static Message Parse(Int32 id, String unparsedMessage)
        {
            Message message = new Message();
            anmar.SharpMimeTools.SharpMessage sharpMessage = null; // http://anmar.eu.org/projects/sharpmimetools/

            // load unparsed message into a memory stream so that sharpmimetools can parse it
            // don't allow attachments or html (plain text only)
            using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(unparsedMessage)))
                sharpMessage = new anmar.SharpMimeTools.SharpMessage(stream, false, false);

            message._id = id; // set by developer for tracking and convenience
            // format From property based on whether a name + email are provided or just an email
            // i.e. "Eric Willis, eric.willis@gmail.com"
            //   or "eric.willis@gmail.com"
            message._from = String.IsNullOrEmpty(sharpMessage.From) ? sharpMessage.FromAddress :
                String.Format("{0}, {1}", sharpMessage.From, sharpMessage.FromAddress);
            message._subject = sharpMessage.Subject;
            message._body = sharpMessage.Body;
            message._fulltextMessage = unparsedMessage;

            return message;
        }

        #endregion
    }
}
