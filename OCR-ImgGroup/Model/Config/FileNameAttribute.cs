using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Model.Config
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FileNameAttribute : Attribute
    {
        public string Name { get; }

        public FileNameAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("文件名不能为空", nameof(name));
            }
            Name = name;
        }
    }
}
