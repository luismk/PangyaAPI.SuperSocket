using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class Logging
    {
        private string _file;
        private string _local;
        public enum e_LogType
        {
            MSG_NORMAL,
            MSG_STATUS,
            MSG_SQL,
            MSG_INFORMATION,
            MSG_NOTICE,
            MSG_WARNING,
            MSG_DEBUG,
            MSG_ERROR,
            MSG_FATALERROR,
            MSG_UNKNOWN_PACKET
        }
        public Logging()
        {
            _local = System.IO.Directory.GetCurrentDirectory() + "\\log";
            if (System.IO.Directory.Exists(_local) == false)
            {
                System.IO.Directory.CreateDirectory(_local);

            }
        }
        public void SetFileName(string filename)
        {
            _file = _local + $"\\{filename}";
        }
        public void DebugPacketUnknown(string message)
        {
            _vShowMessage(e_LogType.MSG_UNKNOWN_PACKET, message);
        }

        public void Information(string message)
        {
            _vShowMessage(e_LogType.MSG_INFORMATION, message);
        }

        public void Notice(string message)
        {
            _vShowMessage(e_LogType.MSG_NOTICE, message);
        }

        public void Warning(string message)
        {
            _vShowMessage(e_LogType.MSG_WARNING, message);
        }

        public void Debug(string message)
        {
            _vShowMessage(e_LogType.MSG_DEBUG, message);
        }
        public void Error(Exception ex, string message, int localError)
        {
            _vShowMessage(e_LogType.MSG_ERROR, ex, message, localError);
        }
        public void Error(Exception ex, string message)
        {
            _vShowMessage(e_LogType.MSG_ERROR, ex, message);
        }
        public void Error(Exception ex)
        {
            _vShowMessage(e_LogType.MSG_ERROR, ex.Message);
        }
        public void Error(string message)
        {
            _vShowMessage(e_LogType.MSG_ERROR, message);
        }

        public void Default(string message)
        {
            _vShowMessage(e_LogType.MSG_DEBUG, message);
        }
        public void FatalError(string message)
        {
            _vShowMessage(e_LogType.MSG_FATALERROR, message);
        }

        public void Write(string message)
        {
            _vShowMessage(e_LogType.MSG_NORMAL, message);
        }
        void _vShowMessage(e_LogType logType = e_LogType.MSG_NORMAL, string msg = "")
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(_file))
            {
                w.WriteLine("[{0}] [{1}] : {2}", DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss] "), logType.ToString(), msg);
            }
        }

        void _vShowMessage(e_LogType logType = e_LogType.MSG_NORMAL, Exception ex = null,string msg = "")
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(_file))
            {
                w.WriteLine("[{0}] [{1}] : {2}", DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss] "), logType.ToString(), msg, ex.Message);
            }
        }
        void _vShowMessage(e_LogType logType = e_LogType.MSG_NORMAL, Exception ex = null, string msg = "", int LocalError =0)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(_file))
            {
                w.WriteLine("[{0}] [{1}] : {2}/Line ({3})", DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss] "), logType.ToString(), msg, ex.Message, LocalError);
            }
        }
    }
}
