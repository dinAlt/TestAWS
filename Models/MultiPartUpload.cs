namespace TestAWS.Models;

public class MultipartUpload
{
  public string Id { get; set; } = null!;
  public long Expires { get; set; }
}
 
public class MultipartUploadDTO {
  public string Id { get; set; } = null!;
  public long? Expires { get; set; }
  public string? CheckSum { get; set; }
  public bool? Done { get; set; }
}