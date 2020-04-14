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
        bool isMuteTime = true;

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
                    MuteThem();
                }
                else
                {
                    break;
                    //Log.Debug("No");
                    //UnmuteThem();
                }
            }
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
        }

        internal void PlyCmd(ConsoleCommandEvent ev)
        {
        }

        internal void PlyJoin(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(CheckMuteLate());
        }

        private IEnumerator<float> CheckMuteLate()
        {
            yield return Timing.WaitForSeconds(0.5f);
            //Log.Debug("Mute Time?");
            if (isMuteTime)
            {
                //Log.Debug("Yes");
                MuteThem();
            }
            else
            {
                //Log.Debug("No");
                UnmuteThem();
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

        public void MuteThem()
        {
            foreach (var ply in PlayerManager.players)
            {
                try
                {
                    //Log.Debug("\"" + ply.GetComponent<ReferenceHub>().serverRoles.MyText + "\"");
                    if (!notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()) && (ply.GetComponent<ReferenceHub>().characterClassManager.Muted || ply.GetComponent<ReferenceHub>().characterClassManager.NetworkMuted))
                    {
                        continue;
                    }
                    if (/*!notActualMutedPlayers.Contains(ply.GetComponent<ReferenceHub>().GetUserId()) && */string.IsNullOrWhiteSpace(ply.GetComponent<ReferenceHub>().serverRoles.MyText))
                    {
                        //Log.Debug("Temp muting player " + ply.GetComponent<ReferenceHub>().GetNickname() + " as they have no role.");
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
                }
                catch (NullReferenceException e)
                {
                }
            }
        }

        internal void RoundWait()
        {
            isMuteTime = true;
            MuteThem();
            Timing.RunCoroutine(CheckMuteLateCont());
        }
    }
}