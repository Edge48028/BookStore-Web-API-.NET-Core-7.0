﻿using DataLayer.EF;
using DataLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModelAndRequest.Account;
using ModelAndRequest.API;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.AccountServices
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IConfiguration config;
        private readonly EShopDbContext eShopDbContext;

        public AccountService()
        {
        }

        public AccountService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config, EShopDbContext eShopDbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.eShopDbContext = eShopDbContext;
            this.config = config;
        }

        /// <summary>
        /// login service
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        public async Task<ApiResult<object>> Login(LoginRequest loginRequest)
        {
            // tim user neu khong co tra ve thong bao loi
            //sqlserver
            var user = await userManager.FindByNameAsync(loginRequest.username);
            //sqlite
            //var user = userManager.Users.FirstOrDefault(user => user.UserName == loginRequest.Username);
            if (user == null)
                return new ApiResult<object>(success: false, messge: "Mật khẩu hoặc tài khoản không đúng", payload: null);

            if (user.isDelete == true)
                return new ApiResult<object>(success: false, messge: "Tài khoản đã bị xóa vui lòng liên hệ quản trị viên để mở lại", payload: null);

            var result = await signInManager.PasswordSignInAsync(user, loginRequest.password, loginRequest.remember, true);

            if (!result.Succeeded)
                return new ApiResult<object>(success: false, messge: "Mật khẩu hoặc tài khoản không đúng", payload: null);

            //tao claims chua thong tin de luu vao payload cua token
            var roles = await userManager.GetRolesAsync(user);

            var roleResult = "user";
            roleResult = roles.Contains("Administrator") ? "admin" : roleResult;
            roleResult = roles.Contains("Sales") ? "sales" : roleResult;

            var tokenResult = await GenerateToken(user);
            return new ApiResult<object>(success: true, messge: "Đăng nhập thành công", payload: new { token = tokenResult, role = roleResult, id = user.Id });
        }

        /// <summary>
        /// regsier service
        /// </summary>
        /// <param name="registerRequest"></param>
        /// <param name="isSale">if sale = true</param>
        /// <returns></returns>
        public async Task<ApiResult<bool>> Register(RegisterRequest registerRequest, bool isSale = false)
        {
            //sql server
            if (await userManager.FindByNameAsync(registerRequest.username) != null)
                //sqlite
                //if (userManager.Users.FirstOrDefault(user => user.UserName == registerRequest.Username) != null)
                return new ApiResult<bool>(success: false, messge: "Username đã tồn tại", payload: false);
            if (userManager.Users.FirstOrDefault(user => user.Email == registerRequest.email) != null)
                return new ApiResult<bool>(success: false, messge: "Email đã tồn tại", payload: false);

            var user = new User()
            {
                UserName = registerRequest.username,
                Email = registerRequest.email,
                IsMale = registerRequest.isMale,
                Dob = registerRequest.dob,
                FullName = registerRequest.fullName,
                PhoneNumber = registerRequest.phonenumber,
                SecurityStamp = string.Empty,
                EmailConfirmed = true,
                Address = registerRequest.address
            };

            if (!isSale)
                user.isUser = true;

            var result = await userManager.CreateAsync(user, registerRequest.password);

            if (result.Succeeded)
            {
                //sql server
                if (!isSale) await userManager.AddToRoleAsync(user, "User");
                else await userManager.AddToRoleAsync(user, "Sales");


                //sqlite
                //var roleUser = roleManager.Roles.FirstOrDefault(role => role.Name == (isSale ? "Sales" : "User"));
                //eShopDbContext.UserRoles.Add(new UserRole()
                //{
                //    UserId = user.Id,
                //    RoleId = roleUser.Id
                //});
                //await eShopDbContext.SaveChangesAsync();

                return new ApiResult<bool>(success: true, messge: "Đăng kí thành công", payload: true);
            }

            return new ApiResult<bool>(success: false, messge: string.Join("", result.Errors.Select(er => er.Description)), payload: false);
        }

        /// <summary>
        /// create sales service (role admin)
        /// </summary>
        /// <param name="registerRequest"></param>
        /// <returns></returns>
        public async Task<ApiResult<bool>> CreateSales(RegisterRequest registerRequest)
        {
            return await Register(registerRequest, true);
        }

        /// <summary>
        /// get info account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiResult<AccountModel>> GetById(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return new ApiResult<AccountModel>(success: false, messge: "Không tìm thấy user", payload: null);

            var result = new AccountModel()
            {
                id = user.Id,
                username = user.UserName,
                fullName = user.FullName,
                email = user.Email,
                phonenumber = user.PhoneNumber,
                address = user.Address,
                dob = user.Dob,
                avatar = user.Avatar,
                isMale = user.IsMale
            };

            return new ApiResult<AccountModel>(success: true, messge: "Thành công", payload: result);
        }

        /// <summary>
        /// get all account (role admin)
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<ApiResult<List<AccountModel>>> GetAllAccount(string role)
        {
            var data = from u in eShopDbContext.Users
                       join ur in eShopDbContext.UserRoles on u.Id equals ur.UserId
                       join r in eShopDbContext.Roles on ur.RoleId equals r.Id
                       where r.Name == role
                       select new { user = u };

            if (data == null)
                return new ApiResult<List<AccountModel>>(success: false, messge: "Không tìm thấy user", payload: null);

            var users = await data.Select(data => new AccountModel()
            {
                id = data.user.Id,
                username = data.user.UserName,
                fullName = data.user.FullName,
                isMale = data.user.IsMale,
                email = data.user.Email,
                phonenumber = data.user.PhoneNumber,
                dob = data.user.Dob,
                address = data.user.Address,
                avatar = data.user.Avatar,
                isDelete = data.user.isDelete,
            }).ToListAsync();

            if (users == null)
                return new ApiResult<List<AccountModel>>(success: false, messge: "Không tìm thấy user", payload: null);

            return new ApiResult<List<AccountModel>>(success: true, messge: "Thành công", payload: users);
        }

        /// <summary>
        /// delete accoubt  (sales
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<ApiResult<bool>> DeleteAccount(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return new ApiResult<bool>(success: false, messge: "Không tìm thấy user", payload: false);

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Administrator"))
                return new ApiResult<bool>(success: false, messge: "Không thể xóa quản trị viên", payload: false);

            if (roles.Contains("User"))
            {
                user.isDelete = true;
                var update = await userManager.UpdateAsync(user);

                if (update.Succeeded)
                    return new ApiResult<bool>(success: true, messge: "Xóa thành công", payload: false);
                return new ApiResult<bool>(success: false, messge: "Xóa không thành công", payload: false);
            }
            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return new ApiResult<bool>(success: false, messge: "Xóa không thành công", payload: false);

            return new ApiResult<bool>(success: true, messge: "Xóa thành công", payload: true);
        }

        public async Task<ApiResult<bool>> RestoreAccount(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return new ApiResult<bool>(success: false, messge: "Không tìm thấy user", payload: false);
            user.isDelete = false;
            var update = await userManager.UpdateAsync(user);

            if (update.Succeeded)
                return new ApiResult<bool>(success: true, messge: "Xóa thành công", payload: false);
            return new ApiResult<bool>(success: false, messge: "Xóa không thành công", payload: false);
        }

        public async Task<string> GenerateToken(User user)
        {
            //tao claims chua thong tin de luu vao payload cua token
            var roles = await userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role, string.Join(";",roles)),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // ma hoa doi xung
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //tao token
            var token = new JwtSecurityToken(config["Tokens:Issuer"], config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds);

            var tokenResult = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenResult;
        }

        public bool VerifyToken(string Token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenVlalidatorParameter = GetValidationParameters();

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(Token, tokenVlalidatorParameter, out validatedToken);
            return true;
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = config["Tokens:Issuer"],
                ValidAudience = config["Tokens:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Tokens:Key"])) // The same key as the one that generate the token
            };
        }

        public async Task<ApiResult<bool>> ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new ApiResult<bool>(success: false, messge: "Không tìm thất user", payload: false);

            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                return new ApiResult<bool>(success: true, messge: "Thay đổi mật khẩu thành công", payload: true);
            }

            return new ApiResult<bool>(success: false, messge: "Mật khẩu hiện tại không đúng", payload: false);
        }

        public async Task<ApiResult<bool>> ChangeInfo(Guid userId, UpdateAccountRequest accountRequest)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            user.FullName = accountRequest.fullName ?? user.FullName;
            user.Email = accountRequest.email ?? user.Email;
            user.NormalizedEmail = accountRequest.email.ToUpper() ?? user.NormalizedEmail;
            user.Address = accountRequest.address ?? user.Address;
            user.PhoneNumber = accountRequest.phoneNumber ?? user.PhoneNumber;
            user.IsMale = accountRequest.isMale ?? user.IsMale;
            user.Avatar = accountRequest.avatar ?? user.Avatar;

            var result = await eShopDbContext.SaveChangesAsync();

            if (result > 0)
                return new ApiResult<bool>(success: true, messge: "Thay đổi thành công", payload: true);
            return new ApiResult<bool>(success: false, messge: "Thay đổi không thành công", payload: false);
        }

        public async Task<ApiResult<string>> RestorePassword(Guid id, string password)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return new ApiResult<string>(success: false, messge: "Không tìm thấy user", payload: "");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var result = await userManager.ResetPasswordAsync(user, token, password);
            if(result.Succeeded)
                return new ApiResult<string>(success: true, messge: "Thành công", payload: $"Mật khẩu mới của user {id.ToString()} là {password}");

            return new ApiResult<string>(success: false, messge: "Thất bại", payload: "");
        }
    }
}