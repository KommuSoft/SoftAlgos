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
using System.Linq;
using System.Reflection;

namespace SoftAlgos {
	public class ModelReader {

		private static Type[] argtypes = new Type[0x00];
		private static Object[] args = new object[0x00];
		private static readonly Dictionary<uint,ModelChunk> basechunks = new Dictionary<uint,ModelChunk> ();

		public ModelReader () {

		}

		public static void LoadAssembly (Assembly assembly) {
			foreach (Type t in assembly.GetTypes()) {
				if (!t.IsAbstract && typeof(ModelChunk).IsAssignableFrom (t)) {
					ConstructorInfo ci = t.GetConstructor (argtypes);
					if(ci != null) {
						ModelChunk mc = (ModelChunk)ci.Invoke (args);
						foreach (ModelChunkAttribute mca in t.GetCustomAttributes(typeof(ModelChunkAttribute),true)) {
							basechunks.Add (mca.ChunkType, mc);
						}
					}
				}
			}
		}

		private static ModelChunk interpretChunk (BinaryReader sr) {
			uint id = sr.ReadUInt32();
			ModelChunk mc;
			if(basechunks.TryGetValue(id,out mc)) {
				mc = mc.Clone();
				mc.Read(sr);
				mc.Fold();
				return mc;
			}
			else {
				return null;
			}
		}

		public ModelCollection ReadModelCollection (BinaryReader reader) {
			ModelCollectionModelChunk mc = (ModelCollectionModelChunk) interpretChunk(reader);
			return mc.FoldModelCollection();
		}

		public ModelCollection ReadModelCollection (Stream s) {
			BinaryReader br = new BinaryReader(s);
			ModelCollection mc = this.ReadModelCollection(br);
			br.Close();
			return mc;
		}

		public ModelCollection ReadModelCollection (string filename) {
			FileStream fs = File.Open(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
			ModelCollection mc = this.ReadModelCollection(fs);
			fs.Close();
			return mc;
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class ModelChunkAttribute : Attribute {

			private uint chunkType;

			public uint ChunkType {
				get {
					return this.chunkType;
				}
			}

			public ModelChunkAttribute (uint chunkType) {
				this.chunkType = chunkType;
			}

		}

		public abstract class ModelChunk : IReadWriteable {

			public abstract int SelfLength {
				get;
			}

			public uint Type {
				get {
					foreach (ModelChunkAttribute mca in this.GetType().GetCustomAttributes(typeof(ModelChunkAttribute),true)) {
						return mca.ChunkType;
					}
					return 0x0000;
				}
			}

			public virtual int Size {
				get {
					return this.SelfLength + 2 * sizeof(int);
				}
			}

			public virtual void AddChild (ModelChunk child) {
				throw new ArgumentException ();
			}

			protected void CheckSelfLength (int selfLength) {
				if(this.SelfLength != selfLength) {
					throw new ArgumentException(String.Format("The length of chunk {0} must be {1}, but is equal to {2}!",this.Type,this.SelfLength,selfLength));
				}
			}
			protected abstract void ReadInternal (BinaryReader sr, int selflength);
			public virtual void Fold () {}

		#region IReadable implementation
			public virtual void Read (BinaryReader sr) {
				int size = sr.ReadInt32 ();
				ReadInternal (sr, size - 2 * sizeof(int));
			}
		#endregion

		#region IWriteable implementation
			public virtual void Write (BinaryWriter sw) {
				sw.Write (this.Type);
				sw.Write (this.Size);
			}
		#endregion

			public ModelChunk Clone () {
				return (ModelChunk) this.GetType().GetConstructor(argtypes).Invoke(args);
			}

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
				this.children.Add (child);
			}

			public override void Read (BinaryReader sr) {
				int size = sr.ReadInt32 ();
				int nch = sr.ReadInt32 ();
				for (int i = 0x00; i < nch; i++) {
					ModelChunk mc = ModelReader.interpretChunk (sr);
					size -= mc.Size;
				}
				ReadInternal (sr, size - 2 * sizeof(int));
			}

			protected T getFirstChild<T> () where T : ModelChunk {
				return (T) this.children.FirstOrDefault(x => typeof(T).IsInstanceOfType(x));
			}

			protected IEnumerable<T> getChildren<T> () where T : ModelChunk {
				return this.children.Where(x => typeof(T).IsInstanceOfType(x)).Cast<T>();
			}

		}

		public abstract class EmptySelfModelChunkWithChildren : ModelChunkWithChildren {

			public override int SelfLength {
				get {
					return 0x00;
				}
			}

			protected override void ReadInternal (BinaryReader sr, int selflength) {
				this.CheckSelfLength(selflength);
			}

		}

		[ModelChunk(0x10000000)]
		public class ModelCollectionModelChunk : EmptySelfModelChunkWithChildren {

			public ModelCollection FoldModelCollection () {
				ModelCollection mc = new ModelCollection();
				foreach(ModelModelChunk mmc in this.getChildren<ModelModelChunk>()) {
					mc.AddModel(mmc.FoldModel());
				}
				return mc;
			}

		}

		public abstract class NamedEmptySelfModelChunkWithChildren : EmptySelfModelChunkWithChildren {

			public string Name {
				get {
					return this.getFirstChild<StringModelChunk>().Value;
				}
			}

		}

		[ModelChunk(0x11000000)]
		public class ModelModelChunk : NamedEmptySelfModelChunkWithChildren {

			public Model FoldModel () {
				return null;
			}

		}

		[ModelChunk(0x11200000)]
		public class ActionModelChunk : NamedEmptySelfModelChunkWithChildren {



		}

		[ModelChunk(0x11100000)]
		public class FrameModelChunk : ModelChunk {

			public override int SelfLength {
				get {
					return sizeof(int);
				}
			}

			private uint index;

			protected override void ReadInternal (BinaryReader sr, int selflength) {
				this.CheckSelfLength(selflength);
				this.index = sr.ReadUInt32 ();
			}

			public override void Write (BinaryWriter sw) {
				base.Write (sw);
				sw.Write (this.index);
			}

		}

		[ModelChunk(0x11100000)]
		public class StructureModelChunk : ModelChunkWithChildren {

			private ModelDataPurpose purpose;
			private uint offset;
			private uint stride;

			public override int SelfLength {
				get {
					return sizeof(ModelDataPurpose)+2*sizeof(int);
				}
			}
			public ModelDataPurpose Purpose {
				get {
					return this.purpose;
				}
			}
			public uint Offset {
				get {
					return this.offset;
				}
			}
			public uint Stride {
				get {
					return this.stride;
				}
			}

			protected override void ReadInternal (BinaryReader sr, int selflength) {
				this.CheckSelfLength(selflength);
				this.purpose = (ModelDataPurpose) sr.ReadUInt32();
				this.offset = sr.ReadUInt32();
				this.stride = sr.ReadUInt32();
			}

		}

		[Flags]
		public enum ModelDataPurpose : uint {

			Color			= 0x01,
			EdgeFlag		= 0x02,
			FogCoord		= 0x04,
			Index			= 0x08,
			Normal			= 0x10,
			SecondaryColor	= 0x20,
			TextureCoord	= 0x40,
			Vertex			= 0x80

		}

		[Flags]
		public enum ModelEnableFlags : ulong {
			PointSmooth,
			LineSmooth,
			LineStipple,
			PolygonSmooth,
			PolygonStipple,
			CullFace,
			Lighting,
			ColorMaterial,
			Fog,
			DepthTest,
			StencilTest,
			Normalize,
			AlphaTest,
			Dither,
			Blend,
			IndexLogicOp,
			ColorLogicOp,
			ScissorTest,
			TextureGenS,
			TextureGenT,
			TextureGenR,
			TextureGenQ,
			AutoNormal,
			Map1Color4,
			Map1Index,
			Map1Normal,
			Map1TextureCoord1,
			Map1TextureCoord2,
			Map1TextureCoord3,
			Map1TextureCoord4,
			Map1Vertex3,
			Map1Vertex4,
			Map2Color4,
			Map2Index,
			Map2Normal,
			Map2TextureCoord1,
			Map2TextureCoord2,
			Map2TextureCoord3,
			Map2TextureCoord4,
			Map2Vertex3,
			Map2Vertex4,
			Texture1D,
			Texture2D,
			PolygonOffsetPoint,
			PolygonOffsetLine,
			ClipPlane0,
			ClipPlane1,
			ClipPlane2,
			ClipPlane3,
			ClipPlane4,
			ClipPlane5,
			Light0,
			Light1,
			Light2,
			Light3,
			Light4,
			Light5,
			Light6,
			Light7,
			Convolution1D,
			Convolution1DExt,
			Convolution2D,
			Convolution2DExt,
			Separable2D,
			Separable2DExt,
			Histogram,
			HistogramExt,
			MinmaxExt,
			PolygonOffsetFill,
			RescaleNormal,
			RescaleNormalExt,
			Texture3DExt,
			VertexArray,
			NormalArray,
			ColorArray,
			IndexArray,
			TextureCoordArray,
			EdgeFlagArray,
			InterlaceSgix,
			Multisample,
			SampleAlphaToCoverage,
			SampleAlphaToMaskSgis,
			SampleAlphaToOne,
			SampleAlphaToOneSgis,
			SampleCoverage,
			SampleMaskSgis,
			TextureColorTableSgi,
			ColorTable,
			ColorTableSgi,
			PostConvolutionColorTable,
			PostConvolutionColorTableSgi,
			PostColorMatrixColorTable,
			PostColorMatrixColorTableSgi,
			Texture4DSgis,
			PixelTexGenSgix,
			SpriteSgix,
			ReferencePlaneSgix,
			IrInstrument1Sgix,
			CalligraphicFragmentSgix,
			FramezoomSgix,
			FogOffsetSgix,
			SharedTexturePaletteExt,
			AsyncHistogramSgix,
			PixelTextureSgis,
			AsyncTexImageSgix,
			AsyncDrawPixelsSgix,
			AsyncReadPixelsSgix,
			FragmentLightingSgix,
			FragmentColorMaterialSgix,
			FragmentLight0Sgix,
			FragmentLight1Sgix,
			FragmentLight2Sgix,
			FragmentLight3Sgix,
			FragmentLight4Sgix,
			FragmentLight5Sgix,
			FragmentLight6Sgix,
			FragmentLight7Sgix,
			FogCoordArray,
			ColorSum,
			SecondaryColorArray,
			TextureCubeMap,
			ProgramPointSize,
			VertexProgramPointSize,
			VertexProgramTwoSide,
			DepthClamp,
			TextureCubeMapSeamless,
			PointSprite,
			RasterizerDiscard,
			FramebufferSrgb,
			SampleMask,
			PrimitiveRestart
		}

		[ModelChunk(0xf0000002)]
		public class ByteArrayModelChunk : ModelChunk {

			private byte[] value;

			public override int SelfLength {
				get {
					return value.Length;
				}
			}
			public byte[] Value {
				get {
					return this.value;
				}
				set {
					this.value = value;
				}
			}

			public ByteArrayModelChunk () {}
			public ByteArrayModelChunk (byte[] value) {
				this.value = value;
			}

			protected override void ReadInternal (BinaryReader sr, int selflength) {
				this.value = sr.ReadBytes (selflength);
			}

			public override void Write (BinaryWriter sw) {
				base.Write (sw);
				int n = value.Length;
				for (int i = 0x00; i < n; i++) {
					sw.Write (value [i]);
				}
			}

		}

		[ModelChunk(0xf0000001)]
		public class StringModelChunk : ModelChunk {

			private string value;

			public override int SelfLength {
				get {
					return 0x02*value.Length+0x02;
				}
			}
			public string Value {
				get {
					return this.value;
				}
				set {
					this.value = value;
				}
			}

			public StringModelChunk () {}
			public StringModelChunk (string value) {
				this.value = value;
			}

			protected override void ReadInternal (BinaryReader sr, int selflength) {
				this.value = sr.ReadString();
			}

			public override void Write (BinaryWriter sw) {
				base.Write (sw);
				sw.Write(this.value);
			}

		}

	}

}