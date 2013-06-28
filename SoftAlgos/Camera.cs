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

	public class Camera : IRenderable {

		private double zoomSpeed = 5.0f;
		private double rotateSpeed = 5.0f;
		private double zoom = 0.5f;
		private double rotateXZ = 0.0f;
		private double rotateY = 0.0f;
		private double zoomTarget = 0.5f;
		private double rotateXZTarget = 0.0f;
		private double rotateYTarget = 0.0f;
		private bool mouseDown = false;
		private Point2d offsetPoint = Point2d.Empty;
		private double rotateXYOffset, rotateZOffset;
		private double[,] savedCameraPositions;//saved cameraPositions
		private int showingView = 0x01;
		
		
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
		
		public Camera () {
			savedCameraPositions = new double[,] {//RotateXZ, RotateY, Zoom
				{	60.0f,		0.0f,		0.5f},//silver saved
				{	60.0f,		180.0f,		0.5f}//red saved
			};
			this.MoveToOwner(0x00);
		}
		
		private void changeCameraView (int newView) {
			this.SaveCameraPosition(this.showingView);
			this.showingView = newView;
			this.LoadSavedCameraPosition(this.showingView);
		}
		public void Render (FrameEventArgs e) {
			GL.MatrixMode(MatrixMode.Modelview);
			double zoomFactor = Math.Min(1.0f,Math.Max(0.0f,(double) (this.zoomSpeed*e.Time)));
			double rotateFactor = Math.Min(1.0f,Math.Max(0.0f,(double) (this.rotateSpeed*e.Time)));
			this.rotateXZ = rotateFactor*this.rotateXZTarget+(1.0f-rotateFactor)*this.rotateXZ;
			this.rotateY = rotateFactor*this.rotateYTarget+(1.0f-rotateFactor)*this.rotateY;
			this.zoom = zoomFactor*this.zoomTarget+(1.0f-zoomFactor)*this.zoom;
			GL.LoadIdentity();
			GL.Translate(0.0f,0.0f,-6.0f);
			GL.Rotate(rotateXZ,1.0f,0.0f,0.0f);
			GL.Rotate(rotateY,0.0f,1.0f,0.0f);
			GL.Scale(zoom,zoom,zoom);
		}
		public void SaveCameraPosition (int index) {
			if((index&0xfc) == 0x00) {
				savedCameraPositions[index,0x00] = this.rotateXZTarget;
				savedCameraPositions[index,0x01] = this.rotateYTarget;
				savedCameraPositions[index,0x02] = this.zoomTarget;
			}
		}
		public void LoadSavedCameraPosition (int index) {
			if((index&0xfc) == 0x00) {
				this.RotateXZTarget = savedCameraPositions[index,0x00];
				this.RotateYTarget = savedCameraPositions[index,0x01];
				this.ZoomTarget = savedCameraPositions[index,0x02];
			}
		}
		public void MoveToOwner (int index) {
			this.ZoomTarget = 0.5f;
			switch (index) {
				case 0x00 :
					this.RotateXZTarget = 30.0f;
					this.RotateYTarget = 0.0f;
					break;
				case 0x01 :
					this.RotateXZTarget = 30.0f;
					this.RotateYTarget = 180.0f;
					break;
				case 0x02 :
					this.RotateXZTarget = 90.0f;
					this.RotateYTarget = 90.0f;
					break;
			}
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
				case Key.F1 :
					this.MoveToOwner(0x00);
					break;
				case Key.F2 :
					this.MoveToOwner(0x01);
					break;
				case Key.F3 :
					this.MoveToOwner(0x02);
					break;
				case Key.F4 :
					this.MoveToPerspective();
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
				//stand still
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