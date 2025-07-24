using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Service.Extension
{
    /// <summary>
    /// JSON序列化全局扩展方法
    /// </summary>
    public static class JsonExtension
    {
        /// <summary>
        /// 转换为JSON
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToJson(this object val)
        {
            return JsonSerializer.Serialize(val);
        }
        /// <summary>
        /// 转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T? ToObj<T>(this string val)
        {
            return JsonSerializer.Deserialize<T>(val);
        }
    }
}
