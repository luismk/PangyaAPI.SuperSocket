using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities;
using ServerConsole.Session;
using System;
using System.Diagnostics;
using System.IO;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Player.Data;

namespace ServerConsole.Server
{
    /// <summary>
    /// Vai ser a class usada no Program.cs
    /// </summary>
    public class MainServer : PangyaServer<Player>

    {
        public MainServer()
        {
        }

        public override void onHeartBeat()
        {
            try
            {
                // Server ainda não está totalmente iniciado
                if (this.State != ServerState.NotInitialized)
                    return;

                // Tirei o list IP/MAC block daqui e coloquei no monitor no server, por que agora eles são da classe server
            }
            catch (exception e)
            {
                _smp.Message_Pool.push("[login_server::onHeartBeat][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
        }

        protected override void onAcceptCompleted(Player _session)
        {
            try
            {
                var packet = new Packet(0x0B00);
                packet.AddInt32(05);
                packet.AddInt32(10101);
               packet.MakeRaw();
                var mb = packet.GetMakedBuf();
                _session.Send(mb.Buffer,0, (int)mb.Length);
            }
            catch (Exception ex)
            {
               
            }

        }

        public void requestLogin(Packet p, Player _session)
        {
            /// Pacote01 Option 0x0F(15) é manutenção

            try
            {


                // Ler dados do packet de login
                var result = new LoginData();
               p.ReadObject(result);

                //  Verify Id is valid
                if (result.id.size() < 2 || System.Text.RegularExpressions.Regex.Match(result.id, (".*[\\^$&,\\?`´~\\|\"@#¨'%*!\\\\].*")).Success)
                    throw new exception("[login_server::requestLogin][Error] ID(" + result.id
                            + ") invalid, less then 2 characters or invalid character include in id.", STDA_ERROR_TYPE.LOGIN_SERVER);

                // Password to MD5
                var pass_md5 = result.password;

                try
                {
                    pass_md5 = result.password.MD5Hash();
                }
                catch (exception e)
                {

                    _smp.Message_Pool.push("[login_server::requestLogin][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                    // Relança
                    throw;
                }
                //if (!haveBanList(_session.m_ip, result.mac_address, !result.mac_address.empty()))
                //{   // Verifica se está na list de ips banidos

                //    var cmd_verifyId = new CmdVerifyID(result.id); // ID

                //    cmd_verifyId.waitEvent();

                //    if (cmd_verifyId.getException().getCodeError() != 0)
                //        throw cmd_verifyId.getException();

                //    if (cmd_verifyId.getUID() > 0)
                //    {   // Verifica se o ID existe

                //        var cmd_verifyPass = new CmdVerifyPass(cmd_verifyId.getUID(), pass_md5); // PASSWORD

                //        cmd_verifyPass.waitEvent();

                //        if (cmd_verifyPass.getException().getCodeError() != 0)
                //            throw cmd_verifyPass.getException();

                //        if (cmd_verifyPass.getLastVerify())
                //        {   // Verifica se a senha bate com a do banco de dados

                //            var cmd_pi = new CmdPlayerInfo(cmd_verifyId.getUID());


                //            cmd_pi.waitEvent();

                //            if (cmd_pi.getException().getCodeError() != 0)
                //                throw cmd_pi.getException();

                //            _session.m_pi = cmd_pi.getInfo();
                //            var pi = _session.m_pi;

                //            var cmd_lc = new CmdLogonCheck(pi.uid);
                //            var cmd_flc = new CmdFirstLoginCheck(pi.uid);
                //            var cmd_fsc = new CmdFirstSetCheck(pi.uid);

                //            cmd_lc.waitEvent();

                //            if (cmd_lc.getException().getCodeError() != 0)
                //                throw cmd_lc.getException();

                //            cmd_flc.waitEvent();

                //            if (cmd_flc.getException().getCodeError() != 0)
                //                throw cmd_flc.getException();

                //            cmd_fsc.waitEvent();

                //            if (cmd_fsc.getException().getCodeError() != 0)
                //                throw cmd_fsc.getException();

                //            // Verifica se tem o mesmo player logado com outro socket
                //            Session player_logado = HasLoggedWithOuterSocket(_session);

                //            if (!canSameIDLogin() && player_logado != null)
                //            {   // Verifica se ja nao esta logado

                //                p.init_plain((short)0x01);

                //                p.addByte(0xE2);
                //                p.addInt32(5100107);

                //                packet_func_ls.session_send(p, _session, 0);

                //                _smp.Message_Pool.push("[login_server::requestLogin][Log] player[UID="
                //                        + (pi.uid) + ", ID=" + (pi.id) + ", IP=" + _session.getIP() + "] ja tem outro Player conectado[UID=" + (player_logado.getUID())
                //                        + ", OID=" + (player_logado.m_oid) + ", IP=" + player_logado.getIP() + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //            }
                //            else if (pi.m_state == 1)
                //            {   // Verifica se já pediu para logar

                //                p.init_plain((short)0x01);

                //                p.addUint8(0xE2);
                //                p.addInt32(500010); // Já esta logado, ja enviei o pacote de logar

                //                packet_func_ls.session_send(p, _session, 0);

                //                if (pi.m_state++ >= 3)  // Ataque, derruba a conexão maliciosa
                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] Player ja esta logado, o pacote de logar ja foi enviado, player[UID="
                //                            + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //            }
                //            else
                //            {

                //                var cmd_vi = new CmdVerifyIP(pi.uid, _session.m_ip);


                //                cmd_vi.waitEvent();

                //                if (cmd_vi.getException().getCodeError() != 0)
                //                    throw cmd_vi.getException();

                //                if (!Convert.ToBoolean(pi.m_cap & 4) && getAccessFlag() && !cmd_vi.getLastVerify())
                //                {   // Verifica se tem permição para acessar

                //                    p.init_plain((short)0x01);

                //                    p.addUint8(0xE2);
                //                    p.addInt32(500015); // Acesso restrito

                //                    packet_func_ls.session_send(p, _session, 0);

                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] acesso restrito para o player [UID=" + (pi.uid)
                //                            + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                }
                //                else if (pi.block_flag.m_id_state.id_state.ull_IDState != 0)
                //                {   // Verifica se está bloqueado

                //                    if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_TEMPORARY && (pi.block_flag.m_id_state.block_time == -1 || pi.block_flag.m_id_state.block_time > 0))
                //                    {

                //                        var tempo = pi.block_flag.m_id_state.block_time / 60 / 60/*Hora*/; // Hora

                //                        p.init_plain((short)0x01);

                //                        p.addUint8(7);
                //                        p.addInt32(pi.block_flag.m_id_state.block_time == -1 || tempo == 0 ? 1/*Menos de uma hora*/ : tempo);   // Block Por Tempo

                //                        // Aqui pode ter uma  com mensagem que o pangya exibe
                //                        //p.addString("ola");

                //                        packet_func_ls.session_send(p, _session, 0);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Bloqueado por tempo[Time="
                //                                + (pi.block_flag.m_id_state.block_time == -1 ? ("indeterminado") : ((pi.block_flag.m_id_state.block_time / 60)
                //                                + "min " + (pi.block_flag.m_id_state.block_time % 60) + "sec"))
                //                                + "]. player [UID=" + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_FOREVER)
                //                    {

                //                        p.init_plain((short)0x01);

                //                        p.addUint8(0x0c);       // Acho que seja block permanente, que fala de email
                //                                                //p.addInt32(500012);	// Block Permanente

                //                        packet_func_ls.session_send(p, _session, 0);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Bloqueado permanente. player [UID=" + (pi.uid)
                //                                + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_ALL_IP)
                //                    {

                //                        // Bloquea todos os IP que o player logar e da error de que a area dele foi bloqueada

                //                        // Add o ip do player para a lista de ip banidos
                //                        new CmdInsertBlockIp(_session.m_ip, "255.255.255.255").waitEvent();

                //                        // Resposta
                //                        p.init_plain((short)0x01);

                //                        p.addUint8(16);
                //                        p.addInt32(500012);     // Ban por Região;

                //                        packet_func_ls.session_send(p, _session, 0);
                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Player[UID=" + (_session.m_pi.uid)
                //                                + ", IP=" + (_session.m_ip) + "] Block ALL IP que o player fizer login.", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (pi.block_flag.m_id_state.id_state.st_IDState.L_BLOCK_MAC_ADDRESS)
                //                    {

                //                        // Bloquea o MAC Address que o player logar e da error de que a area dele foi bloqueada

                //                        // Add o MAC Address do player para a lista de MAC Address banidos
                //                        var mac = new CmdInsertBlockMac(result.mac_address);

                //                        mac.waitEvent();
                //                        // Resposta
                //                        p.init_plain((short)0x01);

                //                        p.addUint8(16);
                //                        p.addInt32(500012);     // Ban por Região;

                //                        packet_func_ls.session_send(p, _session, 0);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Player[UID=" + (_session.m_pi.uid)
                //                                + ", IP=" + (_session.m_ip) + ", MAC=" + result.mac_address + "] Block MAC Address que o player fizer login.", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (!cmd_flc.getLastCheck())
                //                    {   // Verifica se fez o primeiro login

                //                        // Authorized a ficar online no server por tempo indeterminado
                //                        _session.m_is_authorized = 1;

                //                        FIRST_LOGIN(_session);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Primeira vez que o player loga. player[UID=" + (pi.uid)
                //                                + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (!cmd_fsc.getLastCheck())
                //                    {   // Verifica se fez o primeiro set do character

                //                        // Authorized a ficar online no server por tempo indeterminado
                //                        _session.m_is_authorized = 1;

                //                        FIRST_SET(_session);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Primeira vez que o player escolhe um character padrao. player[UID="
                //                                + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (cmd_lc.getLastCheck())
                //                    {   // Verifica se já esta logado no game server

                //                        // Pega o Server UID para usar depois no packet004, para derrubar do server
                //                        _session.m_pi.m_server_uid = cmd_lc.getServerUID();

                //                        // Já está varrizado a ficar online, o login server só vai derrubar o outro que está online no game server
                //                        // Authorized a ficar online no server por tempo indeterminado
                //                        _session.m_is_authorized = 1;

                //                        p.init_plain((short)0x01);

                //                        p.addUint8(4);

                //                        packet_func_ls.session_send(p, _session, 0);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] Player ja esta logado no game server. player[UID="
                //                                + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else if (Convert.ToBoolean(pi.m_cap & 4))
                //                    {   // Acesso permtido

                //                        // Authorized a ficar online no server por tempo indeterminado
                //                        _session.m_is_authorized = 1;

                //                        SUCCESS_LOGIN("requestLogin", _session);

                //                        _smp.Message_Pool.push("[login_server::requestLogin][Log] GM logou[UID=" + (pi.uid)
                //                                + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                    }
                //                    else
                //                    {

                //                        // Authorized a ficar online no server por tempo indeterminado
                //                        _session.m_is_authorized = 1;

                //                        SUCCESS_LOGIN("requestLogin", _session);
                //                    }

                //                }
                //                else if (!cmd_flc.getLastCheck())
                //                {   // Verifica se fez o primeiro login

                //                    // Authorized a ficar online no server por tempo indeterminado
                //                    _session.m_is_authorized = 1;

                //                    FIRST_LOGIN(_session);

                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] Primeira vez que o player loga. player[UID=" + (pi.uid)
                //                            + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                }
                //                else if (!cmd_fsc.getLastCheck())
                //                {   // Verifica se fez o primeiro set do character

                //                    // Authorized a ficar online no server por tempo indeterminado
                //                    _session.m_is_authorized = 1;

                //                    FIRST_SET(_session);

                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] Primeira vez que o player escolhe um character padrao. player[UID="
                //                            + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                }
                //                else if (cmd_lc.getLastCheck())
                //                {   // Verifica se já esta logado no game server

                //                    // Pega o Server UID para usar depois no packet004, para derrubar do server
                //                    _session.m_pi.m_server_uid = cmd_lc.getServerUID();

                //                    // Já está varrizado a ficar online, o login server só vai derrubar o outro que está online no game server
                //                    // Authorized a ficar online no server por tempo indeterminado
                //                    _session.m_is_authorized = 1;

                //                    p.init_plain((short)0x01);

                //                    p.addUint8(4);

                //                    packet_func_ls.session_send(p, _session, 0);

                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] Player ja esta logado no game server. player[UID="
                //                            + (pi.uid) + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                }
                //                else if (Convert.ToBoolean(pi.m_cap & 4))
                //                {   // Acesso permtido

                //                    // Authorized a ficar online no server por tempo indeterminado
                //                    _session.m_is_authorized = 1;

                //                    SUCCESS_LOGIN("requestLogin", _session);

                //                    _smp.Message_Pool.push("[login_server::requestLogin][Log] GM logou[UID=" + (pi.uid)
                //                            + ", ID=" + (pi.id) + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //                }
                //                else
                //                {

                //                    // Authorized a ficar online no server por tempo indeterminado
                //                    _session.m_is_authorized = 1;

                //                    SUCCESS_LOGIN("requestLogin", _session);
                //                }
                //            }

                //        }
                //        else
                //        {

                //            p = packet_func_ls.pacote001(_session, 6/* ID ou PW errado*/);
                //            packet_func_ls.session_send(p, _session, 1); // Erro pass


                //            _smp.Message_Pool.push("[login_server::requestLogin][Log] senha errada. ID: " + cmd_verifyId.getID()
                //                    + "  senha: " + pass_md5/*cmd_verifyPass.getPass()*/, _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
                //        }

                //    }

                //    else if (!getAccessFlag() && getCreateUserFlag())
                //    {

                //        //// Authorized a ficar online no server por tempo indeterminado
                //        _session.m_is_authorized = 1;

                //        _smp.Message_Pool.push("[login_server::requestLogin][Log] Criando um novo usuario[ID=" + cmd_verifyId.getID()
                //                + ", PASSWORD=" + pass_md5/*pass*/ + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //        var ip = (_session.getIP());

                //        var cmd_cu = new CmdCreateUser(cmd_verifyId.getID(), result.password, ip, m_si.UID);


                //        cmd_cu.waitEvent();

                //        if (cmd_cu.getException().getCodeError() != 0)
                //            throw cmd_cu.getException();

                //        var pi = _session.m_pi;

                //        pi.uid = cmd_cu.getUID();

                //        var cmd_pi = new CmdPlayerInfo(pi.uid);

                //        cmd_pi.waitEvent();

                //        if (cmd_pi.getException().getCodeError() != 0)
                //            throw cmd_pi.getException();

                //        pi = cmd_pi.getInfo().GetInfo();

                //        FIRST_LOGIN(_session);

                //        // Log
                //        _smp.Message_Pool.push("[login_server::requestLogin][Log] Conta Criada com sucesso. Player[UID=" + (pi.uid)
                //                + ", ID=" + pi.id + ", PASSWORD=" + pass_md5/*pi.pass*/ + "]", _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //    }
                //    else
                //    {

                //        p = packet_func_ls.pacote001(_session, 6/*ID é 2, 6 é o ID ou pw errado*/);
                //        packet_func_ls.session_send(p, _session, 1);
                //        _session.m_pi.id = result.id;
                //        _smp.Message_Pool.push("[login_server::requestLogin][Log] ID nao existe, ID: " + cmd_verifyId.getID(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
                //        Disconnect(_session);
                //    }

                //}
                //else
                //{   // Ban IP/MAC por região

                //    p.init_plain((short)0x01);

                //    p.addUint8(16);
                //    p.addInt32(500012);     // Ban por Região;

                //    packet_func_ls.session_send(p, _session, 0);
                //    _smp.Message_Pool.push("[login_server::requestLogin][Log] Block por Regiao o IP/MAC: " + (_session.m_ip) + "/" + result.mac_address, _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
                //}

            }

            catch (exception e)
            {

                _smp.Message_Pool.push("[login_server::requestLogin][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);

                //if (e.getCodeError() == STDA_ERROR_TYPE.LOGIN_SERVER)
                //{

                //    // Invalid ID
                //    p = packet_func_ls.pacote001(_session, 2/*Invlid ID*/);
                //    packet_func_ls.session_send(p, _session, 1);

                //}
                //else
                //{

                //    // Unknown Error (System Fail)
                //    p.init_plain((short)0x01);

                //    p.addUint8(0xE2);
                //    p.addInt32(500050);     // System Error

                //    packet_func_ls.session_send(p, _session, 0);
                //}

            }
        }            
    }
}
