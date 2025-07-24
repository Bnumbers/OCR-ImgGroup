using OCR_ImgGroup.Model.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Service.Extension
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return string.Empty;

            var field = type.GetField(name);
            if (field == null) return string.Empty;

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), inherit: false) is DescriptionAttribute descriptionAttribute) return descriptionAttribute.Description;
            return string.Empty;
        }
        public static List<EnumItem> GetEnumItems(this Type enumType)
        {
            return Enum.GetValues(enumType)
                      .Cast<Enum>()
                      .Select(e => new EnumItem
                      {
                          Value = e,
                          Description = e.GetDescription()
                      }).ToList();
        }
    }
}
