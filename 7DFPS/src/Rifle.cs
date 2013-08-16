using System;

namespace DFPS {
	public class Rifle : Gun {
		public Rifle() : base(TextureTools.TextureRifleInv) {
		}

		public override float Damage {
			get { return 1f; }
		}
		public override float FlashTime {
			get { return .5f; }
		}
		public override bool SemiAutomatic {
			get { return false; }
		}
		public override bool OpensDoors {
			get { return false; }
		}
		public override int RoundsPerSecond {
			get { return -1; }
		}
		public override Pencil.Gaming.Audio.Sound Shot {
			get { return Sounds.Shoot2; }
		}
		public override int DefaultAmmo {
			get { return 5; }
		}
		public override Buffer2D<uint> AmmoTexture {
			get { return TextureTools.TextureRifleAmmo; }
		}
		public override Buffer2D<uint> Scope {
			get { return TextureTools.TextureCrossHairRifle; }
		}
		public override Buffer2D<uint> HoldingInHandTexture {
			get { return TextureTools.TextureHoldingInHandRifle; }
		}
		public override int MagazineSize {
			get { return 5; }
		}
		public override float ReloadTime {
			get { return 1f; }
		}
	}
}

