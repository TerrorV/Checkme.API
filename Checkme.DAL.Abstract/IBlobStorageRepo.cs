using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Checkme.DAL.Abstract
{
    public interface IBlobStorageRepo
    {
        Task<string> AddResource<T>(T resource, string resourceId);
        Task<IEnumerable<string>> GetAllResourceIds();
        Task<T> GetResource<T>(string resourceId);
        Task<string> SaveBlob<T>(T resource, string resourceId);
        Task<string> SaveBlobStream(string resourceId, Stream stream);
    }
}