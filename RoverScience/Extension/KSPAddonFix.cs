using System;

namespace RoverScience
{

    //ADDONFIX BY MAJIR
    public class KSPAddonFixed : KSPAddon, IEquatable<KSPAddonFixed>
	{
		private readonly Type type;

		public KSPAddonFixed(KSPAddon.Startup startup, bool once, Type type)
			: base(startup, once)
		{
			this.type = type;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != this.GetType()) { return false; }
			return Equals((KSPAddonFixed)obj);
		}

		public bool Equals(KSPAddonFixed other)
		{
			if (this.once != other.once) { return false; }
			if (this.startup != other.startup) { return false; }
			if (this.type != other.type) { return false; }
			return true;
		}

		public override int GetHashCode()
		{
			return this.startup.GetHashCode() ^ this.once.GetHashCode() ^ this.type.GetHashCode();
		}
	}

}