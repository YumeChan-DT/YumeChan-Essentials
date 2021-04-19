using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nodsoft.YumeChan.Essentials.Chat
{
	public static class Utils
	{
		public static Emoji GreenCheckEmoji { get; } = new("\u2705");
		public static Emoji GreenCrossEmoji { get; } = new("\u274e");

		public static IVoiceChannel FindUserCurrentVoiceChannel(SocketCommandContext context) => (context.User as IGuildUser)?.VoiceChannel;

		public static IEmote FindEmote(SocketCommandContext context, string emoteName)
		{
			return context.Guild.Emotes.FirstOrDefault(x => x.Name.IndexOf(emoteName, StringComparison.OrdinalIgnoreCase) != -1);
		}

		public static async Task MarkCommandAsCompleted(SocketCommandContext context) => await context.Message.AddReactionAsync(GreenCheckEmoji);
		public static async Task MarkCommandAsFailed(SocketCommandContext context) => await context.Message.AddReactionAsync(GreenCrossEmoji);

		public static string BuildChannelLink(ChannelLinkTypes type, IVoiceChannel channel)
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
	}
}
