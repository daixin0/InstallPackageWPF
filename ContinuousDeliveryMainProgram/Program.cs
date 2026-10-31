
using ContinuousDeliveryMainProgram;

bool programsOnly = args.Length > 0 && args[0] == "--programs-only";
var configArguments = programsOnly ? args[1..] : args;
if (configArguments.Length > 1 || configArguments.Any(argument => argument.StartsWith("--")))
{
    Console.Error.WriteLine("用法：ContinuousDeliveryMainProgram [--programs-only] [构建配置.json]");
    Console.Error.WriteLine("省略配置时使用默认配置，待打包目录为 F:/GitCode/Filigree/Output。");
    return 1;
}

try
{
    var configurationPath = configArguments.Length == 0 ? FindDefaultConfiguration() : configArguments[0];
    var options = InstallerBuildConfiguration.Load(configurationPath);
    Console.WriteLine($"构建配置：{Path.GetFullPath(configurationPath)}");
    Console.WriteLine($"待打包程序目录：{options.PayloadDirectory}");
    if (programsOnly)
    {
        await MaintenanceProgramBuilder.BuildAsync(options);
        Console.WriteLine($"更新器和卸载器已写入待安装目录：{options.PayloadDirectory}");
        return 0;
    }
    await InstallerPackageBuilder.BuildAsync(options);
    Console.WriteLine($"安装程序已发布到：{options.GetOutputDirectory()}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

// 从程序所在位置定位仓库中的默认配置，不依赖启动时的工作目录。
static string FindDefaultConfiguration()
{
    for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
    {
        var path = Path.Combine(directory.FullName, "ContinuousDeliveryMainProgram", "installer-build.example.json");
        if (File.Exists(path))
            return path;
    }
    throw new FileNotFoundException("找不到默认构建配置 installer-build.example.json，请传入构建配置文件路径。");
}

