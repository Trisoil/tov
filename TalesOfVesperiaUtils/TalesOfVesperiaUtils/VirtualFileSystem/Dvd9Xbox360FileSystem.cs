﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.VirtualFileSystem;
using TalesOfVesperiaUtils.Formats.Packages;

namespace TalesOfVesperiaUtils.VirtualFileSystem
{
	public class Dvd9Xbox360FileSystem : FileSystem
	{
		Dvd9Xbox360 Dvd9Xbox360;

		public Dvd9Xbox360FileSystem(Dvd9Xbox360 Dvd9Xbox360)
		{
			this.Dvd9Xbox360 = Dvd9Xbox360;
		}

		protected override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			return new FileSystemFileStreamStream(this, Dvd9Xbox360.RootEntry[FileName].Open());
		}

		protected override void ImplWriteFile(FileSystemFileStream FileStream, byte[] Buffer, int Offset, int Count)
		{
			throw new NotImplementedException();
		}

		protected override int ImplReadFile(FileSystemFileStream FileStream, byte[] Buffer, int Offset, int Count)
		{
			throw new NotImplementedException();
		}

		protected override void ImplCloseFile(FileSystemFileStream FileStream)
		{
		}

		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			throw new NotImplementedException();
		}

		private FileSystemEntry ConvertEntry(Dvd9Xbox360.Entry Entry)
		{
			return new FileSystemEntry(this, Entry.FullName)
			{
				Size = Entry.Size,
				Type = Entry.IsDirectory ? FileSystemEntry.EntryType.Directory : FileSystemEntry.EntryType.File,
			};
		}

		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			return ConvertEntry(Dvd9Xbox360.RootEntry[Path]);
		}

		protected override void ImplDeleteFile(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplDeleteDirectory(string Path)
		{
			throw new NotImplementedException();
		}

		protected override void ImplCreateDirectory(string Path, int Mode = 0777)
		{
			throw new NotImplementedException();
		}

		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			foreach (var ChildEntry in Dvd9Xbox360.RootEntry[Path])
			{
				yield return ConvertEntry(ChildEntry);
			}
		}
	}
}