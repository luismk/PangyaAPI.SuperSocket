using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class Logger : List<Message>
    {
        DateTime date { get; set; }
        public Logger()
        {
            date = DateTime.Now;
        }

        private void LogOnly()
        {
            var _local = System.IO.Directory.GetCurrentDirectory() + "\\log";
            if (System.IO.Directory.Exists(_local) == false)
            {
                System.IO.Directory.CreateDirectory(_local);

            }
            var _file = System.IO.Directory.GetCurrentDirectory() + "\\log\\log " + date.ToString("ddMMyyyyHHmmss") + ".log";
            var m = getMessage();
            using (System.IO.StreamWriter w = System.IO.File.AppendText(_file))
            {
                w.WriteLine(m.get());
            }
        }

        private void LogAndConsole()
        {
            LogOnly();
            console_log();
        }

        void console_log()
        {
            Message m = getMessage();

            if (m != null)
            {

                Console.WriteLine(m.get());
            }
            else
                throw new Exception("Message is null. message_pool::console_log()");
        }
        public void push(string s, type_msg _tipo = 0)
        {
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            push(new Message(s, _tipo));
        }
        public void push(Message m)
        {
            this.Add(m);


            switch (m.getTipo())
            {
                case type_msg.CL_FILE_LOG_AND_CONSOLE:
                    LogAndConsole();
                    break;
                case type_msg.CL_ONLY_CONSOLE:
                    console_log();
                    break;
                case type_msg.CL_FILE_TIME_LOG_AND_CONSOLE:
                    break;
                case type_msg.CL_ONLY_FILE_LOG:
                    LogOnly();
                    break;
                case type_msg.CL_ONLY_FILE_TIME_LOG:
                    break;
                case type_msg.CL_ONLY_FILE_LOG_IO_DATA:
                    break;
                case type_msg.CL_FILE_LOG_IO_DATA_AND_CONSOLE:
                    break;
                case type_msg.CL_ONLY_FILE_LOG_TEST:
                    break;
                case type_msg.CL_FILE_LOG_TEST_AND_CONSOLE:
                    break;
                default:
                    break;
            }
            Clear();
        }
        Message getMessage() { return getFirstMessage(); }

        Message getFirstMessage() { return this[0]; }

        public bool checkUpdateDayLog()
        {

            bool ret = false;

            var ti_day = new DateTime();
            // Criar novos Logs que trocou o Dia do Log
            if (ti_day.Year < DateTime.Now.Year || ti_day.Month < DateTime.Now.Month || ti_day.Day < DateTime.Now.Day)
            {

                // Criou novos logs, trocou o dia do log
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                {
                    ret = true; date = DateTime.Now;
                }
            }


            return ret;
        }

        private readonly static string m_SessionInfoTemplate = "Session: {0}/{1}";

        /// <summary>
        /// Logs the error
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="title">The title.</param>
        /// <param name="e">The e.</param>
        public static void Error(ISessionBase session, string title, Exception e)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + title, e);
        }

        /// <summary>
        /// Logs the error
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Error(ISessionBase session, string message)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        /// <summary>
        /// Logs the information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Info(ISessionBase session, string message)
        {
            string info = string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message;
            logger.Info(info);
        }

        /// <summary>
        /// Logs the debug message
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public static void Debug(ISessionBase session, string message)
        {
            if (!logger.IsDebugEnabled)
                return;

            logger.Debug(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

    }
}
