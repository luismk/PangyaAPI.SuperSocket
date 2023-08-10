using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities;
using static System.Collections.Specialized.BitVector32;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class AppSessionManager : GenericDisposableCollection<IAppSession>
    {
        private static uint m_count = 0u;
        private static bool m_is_init = false;
        private object m_cs = new object(); // Use object for locking in C#
        private List<AppSession> m_session_del = new List<AppSession>();
        private int m_max_session;
        private int m_TTL;
        public AppSessionManager(int maxSession)
        {
            m_max_session = maxSession;
            m_TTL = 0;

            // Load config from server.ini
            ConfigInit();

            m_is_init = true;

            for (int i = 0; i < maxSession; i++)
            {
                Add(new AppSession());
            }
        }

        public void Clear()
        {
            lock (m_cs)
            {
                m_session_del.Clear();
                foreach (var session in Model)
                {
                    if (session != null)
                    {
                        session.Dispose();
                    }
                }
                Model.Clear();
            }
        }

        public AppSession AddSession(Socket sock, IAppServer server)
        {
            if (sock == null || server == null)
                throw new Exception("[session_manager::addSession][ERR_SESSION] _sock is invalid.");

           AppSession pSession = null;
            uint index = uint.MaxValue;

            lock (m_cs)
            {
                index = FindSessionFree();
                if (index == uint.MaxValue)
                {
                    throw new Exception("[session_manager::addSession][ERR_SESSION] already goal limit session estabilized.");
                }

                pSession = (AppSession)Model[(int)index];

                pSession.Initialize(server, sock);
                pSession.SetOID(index);
                pSession.SetTimeStartAndTick(Environment.TickCount);
                m_count++;
            }

            return pSession;
        }

        public bool DeleteSession(AppSession session)
        {
            bool ret = true;

            lock (m_cs)
            {
                int oid = (int)session.m_oid;
                if ((ret = session.Clear()))
                {
                    if (m_count != 0)
                    {
                        m_count--;
                    }
                }
            }

            return ret;
        }

        // Other methods...

        private void ConfigInit()
        {
            // Load config from INI file and set m_TTL
        }

        private uint FindSessionFree()
        {
            for (int i = 0; i < Count; ++i)
            {
                if (Model[i].SocketSession == null)
                {
                    return Convert.ToUInt32(i);
                }
            }
            return uint.MaxValue;
        }


        public uint NumSessionOnline
        {
            get
            {
                uint curr_online = 0;
                lock (m_cs)
                {
                    curr_online = m_count;
                }
                return curr_online;
            }
        }

        public AppSession FindSessionByOID(uint _oid)
        {
            AppSession _session = null;
            lock (m_cs)
            {
                _session = (AppSession)Model.FirstOrDefault(el =>
                {
                    return el.SocketSession != null &&
                           el.m_oid == _oid;
                });
            }
            return _session;
        }

        public AppSession FindSessionByUID(uint _uid)
        {
            AppSession _session = null;
            lock (m_cs)
            {
                _session = (AppSession)Model.FirstOrDefault(el =>
                {
                    return el.SocketSession != null &&
                           el.GetUID() == _uid;
                });
            }
            return _session;
        }

        public List<IAppSession> FindAllSessionByUID(uint _uid)
        {
            List<IAppSession> v_s = new List<IAppSession>();
            lock (m_cs)
            {
                v_s = Model.Where(el =>
                {
                    return el.SocketSession != null &&
                           el.GetUID() == _uid;
                }).ToList();
            }
            return v_s;
        }

        public AppSession FindSessionByNickname(string _nickname)
        {
            AppSession s = null;
            lock (m_cs)
            {
                s = (AppSession)Model.FirstOrDefault(el =>
                {
                    return el.SocketSession != null &&
                           string.Equals(el.GetNickname(), _nickname);
                });
            }
            return s;
        }
        public bool IsFull()
        {
            bool ret = false;
            lock (m_cs)
            {
                ret = Model.Count(session =>
                {
                    return session.SocketSession != null;
                }) == Model.Count;
            }
            return ret;
        }

    }
}
