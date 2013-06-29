//
//  Camera.cs
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
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace SoftAlgos {

	public class Camera : TimeSensitiveBase, IRenderable {

		private double zoomSpeed = 5.0d;
		private double rotateSpeed = 5.0d;
		private double translateSpeed = 5.0d;
		private double zoom = 0.5f;
		private double rotateXZ = 0.0f;
		private double rotateY = 30.0f;
		private double zoomTarget = 0.5f;
		private double rotateXZTarget = 0.0f;
		private double rotateYTarget = 0.0f;
		private bool mouseDown = false;
		private Point2d offsetPoint = Point2d.Empty;
		private Vector3d translate = new Vector3d(0.0d,0.0d,-6.0d);
		private Vector3d translateTarget = new Vector3d(0.0d,0.0d,-6.0d);
		private double rotateXYOffset, rotateZOffset;
		
		
		public double ZoomSpeed {
			get {
				return this.zoomSpeed;
			}
			set {
				this.zoomSpeed = value;
			}
		}
		public double RotateSpeed {
			get {
				return this.rotateSpeed;
			}
			set {
				this.rotateSpeed = value;
			}
		}
		public double TranslateSpeed {
			get {
				return this.translateSpeed;
			}
			set {
				this.translateSpeed = value;
			}
		}
		public double RotateXZ {
			get {
				return this.rotateXZ;
			}
		}
		public double RotateY {
			get {
				return this.rotateY;
			}
		}
		public double Zoom {
			get {
				return this.zoom;
			}
		}
		public Vector3d Translate {
			get {
				return this.translate;
			}
			set {
				this.translate = value;
			}
		}
		public double RotateXZTarget {
			get {
				return this.rotateXZTarget;
			}
			set {
				double diff = value-this.rotateXZ;
				diff -= 360.0f*(double) Math.Floor(diff/360.0f);
				if(diff >= 180.0f)
					diff -= 360.0f;
				this.rotateXZTarget = this.rotateXZ+diff;
			}
		}
		public double RotateYTarget {
			get {
				return this.rotateYTarget;
			}
			set {
				double diff = value-this.rotateY;
				diff -= 360.0f*(double) Math.Floor(diff/360.0f);
				if(diff >= 180.0f)
					diff -= 360.0f;
				this.rotateYTarget = this.rotateY+diff;
			}
		}
		public double ZoomTarget {
			get {
				return this.zoomTarget;
			}
			set {
				this.zoomTarget = value;
			}
		}
		public Vector3d TranslateTarget {
			get {
				return this.translateTarget;
			}
			set {
				this.translateTarget = value;
			}
		}
		
		public Camera () {
		}

		public override void AdvanceTime (double dt) {
			double zF = Utils.Border(0.0d,(double) (this.zoomSpeed*dt),1.0d);
			double rF = Utils.Border(0.0d,(double) (this.rotateSpeed*dt),1.0d);
			double tF = Utils.Border(0.0d,(double) (this.translateSpeed*dt),1.0d);
			double tFa = 1.0d-tF;
			this.rotateXZ = rF*this.rotateXZTarget+(1.0f-rF)*this.rotateXZ;
			this.rotateY = rF*this.rotateYTarget+(1.0f-rF)*this.rotateY;
			this.zoom = zF*this.zoomTarget+(1.0f-zF)*this.zoom;
			this.translate.X = tFa*this.translate.X+tF*this.translateTarget.X;
			this.translate.Y = tFa*this.translate.Y+tF*this.translateTarget.Y;
			this.translate.Z = tFa*this.translate.Z+tF*this.translateTarget.Z;
		}

		public void Render (FrameEventArgs e) {
			GL.LoadIdentity();
			GL.Translate(this.translate);
			GL.Rotate(rotateXZ,1.0f,0.0f,0.0f);
			GL.Rotate(rotateY,0.0f,1.0f,0.0f);
			GL.Scale(zoom,zoom,zoom);
		}

		public void MoveToPerspective () {
			this.RotateXZTarget = 45.0f;
			this.RotateYTarget = -45.0f;
			this.ZoomTarget = 0.4f;
		}
		public void OnKeyDown (KeyboardKeyEventArgs e) {
			this.OnKeyDown(null,e);
		}
		public void OnKeyDown (object s, KeyboardKeyEventArgs e) {
			switch(e.Key) {
				case Key.Up :
					this.translateTarget.Z += 1.0d;
					break;
				case Key.Down :
					this.translateTarget.Z -= 1.0d;
					break;
				case Key.Left :
					this.translateTarget.X -= 1.0d;
					break;
				case Key.Right :
					this.translateTarget.X += 1.0d;
					break;
			}
		}
		public void OnMouseWheel (MouseWheelEventArgs e) {
			OnMouseWheel (null,e);
		}
		public void OnMouseWheel (object s, MouseWheelEventArgs e) {
			this.ZoomTarget *= (double) Math.Pow(1.1f,e.DeltaPrecise);
		}
		public void OnMouseDown (object s, MouseButtonEventArgs e) {
			if(e.Button == MouseButton.Right) {
				this.RotateXZTarget = this.rotateXZ;
				this.RotateYTarget = this.rotateY;
				this.ZoomTarget = this.zoom;
				this.offsetPoint = e.Position;
				this.rotateXYOffset = this.rotateXZ;
				this.rotateZOffset = this.rotateY;
				this.mouseDown = true;
			}
		}
		public void OnMouseMove (object s, MouseMoveEventArgs e) {
			if(mouseDown) {
				this.rotateY = this.rotateZOffset+(e.X-this.offsetPoint.X);
				this.rotateXZ = this.rotateXYOffset+(e.Y-this.offsetPoint.Y);
				this.rotateXZTarget = this.rotateXZ;
				this.rotateYTarget = this.rotateY;
			}
		}
		public void OnMouseUp (object s, MouseButtonEventArgs e) {
			if(e.Button == MouseButton.Right)
				this.mouseDown = false;
		}
		
	}
	
}