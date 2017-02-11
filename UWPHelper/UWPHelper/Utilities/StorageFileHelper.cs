﻿using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPHelper.Utilities
{
    public static class StorageFileHelper
    {
        public static async Task<bool> SaveObjectAsync(object obj, string fileName, StorageFolder folder, CreationCollisionOption creationCollisionOption = CreationCollisionOption.OpenIfExists)
        {
            CheckFileName(fileName);
            CheckFolder(folder);

            bool success = true;

            try
            {
                StorageFile file    = await folder.CreateFileAsync(fileName, creationCollisionOption);
                string json         = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(obj));

                await FileIO.WriteTextAsync(file, json);
            }
            catch
            {
                success = false;
            }

            DebugHelper.OperationInfo(fileName, "saving", success);
            return success;
        }

        public static async Task<LoadObjectAsyncResult<T>> LoadObjectAsync<T>(string fileName, StorageFolder folder) where T : class, new()
        {
            CheckFileName(fileName);
            CheckFolder(folder);

            StorageFile file = await folder.TryGetItemAsync(fileName) as StorageFile;
            return file != null ? await LoadObjectAsync<T>(file) : new LoadObjectAsyncResult<T>(new T(), true);
        }

        public static async Task<LoadObjectAsyncResult<T>> LoadObjectAsync<T>(StorageFile file) where T : class, new()
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            bool success = true;
            T obj = null;
            
            try
            {
                // Reading from StorageFile could fail while the file is used by another proccess
                string json = await FileIO.ReadTextAsync(file);
                obj = !string.IsNullOrWhiteSpace(json) ? await Task.Run(() => JsonConvert.DeserializeObject<T>(json)) : new T();
            }
            catch
            {
                obj     = new T();
                success = false;
            }

            DebugHelper.OperationInfo(file.Name, "loading", success);
            return new LoadObjectAsyncResult<T>(obj, success);
        }

        private static void CheckFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Value cannot be empty or null.", nameof(fileName));
            }
        }

        private static void CheckFolder(StorageFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }
        }

        public sealed class LoadObjectAsyncResult<T> where T : class, new()
        {
            public T Object { get; }
            public bool Success { get; }

            public LoadObjectAsyncResult(T @object, bool success)
            {
                Object  = @object;
                Success = success;
            }
        }
    }
}
