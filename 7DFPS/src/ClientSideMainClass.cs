using System;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;

namespace DFPS {
	public class ClientSideMainClass : MainClass, IDisposable {
		private int oglTexture;

		public void Run() {

			Glfw.Init();
			try {
				Glfw.OpenWindowHint(OpenWindowHint.NoResize, 1);
				#if !DEBUG
				GlfwVidMode mode;
				Glfw.GetDesktopMode(out mode);
				Glfw.OpenWindow(mode.Width, mode.Height, 8, 8, 8, 0, 24, 0, WindowMode.FullScreen);
				#else
				Glfw.OpenWindow(WindowWidth, WindowHeight, 8, 8, 8, 0, 24, 0, WindowMode.Window);
				#endif
				Glfw.SetWindowPos(100, 100);
				Glfw.SetWindowTitle("Caribbean Stick - The Dark Project");
				Glfw.SwapInterval(false);
				GL.Enable(EnableCap.TextureRectangle);
				GL.GenTextures(1, out oglTexture);
				GL.BindTexture(TextureTarget.TextureRectangle, oglTexture);
				GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

				// TODO: non-constant size???
				#if !DEBUG
				GL.Viewport(mode.Width / 2 - WindowWidth / 2, mode.Height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
				#else
				GL.Viewport(0, 0, WindowWidth, WindowHeight);
				#endif
				GL.MatrixMode(MatrixMode.Projection);
				GL.Ortho(0.0, WindowWidth, WindowHeight, 0.0, 0.0, 1.0);
				GL.MatrixMode(MatrixMode.Modelview);

				CurrentGameState = new MenuState(this);

				float prevTime = (float)Glfw.GetTime();

				Glfw.PollEvents();
				while (Glfw.GetWindowParam(WindowParam.Opened) == 1) {
					CurrentKS = KeyboardState.GetState();
					MouseClickCurrent = Glfw.GetMouseButton(MouseButton.LeftButton);
					if (PreviousKS == null) {
						PreviousKS = CurrentKS;
					}

//					if (CurrentKS[Key.Escape]) {
//						Glfw.CloseWindow();
//					}

					float time = (float)Glfw.GetTime();
					float delta = time - prevTime;

					CurrentGameState.Update(delta);
					CurrentGameState.Draw();
					DrawOGL();

					prevTime = time;

					PreviousKS = CurrentKS;
					MouseClickPrevious = MouseClickCurrent;

					Glfw.SwapBuffers();
					Glfw.PollEvents();
				}
			} finally {
				CurrentGameState.Dispose();
				Glfw.Terminate();
			}
		}

		private void DrawOGL() {
			GL.BindTexture(TextureTarget.TextureRectangle, oglTexture);
			GL.TexImage2D(TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba, ScreenWidth, ScreenHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, Screen.Data);

			GL.Begin(BeginMode.Quads);

			GL.TexCoord2(0, 0);
			GL.Vertex2(0, 0);
			GL.TexCoord2(ScreenWidth, 0);
			GL.Vertex2(WindowWidth, 0);
			GL.TexCoord2(ScreenWidth, ScreenHeight);
			GL.Vertex2(WindowWidth, WindowHeight);
			GL.TexCoord2(0, ScreenHeight);
			GL.Vertex2(0, WindowHeight);

			GL.End();
		}

		void IDisposable.Dispose() {
			GL.DeleteTextures(1, ref oglTexture);
		}

		public static void Main(string[] args) {
			using (ClientSideMainClass main = new ClientSideMainClass()) {
				main.Run();
			}
			MultiplayerClient.Flag = true;
		}
	}
}

