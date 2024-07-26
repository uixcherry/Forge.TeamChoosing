using Forge.TeamChoosing.Modules;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forge.TeamChoosing.Commands
{
    public class CommandTeam : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "team";
        public string Help => "Manage teams and players.";
        public string Syntax => "/team <change|info|wipe|kick|setspawn|setstart> [arguments]";
        public List<string> Aliases => new List<string> { "t" };
        public List<string> Permissions => new List<string> { "forge.team" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, Help);
                return;
            }

            switch (command[0].ToLower())
            {
                case "change":
                    HandleChangeCommand(caller, command);
                    break;
                case "info":
                    HandleInfoCommand(caller, command);
                    break;
                case "wipe":
                    HandleWipeCommand(caller);
                    break;
                case "kick":
                    HandleKickCommand(caller, command);
                    break;
                case "setspawn":
                    HandleSetSpawnCommand(caller, command);
                    break;
                case "setstart":
                    HandleSetStartCommand(caller);
                    break;
                default:
                    UnturnedChat.Say(caller, $"Invalid command. Use {Syntax}");
                    break;
            }
        }

        private void HandleChangeCommand(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 3)
            {
                UnturnedChat.Say(caller, $"Usage: /team change <player> <team>");
                return;
            }

            var playerName = command[1];
            var teamName = command[2];
            var teamConfig = Plugin.Instance.Configuration.Instance.Teams.FirstOrDefault(team => team.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (teamConfig == null)
            {
                UnturnedChat.Say(caller, $"Team '{teamName}' not found.");
                return;
            }

            var targetPlayer = UnturnedPlayer.FromName(playerName);
            if (targetPlayer == null)
            {
                UnturnedChat.Say(caller, $"Player '{playerName}' not found.");
                return;
            }

            Events.AssignPlayerToTeam(targetPlayer, teamConfig);
            UnturnedChat.Say(caller, $"Player '{playerName}' has been moved to team '{teamName}'.");
        }

        private void HandleInfoCommand(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, $"Usage: /team info <team>");
                return;
            }

            var teamName = command[1];
            var teamConfig = Plugin.Instance.Configuration.Instance.Teams.FirstOrDefault(team => team.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (teamConfig == null)
            {
                UnturnedChat.Say(caller, $"Team '{teamName}' not found.");
                return;
            }

            var teamMembers = Provider.clients
                .Where(client => client.player.quests.groupID == Plugin.Instance.teamIds[teamConfig.Name])
                .Select(client => client.playerID.playerName)
                .ToList();

            UnturnedChat.Say(caller, $"Team '{teamName}':\nMembers: {string.Join(", ", teamMembers)}");
        }

        private void HandleWipeCommand(IRocketPlayer caller)
        {
            foreach (var team in Plugin.Instance.Configuration.Instance.Teams)
            {
                var teamID = Plugin.Instance.teamIds[team.Name];
                GroupManager.deleteGroup(teamID);
            }

            UnturnedChat.Say(caller, "All teams have been wiped.");
        }

        private void HandleKickCommand(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, $"Usage: /team kick <player>");
                return;
            }

            var playerName = command[1];
            var targetPlayer = UnturnedPlayer.FromName(playerName);

            if (targetPlayer == null)
            {
                UnturnedChat.Say(caller, $"Player '{playerName}' not found.");
                return;
            }

            targetPlayer.Player.quests.leaveGroup(true);
            UnturnedChat.Say(caller, $"Player '{playerName}' has been kicked from their team.");
        }

        private void HandleSetSpawnCommand(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, $"Usage: /team setspawn <team>");
                return;
            }

            var teamName = command[1];
            var teamConfig = Plugin.Instance.Configuration.Instance.Teams.FirstOrDefault(team => team.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (teamConfig == null)
            {
                UnturnedChat.Say(caller, $"Team '{teamName}' not found.");
                return;
            }

            var player = caller as UnturnedPlayer;
            if (player == null)
            {
                UnturnedChat.Say(caller, "This command can only be used by a player.");
                return;
            }

            teamConfig.BaseLocation = player.Position;
            Plugin.Instance.Configuration.Save();
            UnturnedChat.Say(caller, $"Spawn point for team '{teamName}' has been set to your current location.");
        }

        private void HandleSetStartCommand(IRocketPlayer caller)
        {
            var player = caller as UnturnedPlayer;
            if (player == null)
            {
                UnturnedChat.Say(caller, "This command can only be used by a player.");
                return;
            }

            Plugin.Instance.Configuration.Instance.DefaultBaseLocation = player.Position;
            Plugin.Instance.Configuration.Save();
            UnturnedChat.Say(caller, "Start location for new players has been set to your current location.");
        }
    }
}