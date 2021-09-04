using System.Net;

namespace YumeChan.Essentials.Network
{
	static class Utils
	{
		internal static bool IsIPAddress(this string address) => IPAddress.TryParse(address, out _);
	}
}