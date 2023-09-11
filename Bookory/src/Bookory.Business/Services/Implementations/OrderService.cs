﻿using AutoMapper;
using Bookory.Business.Services.Interfaces;
using Bookory.Business.Utilities.DTOs.BasketDtos;
using Bookory.Business.Utilities.DTOs.Common;
using Bookory.Business.Utilities.DTOs.OrderDetailDtos;
using Bookory.Business.Utilities.DTOs.OrderDtos;
using Bookory.Business.Utilities.DTOs.OrderItemDtos;
using Bookory.Business.Utilities.DTOs.PaymentDetailDto;
using Bookory.Business.Utilities.Exceptions.AuthException;
using Bookory.Business.Utilities.Exceptions.BasketItemException;
using Bookory.Business.Utilities.Exceptions.ShoppingSessionException;
using Bookory.Business.Utilities.Exceptions.UserAddressException;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;

namespace Bookory.Business.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IShoppingSessionService _shoppingSessionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserAddressService _userAddressService;
    private readonly IBasketItemService _basketItemService;
    private readonly IBasketService _basketService;
    private readonly bool? _isAuthenticated;

    private readonly IPaymentDetailService _paymentDetailService;
    private readonly IOrderDetailService _orderDetailService;
    private readonly IOrderItemService _orderItemService;

    private readonly IStripeService _stripeService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public OrderService(IUserAddressService userAddressService, IUserService userService, IHttpContextAccessor httpContextAccessor, IBasketService basketService, IShoppingSessionService shoppingSessionService, IBasketItemService basketItemService, IMapper mapper, IStripeService stripeService, IOrderDetailService orderDetailService, IOrderItemService orderItemService, IPaymentDetailService paymentDetailService)
    {
        _isAuthenticated = _httpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated;
        _httpContextAccessor = httpContextAccessor;
        _shoppingSessionService = shoppingSessionService;
        _paymentDetailService = paymentDetailService;
        _orderDetailService = orderDetailService;
        _userAddressService = userAddressService;
        _basketItemService = basketItemService;
        _orderItemService = orderItemService;
        _stripeService = stripeService;
        _basketService = basketService;
        _userService = userService;
        _mapper = mapper;
    }


    public async Task<ResponseDto> PurchaseBooks(OrderPostDto orderPostDto)
    {
        // Find Active User
        var userId = await GetUserIdAsync();

        var userAddress = await _userAddressService.GetAddressByIdAsync(orderPostDto.AddressId);
        if (userAddress is null)
            throw new UserAddressNotFoundException("User Address not found");

        decimal totalPrice = 0;

        var userSession = await _shoppingSessionService.GetShoppingSessionByUserIdAsync(userId);
        if (userSession is null)
            throw new ShoppingSessionNotFoundException("Shopping Session Not Found");

        var basketItems = _mapper.Map<List<BasketGetResponseDto>>(userSession.BasketItems);

        foreach (var basketItem in basketItems)
        {
            if (basketItem.Quantity < basketItem.BasketBook.StockQuantity)
            {
                decimal DiscountPrice = basketItem.BasketBook.Price - basketItem.BasketBook.DiscountPrice;

                totalPrice += (DiscountPrice * basketItem.Quantity);
            }
            else
                throw new BasketItemQuantityNotEnoughException("Basket item quantity not enough");
        }

        string transactionId = await _stripeService.ChargeAsync(orderPostDto.StripeEmail, orderPostDto.StripeToken, totalPrice);
        if (string.IsNullOrEmpty(transactionId))
            throw new Exception("Payment failed");


        var newPayment = await _paymentDetailService.CreatePaymentDetailAsync(new PaymentDetailPostDto(Convert.ToInt64(totalPrice * 100), transactionId));

        var orderDetail = await _orderDetailService.CreateOrderDetailAsync(new OrderDetailPostDto(totalPrice, userId, newPayment.Id));

        foreach (var basketItem in basketItems)
        {
            await _orderItemService.CreateOrderItemAsync(new OrderItemPostDto(basketItem.Quantity, basketItem.BasketBook.Id, orderDetail.Id));
            await _basketItemService.DeleteBasketItemAsync(basketItem.Id);
        }
        userSession.IsOrdered = true;

        return new ResponseDto((int)HttpStatusCode.OK, "Thank You");
    }

    private async Task<string> GetUserIdAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user is null)
            throw new UserNotFoundException($"User not found by Id {userId}");

        return userId;
    }
}