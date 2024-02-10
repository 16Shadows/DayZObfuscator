using DayZObfuscatorModel.PBO.Config;

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
		/// <param name="infoProvider">
		/// A provider which can be used to access finalized data from previous steps (after all components are done).
		/// Mutating the data won't have any effect on the PBO but may cause issues in other components.
		/// </param>
		public abstract void ProcessConfig(PBOConfig config, PBOPacker.InfoProvider infoProvider);

		/// <summary>
		/// Applies changes to a file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="infoProvider">
		/// A provider which can be used to access finalized data from previous steps (after all components are done).
		/// Mutating the data won't have any effect on the PBO but may cause issues in other components.
		/// </param>
		public abstract void ProcessFile(PBOFile file, PBOPacker.InfoProvider infoProvider);
	}
}
