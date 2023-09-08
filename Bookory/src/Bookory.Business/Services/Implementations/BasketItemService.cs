﻿using Bookory.Business.Services.Interfaces;
using Bookory.Business.Utilities.DTOs.Common;
using Bookory.Business.Utilities.Exceptions.BasketException;
using Bookory.Business.Utilities.Exceptions.BookExceptions;
using Bookory.Core.Models;
using Bookory.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Bookory.Business.Services.Implementations;

public class BasketItemService : IBasketItemService
{
    private readonly IBasketItemRepository _basketItemRepository;

    public BasketItemService(IBasketItemRepository basketItemRepository)
    {
        _basketItemRepository = basketItemRepository;
    }

 

    public async Task<BasketItem> GetBasketItemByBookIdAsync(Guid id)
    {
        var basketItem = await _basketItemRepository.GetSingleAsync(bi => bi.BookId == id);
        if (basketItem is null)
            throw new BookNotFoundException($"Book does not exist in basket");

        return basketItem;
    }

    public async Task<List<BasketItem>> GetBasketItemBySessionIdAsync(Guid id)
    {
        List<BasketItem> basketItems =await _basketItemRepository.GetFiltered(bi => bi.SessionId == id).ToListAsync();

        if (basketItems.Count == 0)
            throw new BasketItemNotFoundException("Basket Item Not Found");

        return basketItems; 
    }

    public async Task<BasketItem> GetExistingBasketItemAsync(Guid userSessionId, Guid bookId)
    {
        return await _basketItemRepository.GetSingleAsync(ss => ss.SessionId == userSessionId && ss.BookId == bookId);
    }

    public async Task<ResponseDto> UpdateBasketItemAsync(BasketItem basketItem)
    {
        _basketItemRepository.Update(basketItem);
        await _basketItemRepository.SaveAsync();

        return new ResponseDto((int)HttpStatusCode.OK, "Basket item is updated");
    }

    public async Task<ResponseDto> DeleteBasketItemAsync(Guid id)
    {
        var basketItem = await _basketItemRepository.GetByIdAsync(id);

        _basketItemRepository.Delete(basketItem);
        await _basketItemRepository.SaveAsync();

        return new ResponseDto((int)HttpStatusCode.OK, "Book is deleted");
    }
}
