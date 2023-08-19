using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Ext
{
    public enum STDA_ERROR_TYPE : uint
    {
        WSA,
        _SOCKET,
        SOCKETSERVER,
        IOCP,
        THREAD,
        THREADPOOL,
        THREADPL_SERVER,
        THREADPL_CLIENT,
        _MYSQL,
        _MSSQL,
        _POSTGRESQL,
        MANAGER,
        EXEC_QUERY,
        PANGYA_DB,
        BUFFER,
        PACKET,
        PACKET_POOL,
        PACKET_FUNC,
        PACKET_FUNC_SV,
        PACKET_FUNC_LS,
        PACKET_FUNC_AS,
        PACKET_FUNC_RS,
        PACKET_FUNC_GG_AS,
        PACKET_FUNC_CLIENT,
        FUNC_ARR,
        SESSION,
        SESSION_POOL,
        JOB,
        JOB_POOL,
        UTIL_TIME,
        MESSAGE,
        MESSAGE_POOL,
        LIST_FIFO,
        LIST_FIFO_CONSOLE,
        CRYPT,
        COMPRESS,
        PANGYA_ST,
        PANGYA_GAME_ST,
        PANGYA_LOGIN_ST,
        PANGYA_MESSAGE_ST,
        PANGYA_RANK_ST,
        _IFF,
        CLIENTVERSION,
        SERVER,
        GAME_SERVER,
        LOGIN_SERVER,
        MESSAGE_SERVER,
        AUTH_SERVER,
        RANK_SERVER,
        GG_AUTH_SERVER,
        CLIENT,
        MULTI_CLIENT,
        CLIENTE,
        TIMER,
        TIMER_MANAGER,
        CHANNEL,
        LOBBY,
        ROOM,
        ROOM_GRAND_PRIX,
        ROOM_GRAND_ZODIAC_EVENT,
        ROOM_BOT_GM_EVENT,
        ROOM_MANAGER,
        LIST_ASYNC,
        _RESULT_SET,
        _RESPONSE,
        _ITEM,
        _ITEM_MANAGER,
        GM_INFO,
        PLAYER_INFO,
        PLAYER,
        PLAYER_MANAGER,
        SESSION_MANAGER,
        CLIENTE_MANAGER,
        READER_INI,
        MGR_ACHIEVEMENT,
        MGR_DAILY_QUEST,
        SYS_ACHIEVEMENT,
        NORMAL_DB,
        LOGIN_MANAGER,
        LOGIN_TASK,
        MAIL_BOX_MANAGER,
        PERSONAL_SHOP,
        LOTTERY,
        CARD_SYSTEM,
        COMET_REFILL_SYSTEM,
        PAPEL_SHOP_SYSTEM,
        BOX_SYSTEM,
        MEMORIAL_SYSTEM,
        PACKET_FUNC_MS,
        FRIEND_MANAGER,
        HOLE,
        GAME,
        TOURNEY_BASE,
        VERSUS_BASE,
        PRACTICE,
        CUBE_COIN_SYSTEM,
        COIN_CUBE_LOCATION_SYSTEM,
        TREASURE_HUNTER_SYSTEM,
    }

    public class AppException : Exception
    {
        private readonly string _messageError;
        private readonly uint _codeError;
        private readonly string _fullMessageError;

        public AppException(string messageError, uint codeError): base(messageError)
        {
            _messageError = messageError;
            _codeError = codeError;
            _fullMessageError = $"{_messageError}\t Error Code: {_codeError}";
        }
        public AppException(string messageError, uint codeError, Exception innerException)
            : base(messageError, innerException)
        {
            _codeError = codeError;
        }

        public string MessageError => _messageError;

        public uint CodeError => _codeError;

        public string FullMessageError => _fullMessageError;
    }
}
