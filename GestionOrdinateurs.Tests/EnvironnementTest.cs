using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace GestionOrdinateurs.Tests;

public class EnvironnementTest : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "Tests";
    public string EnvironmentName { get; set; } = "Test";
    public string ContentRootPath { get; set; } = "";
    public string WebRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
}