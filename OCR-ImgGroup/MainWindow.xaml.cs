using OCR_ImgGroup.Model.Config;
using OCR_ImgGroup.Service.Utils;
using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace OCR_ImgGroup;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private AppConfig _app;
    private CancellationTokenSource _cts;
    private bool _isRunning = false;

    private FullOcrModel _config;
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        LogViewer.Debug(TextBuild.GetUseText());
    }

    #region 初始化
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _config = new FullOcrModel(
                DetectionModel.FromDirectory(@"ch_PP-OCRv4\ch_PP-OCRv4_det_infer", ModelVersion.V4),
                ClassificationModel.FromDirectory(@"ch_PP-OCRv4\ch_ppocr_mobile_v2.0_cls_infer"),
                RecognizationModel.FromDirectory(@"ch_PP-OCRv4\ch_PP-OCRv4_rec_infer", @"ch_PP-OCRv4\ppocr_keys.txt", ModelVersion.V4));
            _app = await JsonConfig<AppConfig>.LoadAsync() ?? new();

            if (_app.Workfolder != "") txt_work.Text = _app.Workfolder;
            if (_app.WorkfolderSave != "") txt_workSave.Text = _app.WorkfolderSave;
        }
        catch (Exception ex)
        {
            await LogViewer.ErrorAsync(ex.Message);
        }
    }
    #endregion 初始化

    #region 任务开始
    /// <summary>
    /// 开始任务方法
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (txt_work.Text == "" || txt_workSave.Text == "")
        {
            await LogViewer.WarningAsync("请指定 [工作文件夹] 和 [存放文件夹] ！");
            return;
        }
        if (string.IsNullOrEmpty(txt_matchStart.Text) && string.IsNullOrEmpty(txt_matchEnd.Text))
        {
            await LogViewer.WarningAsync("[匹配开始文字] 和 [匹配结束文字] 至少要指定一个！");
            return;
        }

        await LogViewer.InfoAsync($"正在获取目标文件夹下的所有图片...");
        //筛选文件夹下所有图片
        var suffixArr = AppConst.SupportEx.Split('/');
        var fileList = Directory.EnumerateFiles(
            txt_work.Text,
            "*",
            cbb_isContain.SelectedIndex == 0 ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
            .Where(x =>
            {
                //文件格式是否符合要求
                var suffix = System.IO.Path.GetExtension(x).ToLower();
                //文件是否已经存在于存放目录下，避免重复操作
                var relativePath = System.IO.Path.GetRelativePath(txt_workSave.Text, x);
                return suffixArr.Contains(suffix) && relativePath.StartsWith("..");
            });
        if (!fileList.Any())
        {
            await LogViewer.ErrorAsync("指定的工作文件夹中没有图片!");
            return;
        }

        if (!AppRunOrStop(false)) return;

        await LogViewer.InfoAsync($"开始分类目录中的所有图片，目录：{txt_work.Text}");
        var engine = new PaddleOcrAll(_config)
        {
            AllowRotateDetection = true,   // 允许检测旋转文本
            Enable180Classification = false // 禁用180度旋转分类
        };
        try
        {
            int curIndex = 1;
            int fileCount = fileList.Count();
            //循环处理所有文件
            foreach (var file in fileList)
            {
                var fileName = System.IO.Path.GetFileName(file);
                await LogViewer.InfoAsync($"[{curIndex}/{fileCount}]{fileName}：正在识别中...");
                using (Mat src = Cv2.ImRead(file, ImreadModes.Grayscale))
                {
                    PaddleOcrResult ocrResult = engine.Run(src);
                    if (ocrResult != null && ocrResult.Regions.Length > 0)
                    {
                        bool IsMatch = false;
                        foreach (var ocr in ocrResult.Regions)
                        {
                            //匹配
                            var match = await MatchTextAndCopyFile(text: ocr.Text, file: file, fileName: fileName);
                            if (match)
                            {
                                IsMatch = true;
                                break;
                            }
                        }

                        //循环结束
                        if (!IsMatch)
                        {
                            await LogViewer.WarningAsync($"{fileName}：已跳过，匹配不到合规的文字");
                        }
                    }
                    else
                    {
                        await LogViewer.WarningAsync($"{fileName}：已跳过，识别失败");
                    }
                    curIndex++;
                }
            }


            await LogViewer.SuccessAsync($"{fileCount}个文件已全部处理完成");
        }
        catch (OperationCanceledException)
        {
            // 任务被取消
            await LogViewer.WarningAsync($"任务被取消，提前终止！");
        }
        catch (Exception ex)
        {
            await LogViewer.ErrorAsync($"发生错误：{ex.Message}");
        }
        finally
        {
            AppRunOrStop();
            engine.Dispose();
        }
    }
    #endregion 任务开始

    #region 文件夹相关
    /// <summary>
    /// 选择工作文件夹
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChooseWork_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "选择工作文件夹",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            txt_work.Text = dialog.FolderName;
            txt_workSave.Text = System.IO.Path.Combine(dialog.FolderName, AppConst.DefaultSaveFolderName);

            _app.Workfolder = txt_work.Text;
            _app.WorkfolderSave = txt_workSave.Text;
        }
    }

    /// <summary>
    /// 选择存放文件夹
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChooseWorkSave_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "选择存放文件夹",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            txt_workSave.Text = dialog.FolderName;

            _app.WorkfolderSave = txt_workSave.Text;
        }
    }

    /// <summary>
    /// 定位文件夹
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ViewFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txt_workSave.Text)) LogViewer.Warning("无法定位到空目录");
            else Process.Start("explorer.exe", txt_workSave.Text);
        }
        catch (Exception ex)
        {
            LogViewer.Error("无法打开所在文件夹:" + ex.Message);
        }
    }
    #endregion 文件夹相关

    #region 详细说明
    /// <summary>
    /// 详细说明按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Instructions_Click(object sender, RoutedEventArgs e)
    {
        var list = TextBuild.GetMatchingRules();
        foreach (var text in list)
        {
            LogViewer.Debug(text);
        }
    }
    #endregion 详细说明

    #region Git/Gitee
    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(AppConst.GitHubUrl);
    }
    private void Gitee_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(AppConst.GiteeUrl);
    }
    #endregion Git/Gitee

    #region 关闭程序
    /// <summary>
    /// 重写关闭方法
    /// </summary>
    /// <param name="e"></param>
    protected override async void OnClosed(EventArgs e)
    {
        _cts?.Dispose();
        await JsonConfig<AppConfig>.SaveAsync(_app);
        base.OnClosed(e);
    }
    #endregion 关闭程序

    #region 私有方法
    /// <summary>
    /// 任务开始或结束
    /// </summary>
    /// <param name="IsBreak">是否中途中止</param>
    private bool AppRunOrStop(bool IsBreak = true)
    {
        if (IsBreak || _isRunning)
        {
            //结束
            _isRunning = false;
            StartButton.Content = "开始分类";
            _cts?.Cancel();
        }
        else
        {
            //开始
            _isRunning = true;
            StartButton.Content = "提前结束";
            _cts = new CancellationTokenSource();
        }
        return _isRunning;
    }

    /// <summary>
    /// 匹配文字并复制/移动文件
    /// </summary>
    /// <param name="text">文字结果</param>
    /// <param name="file">文件完整路径</param>
    /// <param name="fileName">文件名称</param>
    /// <returns></returns>
    private async Task<bool> MatchTextAndCopyFile(string text, string file, string fileName)
    {
        bool IsMatch = false;
        string str = Regex.Replace(text, @"\s+", "");
        if (!string.IsNullOrEmpty(txt_matchStart.Text))
        {
            var i = str.IndexOf(txt_matchStart.Text.Trim());
            if (i > -1)
            {
                str = str.Substring(i + txt_matchStart.Text.Trim().Length);
                IsMatch = true;
            }
        }
        if (!string.IsNullOrEmpty(txt_matchEnd.Text))
        {
            var i = str.IndexOf(txt_matchEnd.Text.Trim());
            if (i > -1)
            {
                str = str.Substring(0, i);
                IsMatch = true;
            }
        }

        if (IsMatch)
        {
            var typeName = cbb_isMove.SelectedIndex == 0 ? "复制" : "移动";
            await LogViewer.InfoAsync($"{fileName}：已匹配到，正在{typeName}到新文件夹...");
            //文件复制
            var dir = System.IO.Path.Combine(txt_workSave.Text, str);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var newFile = System.IO.Path.Combine(dir, fileName);

            if (cbb_isMove.SelectedIndex == 0) File.Copy(file, newFile, true);
            else File.Move(file, newFile, true);
            await LogViewer.SuccessAsync($"{fileName}：{typeName}完成");
        }
        return IsMatch;
    }

    /// <summary>
    /// 用默认浏览器打开url
    /// </summary>
    /// <param name="url"></param>
    private void OpenUrl(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) LogViewer.Error("无法跳转空链接");
            else Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            LogViewer.Error($"打开浏览器失败: {ex.Message}");
        }
    }
    #endregion 私有方法
}