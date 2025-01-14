using System.ComponentModel;
using System.Reflection;

namespace Chillde.Services.Utils
{
    public static class EnumHelper
    {
      
        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return value.ToString();
            }
            DescriptionAttribute? attribute = (DescriptionAttribute?)field.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
