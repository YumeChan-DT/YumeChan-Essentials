﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nodsoft.YumeChan.Essentials.Network
{
	[Group("ping")]
	public class PingModule : ModuleBase<SocketCommandContext>
	{
		[Command("", RunMode = RunMode.Async)]
		public async Task NetworkPingCommand(string host)
		{

			// 1A. Find out if supplied Hostname or IP
			bool hostIsIP = host.IsIPAddress();

			// 1B. Resolve if necessary
			if (!Resolve.TryResolveHostname(host, out IPAddress hostResolved, out Exception e))
			{
				await ReplyAsync($"{Context.User.Mention}, Hostname ``{host}`` could not be resolved.\nException Thrown : {e.Message}");
				return;
			}

			// 2. Ping the IP
			const int PingCount = 4;
			PingModuleReply[] pingReplies = TcpPing(hostResolved, 80, PingCount).Result;

			// 3. Retrieve statistics			// 4. Return results to user with ReplyAsync(); (Perhaps Embed ?)
			List<long> roundTripTimings = new();

			EmbedBuilder embedBuilder = new()
			{
				Title = "Ping Results",
				Description = $"Results of Ping on **{host}** {(hostIsIP ? ":" : $"({hostResolved}) :")}"
			};

			for (int i = 0; i < PingCount; i++)
			{
				EmbedFieldBuilder field = new() { Name = $"Ping {i}", IsInline = true };
				if (pingReplies[i].Status is IPStatus.Success)
				{
					field.Value = $"RTD = **{pingReplies[i].RoundTripTime}** ms";
					roundTripTimings.Add(pingReplies[i].RoundTripTime);
				}
				else
				{
					field.Value = $"Error : **{pingReplies[i].Status}**";
				}

				embedBuilder.AddField(field);
			}

			embedBuilder.AddField("Average RTD", roundTripTimings.Any()
				? $"Average Round-Trip Time/Delay = **{roundTripTimings.Average()}** ms / **{roundTripTimings.Count}** packets"
				: $"No RTD Average Assertable : No packets returned from Pings.");

			await ReplyAsync(message: Context.User.Mention, embed: embedBuilder.Build()); //Quote user in main message, and attach Embed.
		}

		internal static async Task<PingModuleReply[]> ComplexPing(IPAddress host, int count) => await ComplexPing(host, count, 2000, new(64, true));
		internal static async Task<PingModuleReply[]> ComplexPing(IPAddress host, int count, int timeout, PingOptions options)
		{
			PingModuleReply[] pingReplies = new PingModuleReply[count];

			// Create a buffer of 32 bytes of data to be transmitted.
			byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

			// Send the request.
			for (int i = 0; i < count; i++)
			{
				Ping ping = new();
				pingReplies[i] = new PingModuleReply(await ping.SendPingAsync(host, timeout, buffer, options).ConfigureAwait(false));
				ping.Dispose();
			}

			return pingReplies;
		}

		// See : https://stackoverflow.com/questions/26067342/how-to-implement-psping-tcp-ping-in-c-sharp
		internal static Task<PingModuleReply[]> TcpPing(IPAddress host, int port, int count)
		{
			PingModuleReply[] pingReplies = new PingModuleReply[count];
			for (int i = 0; i < count; i++)
			{
				using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = true };

				Stopwatch latencyMeasurement = new();
				IPStatus? status = null;

				try
				{
					latencyMeasurement.Start();
					socket.Connect(host, port);
					latencyMeasurement.Stop();
				}
				catch
				{
					status = IPStatus.TimedOut;
				}

				pingReplies[i] = new PingModuleReply(host, latencyMeasurement.ElapsedMilliseconds, status ?? IPStatus.Success);
			}

			return Task.FromResult(pingReplies);
		}


		internal record PingModuleReply(IPAddress Host, long RoundTripTime, IPStatus Status)
		{
			public PingModuleReply(PingReply reply) : this(reply.Address, reply.RoundtripTime, reply.Status) { }
		}
	}
}
