using System.Net;

namespace Nodsoft.YumeChan.Essentials.Network
{
	static class Utils
	{
		internal static bool IsIPAddress(this string address)
		{
			return IPAddress.TryParse(address, out _);
		}
	}
}