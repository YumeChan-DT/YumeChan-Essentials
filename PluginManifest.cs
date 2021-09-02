using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;
using System;
using System.Threading.Tasks;

namespace YumeChan.Essentials
{
	public class PluginManifest : Plugin
	{
		private readonly ILogger<PluginManifest> logger;

		public PluginManifest(ILogger<PluginManifest> logger)
		{
			this.logger = logger;
		}

		public override string DisplayName { get; } = "Yume-Chan Essentials";

		public override bool StealthMode { get; } = false;


		public override async Task LoadAsync()
		{
			await base.LoadAsync();
			logger.LogInformation("Ready.");
		}

		public override async Task UnloadAsync()
		{
			logger.LogInformation("Unloading...");
			await base.UnloadAsync();
		}
	}
}
