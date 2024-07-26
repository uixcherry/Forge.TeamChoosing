using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Forge.TeamChoosing
{
    public class Configuration : IRocketPluginConfiguration
    {
        public string DiscordText { get; set; } = "Join our Discord!";
        public string DiscordURL { get; set; } = "https://your-discord-link";

        public string WebText { get; set; } = "Visit our Website!";
        public string WebURL { get; set; } = "https://your-website-link";

        public ushort EffectID { get; set; } = 100;
        public short EffectKey { get; set; } = 1;

        public byte MaxTeamImbalance { get; set; } = 2;

        [XmlArrayItem(ElementName = "Team")]
        public List<TeamConfig> Teams { get; set; } = new List<TeamConfig>();

        public void LoadDefaults()
        {
            Teams = new List<TeamConfig>
            {
                new TeamConfig {
                    Name = "Team Alpha",
                    Description = "The fearless warriors.",
                    Permission = "forge.team.alpha",
                    Group = "alpha",
                    RadioFrequency = 1,
                    BaseLocation = new Vector3(0, 0, 0)
                },
                new TeamConfig {
                    Name = "Team Bravo",
                    Description = "The cunning strategists.",
                    Permission = "forge.team.bravo",
                    Group = "bravo",
                    RadioFrequency = 2,
                    BaseLocation = new Vector3(100, 0, 100)
                }
            };
        }
    }

    public class TeamConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Permission { get; set; }
        public string Group { get; set; }
        public uint RadioFrequency { get; set; }
        public Vector3 BaseLocation { get; set; }
    }
}