using DayZObfuscatorModel.PBO.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Packer
{
	public abstract class PBOPackerComponent
	{
		/// <summary>
		/// Resets the state of this component to get it ready for the next packing operation.
		/// Should reset internal state, not configuration.
		/// </summary>
		public abstract void ResetState();

		/// <summary>
		/// Sets properties which will be added to the properties header.
		/// This method is allowed to access <see cref="Pbo"/> but should not mutate it.
		/// </summary>
		/// <param name="properties">Properties dictionary which should be filled by the method. Properties may be overriden by this method.</param>
		public abstract void SetProperties(IDictionary<string, string> properties);

		/// <summary>
		/// Applies changes to PBO's config
		/// </summary>
		/// <param name="config">The config to modify.</param>
		public abstract void ProcessConfig(PBOConfig config);

		/// <summary>
		/// Applies changes to a file
		/// </summary>
		/// <param name="file"></param>
		public abstract void ProcessFile(PBOFile file);
	}
}
