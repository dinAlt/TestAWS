using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAWS.Models;
using TestAWS.Services;

namespace TestAWS.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  // [Authorize]
  public class MultipartUploadsController : ControllerBase
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly MultipartUploadsStore _store;
    private readonly S3UploadService _s3;

    public MultipartUploadsController(IServiceProvider services, MultipartUploadsStore store, S3UploadService s3)
    {
      _serviceProvider = services;
      _store = store;
      _s3 = s3;
    }

    [HttpGet]
    public async Task<ActionResult<MultipartUpload>> GetUpload(string id)
    {
      var upload = _store.GetOrDefault(id);
      if (upload != null) {
        return upload;
      }

      using var scope = _serviceProvider.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<MultipartUploadsContext>();

      upload = await context.MultipartUploads.SingleOrDefaultAsync(v => v.Id == id);
      if (upload == null) {
        return NotFound();
      }

      _store.Put(upload);
      return upload;
    }

    [HttpPost]
    public async Task<ActionResult<MultipartUpload>> PostUploadStart()
    {
      using var scope = _serviceProvider.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<MultipartUploadsContext>();
      var upload = new MultipartUpload{
        Expires = DateTime.UtcNow.AddMinutes(10).Ticks,
      };
      context.MultipartUploads.Add(upload);
      await context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetUpload), new { id = upload.Id }, upload);
    }

    [HttpPost("{id}/chunks")]
    public async Task<ActionResult> PostChunk(string id, [FromForm] Chunk chunk)
    {
      var data = chunk.Data;

      Console.WriteLine(data.Length);
      return Ok();
    }

    [HttpPost("{id}")]
    public async Task<ActionResult> PostUploadDone(MultipartUploadDTO upload) {
      return Ok();
    }

    [HttpPost("do")]
    public async Task<ActionResult> PostDo() {
      await _s3.upload("/home/dinalt/img.jpg");
      return Ok();
    }
  }

  public class Chunk
  {
    public IFormFile Data { get; set; } = null!;
  }
}
