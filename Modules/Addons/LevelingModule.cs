using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Database.Repositories.Bot;

namespace Finder.Bot.Modules.Addons {
    [Group("leveling", "Command For Managing Leveling")]
    public class LevelingModule : InteractionModuleBase<ShardedInteractionContext> {
        private readonly LevelingRepository levelingRepository;
        private readonly AddonsRepository addonsRepository;
        public LevelingModule(LevelingRepository _levelingRepository, AddonsRepository _addonsRepository) {
            levelingRepository = _levelingRepository;
            addonsRepository = _addonsRepository;
        }

        [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
        public async Task LevelCommand() {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Leveling")) {
                await RespondAsync(embed: new EmbedBuilder {
                    Title = "Leveling",
                    Description = "This addon is disabled on this server.",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "Enable",
                            Value = "Use `/addons install Leveling` to enable this addon."
                        }
                    }
                }.Build());
                return;
            }
            var levels = await levelingRepository.GetLevelingModelAsync(((SocketGuildUser)Context.User).Guild.Id, Context.User.Id);
            await RespondAsync(embed: new EmbedBuilder {
                Title = "Level",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder {
                        Name = "Level",
                        Value = levels.Level.ToString()
                    },
                    new EmbedFieldBuilder {
                        Name = "Exp",
                        Value = levels.Exp.ToString()
                    }
                },
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
        }
        
        public async Task OnMessageReceivedEvent(SocketMessage message) {
            if (!await addonsRepository.AddonEnabled(Context.Guild.Id, "Leveling")) return;
            if (message.Author.IsBot) return;
            var levels = await levelingRepository.GetLevelingModelAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id);
            var expToGet = 50 * (int)Math.Pow(1.5, levels.Level + 1);
            if (++levels.Exp > expToGet) {
                await levelingRepository.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, 0);
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder {
                    Title = $"Level Up {message.Author.Username}",
                    Fields = new List<EmbedFieldBuilder> {
                        new EmbedFieldBuilder {
                            Name = "You have leveled up to level",
                            Value = levels.Level + 1
                        }
                    },
                    Footer = new EmbedFooterBuilder {
                        Text = "FinderBot"
                    }
                }.Build());
            } else {
                await levelingRepository.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, levels.Exp);
            }
        }
    }
}