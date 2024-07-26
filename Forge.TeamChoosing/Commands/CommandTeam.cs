using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Forge.TeamChoosing.Commands
{
    public class CommandTeam : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "team";
        public string Help => "Manage teams and players.";
        public string Syntax => "/team <change|info|wipe|kick> [arguments]";
        public List<string> Aliases => new List<string> { "t" };
        public List<string> Permissions => new List<string> { "forge.team" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, Help);
                return;
            }

            switch (command[0].ToLower())
            {
                case "change":
                    HandleChangeCommand(player, command);
                    break;
                case "info":
                    HandleInfoCommand(player, command);
                    break;
                case "wipe":
                    HandleWipeCommand(player);
                    break;
                case "kick":
                    HandleKickCommand(player, command);
                    break;
                default:
                    UnturnedChat.Say(caller, $"Invalid command. Use {Syntax}");
                    break;
            }
        }

        private void HandleChangeCommand(UnturnedPlayer player, string[] command)
        {
            // TODO: Implement /team change <player> <team> logic
            UnturnedChat.Say(player, "Changing team... (not implemented)");
        }

        private void HandleInfoCommand(UnturnedPlayer player, string[] command)
        {
            // TODO: Implement /team info <team> logic
            UnturnedChat.Say(player, "Getting team info... (not implemented)");
        }

        private void HandleWipeCommand(UnturnedPlayer player)
        {
            // TODO: Implement /team wipe logic
            UnturnedChat.Say(player, "Wiping teams... (not implemented)");
        }

        private void HandleKickCommand(UnturnedPlayer player, string[] command)
        {
            // TODO: Implement /team kick <player> logic
            UnturnedChat.Say(player, "Kicking player... (not implemented)");
        }
    }
}