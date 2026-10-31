using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ContinuousDeliveryMainProgram.MSBuild;

/// <summary>使用 XmlSerializer 读写整个项目，保留 SDK/旧项目的命名空间。</summary>
public sealed class MsBuildProjectFile
{
    public MsBuildProject Project { get; }
    private readonly string xmlNamespace;

    private MsBuildProjectFile(MsBuildProject project, string xmlNamespace)
    {
        Project = project;
        this.xmlNamespace = xmlNamespace;
    }

    public static MsBuildProjectFile Load(string path)
    {
        using var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        reader.MoveToContent();
        var xmlNamespace = reader.NamespaceURI;
        var serializer = new XmlSerializer(typeof(MsBuildProject), xmlNamespace);
        var project = serializer.Deserialize(reader) as MsBuildProject
            ?? throw new InvalidDataException("不是有效的 MSBuild 项目。");
        return new MsBuildProjectFile(project, xmlNamespace);
    }

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(MsBuildProject), xmlNamespace);
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", xmlNamespace);
        // 先完成序列化，再写回，避免序列化异常截断原项目。
        using var buffer = new MemoryStream();
        using (var writer = XmlWriter.Create(buffer, new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
            CloseOutput = false
        }))
            serializer.Serialize(writer, Project, namespaces);
        File.WriteAllBytes(path, buffer.ToArray());
    }
}
