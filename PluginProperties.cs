using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;
using System;
using System.Threading.Tasks;

namespace YumeChan.Essentials
{
	public class PluginProperties : Plugin
	{
		private readonly ILogger<PluginProperties> logger;	

		public PluginProperties(ILogger<PluginProperties> logger)
		{
			this.logger = logger;
		}

		public override string PluginDisplayName { get; } = "Yume-Chan Essentials";

		public override bool PluginStealth { get; } = false;


		public override async Task LoadPlugin()
		{
			await base.LoadPlugin();
			logger.LogInformation("Ready.");
		}

		public override async Task UnloadPlugin()
		{
			logger.LogInformation("Unloading...");
			await base.UnloadPlugin();
		}
	}
}
