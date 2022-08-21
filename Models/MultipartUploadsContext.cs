using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace TestAWS.Models;

public class MultipartUploadsContext : DbContext
{
  public MultipartUploadsContext(DbContextOptions<MultipartUploadsContext> options) : base(options)
  {
  }
  
  public DbSet<MultipartUpload> MultipartUploads { get; set; } = null!;
}