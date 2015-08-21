using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TM.Shared
{
   public interface IFileSystemProxy
   {

      FileStream OpenFileForWrite(string path);

      IEnumerable<string> EnumerateFiles(string path);
      IEnumerable<string> EnumerateFiles(string path, string searchPattern);
      IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

      bool IsDirectoryExists(string path);
      bool IsFileExists(string path);

      DirectoryInfo CreateDirectory(string path);

      void WriteTextToNewFile(string path, string text);
      Task WriteTextToNewFileAsync(string path, string text);
      Task WriteTextToNewFileAsync(string path, string text, Encoding encoding);
      Task WriteToNewFileAsync(string path, byte[] data);
      Task WriteToFileAsync(string path, byte[] data);

      string ReadTextFromFile(string path);
      Task<string> ReadTextFromFileAsync(string path);
      Task<string> ReadTextFromFileAsync(string path, Encoding encoding);
   }

   public class FileSystemProxy : IFileSystemProxy
   {
      private static readonly FileSystemProxy ProxyInstance = new FileSystemProxy();

      public static FileSystemProxy Instance
      {
         get { return ProxyInstance; }
      }


      public FileStream OpenFileForWrite(string path)
      {
         return File.OpenWrite(path);
      }

      public IEnumerable<string> EnumerateFiles(string path)
      {
         return Directory.EnumerateFiles(path);
      }

      public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
      {
         return Directory.EnumerateFiles(path, searchPattern);
      }

      public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
      {
         return Directory.EnumerateFiles(path, searchPattern, searchOption);
      }

      public bool IsDirectoryExists(string path)
      {
         return Directory.Exists(path);
      }

      public bool IsFileExists(string path)
      {
         var isExists =  File.Exists(path);

         return isExists;
      }

      public DirectoryInfo CreateDirectory(string path)
      {
         return Directory.CreateDirectory(path);
      }

      public void WriteTextToNewFile(string path, string text)
      {
         File.WriteAllText(path, text);
      }

      public async Task WriteTextToNewFileAsync(string path, string text)
      {
         await WriteTextToNewFileAsync(path, text, Encoding.UTF8);
      }
      
      public async Task WriteTextToNewFileAsync(string path, string text, Encoding encoding)
      {
         var encodedText = encoding.GetBytes(text);

         await WriteToNewFileAsync(path, encodedText);
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="path"/> or
      /// <paramref name="data"/> is <see langword="null" />.</exception>
      public async Task WriteToNewFileAsync(string path, byte[] data)
      {
         if (path == null)
            throw new ArgumentNullException("path");

         if (data == null)
            throw new ArgumentNullException("data");

         using (
            var sourceStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
               bufferSize: 4096, useAsync: true))
         {
            await sourceStream.WriteAsync(data, 0, data.Length);
         }
      }

      public async Task WriteToFileAsync(string path, byte[] data)
      {
         if (path == null)
            throw new ArgumentNullException("path");

         if (data == null)
            throw new ArgumentNullException("data");

         using (
            var sourceStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
               bufferSize: 4096, useAsync: true))
         {
            await sourceStream.WriteAsync(data, 0, data.Length);
         }
      }

      public string ReadTextFromFile(string path)
      {
         return File.ReadAllText(path);
      }

      public async Task<string> ReadTextFromFileAsync(string path)
      {
         return await ReadTextFromFileAsync(path, Encoding.UTF8);
      }

      public async Task<string> ReadTextFromFileAsync(string path, Encoding encoding)
      {
         using (var sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true))
         {
            var sb = new StringBuilder();

            var buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
               var text = encoding.GetString(buffer, 0, numRead);
               sb.Append(text);
            }

            return sb.ToString();
         }
      }

   }
}