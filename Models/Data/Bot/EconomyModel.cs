using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class EconomyModel {
    [Key]
    public Int64 GuildId { get; set; }
    [Key]
    public Int64 UserId { get; set; }
    public int Money { get; set; }
    public int Bank { get; set; }
}
