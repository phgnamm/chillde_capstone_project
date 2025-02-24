using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Enums
{
    public enum OrderStage
    {
        ReviewRequirement,
        SketchInProcess,
        ReviewSketch,
        DeliveryInProcess,
        ReviewDelivery,
        Late,
        Canceled,
        Shipping,
        Completed,
    }
}
