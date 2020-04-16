using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;

namespace NoRoleMute
{
    public class MutePlugin : Plugin
    {
        internal MuteEventManager PLEV;
        public override string getName => "NoRoleMute";

        public override void OnDisable()
        {
            if (!Config.GetBool("no_role_mute_enable"))
                return;
            Events.RoundEndEvent -= PLEV.RoundEnd;
            Events.RoundStartEvent -= PLEV.RoundStart;
            Events.WaitingForPlayersEvent -= PLEV.RoundWait;
            Events.RemoteAdminCommandEvent -= PLEV.RACmd;
            Events.PlayerJoinEvent -= PLEV.PlyJoin;
            Events.ConsoleCommandEvent -= PLEV.PlyCmd;
            PLEV = null;
        }

        public override void OnEnable()
        {
            if (!Config.GetBool("no_role_mute_enable"))
                return;
            PLEV = new MuteEventManager(this);
            Events.RoundEndEvent += PLEV.RoundEnd;
            Events.RoundStartEvent += PLEV.RoundStart;
            Events.WaitingForPlayersEvent += PLEV.RoundWait;
            Events.RemoteAdminCommandEvent += PLEV.RACmd;
            Events.PlayerJoinEvent += PLEV.PlyJoin;
            Events.ConsoleCommandEvent += PLEV.PlyCmd;
        }

        public override void OnReload()
        {
        }
    }
}
