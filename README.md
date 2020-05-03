# InstallPackageWPF
WPF Custom Setup

所有程序使用.net framework 4.5.2开发，安装包可以更改为.net framework 4.0的框架，并不影响。

假如安装包是4.0版本，你的执行程序要求版本较高（4.0以上），可以将较高版本的.net framework拷贝到Resources的Environmental目录下，然后打开MainWindow.xaml.cs文件，找到Worker_DoWork函数，然后去掉里面注释的前两行。注意文件名不要错了。


包含程序：

    	1.打包程序
    	2.更新程序
    	3.卸载程序
	4.示例程序
	
打包程序功能支持：
	
	1.支持写入32位和64位注册表（用来在控制面板中显示和卸载），已自动处理，注册表的key是uuid+BIOSSerialNumber组成
 	2.内涵粒子动画（在MainWindow.xaml.cs文件中的ChildWindow_Loaded函数中，将CurrentSetupState = SetupState.Default改为CurrentSetupState = SetupState.SetupProgress即可看到粒子动画效果）
   	3.支持任意第三方或自己开发的程序进行打包，将所有文件放入打包程序的Resources目录并设置文件属性（生成操作）为Resource，在打包程序的LocalInstallTesting文件中的CopyAllFile函数中添加执行文件的拷贝代码（代码参照内部已有示例）
   	4.内置.net framework和vc++环境检测(.net framework版本默认检测4.5.2，VC++默认检测2015版本)
   	5.内置展示软件协议的流文档
   	6.盘符剩余空间检测
   	7.32和64位系统检测
   	8.生成桌面和应用程序菜单的快捷方式
   	9.增加了管理启动权限
   
卸载程序功能支持：

  	1.删除安装目录的文件
  	2.删除桌面和应用程序菜单快捷方式
  	3.删除注册表
  	4.删除卸载程序本身
  
  
  
更新程序支持：

  	1.读取注册表当前版本
  	2.通过主程序传入的服务器地址进行更新，更新完成后启动主程序
	3.验证是否存在网络
  
  
示例程序：

  	1.增加了更新程序的调用
  	2.可以通过App.config文件中的IsAutomaticInspectUpdate配置是否启动更新界面
