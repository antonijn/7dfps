using System;
using System.Reflection;

using Pencil.Gaming.MathUtils;

using Texture2D = DFPS.Buffer2D<uint>;
using Font = System.Collections.Generic.Dictionary<char, DFPS.Buffer2D<uint>>;

namespace DFPS {
	public abstract class InventoryStack : InventoryItem {
		private int amount = 1;
		public int Amount {
			get { return amount; }
			set {
				if (value > MaxValue) {
					throw new ArgumentException();
				}
				amount = value;
				ChangeTexture();
			}
		}
		public int MaxValue { get; protected set; }

		protected InventoryStack(Texture2D tex) : base(tex) {
		}

		public override bool LeftClick(MainGameState game, float time) {
			if (!game.Game.MouseClickPrevious) {
				bool result = GetItem().LeftClick(game, time);
				if (result) {
					--Amount;
				}
			}
			return Amount == 0;
		}

		public abstract InventoryItem GetItem();
		protected abstract void ChangeTexture();
		public abstract Type GetItemType();
	}

	public class DroppedItemInventoryStack : InventoryStack {
		private Type type;
		private Texture2D holdingInHand;

		public DroppedItemInventoryStack(Type t, int maxvalue) : base(GetTexture(t, 1)) {
			MaxValue = maxvalue;
			type = t;
			holdingInHand = GetItem().HoldingInHandTexture;
		}

		public override Texture2D HoldingInHandTexture {
			get { return holdingInHand; }
		}

		public override Type GetItemType() {
			return type;
		}

		protected override void ChangeTexture() {
			Texture = GetTexture(type, Amount);
		}

		public override InventoryItem GetItem() {
			return (InventoryItem)Activator.CreateInstance(type);
		}

		public static Texture2D GetTexture(Type type, int stack) {
			InventoryItem item = (InventoryItem)Activator.CreateInstance(type);

			if (stack == 1) {
				return item.Texture;
			}

			Texture2D tex = new Texture2D(item.Texture.Width, item.Texture.Height);
			Texture2D amountTex = TextureTools.Font [stack.ToString() [0]];
			item.Texture.Blit(new Rectanglei(0, 0, item.Texture.Width, item.Texture.Height), new Vector2i(0, 0), tex);
			amountTex.Blit(new Rectanglei(0, 0, amountTex.Width, amountTex.Height), new Vector2i(tex.Width - amountTex.Width - 1, tex.Height - amountTex.Height - 1), tex);
			return tex;
		}
	}

	public class InventoryStack<T> : InventoryStack where T : InventoryItem, new() {
		public InventoryStack(int maxvalue) : base(GetTexture(1)) {
			MaxValue = maxvalue;
		}

		public override Texture2D HoldingInHandTexture {
			get { return GetItem().HoldingInHandTexture; }
		}

		protected override void ChangeTexture() {
			Texture = GetTexture(Amount);
		}

		public override Type GetItemType() {
			return typeof(T);
		}

		public override InventoryItem GetItem() {
			return new T();
		}

		public static Texture2D GetTexture(int stack) {
			T item = new T();

			if (stack == 1) {
				return item.Texture;
			}

			Texture2D tex = new Texture2D(item.Texture.Width, item.Texture.Height);
			Texture2D amountTex = TextureTools.Font [stack.ToString() [0]];
			item.Texture.Blit(new Rectanglei(0, 0, item.Texture.Width, item.Texture.Height), new Vector2i(0, 0), tex);
			amountTex.Blit(new Rectanglei(0, 0, amountTex.Width, amountTex.Height), new Vector2i(tex.Width - amountTex.Width - 1, tex.Height - amountTex.Height - 1), tex);
			return tex;
		}
	}
}

