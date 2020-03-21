﻿using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenDirectoryDownloader.FileUpload
{
    public class UploadFileIo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<UploadFilesFile> UploadFile(HttpClient httpClient, string path)
        {
            using (MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent($"Upload----{Guid.NewGuid()}"))
            {
                multipartFormDataContent.Add(new StreamContent(new FileStream(path, FileMode.Open)), "file", Path.GetFileName(path));

                int i = 0;
                int retries = 5;

                while (i < retries)
                {
                    try
                    {
                        using (HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("https://up.uploadfiles.io/upload", multipartFormDataContent))
                        {
                            if (httpResponseMessage.IsSuccessStatusCode)
                            {
                                string response = await httpResponseMessage.Content.ReadAsStringAsync();

                                Logger.Debug($"Response from Uploadfiles.io: {response}");

                                return JsonConvert.DeserializeObject<UploadFilesFile>(response);
                            }
                            else
                            {
                                Logger.Error($"Error uploading file... Retry in 5 seconds!!!");
                                await Task.Delay(TimeSpan.FromSeconds(5));
                            }

                            retries++;
                        }
                    }
                    catch (Exception)
                    {
                        Logger.Error($"Error uploading file... Retry in 5 seconds!!!");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }

                throw new Exception("Error uploading Urls file...");
            }
        }
    }

    public class UploadFilesFile
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("destination")]
        public Uri Destination { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("timing")]
        public string Timing { get; set; }
    }
}
