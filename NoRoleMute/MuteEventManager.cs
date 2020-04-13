using System;
using System.Collections.Generic;
using EXILED;
using EXILED.Extensions;

namespace NoRoleMute
{
    public class MuteEventManager
    {
        public MutePlugin plugin;
        public static List<string> notActualMutedPlayers = new List<string>();
        bool isMuteTime = false;

        public MuteEventManager(MutePlugin pl)
        {
            plugin = pl;
            if (notActualMutedPlayers == null)
                notActualMutedPlayers = new List<string>();
        }

        internal void RoundEnd()
        {
            isMuteTime = true;
            MuteThem();
        }

        internal void RoundStart()
        {
            isMuteTime = false;
            UnmuteThem();
        }

        public void UnmuteThem()
        {
            foreach (var ply in PlayerManager.players)
            {
                if (notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
                {
                    Log.Debug("Unmuting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role and were muted by this plugin.");
                    ply.GetComponent<ReferenceHub>().Unmute();
                    ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been unmuted.", false);
                    notActualMutedPlayers.Remove(ply.GetComponent<ReferenceHub>().GetUserId());
                }
            }
        }

        internal void PlyJoin(PlayerJoinEvent ev)
        {
            if (isMuteTime)
                MuteThem();
            else
                UnmuteThem();
        }

        internal void RACmd(ref RACommandEvent ev)
        {
            string[] args = ev.Command.Split(' ');
            if (args[0].ToLower().StartsWith("mute"))
            {
                foreach (string str in args[1].Split('.'))
                {
                    ReferenceHub hub = Player.GetPlayer(int.Parse(str));
                    notActualMutedPlayers.Remove(hub.GetUserId());
                }
            }
        }

        public void MuteThem()
        {
            foreach (var ply in PlayerManager.players)
            {
                if (ply.GetComponent<ReferenceHub>().IsMuted())
                {
                    continue;
                }
                if (!notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()) && string.IsNullOrWhiteSpace(ply.GetComponent<ReferenceHub>().serverRoles.GetUncoloredRoleString()))
                {
                    Log.Debug("Temp muting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role.");
                    ply.GetComponent<ReferenceHub>().Mute();
                    ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been muted.", false);
                    notActualMutedPlayers.Add(ply.GetComponent<ReferenceHub>().GetUserId());
                }
            }
        }

        internal void RoundWait()
        {
            isMuteTime = true;
            MuteThem();
        }
    }
}