using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.Services.Interfaces;

public interface IAccountService
{
    Task<ResponseModel> SignUp(AccountSignUpModel accountSignUpModel);
    Task<ResponseModel> SignIn(AccountSignInModel accountSignInModel);
    Task<ResponseModel> SignInGoogle(string code);
    Task<ResponseModel> RefreshToken(AccountRefreshTokenModel accountRefreshTokenModel);
    Task<ResponseModel> RevokeTokens(AccountEmailModel accountEmailModel);
    Task<ResponseModel> VerifyEmail(string email, string verificationCode);
    Task<ResponseModel> SendVerifyPhone(string email);
    Task<ResponseModel> ResendVerificationEmail(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ChangePassword(AccountChangePasswordModel accountChangePasswordModel);
    Task<ResponseModel> ForgotPassword(AccountEmailModel accountEmailModel);
    Task<ResponseModel> ResetPassword(AccountResetPasswordModel accountResetPasswordModel);
    Task<ResponseModel> AddRange(AccountAddRangeModel accountAddRangeModel);
    Task<ResponseModel> Get(string idOrUsername);
    Task<ResponseModel> GetAll(AccountFilterModel accountFilterModel);
    Task<ResponseModel> Update(Guid id, AccountUpdateModel accountUpdateModel);
    Task<ResponseModel> UpdateRoles(Guid id, AccountUpdateRolesModel accountUpdateRolesModel);
    Task<ResponseModel> Delete(Guid id);
    Task<ResponseModel> Restore(Guid id);
    Task<ResponseModel> BecomeASeller(Guid id, AccountBecomeASellerModel accountBecomeASellerModel);
    Task<ResponseModel> GetVoucher(Guid packageId, decimal? totalPriceOfOrder);
    Task<ResponseModel> GetVoucherAdmin(Guid orderId);
    Task<ResponseModel> GetSearchHistories();
    Task<ResponseModel> GetCategoryByArtisan(Guid id, FilterModel filterModel);
    Task<ResponseModel> GetDefaultShippingAddress(Guid id);
    Task<ResponseModel> BanAccountRole(BanAccountRoleModel request);
    Task<ResponseModel> DeleteAccountRole(Guid accountId, Guid accountRoleId);
    Task<ResponseModel> RestoreAccountRole(Guid accountId, Guid accountRoleId);
}