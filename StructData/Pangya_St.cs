using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.InteropServices;
namespace PangyaAPI.SuperSocket.StructData
{
    // que guarda a estrutura de bits da propriedade do server
    public class uProperty
    {
        public uProperty(uint _ul = 0u)
        {
        }
        public void clear()
        {
            ulProperty = 0u;
        }
        public uint ulProperty;
    }

    public class uPropertyEx : uProperty
    {
        public uPropertyEx(uint prop)
        {
            ulProperty = prop;
        }
        public _stBit stBit;
        public struct _stBit
        {
            public uint mantle; // = 0; // Só GM ou pessoas autorizadas pode ver esse server
            public uint only_rookie; // = 0; // Só Rookie(Iniciante) Pode entrar
            public uint natural; // = 0; // Natural modo
            public uint verde; // = 0; // Cor Verde
            public uint azul; // = 0; // Cor Azul
            public uint grand_prix; // = 0; // Grand Prix
        }
    }

    // que guarda a estrutura de bits do event flag do server
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public class uEventFlag
    {
        public short usEventFlag { get; set; }
    }



    // que guarda a estrutura de bits do event flag do server
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public class uEventFlagEx : uEventFlag
    {
        public uEventFlagEx()
        { stBit = new _stBit(); }
        public _stBit stBit;
        [StructLayout(LayoutKind.Sequential)]
        public class _stBit
        {
            public bool unknown; // Unknown
            public bool pang_x_plus; // Pang X2 e Maior que 2
            public bool exp_x2; // Exp X2
            public bool angel_wing; // Event Angel Wing(Diminui Quit a cada jogo)
            public bool exp_x_plus; // Exp X3 e Maior que 3
            public bool unknown2; // Unknown
            public bool club_mastery_x_plus; // Club Mastert X2 e Maior que 2
        }

        public void setState()
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(usEventFlag));
            bits = PadToFullByte(bits);

            stBit = new _stBit()
            {
                unknown = bits.Get(0),
                pang_x_plus = bits.Get(1),
                exp_x2 = bits.Get(2),
                angel_wing = bits.Get(3),
                exp_x_plus = bits.Get(4),
                unknown2 = bits.Get(5),
                club_mastery_x_plus = bits.Get(6)
            };
        }

        public uEventFlag setState(short value)
        {
            usEventFlag = value;
            BitArray bits = new BitArray(BitConverter.GetBytes(value));
            bits = PadToFullByte(bits);

            stBit = new _stBit()
            {
                unknown = bits.Get(0),
                pang_x_plus = bits.Get(1),
                exp_x2 = bits.Get(2),
                angel_wing = bits.Get(3),
                exp_x_plus = bits.Get(4),
                unknown2 = bits.Get(5),
                club_mastery_x_plus = bits.Get(6)
            };
            return this;
        }
        BitArray PadToFullByte(BitArray bits)
        {
            BitArray array = new BitArray(8, false);
            if (bits.Count > 0)
            {
                for (int i = 0; i < bits.Count; i++)
                {
                    if ((bits.Count > 8) && (i < 8))
                    {
                        array.Set(i, bits[i]);
                    }
                }
            }
            return array;
        }

        byte ConvertToByte(BitArray bits)
        {
            byte[] array = new byte[1];
            bits.CopyTo(array, 0);
            return array[0];
        }
    }
    public class uFlag
    {
        public uFlag(ulong _ull = 0u)

        { ullFlag = _ull; stBit = new _stBit(); }
        public ulong ullFlag;
        public class _stBit
        {
            public ulong all_game { get; set; } = 0; // Não pode jogar nada
            public ulong buy_and_gift_shop { get; set; } = 0; // Não pode comprar no shop
            public ulong gift_shop { get; set; } = 0; // Não pode enviar presente
            public ulong papel_shop { get; set; } = 0; // Não pode jogar no Papel Shop
            public ulong personal_shop { get; set; } = 0; // Não pode vender no personal shop
            public ulong stroke { get; set; } = 0; // Não pode jogar Stroke
            public ulong match { get; set; } = 0; // Não pode jogar Match
            public ulong tourney { get; set; } = 0; // Não pode jogar Tourney
            public ulong team_tourney { get; set; } = 0; // Não pode jogar Team Tourney(Agora é Short Game)
            public ulong guild_battle { get; set; } = 0; // Não pode jogar Guild Battle
            public ulong pang_battle { get; set; } = 0; // Não pode jogar Pang Battle
            public ulong approach { get; set; } = 0; // Não pode jogar Approach
            public ulong lounge { get; set; } = 0; // Não pode criar sala lounge e entrar sala lounge
            public ulong scratchy { get; set; } = 0; // Não pode jogar no Scratchy System
            public ulong rank_server { get; set; } = 0; // Não pode abrir o rank server
            public ulong ticker { get; set; } = 0; // Não pode mandar ticker
            public ulong mail_box { get; set; } = 0; // Desabilita Mail Box
            public ulong grand_zodiac { get; set; } = 0; // Acho que é o grand zodiac, se não for vai ser
            public ulong single_play { get; set; } = 0; // Acho que é o Single Play, se não for vai ser
            public ulong grand_prix { get; set; } = 0; // Acho que é o Grand Prix, se não for vai ser
            public ulong guild { get; set; } = 0; // Desabilita Guild
            public ulong ssc { get; set; } = 0; // Não pode jogar Special Shuffle Course
            public ulong memorial_shop { get; set; } = 0; // Não pode jogar no Memorial Shop
            public ulong short_game { get; set; } = 0; // Não pode jogar Short Game
            public ulong char_mastery { get; set; } = 0; // Não pode mexer no Character Mastery System
            public ulong lolo_copound_card { get; set; } = 0; // Não pode jogar no Lolo Copound Card System
            public ulong cadie_recycle { get; set; } = 0; // Não pode usar o Caddie Recycle Item System
            public ulong legacy_tiki_shop { get; set; } = 0; // Não pode usar o Legacy Tiki Shop System }
        }
        public _stBit stBit { get; set; }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 92)]
    public class ServerInfo
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Name { get; set; }
        public int UID { get; set; }
        public int MaxUser { get; set; }
        public int Curr_User { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string IP { get; set; }
        public int Port { get; set; }
        public uProperty Property { get; set; }
        public int AngelicWingsNum { get; set; }
        public uEventFlag EventFlag;
        public short EventMap { get; set; }
        public short AppRate { get; set; }
        public short Unknown { get; set; }
        public short ImgNo { get; set; }
        public void Init()
        {
            UID = int.MaxValue;
            IP = "";
            Name = "";
        }
        public ServerInfo()
        {
            Init();
            Property = new uProperty();
            EventFlag = new uEventFlag();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ServerInfoEx : ServerInfo
    {
        public byte Tipo { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Version { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Version_Client { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]//desnecessario, mas e so para realizar o calculo
        public string Auth_IP { get; set; } = "";
        public uint Auth_Port { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public RateConfigInfo Rate { get; set; }
        public uFlag flag;
        public new uEventFlagEx EventFlag { get; set; }
        public new uPropertyEx Property { get; set; }
        public void SetInit()
        {
            Init();
            Version = "";
            Version_Client = "";
            Auth_IP = "";
            flag = new uFlag();
            Rate = new RateConfigInfo();
        }
        public byte[] GetInfoBytes()
        {
            int size = Marshal.SizeOf(new ServerInfo());//realiza um calculo sobre a classe
            byte[] arr = new byte[size];//cria o tamanho calculado em bytes

            IntPtr ptr = Marshal.AllocHGlobal(size);//aloca o tamanho nao gerenciavel 
            Marshal.StructureToPtr(this, ptr, true); //cria a estrutura 
            Marshal.Copy(ptr, arr, 0, size);//copia a estrutura e passa em bytes
            Marshal.FreeHGlobal(ptr); // libera o alocamento
            return arr;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RateConfigInfo
    {
        public short scratchy { get; set; }
        public short papel_shop_rare_item { get; set; }
        public short papel_shop_cookie_item { get; set; }
        public short treasure { get; set; }
        public short pang { get; set; }
        public short exp { get; set; }
        public short club_mastery { get; set; }
        public short chuva { get; set; }
        public short memorial_shop { get; set; }
        public short grand_zodiac_event_time { get; set; }          // Verifica se o evento do grand zodiac está ativado no server
        public short angel_event { get; set; }                      // Verifica se o Angel Event Quit Reduce está ativo no server
        public short grand_prix_event { get; set; }             // Verifica se o Grand Prix evento está ativado no server
        public short golden_time_event { get; set; }                // Verifica se o Golden Time está ativado no server
        public short login_reward_event { get; set; }               // Verifica se o Login Reward está ativado no server
        public short bot_gm_event { get; set; }                 // Verifica se o Bot GM Event está ativado no server
        public short smart_calculator { get; set; }				// Verifica se o Smart Calculator está ativado no server
        public int countBitGrandPrixEvent()
        {

            int count = 0;
            // 16 Bit public short
            for (var i = 0; i < 16u; ++i)
            {
                var check = (grand_prix_event >> i);
                if ((check & 1) == 1)
                    count++;
            }
            return count;
        }

        public List<int> getValueBitGrandPrixEvent()
        {

            List<int> v_value = new List<int>();

            // 16 Bit unisgned short
            for (var i = 0; i < 16; ++i)
            {
                var check = (grand_prix_event >> i);
                if ((check & 1) == 1)
                    v_value.Add(i + 1);
            }
            return v_value;
        }

        public bool checkBitGrandPrixEvent(int _type)
        {

            if (_type == 0)
                return false;

            return ((grand_prix_event >> (_type - 1)) & 1) == 1;
        }
    }

    public partial class TableMac
    {
        public string Mac_Adress { get; set; }
        public DateTime Date { get; set; }

        public TableMac(string adress, DateTime insert_time)
        {
            Mac_Adress = adress;
            Date = insert_time;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class chat_macro_user
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string[] macro { get; set; }
        public chat_macro_user Init()
        {
            macro = new string[9];
            for (int i = 0; i < 9; i++)
            {
                macro[i] = "Pangya!";
            }

            return this;
        }
    }


    // Auth Server Key Struture
    public class AuthServerKey
    {
        public AuthServerKey()
        {
        }
        public bool isValid()
        {
            return (valid == 1 && key[0] != '\0');
        }
        public bool checkKey(string _str)
        {
            return (isValid() && string.Compare(_str, key) == 0);
        }
        public int server_uid;
        public string key;               // 16 + null termineted string
        public byte valid = 1;
    }


    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class KeysOfLogin
    {
        public KeysOfLogin()
        {
            keys = new string[2];
        }
        public byte valid;
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string[] keys { get; set; } = new string[2];
    }

    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AuthKeyInfo
    {
        public byte valid;
        public string key { get; set; }
    }


    // Auth Key Login Info
    // Keys Of Login
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AuthKeyLoginInfo : AuthKeyInfo
    {
    }
    // Auth Key Game Info
    public class AuthKeyGameInfo : AuthKeyInfo
    {
        public int server_uid;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial class CharacterInfo
    {
        public enum Stats : byte
        {
            S_POWER,
            S_CONTROL,
            S_ACCURACY,
            S_SPIN,
            S_CURVE,
        }
        public uint _typeid { get; set; }
        public uint id { get; set; }
        public byte default_hair { get; set; }
        public byte default_shirts { get; set; }
        public byte gift_flag { get; set; }
        public byte Purchase { get; set; }
        /// <summary>
        /// Parts typeid, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_typeid { get; set; }
        /// <summary>
        /// Parts id, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_id { get; set; }
        /// <summary>
        ///Não sei bem direito o que é aqui
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
        public byte[] Blank { get; set; }
        /// <summary>
        ///Auxiliar Parts 5, aqui fica anel
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] AuxPart { get; set; }
        /// <summary>
        ///Cut-in, no primeiro mas acho que pode ser cut-in no resto
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Cut_in { get; set; }
        /// <summary>
        ///Aqui é o character stats, como controle, força, spin e etc
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] PCL { get; set; }
        /// <summary>
        /// Mastery, que aumenta os slot do stats do character
        /// </summary>
        public uint MasteryPoint { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Character { get; set; }				// 4 Slot de card Character
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Caddie { get; set; }             // 4 Slot de card Caddie
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_NPC { get; set; }

        public void Init()
        {
            if (Card_NPC == null)
                Card_NPC = new uint[4];
            if (Card_Character == null)
                Card_Character = new uint[4];
            if (Card_Caddie == null)
                Card_Caddie = new uint[4];
            if (parts_id == null)
                parts_id = new uint[24];
            if (parts_typeid == null)
                parts_typeid = new uint[24];
            if (AuxPart == null)
                AuxPart = new uint[5];
            if (Blank == null)
                Blank = new byte[216];
            if (Cut_in == null)
                Cut_in = new uint[5];
            if (PCL == null)
                PCL = new byte[5];
        }

        public void initComboDef()
        {
            Init();
        }
    }

    #region User Info


    public class BlockFlag
    {
        public BlockFlag()
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(0);
            }

            m_id_state = new IDStateBlockFlag(0);
        }
        public void setIDState(UInt64 _id_state)
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(_id_state);
            }

            m_id_state = new IDStateBlockFlag(_id_state);

            // Block Recursos do player
            if ((m_id_state.id_state.st_IDState.L_BLOCK_LOUNGE/* & 4*/)) // Block Lounge
                m_flag.ullFlag = m_flag.stBit.lounge = 1u; // Block Lounge
            if ((m_id_state.id_state.st_IDState.L_BLOCK_SHOP_LOUNGE/* & 8*/)) // Block Shop Lounge
                m_flag.stBit.personal_shop = 1u; // Block Shop Lounge
            if ((m_id_state.id_state.st_IDState.L_BLOCK_GIFT_SHOP/* & 16*/)) // Block Gift Shop
                m_flag.stBit.gift_shop = 1u; // Block Gift Shop
            if ((m_id_state.id_state.st_IDState.L_BLOCK_PAPEL_SHOP/* & 32*/)) // Block Papel Shop
                m_flag.stBit.papel_shop = 1u; // Block Papel Shop
            if ((m_id_state.id_state.st_IDState.L_BLOCK_SCRATCHY/* & 64*/)) // Block Scratchy
                m_flag.stBit.scratchy = 1u; // Block Scratchy
            if ((m_id_state.id_state.st_IDState.L_BLOCK_TICKER/* & 128*/)) // Block Ticker
                m_flag.stBit.ticker = 1u; // Block Ticker
            if ((m_id_state.id_state.st_IDState.L_BLOCK_MEMORIAL_SHOP/* & 256*/)) // Block Memorial Shop
                m_flag.stBit.memorial_shop = 1u; // Block Memorial Shop
        }

        public IDStateBlockFlag m_id_state;
        public uFlag m_flag;
    }

    // ------------------ Player Account Basic ---------------- //
    // Struct ID State Block Flag
    public class IDStateBlockFlag
    {
        public IDStateBlockFlag(ulong _id_state, int _block_time = -1)
        {
            id_state = new _uIDState(_id_state);
            block_time = _block_time;
        }
        public IDStateBlockFlag()
        {
            id_state = new _uIDState();
            block_time = -1;
        }

        public class _uIDState
        {
            public _uIDState(ulong _ull = 0u)
            { ull_IDState = _ull; setState(); }
            public _uIDState()
            { ull_IDState = 0; setState(); }
            public void setState()
            {
                BitArray bits = new BitArray(BitConverter.GetBytes(ull_IDState));
                bits = PadToFullByte(bits);

                st_IDState = new _stIDState()
                {
                    L_BLOCK_TEMPORARY = bits.Get(0),
                    L_BLOCK_FOREVER = bits.Get(1),
                    L_BLOCK_LOUNGE = bits.Get(2),
                    L_BLOCK_SHOP_LOUNGE = bits.Get(3),
                    L_BLOCK_GIFT_SHOP = bits.Get(4),
                    L_BLOCK_PAPEL_SHOP = bits.Get(5),
                    L_BLOCK_SCRATCHY = bits.Get(6),
                    L_BLOCK_TICKER = bits.Get(7),
                    L_BLOCK_MEMORIAL_SHOP = ull_IDState == 256,
                    L_BLOCK_ALL_IP = false,
                    L_BLOCK_MAC_ADDRESS = false
                };
            }

            BitArray PadToFullByte(BitArray bits)
            {
                BitArray array = new BitArray(8, false);
                if (bits.Count > 0)
                {
                    for (int i = 0; i < bits.Count; i++)
                    {
                        if ((bits.Count > 8) && (i < 8))
                        {
                            array.Set(i, bits[i]);
                        }
                    }
                }
                return array;
            }

            byte ConvertToByte(BitArray bits)
            {
                byte[] array = new byte[1];
                bits.CopyTo(array, 0);
                return array[0];
            }
            public class _stIDState
            {
                public bool L_BLOCK_TEMPORARY { get; set; } = true;
                public bool L_BLOCK_FOREVER { get; set; } = true;
                public bool L_BLOCK_LOUNGE { get; set; } = true;
                public bool L_BLOCK_SHOP_LOUNGE { get; set; } = true;
                public bool L_BLOCK_GIFT_SHOP { get; set; } = true;
                public bool L_BLOCK_PAPEL_SHOP { get; set; } = true;
                public bool L_BLOCK_SCRATCHY { get; set; } = true;
                public bool L_BLOCK_TICKER { get; set; } = true;
                public bool L_BLOCK_MEMORIAL_SHOP { get; set; } = true;
                public bool L_BLOCK_ALL_IP { get; set; } = true;           // Bloquea todo IP que o player logar
                public bool L_BLOCK_MAC_ADDRESS { get; set; } = true;      // Bloquea o MAC Address
            }
            public ulong ull_IDState;
            public _stIDState st_IDState;
        }
        public _uIDState id_state;
        public int block_time;
    }

    #endregion
}
