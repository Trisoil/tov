﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Drawing;
using CSharpUtils.Endian;
using TalesOfVesperiaUtils.Imaging.Internal;

namespace TalesOfVesperiaUtils.Imaging
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TBlock"></typeparam>
	unsafe abstract public class DXT<TBlock> where TBlock : struct
	{
		/// <summary>
		/// 
		/// </summary>
		static protected readonly int BlockSize = Marshal.SizeOf(typeof(TBlock));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Bitmap"></param>
		/// <param name="File"></param>
		/// <param name="mode"></param>
		public void SaveSwizzled2D(Bitmap Bitmap, Stream File, CompressDXT.CompressionMode mode = CompressDXT.CompressionMode.Normal)
		{
			int Width = Bitmap.Width, Height = Bitmap.Height;
			if ((Width % 4) != 0 || (Height % 4) != 0) throw (new InvalidDataException());

			Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				var Base = (ARGB_Rev*)BitmapData.Scan0.ToPointer();

				int BlockWidth = Width / 4;
				int BlockHeight = Height / 4;
				var BlockCount = BlockWidth * BlockHeight;
				var CurrentDecodedColors = new ARGB_Rev[4 * 4];
				var Blocks = new TBlock[(uint)BlockCount];

				for (int dxt5_n = 0; dxt5_n < BlockCount; dxt5_n++)
				{
					int TileX, TileY;
					Swizzling.XGAddress2DTiledXY(dxt5_n, BlockWidth, BlockSize, out TileX, out TileY);

					int PositionX = TileX * 4;
					int PositionY = TileY * 4;
					int n = 0;

					if ((PositionX + 3 >= Width) || (PositionY + 3 >= Height))
					{
						Console.Error.WriteLine("(Warning! [Write] Position outsude ({0}, {1}) - ({2}x{3}))", PositionX, PositionY, Width, Height);
						continue;
					}

					for (int y = 0; y < 4; y++)
					{
						for (int x = 0; x < 4; x++)
						{
							CurrentDecodedColors[n] = Base[(PositionY + y) * Width + (PositionX + x)];
							n++;
						}
					}

					EncodeBlock(ref Blocks[dxt5_n], ref CurrentDecodedColors, mode);
				}

				File.WriteStructVector(Blocks);
				File.Flush();
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="File"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="_Depth"></param>
		/// <param name="Swizzled"></param>
		/// <returns></returns>
		private BitmapList _LoadSwizzled(Stream File, int Width, int Height, int? _Depth, bool Swizzled = true)
		{
			if ((Width % 4) != 0 || (Height % 4) != 0) throw (new InvalidDataException());

			int Depth = _Depth ?? 1;
			bool Is3D = _Depth.HasValue;
			var BitmapList = new BitmapList(Depth);
			var BitmapListData = new BitmapData[Depth];
			var BitmapListPointers = new ARGB_Rev*[Depth];
			for (int n = 0; n < Depth; n++)
			{
				BitmapList.Bitmaps[n] = new Bitmap(Width, Height);
			}

			for (int n = 0; n < Depth; n++)
			{
				var Bitmap = BitmapList.Bitmaps[n];
				BitmapListData[n] = Bitmap.LockBits(Bitmap.GetFullRectangle(), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				BitmapListPointers[n] = (ARGB_Rev*)BitmapListData[n].Scan0.ToPointer();
			}

			int BlockWidth = Width / 4;
			int BlockHeight = Height / 4;
			var BlockCount = BlockWidth * BlockHeight * Depth;
			var CurrentDecodedColors = new ARGB_Rev[4 * 4];
			var Blocks = File.ReadStructVector<TBlock>((uint)BlockCount);

			for (int BlockN = 0; BlockN < BlockCount; BlockN++)
			{
				int TileX, TileY, TileZ;

				if (Swizzled)
				{
					if (Is3D)
					{
						Swizzling.XGAddress3DTiledXYZ(BlockN, BlockWidth, BlockHeight, BlockSize, out TileX, out TileY, out TileZ);
					}
					else
					{
						Swizzling.XGAddress2DTiledXY(BlockN, BlockWidth, BlockSize, out TileX, out TileY);
						TileZ = 0;
					}
				}
				else
				{
					TileX = BlockN % BlockWidth;
					TileY = BlockN / BlockWidth;
					TileZ = 0;
					Console.Error.Write("(Not implemented!)");
				}

				DecodeBlock(ref Blocks[BlockN], ref CurrentDecodedColors);

				int PositionX = TileX * 4;
				int PositionY = TileY * 4;

				if ((PositionX + 3 >= Width) || (PositionY + 3 >= Height))
				{
					Console.Error.WriteLine(
						"(Warning! [Read] Position outsude ({0}, {1}) - ({2}x{3}) ;; ({4}, {5})) - ({6}x{7}) ;; {8}",
						PositionX, PositionY,
						Width, Height,
						TileX, TileY,
						BlockWidth, BlockHeight,
						BlockN
					);
					continue;
				}

				int n = 0;
				var BitmapPointer = BitmapListPointers[TileZ];
				for (int y = 0; y < 4; y++)
				{
					int BaseOffset = (PositionY + y) * Width + (PositionX);
					for (int x = 0; x < 4; x++)
					{
						BitmapPointer[BaseOffset + x] = CurrentDecodedColors[n];
						n++;
					}
				}
			}

			for (int n = 0; n < Depth; n++)
			{
				BitmapList.Bitmaps[n].UnlockBits(BitmapListData[n]);
			}

			return BitmapList;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Seems to have problems with non-power of two Width/Height s</remarks>
		/// <param name="File"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="Swizzled"></param>
		/// <returns></returns>
		public Bitmap LoadSwizzled2D(Stream File, int Width, int Height, bool Swizzled = true)
		{
			return _LoadSwizzled(File, Width, Height, null, Swizzled).Bitmaps[0];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="File"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		/// <param name="Depth"></param>
		/// <param name="Swizzled"></param>
		/// <returns></returns>
		public BitmapList LoadSwizzled3D(Stream File, int Width, int Height, int Depth, bool Swizzled = true)
		{
			return _LoadSwizzled(File, Width, Height, Depth, Swizzled);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Block"></param>
		/// <param name="Colors"></param>
		/// <param name="CompressionMode"></param>
		abstract protected void EncodeBlock(ref TBlock Block, ref ARGB_Rev[] Colors, CompressDXT.CompressionMode CompressionMode);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Block"></param>
		/// <param name="Colors"></param>
		abstract protected void DecodeBlock(ref TBlock Block, ref ARGB_Rev[] Colors);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static internal void EXT_INS(ref ushort container, ref byte value, bool extract, int offset, int len, int offset_value = 0)
		{
			var mask = (ushort)((1 << len) - 1);
			if (extract)
			{
				value &= (byte)~((uint)mask << offset_value);
				value |= (byte)((((int)container >> (int)offset) & mask) << offset_value);
			}
			else
			{
				container = (ushort)((container & ~(mask << offset)) | (((value >> offset_value) & mask) << offset));
				//Console.WriteLine(container);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static internal void exs_ins2(byte[] data_transfer, ref ushort_be first_data, bool extract, uint m, uint n, int offset, int len, int offset_value = 0)
		{
			fixed (ushort_be* data = &first_data)
			{
				ushort value = data[m];
				EXT_INS(ref value, ref data_transfer[n], extract, offset, len, offset_value);
				if (!extract) data[m] = value;
			}
		}
	}
}