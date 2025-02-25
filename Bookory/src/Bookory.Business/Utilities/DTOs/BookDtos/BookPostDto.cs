﻿using Microsoft.AspNetCore.Http;

namespace Bookory.Business.Utilities.DTOs.BookDtos;
public record BookPostDto(string Title, List<IFormFile> Images, int MainImageIndex, string Description, decimal Price, decimal DiscountPercentage, Guid AuthorId, List<Guid> GenreIds, int? StockQuantity);
