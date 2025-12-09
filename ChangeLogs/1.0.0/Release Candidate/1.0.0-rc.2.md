# 1.0.0-rc.2

> [!IMPORTANT]
> 我们完成了**工具类**以及**控件**的重构，耗时 **1** 天!

## 重构

### 类

* AppDownloader
  + 独立下载器为单独实例，全程序共用一个

### 控件

* UserCard
  + 改用**代码创建控件**而非**Xaml字符串**转换为控件实例

### 工具

* ConfigManager
  + 去除方法名的 `All`
  + 添加方法 `CreateDefaultConfig`, 用于创建默认配置文件
  + 更名 `ReadConifg()` 为 `LoadConfig()`
  + 为读写配置。两个方法添加保护
  
* DialogManager
  + 使用 `SemaphoreSlim` 重写，完全解决了伪等待
  + 修复了同时显示超过2个对话框就无法解决冲突的问题

* DirectoryManager
  + *(这个不用重构，至少能用)*

* ErrorReportDialog
  + 更改日志**输出格式**，使其变得更加**美观**
  + 添加**滚动视图**，解决错误过长无法显示问题
  + 优化显示

* GameManager
  + 为方法名添加后缀 `Async`
  + 重构逻辑

* NavigationController
  + 使用Tag来匹配页，而非Name检测

* Updater
  + 使用独立的 `Downloader`
  + 添加更新服务文件存在性检测
  + 移除临时文件的**后缀名**
  + 修复了在程序启动时检测更新会导致**更新失败**问题
