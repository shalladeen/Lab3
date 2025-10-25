using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace PodcastApp.Services;
public class S3Service
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    public S3Service(IConfiguration cfg)
    {
        var region = RegionEndpoint.GetBySystemName(cfg["AWS:Region"] ?? "ca-central-1");
        _s3 = new AmazonS3Client(region);
        _bucket = cfg["AWS:S3Bucket"]!;
    }

    public async Task<string> UploadAsync(Stream file, string key)
    {
        var xfer = new TransferUtility(_s3);
        await xfer.UploadAsync(file, _bucket, key);
        return $"https://{_bucket}.s3.amazonaws.com/{Uri.EscapeDataString(key)}";
    }
}
