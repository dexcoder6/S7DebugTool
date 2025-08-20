# S7 Debug Tool - 西门子PLC调试工具

<div align="center">
  <h1>🔧 S7 Debug Tool</h1>
  <p>
    <strong>原生S7协议实现的西门子PLC调试工具</strong>
  </p>
  <p>
    <img src="https://img.shields.io/badge/Platform-.NET%208-blue" alt="Platform">
    <img src="https://img.shields.io/badge/Framework-WPF-green" alt="Framework">
    <img src="https://img.shields.io/badge/Protocol-S7-orange" alt="Protocol">
    <img src="https://img.shields.io/badge/License-MIT-lightgrey" alt="License">
  </p>
</div>

## 📝 项目简介

S7 Debug Tool 是一个基于 .NET 8 和 WPF 开发的西门子 PLC 调试工具。该工具完全使用原生 C# 实现了 S7 通信协议，无需依赖任何第三方协议库，提供了一个直观易用的界面来读写 PLC 数据。

### ✨ 核心特性

- **🚀 原生协议实现** - 完全自主实现 S7 协议栈，无第三方依赖
- **📊 全数据类型支持** - 支持所有 S7 数据类型的读写操作
  - Bool, Byte, Word, DWord, Int, DInt, Real, LReal, String, DateTime
- **🔄 智能数据分块** - 自动处理超过 PDU 大小限制的数据传输
- **🎯 多内存区域访问** - 支持 DB、Input、Output、Memory 等区域
- **📋 批量操作** - 支持批量读写和任务管理
- **🖥️ 现代化UI** - 基于 WPF 的响应式用户界面，支持自动滚动日志
- **🔍 多格式显示** - 支持十六进制、十进制、ASCII格式数据显示

## 🏗️ 技术架构

### 协议栈实现

```
应用层 (S7 Debug Tool)
    ↓
S7 协议层 (S7 Protocol)
    ↓
ISO-COTP 层 (ISO 8073)
    ↓
TPKT 层 (RFC1006)
    ↓
TCP 层 (Port 102)
```

### 项目结构

```
S7DebugTool/
├── S7DebugTool.sln            # 解决方案文件
├── S7DebugTool/               # WPF项目
│   ├── Services/              # 核心服务层
│   │   ├── S7Client.cs        # S7协议客户端实现
│   │   ├── S7DataHelper.cs    # 数据类型转换辅助类
│   │   └── S7ClientExtensions.cs # 扩展方法
│   ├── ViewModels/            # MVVM视图模型
│   │   └── MainViewModel.cs   # 主视图模型
│   ├── Views/                 # WPF视图
│   │   ├── MainWindow.xaml    # 主窗口
│   │   └── MainWindow.xaml.cs # 主窗口代码
│   ├── Models/                # 数据模型
│   │   └── BatchTask.cs       # 批量任务模型
│   ├── Converters/            # 值转换器
│   ├── App.xaml               # 应用程序配置
│   └── S7DebugTool.csproj     # 项目文件
└── README.md                  # 项目说明文档
```

## 系统要求

- .NET 8.0 或更高版本
- Windows 10/11
- Visual Studio 2022 或 VS Code with C# Dev Kit

## 快速开始

### 编译运行

1. 打开命令行，进入项目目录：
```bash
cd path\to\S7DebugTool
```

2. 还原NuGet包：
```bash
dotnet restore
```

3. 编译项目：
```bash
dotnet build
```

4. 运行应用：
```bash
dotnet run --project S7DebugTool
```

## 📊 支持的数据类型

| 数据类型 | 描述 | 字节数 | 读取 | 写入 |
|---------|------|--------|------|------|
| Bool | 布尔值 | 1 bit | ✅ | ✅ |
| Byte | 字节 | 1 | ✅ | ✅ |
| Word | 无符号字 | 2 | ✅ | ✅ |
| DWord | 无符号双字 | 4 | ✅ | ✅ |
| Int | 有符号整数 | 2 | ✅ | ✅ |
| DInt | 有符号双整数 | 4 | ✅ | ✅ |
| Real | 浮点数 | 4 | ✅ | ✅ |
| LReal | 双精度浮点数 | 8 | ✅ | ✅ |
| String | 字符串 | 可变 | ✅ | ✅ |
| DateTime | 日期时间 | 8 | ✅ | ✅ |

## 🛠️ 使用说明

### 1. 连接PLC
- 输入PLC的IP地址
- 设置机架号（通常为0）
- 设置插槽号（S7-1200/1500通常为1，S7-300/400通常为2）
- 点击"连接"按钮

### 2. 数据类型读写（第一个标签页）
- **读取数据**
  - 选择内存区域（DB块、输入、输出、标志位）
  - 选择数据类型（Bool, Byte, Int, Real等）
  - 输入起始地址
  - 系统会自动设置合适的读取长度
  - 点击"读取"按钮
  
- **写入数据**
  - 选择内存区域和数据类型
  - 输入起始地址
  - 根据提示输入相应格式的数据
  - 点击"写入"按钮

### 3. 批量字节读写（第二个标签页）
- **批量读取**
  - 设置起始地址和读取长度
  - 选择显示格式（HEX/DEC/ASCII）
  - 可选择是否显示地址
  - 点击"批量读取"按钮
  
- **批量写入**
  - 输入起始地址
  - 输入字节数据（支持多种格式）
  - 点击"批量写入"按钮

### 4. 批量任务（第三个标签页）
- 添加多个读写任务
- 设置任务参数
- 点击"执行所有"批量执行

## 📚 技术细节

### S7协议说明
- **TCP端口**：102
- **协议层次**：S7 → ISO-COTP → TPKT → TCP
- **连接流程**：
  1. TCP连接（端口102）
  2. COTP连接请求/确认
  3. S7通信设置（协商PDU大小）

### 内存区域代码
- `0x81`：输入区 (I)
- `0x82`：输出区 (Q)
- `0x83`：标志位 (M)
- `0x84`：数据块 (DB)

### PDU 大小处理
工具自动处理 PDU（协议数据单元）大小限制：
- 默认 PDU 大小：240 字节
- 自动协商最大 PDU
- 大数据自动分块传输

### 字节序处理
S7 协议使用大端字节序（Big Endian），工具自动处理：
- 读取时：Big Endian → Little Endian
- 写入时：Little Endian → Big Endian

## 🐛 故障排除

### 常见问题

1. **连接失败**
   - 检查 IP 地址是否正确
   - 确认 PLC 是否在线
   - 验证机架/插槽号设置
   - 检查防火墙设置，确保端口 102 开放

2. **读写失败**
   - 确认数据块是否存在
   - 检查地址范围是否有效
   - 验证访问权限
   - 确保PLC的防护等级允许外部访问

3. **数据不正确**
   - 确认数据类型选择正确
   - 检查字节序设置
   - 验证 PLC 数据格式

## ⚠️ 注意事项

- 建议在测试环境中使用，避免影响生产系统
- 写入操作请谨慎，可能影响PLC运行状态
- 定期备份PLC程序和数据

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

### 贡献指南

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📮 联系方式

- 项目主页: [GitHub](https://github.com/dexcoder/s7-debug-tool)
- Bug 报告: [Issues](https://github.com/dexcoder/s7-debug-tool/issues)

## 🙏 致谢

- 感谢 SIEMENS 提供的 S7 协议文档
- 感谢所有贡献者的努力

---

<div align="center">
  Made with ❤️ by S7 Debug Tool Team
</div>