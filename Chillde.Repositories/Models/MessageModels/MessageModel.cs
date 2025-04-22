using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.OfferModels;

namespace Chillde.Repositories.Models.MessageModels;

public class MessageModel : BaseEntity
{
    public string? Content { get; set; }
    public string? AttachmentUrl { get; set; }
    public MediaType MessageType { get; set; }
    public bool IsPinned { get; set; }
    public bool IsModified { get; set; }
    public Guid? OfferId { get; set; }
    public OfferModel? Offer { get; set; }

    // Foreign key
    public Guid? ParentMessageId { get; set; }

    // Relationship
    public List<AccountLiteModel>? IsReadBy { get; set; }
}