using System.IO;
using System.Threading.Tasks;


namespace Checkme.BL.Abstract
{
    public interface IResourceService
    {
        Task<string> AddResource(Stream resource, string resourceType);
    }
}