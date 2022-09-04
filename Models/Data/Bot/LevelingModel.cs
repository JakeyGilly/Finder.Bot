using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class LevelingModel {
    [Key]
    public Int64 GuildId { get; set; }
    [Key]
    public Int64 UserId { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
}