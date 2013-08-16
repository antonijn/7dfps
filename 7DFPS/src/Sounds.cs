using System;
using System.Reflection;

using Pencil.Gaming.Audio;

namespace DFPS {
	public static class Sounds {
		public static readonly Sound Reload1 = LoadSound("reload.wav");
		public static readonly Sound Shoot1 = LoadSound("shoot1.wav");
		public static readonly Sound Shoot2 = LoadSound("shoot2.wav");
		public static readonly Sound Shoot3 = LoadSound("shoot3.wav");
		public static readonly Sound Squeak = LoadSound("butter.wav");
		public static readonly Sound Grenade = LoadSound("grenade.wav");
		public static readonly Sound PickupCrate = LoadSound("pickup_crate.wav");
		public static readonly Sound PickupWeapon = LoadSound("pickup_weapon.wav");
		public static readonly Sound CharPlot = LoadSound("char_plot.wav");

		private static Sound LoadSound(string name) {
			if (!MainGameState.IsServer) {
				return new Sound(Assembly.GetEntryAssembly().GetManifestResourceStream(name), "wav");
			}

			return null;
		}
	}
}
