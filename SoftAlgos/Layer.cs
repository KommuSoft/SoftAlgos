//
//  Layer.cs
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

	public class Layer : ConfigSensibleBase, IRenderable, IPaintable {

		private readonly Tile[,] tiles;

		public Tile this [int y, int x] {
			get {
				return this.tiles[y,x];
			}
		}
		public int Height {
			get {
				return this.tiles.GetLength(0);
			}
		}
		public int Width {
			get {
				return this.tiles.GetLength(1);
			}
		}

		public Layer (ConfigurationOptions options, int h, int w) : base(options)
		{
			this.tiles = new Tile[h, w];
			for (int y = 0; y < h; y++) {
				for(int x = 0; x < w; x++) {
					this.tiles[y,x] = new EmptyTile();
				}
			}
		}

		#region IRenderable implementation
		public void Render (FrameEventArgs e) {
			double dxy = this.RenderOptions.TileSize;
			int h = this.Height;
			int w = this.Width;
			for(int y = 0; y < h; y++) {
				for(int x = 0; x < w; x++) {
					if(tiles[y,x] != null) {
						GL.PushMatrix();
						tiles[y,x].Render(e);
						GL.PopMatrix();
					}
					GL.Translate(dxy,0.0d,0.0d);
				}
				GL.Translate(-dxy*w,0.0d,dxy);
			}
		}
		#endregion
		#region IPaintable implementation
		public void Paint (Cairo.Context context) {
			throw new System.NotImplementedException ();
		}
		#endregion

	}
}

