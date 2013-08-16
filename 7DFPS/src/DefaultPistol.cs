using System;

using Pencil.Gaming.Audio;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class DefaultPistol : Gun {
		public DefaultPistol() : base(TextureTools.TextureDefaultPistolInv) {

		}

		public override float Damage {
			get { return .5f; }
		}
		public override float FlashTime {
			get { return .2f; }
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
		public override Sound Shot {
			get { return Sounds.Shoot1; }
		}
		public override int DefaultAmmo {
			get { return 10; }
		}
		public override Texture2D AmmoTexture {
			get { return TextureTools.TexturePistolAmmo; }
		}
		public override Texture2D Scope {
			get { return TextureTools.TextureCrossHairPistol; }
		}
		public override Texture2D HoldingInHandTexture {
			get { return TextureTools.TextureHoldingInHandPistol; }
		}
		public override int MagazineSize {
			get { return 3; }
		}
		public override float ReloadTime {
			get { return .5f; }
		}
	}
}

