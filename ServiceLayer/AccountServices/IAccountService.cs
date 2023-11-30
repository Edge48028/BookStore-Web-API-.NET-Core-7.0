﻿using ModelAndRequest.Account;
using ModelAndRequest.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.AccountServices
{
    public interface IAccountService
    {
        Task<ApiResult<object>> Login(LoginRequest loginRequest);

        Task<ApiResult<bool>> Register(RegisterRequest registerRequest, bool isSale = false);

        Task<ApiResult<bool>> ChangePassword(Guid userId, string oldPassword, string newPassword);

        Task<ApiResult<bool>> ChangeInfo(Guid userId, UpdateAccountRequest accountRequest);

        Task<ApiResult<bool>> CreateSales(RegisterRequest registerRequest);

        Task<ApiResult<AccountModel>> GetById(Guid id);

        Task<ApiResult<List<AccountModel>>> GetAllAccount(string role);

        Task<ApiResult<bool>> DeleteAccount(Guid id);

        Task<ApiResult<bool>> RestoreAccount(Guid id);

        Task<ApiResult<string>> RestorePassword(Guid id, string password);
    }
}