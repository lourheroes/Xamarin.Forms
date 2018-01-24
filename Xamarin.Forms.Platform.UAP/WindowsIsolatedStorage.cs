using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace Xamarin.Forms.Platform.UWP
{
	internal class WindowsIsolatedStorage : Internals.IIsolatedStorageFile
	{
		 StorageFolder _folder;

		public WindowsIsolatedStorage(StorageFolder folder)
		{
			if (folder == null)
				throw new ArgumentNullException("folder");

			_folder = folder;
		}

		public Task CreateDirectoryAsync(string path)
		{
			return _folder.CreateFolderAsync(path).AsTask();
		}

		public async Task<bool> GetDirectoryExistsAsync(string path)
		{
			try
			{
				await _folder.GetFolderAsync(path).AsTask().ConfigureAwait(false);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public async Task<bool> GetFileExistsAsync(string path)
		{
			try
			{
				await _folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public async Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
		{
			StorageFile file = await _folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			BasicProperties properties = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
			return properties.DateModified;
		}

		public async Task<Stream> OpenFileAsync(string path, System.IO.FileMode mode, System.IO.FileAccess access)
		{
			StorageFile file;

			switch (mode)
			{
				case System.IO.FileMode.CreateNew:
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.FailIfExists).AsTask().ConfigureAwait(false);
					break;

				case System.IO.FileMode.Create:
				case System.IO.FileMode.Truncate: // TODO See if ReplaceExisting already truncates
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
					break;

				case System.IO.FileMode.OpenOrCreate:
				case System.IO.FileMode.Append:
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
					break;

				case System.IO.FileMode.Open:
					file = await _folder.GetFileAsync(path);
					break;

				default:
					throw new ArgumentException("mode was an invalid FileMode", "mode");
			}

			switch (access)
			{
				case System.IO.FileAccess.Read:
					return await file.OpenStreamForReadAsync().ConfigureAwait(false);
				case System.IO.FileAccess.Write:
					Stream stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false);
					if (mode == System.IO.FileMode.Append)
						stream.Position = stream.Length;

					return stream;

				case System.IO.FileAccess.ReadWrite:
					IRandomAccessStream randStream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false);
					return randStream.AsStream();

				default:
					throw new ArgumentException("access was an invalid FileAccess", "access");
			}
		}

		public Task<Stream> OpenFileAsync(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
		{
			return OpenFileAsync(path, mode, access);
		}
	}
}