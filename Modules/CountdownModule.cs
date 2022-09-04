using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Repositories.Bot;
using Pathoschild.NaturalTimeParser.Parser;
using System.Text;

namespace Finder.Bot.Modules {
    public class CountdownModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly CountdownRepository countdownRepository;
        public CountdownModule(CountdownRepository _countdownRepository) {
            countdownRepository = _countdownRepository;
        }
        [SlashCommand("countdown", "Countdown to a specific date or time", runMode: RunMode.Async)]
        public async Task CountdownCommand(string datetime, IMentionable? ping = null) {
            DateTime date;
            try {
                date = DateTime.Now.Offset(datetime);
            } catch (TimeParseFormatException) {
                await RespondAsync("Invalid date or time");
                return;
            }
            var timeLeft = date - DateTime.Now;
            if (timeLeft.TotalSeconds < 0) {
                await RespondAsync("Date or time is in the past");
                return;
            }
            if (timeLeft.TotalDays > 365) {
                await RespondAsync("The date or time is too far in the future");
                return;
            }
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Countdown",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Time left",
                        Value = $"{timeLeft} left"
                    }
                },
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
            var messages = await GetOriginalResponseAsync();
            if (ping != null) {
                switch(ping) {
                    case SocketRole role:
                        await countdownRepository.AddCountdownAsync(messages.Id, Context.Channel.Id, Context.Guild.Id, date.ToUniversalTime(), null, role.Id);
                        break;
                    case SocketGuildUser user:
                        await countdownRepository.AddCountdownAsync(messages.Id, Context.Channel.Id, Context.Guild.Id, date.ToUniversalTime(), user.Id, null);
                        break;
                    default:
                        await RespondAsync("Invalid mention");
                        break;
                }
            } else {
                await countdownRepository.AddCountdownAsync(messages.Id, Context.Channel.Id, Context.Guild.Id, date.ToUniversalTime(), null, null);
            }
            await countdownRepository.SaveAsync();
        }

        public static string HumanizeTime(TimeSpan time) {
            var sb = new StringBuilder();
            switch(time) {
                case { Days: > 0 }:
                    sb.Append($"{time.Days} {(time.Days == 1 ? "day" : "days")}");
                    break;
                case { Hours: > 0 }:
                    if (sb.Length != 0) sb.Append(", ");
                    sb.Append($"{time.Hours} {(time.Hours == 1 ? "hour" : "hours")}");
                    break;
                case { Minutes: > 0 }:
                    if (sb.Length != 0) sb.Append(", ");
                    sb.Append($"{time.Minutes} {(time.Minutes == 1 ? "minute" : "minutes")}");
                    break;
                case { Seconds: > 0 }:
                    if (sb.Length != 0) sb.Append(", ");
                    sb.Append($"{time.Seconds} {(time.Seconds == 1 ? "second" : "seconds")}");
                    break;
            }
            return sb.ToString();
        }
    }
}