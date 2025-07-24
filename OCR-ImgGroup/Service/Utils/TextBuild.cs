using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_ImgGroup.Service.Utils
{
    public static class TextBuild
    {
        public static string GetUseText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------使用说明--------");
            sb.AppendLine("1.指定工作文件夹和存放文件夹");
            sb.AppendLine("2.指定匹配规则");
            sb.AppendLine("3.点击开始分类等待操作完成即可");
            sb.AppendFormat("支持 {0} 图片格式", AppConst.SupportEx);
            return sb.ToString();
        }
        public static List<string> GetMatchingRules()
        {
            List<string> res = new List<string>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------匹配帮助--------");
            sb.AppendLine("1.“匹配开始文字”与“匹配结束文字”用于定位分组的文件夹名");
            sb.AppendLine("2.图片中有多行文字时以每一行文字为单位匹配");
            sb.Append("3.每行文字之间的空格会被忽略");
            res.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("--例子1--");
            sb.AppendLine("图片a上文字内容：甲有10个苹果");
            sb.AppendLine("图片b上文字内容：乙有12个橘子");
            sb.AppendLine("匹配开始文字填入：");
            sb.AppendLine("匹配结束文字填入：有");
            sb.Append("分组结果：文件夹[甲] => [图片a]，文件夹[乙] => [图片b]");
            res.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("--例子2--");
            sb.AppendLine("图片a上文字内容：甲有10个苹果");
            sb.AppendLine("图片b上文字内容：乙有12个橘子");
            sb.AppendLine("匹配开始文字填入：个");
            sb.AppendLine("匹配结束文字填入：");
            sb.Append("分组结果：文件夹[苹果] => [图片a]，文件夹[橘子] => [图片b]");
            res.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("--例子3--");
            sb.AppendLine("图片a上文字内容：甲   有1 0 个苹 果");
            sb.AppendLine("图片b上文字内容：乙 有  12个 橘 子");
            sb.AppendLine("匹配开始文字填入：有");
            sb.AppendLine("匹配结束文字填入：个");
            sb.Append("分组结果：文件夹[10] => [图片a]，文件夹[12] => [图片b]");
            res.Add(sb.ToString());
            return res;
        }
    }
}
