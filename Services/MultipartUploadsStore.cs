using TestAWS.Models;
using System.Collections.Concurrent;

namespace TestAWS.Services;

public class MultipartUploadsStore {
  private ConcurrentDictionary<string, MultipartUpload> _store { get; set; }

  public MultipartUploadsStore()
  {
    _store = new();
  }

  public MultipartUpload? GetOrDefault(string id) {
      return _store.GetValueOrDefault(id);
  }

  public void Put(MultipartUpload upload) {
      _store[upload.Id] = upload;
  }

  public void Delete(string id) {
      _store.Remove(id, out MultipartUpload _);
  }
}