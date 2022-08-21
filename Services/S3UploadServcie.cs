namespace TestAWS.Services;

using System.IO;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

public class S3UploadService
{
  IAmazonS3 _client { get; set; }
  string _key = "test2.jpg";
  string _bucket = "test-me";

  public S3UploadService(IAmazonS3 client)
  {
    _client = client;
  }

  public async Task upload(string fileName)
  {
    Console.WriteLine($"Config: {_client.Config}");
    Console.WriteLine($"ServiceURL: {_client.Config.ServiceURL}");
    Console.WriteLine($"Region: {_client.Config.RegionEndpoint}");
    InitiateMultipartUploadRequest initiateRequest = new()
    {
      BucketName = _bucket,
      Key = _key,
    };

    InitiateMultipartUploadResponse initResponse = await _client.InitiateMultipartUploadAsync(initiateRequest);
    long contentLength = new FileInfo(fileName).Length;
    long partSize = (long)Math.Pow(2, 20);
    // using FileStream file = File.Open(fileName, FileMode.Open);
    // var sr = new BinaryReader(file);
    List<UploadPartResponse> partResponses = new();
    try
    {
      long filePosition = 0;

      for (int i = 1; filePosition < contentLength; i++)
      {
        var buf = new byte[partSize];
        // var cntRead = sr.Read(buf, 0, buf.Length);
        // using MemoryStream stream = new();
        // using BinaryWriter sw = new(stream);
        // sw.Write(buf, 0, cntRead);
        // stream.Seek(0, SeekOrigin.Begin);
        UploadPartRequest uploadRequest = new()
        {
          BucketName = _bucket,
          Key = _key,
          UploadId = initResponse.UploadId,
          PartNumber = i,
          PartSize = partSize,
          // InputStream = stream,
          FilePath = fileName,
          FilePosition = filePosition,
        };

        uploadRequest.StreamTransferProgress += new EventHandler<StreamTransferProgressArgs>(
          (object? sender, StreamTransferProgressArgs e) =>
          Console.WriteLine($"{e.TransferredBytes}/{e.TotalBytes}")
        );

        partResponses.Add(await _client.UploadPartAsync(uploadRequest));

        filePosition += partSize;

        Console.WriteLine("part uploaded");
      }

      CompleteMultipartUploadRequest completeRequest = new()
      {
        BucketName = _bucket,
        Key = _key,
        UploadId = initResponse.UploadId,
      };
      ListPartsRequest listRequest = new()
      {
        BucketName = _bucket,
        Key = _key,
        UploadId = initResponse.UploadId,
      };
      var listPartsResponse = await _client.ListPartsAsync(listRequest);

      Console.WriteLine("uploaded parts:");
      foreach (var part in listPartsResponse.Parts)
      {

        Console.WriteLine($"num: {part.PartNumber}, etag: {part.ETag}");
      }
      Console.WriteLine("sent parts:");
      foreach (var res in partResponses)
      {
        Console.WriteLine($"num: {res.PartNumber}, etag: {res.ETag}");
      }
      completeRequest.AddPartETags(partResponses);
      for (var i = 0; i < 3; i++)
      {
        try
        {
          CompleteMultipartUploadResponse completeResponse = await _client.CompleteMultipartUploadAsync(completeRequest);
          break;
        }
        catch (Exception e)
        {
          Console.WriteLine($"An AmazonS3Exception was thrown: {e.Message}");
          if (i == 2)
          {
            throw;
          }
          Thread.Sleep(3000);
        }
      }

      Console.WriteLine($"request completed");
    }
    catch (AmazonS3Exception e)
    {
      Console.WriteLine($"An AmazonS3Exception was thrown: {e.ErrorCode}, {e.ErrorType}, {e.Retryable}");
      foreach (var d in e.Data.Keys)
      {

        Console.WriteLine($"An exception data: {d}: {e.Data[d]}");
      }

      AbortMultipartUploadRequest abortMPURequest = new()
      {
        BucketName = _bucket,
        Key = _key,
        UploadId = initResponse.UploadId,
      };

      await _client.AbortMultipartUploadAsync(abortMPURequest);

      Console.WriteLine($"response aborted");
    }
  }
}