using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class AddonsModel {
    [Key]
    public Int64 GuildId { get; set; }
    public Dictionary<string, string> Addons { get; set; }
}