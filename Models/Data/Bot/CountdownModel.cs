using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class CountdownModel {
    [Key]
    public Int64 MessageId { get; set; }
    public Int64 ChannelId { get; set; }
    public Int64 GuildId { get; set; }
    public DateTime DateTime { get; set; }
    public Int64? PingUserId { get; set; }
    public Int64? PingRoleId { get; set; }
}