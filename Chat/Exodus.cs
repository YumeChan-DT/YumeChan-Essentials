using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Nodsoft.YumeChan.Essentials.Chat
{
	[Group("exodus"), RequireBotPermission(GuildPermission.MoveMembers)]
	public class Exodus : ModuleBase<SocketCommandContext>
	{
		[Command("from", RunMode = RunMode.Async), RequireOwner, RequireUserPermission(GuildPermission.MoveMembers)]
		public async Task FromChannelAsync(ulong channelId)
		{
			if ((Context.User as IGuildUser).VoiceChannel is IVoiceChannel dest and not null)
			{
				if (Context.Guild.GetVoiceChannel(channelId) is IVoiceChannel source and not null)
				{
					if ((source as SocketVoiceChannel).Users.Count is 0)
					{
						await ReplyAsync(EmptyChannelMessage).ConfigureAwait(false);
					}

					await ExodeAsync(source, dest);
				}
				else
				{
					await ReplyAsync(GetInvalidChannelMessage(channelId)).ConfigureAwait(false);
					return;
				}
			}
			else
			{
				await ReplyAsync(NotVoiceConnectedMessage).ConfigureAwait(false);
				return;
			}
		}

		[Command("to", RunMode = RunMode.Async), RequireOwner, RequireUserPermission(GuildPermission.MoveMembers)]
		public async Task ToChannelAsync(ulong channelId)
		{
			if (Context.Guild.GetVoiceChannel(channelId) is IVoiceChannel dest and not null)
			{
				if ((Context.User as IGuildUser).VoiceChannel is IVoiceChannel source and not null)
				{
					await ExodeAsync(source, dest);
				}
				else
				{
					await ReplyAsync(GetInvalidChannelMessage(channelId)).ConfigureAwait(false);
					return;
				}
			}
			else
			{
				await ReplyAsync(NotVoiceConnectedMessage).ConfigureAwait(false);
				return;
			}
		}


		internal async Task ExodeAsync(IVoiceChannel source, IVoiceChannel dest)
		{
			IReadOnlyCollection<SocketGuildUser> users = (source as SocketVoiceChannel).Users;

			if (!await CheckChannelCapacityAsync(dest, users.Count).ConfigureAwait(false))
			{
				return;
			}

			Dictionary<IGuildUser, Exception> erroredUsers = new();

			await foreach (SocketGuildUser user in users.ToAsyncEnumerable().ConfigureAwait(false))
			{
				try
				{
					await user.ModifyAsync(user => user.Channel = new(dest)).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					erroredUsers.Add(user, e);
				}
			}

			if (erroredUsers.Count is not 0)
			{
				StringBuilder builder = new($"**The following user{(erroredUsers.Count is 1 ? " was" : "s were")} unable to be moved :** \n");
				foreach (KeyValuePair<IGuildUser, Exception> erroredUser in erroredUsers)
				{
					builder.AppendLine($" - {erroredUser.Key.Mention} - Reason : ``{erroredUser.Value.Message ?? "Unknown"}`` {(erroredUser.Value.Source is null ? null : $"(Source : ``{erroredUser.Value.Source}``)")} ");
				}
				builder.AppendLine($"\n{(erroredUsers.Count is 1 ? " This user" : "These users")} may have to be moved manually.");

				await ReplyAsync(builder.ToString());
			}
			else
			{
				await ReplyAsync($"{Context.User.Mention} All **{users.Count}** user{(users.Count is 1 ? null : "s")} successfully moved to ``{dest.Name}``.");
			}
		}




		private async Task<bool> CheckChannelCapacityAsync(IVoiceChannel channel, int loadCount)
		{
			int currentCount = await channel.GetUsersAsync().CountAsync().ConfigureAwait(false);

			if (channel.UserLimit is not null && loadCount > channel.UserLimit - currentCount)
			{
				await ReplyAsync($"{Context.User.Mention} Cannot move ; Channel ``{channel.Name}`` does not have enough user slots. : \n" +
				$"(**{loadCount}** user{(loadCount is 1 ? null : "s")} to move, **{currentCount}** user{(currentCount is 1 ? null : "s")} currently in the channel.) \n" +
				$"Channel has a limit of **{channel.UserLimit}**, and would be over-capacity by **{loadCount + currentCount - channel.UserLimit}**.").ConfigureAwait(false);
				
				return false;
			}

			return true;
		}


		private string EmptyChannelMessage => $"{Context.User.Mention} Channel is empty, nobody's moving.";
		private string NotVoiceConnectedMessage => $"{Context.User.Mention} Please connect to a voice channel, then retry.";
		private string GetInvalidChannelMessage(ulong id) => $"{Context.User.Mention} Invalid channel : ``{Context.Guild.GetVoiceChannel(id)?.Name ?? id.ToString()}``.";
	}
}
