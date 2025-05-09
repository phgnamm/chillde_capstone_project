using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.DashBoardModels;
using Chillde.Repositories.Models.SearchModels;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Repositories.Models.VoucherModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.AccountModels.OAuth2;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.DashBoardModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.TokenModels;
using Chillde.Services.Utils;
using CloudinaryDotNet;
using Elasticsearch.Net;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Nest;
using RabbitMQ.Client;
using StackExchange.Redis;
using static System.Net.Mime.MediaTypeNames;


//using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Account = Chillde.Repositories.Entities.Account;
using Role = Chillde.Repositories.Enums.Role;

namespace Chillde.Services.Services;

public class AccountService : IAccountService
{
    private readonly IClaimService _claimService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryHelper _cloudinaryHelper;
    private readonly IConfiguration _configuration;
    private readonly IEmailHelper _iIEmailHelper;
    private readonly IMapper _mapper;
    private readonly IRedisHelper _redisHelper;
    private readonly IShippingAddressService _shippingAddressService;
    private readonly HttpClient _httpClient;
    private readonly string _stringeeSIDkey;
    private readonly string _stringeeSecretKey;
    private readonly string _stringeeSenderPhone;

    public AccountService(IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IConfiguration configuration,
        IEmailHelper iIEmailHelper, IMapper mapper, IRedisHelper redisHelper, IUnitOfWork unitOfWork,
        IShippingAddressService shippingAddressService, HttpClient httpClient)
    {
        _claimService = claimService;
        _cloudinaryHelper = cloudinaryHelper;
        _configuration = configuration;
        _iIEmailHelper = iIEmailHelper;
        _mapper = mapper;
        _redisHelper = redisHelper;
        _unitOfWork = unitOfWork;
        _shippingAddressService = shippingAddressService;
        _httpClient = httpClient;
        _stringeeSIDkey = configuration["Stringee:SIDkey"]!;
        _stringeeSecretKey = configuration["Stringee:SecretKey"]!;
        _stringeeSenderPhone = configuration["Stringee:SenderPhone"]!;
    }

    public async Task<ResponseModel> SignUp(AccountSignUpModel accountSignUpModel)
    {
        var existedEmail = await _unitOfWork.AccountRepository.FindByEmailAsync(accountSignUpModel.Email);
        if (existedEmail != null)
            return new ResponseModel
            {
                Code = StatusCodes.Status409Conflict,
                Message = "Email already exists"
            };

        if (!string.IsNullOrWhiteSpace(accountSignUpModel.Username))
        {
            var existedUsername = await _unitOfWork.AccountRepository.FindByUsernameAsync(accountSignUpModel.Username);
            if (existedUsername != null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Username already exists"
                };
        }
        else
        {
            accountSignUpModel.Username = AuthenticationTools.GenerateUsername();
        }

        var account = _mapper.Map<Account>(accountSignUpModel);
        account.HashedPassword = AuthenticationTools.HashPassword(accountSignUpModel.Password);
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(Constant.VerificationCodeLength);
        account.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(Constant.VerificationCodeValidityInMinutes);
        account.Wallet = new Wallet
        {
            Balance = 0,
        };
        await _unitOfWork.AccountRepository.AddAsync(account);
        account.Wallet.CreatedById = account.Id;

        // Add "user" role as default
        var role = await _unitOfWork.RoleRepository.FindByNameAsync(Role.Customer.ToString());
        var accountRole = new AccountRole
        {
            Account = account,
            Role = role!
        };
        await _unitOfWork.AccountRoleRepository.AddAsync(accountRole);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            // Email verification
            await SendVerificationEmail(account);
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Sign up successfully, please verify your email"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot sign up"
        };
    }

    public async Task<ResponseModel> SignIn(AccountSignInModel accountSignInModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountSignInModel.Email);
        if (account != null)
        {
            if (account.IsDeleted)
                return new ResponseModel
                {
                    Code = StatusCodes.Status410Gone,
                    Message = "Account has been deleted"
                };

            if (AuthenticationTools.VerifyPassword(accountSignInModel.Password, account.HashedPassword))
            {
                var tokenModel = await GenerateJwtToken(account);
                if (tokenModel != null)
                    return new ResponseModel
                    {
                        Message = "Sign in successfully",
                        Data = tokenModel
                    };

                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Cannot sign in"
                };
            }
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status404NotFound,
            Message = "Invalid email or password"
        };
    }

    /// <summary>
    ///     To get the code, make an HTTP request to https://accounts.google.com/o/oauth2/v2/auth with these query string
    ///     parameters:
    ///     client_id=&amp;redirect_uri=&amp;response_type=code&amp;
    ///     scope=https://www.googleapis.com/auth/userinfo.email%20https://www.googleapis.com/auth/userinfo.profile&amp;
    ///     access_type=offline
    ///     Document: https://developers.google.com/identity/protocols/oauth2/web-server#creatingclient
    /// </summary>
    /// <param name="code">The authorization code that is returned to the web server appears on the query string</param>
    /// <returns></returns>
    public async Task<ResponseModel> SignInGoogle(string code)
    {
        var clientId = _configuration["OAuth2:Google:ClientId"];
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        var clientSecret = _configuration["OAuth2:Google:ClientSecret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        var redirectUrl = _configuration["URL:Client"];
        ArgumentException.ThrowIfNullOrWhiteSpace(redirectUrl);

        // Exchange authorization code for refresh and access tokens
        // Document: https://developers.google.com/identity/protocols/oauth2/web-server#exchange-authorization-code
        var googleTokenClient = new HttpClient();
        var googleTokenResponse = await googleTokenClient.PostAsJsonAsync(
            "https://oauth2.googleapis.com/token", new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code,
                grant_type = "authorization_code",
                redirect_uri = redirectUrl
            });
        if (!googleTokenResponse.IsSuccessStatusCode)
            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Error when trying to connect to Google API"
            };

        // Get user information with Google access token
        var googleTokenModel =
            JsonSerializer.Deserialize<GoogleTokenModel>(await googleTokenResponse.Content.ReadAsStringAsync());
        var googleUserInformationClient = new HttpClient();
        googleUserInformationClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", googleTokenModel!.AccessToken);
        var googleUserInformationResponse =
            await googleUserInformationClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!googleUserInformationResponse.IsSuccessStatusCode)
            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Error when trying to connect to Google API"
            };

        // Handle business
        var googleUserInformationModel =
            JsonSerializer.Deserialize<GoogleUserInformationModel>(await googleUserInformationResponse.Content
                .ReadAsStringAsync());
        Console.WriteLine(await googleUserInformationResponse.Content
            .ReadAsStringAsync());
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(googleUserInformationModel!.Email);
        if (account != null)
        {
            var tokenModel = await GenerateJwtToken(account);
            if (tokenModel != null)
                return new ResponseModel
                {
                    Message = "Sign in successfully",
                    Data = tokenModel
                };

            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Cannot sign in"
            };
        }

        account = new Account
        {
            FirstName = googleUserInformationModel.FirstName,
            LastName = googleUserInformationModel.LastName,
            Username = AuthenticationTools.GenerateUsername(),
            Email = googleUserInformationModel.Email,
            HashedPassword = AuthenticationTools.HashPassword(AuthenticationTools.GenerateUniqueToken()),
            Image = googleUserInformationModel.Image,
            EmailConfirmed = true
        };
        account.Wallet = new Wallet
        {
            Balance = 0,
        };
        await _unitOfWork.AccountRepository.AddAsync(account);
        account.Wallet.CreatedById = account.Id;

        // Add "user" role as default
        var role = await _unitOfWork.RoleRepository.FindByNameAsync(Role.Customer.ToString());
        var accountRole = new AccountRole
        {
            Account = account,
            Role = role!
        };
        await _unitOfWork.AccountRoleRepository.AddAsync(accountRole);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            var tokenModel = await GenerateJwtToken(account);
            if (tokenModel != null)
                return new ResponseModel
                {
                    Message = "Sign in successfully",
                    Data = tokenModel
                };

            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Cannot sign in"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot sign in"
        };
    }

    public async Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel)
    {
        if (!accountRefreshTokenModel.DeviceId.HasValue ||
            string.IsNullOrWhiteSpace(accountRefreshTokenModel.AccessToken) ||
            !accountRefreshTokenModel.RefreshToken.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid information"
            };

        var refreshToken =
            await _unitOfWork.RefreshTokenRepository.FindByDeviceIdAsync(accountRefreshTokenModel.DeviceId.Value);
        if (refreshToken == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Device not found"
            };

        // Validate access token
        var principal =
            AuthenticationTools.GetPrincipalFromExpiredToken(accountRefreshTokenModel.AccessToken, _configuration);
        if (principal == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid access token"
            };

        var accountIdFromPrincipal = principal.FindFirst("accountId")?.Value;
        if (string.IsNullOrWhiteSpace(accountIdFromPrincipal) ||
            !Guid.TryParse(accountIdFromPrincipal, out var accountId))
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid account"
            };

        // Validate refresh token
        var account = await _unitOfWork.AccountRepository.GetAsync(accountId);
        if (account == null || account.IsDeleted || refreshToken.CreatedById != account.Id ||
            refreshToken.Token != accountRefreshTokenModel.RefreshToken ||
            refreshToken.Expires < DateTime.UtcNow)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid refresh token"
            };

        var tokenModel = await GenerateJwtToken(account, refreshToken, principal);
        if (tokenModel != null)
            return new ResponseModel
            {
                Message = "Refresh token successfully",
                Data = tokenModel
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot refresh token"
        };
    }

    public async Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel)
    {
        var refreshTokens =
            await _unitOfWork.RefreshTokenRepository.GetAllAsync(refreshToken =>
                refreshToken.CreatedBy.Email == accountEmailModel.Email);
        _unitOfWork.RefreshTokenRepository.HardRemoveRange(refreshTokens.Data);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseModel
        {
            Message = "Revoke tokens successfully"
        };
    }

    public async Task<ResponseModel> VerifyEmail(string email, string verificationCode)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email,
            include: account =>
                account.Include(a => a.AccountRoles.Where(ar => ar.Role.Name == Role.Customer.ToString())));
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Message = "Email has been verified"
            };

        if (account.VerificationCodeExpiryTime < DateTime.UtcNow)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "The code is expired"
            };

        if (account.VerificationCode == verificationCode)
        {
            account.EmailConfirmed = true;
            account.VerificationCode = null;
            account.VerificationCodeExpiryTime = null;
            account.Status = AccountStatus.Active;
            account.AccountRoles.First().Status = AccountStatus.Active;
            _unitOfWork.AccountRepository.Update(account);
            _unitOfWork.AccountRoleRepository.Update(account.AccountRoles.First());
            if (await _unitOfWork.SaveChangeAsync() > 0)
            {
                await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
                await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
                await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

                return new ResponseModel
                {
                    Message = "Verify email successfully"
                };
            }
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status400BadRequest,
            Message = "Cannot verify email"
        };
    }

    public async Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (account.EmailConfirmed)
            return new ResponseModel
            {
                Message = "Email has been verified"
            };

        // Update new verification code
        account.VerificationCode = AuthenticationTools.GenerateDigitCode(Constant.VerificationCodeLength);
        account.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(Constant.VerificationCodeValidityInMinutes);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await SendVerificationEmail(account);

            return new ResponseModel
            {
                Message = "Resend verification email successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot resend verification email"
        };
    }

    public async Task<ResponseModel> SendVerifyPhone(string email)
    {
        //var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email);
        //if (account == null)
        //    return new ResponseModel
        //    {
        //        Code = StatusCodes.Status404NotFound,
        //        Message = "Account not found"
        //    };

        //account.VerificationCode = AuthenticationTools.GenerateDigitCode(Constant.VerificationCodeLength);
        //account.VerificationCodeExpiryTime = DateTime.UtcNow.AddMinutes(Constant.VerificationCodeValidityInMinutes);

        //string phone = null;
        //if (account.PhoneNumber.StartsWith("0"))
        //{
        //    phone = $"84{account.PhoneNumber.Substring(1)}";
        //}

        //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_stringeeSecretKey));
        //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //var claims = new[]
        //{
        //    new Claim("jti", Guid.NewGuid().ToString()),
        //    new Claim("iss", _stringeeSIDkey),
        //    new Claim("exp", ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds().ToString()), // Expiry in 1 hour
        //    new Claim("rest_api", "true")
        //};

        //var token = new JwtSecurityToken(
        //    issuer: _stringeeSIDkey,
        //    claims: claims,
        //    expires: DateTime.UtcNow.AddHours(1),
        //    signingCredentials: credentials
        //);

        //string stringeeAccessToken = new JwtSecurityTokenHandler().WriteToken(token);
        //string apiUrl = "https://api.stringee.com/v1/call2/callout";

        //using (HttpClient client = new HttpClient())
        //{
        //    client.DefaultRequestHeaders.Add("X-STRINGEE-AUTH", stringeeAccessToken);
        //    //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        //    var requestData = new
        //    {
        //        from = new
        //        {
        //            type = "external",
        //            number = _stringeeSenderPhone, // Replace with your Stringee virtual number
        //            alias = _stringeeSenderPhone
        //        },
        //        to = new[]
        //{
        //    new
        //    {
        //        type = "external",
        //        number = phone, // The recipient’s phone number
        //        alias = phone
        //    }
        //},
        //        answer_url = "https://https://developer.stringee.com/scco_helper/simple_project_answer_url?record=false&appToPhone=auto&recordFormat=mp3/answerurl", // URL that will return call actions
        //        actions = new[]
        //{
        //    new
        //    {
        //        action = "talk",
        //        text = $"Mã OTP của bạn là {account.VerificationCode}. Xin nhắc lại mã otp của bạn là {account.VerificationCode}."
        //    }
        //}
        //    };

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
        //    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        //    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
        //    string responseString = await response.Content.ReadAsStringAsync();

        //return new ResponseModel
        //{
        //    Code = int.Parse(response.StatusCode.ToString()),
        //    Message = responseString
        //};

        return new ResponseModel
        {
            Code = StatusCodes.Status200OK
            //Message = responseString
        };
        //}
    }

    public async Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value);
        if (AuthenticationTools.VerifyPassword(accountChangePasswordModel.OldPassword, account!.HashedPassword))
        {
            account.HashedPassword = AuthenticationTools.HashPassword(accountChangePasswordModel.NewPassword);
            _unitOfWork.AccountRepository.Update(account);
            if (await _unitOfWork.SaveChangeAsync() > 0)
                return new ResponseModel
                {
                    Message = "Change password successfully"
                };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot change password"
        };
    }

    public async Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountEmailModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        var resetPasswordToken =
            AuthenticationTools.GenerateUniqueToken(
                DateTime.UtcNow.AddDays(Constant.ResetPasswordTokenValidityInMinutes));
        account.ResetPasswordToken = resetPasswordToken;
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _iIEmailHelper.SendEmailAsync(account.Email, "Reset your password",
                $"Your token is {resetPasswordToken}. The token will expire in {Constant.ResetPasswordTokenValidityInMinutes} minutes.",
                true);

            return new ResponseModel
            {
                Message = "An email has been sent, please check your inbox"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot send email"
        };
    }

    public async Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel)
    {
        var account = await _unitOfWork.AccountRepository.FindByEmailAsync(accountResetPasswordModel.Email);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (accountResetPasswordModel.Token != account.ResetPasswordToken)
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid token"
            };

        account.ResetPasswordToken = null;
        account.HashedPassword = AuthenticationTools.HashPassword(accountResetPasswordModel.Password);
        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
            return new ResponseModel
            {
                Message = "Reset password successfully"
            };

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot reset password"
        };
    }

    public async Task<ResponseModel> AddRange(AccountAddRangeModel accountAddRangeModel)
    {
        var accounts = new List<Account>();
        var roles = await _unitOfWork.RoleRepository.GetAllAsync();
        var userRole = await _unitOfWork.RoleRepository.FindByNameAsync(Role.Customer.ToString());
        foreach (var accountSignUpModel in accountAddRangeModel.Accounts)
        {
            if (!string.IsNullOrWhiteSpace(accountSignUpModel.Username))
            {
                var existedUsername =
                    await _unitOfWork.AccountRepository.FindByUsernameAsync(accountSignUpModel.Username);
                if (existedUsername != null)
                    accountSignUpModel.Username = AuthenticationTools.GenerateUsername();
            }
            else
            {
                accountSignUpModel.Username = AuthenticationTools.GenerateUsername();
            }

            var account = _mapper.Map<Account>(accountSignUpModel);
            account.HashedPassword = AuthenticationTools.HashPassword(accountSignUpModel.Password);
            if (accountSignUpModel.Roles != null && accountSignUpModel.Roles.Any())
            {
                var rolesOfAccount = roles.Data.Where(role =>
                    accountSignUpModel.Roles.Select(r => r.ToString()).Distinct().Contains(role.Name)).ToList();
                if (rolesOfAccount.Any())
                    foreach (var role in rolesOfAccount)
                    {
                        account.AccountRoles.Add(new AccountRole { Account = account, Role = role });
                        if (role.Name != Role.Admin.ToString())
                        {
                            account.Wallet = new Wallet
                            {
                                Balance = 0,
                            };
                        }
                    }
            }
            else
            {
                account.AccountRoles.Add(new AccountRole { Account = account, Role = userRole! });
            }

            accounts.Add(account);
        }

        await _unitOfWork.AccountRepository.AddRangeAsync(accounts);
        accounts.ForEach(_ => _.Wallet.Id = _.Id);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Add accounts successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = $"Cannot add {accountAddRangeModel.Accounts.Count} accounts"
        };
    }

    public async Task<ResponseModel> Get(string idOrUsername)
    {
        var cacheKey = $"account_{idOrUsername}";
        var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
        {
            Account? account;
            if (Guid.TryParse(idOrUsername, out var id))
                account = await _unitOfWork.AccountRepository.GetAsync(id, accounts =>
                    accounts
                        .Include(a => a.AccountRoles).ThenInclude(accountRole => accountRole.Role)
                        .Include(a => a.ShippingAddresses)
                        .Include(a => a.Wallet)
                        .ThenInclude(w => w.Transactions)
                        .Include(a => a.Orders)
                        .Include(a => a.Services));
            else
                account = await _unitOfWork.AccountRepository.FindByUsernameAsync(idOrUsername, accounts =>
                    accounts
                        .Include(a => a.AccountRoles).ThenInclude(accountRole => accountRole.Role)
                        .Include(a => a.ShippingAddresses)
                        .Include(a => a.Wallet)
                        .ThenInclude(w => w.Transactions)
                        .Include(a => a.Orders)
                        .Include(a => a.Services));

            if (account == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Account not found"
                };

            var accountModel = _mapper.Map<AccountModel>(account);

            return new ResponseModel
            {
                Message = "Get account successfully",
                Data = accountModel
            };
        });

        return responseModel;
    }

    public async Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel)
    {
        var accounts = await _unitOfWork.AccountRepository.GetAllAsync(
            account =>
                (!accountFilterModel.IsDeleted.HasValue || account.IsDeleted == accountFilterModel.IsDeleted) &&
                (!accountFilterModel.Gender.HasValue || account.Gender == accountFilterModel.Gender) &&
                // (!accountFilterModel.Status.HasValue || account.Status == accountFilterModel.Status) &&
                (!accountFilterModel.Role.HasValue || account.AccountRoles.Where(accountRole =>
                        !accountFilterModel.Status.HasValue || accountRole.Status == accountFilterModel.Status)
                    .Select(accountRole => accountRole.Role.Name)
                    .Contains(accountFilterModel.Role.ToString())) &&
                (string.IsNullOrWhiteSpace(accountFilterModel.Search) ||
                 account.FirstName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                 account.LastName.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                 account.Username.ToLower().Contains(accountFilterModel.Search.ToLower()) ||
                 account.Email.ToLower().Contains(accountFilterModel.Search.ToLower())),
            accounts =>
            {
                switch (accountFilterModel.Order.ToLower())
                {
                    case "firstName":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.FirstName)
                            : accounts.OrderBy(account => account.FirstName);
                    case "lastName":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.LastName)
                            : accounts.OrderBy(account => account.LastName);
                    case "dateOfBirth":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.DateOfBirth)
                            : accounts.OrderBy(account => account.DateOfBirth);
                    case "email":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Email)
                            : accounts.OrderBy(account => account.Email);
                    case "phoneNumber":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.PhoneNumber)
                            : accounts.OrderBy(account => account.PhoneNumber);
                    case "isDeleted":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.IsDeleted)
                            : accounts.OrderBy(account => account.IsDeleted);
                    case "gender":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Gender)
                            : accounts.OrderBy(account => account.Gender);
                    case "userName":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Username)
                            : accounts.OrderBy(account => account.Username);
                    default:
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.CreationDate)
                            : accounts.OrderBy(account => account.CreationDate);
                }
            },
            accounts => accounts
                .Include(account => account.ShippingAddresses)
                .Include(account => account.AccountRoles)
                .ThenInclude(accountRole => accountRole.Role)
                .Include(account => account.Wallet)
                .Include(account => account.Orders)
                .Include(account => account.Services),
            accountFilterModel.PageIndex,
            accountFilterModel.PageSize
        );
        var accountModels = _mapper.Map<List<AccountModel>>(accounts.Data);
        var accountIds = accountModels.Select(a => a.Id).ToList();
        var orderCounts = await _unitOfWork.Context.Orders
            .Where(x => accountIds.Contains(x.Package.CreatedById!.Value) && !x.IsDeleted)
            .GroupBy(x => x.Package.CreatedById)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var accountModel in accountModels)
        {
            accountModel.OrderArtisanCount = orderCounts
                .FirstOrDefault(x => x.AccountId == accountModel.Id)?.Count ?? 0;
        }

        var result = new Pagination<AccountModel>(accountModels, accountFilterModel.PageIndex,
            accountFilterModel.PageSize, accounts.TotalCount);

        return new ResponseModel
        {
            Message = "Get all accounts successfully",
            Data = result
        };
    }

    public async Task<ResponseModel> Update(Guid id, AccountUpdateModel accountUpdateModel)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        if (accountUpdateModel.Username != account.Username)
        {
            var existedUsername = await _unitOfWork.AccountRepository.FindByUsernameAsync(accountUpdateModel.Username);
            if (existedUsername != null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Username already exists"
                };
        }

        _mapper.Map(accountUpdateModel, account);
        // if (accountUpdateModel.NewImage != null)
        //     account.Image = await _cloudinaryHelper.UploadImageAsync(accountUpdateModel.NewImage,
        //         $"{account.Id.ToString()}_image",
        //         $"{account.Id.ToString()}_image", folderName: FolderAttachment.ACCOUNT);
        //
        // if (accountUpdateModel.NewBanner != null)
        //     account.Banner = await _cloudinaryHelper.UploadImageAsync(accountUpdateModel.NewBanner,
        //         $"{account.Id.ToString()}_banner",
        //         $"{account.Id.ToString()}_banner", folderName: FolderAttachment.ACCOUNT);

        _unitOfWork.AccountRepository.Update(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Update account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot update account"
        };
    }

    public async Task<ResponseModel> UpdateRoles(Guid id, AccountUpdateRolesModel accountUpdateRolesModel)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        var existedAccountRoles =
            await _unitOfWork.AccountRoleRepository.GetAllAsync(accountRole => accountRole.AccountId == id,
                include: accountRoles => accountRoles.Include(accountRole => accountRole.Role));
        var roles = await _unitOfWork.RoleRepository.GetAllAsync();
        var newRoles = roles.Data.Where(role =>
            accountUpdateRolesModel.Roles.Select(r => r.ToString()).Distinct().Contains(role.Name)).ToList();
        if (!newRoles.Any())
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid roles"
            };

        var rolesToRemove = existedAccountRoles.Data.Select(accountRole => accountRole.Role).Except(newRoles).ToList();
        // var rolesToAdd = newRoles.Except(existedAccountRoles.Data.Select(accountRole => accountRole.Role)).ToList();
        var rolesToAdd = newRoles;
        if (!rolesToRemove.Any() && !rolesToAdd.Any())
            return new ResponseModel
            {
                Code = StatusCodes.Status409Conflict,
                Message = "Account roles already exists"
            };

        // Remove roles
        var accountRolesToRemove = existedAccountRoles.Data
            .Where(accountRole => rolesToRemove.Contains(accountRole.Role)).ToList();
        foreach (var accountRole in accountRolesToRemove)
        {
            accountRole.Status = AccountStatus.Suspended;
            accountRole.IsDeleted = true;
        }

        if (rolesToRemove.Any()) _unitOfWork.AccountRoleRepository.UpdateRange(accountRolesToRemove);

        // Add roles
        if (rolesToAdd.Any())
        {
            var accountRolesToAdd = new List<AccountRole>();
            var accountRolesToRestore = new List<AccountRole>();
            foreach (var role in rolesToAdd)
            {
                var existedAccountRole =
                    existedAccountRoles.Data.FirstOrDefault(accountRole => accountRole.Role.Id == role.Id);
                if (existedAccountRole != null)
                {
                    existedAccountRole.Status = AccountStatus.Active;
                    existedAccountRole.IsDeleted = false;
                    accountRolesToRestore.Add(existedAccountRole);
                }
                else
                {
                    accountRolesToAdd.Add(new AccountRole
                    {
                        Account = account,
                        Role = role,
                        Status = AccountStatus.Active
                    });
                }
            }

            _unitOfWork.AccountRoleRepository.UpdateRange(accountRolesToRestore);
            await _unitOfWork.AccountRoleRepository.AddRangeAsync(accountRolesToAdd);
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Update account roles successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot update account roles"
        };
    }

    public async Task<ResponseModel> Delete(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.SoftRemove(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Delete account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot delete account"
        };
    }

    public async Task<ResponseModel> Restore(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        _unitOfWork.AccountRepository.Restore(account);
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Restore account successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot restore account"
        };
    }

    public async Task<ResponseModel> BecomeASeller(Guid id, AccountBecomeASellerModel accountBecomeASellerModel)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };

        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (!account!.EmailConfirmed)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Email must be confirmed"
            };
        }

        _mapper.Map(accountBecomeASellerModel, account);
        // if (accountBecomeASellerModel.NewImage != null)
        //     account!.Image = await _cloudinaryHelper.UploadImageAsync(accountBecomeASellerModel.NewImage,
        //         $"{account.Id.ToString()}_image",
        //         $"{account.Id.ToString()}_image");
        //
        // if (accountBecomeASellerModel.NewBanner != null)
        //     account!.Banner = await _cloudinaryHelper.UploadImageAsync(accountBecomeASellerModel.NewBanner,
        //         $"{account.Id.ToString()}_banner",
        //         $"{account.Id.ToString()}_banner");

        if (account!.Image == null || account.Banner == null)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Image and banner is required"
            };
        }

        accountBecomeASellerModel.ShippingAddress.IsDefault = true;
        await _shippingAddressService.AddShippingAddressAsync(accountBecomeASellerModel.ShippingAddress);
        _unitOfWork.AccountRepository.Update(account);
        var currentRoles = await _unitOfWork.RoleRepository.GetAllByAccountIdAsync(id);
        if (currentRoles.All(role => role.Name != Role.Artisan.ToString()))
        {
            var roleArtisan = await _unitOfWork.RoleRepository.FindByNameAsync(Role.Artisan.ToString());
            await _unitOfWork.AccountRoleRepository.AddAsync(
                new AccountRole
                {
                    Account = account,
                    Role = roleArtisan!,
                    Status = AccountStatus.Active,
                }
            );
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Id}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel
            {
                Message = "Become a seller successfully"
            };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot become a seller"
        };
    }

    #region Helper

    private async Task SendVerificationEmail(Account account)
    {
        await _iIEmailHelper.SendEmailAsync(account.Email, "Verify your email",
            $"Your verification code is {account.VerificationCode}. The code will expire in {Constant.VerificationCodeValidityInMinutes} minutes.",
            true);
    }

    private async Task<TokenModel?> GenerateJwtToken(Account account, RefreshToken? refreshToken = null,
        ClaimsPrincipal? principal = null)
    {
        // Refresh token information
        var authClaims = new List<Claim>();
        var deviceId = Guid.NewGuid();
        var refreshTokenString = Guid.NewGuid();
        DateTime expires = DateTime.UtcNow.AddDays(Constant.RefreshTokenValidityInDays);
        var roles = await _unitOfWork.RoleRepository.GetAllByAccountIdAsync(account.Id);

        // If refresh token then reuse the claims
        if (refreshToken != null && principal != null)
        {
            authClaims = principal.Claims
                .Where(claim => claim.Type != ClaimTypes.Role && claim.Type != JwtRegisteredClaimNames.Aud).ToList();
            foreach (var role in roles) authClaims.Add(new Claim(ClaimTypes.Role, role.Name));
            refreshToken.Token = refreshTokenString;
            refreshToken.Expires = expires;
            deviceId = refreshToken.DeviceId;
            _unitOfWork.RefreshTokenRepository.Update(refreshToken);
        }

        // If sign in then add claims
        else
        {
            authClaims.Add(new Claim("accountId", account.Id.ToString()));
            authClaims.Add(new Claim("accountEmail", account.Email));
            authClaims.Add(new Claim("deviceId", deviceId.ToString()));
            authClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            foreach (var role in roles) authClaims.Add(new Claim(ClaimTypes.Role, role.Name));
            await _unitOfWork.RefreshTokenRepository.AddAsync(new RefreshToken
            {
                DeviceId = deviceId,
                Token = refreshTokenString,
                Expires = expires,
                CreatedBy = account
            });
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            var jwtToken = AuthenticationTools.CreateJwtToken(authClaims, _configuration);

            return new TokenModel
            {
                DeviceId = deviceId,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                RefreshToken = refreshTokenString,
                RefreshTokenExpires = expires,
            };
        }

        return null;
    }

    #endregion

    public async Task<ResponseModel> GetVoucher(Guid packageId, decimal? totalPriceOfOrder)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized."
            };
        }

        var artisanPackage =
            await _unitOfWork.PackageRepository.GetAsync(packageId, include: _ => _.Include(_ => _.Service));
        if (artisanPackage == null)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Artisan not found."
            };
        }

        var artisanId = (Guid)artisanPackage.Service.CreatedById;

        var vouchersByArtisan = await _unitOfWork.VoucherRepository.GetAllAsync(
            filter: _ => _.CreatedById == artisanId &&
                         _.VoucherType == Repositories.Enums.VoucherType.ArtistToCustomer &&
                         _.ExpiredTime >= DateTime.UtcNow &&
                         _.StartTime <= DateTime.UtcNow &&
                         _.VoucherStatus == Repositories.Enums.VoucherStatus.Pending
        );

        if (vouchersByArtisan.Data == null || vouchersByArtisan.Data.Count == 0)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Data = null,
                Message = "No vouchers available."
            };
        }

        var completedOrders = await _unitOfWork.OrderRepository.NumberCompletedOrder(currentUserId.Value, artisanId);
        var customer = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value,
            include: _ => _.Include(_ => _.AccountRoles).ThenInclude(_ => _.Role));
        var customerReputation = customer?.AccountRoles
            ?.FirstOrDefault(_ => _.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString())
            ?.TotalReputation ?? 0;

        var voucherModelLists = new List<VoucherModel>();

        foreach (var voucher in vouchersByArtisan.Data)
        {
            var hasUsed =
                await _unitOfWork.VoucherUsageLogRepository.CheckCustomerHasUsedVoucher(voucher.Id,
                    currentUserId.Value, null);

            if (hasUsed)
                continue;

            bool isValid = true;

            if (voucher.MinOrderRequired.HasValue && completedOrders < voucher.MinOrderRequired) isValid = false;

            if (voucher.MinReputation.HasValue && customerReputation < voucher.MinReputation) isValid = false;

            if (voucher.MinOrderValue.HasValue && totalPriceOfOrder.HasValue && totalPriceOfOrder.Value < voucher.MinOrderValue) isValid = false;


            if (!isValid)
                continue;

            voucherModelLists.Add(new VoucherModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                MinOrderValue = voucher.MinOrderValue,
                MaxDiscountValue = voucher.MaxDiscountValue,
                DiscountValue = voucher.DiscountValue,
                StartTime = voucher.StartTime,
                ExpiredTime = voucher.ExpiredTime,
            });
        }

        return new ResponseModel
        {
            Data = voucherModelLists,
            Message = "Vouchers retrieved successfully."
        };
    }

    // ngheej nhan chi duoc su dung 1 voucher cho 1 don hang
    public async Task<ResponseModel> GetVoucherAdmin(Guid orderId)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized."
            };
        }

        var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
        if (order == null)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Order not found."
            };
        }

        var completedOrders = await _unitOfWork.OrderRepository.NumberCompletedOrderOfArtisan(currentUserId.Value);
        var artisan = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value,
            include: _ => _.Include(_ => _.AccountRoles).ThenInclude(_ => _.Role));
        var artisanReputation = artisan?.AccountRoles
                                    ?.FirstOrDefault(_ =>
                                        _.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString())
                                    ?.TotalReputation ??
                                0;
        var check = await _unitOfWork.VoucherUsageLogRepository.CheckOrderHasUsedVoucher(orderId, currentUserId.Value);
        if (check)
        {
            return new ResponseModel
            {
                Data = null,
                Message = "This order has used voucher"
            };
        }
        var allVouchers = await _unitOfWork.VoucherRepository.GetAllAsync(
            filter: _ =>
                _.ExpiredTime >= DateTime.UtcNow &&
                _.StartTime <= DateTime.UtcNow &&
                _.VoucherType == Repositories.Enums.VoucherType.AdminToArtist &&
                _.VoucherStatus == Repositories.Enums.VoucherStatus.Pending &&
                (!_.RemainingQuantity.HasValue || _.RemainingQuantity > 0) &&
                (
                    _.ReceiverId == currentUserId ||
                    (
                        _.ReceiverId == null &&
                        (!_.MinOrderValue.HasValue || ((order.TotalPrice - order.ShippingPrice) >= _.MinOrderValue)) &&
                        (!_.MinOrderRequired.HasValue || completedOrders >= _.MinOrderRequired) &&
                        (!_.MinReputation.HasValue || artisanReputation >= _.MinReputation)
                    )
                ),
            include: _ => _.Include(_ => _.Receiver)
                .Include(_ => _.VoucherUsageLogs)
        );

        if (allVouchers?.Data == null || !allVouchers.Data.Any())
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Data = null,
                Message = "No vouchers available."
            };
        }

        var voucherModelLists = allVouchers.Data
            .Where(_ => !_.VoucherUsageLogs.Any(_ => _.OrderId == orderId && _.CreatedById == currentUserId.Value))
            .Select(_ => new VoucherModel
            {
                Id = _.Id,
                Code = _.Code,
                MinOrderValue = _.MinOrderValue,
                MaxDiscountValue = _.MaxDiscountValue,
                DiscountValue = _.DiscountValue,
                StartTime = _.StartTime,
                ExpiredTime = _.ExpiredTime
            });

        return new ResponseModel
        {
            Data = voucherModelLists,
            Message = "Vouchers retrieved successfully."
        };
    }


    public async Task<ResponseModel> GetSearchHistories()
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (!currentUserId.HasValue)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized."
            };
        }

        var searchHistories =
            await _unitOfWork.SearchHistoryRepository.GetAllAsync(filter: _ => _.CreatedById == currentUserId.Value);
        if (!searchHistories.Data.Any())
        {
            return new ResponseModel { Message = "Not found.", Code = StatusCodes.Status400BadRequest };
        }

        var searchHistoryModels = searchHistories.Data.OrderByDescending(_ => _.CreationDate).Select(_ =>
            new SearchModel
            {
                Id = _.Id,
                SearchText = _.SearchText
            }).ToList();
        return new ResponseModel { Data = searchHistoryModels };
    }

    public async Task<ResponseModel> GetCategoryByArtisan(Guid id, FilterModel filterModel)
    {
        try
        {
            var account = await _unitOfWork.AccountRepository.GetAsync(id);
            if (account == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Account not found."
                };
            }

            Expression<Func<Service, bool>> filter = s => s.CreatedById == id;
            if (!string.IsNullOrEmpty(filterModel.Search))
            {
                var search = filterModel.Search.ToLower();
                filter = s => s.CreatedBy.Id == id &&
                              s.Category.Name != null &&
                              s.Category.Slug != null &&
                              (s.Category.Name.ToLower().Contains(search) ||
                               s.Category.Slug.ToLower().Contains(search));
            }

            var serviceResult = await _unitOfWork.ServiceRepository.GetAllAsync(
                filter: filter,
                include: s => s.Include(x => x.Category),
                pageIndex: filterModel.PageIndex,
                pageSize: filterModel.PageSize
            );

            var categories = serviceResult.Data
                .GroupBy(s => s.Category.Id)
                .Select(g => g.First().Category)
                .Select(c => new CategoryModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    AttachmentUrl = c.AttachmentUrl,
                    AttachmentAlt = c.AttachmentAlt,
                    CreatedById = c.CreatedById,
                    CreationDate = c.CreationDate,
                    ModificationDate = c.ModificationDate,
                    ModifiedById = c.ModifiedById,
                    DeletionDate = c.DeletionDate,
                    IsDeleted = c.IsDeleted,
                })
                .ToList();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get all categories of artisan successfully",
                Data = categories
            };
        }
        catch (Exception ex)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = $"An error occurred while retrieving categories: {ex.Message}"
            };
        }
    }

    public async Task<ResponseModel> GetDefaultShippingAddress(Guid id)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(id);
        if (account == null)
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };

        var shippingAddress =
            await _unitOfWork.ShippingAddressRepository.FindDefaultShippingAddressByAccountIdAsync(id);

        return new ResponseModel()
        {
            Message = "Get default shipping address successfully.",
            Data = _mapper.Map<ShippingAddressModel>(shippingAddress),
        };
    }

    public async Task<ResponseModel> BanAccountRole(BanAccountRoleModel request)
    {
        var account = await _unitOfWork.AccountRepository.GetAsync(request.AccountId,
            a => a.Include(x => x.AccountRoles).ThenInclude(ar => ar.Role));
        if (account == null)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account not found"
            };
        }

        //var accountRole = account.AccountRoles
        //    .FirstOrDefault(ar => ar.Role.Name == request.Role.ToString());

        //if (accountRole == null)
        //{
        //    return new ResponseModel
        //    {
        //        Code = StatusCodes.Status404NotFound,
        //        Message = $"Role {request.Role} not found for this account"
        //    };
        //}
        account.Status = AccountStatus.Suspended;
        account.IsDeleted = true;
        //accountRole.Status = AccountStatus.Suspended;
        //accountRole.ModificationDate = DateTime.UtcNow;

        _unitOfWork.AccountRepository.Update(account);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseModel
        {
            Code = StatusCodes.Status200OK,
            Message = $"Account has been banned successfully"
        };
    }

    public async Task<ResponseModel> DeleteAccountRole(Guid accountId, Guid accountRoleId)
    {
        var accountRole = await _unitOfWork.AccountRoleRepository.GetAsync(accountRoleId);
        if (accountRole == null || accountRole.AccountId != accountId)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account role not found"
            };
        }

        if (accountRole.IsDeleted && accountRole.Status == AccountStatus.Suspended)
        {
            return new ResponseModel
            {
                Message = "Account role has been deleted"
            };
        }

        accountRole.IsDeleted = true;
        accountRole.Status = AccountStatus.Suspended;
        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            return new ResponseModel { Message = "Delete account role successfully" };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot delete account role"
        };
    }

    public async Task<ResponseModel> ToggleAccountRoleStatus(Guid accountId, Guid accountRoleId)
    {
        var accountRole = await _unitOfWork.AccountRoleRepository.GetAsync(accountRoleId, a => a.Include(x => x.Account).ThenInclude(_ => _.Wallet).Include(x => x.Role)); ;
        if (accountRole == null || accountRole.AccountId != accountId)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = "Account role not found"
            };
        }

        if (!accountRole.IsDeleted && accountRole.Status == AccountStatus.Active)
        {
            if (accountRole.Role.Name == "Artisan")
            {
                var services = await _unitOfWork.ServiceRepository.GetAllAsync(filter: _ => _.CreatedById == accountRole.AccountId);
                foreach (var service in services.Data)
                {
                    service.IsDeleted = true;
                    _unitOfWork.ServiceRepository.Update(service);
                }
                var orders = await _unitOfWork.OrderRepository.GetAllAsync(filter: _ => _.Package.CreatedById == accountRole.AccountId && (_.Status == OrderStatus.Pending || _.Status == OrderStatus.Accepted), include: _ => _.Include(_ => _.Package).ThenInclude(_ => _.Service).Include(_ => _.CreatedBy).ThenInclude(_ => _.Wallet));
                foreach (var order in orders.Data)
                {
                    var customerAccount = order.CreatedBy;
                    var transaction = new Transaction
                    {
                        Amount = order.TotalPrice,
                        Type = TransactionType.TransferIn,
                        CreatedById = accountRole.AccountId,
                        Status = TransactionStatus.Completed,
                        WalletId = customerAccount.Wallet.Id,
                        Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Customer)
                    };
                    customerAccount.Wallet.Balance += (decimal)order.TotalPrice;
                    order.Transactions.Add(transaction);
                    order.Status = OrderStatus.Cancelled;
                    order.Stage = OrderStage.Cancelled;
                    order.SystemCancelReason = SystemCancelReason.BanArtist;
                    _unitOfWork.OrderRepository.Update(order);
                    _unitOfWork.AccountRepository.Update(customerAccount);
                }

            }
            accountRole.Status = AccountStatus.Suspended;
            accountRole.IsDeleted = true;
        }
        else if (accountRole.IsDeleted && accountRole.Status == AccountStatus.Suspended)
        {
            if (accountRole.Role.Name == "Artisan")
            {
                var services = await _unitOfWork.ServiceRepository.GetAllAsync(filter: _ => _.CreatedById == accountRole.AccountId);
                foreach (var service in services.Data)
                {
                    service.IsDeleted = false;
                    _unitOfWork.ServiceRepository.Update(service);
                }
            }
            accountRole.Status = AccountStatus.Active;
            accountRole.IsDeleted = false;
        }

        if (await _unitOfWork.SaveChangeAsync() > 0)
        {
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{accountRole.AccountId}");
            await _redisHelper.InvalidateCacheByPatternAsync($"account_{accountRole.Account.Username}");
            await _redisHelper.InvalidateCacheByPatternAsync("accounts_*");

            return new ResponseModel { Message = "Toggle account role status successfully" };
        }

        return new ResponseModel
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Cannot toggle account role status"
        };
    }
    public async Task<ResponseDashboardModel<ArtisanDashboardModel>> GetArtisanDashboard(DashboardFilterModel dashboardFilterModel)
    {
        var currentUserId = _claimService.GetCurrentUserId!.Value;
        var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId, include: a => a.Include(a => a.Wallet));
        // Create a date range for the specified month and year
        var startDate = DateTime.SpecifyKind(new DateTime(dashboardFilterModel.Year, dashboardFilterModel.Month, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
        var today = DateTime.UtcNow.Date;


        int totalOrder = await _unitOfWork.Context.Orders
            .Where(x => x.Package.CreatedById == currentUserId && !x.IsDeleted)
            .CountAsync();

        int totalPendingOrder = await _unitOfWork.Context.Orders
            .Where(x => x.Package.CreatedById == currentUserId && !x.IsDeleted)
            .CountAsync();

        int totalOrderPerDay = await _unitOfWork.Context.Orders
            .Where(x => x.Package.CreatedById == currentUserId &&
                        x.Status == OrderStatus.Pending &&
                        !x.IsDeleted &&
                        x.CreationDate.Date == today)
            .CountAsync();

        int totalActiveOrder = await _unitOfWork.Context.Orders
            .Where(x => x.Package.CreatedById == currentUserId && x.Status == OrderStatus.Pending && !x.IsDeleted)
            .CountAsync();

        int totalCancelOrder = await _unitOfWork.Context.Orders
          .Where(x => x.Package.CreatedById == currentUserId && x.Status == OrderStatus.Cancelled && !x.IsDeleted)
          .CountAsync();

        int totalDeliveredOrder = await _unitOfWork.Context.Orders
            .Where(x => !x.IsDeleted && x.Package.CreatedById == currentUserId && x.Status == OrderStatus.Completed)
            .CountAsync();

        decimal totalRevenue = (decimal)await _unitOfWork.Context.Transaction
            .Where(x => !x.IsDeleted && x.Type == TransactionType.TransferIn && x.Status == TransactionStatus.Completed && x.CreationDate >= startDate &&
                        x.CreationDate <= endDate && x.Order.Package.CreatedById == currentUserId && x.WalletId == account!.WalletId)
            .SumAsync(x => x.Amount);

        // Get the sum of transactions for each day within the specified month and year
        var earningsGroupedByDate = await _unitOfWork.Context.Transaction
            .Where(x => !x.IsDeleted && x.Type == TransactionType.TransferIn && x.Status == TransactionStatus.Completed && x.CreationDate >= startDate &&
                        x.CreationDate <= endDate && x.Order.Package.CreatedById == currentUserId && x.WalletId == account!.WalletId)
           .GroupBy(x => x.CreationDate.ToUniversalTime().Date)
            .Select(g => new
            {
                Date = DateOnly.FromDateTime(g.Key),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        // Create a list of all days in the month
        var allDaysInMonth = Enumerable.Range(0,
                DateTime.DaysInMonth(dashboardFilterModel.Year, dashboardFilterModel.Month))
            .Select(day => new DateOnly(dashboardFilterModel.Year, dashboardFilterModel.Month, day + 1))
            .ToList();

        // Create a list of Earning objects, ensuring all days are included
        List<Earning> earnings = allDaysInMonth
            .Select(day => new Earning
            {
                Date = day,
                Amount = earningsGroupedByDate.FirstOrDefault(e => e.Date == day)?.TotalAmount ?? 0
            })
            .ToList();

        // Return the assembled dashboard data
        return new ResponseDashboardModel<ArtisanDashboardModel>
        {
            Message = "Get admin dashboard successfully",
            Code = StatusCodes.Status200OK,
            Data = new ArtisanDashboardModel
            {
                TotalOrder = totalOrder,
                TotalRevenue = totalRevenue,
                TotalOrderPerDay = totalOrderPerDay,
                TotalPendingOrder = totalPendingOrder,
                TotalActiveOrder = totalActiveOrder,
                TotalCancelOrder = totalCancelOrder,
                TotalDeliveredOrder = totalDeliveredOrder,
                Earnings = earnings
            }
        };
    }

    public async Task<ResponseModel> GetAllAccount(AccountFilterModel accountFilterModel)
    {
        var accounts = await _unitOfWork.AccountRepository.GetAllAsync(
            account =>
                (!accountFilterModel.IsDeleted.HasValue || account.IsDeleted == accountFilterModel.IsDeleted) &&
                (!accountFilterModel.Role.HasValue || account.AccountRoles.Where(accountRole =>
                        !accountFilterModel.Status.HasValue || accountRole.Status == accountFilterModel.Status)
                    .Select(accountRole => accountRole.Role.Name)
                    .Contains(accountFilterModel.Role.ToString())),
            accounts =>
            {
                switch (accountFilterModel.Order.ToLower())
                {
                    case "firstname":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.FirstName)
                            : accounts.OrderBy(account => account.FirstName);
                    case "lastname":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.LastName)
                            : accounts.OrderBy(account => account.LastName);
                    case "dateofbirth":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.DateOfBirth)
                            : accounts.OrderBy(account => account.DateOfBirth);
                    case "email":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Email)
                            : accounts.OrderBy(account => account.Email);
                    case "phonenumber":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.PhoneNumber)
                            : accounts.OrderBy(account => account.PhoneNumber);
                    case "isdeleted":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.IsDeleted)
                            : accounts.OrderBy(account => account.IsDeleted);
                    case "gender":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Gender)
                            : accounts.OrderBy(account => account.Gender);
                    case "username":
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.Username)
                            : accounts.OrderBy(account => account.Username);
                    default:
                        return accountFilterModel.OrderByDescending
                            ? accounts.OrderByDescending(account => account.CreationDate)
                            : accounts.OrderBy(account => account.CreationDate);
                }
            },
            accounts => accounts
                .Include(account => account.ShippingAddresses)
                .Include(account => account.AccountRoles)
                .ThenInclude(accountRole => accountRole.Role)
                .Include(account => account.Wallet)
                .Include(account => account.Orders)
                .Include(account => account.Services)
        );
        var accountModels = _mapper.Map<List<AccountModel>>(accounts.Data);
        return new ResponseModel
        {
            Message = "Get all accounts successfully",
            Data = accountModels
        };
    }
    public async Task<ResponseDashboardModel<AdminDashboardModel>> GetAdminDashboard(DashboardFilterModel dashboardFilterModel)
    {
        var currentUserId = _claimService.GetCurrentUserId!.Value;
        var now = DateTime.UtcNow;
        var today = now.Date;
        var lastMonth = now.AddMonths(-1);
        var month = now.Month;
        var year = now.Year;


        var currentRevenue = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Month == now.Month && o.CreationDate.Year == now.Year && o.Stage == OrderStage.Completed && o.Status == OrderStatus.Completed && o.IsDeleted == false)
            .SumAsync(o => o.TotalPrice - o.ShippingPrice) ?? 0;

        var totalMoneyOfOrder = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Year == now.Year && !o.IsDeleted)
            .SumAsync(o =>
                (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Accepted)
                ? o.TotalPrice
                : 0
            );
        var totalMoneyOfPlatform = await _unitOfWork.Context.Orders
      .Where(o => o.CreationDate.Date == today && o.CreationDate.Year == now.Year && !o.IsDeleted)
      .SumAsync(o =>
          (o.Status == OrderStatus.Completed && o.Stage == OrderStage.Completed)
          ? (o.AdminCommUsedVch ?? o.AdminCommDefault)
          : 0
      );


        var lastMonthRevenue = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Month == lastMonth.Month && o.CreationDate.Year == lastMonth.Year && o.Stage == OrderStage.Completed && o.Status == OrderStatus.Completed && o.IsDeleted == false)
            .SumAsync(o => o.TotalPrice - o.ShippingPrice) ?? 0;

        var customerCount = await _unitOfWork.Context.Accounts.Include(u => u.AccountRoles)
        .ThenInclude(ar => ar.Role).CountAsync(u => u.AccountRoles.Any(r => r.Role.Name.Equals(Role.Customer.ToString())));
        var artisanCount = await _unitOfWork.Context.Accounts.Include(u => u.AccountRoles)
        .ThenInclude(ar => ar.Role).CountAsync(u => u.AccountRoles.Any(r => r.Role.Name.Equals(Role.Artisan.ToString())));
        var currentAccountCount = await _unitOfWork.Context.Accounts.CountAsync(u => u.CreationDate.Date == today);
        var yesterdayAccountCount = await _unitOfWork.Context.Accounts.CountAsync(u => u.CreationDate.Date == today.AddDays(-1));


        var totalOrder = await _unitOfWork.Context.Orders.CountAsync();
        var totalCompleteOrder = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Completed && o.Stage == OrderStage.Completed).CountAsync();
        var totalCancelOrder = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Cancelled).CountAsync();
        var totalAcceptedOrders = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Accepted).CountAsync();
        var totalPendingOrders = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Pending).CountAsync();
        var totalRejectedOrders = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Rejected).CountAsync();
        var totalRefundedOrders = await _unitOfWork.Context.Orders.Where(o => o.Status == OrderStatus.Refunded).CountAsync();

        var currentOrderCount = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Date == today)
            .CountAsync();

        var yesterdayOrderCount = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Date == today.AddDays(-1))
        .CountAsync();

        var allMonthsInYear = Enumerable.Range(1, 12)
      .Select(month => new DateOnly(dashboardFilterModel.Year, month, 1))
      .ToList();

        //var startOfYear = new DateTime(dashboardFilterModel.Year, dashboardFilterModel.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(dashboardFilterModel.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var endOfYear = startOfYear.AddYears(1);
        var completedOrders = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate >= startOfYear && o.CreationDate < endOfYear
                        && !o.IsDeleted && o.Status == OrderStatus.Completed && o.Stage == OrderStage.Completed)
            .ToListAsync();

        var canceledOrders = await _unitOfWork.Context.Orders
            .Where(o => o.CreationDate >= startOfYear && o.CreationDate < endOfYear
                        && !o.IsDeleted && o.Status == OrderStatus.Cancelled && o.Stage == OrderStage.Cancelled && o.ArtistRevenueAfterCancel > 0)
            .ToListAsync();

        var canceledRevenueByMonth = canceledOrders
            .GroupBy(o => new { o.CreationDate.Year, o.CreationDate.Month })
            .ToDictionary(
                g => new DateOnly(g.Key.Year, g.Key.Month, 1),
                g => g.Sum(x => x.ArtistRevenueAfterCancel ?? 0)
            );

        var revenueByMonth = completedOrders
            .GroupBy(o => new { o.CreationDate.Year, o.CreationDate.Month })
            .Select(g =>
            {
                var monthKey = new DateOnly(g.Key.Year, g.Key.Month, 1);
                var revenueAfterCancel = canceledRevenueByMonth.TryGetValue(monthKey, out var val) ? val : 0;

                return new
                {
                    Month = monthKey,
                    TotalPriceWithoutShipFee = g.Sum(x => x.TotalPrice - x.ShippingPrice),
                    TotalPlatformFee = g.Sum(x => x.AdminCommUsedVch ?? x.AdminCommDefault),
                    TotalArtisanRevenue = g.Sum(x => x.ArtistRevenue ?? 0) + revenueAfterCancel
                };
            })
            .ToList();


        var artisanWithHighestRevenues = _unitOfWork.Context.Orders
    .Where(o => o.Status == OrderStatus.Completed
             && o.Stage == OrderStage.Completed
             && !o.IsDeleted
             && (o.Package.Service != null || o.Package.Offer != null))
    .Select(o => new
    {
        ArtisanId = o.Package.Service != null ? o.Package.Service.CreatedById : o.Package.Offer.CreatedById,
        ArtisanName = o.Package.Service != null ? (o.Package.Service.CreatedBy.LastName + o.Package.Service.CreatedBy.FirstName) :
                                                    o.Package.Offer.CreatedBy.LastName + o.Package.Offer.CreatedBy.FirstName,
        Image = o.Package.Service != null ? (o.Package.Service.CreatedBy.Image) : "",
        Revenue = o.ArtistRevenue ?? 0
    })
    .GroupBy(x => new { x.ArtisanId, x.ArtisanName, x.Image })
    .Select(g => new ArtisanWithHighestRevenue
    {
        Image = g.Key.Image,
        ArtisanId = (Guid)g.Key.ArtisanId,
        ArtisanName = g.Key.ArtisanName,
        TotalRevenue = g.Sum(x => x.Revenue)
    })
    .OrderByDescending(x => x.TotalRevenue)
    .Take(10)
    .ToList();

        List<RevenueChart> revenueCharts = allMonthsInYear.Select(month =>
        {
            var data = revenueByMonth.FirstOrDefault(r => r.Month == month);
            return new RevenueChart
            {
                Month = $"Tháng {month.Month}",
                TotalPriceWithoutShipFee = data?.TotalPriceWithoutShipFee ?? 0,
                TotalPlatformFee = data?.TotalPlatformFee ?? 0,
                TotalArtisanRevenue = data?.TotalArtisanRevenue ?? 0
            };
        }).ToList();


        return new ResponseDashboardModel<AdminDashboardModel>
        {
            Message = "Get admin dashboard successfully",
            Code = StatusCodes.Status200OK,
            Data = new AdminDashboardModel
            {
                Orders = new OrderStat
                {
                    Total = totalOrder,
                    ChangePercentage = CalculateChange(currentOrderCount, yesterdayOrderCount),
                    TotalCompleteOrder = totalCompleteOrder,
                    TotalCancelOrders = totalCancelOrder,
                    TotalAcceptedOrders = totalAcceptedOrders,
                    TotalPendingOrders = totalPendingOrders,
                    TotalRefundedOrders = totalRefundedOrders,
                    TotalRejectedOrders = totalRejectedOrders
                },
                Revenue = new RevenueStat
                {
                    Total = currentRevenue,
                    TotalMoneyOfPlatForm = (totalMoneyOfPlatform + totalMoneyOfOrder) ?? 0,
                    ChangePercentage = CalculateRevenueChange(currentRevenue, lastMonthRevenue),
                },
                Users = new UserStat
                {
                    Total = artisanCount + customerCount,
                    ChangePercentage = CalculateChange(currentAccountCount, yesterdayAccountCount),
                    Details = new UserDetail
                    {
                        Artisan = artisanCount,
                        Customer = customerCount
                    }

                },
                RevenueCharts = revenueCharts,
                ArtisanWithHighestRevenues = artisanWithHighestRevenues
            }
        };

    }

    public async Task<ResponseModel> GetRevenueByMonth(DashboardFilterModel dashboardFilterModel)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var lastMonth = now.AddMonths(-1);
        var month = now.Month;
        var year = now.Year;

        var monthlyRevenue = _unitOfWork.Context.Orders
            .Where(o => o.CreationDate.Year == dashboardFilterModel.Year && o.Stage == OrderStage.Completed && o.Status == OrderStatus.Completed && o.IsDeleted == false) // Lấy đơn hàng trong năm hiện tại
            .GroupBy(o => o.CreationDate.Month)          // Nhóm theo tháng
            .Select(g => new
            {
                Month = g.Key,
                Total = g.Sum(o => o.TotalPrice - o.ShippingPrice) // hoặc trường tổng tiền bạn lưu
            })
            .OrderBy(r => r.Month) // Sắp xếp theo tháng
            .ToList();




        var months = new[]
{
    "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
    "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
};

        var result = Enumerable.Range(1, 12)
            .Select(i => new
            {
                name = months[i - 1],
                total = monthlyRevenue.FirstOrDefault(x => x.Month == i)?.Total ?? 0
            })
            .ToList();


        return new ResponseModel
        {
            Message = "Get admin dashboard successfully",
            Code = StatusCodes.Status200OK,
            Data = result
        };

    }

    public async Task<ResponseModel> GetRevenueByCategory(DashboardFilterModel dashboardFilterModel)
    {
        var orders = _unitOfWork.Context.Orders
    .Where(o => o.CreationDate.Year == dashboardFilterModel.Year && o.Stage == OrderStage.Completed && o.Status == OrderStatus.Completed && o.IsDeleted == false)
    .Include(x => x.Package)
        .ThenInclude(p => p.Service)
            .ThenInclude(s => s.Category)
                .ThenInclude(c => c.Parent)
    .Include(x => x.Package)
        .ThenInclude(p => p.Offer)
            .ThenInclude(o => o.Service)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c.Parent)
    .ToList();


        var revenueByRootCategory = orders
    .Select(order =>
    {
        var service = order.Package.Service ?? order.Package.Offer?.Service;
        var category = service?.Category;

        // Truy ngược đến category gốc (Parent == null)
        while (category?.Parent != null)
        {
            category = category.Parent;
        }

        return new
        {
            RootCategoryName = category?.Name,
            Revenue = (order.TotalPrice - order.ShippingPrice) ?? 0
        };
    })
    .Where(x => x.RootCategoryName != null)
    .GroupBy(x => x.RootCategoryName)
    .Select(g => new
    {
        name = g.Key!,
        total = g.Sum(x => x.Revenue)
    })
    .OrderByDescending(x => x.total)
    .Take(5)
    .ToList();



        return new ResponseModel
        {
            Message = "Get admin dashboard successfully",
            Code = StatusCodes.Status200OK,
            Data = revenueByRootCategory
        };

    }
    private double CalculateChange(double current, double previous)
    {
        if (previous == 0) return 100;
        return Math.Round(((current - previous) / previous) * 100, 2);
    }
    private double CalculateRevenueChange(decimal current, decimal previous)
    {
        if (previous == 0) return 100;
        return Math.Round((double)((current - previous) / previous) * 100, 2);
    }

}
