using System.IO;
using System.Threading.Tasks;

namespace WebUpload
{
    public interface IBlobService
    {
        Task<bool> UploadFileBlobAsync(string blobContainerName, Stream fileStream, string fileName);

        string DownloadFileBlobAsync(string blobContainerName, string fileName);

    }
}
