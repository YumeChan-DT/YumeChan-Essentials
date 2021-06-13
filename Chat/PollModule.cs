using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.Essentials.Chat
{
	[Group("poll")]
	public class PollModule : BaseCommandModule
	{
		public List<Poll> DraftPolls { get; internal set; } = new List<Poll>();
//		public List<Poll> CurrentPolls { get; internal set; } = new List<Poll>();


		[Command("init")]
		public async Task InitPollAsync(CommandContext context)
		{
			DiscordMessage reply;

			if (QueryUserPoll(context.User, DraftPolls) is Poll poll)
			{
				reply = await context.RespondAsync("Resetting existing Poll...");
				DraftPolls.Find(x => x == poll).Reset();
			}
			else
			{
				reply = await context.RespondAsync("Creating a new Poll...");
				DraftPolls.Add(new(context.User));
			}

			await reply.ModifyAsync(msg => msg.Content += " Done !");
		}

		[Command("name")]
		public async Task SetPollNameAsync(CommandContext context, [RemainingText] string name)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.Name = name;
				await Utilities.MarkCommandAsCompleted(context);
			}
		}
		[Command("notice")]
		public async Task SetPollNoticeAsync(CommandContext context, [RemainingText] string notice)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.Notice = notice;
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		[Command("addoption")]
		public async Task AddPollOptionAsync(CommandContext context, DiscordEmoji reactionEmote, [RemainingText] string description)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.VoteOptions.Add(new PollVoteOption { ReactionEmote = reactionEmote, Description = description });
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		//[Command("setoption"), Priority(1)]
		public async Task SetPollOptionAsync(CommandContext context, byte index, DiscordEmoji reactionEmote, [RemainingText] string description)
		{
			if (await GetUserPollAsync(context) is Poll poll && !await Poll.VoteOptionsIndexIsOutsideRange(index, context))
			{
				poll.VoteOptions[index] = new PollVoteOption { ReactionEmote = reactionEmote, Description = description };
				await Utilities.MarkCommandAsCompleted(context);
			}
		}
		//[Command("setoption")]
		public async Task SetPollOptionAsync(CommandContext context, DiscordEmoji reactionEmote, [RemainingText] string description)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.VoteOptions.Find(x => x.ReactionEmote == reactionEmote).Description = description;
				await Utilities.MarkCommandAsCompleted(context);
			}
		}
		[Command("removeoption"), Priority(1)]
		public async Task RemovePollOptionAsync(CommandContext context, byte index)
		{
			if (await GetUserPollAsync(context) is Poll poll && !await Poll.VoteOptionsIndexIsOutsideRange(index, context))
			{
				poll.VoteOptions.RemoveAt(index);
				await Utilities.MarkCommandAsCompleted(context);
			}
		}
		[Command("removeoption")]
		public async Task RemovePollOptionAsync(CommandContext context, DiscordEmoji emote)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				try
				{
					poll.VoteOptions.Remove(poll.VoteOptions.First(option => option.ReactionEmote == emote));
					await Utilities.MarkCommandAsCompleted(context);
				}
				catch (System.InvalidOperationException)
				{
					await Utilities.MarkCommandAsFailed(context);
				}
			}
		}
		[Command("clearoptions")]
		public async Task ClearPollOptionsAsync(CommandContext context)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.VoteOptions.Clear();
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		[Command("previewoptions")]
		public async Task PreviewOptionsAsync(CommandContext context)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				if (poll.VoteOptions.Count is 0)
				{
					await context.RespondAsync($"**No Vote Options registered.** Be sure to add some before attempting to Preview.");
				}
				else
				{
					StringBuilder previewBuilder = new($" Previewing Vote Options on **{poll.Name ?? "Unnamed Poll"}** :\n\n");
					byte index = 0;

					foreach (PollVoteOption option in poll.VoteOptions)
					{
						index++;
						previewBuilder.AppendLine($"**{index} :** {option.ReactionEmote} - {option.Description}");
					}

					await context.RespondAsync(previewBuilder.ToString());
				}
			}
		}

		[Command("previewpoll")]
		public async Task PreviewPollAsync(CommandContext context)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				DiscordMessage message = await context.RespondAsync(BuildPollMessage(poll));
				await AddPollReactionsAsync(poll, message);
			}
		}
		[Command("publish"), RequireGuild]
		public async Task PublishPollAsync(CommandContext context)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.PublishedPollMessage = await context.Channel.SendMessageAsync(BuildPollMessage(poll));
				await AddPollReactionsAsync(poll, poll.PublishedPollMessage);
				await context.Member.SendMessageAsync($"Published Poll ``{poll.Name}`` in channel ``{poll.PublishedPollMessage.Channel.Name}``.");
				await context.Message.DeleteAsync();

				// CurrentPolls.Add(poll);
				DraftPolls.Remove(poll);
			}
		}

		public static Poll QueryUserPoll(DiscordUser user, List<Poll> list) => list.FirstOrDefault(poll => poll.Author.Id == user.Id);

		protected static DiscordEmbed BuildPollMessage(Poll poll)
		{
			DiscordEmbedBuilder embed = new()
			{
				Title = "**Poll**",
				Description = poll.Name ?? "No Description.",
			};

			embed.WithAuthor(poll.Author);

			if (!string.IsNullOrWhiteSpace(poll.Notice))
			{
				embed.WithFooter(poll.Notice);
			}

			foreach (PollVoteOption option in poll.VoteOptions)
			{
				embed.AddField(option.Description, option.ReactionEmote, true);
			}

			return embed.Build();
		}

		protected static async Task AddPollReactionsAsync(Poll poll, DiscordMessage message)
		{
			foreach (DiscordEmoji emoji in poll.VoteOptions.Select(x => x.ReactionEmote))
			{
				await poll.PublishedPollMessage.CreateReactionAsync(emoji);
			}
		}

		private async Task<Poll> GetUserPollAsync(CommandContext context)
		{
			Poll poll = QueryUserPoll(context.User, DraftPolls);

			if (poll is null)
			{
				await context.RespondAsync("Cannot perform action, no Poll was found.\nYou may initialize a Poll by typing ``==poll init``.");
			}
			return poll;
		}
	}

	public class Poll
	{
		public DiscordUser Author { get; private set; }

		public string Name { get; set; }
		public string Notice { get; set; }

		public List<PollVoteOption> VoteOptions { get; set; } = new(20);

		public DiscordMessage PublishedPollMessage { get; internal set; }

		public Poll(DiscordUser author) => Author = author;

		public void Reset()
		{
			Name = null;
			Notice = null;
			VoteOptions = new(20);
			PublishedPollMessage = null;
		}

		public static async Task<bool> VoteOptionsIndexIsOutsideRange(byte index, CommandContext context)
		{
			switch (index)
			{
				case > 20:
					await context.Channel.SendMessageAsync($"{context.User.Mention} You have entered an index greater than 20. Please note that Discord only authorizes up to 20 reaction types per message.");
					return true;

				default:
					return false;
			}
		}
	}

	public class PollVoteOption
	{
		public DiscordEmoji ReactionEmote { get; set; }

		public string Description { get; set; }

		// public List<IGuildUser> Voters { get; internal set; }
	}
}
