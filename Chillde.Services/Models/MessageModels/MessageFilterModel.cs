using Chillde.Repositories.Common;
using Chillde.Services.Common;

namespace Chillde.Services.Models.MessageModels;

public class MessageFilterModel : FilterParameter
{
    protected override int MinPageSize { get; set; } = Constant.MessageMinPageSize;
    protected override int MaxPageSize { get; set; } = Constant.MessageMaxPageSize;
}