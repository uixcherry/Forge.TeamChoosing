using Forge.TeamChoosing.Modules;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Forge.TeamChoosing
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance { get; private set; }
        public Dictionary<string, CSteamID> teamIds = new Dictionary<string, CSteamID>();

        protected override void Load()
        {
            Instance = this;

            GroupManager.groupInfoReady += OnGroupInfoReady;

            U.Events.OnPlayerConnected += Events.OnPlayerConnected;
            EffectManager.onEffectButtonClicked += Events.onEffectButtonClicked;
            UnturnedPlayerEvents.OnPlayerRevive += Events.OnPlayerRevive;

            CreateTeamGroups();
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerRevive += Events.OnPlayerRevive;

            U.Events.OnPlayerConnected -= Events.OnPlayerConnected;
            EffectManager.onEffectButtonClicked -= Events.onEffectButtonClicked;

            GroupManager.groupInfoReady -= OnGroupInfoReady;

            Instance = null;
        }

        private void OnGroupInfoReady(GroupInfo group)
        {
            Logger.Log($"Group info received: {group.name} (ID: {group.groupID})");
        }

        private void CreateTeamGroups()
        {
            foreach (var teamConfig in Configuration.Instance.Teams)
            {
                var groupId = CreateGroupIfNotExist(teamConfig.Name);
                teamIds[teamConfig.Name] = groupId;
                Logger.Log($"Team '{teamConfig.Name}' assigned to group ID: {groupId}");
            }
        }

        private GroupInfo getGroupInfoByName(string groupName)
        {
            var knownGroups = GetKnownGroups();
            foreach (var group in knownGroups.Values)
            {
                if (group.name == groupName)
                {
                    return group;
                }
            }
            return null;
        }

        private CSteamID CreateGroupIfNotExist(string groupName)
        {
            GroupInfo existingGroup = getGroupInfoByName(groupName);
            if (existingGroup != null)
            {
                return existingGroup.groupID;
            }

            CSteamID newGroupId = GroupManager.generateUniqueGroupID();
            bool wasCreated;
            GroupInfo newGroupInfo = GroupManager.getOrAddGroup(newGroupId, groupName, out wasCreated);
            if (wasCreated)
            {
                newGroupInfo.members = 0;
                GroupManager.sendGroupInfo(newGroupInfo);
                Logger.Log($"Created new group '{groupName}' with ID: {newGroupId}");
            }

            return newGroupId;
        }

        private Dictionary<CSteamID, GroupInfo> GetKnownGroups()
        {
            var field = typeof(GroupManager).GetField("knownGroups", BindingFlags.NonPublic | BindingFlags.Static);
            return (Dictionary<CSteamID, GroupInfo>)field.GetValue(null);
        }

        public void TeamChoosingUI(UnturnedPlayer player)
        {
            var transportConnection = player.Player.channel.owner.transportConnection;

            EffectManager.sendUIEffect(Configuration.Instance.EffectID, Configuration.Instance.EffectKey, transportConnection, true);

            for (int i = 0; i < Configuration.Instance.Teams.Count; i++)
            {
                var teamConfig = Configuration.Instance.Teams[i];
                var effectData = new Dictionary<string, string>
                {
                    { $"forge.team{i + 1}name", teamConfig.Name },
                    { $"forge.team{i + 1}desc", teamConfig.Description },
                    { $"forge.team{i + 1}members", GetTeamMemberCount(teamIds[teamConfig.Name]).ToString() }
                };

                foreach (var data in effectData)
                {
                    EffectManager.sendUIEffectText(Configuration.Instance.EffectKey, transportConnection, true, data.Key, data.Value);
                }
            }
        }

        public int GetTeamMemberCount(CSteamID teamId)
        {
            return Provider.clients.Count(client => client.player != null && client.player.quests.groupID == teamId);
        }
    }
}