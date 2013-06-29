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
using System.IO;

namespace SoftAlgos {

	public class ModelReader {

		public ModelReader () {

		}

	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ModelChunkAttribute : Attribute {

		private int chunkType;

		public int ChunkType {
			get {
				return this.chunkType;
			}
		}

		public ModelChunkAttribute (int chunkType) {
			this.chunkType = chunkType;
		}

	}

	public abstract class ModelChunk : IReadWriteable {

		public abstract int SelfLength {
			get;
		}

		public int Type {
			get {
				foreach(ModelChunkAttribute mca in this.GetType().GetCustomAttributes(typeof(ModelChunkAttribute),true)) {
					return mca.ChunkType;
				}
				return 0x0000;
			}
		}

		public virtual int Size {
			get {
				return this.SelfLength + 2*sizeof(int);
			}
		}

		public virtual void AddChild (ModelChunk child) {
			throw new ArgumentException();
		}

		protected abstract void ReadInternal (BinaryReader sr, int selflength);

		#region IReadable implementation
		public virtual void Read (BinaryReader sw) {
			int size = sw.ReadInt32();
			ReadInternal(sw,size-2*sizeof(int));
		}
		#endregion

		#region IWriteable implementation
		public virtual void Write (BinaryWriter sw) {
			sw.Write(this.Type);
			sw.Write(this.Size);
		}
		#endregion

	}

	public abstract class ModelChunkWithChildren : ModelChunk {

		private List<ModelChunk> children;

		public override int Size {
			get {
				int val = base.Size;
				foreach (ModelChunk child in this.children) {
					val += child.Size;
				}
				return val;
			}
		}

		public override void AddChild (ModelChunk child) {
			this.children.Add(child);
		}


	}

	[ModelChunk(0x0001)]
	public class ByteArrayChunk : ModelChunk {

		byte[] data;

		public override int SelfLength {
			get {
				return data.Length;
			}
		}

		protected override void ReadInternal (BinaryReader sr, int selflength) {
			this.data = sr.ReadBytes(selflength);
		}

		public override void Write (BinaryWriter sw) {
			base.Write(sw);
			int n = data.Length;
			for(int i = 0x00; i < n; i++) {
				sw.Write(data[i]);
			}
		}

	}

}