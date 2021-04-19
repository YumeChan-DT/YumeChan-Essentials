using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nodsoft.YumeChan.Essentials
{
	public static class Utilities
	{
		public static DiscordEmoji GreenCheckEmoji { get; } = DiscordEmoji.FromUnicode("\u2705");
		public static DiscordEmoji GreenCrossEmoji { get; } = DiscordEmoji.FromUnicode("\u274e");

		public static DiscordChannel FindUserCurrentVoiceChannel(CommandContext context) =>
			(from channel in context.Guild.Channels?.Values
			where channel.Type is ChannelType.Voice
			where channel.Users.Contains(context.Member)
			select channel).FirstOrDefault();

		public static DiscordEmoji FindEmote(CommandContext context, string emoteName)
		{
			return context.Guild.Emojis?.Values.FirstOrDefault(x => x.Name.IndexOf(emoteName, StringComparison.OrdinalIgnoreCase) != -1);
		}

		public static async Task MarkCommandAsCompleted(CommandContext context) => await context.Message.CreateReactionAsync(GreenCheckEmoji);
		public static async Task MarkCommandAsFailed(CommandContext context) => await context.Message.CreateReactionAsync(GreenCrossEmoji);

		public static string BuildChannelLink(ChannelLinkTypes type, DiscordChannel channel)
		{
			string linkType = type switch
			{
				ChannelLinkTypes.Discord => "discord",
				ChannelLinkTypes.Https => "https",
				_ => throw new Exception()
			};

			return $"{linkType}://discordapp.com/channels/{channel.GuildId}/{channel.Id}";
		}

		public enum ChannelLinkTypes
		{
			Discord = 0,
			Https = 1
		}

		public static DiscordEmbedBuilder WithAuthor(this DiscordEmbedBuilder embed, DiscordUser user)
		{
			return embed.WithAuthor(user.GetFullUsername(), null, user.GetAvatarUrl(ImageFormat.Auto, 128));
		}

		public static string GetFullUsername(this DiscordUser user) => $"{user.Username}#{user.Discriminator}";
	}
}
