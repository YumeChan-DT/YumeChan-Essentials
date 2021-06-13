using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using static YumeChan.Essentials.Utilities;

#pragma warning disable CA1822

namespace YumeChan.Essentials.Chat
{
	public class Invite : BaseCommandModule
	{
		[Command("invite"), Aliases("inv")]
		public async Task InviteCommandAsync(CommandContext context, DiscordMember member)
		{
			DiscordChannel currentChannel = context.Member.VoiceState.Channel;

			if (member is null)
			{
				await context.RespondAsync("Please quote an existing User, or enter a valid username.");
			}
			else if (currentChannel is null)
			{
				await context.RespondAsync("Please connect to a Voice Channel before inviting another user.");
			}
			else
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithAuthor(context.User)
					.WithTitle("Invitation to Voice Channel")
					.WithDescription($"You have been invited by {context.User.Mention} to join a voice channel.")
					.AddField("Server", context.Guild.Name, true);

				if (currentChannel.Parent is DiscordChannel category and not null)
				{
					embed.AddField("Category", category.Name, true); 
				}

				embed.AddField("Channel", currentChannel.Name, true)
					.AddField("Invite Link", $"Use this link for quick access to ``{currentChannel.Name}`` :\n{BuildChannelLink(ChannelLinkTypes.Https, currentChannel)}");

				await member.SendMessageAsync(embed: embed.Build());
				await context.Member.SendMessageAsync($"Sent {member.Mention} an invite to ``{currentChannel.Name}``.");
			}
		}
	}
}
