using System.Xml;
using System.Xml.Serialization;

namespace ContinuousDeliveryMainProgram.MSBuild;

/// <summary>完整项目模型。混合列表保留分组、导入及扩展节点的原始顺序。</summary>
[XmlRoot("Project")]
public sealed class MsBuildProject
{
    [XmlAttribute]
    public string? Sdk { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[]? OtherAttributes { get; set; }

    [XmlElement("PropertyGroup", typeof(MsBuildPropertyGroup))]
    [XmlElement("ItemGroup", typeof(MsBuildItemGroup))]
    [XmlElement("Import", typeof(MsBuildImport))]
    [XmlAnyElement]
    public List<object> Elements { get; set; } = [];

    /// <summary>替换本工具管理的分组，保持已有分组位置和其他节点不变。</summary>
    public void SetGeneratedGroup(MsBuildGroup group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group.Label);
        var index = Elements.FindIndex(element => element is MsBuildGroup existing && existing.Label == group.Label);
        if (index < 0)
        {
            Elements.Add(group);
            return;
        }

        Elements[index] = group;
        for (var i = Elements.Count - 1; i > index; i--)
            if (Elements[i] is MsBuildGroup existing && existing.Label == group.Label)
                Elements.RemoveAt(i);
    }
}

public abstract class MsBuildGroup
{
    [XmlAttribute]
    public string? Label { get; set; }

    [XmlAttribute]
    public string? Condition { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[]? OtherAttributes { get; set; }
}

public sealed class MsBuildPropertyGroup : MsBuildGroup
{
    [XmlElement("AssemblyName", typeof(MsBuildAssemblyName))]
    [XmlElement("RootNamespace", typeof(MsBuildRootNamespace))]
    [XmlElement("OutputType", typeof(MsBuildOutputType))]
    [XmlElement("TargetFramework", typeof(MsBuildTargetFramework))]
    [XmlElement("Nullable", typeof(MsBuildNullable))]
    [XmlElement("ImplicitUsings", typeof(MsBuildImplicitUsings))]
    [XmlElement("UseWPF", typeof(MsBuildUseWPF))]
    [XmlElement("OutputPath", typeof(MsBuildOutputPath))]
    [XmlElement("BaseOutputPath", typeof(MsBuildBaseOutputPath))]
    [XmlElement("AppendTargetFrameworkToOutputPath", typeof(MsBuildAppendTargetFrameworkToOutputPath))]
    [XmlElement("AppendRuntimeIdentifierToOutputPath", typeof(MsBuildAppendRuntimeIdentifierToOutputPath))]
    [XmlElement("ApplicationIcon", typeof(MsBuildValue))]
    [XmlAnyElement]
    public List<object> Properties { get; set; } = [];
}

public sealed class MsBuildItemGroup : MsBuildGroup
{
    [XmlElement("EmbeddedResource", typeof(MsBuildEmbeddedResource))]
    [XmlElement("Resource", typeof(MsBuildResource))]
    [XmlElement("AvaloniaResource", typeof(MsBuildAvaloniaResource))]
    [XmlElement("Content", typeof(MsBuildContent))]
    [XmlAnyElement]
    public List<object> Items { get; set; } = [];
}

public class MsBuildItem
{
    [XmlAttribute]
    public string? Include { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[]? OtherAttributes { get; set; }

    [XmlElement("LogicalName", typeof(MsBuildLogicalName))]
    [XmlElement("CopyToOutputDirectory", typeof(MsBuildCopyToOutputDirectory))]
    [XmlAnyElement]
    public List<object> Metadata { get; set; } = [];
}

public sealed class MsBuildEmbeddedResource : MsBuildItem;
public sealed class MsBuildResource : MsBuildItem;
public sealed class MsBuildAvaloniaResource : MsBuildItem;
public sealed class MsBuildContent : MsBuildItem;

public class MsBuildValue
{
    [XmlText]
    public string? Value { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[]? OtherAttributes { get; set; }
}

public sealed class MsBuildOutputType : MsBuildValue;
public sealed class MsBuildTargetFramework : MsBuildValue;
public sealed class MsBuildNullable : MsBuildValue;
public sealed class MsBuildImplicitUsings : MsBuildValue;
public sealed class MsBuildUseWPF : MsBuildValue;
public sealed class MsBuildOutputPath : MsBuildValue;
public sealed class MsBuildBaseOutputPath : MsBuildValue;
public sealed class MsBuildAppendTargetFrameworkToOutputPath : MsBuildValue;
public sealed class MsBuildAppendRuntimeIdentifierToOutputPath : MsBuildValue;

public sealed class MsBuildLogicalName : MsBuildValue;
public sealed class MsBuildCopyToOutputDirectory : MsBuildValue;

public sealed class MsBuildImport
{
    [XmlAttribute]
    public string? Project { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[]? OtherAttributes { get; set; }
}


public sealed class MsBuildAssemblyName : MsBuildValue;
public sealed class MsBuildRootNamespace : MsBuildValue;


