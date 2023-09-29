﻿namespace Bookory.Business.Utilities.DTOs.BookDtos;

public record BookFiltersDto(List<Guid>? Authors , List<Guid>? Genres, decimal? MinPrice , decimal? MaxPrice);
