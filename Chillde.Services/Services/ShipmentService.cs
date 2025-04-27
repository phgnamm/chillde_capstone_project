using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Chillde.Services.Models.AccountModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Chillde.Services.Models.ShipmentStatusHistoryModels;
using Microsoft.EntityFrameworkCore.Storage;
using Chillde.Services.Helpers;
namespace Chillde.Services.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _ghtkUrl;
        private readonly string _ghtkUsername;
        private readonly string _ghtkPassword;

        public ShipmentService(IHttpClientFactory httpClientFactory,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _unitOfWork = unitOfWork;
            _ghtkUrl = configuration["GhtkSettings:BaseUrl"]!;
            _ghtkUsername = configuration["GhtkSettings:Username"]!;
            _ghtkPassword = configuration["GhtkSettings:Password"]!;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel requestModel)
        {
            var url = $"https://services.giaohangtietkiem.vn/services/shipment/fee?" +
              $"address={Uri.EscapeDataString(requestModel.Address ?? string.Empty)}&" +
              $"province={Uri.EscapeDataString(requestModel.Province)}&" +
              $"district={Uri.EscapeDataString(requestModel.District)}&" +
              $"pick_province={Uri.EscapeDataString(requestModel.PickProvince)}&" +
              $"pick_district={Uri.EscapeDataString(requestModel.PickDistrict)}&" +
              $"pick_ward={Uri.EscapeDataString(requestModel.PickWard ?? string.Empty)}&" +
              $"pick_address={Uri.EscapeDataString(requestModel.PickAddress ?? string.Empty)}&" +
              $"ward={Uri.EscapeDataString(requestModel.Ward ?? string.Empty)}&" +
              $"transport={Uri.EscapeDataString(requestModel.Transport ?? string.Empty)}&" +
              $"weight={requestModel.Weight}&" +
              $"value={requestModel.Value}&" +
              $"deliver_option={requestModel.DeliverOption}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Headers.Add("Token", "140QYIuQUX4Fh3GvqhpzC2yFCTb8Zsqu3hPO3rh");

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = $"API error: {errorContent}",
                        Data = null
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

                var shipmentData = jsonObject?["fee"]?.ToObject<ShippingFeeResponseModel>();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = shipmentData
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Exception: {ex.Message}",
                    Data = null
                };
            }
        }


        public async Task<CancelShipmentResponseModel> CancelShipmentAsync(string trackingOrder)
        {
            var url = $"{_ghtkUrl}/shipment/cancel/{trackingOrder}";
            var shipment = _unitOfWork.ShipmentRepository.GetShipmentByPartnerIdOrLabel(trackingOrder);
            if (shipment == null)
            {
                return new CancelShipmentResponseModel
                {
                    Success = false,
                    Message = "Shipment not found.",
                    LogId = null
                };
            }
            var cancellableStatuses = new[] { ShipmentStatus.NotReceived, ShipmentStatus.Received, ShipmentStatus.PickupArranging };
            if (!cancellableStatuses.Contains(shipment.CurrentStatusId))
            {
                return new CancelShipmentResponseModel
                {
                    Success = false,
                    Message = "Shipment cannot be cancelled at this status.",
                    LogId = null
                };
            }
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                //response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var cancelResponse = JsonConvert.DeserializeObject<CancelShipmentResponseModel>(content);

                //var shipment = _unitOfWork.ShipmentRepository.GetShipmentByPartnerIdOrLabel(trackingOrder);
                shipment!.CurrentStatusId = ShipmentStatus.Cancelled;

                var statusHistory = new ShipmentStatusHistory
                {
                    ShipmentId = shipment.Id,
                    StatusId = ShipmentStatus.Cancelled,
                };
                shipment.ShipmentStatusHistorys.Add(statusHistory);
                _unitOfWork.ShipmentRepository.Update(shipment);
                await _unitOfWork.SaveChangeAsync();

                if (cancelResponse != null) return cancelResponse;
            }
            catch (HttpRequestException ex)
            {
                return new CancelShipmentResponseModel
                {
                    Success = false,
                    Message = ex.Message,
                    LogId = null
                };
            }

            return null!;
        }

        //public async Task<ResponseModel> GetOrderStatusAsync(string trackingOrder)
        //{
        //    if (string.IsNullOrEmpty(trackingOrder))
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Tracking order is required.",
        //            Data = null
        //        };
        //    }

        //    var url = $"{_ghtkUrl}/shipment/v2/{trackingOrder}";
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        //    try
        //    {
        //        var response = await _httpClient.SendAsync(requestMessage);
        //        response.EnsureSuccessStatusCode();

        //        var content = await response.Content.ReadAsStringAsync();
        //        var jsonObject = JsonConvert.DeserializeObject<JObject>(content);

        //        if (jsonObject?["success"]?.Value<bool>() == true)
        //        {
        //            var orderStatusResponse = jsonObject["order"]?.ToObject<OrderStatusResponseModel>();
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status200OK,
        //                Message = "Success",
        //                Data = orderStatusResponse
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status400BadRequest,
        //                Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
        //                Data = loginContent
        //            };
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = ex.Message,
        //            Data = null
        //        };
        //    }
        //}

        public async Task<ResponseModel> GetOrderStatusAsync(string trackingOrder)
        {
            if (string.IsNullOrEmpty(trackingOrder))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Tracking order is required.",
                    Data = null
                };
            }

            var ghtkLoginUrl = "https://web-staging.ghtklab.com/api/v1/auth/login";
            var loginPayload = new
            {
                username = _ghtkUsername,
                password = _ghtkPassword,
                new_version = "true"
            };

            var ghtkJwtToken = _httpContextAccessor.HttpContext?.Session.GetString("GhtkToken");
            if (!CheckGHTKJwtValidity(ghtkJwtToken))
            {
                var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, ghtkLoginUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json")
                };
                var loginResponse = await _httpClient.SendAsync(loginRequestMessage);
                var loginContent = await loginResponse.Content.ReadAsStringAsync();
                var jwt = JsonConvert.DeserializeObject<GHTKAccountInfo>(loginContent)!.Data!.Jwt;
                ghtkJwtToken = JsonConvert.DeserializeObject<GHTKAccountInfo>(loginContent)!.Data!.Jwt;
                _httpContextAccessor.HttpContext?.Session.SetString("GhtkToken", ghtkJwtToken);
            }

            var url = $"https://web-staging.ghtklab.com/api/v1/package/package-detail?alias={trackingOrder}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ghtkJwtToken);


            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<ShipmentStatusResponseModel>(content);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = jsonObject
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<byte[]> GetShippingLabelAsync(string trackingOrder)
        {
            string url = $"{_ghtkUrl}/label/{trackingOrder}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                throw new Exception($"Lỗi khi in nhãn đơn hàng: {await response.Content.ReadAsStringAsync()}");
            }
        }

        public bool CheckGHTKJwtValidity(string ghtkJwtToken)
        {
            if (string.IsNullOrEmpty(ghtkJwtToken))
            {
                return false;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var token = handler.ReadJwtToken(ghtkJwtToken);
                var payload = token.Payload as IDictionary<string, object>;

                if (token == null)
                {
                    return false;
                }

                var invalidAt = payload["invalid_at"].ToString();
                using JsonDocument doc = JsonDocument.Parse(invalidAt);
                string dateTimeInvalid = doc.RootElement.GetProperty("date").GetString();

                if (DateTime.Parse(dateTimeInvalid) < DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UpdateShipmentStatusAsync(ShipmentUpdateRequestModel request)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                var shipment = await _unitOfWork.ShipmentRepository.GetByTrackingIdAsync(request.LabelId);
                if (shipment == null)
                {
                    return false;
                }

                if (shipment.PartnerId != request.PartnerId)
                {
                    return false;
                }

                if (!Enum.IsDefined(typeof(ShipmentStatus), request.StatusId))
                {
                    throw new ArgumentException($"StatusId {request.StatusId} không hợp lệ.");
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(
                    shipment.OrderId,
                    include: o => o.Include(o => o.Package).ThenInclude(p => p.Service)
                );
                if (order == null)
                {
                    return false;
                }

                bool isReturn = !string.IsNullOrEmpty(request.PartnerId) && request.PartnerId.Contains("_Return_");

                shipment.CurrentStatusId = (ShipmentStatus)request.StatusId;
                var history = new ShipmentStatusHistory
                {
                    ShipmentId = shipment.Id,
                    StatusId = (ShipmentStatus)request.StatusId,
                };
                shipment.ShipmentStatusHistorys.Add(history);
                _unitOfWork.ShipmentRepository.Update(shipment);

                if (request.StatusId == (int)ShipmentStatus.Reconciled)
                {
                    if (isReturn)
                    {
                        var customerAccount = await _unitOfWork.AccountRepository.GetAsync(
                            (Guid)order.CreatedById!,
                            include: a => a.Include(a => a.Wallet)
                        );
                        if (customerAccount == null || customerAccount.Wallet == null)
                        {
                            return false;
                        }

                        if (order.Package?.Service?.CreatedById == null)
                        {
                            return false;
                        }

                        // Lấy tài khoản nghệ nhân
                        var artisanAccount = await _unitOfWork.AccountRepository.GetAsync(
                            (Guid)order.Package.Service.CreatedById,
                            include: a => a.Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                        );
                        if (artisanAccount == null)
                        {
                            return false;
                        }

                        var accountRoleArtisan = artisanAccount.AccountRoles
                            .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());
                        if (accountRoleArtisan == null)
                        {
                            return false;
                        }

                        // Hoàn tiền cho khách
                        var wallet = customerAccount.Wallet;
                        wallet.Balance += (decimal)order.TotalPrice!;
                        _unitOfWork.WalletRepository.Update(wallet);

                        order.Transactions.Add(new Transaction
                        {
                            WalletId = wallet.Id,
                            Amount = order.TotalPrice,
                            Type = TransactionType.TransferIn,
                            Status = TransactionStatus.Completed,
                            CreatedById = customerAccount.Id,
                            Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Customer)
                        });

                        // Trừ điểm uy tín nghệ nhân
                        if (accountRoleArtisan.TotalReputation > 0)
                        {
                            accountRoleArtisan.TotalReputation = Math.Max(0, accountRoleArtisan.TotalReputation - 5);
                            _unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);

                            var artisanReputationLog = new ReputationLog
                            {
                                PointChange = -5,
                                Reason = $"Đơn hàng trả lại {order.Code} do lỗi vận chuyển",
                                OrderId = order.Id,
                                AccountRoleId = accountRoleArtisan.Id,
                                CreatedById = artisanAccount.Id
                            };
                            await _unitOfWork.ReputationLogRepository.AddAsync(artisanReputationLog);
                        }
                    }
                    else
                    {
                        order.Stage = OrderStage.AwaitingClosure;
                        _unitOfWork.OrderRepository.Update(order);
                    }
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var saveResult = await _unitOfWork.SaveChangeAsync();
                    if (saveResult <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return false;
                    }

                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResponseModel> GetALlShipmentAsync(ShipmentFilterModel model)
        {
            Expression<Func<Shipment, bool>> filter = s =>
               s.IsDeleted == model.IsDeleted &&
               (string.IsNullOrEmpty(model.Search) ||
                s.TrackingId!.Contains(model.Search) ||
                s.PartnerId!.Contains(model.Search) ||
                s.Label!.Contains(model.Search) ||
                s.Fee!.Equals(model.Search));

            var shipmentdetails = await _unitOfWork.ShipmentRepository.GetAllAsync(
            filter: filter,
            pageIndex: model.PageIndex,
            pageSize: model.PageSize
        );

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = shipmentdetails.Data
            };
        }

        public async Task<ResponseModel> GetAllStatusByShipmentId(Guid shipmentId, ShipmentStatusFilterModel filterModel)
        {
            var shipment = await _unitOfWork.ShipmentRepository.GetAsync(shipmentId);
            if (shipment == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy shipment."
                };
            }

            Expression<Func<ShipmentStatusHistory, bool>> filter = s =>
                s.ShipmentId == shipmentId && s.IsDeleted == filterModel.IsDeleted;

            var statusHistories = await _unitOfWork.ShipmentStatusHistoryRepository.GetAllAsync(
                filter: filter,
                order: q =>
                {
                    return filterModel.OrderByDescending
                        ? q.OrderByDescending(s => s.CreationDate)
                        : q.OrderBy(s => s.CreationDate);
                },
                pageIndex: filterModel.PageIndex,
                pageSize: filterModel.PageSize
            );

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get all status list successfully",
                Data = statusHistories.Data.Select(x => new
                {
                    x.StatusId,
                    x.CreationDate
                }),
            };
        }

        public async Task<ResponseModel> GetShipmentByOrderIdAsync(Guid orderId)
        {
            try
            {
                if (orderId == Guid.Empty)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid order ID."
                    };
                }

                var shipment = await _unitOfWork.ShipmentRepository.GetByOrderIdAsync(orderId);

                if (shipment == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "No shipment found for the specified order."
                    };
                }

                var shipmentResponse = new ShipmentDetailModel
                {
                    TrackingId = shipment.TrackingId,
                    CurrentStatusId = shipment.CurrentStatusId,
                    PartnerId = shipment.PartnerId,
                    EstimatedPickTime = shipment.EstimatedPickTime,
                    EstimatedDeliverTime = shipment.EstimatedDeliverTime,
                    ProductShipments = shipment.ProductShipments.Select(ps => new ProductShipmentResponse
                    {
                        Name = ps.Name,
                        Quantity = ps.Quantity
                    }).ToList()
                };

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Shipment retrieved successfully.",
                    Data = shipmentResponse
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error retrieving shipment: {ex.Message}"
                };
            }
        }
        public async Task<ResponseModel> SeedShipmentStatusHistoryAsync(Guid shipmentId)
        {
            try
            {
                if (shipmentId == Guid.Empty)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid shipment ID."
                    };
                }

                var shipment = await _unitOfWork.ShipmentRepository.GetAsync(
                    shipmentId,
                    include: q => q.Include(s => s.ShipmentStatusHistorys)
                );
                if (shipment == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Shipment with ID {shipmentId} not found."
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(
                    shipment.OrderId,
                    include: o => o.Include(o => o.Package).ThenInclude(p => p.Service)
                );
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Order with ID {shipment.OrderId} not found."
                    };
                }

                if (order.Stage == OrderStage.Cancelled)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot seed status for a cancelled order."
                    };
                }

                var statusSequence = new[]
                {
                    ShipmentStatus.Received,
                    ShipmentStatus.PickupArranging,
                    ShipmentStatus.PickedUp,
                    ShipmentStatus.Delivering,
                    ShipmentStatus.DeliveredNotReconciled,
                    ShipmentStatus.Reconciled
                };

                var baseTime = DateTime.UtcNow;
                int startIndex = shipment.CurrentStatusId == ShipmentStatus.NotReceived ? 0 : 1;

                for (int i = startIndex; i < statusSequence.Length; i++)
                {
                    var history = new ShipmentStatusHistory
                    {
                        ShipmentId = shipment.Id,
                        StatusId = statusSequence[i],
                        CreationDate = baseTime.AddHours(i - startIndex)
                    };
                    shipment.ShipmentStatusHistorys.Add(history);
                }

                shipment.CurrentStatusId = ShipmentStatus.Reconciled;
                _unitOfWork.ShipmentRepository.Update(shipment);

                bool isReturn = !string.IsNullOrEmpty(shipment.PartnerId) && shipment.PartnerId.Contains("_Return_");

                if (isReturn)
                {
                    // Lấy tài khoản khách hàng
                    var customerAccount = await _unitOfWork.AccountRepository.GetAsync(
                        (Guid)order.CreatedById!,
                        include: a => a.Include(a => a.Wallet)
                    );
                    if (customerAccount == null || customerAccount.Wallet == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Customer account or wallet for order {order.Id} not found."
                        };
                    }

                    // Kiểm tra Service và CreatedById
                    if (order.Package?.Service?.CreatedById == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Service or artisan ID for order {order.Id} not found."
                        };
                    }

                    // Lấy tài khoản nghệ nhân
                    var artisanAccount = await _unitOfWork.AccountRepository.GetAsync(
                        (Guid)order.Package.Service.CreatedById,
                        include: a => a.Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                    );
                    if (artisanAccount == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Artisan account for order {order.Id} not found."
                        };
                    }

                    var accountRoleArtisan = artisanAccount.AccountRoles
                        .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());
                    if (accountRoleArtisan == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Artisan role not found for account {artisanAccount.Id} in order {order.Id}."
                        };
                    }

                    // Hoàn tiền cho khách
                    var wallet = customerAccount.Wallet;
                    wallet.Balance += (decimal)order.TotalPrice!;
                    _unitOfWork.WalletRepository.Update(wallet);

                    order.Transactions.Add(new Transaction
                    {
                        WalletId = wallet.Id,
                        Amount = order.TotalPrice,
                        Type = TransactionType.TransferIn,
                        Status = TransactionStatus.Completed,
                        CreatedById = customerAccount.Id,
                        Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Customer)

                    });

                    // Trừ điểm uy tín nghệ nhân
                    if (accountRoleArtisan.TotalReputation > 0)
                    {
                        accountRoleArtisan.TotalReputation = Math.Max(0, accountRoleArtisan.TotalReputation - 5);
                        _unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);

                        var artisanReputationLog = new ReputationLog
                        {
                            PointChange = -5,
                            Reason = $"Đơn hàng trả lại {order.Code} do lỗi vận chuyển",
                            OrderId = order.Id,
                            AccountRoleId = accountRoleArtisan.Id,
                            CreatedById = artisanAccount.Id
                        };
                        await _unitOfWork.ReputationLogRepository.AddAsync(artisanReputationLog);
                    }
                }
                else
                {
                    order.Stage = OrderStage.AwaitingClosure;
                    _unitOfWork.OrderRepository.Update(order);
                }
                try
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var saveResult = await _unitOfWork.SaveChangeAsync();
                    if (saveResult <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to seed shipment status history."
                        };
                    }

                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = $"Error saving changes: {ex.Message}"
                    };
                }
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = isReturn ? "Return shipment status history seeded, refunded, and artisan reputation updated successfully." : "Shipment status history seeded successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error seeding shipment status history: {ex.Message}"
                };
            }
        }
    }
}

