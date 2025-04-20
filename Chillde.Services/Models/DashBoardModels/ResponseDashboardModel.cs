using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.DashBoardModels
{
    public class ResponseDashboardModel<T> : ResponseModel
    {
        public new T? Data
        {
            get => (T?)base.Data;
            set => base.Data = value;
        }
    }
}
