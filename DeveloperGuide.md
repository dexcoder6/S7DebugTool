# S7Client 开发者文档

## 目录

1. [概述](#概述)
2. [快速开始](#快速开始)
3. [S7Client 核心类](#s7client-核心类)
4. [S7DataHelper 数据转换类](#s7datahelper-数据转换类)
5. [S7ClientExtensions 扩展方法](#s7clientextensions-扩展方法)
6. [完整示例](#完整示例)
7. [最佳实践](#最佳实践)
8. [错误处理](#错误处理)

---

## 概述

S7Client 是一个原生 C# 实现的西门子 S7 协议客户端库，无需依赖第三方库即可与 S7 系列 PLC 进行通信。

### 主要特性

- ✅ 完全原生 C# 实现
- ✅ 支持 S7-300/400/1200/1500
- ✅ 自动 PDU 大小协商和数据分块
- ✅ 完整的数据类型支持
- ✅ 线程安全设计
- ✅ 异步操作支持

### 命名空间

```csharp
using S7DebugTool.Services;
```

---

## 快速开始

### 最简单的例子

```csharp
using S7DebugTool.Services;

// 创建客户端实例
var client = new S7Client("192.168.0.1", rack: 0, slot: 1);

// 连接到 PLC
await client.ConnectAsync();

// 读取数据
byte[]? data = await client.ReadDBAsync(dbNumber: 1, startAddress: 0, length: 10);

// 写入数据
byte[] writeData = new byte[] { 0x01, 0x02, 0x03 };
bool success = await client.WriteDBAsync(dbNumber: 1, startAddress: 0, writeData);

// 断开连接
client.Disconnect();
```

---

## S7Client 核心类

### 构造函数

```csharp
public S7Client(string ip, int rack = 0, int slot = 2)
```

**参数说明**：
- `ip`: PLC 的 IP 地址
- `rack`: 机架号（默认 0）
- `slot`: 插槽号（S7-1200/1500 通常为 1，S7-300/400 通常为 2）

### 属性

```csharp
public bool IsConnected { get; }      // 连接状态
public string IpAddress { get; }      // IP 地址
public int Rack { get; }              // 机架号
public int Slot { get; }              // 插槽号
public int PduSize { get; }           // 协商的 PDU 大小
```

### 事件

```csharp
public event EventHandler<string>? LogMessage;  // 日志消息事件
```

**使用示例**：
```csharp
client.LogMessage += (sender, message) => 
{
    Console.WriteLine($"[S7] {message}");
};
```

### 连接方法

#### 异步连接
```csharp
public async Task<bool> ConnectAsync()
```

**返回值**：
- `true`: 连接成功
- `false`: 连接失败

**示例**：
```csharp
if (await client.ConnectAsync())
{
    Console.WriteLine("连接成功");
}
else
{
    Console.WriteLine("连接失败");
}
```

#### 断开连接
```csharp
public void Disconnect()
```

### 数据块(DB)读写

#### 读取数据块
```csharp
public async Task<byte[]?> ReadDBAsync(int dbNumber, int startAddress, int length)
public byte[]? ReadDB(int dbNumber, int startAddress, int length)  // 同步版本
```

**参数**：
- `dbNumber`: 数据块编号
- `startAddress`: 起始字节地址
- `length`: 读取字节数

**返回值**：
- 成功：字节数组
- 失败：null

**示例**：
```csharp
// 读取 DB100 从地址 0 开始的 100 字节
byte[]? data = await client.ReadDBAsync(100, 0, 100);
if (data != null)
{
    Console.WriteLine($"读取成功: {BitConverter.ToString(data)}");
}
```

#### 写入数据块
```csharp
public async Task<bool> WriteDBAsync(int dbNumber, int startAddress, byte[] data)
public bool WriteDB(int dbNumber, int startAddress, byte[] data)  // 同步版本
```

**参数**：
- `dbNumber`: 数据块编号
- `startAddress`: 起始字节地址
- `data`: 要写入的字节数组

**返回值**：
- `true`: 写入成功
- `false`: 写入失败

**示例**：
```csharp
byte[] data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
bool success = await client.WriteDBAsync(100, 10, data);
```

### 其他内存区域读取

#### 读取输入区(I)
```csharp
public byte[]? ReadInput(int startAddress, int length)
```

#### 读取输出区(Q)
```csharp
public byte[]? ReadOutput(int startAddress, int length)
```

#### 读取标志区(M)
```csharp
public byte[]? ReadMemory(int startAddress, int length)
```

**示例**：
```csharp
// 读取 M 区从地址 100 开始的 10 字节
byte[]? memoryData = client.ReadMemory(100, 10);

// 读取输入 I10.0 - I11.7 (2字节)
byte[]? inputData = client.ReadInput(10, 2);
```

### 大数据自动分块

当读写数据超过 PDU 限制时，S7Client 会自动进行分块处理：

```csharp
// 读取 5000 字节（自动分块）
byte[]? largeData = await client.ReadDBAsync(1, 0, 5000);
// 客户端会自动将请求分成多个小于 PDU 大小的块
```

---

## S7DataHelper 数据转换类

S7DataHelper 提供了 S7 数据类型与 .NET 数据类型之间的转换方法。

### 读取数据类型

#### 读取位(Bool)
```csharp
public static bool GetBitAt(byte[] buffer, int byteIndex, int bitIndex)
```

**示例**：
```csharp
byte[] data = await client.ReadDBAsync(1, 0, 1);
bool bit3 = S7DataHelper.GetBitAt(data, 0, 3);  // 读取第0字节的第3位
```

#### 读取整数类型
```csharp
public static ushort GetWordAt(byte[] buffer, int index)    // 16位无符号
public static uint GetDWordAt(byte[] buffer, int index)      // 32位无符号
public static short GetIntAt(byte[] buffer, int index)       // 16位有符号
public static int GetDIntAt(byte[] buffer, int index)        // 32位有符号
```

**示例**：
```csharp
byte[] data = await client.ReadDBAsync(1, 0, 4);
int value = S7DataHelper.GetDIntAt(data, 0);  // 读取32位整数
```

#### 读取浮点数
```csharp
public static float GetRealAt(byte[] buffer, int index)      // 32位浮点
public static double GetLRealAt(byte[] buffer, int index)    // 64位浮点
```

**示例**：
```csharp
byte[] data = await client.ReadDBAsync(1, 0, 4);
float temperature = S7DataHelper.GetRealAt(data, 0);
```

#### 读取字符串
```csharp
public static string GetStringAt(byte[] buffer, int index)
```

**S7字符串格式**：
- 字节0：最大长度
- 字节1：实际长度
- 字节2+：字符数据

**示例**：
```csharp
byte[] data = await client.ReadDBAsync(1, 0, 256);
string text = S7DataHelper.GetStringAt(data, 0);
```

#### 读取日期时间
```csharp
public static DateTime GetDateTimeAt(byte[] buffer, int index)
```

### 写入数据类型

#### 设置位(Bool)
```csharp
public static void SetBitAt(ref byte value, int bitIndex, bool bit)
```

#### 写入整数类型
```csharp
public static void SetWordAt(byte[] buffer, int index, ushort value)
public static void SetDWordAt(byte[] buffer, int index, uint value)
public static void SetIntAt(byte[] buffer, int index, short value)
public static void SetDIntAt(byte[] buffer, int index, int value)
```

**示例**：
```csharp
byte[] buffer = new byte[4];
S7DataHelper.SetDIntAt(buffer, 0, 12345);
await client.WriteDBAsync(1, 0, buffer);
```

#### 写入浮点数
```csharp
public static void SetRealAt(byte[] buffer, int index, float value)
public static void SetLRealAt(byte[] buffer, int index, double value)
```

#### 写入字符串
```csharp
public static void SetStringAt(byte[] buffer, int index, int maxLength, string value)
```

**示例**：
```csharp
byte[] buffer = new byte[256];
S7DataHelper.SetStringAt(buffer, 0, 254, "Hello PLC");
await client.WriteDBAsync(1, 100, buffer);
```

---

## S7ClientExtensions 扩展方法

扩展方法提供了更便捷的类型化读写操作。

### 读取扩展方法

```csharp
// 读取单个位
bool? value = await client.ReadBitAsync(dbNumber: 1, byteAddress: 0, bitAddress: 3);

// 读取字节
byte? value = await client.ReadByteAsync(dbNumber: 1, address: 0);

// 读取字(Word)
ushort? value = await client.ReadWordAsync(dbNumber: 1, address: 0);

// 读取双字(DWord)
uint? value = await client.ReadDWordAsync(dbNumber: 1, address: 0);

// 读取整数(Int)
short? value = await client.ReadIntAsync(dbNumber: 1, address: 0);

// 读取双整数(DInt)
int? value = await client.ReadDIntAsync(dbNumber: 1, address: 0);

// 读取浮点数(Real)
float? value = await client.ReadRealAsync(dbNumber: 1, address: 0);

// 读取双精度浮点数(LReal)
double? value = await client.ReadLRealAsync(dbNumber: 1, address: 0);

// 读取字符串
string? text = await client.ReadStringAsync(dbNumber: 1, address: 0, maxLength: 254);

// 读取日期时间
DateTime? dt = await client.ReadDateTimeAsync(dbNumber: 1, address: 0);
```

### 写入扩展方法

```csharp
// 写入位
bool success = await client.WriteBitAsync(dbNumber: 1, byteAddress: 0, bitAddress: 3, value: true);

// 写入字节
bool success = await client.WriteByteAsync(dbNumber: 1, address: 0, value: 0xFF);

// 写入字
bool success = await client.WriteWordAsync(dbNumber: 1, address: 0, value: 12345);

// 写入双字
bool success = await client.WriteDWordAsync(dbNumber: 1, address: 0, value: 123456789);

// 写入整数
bool success = await client.WriteIntAsync(dbNumber: 1, address: 0, value: -100);

// 写入双整数
bool success = await client.WriteDIntAsync(dbNumber: 1, address: 0, value: -1000000);

// 写入浮点数
bool success = await client.WriteRealAsync(dbNumber: 1, address: 0, value: 3.14159f);

// 写入双精度浮点数
bool success = await client.WriteLRealAsync(dbNumber: 1, address: 0, value: 3.141592653589793);

// 写入字符串
bool success = await client.WriteStringAsync(dbNumber: 1, address: 0, value: "Hello", maxLength: 254);

// 写入日期时间
bool success = await client.WriteDateTimeAsync(dbNumber: 1, address: 0, value: DateTime.Now);
```

### 批量数组操作

```csharp
// 读取整数数组
short[]? values = await client.ReadIntArrayAsync(dbNumber: 1, address: 0, count: 10);

// 写入整数数组
short[] data = new short[] { 1, 2, 3, 4, 5 };
bool success = await client.WriteIntArrayAsync(dbNumber: 1, address: 0, values: data);

// 读取浮点数数组
float[]? values = await client.ReadRealArrayAsync(dbNumber: 1, address: 0, count: 10);

// 写入浮点数数组
float[] data = new float[] { 1.1f, 2.2f, 3.3f };
bool success = await client.WriteRealArrayAsync(dbNumber: 1, address: 0, values: data);
```

---

## 完整示例

### 示例1：基本读写操作

```csharp
using System;
using System.Threading.Tasks;
using S7DebugTool.Services;

class Program
{
    static async Task Main(string[] args)
    {
        // 创建客户端
        var client = new S7Client("192.168.0.1", rack: 0, slot: 1);
        
        // 添加日志处理
        client.LogMessage += (s, msg) => Console.WriteLine($"[LOG] {msg}");
        
        try
        {
            // 连接 PLC
            if (!await client.ConnectAsync())
            {
                Console.WriteLine("连接失败");
                return;
            }
            
            Console.WriteLine($"连接成功，PDU大小: {client.PduSize}");
            
            // 读取不同数据类型
            int? counter = await client.ReadDIntAsync(1, 0);
            float? temperature = await client.ReadRealAsync(1, 4);
            bool? alarm = await client.ReadBitAsync(1, 8, 0);
            
            Console.WriteLine($"计数器: {counter}");
            Console.WriteLine($"温度: {temperature}°C");
            Console.WriteLine($"报警: {alarm}");
            
            // 写入数据
            await client.WriteDIntAsync(1, 0, 100);
            await client.WriteRealAsync(1, 4, 25.5f);
            await client.WriteBitAsync(1, 8, 0, false);
            
            Console.WriteLine("数据写入完成");
        }
        finally
        {
            client.Disconnect();
        }
    }
}
```

### 示例2：批量数据处理

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using S7DebugTool.Services;

class BatchOperations
{
    static async Task ProcessBatchData()
    {
        var client = new S7Client("192.168.0.1");
        
        if (!await client.ConnectAsync())
            return;
        
        try
        {
            // 读取生产数据数组（100个浮点数）
            float[]? productionData = await client.ReadRealArrayAsync(
                dbNumber: 100, 
                address: 0, 
                count: 100
            );
            
            if (productionData != null)
            {
                // 数据处理
                float average = productionData.Average();
                float max = productionData.Max();
                float min = productionData.Min();
                
                Console.WriteLine($"平均值: {average}");
                Console.WriteLine($"最大值: {max}");
                Console.WriteLine($"最小值: {min}");
                
                // 写回统计结果
                byte[] buffer = new byte[12];
                S7DataHelper.SetRealAt(buffer, 0, average);
                S7DataHelper.SetRealAt(buffer, 4, max);
                S7DataHelper.SetRealAt(buffer, 8, min);
                
                await client.WriteDBAsync(101, 0, buffer);
            }
        }
        finally
        {
            client.Disconnect();
        }
    }
}
```

### 示例3：结构化数据读写

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using S7DebugTool.Services;

// 定义 PLC 数据结构
public class PlcData
{
    public int ProductId { get; set; }        // DBD0
    public float Temperature { get; set; }     // DBD4
    public float Pressure { get; set; }        // DBD8
    public bool Running { get; set; }          // DBX12.0
    public bool Alarm { get; set; }            // DBX12.1
    public string ProductName { get; set; }    // DBB14 (String[50])
}

class StructuredData
{
    static async Task<PlcData?> ReadPlcData(S7Client client, int dbNumber)
    {
        // 读取整个数据块
        byte[]? buffer = await client.ReadDBAsync(dbNumber, 0, 66);
        
        if (buffer == null)
            return null;
        
        // 解析数据
        var data = new PlcData
        {
            ProductId = S7DataHelper.GetDIntAt(buffer, 0),
            Temperature = S7DataHelper.GetRealAt(buffer, 4),
            Pressure = S7DataHelper.GetRealAt(buffer, 8),
            Running = S7DataHelper.GetBitAt(buffer, 12, 0),
            Alarm = S7DataHelper.GetBitAt(buffer, 12, 1),
            ProductName = S7DataHelper.GetStringAt(buffer, 14)
        };
        
        return data;
    }
    
    static async Task WritePlcData(S7Client client, int dbNumber, PlcData data)
    {
        byte[] buffer = new byte[66];
        
        // 填充缓冲区
        S7DataHelper.SetDIntAt(buffer, 0, data.ProductId);
        S7DataHelper.SetRealAt(buffer, 4, data.Temperature);
        S7DataHelper.SetRealAt(buffer, 8, data.Pressure);
        
        // 设置位
        byte statusByte = 0;
        S7DataHelper.SetBitAt(ref statusByte, 0, data.Running);
        S7DataHelper.SetBitAt(ref statusByte, 1, data.Alarm);
        buffer[12] = statusByte;
        
        // 设置字符串
        S7DataHelper.SetStringAt(buffer, 14, 50, data.ProductName);
        
        // 写入PLC
        await client.WriteDBAsync(dbNumber, 0, buffer);
    }
}
```

### 示例4：实时监控

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using S7DebugTool.Services;

class RealtimeMonitoring
{
    private static S7Client client;
    private static CancellationTokenSource cts;
    
    static async Task StartMonitoring()
    {
        client = new S7Client("192.168.0.1");
        cts = new CancellationTokenSource();
        
        if (!await client.ConnectAsync())
        {
            Console.WriteLine("连接失败");
            return;
        }
        
        Console.WriteLine("开始监控，按任意键停止...");
        
        // 启动监控任务
        var monitorTask = MonitorDataAsync(cts.Token);
        
        // 等待用户输入
        Console.ReadKey();
        
        // 停止监控
        cts.Cancel();
        await monitorTask;
        
        client.Disconnect();
    }
    
    static async Task MonitorDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 读取关键数据
                var temperature = await client.ReadRealAsync(1, 0);
                var pressure = await client.ReadRealAsync(1, 4);
                var alarm = await client.ReadBitAsync(1, 8, 0);
                
                // 显示数据
                Console.Clear();
                Console.WriteLine($"时间: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"温度: {temperature:F1}°C");
                Console.WriteLine($"压力: {pressure:F2} bar");
                Console.WriteLine($"报警: {(alarm == true ? "是" : "否")}");
                
                // 检查报警条件
                if (temperature > 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("警告：温度过高！");
                    Console.ResetColor();
                }
                
                // 延时
                await Task.Delay(1000, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"监控错误: {ex.Message}");
            }
        }
    }
}
```

---

## 最佳实践

### 1. 连接管理

```csharp
public class PlcConnection : IDisposable
{
    private S7Client client;
    private readonly string ip;
    private readonly int rack;
    private readonly int slot;
    
    public PlcConnection(string ip, int rack = 0, int slot = 1)
    {
        this.ip = ip;
        this.rack = rack;
        this.slot = slot;
    }
    
    public async Task<bool> ConnectAsync()
    {
        client = new S7Client(ip, rack, slot);
        return await client.ConnectAsync();
    }
    
    public void Dispose()
    {
        client?.Disconnect();
        client?.Dispose();
    }
}

// 使用
using (var plc = new PlcConnection("192.168.0.1"))
{
    if (await plc.ConnectAsync())
    {
        // 操作
    }
}
```

### 2. 错误重试机制

```csharp
public async Task<T?> ReadWithRetry<T>(Func<Task<T?>> readFunc, int maxRetries = 3)
    where T : struct
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var result = await readFunc();
            if (result.HasValue)
                return result;
        }
        catch (Exception ex)
        {
            if (i == maxRetries - 1)
                throw;
            
            await Task.Delay(100 * (i + 1));  // 递增延时
        }
    }
    return null;
}

// 使用
var value = await ReadWithRetry(
    () => client.ReadDIntAsync(1, 0), 
    maxRetries: 3
);
```

### 3. 数据缓存

```csharp
public class PlcDataCache
{
    private readonly S7Client client;
    private readonly Dictionary<string, (DateTime time, object value)> cache;
    private readonly TimeSpan cacheTimeout;
    
    public PlcDataCache(S7Client client, TimeSpan timeout)
    {
        this.client = client;
        this.cacheTimeout = timeout;
        this.cache = new Dictionary<string, (DateTime, object)>();
    }
    
    public async Task<T?> GetValueAsync<T>(
        string key, 
        Func<Task<T?>> readFunc) where T : struct
    {
        if (cache.TryGetValue(key, out var cached))
        {
            if (DateTime.Now - cached.time < cacheTimeout)
            {
                return (T)cached.value;
            }
        }
        
        var value = await readFunc();
        if (value.HasValue)
        {
            cache[key] = (DateTime.Now, value.Value);
        }
        
        return value;
    }
}
```

### 4. 线程安全操作

```csharp
public class ThreadSafePlcClient
{
    private readonly S7Client client;
    private readonly SemaphoreSlim semaphore;
    
    public ThreadSafePlcClient(S7Client client)
    {
        this.client = client;
        this.semaphore = new SemaphoreSlim(1, 1);
    }
    
    public async Task<byte[]?> ReadDBAsync(int db, int address, int length)
    {
        await semaphore.WaitAsync();
        try
        {
            return await client.ReadDBAsync(db, address, length);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
```

---

## 错误处理

### 异常类型

```csharp
try
{
    await client.ConnectAsync();
}
catch (InvalidOperationException ex)
{
    // 操作无效（如未连接时尝试读写）
    Console.WriteLine($"操作错误: {ex.Message}");
}
catch (TimeoutException ex)
{
    // 连接或操作超时
    Console.WriteLine($"超时: {ex.Message}");
}
catch (Exception ex)
{
    // 其他错误
    Console.WriteLine($"未知错误: {ex.Message}");
}
```

### 返回值检查

```csharp
// 检查读取结果
byte[]? data = await client.ReadDBAsync(1, 0, 10);
if (data == null)
{
    Console.WriteLine("读取失败");
    return;
}

// 检查写入结果
bool success = await client.WriteDBAsync(1, 0, data);
if (!success)
{
    Console.WriteLine("写入失败");
    return;
}

// 检查类型化读取
int? value = await client.ReadDIntAsync(1, 0);
if (!value.HasValue)
{
    Console.WriteLine("读取失败或数据无效");
    return;
}
```

### 日志诊断

```csharp
client.LogMessage += (sender, message) =>
{
    // 根据消息内容进行分类处理
    if (message.Contains("错误") || message.Contains("失败"))
    {
        LogError(message);
    }
    else if (message.Contains("警告"))
    {
        LogWarning(message);
    }
    else
    {
        LogInfo(message);
    }
};
```

---

## 性能优化建议

### 1. 批量读取

```csharp
// 不推荐：多次单独读取
var value1 = await client.ReadDIntAsync(1, 0);
var value2 = await client.ReadRealAsync(1, 4);
var value3 = await client.ReadWordAsync(1, 8);

// 推荐：一次读取，本地解析
byte[]? buffer = await client.ReadDBAsync(1, 0, 10);
if (buffer != null)
{
    var value1 = S7DataHelper.GetDIntAt(buffer, 0);
    var value2 = S7DataHelper.GetRealAt(buffer, 4);
    var value3 = S7DataHelper.GetWordAt(buffer, 8);
}
```

### 2. 连接复用

```csharp
// 保持长连接，避免频繁连接断开
public class PlcService
{
    private S7Client client;
    
    public async Task Initialize()
    {
        client = new S7Client("192.168.0.1");
        await client.ConnectAsync();
    }
    
    // 复用连接进行多次操作
    public async Task<int?> ReadCounter() => 
        await client.ReadDIntAsync(1, 0);
    
    public async Task<float?> ReadTemperature() => 
        await client.ReadRealAsync(1, 4);
}
```

### 3. 异步并发

```csharp
// 并发读取多个数据块
var tasks = new[]
{
    client.ReadDBAsync(1, 0, 100),
    client.ReadDBAsync(2, 0, 100),
    client.ReadDBAsync(3, 0, 100)
};

byte[][] results = await Task.WhenAll(tasks);
```

---

## 附录：内存区域和数据类型

### 内存区域代码

| 区域 | 代码 | 说明 |
|-----|------|------|
| 输入 | 0x81 | 输入映像区 (I) |
| 输出 | 0x82 | 输出映像区 (Q) |
| 标志 | 0x83 | 内部标志区 (M) |
| 数据块 | 0x84 | 数据块区 (DB) |

### S7 数据类型映射

| S7 类型 | .NET 类型 | 字节数 | 说明 |
|---------|----------|--------|------|
| BOOL | bool | 1 bit | 布尔值 |
| BYTE | byte | 1 | 无符号字节 |
| WORD | ushort | 2 | 16位无符号 |
| DWORD | uint | 4 | 32位无符号 |
| INT | short | 2 | 16位有符号 |
| DINT | int | 4 | 32位有符号 |
| REAL | float | 4 | 单精度浮点 |
| LREAL | double | 8 | 双精度浮点 |
| STRING | string | 可变 | 字符串 |
| DATE_AND_TIME | DateTime | 8 | 日期时间 |

---

<div align="center">
  
  **S7Client API Documentation v1.0**
  
  专业的 S7 协议开发库
  
</div>