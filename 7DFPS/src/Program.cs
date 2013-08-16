using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Pencil.Gaming;
using Pencil.Gaming.Audio;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class MainClass {

		public static readonly Random Random = new Random();

		public GameState CurrentGameState { get; set; }

		public KeyboardState PreviousKS { get; protected set; }
		public KeyboardState CurrentKS { get; protected set; }
		public bool MouseClickCurrent { get; set; }
		public bool MouseClickPrevious { get; set; }

		public const int WindowWidth = 960;
		public const int WindowHeight = 640;

		public const int Scale = 4;

		public const int ScreenWidth = WindowWidth / Scale;
		public const int ScreenHeight = WindowHeight / Scale;

		public readonly Texture2D Screen = new Texture2D(ScreenWidth, ScreenHeight);
		public readonly Buffer2D<float> ZBuffer = new Buffer2D<float>(ScreenWidth, ScreenHeight);
	}
}
