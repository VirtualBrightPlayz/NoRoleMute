using System;
using System.Collections.Generic;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace NoRoleMute
{
    public class MuteEventManager
    {
        public MutePlugin plugin;
        public static List<string> notActualMutedPlayers = new List<string>();
        public static bool isMuteTime = true;

        public MuteEventManager(MutePlugin pl)
        {
            plugin = pl;
            if (notActualMutedPlayers == null)
                notActualMutedPlayers = new List<string>();
            Timing.RunCoroutine(CheckMuteLateCont());
        }

        private IEnumerator<float> CheckMuteLateCont()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                //Log.Debug("Mute Time?");
                if (isMuteTime)
                {
                    //Log.Debug("Yes");
                    MuteAllThem();
                }
                else
                {
                    UnmuteAllThem();
                    break;
                    //Log.Debug("No");
                    //UnmuteThem();
                }
            }
        }

        internal void RoundEnd()
        {
            isMuteTime = true;
            MuteAllThem();
            Timing.RunCoroutine(CheckMuteLateCont());
        }

        internal void RoundStart()
        {
            isMuteTime = false;
            UnmuteAllThem();
            notActualMutedPlayers.Clear();
        }

        public void MuteAllThem()
        {
            foreach (var player in PlayerManager.players)
            {
                if (isMuteTime)
                {
                    CheckMute(player.GetPlayer());
                }
                else
                {
                    CheckUnmute(player.GetPlayer());
                }
            }
        }

        public void UnmuteAllThem()
        {
            foreach (var player in PlayerManager.players)
            {
                if (isMuteTime)
                {
                    CheckMute(player.GetPlayer());
                }
                else
                {
                    CheckUnmute(player.GetPlayer());
                }
            }
        }

        /*public void UnmuteThem()
        {
            foreach (var ply in PlayerManager.players)
            {
                try
                {
                    if (notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
                    {
                        //Log.Debug("Unmuting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role and were muted by this plugin.");
                        ply.GetComponent<ReferenceHub>().Unmute();
                        ply.GetComponent<ReferenceHub>().characterClassManager.Muted = false;
                        ply.GetComponent<ReferenceHub>().characterClassManager.SetMuted(false);
                        notActualMutedPlayers.Remove(ply.GetComponent<ReferenceHub>().GetUserId());
                        //ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been unmuted.", false);
                    }
                }
                catch (NullReferenceException e)
                {
                }
            }
        }*/

        internal void PlyCmd(ConsoleCommandEvent ev)
        {
        }

        internal void PlyJoin(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(CheckMuteLate(ev.Player));
        }

        private IEnumerator<float> CheckMuteLate(ReferenceHub ply)
        {
            yield return Timing.WaitForSeconds(1.2f);
            Log.Debug("Mute Time?");
            if (isMuteTime)
            {
                Log.Debug("Yes");
                try
                {
                    CheckMute(ply);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
                //MuteThem();
            }
            else
            {
                Log.Debug("No");
                CheckUnmute(ply);
                //UnmuteThem();
            }
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

        public void CheckMute(ReferenceHub ply)
        {
            if (ply.GetComponent<ReferenceHub>().CheckPermission("norolemute.unmute") && notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()) && ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted)
            {
                if (plugin.debug)
                    Log.Info("Player " + ply.GetComponent<ReferenceHub>().GetNickname() + " has a role, unmuting them.");
                ply.GetComponent<ReferenceHub>().Unmute();
                ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been unmuted.", false);
                notActualMutedPlayers.RemoveAll((p) => p.Equals(ply.GetComponent<ReferenceHub>().GetUserId()));
            }
            if (ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted)
            {
                return;
            }
            if (!ply.GetComponent<ReferenceHub>().CheckPermission("norolemute.unmute") && !notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
            {
                if (plugin.debug)
                    Log.Info("Temp muting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role.");
                ply.GetComponent<ReferenceHub>().Unmute();
                ply.GetComponent<ReferenceHub>().Mute();
                notActualMutedPlayers.Add(ply.GetComponent<ReferenceHub>().GetUserId());
                ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been muted.", false);
            }
            if (ply.GetComponent<ReferenceHub>().CheckPermission("norolemute.unmute") && notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
            {
                if (plugin.debug)
                    Log.Info("Player " + ply.GetComponent<ReferenceHub>().GetNickname() + " has a role, unmuting them.");
                ply.GetComponent<ReferenceHub>().Unmute();
                ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been unmuted.", false);
                notActualMutedPlayers.RemoveAll((p) => p.Equals(ply.GetComponent<ReferenceHub>().GetUserId()));
            }
        }

        public void CheckUnmute(ReferenceHub ply)
        {
            if (notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
            {
                if (plugin.debug)
                    Log.Info("Unmuting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role.");
                ply.GetComponent<ReferenceHub>().Unmute();
                notActualMutedPlayers.RemoveAll((p) => p.Equals(ply.GetComponent<ReferenceHub>().GetUserId()));
                ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been unmuted.", false);
            }
        }

        /*public void MuteThem()
        {
            foreach (var ply in PlayerManager.players)
            {
                try
                {
                    //Log.Debug("\"" + ply.GetComponent<ReferenceHub>().serverRoles.MyText + "\"");
                    if (ply.GetComponent<ReferenceHub>().characterClassManager.Muted || ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted)
                    {
                        continue;
                    }
                    if (!ply.GetComponent<ReferenceHub>().CheckPermission("norolemute.unmute") && !ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted/*!notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()) && *//*string.IsNullOrWhiteSpace(ply.GetComponent<ReferenceHub>().serverRoles.MyText)*)
                    {
                        Log.Debug("Temp muting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role.");
                        try
                        {
                            ply.GetComponent<ReferenceHub>().Unmute();
                            ply.GetComponent<ReferenceHub>().Mute();
                            ply.GetComponent<ReferenceHub>().characterClassManager.Muted = true;
                            ply.GetComponent<ReferenceHub>().characterClassManager.SetMuted(true);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Unable to mute! " + e.ToString());
                        }
                        notActualMutedPlayers.Add(ply.GetComponent<ReferenceHub>().GetUserId());
                        if (!notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()))
                        {
                            //ply.GetComponent<ReferenceHub>().Broadcast(3, "[NoRoleMute] You have been muted.", false);
                        }
                    }
                    if (ply.GetComponent<ReferenceHub>().CheckPermission("norolemute.unmute") && ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted)
                    {
                        ply.GetComponent<ReferenceHub>().Unmute();
                        notActualMutedPlayers.Remove(ply.GetComponent<ReferenceHub>().GetUserId());
                    }
                }
                catch (NullReferenceException e)
                {
                }
            }
        }*/

        internal void RoundWait()
        {
            isMuteTime = true;
            //MuteThem();
            Timing.RunCoroutine(CheckMuteLateCont());
        }
    }
}