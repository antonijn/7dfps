using System;

namespace DFPS {
	public class SemiAuto : Gun {
		public SemiAuto() : base(TextureTools.TextureSemiAutoInv) {
		}

		public override Buffer2D<uint> HoldingInHandTexture {
			get { return TextureTools.TextureHoldingInHandSemiAuto; }
		}

		public override float Damage {
			get { return 1f / 3f; }
		}
		public override float FlashTime {
			get { return .2f; }
		}
		public override bool SemiAutomatic {
			get { return true; }
		}
		public override bool OpensDoors {
			get { return false; }
		}
		public override int RoundsPerSecond {
			get { return 10; }
		}
		public override Pencil.Gaming.Audio.Sound Shot {
			get { return Sounds.Shoot3; }
		}
		public override int DefaultAmmo {
			get { return 40; }
		}
		public override Buffer2D<uint> AmmoTexture {
			get { return TextureTools.TexturePistolAmmo; }
		}
		public override Buffer2D<uint> Scope {
			get { return TextureTools.TextureCrossHairSemiAuto; }
		}
		public override int MagazineSize {
			get { return 2; }
		}
		public override float ReloadTime {
			get { return 2f; }
		}
	}
}

