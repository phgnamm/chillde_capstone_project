using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ICategoryServive
    {
        Task<ResponseModel> Add(CategoryAddModel categoryAddModel);
    }
}
