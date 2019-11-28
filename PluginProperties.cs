using Nodsoft.YumeChan.PluginBase;
using System;
using System.Threading.Tasks;

namespace Nodsoft.YumeChan.Essentials
{
	public class PluginProperties : Plugin
	{
		public override string PluginDisplayName { get; } = "Yume-Chan Essentials";

		public override bool PluginStealth { get; } = false;

		public override async Task LoadPlugin()
		{
			await base.LoadPlugin();
		}

		public override async Task UnloadPlugin()
		{
			await base.UnloadPlugin();
		}
	}
}
