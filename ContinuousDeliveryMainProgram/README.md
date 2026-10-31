# 安装包构建

无参数启动时，自动从程序所在位置向上查找仓库内的 ContinuousDeliveryMainProgram/installer-build.example.json，使用其中的待打包目录 F:/GitCode/Filigree/Output。默认配置中的产品信息、更新器和卸载器配置同样生效；安装包输出到本仓库的 Output。

    dotnet run --project ContinuousDeliveryMainProgram/ContinuousDeliveryMainProgram.csproj

也可以直接运行 Output/ContinuousDeliveryMainProgram.exe。传入配置文件路径时使用指定配置；单独传入 --programs-only 时使用默认配置只生成维护程序。

WPF 项目复制 installer-build.example.json，Avalonia 项目复制 installer-build.avalonia.example.json，保存为 installer-build.json。按中文注释填写输入路径、EXE 名称、资源名称和构建参数。

配置使用带注释的 JSON（JSONC 语法），加载器支持 //、/* */ 注释及末尾逗号；普通严格 JSON 解析器不能直接读取注释。未知字段会报错，以避免拼写错误被忽略。未填写的可选配置使用实体默认值。

    dotnet run --project ContinuousDeliveryMainProgram.csproj -- installer-build.json

代码调用：

    var options = InstallerBuildConfiguration.Load("installer-build.json");
    await InstallerPackageBuilder.BuildAsync(options);

只准备资源、更新项目而不编译：

    InstallerPackageBuilder.PrepareResources(options);

## 配置分组

- 根节点：UIFramework（Wpf / Avalonia）和输入文件路径。
- PayloadDirectory：已编译好的待安装程序目录，直接使用其中的现有文件。
- Project：EXE 名称（AssemblyName）、根命名空间和 TargetFramework。AssemblyName、RootNamespace 为 null 时沿用项目值；UseWPF 根据 UIFramework 自动决定。
- Resources：配置、图标、Logo 文件名及扩展名过滤规则；资源目录和清单文件名固定。
- UninstallerConfigPath、UpdaterConfigPath：用于生成卸载器、更新器的 JSON 路径，支持相对路径；null 或省略表示不生成该程序。

根节点的相对路径以配置文件所在目录为基准。安装包沿用现有的 $(SolutionDir)\Output 输出规则，编译和发布结果均放到仓库根目录的 Output 中。旧 JSON 中的 OutputDirectory 字段需要删除，不再接受配置。直接调用构建 API 时，输入相对路径以进程当前目录为基准。

待安装文件固定复制到项目下的 Resources 目录，实际嵌入名称固定为 $(RootNamespace).Resources.{清单中的 ResourceName}。清单文件名固定为 InstallAllFileInfo.json，嵌入名称为 $(RootNamespace).InstallAllFileInfo.json。目录名、资源名前缀和清单文件名不支持配置；旧配置中的 DirectoryName、LogicalNamePrefix 和 ManifestFileName 字段需删除，否则加载时会报未知字段错误。配置的嵌入名称为 $(RootNamespace).{配置的文件名}。ICO 和 PNG 按 UIFramework 使用 WPF Resource 或 AvaloniaResource。修改配置文件名后，安装程序读取端也要同步修改。

## 项目生成

项目文件操作类 MsBuildProjectFile.cs 和模型类 MsBuildProject.cs 统一放在 MSBuild 文件夹，命名空间为 ContinuousDeliveryMainProgram.MSBuild。

MsBuildProjectFile 使用 XmlSerializer 将整个 .csproj 反序列化为实体，替换本工具管理的属性组和资源组，再整体序列化写回。固定规则位于 InstallerProjectRules.cs：分组 Label、旧版导入名称、框架与图片资源类型的映射。MSBuild 节点模型位于 MsBuildProject.cs，清单字段协议位于 FileData.cs；这些协议不开放任意改名，避免破坏项目构建或资源读取。

模型保留原有分组顺序、条件及未建模节点和属性。支持 SDK 项目与旧 MSBuild 命名空间。整体序列化会重新排版 XML，不保留注释。不再生成独立 .targets；上一版生成的导入会移除，旧文件保留但不再使用。

安装清单仍按 FileData 的原有字段生成 JSON。旧资源副本不物理删除，但会从生成的嵌入列表移除。本工具管理的资源不要再手工重复添加到 csproj。

安装器默认使用 win-x64、依赖框架的单文件发布，不附带 .NET/WPF 运行库。设置统一放在 InstallProgramWPF.csproj 中。Debug 和 Release 生成均自动发布：编译文件直接生成到 Output/Build/对应配置，最终单文件 EXE 放在 Output 根目录，不创建 Publish 子目录。通过 NoBuild 和 AutoPublishRunning 避免循环编译。打包工具使用相同的项目配置。发布成功后清理根目录中旧安装器的同名 DLL、deps.json、runtimeconfig.json 和 PDB，不清空共享输出目录。运行安装器需要 .NET 10 桌面运行时（x64）。

LauncherResourceWriter 保留原启动程序资源接口，并增加可选的 uiFramework 参数（默认 Wpf），用于选择登录图片资源类型。本配置用于 InstallerPackageBuilder；InstallProgramWPF 的安装解压界面不属于本构建流程。


## WPF 与 Avalonia

- WPF：UIFramework 为 Wpf，TargetFramework 一般为 net10.0-windows，生成 UseWPF=true 和 Resource 图片节点。
- Avalonia：UIFramework 为 Avalonia，示例 TargetFramework 为 net10.0，生成 UseWPF=false 和 AvaloniaResource 图片节点。
- UseWPF 由 UIFramework 自动决定，不接受配置。安装项目路径同样自动选择：Wpf 使用 ../InstallProgramWPF/InstallProgramWPF.csproj，Avalonia 使用 ../InstallProgramAvalonia/InstallProgramAvalonia.csproj，均相对于配置文件所在目录。直接创建配置对象调用时，以创建对象时的当前目录为基准；GetProjectPath() 返回所选项目的绝对路径。切换 UIFramework 时仍需检查 TargetFramework 和程序集名称。
- 两者的安装文件、清单和配置始终使用 EmbeddedResource，不改变 FileData 清单结构。
- Avalonia 的 Logo 位于项目根目录时，可通过 avares://程序集名称/installLogo.png 读取；自定义名称后调整对应 URI。参见 https://docs.avaloniaui.net/docs/fundamentals/including-assets 。

自动选择的安装项目必须已经存在。本工具保留现有 Avalonia 包引用和 Assets 配置，不创建界面、安装依赖或将 WPF XAML 转换成 Avalonia。旧配置需删除顶层 ProjectPath、PayloadProgram 和 Project.UseWPF，否则会报未知字段错误。生成的图标和 Logo 使用单独的资源项，请避免在原项目中重复包含同一文件。

目前验证覆盖两种框架的 XML 写入、重复生成、资源类型切换、原有包引用和资源节点保留；尚未对真实 Avalonia 项目执行发布。

## 固定项目属性

以下属性在 InstallerProjectRules.cs 中固定，由 WPF 和 Avalonia 共用，不接受 JSON 配置：

- OutputType：WinExe
- Nullable：enable
- ImplicitUsings：enable
- OutputPath、BaseOutputPath：$(SolutionDir)\Output
- AppendTargetFrameworkToOutputPath、AppendRuntimeIdentifierToOutputPath：false

旧配置中如果包含这些字段，需要删除；配置加载器会拒绝未知字段。

安装器发布参数固定在 InstallerProjectRules.cs 中：Configuration=Release、RuntimeIdentifier=win-x64、SelfContained=false。JSON 不接受这三个字段。
安装程序自身图标由 JSON 的 IconPath 指定，Resources.IconFileName 决定复制后的名称和 ApplicationIcon。

## 图标与构建流程

安装界面的所需磁盘空间按清单内实际嵌入文件的字节数计算，包含维护程序，磁盘选择和安装前校验使用同一大小。环境包不参与打包：排除 Environmental 目录和常见 NDP、VC++、.NET 离线安装 EXE，保留应用运行所需的 DLL 和 runtimes 目录。

安装前读取主程序 EXE.config 声明的 .NET Framework 版本，并按微软 Release 注册表规则检测兼容版本；缺少环境时提示用户先自行安装，当前安装不继续，不下载或执行环境包。当前 Filigree 主程序要求 .NET Framework 4.7.2。检测规则参见 https://learn.microsoft.com/dotnet/framework/install/how-to-determine-which-versions-are-installed 。

- 安装程序自身：IconPath 指向 ICO；Resources.IconFileName 是复制到安装项目后的名称，写入安装项目的 ApplicationIcon。
- 更新程序和卸载程序：使用同一个 IconPath。
- 待安装主程序：直接使用 PayloadDirectory 中已编译好的 EXE，不修改其图标，也不编译其源码。

路径相对于配置文件。BuildAsync 按以下顺序执行：

1. 根据 UninstallerConfigPath 和 UpdaterConfigPath 生成维护程序，写入 PayloadDirectory。
2. 按 Resources.ExcludedExtensions 过滤规则，将待安装目录中的文件作为安装资源写入安装项目。
3. 编译并发布安装程序到仓库根目录的 Output。

PayloadDirectory 必须独立于安装项目目录，且不能包含输出目录。某一步发布失败后，不再继续后续打包。

PrepareResources 只准备当前目录中的资源，不触发编译；生成维护程序和安装程序需调用 BuildAsync。

ApplicationIcon 控制 Windows EXE 图标，不会自动修改 WPF/Avalonia 窗口的 Icon、Logo 或快捷方式图标。安装程序与待安装程序可以使用不同 UI 框架，待安装项目保留自身的框架、包引用和界面配置。

## 按产品生成更新器和卸载器

用户配置放在本目录的 UninstallerConfig.json 和 UpdaterConfig.json，字段说明直接写在 JSON 注释中。打包时校验配置，将不含注释的 JSON 写入 Output/MaintenancePrograms 下的本次生成目录并内嵌到程序。模板源码不写入产品名称；更换产品后修改 JSON，再重新生成即可。

DisplayName、MainProgram、RegistryTag 必须与 InstallerProductInfo.json 一致；卸载快捷方式字段、更新器的 StartExe 也会校验一致性。安装路径、当前版本、机器标识、安装时间自动获取，不需要用户配置。生成卸载器和更新器需要安装器启用 ControlPanelDisplayName，以便写入它们读取的安装信息。

模板固定使用 ../UninstallProgramWPF/UninstallProgramWPF.csproj 和 ../UpdateProgramWPF/UpdateProgramWPF.csproj（相对于构建配置目录）。这两个工具始终为 WPF，与安装器的 UIFramework 独立。默认生成 Release、win-x64、自包含单文件 EXE，直接写入 PayloadDirectory；它们使用构建配置的 IconPath，不需要用户另外配置程序集名、项目路径或运行时。安装程序仍由原流程生成。

完整构建会先生成维护程序再嵌入安装包。仅生成维护程序，可从仓库根目录运行：

    dotnet run --project ContinuousDeliveryMainProgram/ContinuousDeliveryMainProgram.csproj -p:SolutionDir=F:/GitCode/InstallPackageWPF/ -- --programs-only ContinuousDeliveryMainProgram/installer-build.example.json

代码调用为 `await MaintenanceProgramBuilder.BuildAsync(options)`。`InstallerPackageBuilder.PrepareResources` 仍只准备当前目录中的资源，不触发工具生成。两个工具都生成成功后才复制到待安装目录；任何生成错误都会阻止后续打包。生成记录保留在 MaintenancePrograms 目录中供检查，不会嵌入安装包。

UpdaterConfig.json 的 VersionManifestUrl 目前为空，更新器只提示尚未配置更新服务，不会终止主程序或尝试联网。未来服务需提供 HTTPS JSON 清单，字段为 releaseVersion、installerFileUrl、installerFileSha256。清单中的安装包地址也必须为 HTTPS，SHA256 必填，暂不支持 HTTP 重定向。用户确认后下载并校验完整安装包，再启动安装包完成更新；不会在下载后提前修改已安装版本。没有执行真实服务器联调。

验证命令（模拟 HTTP，不启动安装器、不写注册表）：

    dotnet run --project Tests/MaintenanceProgramChecks/MaintenanceProgramChecks.csproj -p:SolutionDir=F:/GitCode/InstallPackageWPF/ -- F:/GitCode/InstallPackageWPF
