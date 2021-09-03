using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using static YumeChan.Essentials.Utilities;

#pragma warning disable CA1822

namespace YumeChan.Essentials.Chat
{
	public class Invite : ApplicationCommandModule
	{
		[ContextMenu(ApplicationCommandType.UserContextMenu, "Invite")]
		public async Task InviteCommandAsync(ContextMenuContext context)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { IsEphemeral = true });

			if (context.Member.VoiceState?.Channel is DiscordChannel currentChannel)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithAuthor(context.User)
					.WithTitle("Invitation to Voice Channel")
					.WithDescription($"You have been invited by {context.User.Mention} to join a voice channel.")
					.AddField("Server", context.Guild.Name, true);

				if (currentChannel.Parent is DiscordChannel category)
				{
					embed.AddField("Category", category.Name, true);
				}

				embed.AddField("Channel", currentChannel.Mention, true);

				await context.TargetMember.SendMessageAsync(embed: embed.Build());

				await context.FollowUpAsync(new()
				{
					Content = $"Sent {context.TargetMember.Mention} an invite to {currentChannel.Mention}.",
					IsEphemeral = true
				});
			}
			else
			{
				await context.FollowUpAsync(new()
				{
					Content = "Please connect to a Voice Channel before inviting another user.",
					IsEphemeral = true
				});
			}
		}
	}
}
