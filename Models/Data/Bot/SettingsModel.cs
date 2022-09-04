using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class SettingsModel {
    [Key]
    public Int64 GuildId { get; set; }
    public Dictionary<string, string> Settings { get; set; }
}