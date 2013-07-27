//
//  MainWindow.cs
//
//  Author:
//       Willem Van Onsem <vanonsem.willem@gmail.com>
//
//  Copyright (c) 2013 Willem Van Onsem
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SoftAlgos {

	public class MainWindow : GameWindow {

		private Matrix4 perspective;
		private Camera camera = new Camera();
		private readonly float[][] l0 = new float[][] {	new float[] {-20.5f,7.0f,-7.0f},//l0P
														new float[] {0.1f,0.1f,0.1f,0.1f},//l0A
														new float[] {1.0f,1.0f,0.8627f,1.0f},//l0D//0.8f,0.8f,0.8f,0.8f
														new float[] {1.0f,1.0f,0.8627f,1.0f},//l0S
														new float[] {1.0f,1.0f,1.0f,1.0f},//l0O
														new float[] {0.0f,0.0f,0.0f,0.0f}};//l0M
		
		private static float[] materialSpecular = { 0.8f, 0.8f, 0.8f, 1.0f };
		private static float[] surfaceShininess = { 96.0f };

		public MainWindow () : base(800,600) {
			this.Title = "SoftAlgos v. 0.1";
		}

		protected override void OnRenderFrame (FrameEventArgs e) {
			base.OnRenderFrame(e);
			TimeSensitiveBase.AdvanceTimeAllItems(e.Time);
			GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);

			this.camera.Render(e);
			GL.Color3(1.0d,1.0d,1.0d);

			GL.Begin(BeginMode.Quads);
			GL.Normal3(0.0d,0.0d,-1.0d);
			GL.Vertex3(-1.0d,1.0d,-1.0d);
			GL.Vertex3(1.0d,1.0d,-1.0d);
			GL.Vertex3(1.0d,-1.0d,-1.0d);
			GL.Vertex3(-1.0d,-1.0d,-1.0d);

			GL.Normal3(-1.0d,0.0d,0.0d);
			GL.Vertex3(-1.0d,1.0d,-1.0d);
			GL.Vertex3(-1.0d,1.0d,1.0d);
			GL.Vertex3(-1.0d,-1.0d,1.0d);
			GL.Vertex3(-1.0d,-1.0d,-1.0d);

			GL.Normal3(1.0d,0.0d,0.0d);
			GL.Vertex3(1.0d,1.0d,-1.0d);
			GL.Vertex3(1.0d,1.0d,1.0d);
			GL.Vertex3(1.0d,-1.0d,1.0d);
			GL.Vertex3(1.0d,-1.0d,-1.0d);

			GL.Normal3(0.0d,-1.0d,0.0d);
			GL.Vertex3(-1.0d,-1.0d,-1.0d);
			GL.Vertex3(-1.0d,-1.0d,1.0d);
			GL.Vertex3(1.0d,-1.0d,1.0d);
			GL.Vertex3(1.0d,-1.0d,-1.0d);

			GL.Normal3(0.0d,1.0d,0.0d);
			GL.Vertex3(-1.0d,1.0d,-1.0d);
			GL.Vertex3(-1.0d,1.0d,1.0d);
			GL.Vertex3(1.0d,1.0d,1.0d);
			GL.Vertex3(1.0d,1.0d,-1.0d);
			GL.End();
			GL.Flush();
			this.SwapBuffers();
		}

		protected override void OnLoad (EventArgs e) {
			base.OnLoad(e);
			//this.WindowState = WindowState.Fullscreen;
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);
			GL.Enable(EnableCap.ColorMaterial);
			GL.Enable(EnableCap.LineSmooth);
			GL.ShadeModel(ShadingModel.Smooth);
			GL.Light(LightName.Light0, LightParameter.Position, this.l0[0x00]);
			GL.Light(LightName.Light0, LightParameter.Ambient, this.l0[0x01]);
			GL.Light(LightName.Light0, LightParameter.Diffuse, this.l0[0x02]);
			GL.Light(LightName.Light0, LightParameter.Specular, this.l0[0x03]);
			GL.Light(LightName.Light0, LightParameter.SpotExponent, this.l0[0x04]);
			GL.LightModel(LightModelParameter.LightModelAmbient, this.l0[0x05]);
			GL.LightModel(LightModelParameter.LightModelTwoSide, 0);
			GL.LightModel(LightModelParameter.LightModelLocalViewer, 0);
			GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, surfaceShininess);
			this.Keyboard.KeyDown += this.camera.OnKeyDown;
			this.Mouse.Move += this.camera.OnMouseMove;
			this.Mouse.ButtonDown += this.camera.OnMouseDown;
			this.Mouse.ButtonUp += this.camera.OnMouseUp;
			this.Mouse.WheelChanged += this.camera.OnMouseWheel;
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize(e);
			GL.Viewport(this.ClientRectangle);
			float temp = (float)this.Height/(float)this.Width;
			this.perspective = Matrix4.CreatePerspectiveFieldOfView(0.125f*(float)Math.PI, temp, 0.001f, 64000.0f);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref perspective);
			GL.MatrixMode(MatrixMode.Modelview);
			base.OnResize (e);
		}

		public static int Main (string[] args) {
			using(MainWindow mw = new MainWindow()) {
				mw.Run();
			}
			return 0x00;
		}

	}
}
