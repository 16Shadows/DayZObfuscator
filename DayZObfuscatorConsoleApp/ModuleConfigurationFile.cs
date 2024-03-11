using Newtonsoft.Json;

namespace DayZObfuscatorConsoleApp
{
	internal class ModuleConfigurationFile
	{
		[JsonProperty("modules", Required = Required.Always)]
		public List<ModuleConfiguration> Modules { get; }

		[JsonConstructor]
		public ModuleConfigurationFile(List<ModuleConfiguration> modules)
		{
			Modules = modules ?? throw new ArgumentNullException(nameof(modules));
		}
	}
}
