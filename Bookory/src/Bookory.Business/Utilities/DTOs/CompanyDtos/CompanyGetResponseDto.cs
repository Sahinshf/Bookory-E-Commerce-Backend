﻿using Bookory.Business.Utilities.DTOs.BookDtos;

namespace Bookory.Business.Utilities.DTOs.CompanyDtos;

public record CompanyGetResponseDto(Guid Id, string UserId, string Name, string Description, string Logo, string BannerImage, string ContactEmail, string ContactPhone, string? Address, decimal? Rating, ICollection<BookGetResponseDto>? Books);
