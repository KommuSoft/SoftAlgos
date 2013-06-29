//
//  ModelReader.cs
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

namespace SoftAlgos {

	public class ModelReader {

		public ModelReader () {

		}

	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ModelChunkAttribute : Attribute {

		private int type;

	}

	public abstract class ModelChunk {

		private int selfLength = 0x00;
		private List<ModelChunk> children;

		public int SelfLength {
			get {
				return this.selfLength;
			}
			protected set {
				this.selfLength = value;
			}
		}
		public int Size {
			get {
				int val = this.selfLength + sizeof(int) + children;
				foreach (ModelChunk child in this.children) {
					val += child.Size;
				}
				return val;
			}
		}

		public void AddChild (ModelChunk child) {
			this.children.Add(child);
		}


	}

	public class ByteArrayChunk : ModelChunk {



	}

}