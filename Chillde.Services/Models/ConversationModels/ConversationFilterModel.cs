using Chillde.Repositories.Common;
using Chillde.Services.Common;

namespace Chillde.Services.Models.ConversationModels;

public class ConversationFilterModel : FilterParameter
{
    public bool? IsArchived { get; set; }

    // protected override int MinPageSize { get; set; } = Constant.;
    protected override int MaxPageSize { get; set; } = Constant.ConversationMaxPageSize;
}