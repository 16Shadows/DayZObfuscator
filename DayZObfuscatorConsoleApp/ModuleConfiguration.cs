using DayZObfuscatorModel.PBO.Packer;
using Newtonsoft.Json;

namespace DayZObfuscatorConsoleApp
{
	internal class ModuleConfiguration
	{
		[JsonConstructor]
		public ModuleConfiguration(string assemblyPath, string moduleName, Dictionary<string, object>? properties)
		{
			AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
			ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
			Properties = properties;
		}

		[JsonProperty("assembly", Required = Required.Always)]
		public string AssemblyPath { get; }
		[JsonProperty("module", Required = Required.Always)]
		public string ModuleName { get; }
		[JsonProperty("properties", Required = Required.Default)]
		public Dictionary<string, object>? Properties { get; }
	}
}
