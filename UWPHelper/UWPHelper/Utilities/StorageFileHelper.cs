﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPHelper.Utilities
{
    public static class StorageFileHelper
    {
        public static async Task<bool> SaveObjectAsync(object obj, string fileName, StorageFolder folder)
        {
            bool success = true;

            try
            {
                StorageFile file = await folder.TryGetItemAsync(fileName) as StorageFile;

                if (file == null)
                {
                    file = await folder.CreateFileAsync(fileName);
                }

                string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(obj));
                await FileIO.WriteTextAsync(file, json);
            }
            catch
            {
                success = false;
            }

            DebugMessages.OperationInfo(fileName, "saving", success);
            return success;
        }

        public static async Task<LoadObjectAsyncResult<T>> LoadObjectAsync<T>(string fileName, StorageFolder folder) where T : class, new()
        {
            LoadObjectAsyncResult<T> output = new LoadObjectAsyncResult<T>();

            try
            {
                StorageFile file = await folder.GetFileAsync(fileName);
                string json = await FileIO.ReadTextAsync(file);

                if (string.IsNullOrWhiteSpace(json))
                {
                    output.Object = new T();
                }
                else
                {
                    output.Object = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(json));
                }
            }
            catch (Exception ex)
            {
                output.Object  = new T();
                output.Success = ex is FileNotFoundException;
            }

            DebugMessages.OperationInfo(fileName, "loading", output.Success);
            return output;
        }

        public class LoadObjectAsyncResult<T> where T : class, new()
        {
            public T Object { get; set; }
            public bool Success { get; set; }

            public LoadObjectAsyncResult()
            {
                Object  = null;
                Success = true;
            }
        }
    }
}