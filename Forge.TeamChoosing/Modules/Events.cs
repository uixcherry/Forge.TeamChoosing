using Rocket.Unturned.Player;
using SDG.Unturned;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Forge.TeamChoosing.Modules
{
    public static class Events
    {
        public static void onEffectButtonClicked(Player player, string buttonName)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);

            switch (buttonName)
            {
                case "forge.teamc_discord":
                    player.sendBrowserRequest(Plugin.Instance.Configuration.Instance.DiscordText, Plugin.Instance.Configuration.Instance.DiscordURL);
                    break;
                case "forge.teamc_web":
                    player.sendBrowserRequest(Plugin.Instance.Configuration.Instance.WebText, Plugin.Instance.Configuration.Instance.WebURL);
                    break;

                case "forge.teamc_team1claim":
                    AssignPlayerToTeam(unturnedPlayer, Plugin.Instance.Configuration.Instance.Teams[0]);
                    break;
                case "forge.teamc_team2claim":
                    AssignPlayerToTeam(unturnedPlayer, Plugin.Instance.Configuration.Instance.Teams[1]);
                    break;
            }
        }

        public static void AssignPlayerToTeam(UnturnedPlayer player, TeamConfig teamConfig)
        {
            if (!HasPermission(player, teamConfig.Permission))
            {
                var team1Count = Plugin.Instance.GetTeamMemberCount(Plugin.Instance.teamIds[Plugin.Instance.Configuration.Instance.Teams[0].Name]);
                var team2Count = Plugin.Instance.GetTeamMemberCount(Plugin.Instance.teamIds[Plugin.Instance.Configuration.Instance.Teams[1].Name]);

                if (Math.Abs(team1Count - team2Count) <= Plugin.Instance.Configuration.Instance.MaxTeamImbalance)
                {
                    var permissionResult = R.Permissions.AddPlayerToGroup(teamConfig.Group, player);
                    if (permissionResult == RocketPermissionsProviderResult.Success)
                    {
                        player.Player.quests.ServerAssignToGroup(Plugin.Instance.teamIds[teamConfig.Name], EPlayerGroupRank.MEMBER, true);
                        player.Player.quests.sendSetRadioFrequency(teamConfig.RadioFrequency);
                        UnturnedChat.Say(player, $"You have been assigned to {teamConfig.Name}.");
                        TeleportPlayer(player, teamConfig.BaseLocation);
                    }
                    else
                    {
                        UnturnedChat.Say(player, $"Failed to assign permissions for {teamConfig.Name}.");
                    }
                }
                else
                {
                    UnturnedChat.Say(player, $"Cannot join {teamConfig.Name}. Team imbalance would be too great.");
                }
            }
            else
            {
                UnturnedChat.Say(player, $"You are already a member of {teamConfig.Name}.");
            }
        }

        private static void TeleportPlayer(UnturnedPlayer player, Vector3 location)
        {
            player.Teleport(location, player.Player.transform.rotation.eulerAngles.y);
        }

        private static bool HasPermission(UnturnedPlayer player, string permission)
        {
            return R.Permissions.HasPermission(player, new List<string> { permission });
        }

        public static void OnPlayerConnected(UnturnedPlayer player)
        {
            if (!HasTeamPermission(player))
            {
                Plugin.Instance.TeamChoosingUI(player);
                TeleportPlayer(player, Plugin.Instance.Configuration.Instance.DefaultBaseLocation);
            }
        }

        private static bool HasTeamPermission(UnturnedPlayer player)
        {
            var playerGroups = R.Permissions.GetGroups(player, true);
            var validPermissions = Plugin.Instance.Configuration.Instance.Teams
                .Where(team => playerGroups.Any(group => R.Permissions.HasPermission(player, new List<string> { team.Permission })))
                .ToList();

            return validPermissions.Any();
        }

        public static void OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            var playerGroups = R.Permissions.GetGroups(player, true);
            var team = Plugin.Instance.Configuration.Instance.Teams
                .FirstOrDefault(t => playerGroups.Any(group => R.Permissions.HasPermission(player, new List<string> { t.Permission })));

            if (team != null)
            {
                TeleportPlayer(player, team.BaseLocation);
            }
        }
    }
}