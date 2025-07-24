using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Model.Config
{
    [FileName("AppConfig.json")]
    public class AppConfig
    {
        /// <summary>
        /// 工作文件夹
        /// </summary>
        public string Workfolder { get; set; } = "";
        /// <summary>
        /// 工作存储文件夹
        /// </summary>
        public string WorkfolderSave { get; set; } = "";
    }
}
