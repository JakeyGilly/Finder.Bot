using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Database.Repositories;
using Finder.Bot.Resources;
using Pathoschild.NaturalTimeParser.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
                await RespondAsync(CountdownLocale.CountdownError_invalid);
                return;
            }
            var timeLeft = date - DateTime.Now;
            if (timeLeft.TotalSeconds < 0) {
                await RespondAsync(CountdownLocale.CountdownError_past);
                return;
            }
            if (timeLeft.TotalDays > 365) {
                await RespondAsync(CountdownLocale.CountdownError_future);
                return;
            }
            await RespondAsync(embed: new EmbedBuilder() {
                Title = CountdownLocale.CountdownEmbed_title,
                Color = Color.Orange,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = CountdownLocale.CountdownEmbed_fieldName,
                        Value = string.Format(CountdownLocale.CountdownEmbed_fieldValue, timeLeft)
                    }
                },
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
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
                        await RespondAsync(CountdownLocale.CountdownError_ping);
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
                case { Days: > 0 } t:
                    sb.AppendFormat(CountdownLocale.CountdownDays, time.Days, (time.Days == 1 ? CountdownLocale.CountdownSingular : CountdownLocale.CountdownPlural));
                    break;
                case { Hours: > 0 } t:
                    if (sb.Length != 0) sb.Append(CountdownLocale.CountdownSeperator);
                    sb.AppendFormat(CountdownLocale.CountdownHours, time.Hours, (time.Hours == 1 ? CountdownLocale.CountdownSingular : CountdownLocale.CountdownPlural));
                    break;
                case { Minutes: > 0 } t:
                    if (sb.Length != 0) sb.Append(CountdownLocale.CountdownSeperator);
                    sb.AppendFormat(CountdownLocale.CountdownMins, time.Minutes, (time.Minutes == 1 ? CountdownLocale.CountdownSingular : CountdownLocale.CountdownPlural));
                    break;
                case { Seconds: > 0 } t:
                    if (sb.Length != 0) sb.Append(CountdownLocale.CountdownSeperator);
                    sb.AppendFormat(CountdownLocale.CountdownSecs, time.Seconds, (time.Seconds == 1 ? CountdownLocale.CountdownSingular : CountdownLocale.CountdownPlural));
                    break;
            }
            return sb.ToString();
        }
    }
}