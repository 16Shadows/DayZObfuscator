using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
