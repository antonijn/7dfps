using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class Buffer2D<T> {
		public readonly int Width;
		public readonly int Height;
		public T[] Data { get; private set; }

		public void Set(int x, int y, T value) {
			x = Math.Max(0, x);
			y = Math.Max(0, y);
			x = Math.Min(Width - 1, x);
			y = Math.Min(Height - 1, y);
			Data [y * Width + x] = value;
		}

		public T Get(int x, int y) {
			x = Math.Max(x, 0);
			x = Math.Min(x, Width - 1);
			y = Math.Max(y, 0);
			y = Math.Min(y, Height - 1);
			return Data[y * Width + x];
		}

		public void Fill(Func<int, int, T> fillFunc) {
			for (int x = 0; x < Width; ++x) {
				for (int y = 0; y < Height; ++y) {
					Set(x, y, fillFunc(x, y));
				}
			}
		}

		public void Blit(Rectanglei source, Vector2i destPos, Buffer2D<T> dest) {
			Blit(source, destPos, 1, dest);
		}
		public void Blit(Rectanglei source, Vector2i destPos, int scale, Buffer2D<T> dest) {
			for (int x = source.Left; x < source.Right + source.Width * (scale - 1); ++x) {
				if (x >= 0 && x < dest.Width) {
					for (int y = source.Top; y < source.Bottom + source.Height * (scale - 1); ++y) {
						if (y >= 0 && y < dest.Height) {
							if (typeof(T) != typeof(uint)) {
								dest.Set(x - source.Left + destPos.X, y - source.Top + destPos.Y, Get(x / scale, y / scale));
							} else {
								uint color = (uint)((object)Get(x / scale, y / scale));
								if ((color & 0xFF000000) != 0x00) {
									dest.Set(x - source.Left + destPos.X, y - source.Top + destPos.Y, (T)(object)color);
								}
							}
						}
					}
				}
			}
		}

		public Buffer2D(int width, int height) {
			Width = width;
			Height = height;
			Data = new T[Width * Height];
		}
	}
}

