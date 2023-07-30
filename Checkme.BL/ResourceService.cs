using Checkme.BL.Abstract.Exceptions;
using Checkme.BL.Abstract;
using Checkme.DAL.Abstract;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Checkme.BL
{

    public class ResourceService : IResourceService
    {
        private IBlobStorageRepo _blobStorage;
        public ResourceService(IBlobStorageRepo blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> AddResource(Stream resource, string resourceType)
        {
            var id = $"{Guid.NewGuid()}.{resourceType.Split('/').Last()}";
            await _blobStorage.SaveBlobStream(id, resource);
            return id;
        }

    }
}
