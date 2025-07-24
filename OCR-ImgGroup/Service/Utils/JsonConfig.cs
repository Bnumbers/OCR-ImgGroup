using OCR_ImgGroup.Model.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Service.Utils
{
    public class JsonConfig<T> where T : new()
    {
        private static readonly string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
        static JsonConfig()
        {
            // 确保目录存在
            if (!Directory.Exists(BaseDir)) Directory.CreateDirectory(BaseDir);
        }

        public static async Task<T?> LoadAsync()
        {
            // 反射获取文件路径
            var attr = Attribute.GetCustomAttribute(typeof(T), typeof(FileNameAttribute)) as FileNameAttribute ?? throw new InvalidOperationException("缺少配置文件特性标记");
            string filePath = Path.Combine(BaseDir, attr.Name);
            if (!File.Exists(filePath))
            {
                await SaveAsync(new T());
            }
            await using FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }
        public static async Task SaveAsync(T data)
        {
            // 反射获取文件路径
            var attr = Attribute.GetCustomAttribute(typeof(T), typeof(FileNameAttribute)) as FileNameAttribute ?? throw new InvalidOperationException("缺少配置文件特性标记");
            string filePath = Path.Combine(BaseDir, attr.Name);
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }
    }
}
