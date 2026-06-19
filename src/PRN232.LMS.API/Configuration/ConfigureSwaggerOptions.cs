using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Configuration;

public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var apiDescription = "ASP.NET Core 8 REST API for a Learning Management System. "
                           + "3-layer architecture with full search, sort, paging, field "
                           + "selection, and expansion support. Built for PRN232 Lab 1.";

        if (description.IsDeprecated)
        {
            apiDescription += " (DEPRECATED)";
        }

        return new OpenApiInfo
        {
            Title = "PRN232 LMS API",
            Version = description.GroupName,
            Description = apiDescription,
            Contact = new OpenApiContact
            {
                Name = "SE151283 - Dai Kim Nguyen",
                Email = "nguyendkse151283@fpt.edu.vn",
                Url = new Uri("https://github.com/Kimdsle/PRN232.LMS.SE151283")
            }
        };
    }
}