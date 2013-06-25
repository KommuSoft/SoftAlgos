//
//  World.cs
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
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SoftAlgos {

	public class World : ConfigSensibleBase, IRenderable {

		private readonly List<Layer> layers = new List<Layer>();

		public World (ConfigurationOptions options, int h, int w) : this(options,1,h,w) {}
		public World (ConfigurationOptions options, int d, int h, int w) : base(options) {
			for (int i = 0; i < d; i++) {
				layers.Add(new Layer(options,h,w));
			}
		}

		#region IRenderable implementation
		public void Render (FrameEventArgs e) {
			double dy = this.RenderOptions.TileHeight;
			foreach(Layer lay in layers) {
				GL.Translate(0.0d,dy,0.0d);
				GL.PushMatrix();
				lay.Render(e);
				GL.PopMatrix();
			}
		}
		#endregion

	}

}