using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Database.Repositories;
using Finder.Bot.Resources;
using Finder.Database.Repositories.Bot;

namespace Finder.Bot.Modules {
    [Group("leveling", "Command For Managing Leveling")]
    public class LevelingModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly LevelingRepository context;
        public LevelingModule(LevelingRepository _context) {
            context = _context;
        }

        [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
        public async Task LevelCommand() {
            var levels = await context.GetLevelingAsync(((SocketGuildUser)Context.User).Guild.Id, Context.User.Id);
            await RespondAsync(embed: new EmbedBuilder() {
                Title = LevelingLocale.LevelingEmbedLevel_title,
                Color = Color.Orange,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        Name = LevelingLocale.LevelingEmbedLevel_field0Name,
                        Value = levels.Level.ToString()
                    },
                    new EmbedFieldBuilder() {
                        Name = LevelingLocale.LevelingEmbedLevel_field1Name,
                        Value = levels.Exp.ToString()
                    }
                },
                Footer = new EmbedFooterBuilder() {
                    Text = Main.EmbedFooter
                }
            }.Build());
        }
        
        public async Task OnMessageReceivedEvent(SocketMessage message) {
            if (message.Author.IsBot) return;
            var levels = await context.GetLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id);
            var expToGet = 50 * (int)Math.Pow(1.5, levels.Level + 1);
            if (++levels.Exp > expToGet) {
                await context.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, 0);
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder() {
                    Title = string.Format(LevelingLocale.LevelingEmbedLvlUp_title, message.Author.Username),
                    Color = Color.Orange,
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder() {
                            Name = LevelingLocale.LevelingEmbedLvlUp_fieldName,
                            Value = levels.Level + 1
                        }
                    },
                    Footer = new EmbedFooterBuilder() {
                        Text = Main.EmbedFooter
                    }
                }.Build());
            } else {
                await context.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, levels.Exp);
            }
        }
    }
}