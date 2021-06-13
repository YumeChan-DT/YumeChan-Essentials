using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1822


namespace YumeChan.Essentials.Chat
{
	[Group("exodus"), RequireGuild, RequireBotPermissions(Permissions.MoveMembers)]
	public class Exodus : BaseCommandModule
	{
		[Command("from"), RequireUserPermissions(Permissions.MoveMembers)]
		public async Task FromChannelAsync(CommandContext context, ulong channelId)
		{
			if (context.Member.VoiceState.Channel is DiscordChannel dest)
			{
				if (context.Guild.Channels.GetValueOrDefault(channelId) is DiscordChannel source)
				{
					if (source.Users.Count() is 0)
					{
						await context.RespondAsync(EmptyChannelMessage);
					}
					else
					{
						await ExodeAsync(context, source, dest);
					}
				}
				else
				{
					await context.RespondAsync(InvalidChannelMessage);
				}
			}
			else
			{
				await context.RespondAsync(NotVoiceConnectedMessage);
			}
		}

		[Command("to"), RequireUserPermissions(Permissions.MoveMembers)]
		public async Task ToChannelAsync(CommandContext context, ulong channelId)
		{
			if (context.Guild.GetChannel(channelId) is DiscordChannel dest)
			{
				if (context.Member.VoiceState.Channel is DiscordChannel source)
				{
					await ExodeAsync(context, source, dest);
				}
				else
				{
					await context.RespondAsync(InvalidChannelMessage);
				}
			}
			else
			{
				await context.RespondAsync(NotVoiceConnectedMessage);
			}
		}


		public static async Task ExodeAsync(CommandContext context, DiscordChannel source, DiscordChannel dest)
		{
			if (source.Type is not ChannelType.Voice || dest.Type is not ChannelType.Voice)
			{
				throw new ArgumentException("Invalid Channel(s) specified.");
			}
		
			IEnumerable<DiscordMember> members = source.Users;

			if (!await CheckChannelCapacityAsync(context, dest, members.Count()).ConfigureAwait(false))
			{
				return;
			}

			Dictionary<DiscordMember, Exception> erroredUsers = new();

			foreach (DiscordMember user in members)
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

				await context.RespondAsync(builder.ToString());
			}
			else
			{
				await context.RespondAsync($"All **{members.Count()}** user{(members.Count() is 1 ? null : "s")} successfully moved to ``{dest.Name}``.");
			}
		}




		private static async Task<bool> CheckChannelCapacityAsync(CommandContext context, DiscordChannel channel, int loadCount)
		{
			int currentCount = channel.Users.Count();

			if (channel.UserLimit is not 0 && loadCount > channel.UserLimit - currentCount)
			{
				StringBuilder stringBuilder = new StringBuilder()
					.AppendFormat("Cannot move ; Destination channel ``{0}`` does not have enough user slots. : \n", channel.Name)
					.AppendFormat("(**{0}** user{1} to move, **{2}** user{3} currently in the channel.) \n", loadCount, loadCount is 1 ? null : "s", currentCount, currentCount is 1 ? null : "s")
					.AppendFormat("Channel has a limit of **{0}**, and would be over-capacity by **{1}**.", channel.UserLimit, loadCount + currentCount - channel.UserLimit);

				await context.RespondAsync(stringBuilder.ToString()).ConfigureAwait(false);
				
				return false;
			}

			return true;
		}


		private const string EmptyChannelMessage = "Source Channel is empty, nobody's moving.";
		private const string NotVoiceConnectedMessage = "Please connect to a voice channel, then retry.";
		private const string InvalidChannelMessage = "Invalid voice channel specified.";
	}
}
