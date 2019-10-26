using Nodsoft.YumeChan.PluginBase;
using System;
using System.Threading.Tasks;

namespace Nodsoft.YumeChan.Essentials
{
	public class PluginProperties : IPlugin
	{
		public Version PluginVersion { get; } = typeof(PluginProperties).Assembly.GetName().Version;

		public string PluginDisplayName { get; } = "Yume-Chan Essentials";

		public bool PluginStealth { get; } = false;

		public bool PluginLoaded { get; internal set; }

		public Task LoadPlugin()
		{
			PluginLoaded = true;
			return Task.CompletedTask;
		}

		public Task UnloadPlugin()
		{
			PluginLoaded = false;
			return Task.CompletedTask;
		}
	}
}
