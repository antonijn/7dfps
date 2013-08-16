using System;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public abstract class InventoryItem {
		public Texture2D Texture { get; set; }

		public InventoryItem(Texture2D texture) {
			Texture = texture;
		}
		
		/// <returns><c>true</c>, if item should be removed from inventory, <c>false</c> otherwise.</returns>
		public abstract bool LeftClick(MainGameState game, float time);
		public virtual void Update(float time) {
		}

		public virtual int MaxStack { 
			get { return 1; } 
		}

		public abstract Texture2D HoldingInHandTexture { get; }
	}
}

