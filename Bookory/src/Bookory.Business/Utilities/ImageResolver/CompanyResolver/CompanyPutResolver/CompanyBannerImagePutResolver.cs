﻿using AutoMapper;
using Bookory.Business.Utilities.DTOs.CompanyDtos;
using Bookory.Business.Utilities.Extension.FileExtensions.Common;
using Bookory.Core.Models;
using Microsoft.AspNetCore.Hosting;

namespace Bookory.Business.Utilities.ImageResolver.CompanyResolver.CompanyPutResolver;

public class CompanyBannerImagePutResolver : IValueResolver<CompanyPutDto, Company, string>
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    public CompanyBannerImagePutResolver(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }


    public string Resolve(CompanyPutDto source, Company destination, string destMember, ResolutionContext context)
    {
        string basePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "companies", "banner");
        return FileHelper.SaveFileAsync(source.BannerImage, basePath).GetAwaiter().GetResult();
    }
}
