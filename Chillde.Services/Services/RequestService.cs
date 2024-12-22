using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.ConversationModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public RequestService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> Add(RequestAddModel requestAddModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            if (requestAddModel.MinBudget > requestAddModel.MaxBudget)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "MinBudget must be less or equal more than MaxBudget"
                };
            }
            var newRequest = new Request
            {
                Id = Guid.NewGuid(),
                CreatedById = currentUserId,
                ItemId = requestAddModel.ItemId,
                Name = requestAddModel.Name,
                Description = requestAddModel.Description,
                MinBudget = requestAddModel.MinBudget,
                MaxBudget = requestAddModel.MaxBudget,
                Timeline = requestAddModel.Timeline,
                RequestDetails = requestAddModel.RequestDetailAddModels.Select(_ => new RequestDetail
                {
                    Id = Guid.NewGuid(),
                    ItemAttributeId = _.ItemAttributeId,
                    Description = _.Description,
                    CreatedById = currentUserId,
                }).ToList()
            };
            if (requestAddModel.AttachmentUrl != null)
            {
                newRequest.AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                    requestAddModel.AttachmentUrl,
                    newRequest.Id.ToString(),
                    newRequest.Id.ToString());
            }

            await _unitOfWork.RequestRepository.AddAsync(newRequest);

            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Request created successfully"
                }
                : new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Failed to create request"
                };

        }

        public async Task<ResponseModel> GetAll(RequestFilterModel requestFilterModel)
        {
            var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                filter: _ => _.IsDeleted == requestFilterModel.IsDeleted &&
                             _.Name.ToLower().Contains(requestFilterModel.Search.ToLower()),
                include: requests => requests.Include(_ => _.Item),
                pageIndex: requestFilterModel.PageIndex,
                pageSize: requestFilterModel.PageSize
            );

            var requestModels = requests.Data.Select(_ => new RequestModel
            {
                Id = _.Id,
                Name = _.Name,
                IsDeleted = _.IsDeleted,
                ItemName = _.Item?.Name,
                CreationDate = _.CreationDate,
                MaxBudget = _.MaxBudget,
                MinBudget = _.MinBudget,
                Timeline = _.Timeline,
                Description = _.Description,
                Status = _.Status,
            }).ToList();

            var result = new Pagination<RequestModel>(requestModels, requestFilterModel.PageIndex,
                requestFilterModel.PageSize, requests.TotalCount);

            return new ResponseModel
            {
                Message = "Get all requests successfully",
                Data = result
            };
        }


        public async Task<ResponseModel> GetById(Guid id)
        {
            var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestDetails)
                                                                                         .ThenInclude(_ => _.ItemAttribute) 
                                                                                         .Include(_ => _.Item));
            if (existingRequest == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Request not found."
                };
            }
            var existingRequestModel = new RequestGetByIdModel
            {
                Id = existingRequest.Id,
                Name = existingRequest.Name ?? "Unknown",
                Description = existingRequest.Description ?? "Unknown",
                MinBudget = (decimal)existingRequest.MinBudget,
                MaxBudget = (decimal)existingRequest.MaxBudget,
                Timeline = (int)existingRequest.Timeline,
                AttachmentUrl = existingRequest.AttachmentUrl ?? "Unknown",
                Status = existingRequest.Status,
                ItemId = existingRequest.ItemId,
                ItemName = existingRequest.Item.Name ?? "Unknown",
                ItemCode = existingRequest.Item.Code ?? "Unknown",
                ItemImageUrl = existingRequest.Item.ImageUrl ?? "Unknown",
                RequestDetailGetByIdModels = existingRequest.RequestDetails.Select(_ => new RequestDetailGetByIdModel
                {
                    Id = _.Id,
                    Description = _.Description,
                    ItemAttributeId = _.ItemAttributeId,
                    ItemAttributeName = _.ItemAttribute.Name ?? "Unknown"
                }).ToList()                
            };
            return new ResponseModel 
            { 
                Data = existingRequestModel, 
                Message = "Get request detail success" 
            };
        }

        public async Task<ResponseModel> Update(Guid id, RequestUpdateModel requestUpdateModel)
        {
            if (requestUpdateModel == null || id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input."
                };
            }

            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }

            var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestDetails));
            if (existingRequest == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Request not found."
                };
            }

            existingRequest.Name = requestUpdateModel.Name ?? existingRequest.Name;
            existingRequest.Description = requestUpdateModel.Description ?? existingRequest.Description;
            existingRequest.MinBudget = requestUpdateModel.MinBudget ?? existingRequest.MinBudget;
            existingRequest.MaxBudget = requestUpdateModel.MaxBudget ?? existingRequest.MaxBudget;
            existingRequest.Timeline = requestUpdateModel.Timeline ?? existingRequest.Timeline;
            existingRequest.ItemId = requestUpdateModel.ItemId != Guid.Empty ? requestUpdateModel.ItemId : existingRequest.ItemId;
            existingRequest.ModifiedById = currentUserId.Value;
            existingRequest.ModificationDate = DateTime.UtcNow;
            if (requestUpdateModel.AttachmentUrl != null)
            {
                existingRequest.AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                    requestUpdateModel.AttachmentUrl,
                    existingRequest.Id.ToString(),
                    existingRequest.Id.ToString());
            }
            if (requestUpdateModel.RequestDetailUpdateModels != null)
            {
                foreach (var detail in requestUpdateModel.RequestDetailUpdateModels)
                {
                    var existingDetail = existingRequest.RequestDetails.FirstOrDefault(_ => _.Id == detail.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.Description = detail.Description ?? existingDetail.Description;
                        existingDetail.ItemAttributeId = detail.ItemAttributeId != Guid.Empty ? detail.ItemAttributeId : existingDetail.ItemAttributeId;
                        existingDetail.ModifiedById = currentUserId.Value;
                        existingDetail.ModificationDate = DateTime.UtcNow;
                    }
                    else
                    {
                        existingRequest.RequestDetails.Add(new RequestDetail
                        {
                            Id = Guid.NewGuid(),
                            Description = detail.Description,
                            ItemAttributeId = detail.ItemAttributeId,
                            CreatedById = currentUserId.Value,
                            CreationDate = DateTime.UtcNow
                        });
                    }
                }
            }

            _unitOfWork.RequestRepository.Update(existingRequest);
            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Request updated successfully."
                }
                : new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Failed to update request."
                };
        }
    }
}
