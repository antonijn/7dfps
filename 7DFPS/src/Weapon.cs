using System;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public abstract class Weapon : InventoryItem {
		public Weapon(Texture2D texture) : base(texture) {
		}
	}
}

