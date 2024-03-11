namespace DayZObfuscatorModel.PBO.Packer
{
	public abstract class PBOPackerComponent
	{
		/// <summary>
		/// Applies this component to a PBO
		/// </summary>
		/// <param name="descriptor">The PBO to apply the component to</param>
		/// <param name="properties">Properties which will be writter to the PBO properties header</param>
		public abstract void Apply(PBODescriptor descriptor, IDictionary<string, string> properties);
	}
}
