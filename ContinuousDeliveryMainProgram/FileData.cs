namespace ContinuousDeliveryMainProgram;

/// <summary>安装文件清单，保留原有 JSON 属性名。</summary>
public sealed class FileData
{
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string ResourceName { get; set; } = "";
    public bool IsJsonFile { get; set; }
    public bool IsIcoImage { get; set; }
}
