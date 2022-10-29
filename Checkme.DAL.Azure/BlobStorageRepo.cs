using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Checkme.DAL.Abstract;
using Microsoft.Extensions.Logging;
using asb = Azure.Storage.Blobs;
using ass = Azure.Storage;
////using Microsoft.WindowsAzure.Storage;
////using Microsoft.WindowsAzure.Storage.Blob;

namespace Checkme.DAL.Azure
{
    public class BlobStorageRepo : IBlobStorageRepo
    {
        private ILogger<BlobStorageRepo> _logger;
        private Config _configuration;
        private BlobContainerClient _cloudBlobContainer;
        public BlobStorageRepo(Config configuration, ILogger<BlobStorageRepo> logger)
        {
            _configuration = configuration;
            _logger = logger;
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.ConnectionString);
            _cloudBlobContainer = blobServiceClient.GetBlobContainerClient(_configuration.TypeId);
        }

        public async Task<IEnumerable<string>> GetAllResourceIds()
        {
            //ass.Sas.
            ////asb.BlobClient blobClient = asb.
            //CloudStorageAccount storageAccount = LoadAccount();
            
            if (!_cloudBlobContainer.Exists())
            {
                return new string[0];
            }

            //cloudBlobContainer.CreateIfNotExists();
            //Console.WriteLine("Created container '{0}'", cloudBlobContainer.Name);
            //Console.WriteLine();

            ////// Set the permissions so the blobs are public. 
            ////BlobContainerPermissions permissions = new BlobContainerPermissions
            ////{
            ////    PublicAccess = BlobContainerPublicAccessType.Blob
            ////};

            ////cloudBlobContainer.per SetPermissions(permissions);

            var list = _cloudBlobContainer.GetBlobs();

            return list.Select(item => item.Name);
        }

        public async Task<string> AddResource<T>(T resource, string resourceId)
        {
            return await SaveBlob(resource, resourceId);
        }

        public void DeleteResource(string resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetResourceLocation<T>(string resourceId)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetResource<T>(string resourceId)
        {
            
            if (!_cloudBlobContainer.Exists())
            {
                return default(T);
            }

            //cloudBlobContainer.CreateIfNotExists();
            //Console.WriteLine("Created container '{0}'", cloudBlobContainer.Name);
            //Console.WriteLine();

            // Set the permissions so the blobs are public. 
            ////BlobContainerPermissions permissions = new BlobContainerPermissions
            ////{
            ////    PublicAccess = BlobContainerPublicAccessType.Blob
            ////};

            ////cloudBlobContainer.SetPermissions(permissions);

            var item = _cloudBlobContainer.GetBlobClient(resourceId);
            var result = await item.DownloadStreamingAsync();

            var content = new StreamReader(result.Value.Content, Encoding.UTF8).ReadToEnd();
            return JsonSerializer.Deserialize<T>(content);
        }

        public async Task<string> SaveBlob<T>(T resource, string resourceId)
        {

            try
            {
                _cloudBlobContainer.CreateIfNotExists();

                ////// Set the permissions so the blobs are public. 
                ////BlobContainerPermissions permissions = new BlobContainerPermissions
                ////{
                ////    PublicAccess = BlobContainerPublicAccessType.Blob
                ////};

                ////cloudBlobContainer.SetPermissions(permissions);


                var result = JsonSerializer.Serialize(resource);

                // Get a reference to the blob address, then upload the file to the blob.
                // Use the value of localFileName for the blob name.
                //string reference = "Checkme" + Guid.NewGuid().ToString() + Path.GetExtension(resource.Filename);
                var blob = _cloudBlobContainer.GetBlobClient(resourceId);

                var response = await blob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(result)),
                                        new BlobHttpHeaders()
                                        {
                                            ContentType = "application/json"
                                        });

                ////cloudBlockBlob.UploadFromByteArray(resource.Content, 0, resource.Content.Length);

                return blob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returned from the service: {ex.Message}");

                throw;
            }
            finally
            {

            }
        }

        public async Task<string> SaveBlobStream(string resourceId, Stream stream)
        {
            string sourceFile = null;
            string destinationFile = null;

            try
            {
                _cloudBlobContainer.CreateIfNotExists();

                var blob = _cloudBlobContainer.GetBlobClient(resourceId);
                await blob.UploadAsync(stream);
                return blob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returned from the service: {ex.Message}");

                throw;
            }
            finally
            {

            }
        }

        public string SaveBlobFromFile(string path)
        {

            string sourceFile = null;
            string destinationFile = null;

            try
            {
                _cloudBlobContainer.CreateIfNotExists();

                // Use the value of localFileName for the blob name.
                string reference = Guid.NewGuid().ToString() + Path.GetExtension(path);
                var blob = _cloudBlobContainer.GetBlobClient(reference);
                ////cloudBlockBlob.UploadFromByteArray(resource.Content, 0, resource.Content.Length);
                blob.Upload(File.Open(path,FileMode.Open));
                ////cloudBlockBlob.UploadFromByteArray(resource.Content, 0, resource.Content.Length);

                return blob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returned from the service: {ex.Message}");

                throw;
            }
            finally
            {

            }
        }

        ////private string GetContentType(ContentType typeId)
        ////{
        ////    switch (typeId)
        ////    {
        ////        case ContentType.PDF:
        ////            return "application/pdf";
        ////        case ContentType.JPG:
        ////            return "image/jpg";
        ////        case ContentType.PNG:
        ////            return "image/png";
        ////        case ContentType.Text:
        ////            return "text/plain";
        ////        case ContentType.Other:
        ////            return "";
        ////        case ContentType.Video:
        ////            return "video/mp4";
        ////        case ContentType.URL:
        ////            return "";
        ////        default:
        ////            return "";
        ////    }
        ////}
    }
}
