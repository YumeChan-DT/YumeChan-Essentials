using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.Essentials.Chat
{
	[SlashCommandGroup("poll", "Provides commands for Poll creation."), SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public class PollModule : ApplicationCommandModule
	{
		public List<Poll> DraftPolls { get; internal set; } = new();
//		public List<Poll> CurrentPolls { get; internal set; } = new List<Poll>();


		[SlashCommand("init", "Initializes a new poll draft, or resets current one.")]
		public async Task InitPollAsync(InteractionContext context)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (GetUserPoll(context.User, DraftPolls) is Poll poll)
			{
				DraftPolls.Find(x => x == poll).Reset();
				await context.FollowUpAsync(new() { Content = "Resetted existing Poll." });
			}
			else
			{
				await context.FollowUpAsync(new() { Content = "Created new Poll." });
				DraftPolls.Add(new(context.User));
			}
		}

		[SlashCommand("name", "Sets poll name.")]
		public async Task SetPollNameAsync(InteractionContext context,
			[Option("Name", "New name to set for this poll")] string name)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.Name = name;
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		[SlashCommand("notice", "Sets poll Notice (footer).")]
		public async Task SetPollNoticeAsync(InteractionContext context,
			[Option("Notice", "Poll notice to set")] string notice)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.Notice = notice;
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		[SlashCommand("add-option", "Add a new poll option.")]
		public async Task AddPollOptionAsync(InteractionContext context,
			[Option("emoji", "Emoji to attach poll option to")] DiscordEmoji reactionEmote,
			[Option("description", "Description to set for poll option")] string description)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.VoteOptions.Add(new PollVoteOption { ReactionEmote = reactionEmote, Description = description });
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

/*		//[Command("setoption"), Priority(1)]
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
*/

		[SlashCommand("remove-option", "Removes poll option for specified Emoji.")]
		public async Task RemovePollOptionAsync(InteractionContext context,
			[Option("emoji", "Emoji to remove poll option from")] DiscordEmoji emote)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				try
				{
					poll.VoteOptions.Remove(poll.VoteOptions.First(option => option.ReactionEmote == emote));
					await Utilities.MarkCommandAsCompleted(context);
				}
				catch (InvalidOperationException)
				{
					return;
				}
			}
		}
		[SlashCommand("clear-options", "Clears all poll options.")]
		public async Task ClearPollOptionsAsync(InteractionContext context)
		{
			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.VoteOptions.Clear();
				await Utilities.MarkCommandAsCompleted(context);
			}
		}

		[SlashCommand("preview-options", "Lists all poll options currently set.")]
		public async Task PreviewOptionsAsync(InteractionContext context)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				if (poll.VoteOptions.Count is 0)
				{
					await context.FollowUpAsync(new() { Content = $"**No Vote Options registered.** Be sure to add some before attempting to Preview." });
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

					await context.FollowUpAsync(new() { Content = previewBuilder.ToString() });
				}
			}
		}

		[SlashCommand("preview-poll", "Previews a draft poll.")]
		public async Task PreviewPollAsync(InteractionContext context)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				DiscordMessage message = await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(BuildPollMessage(poll)));
				await AddPollReactionsAsync(poll, message);
			}
		}
		[SlashCommand("publish", "Publishes a poll."), SlashRequireGuild, RequirePermissions(Permissions.SendMessages)]
		public async Task PublishPollAsync(InteractionContext context)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (await GetUserPollAsync(context) is Poll poll)
			{
				poll.PublishedPollMessage = await context.Channel.SendMessageAsync(BuildPollMessage(poll));
				await AddPollReactionsAsync(poll, poll.PublishedPollMessage);
				await context.FollowUpAsync(new() { Content = $"Published Poll ``{poll.Name}`` in channel ``{poll.PublishedPollMessage.Channel.Name}``." });

				// CurrentPolls.Add(poll);
				DraftPolls.Remove(poll);
			}
		}

		public static Poll GetUserPoll(DiscordUser user, List<Poll> list) => list.FirstOrDefault(poll => poll.Author.Id == user.Id);

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

		private async Task<Poll> GetUserPollAsync(InteractionContext context)
		{
			Poll poll = GetUserPoll(context.User, DraftPolls);

			if (poll is null)
			{
				try
				{
					await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });
				}
				catch (NotFoundException) { }

				await context.FollowUpAsync(new() {	Content = "Cannot perform action, no Poll was found.\nYou may initialize a Poll by typing ``/poll init``.", IsEphemeral = true });
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
