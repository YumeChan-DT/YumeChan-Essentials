using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1822

namespace YumeChan.Essentials.Network
{
	public partial class NetworkTopModule
	{
		[SlashCommand("resolve", "Resolves the IP address behind a given domain name.")]
		public async Task ResolveCommand(InteractionContext context,
			[Option("Host", "Host to attempt resolution on.")] string host)
		{
			await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

			if (host.IsIPAddress())
			{
				await context.FollowUpAsync($"Isn't ``{host}`` already an IP address ?", true);
			}
			else
			{
				await context.FollowUpAsync(TryResolveHostname(host, out string hostResolved, out Exception e)
					? $"Hostname ``{host}`` resolves to IP Address ``{hostResolved}``."
					: $"Hostname ``{host}`` could not be resolved. \nException Thrown : {e.Message}"
				);
			}
		}

		public static async Task<IPAddress> ResolveHostnameAsync(string hostname)
		{
			IPAddress[] a = await Dns.GetHostAddressesAsync(hostname);
			return a.FirstOrDefault();
		}

		public static bool TryResolveHostname(string hostname, out string resolved, out Exception exception)
		{
			bool tryResult = TryResolveHostname(hostname, out IPAddress resolvedIp, out exception);
			resolved = resolvedIp.ToString();
			return tryResult;
		}
		public static bool TryResolveHostname(string hostname, out IPAddress resolved, out Exception exception)
		{
			IPAddress[] a;

			try
			{
				a = Dns.GetHostAddresses(hostname);
				resolved = a.FirstOrDefault();
				exception = null;
				return true;
			}
			catch (Exception e)
			{
				resolved = null;
				exception = e;
				return false;
			}
		}
	}
}
