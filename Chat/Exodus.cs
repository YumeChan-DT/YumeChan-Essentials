using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1822


namespace YumeChan.Essentials.Chat
{
	[SlashCommandGroup("exodus", "Provides commands for moving users from one channel to another.")]
	[SlashRequireGuild, SlashRequirePermissions(Permissions.MoveMembers)]
	public class Exodus : ApplicationCommandModule
	{
		[SlashCommand("from", "Moves all users from a specified voice channel.")]
		public async Task FromChannelAsync(InteractionContext context,
			[Option("Channel", "Voice channel to move users from")] DiscordChannel source)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

			if (context.Member.VoiceState.Channel is DiscordChannel dest)
			{
				if (source.Type is ChannelType.Voice)
				{
					if (source.Users.Count() is 0)
					{
						await context.FollowUpAsync(EmptyChannelMessage, true);
					}
					else
					{
						await ExodeAsync(context, source, dest);
					}
				}
				else
				{
					await context.FollowUpAsync(InvalidChannelMessage, true);
				}
			}
			else
			{
				await context.FollowUpAsync(NotVoiceConnectedMessage, true);
			}
		}

		[SlashCommand("to", "Moves all users to a specified voice channel.")]
		public async Task ToChannelAsync(InteractionContext context,
			[Option("channel", "Voice channel to move users to")] DiscordChannel dest)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

			if (dest.Type is ChannelType.Voice)
			{
				if (context.Member.VoiceState.Channel is DiscordChannel source)
				{
					await ExodeAsync(context, source, dest);
				}
				else
				{
					await context.FollowUpAsync(InvalidChannelMessage, true);
				}
			}
			else
			{
				await context.FollowUpAsync(NotVoiceConnectedMessage, true);
			}
		}


		public static async Task ExodeAsync(InteractionContext context, DiscordChannel source, DiscordChannel dest)
		{
			int loadCount = source.Users.Count();

			if (!await CheckChannelCapacityAsync(context, dest, loadCount).ConfigureAwait(false))
			{
				return;
			}

			Dictionary<DiscordMember, Exception> erroredUsers = new();

			foreach (DiscordMember user in source.Users)
			{
				try
				{
					await user.ModifyAsync(user => user.VoiceChannel = dest).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					erroredUsers.Add(user, e);
				}
			}

			if (erroredUsers.Count is not 0)
			{
				StringBuilder builder = new($"**The following user{(erroredUsers.Count is 1 ? " was" : "s were")} unable to be moved :** \n");

				foreach (KeyValuePair<DiscordMember, Exception> erroredUser in erroredUsers)
				{
					builder.AppendFormat(" - {0} - Reason : ``{1}`` {2} \n", erroredUser.Key.Mention, erroredUser.Value.Message ?? "Unknown", erroredUser.Value.Source is null ? null : $"(Source : ``{erroredUser.Value.Source}``)");
				}

				builder.AppendFormat("\n{0} may have to be moved manually.", erroredUsers.Count is 1 ? " This user" : "These users");

				await context.FollowUpAsync(builder.ToString());
			}
			else
			{
				await context.FollowUpAsync($"All **{loadCount}** user{(loadCount is 1 ? null : "s")} successfully moved to ``{dest.Name}``.");
			}
		}




		private static async Task<bool> CheckChannelCapacityAsync(InteractionContext context, DiscordChannel channel, int loadCount)
		{
			int currentCount = channel.Users.Count();

			if (channel.UserLimit is not 0 && loadCount > channel.UserLimit - currentCount)
			{
				StringBuilder stringBuilder = new StringBuilder()
					.AppendFormat("Cannot move ; Destination channel ``{0}`` does not have enough user slots. : \n", channel.Name)
					.AppendFormat("(**{0}** user{1} to move, **{2}** user{3} currently in the channel.) \n", loadCount, loadCount is 1 ? null : "s", currentCount, currentCount is 1 ? null : "s")
					.AppendFormat("Channel has a limit of **{0}**, and would be over-capacity by **{1}**.", channel.UserLimit, loadCount + currentCount - channel.UserLimit);

				await context.FollowUpAsync(stringBuilder.ToString(), true);

				return false;
			}

			return true;
		}


		private const string EmptyChannelMessage = "Source Channel is empty, nobody's moving.";
		private const string NotVoiceConnectedMessage = "Please connect to a voice channel, then retry.";
		private const string InvalidChannelMessage = "Invalid voice channel specified.";
	}
}
