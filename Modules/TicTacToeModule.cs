using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Finder.Bot.Resources;

namespace Finder.Bot.Modules {
    // TODO:
    // delete channel after
    // check win conditions
    // check delays on line 81
    // make so you dont need context in class
    public class TicTacToeModule : InteractionModuleBase<ShardedInteractionContext> {
        private static List<string> validEmotes = new List<string>() {
            "1️⃣",
            "2️⃣",
            "3️⃣",
            "4️⃣",
            "5️⃣",
            "6️⃣",
            "7️⃣",
            "8️⃣",
            "9️⃣",
            "✅",
            "❌"
        };
        private static List<TicTacToe> games = new List<TicTacToe>();

        [SlashCommand("tictactoe", "Play TicTacToe", runMode: RunMode.Async)]
        public async Task TicTacToeCommand(SocketGuildUser user) {
            if (user.IsBot) {
                await RespondAsync(TicTacToeLocale.TicTacToeError_userBot);
                return;
            }
            if (user.Id == Context.User.Id) {
                await RespondAsync(TicTacToeLocale.TicTacToeError_self);
                return;
            }
            await RespondAsync(string.Format(TicTacToeLocale.TicTacToeInvite, user.Mention, Context.User.Mention));
            IUser p1 = new Random().Next(0, 2) == 0 ? Context.User : user;
            IUser p2 = Context.User == p1 ? user : Context.User;
            var p1Symbol = new Random().Next(0, 2) == 0 ? "❌" : "⭕";
            var p2Symbol = p1Symbol == "❌" ? "⭕" : "❌";
            games.Add(new TicTacToe(Context, p1, p2, p1Symbol, p2Symbol));
        }
        public static async Task OnReactionAddedEvent(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction) {
            foreach (var game in games) {
                if (reaction.User.Value.IsBot) return;
                var valid = validEmotes.Any(symbol => reaction.Emote.Name == symbol);
                if (!valid) return;
                if (game.playChannel == null || game.lobbyMessage == null) continue;
                if (game.playChannel.Id != reaction.Channel.Id) continue;
                if (game.lobby && reaction.MessageId == game.lobbyMessage.Id) {
                    switch(reaction.Emote.Name) {
                        case "✅":
                            game.p1Ready = game.player1.Id == reaction.UserId || game.p1Ready;
                            game.p2Ready = game.player2.Id == reaction.UserId || game.p2Ready;
                            await game.playChannel.SendMessageAsync(string.Format(TicTacToeLocale.TicTacToeReady, reaction.User.Value.Mention));
                            if (!game.p1Ready || !game.p2Ready) return;
                            game.lobby = false;
                            await game.playChannel.SendMessageAsync(string.Format(TicTacToeLocale.TicTacToeStarting, game.player1.Mention, game.player2.Mention));
                            game.playMessage = await game.playChannel.SendMessageAsync(embed: new EmbedBuilder() {
                                Title = TicTacToeLocale.TicTacToeEmbed_title,
                                Color = Color.Blue,
                                Fields = new List<EmbedFieldBuilder> {
                                    new EmbedFieldBuilder {
                                        Name = TicTacToeLocale.TicTacToeEmbed_fieldName,
                                        Value = TicTacToeLocale.TicTacToeEmbed_fieldValue_wait
                                    }
                                },
                                Footer = new EmbedFooterBuilder {
                                    Text = Main.EmbedFooter
                                }
                            }.Build());
                            var emojis = game.board.Select(item => new Emoji(item)).ToList();
                            await game.playMessage.AddReactionsAsync(emojis);
                            await game.playMessage.ModifyAsync((x) => {
                                x.Embed = new EmbedBuilder() {
                                    Title = TicTacToeLocale.TicTacToeEmbed_title,
                                    Color = Color.Orange,
                                    Fields = new List<EmbedFieldBuilder> {
                                        new EmbedFieldBuilder {
                                            Name = TicTacToeLocale.TicTacToeEmbed_fieldName,
                                            Value = game.GenerateGrid()
                                        }
                                    },
                                    Footer = new EmbedFooterBuilder {
                                        Text = Main.EmbedFooter
                                    }
                                }.Build();
                                x.Content = string.Format(TicTacToeLocale.TicTacToeMessage, game.player1.Mention, game.p1Symbol, game.player2.Mention, game.p2Symbol, game.player1.Mention);
                            });
                            return;
                        case "❌":
                            await game.playChannel.SendMessageAsync(string.Format(TicTacToeLocale.TicTacToeCancelled, reaction.User.Value.Mention));
                            // dunno if this makes the bot unresponsive
                            Thread.Sleep(2000);
                            await game.playChannel.DeleteAsync();
                            games.Remove(game);
                            return;
                    }
                }
                if (game.playMessage == null) return;
                if (!game.win && game.playMessage.Id == reaction.MessageId && (game.p1go && game.player1.Id == reaction.UserId || !game.p1go && game.player2.Id == reaction.UserId)) {
                    game.p1go = !game.p1go;
                    foreach (var slot in game.board.Where(slot => slot == reaction.Emote.Name)) {
                        game.board[game.board.IndexOf(slot)] = game.p1go ? game.p1Symbol : game.p2Symbol;
                        break;
                    }
                    await game.playMessage.ModifyAsync((x) => {
                        x.Embed = new EmbedBuilder() {
                            Title = TicTacToeLocale.TicTacToeEmbed_title,
                            Color = Color.Orange,
                            Fields = new List<EmbedFieldBuilder> {
                                new EmbedFieldBuilder {
                                    Name = TicTacToeLocale.TicTacToeEmbed_fieldName,
                                    Value = game.GenerateGrid()
                                }
                            },
                            Footer = new EmbedFooterBuilder {
                                Text = Main.EmbedFooter
                            }
                        }.Build();
                        x.Content = string.Format(TicTacToeLocale.TicTacToeMessage, game.player1.Mention, game.p1Symbol, game.player2.Mention, game.p2Symbol, (game.p1go ? game.player1.Mention : game.player2.Mention));
                    });
                    await game.playMessage.RemoveAllReactionsForEmoteAsync(reaction.Emote);
                    var winner = game.CheckWin();
                    if (winner != null && winner.Id == game.player1.Id) {
                        await game.playMessage.ModifyAsync((x) => {
                            x.Embed = new EmbedBuilder() {
                                Title = TicTacToeLocale.TicTacToeEmbed_title,
                                Color = Color.Green,
                                Fields = new List<EmbedFieldBuilder> { new EmbedFieldBuilder { Name = TicTacToeLocale.TicTacToeEmbed_fieldName, Value = game.GenerateGrid() } },
                                Footer = new EmbedFooterBuilder { Text = Main.EmbedFooter }
                            }.Build();
                        });
                        await game.playChannel.SendMessageAsync(string.Format(TicTacToeLocale.TicTacToeWin, game.player1.Mention));
                        game.win = true;
                    } else if (winner != null && winner.Id == game.player2.Id) {
                        await game.playMessage.ModifyAsync((x) => {
                            x.Embed = new EmbedBuilder() {
                                Title = TicTacToeLocale.TicTacToeEmbed_title,
                                Color = Color.Green,
                                Fields = new List<EmbedFieldBuilder> { new EmbedFieldBuilder { Name = TicTacToeLocale.TicTacToeEmbed_fieldName, Value = game.GenerateGrid() } },
                                Footer = new EmbedFooterBuilder { Text = Main.EmbedFooter }
                            }.Build();
                        });
                        await game.playChannel.SendMessageAsync(string.Format(TicTacToeLocale.TicTacToeWin, game.player1.Mention));
                        game.win = true;
                    } else if (game.board.All(x => x is "⭕" or "❌")) {
                        await game.playChannel.SendMessageAsync(TicTacToeLocale.TicTacToeDraw);
                        game.win = true;
                    }
                }
                return;
            }
        }

        public class TicTacToe {
            private ShardedInteractionContext ctx;
            public RestTextChannel? playChannel;
            public RestUserMessage? playMessage;
            public RestUserMessage? lobbyMessage;
            public IUser player1;
            public IUser player2;
            public string p1Symbol;
            public string p2Symbol;
            public bool p1Ready;
            public bool p2Ready;
            public bool win;
            public bool p1go;
            public List<string> board;
            public bool lobby;

            public TicTacToe(ShardedInteractionContext _ctx, IUser _player1, IUser _player2, string _p1Symbol, string _p2Symbol) {
                ctx = _ctx;
                player1 = _player1;
                player2 = _player2;
                p1Symbol = _p1Symbol;
                p2Symbol = _p2Symbol;
                win = false;
                p1go = true;
                p1Ready = false;
                p2Ready = false;
                board = new List<string>() {
                    "1️⃣", "2️⃣", "3️⃣",
                    "4️⃣", "5️⃣", "6️⃣",
                    "7️⃣", "8️⃣", "9️⃣"
                };
                playMessage = null;
                playChannel = null;
                lobbyMessage = null;
                lobby = true;
                newChannel(player1, player2);
            }

            private async void newChannel(IUser player1, IUser player2) {
                playChannel = await ctx.Guild.CreateTextChannelAsync("tictactoe", (x) => {
                    x.Topic = TicTacToeLocale.TicTacToeChannelTopic;
                    x.PermissionOverwrites = new List<Overwrite> {
                        new Overwrite(ctx.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
                        new Overwrite(player1.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
                        new Overwrite(player2.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
                    };
                });
                lobbyMessage = await playChannel.SendMessageAsync(TicTacToeLocale.TicTacToeConfirm);
                await lobbyMessage.AddReactionsAsync(new Emoji[] { new Emoji("✅"), new Emoji("❌") });
            }
            public string GenerateGrid() {
                string grid = "⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n";
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) grid += $"⬛⬛⬛{board[i * 3 + j]}";
                    grid += "⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n";
                }
                return grid;
            }
            public IUser? CheckWin() {
                if (
                   board[1] == board[2] && board[2] == board[3] && board[3] == p1Symbol // 1, 2, 3
                || board[4] == board[5] && board[5] == board[6] && board[6] == p1Symbol // 4, 5, 6
                || board[7] == board[8] && board[8] == board[9] && board[9] == p1Symbol // 7, 8, 9
                || board[1] == board[4] && board[4] == board[7] && board[7] == p1Symbol // 1, 4, 7
                || board[2] == board[5] && board[5] == board[8] && board[8] == p1Symbol // 2, 5, 8
                || board[3] == board[6] && board[6] == board[9] && board[9] == p1Symbol // 3, 6, 9
                || board[1] == board[5] && board[5] == board[9] && board[9] == p1Symbol // 1, 5, 9
                || board[3] == board[5] && board[5] == board[7] && board[7] == p1Symbol // 3, 5, 7 
                ) {return player1;}
                else if (
                   board[1] == board[2] && board[2] == board[3] && board[3] == p2Symbol // 1, 2, 3
                || board[4] == board[5] && board[5] == board[6] && board[6] == p2Symbol // 4, 5, 6
                || board[7] == board[8] && board[8] == board[9] && board[9] == p2Symbol // 7, 8, 9
                || board[1] == board[4] && board[4] == board[7] && board[7] == p2Symbol // 1, 4, 7
                || board[2] == board[5] && board[5] == board[8] && board[8] == p2Symbol // 2, 5, 8
                || board[3] == board[6] && board[6] == board[9] && board[9] == p2Symbol // 3, 6, 9
                || board[1] == board[5] && board[5] == board[9] && board[9] == p2Symbol // 1, 5, 9
                || board[3] == board[5] && board[5] == board[7] && board[7] == p2Symbol // 3, 5, 7
                ) {return player2;}
                return null;
            }
        }
    }
}
