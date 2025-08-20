using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S7DebugTool.Services;
using S7DebugTool.Models;
using System.Collections.Generic;

namespace S7DebugTool.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private S7Client? s7Client;
        private readonly StringBuilder logBuilder = new StringBuilder();

        [ObservableProperty]
        private string ipAddress = "127.0.0.1";

        [ObservableProperty]
        private int rack = 0;

        [ObservableProperty]
        private int slot = 1;

        [ObservableProperty]
        private bool isConnected = false;

        [ObservableProperty]
        private bool canConnect = true;

        [ObservableProperty]
        private string logText = "";

        [ObservableProperty]
        private bool autoScroll = true;

        // 读取相关属性
        [ObservableProperty]
        private string readAreaType = "数据块(DB)";

        [ObservableProperty]
        private int readDbNumber = 1;

        [ObservableProperty]
        private int readStartAddress = 0;

        [ObservableProperty]
        private int readLength = 10;

        [ObservableProperty]
        private string readDataType = "Byte";

        [ObservableProperty]
        private int readBitAddress = 0;

        [ObservableProperty]
        private bool showBitAddress = false;

        [ObservableProperty]
        private string readResult = "";

        [ObservableProperty]
        private bool showHex = true;

        [ObservableProperty]
        private bool showDecimal = false;

        [ObservableProperty]
        private bool showAscii = false;

        [ObservableProperty]
        private bool showDbNumber = true;

        // 写入相关属性
        [ObservableProperty]
        private string writeAreaType = "数据块(DB)";

        [ObservableProperty]
        private int writeDbNumber = 1;

        [ObservableProperty]
        private int writeStartAddress = 0;

        [ObservableProperty]
        private string writeData = "";

        [ObservableProperty]
        private string writeDataType = "Byte";

        [ObservableProperty]
        private int writeBitAddress = 0;

        [ObservableProperty]
        private bool showWriteBitAddress = false;

        [ObservableProperty]
        private string writeDataHint = "输入十六进制值，如: 01 02 03 04";

        [ObservableProperty]
        private bool showWriteDbNumber = true;

        // 批量字节读写属性
        [ObservableProperty]
        private string batchReadAreaType = "数据块(DB)";

        [ObservableProperty]
        private int batchReadDbNumber = 1;

        [ObservableProperty]
        private int batchReadStartAddress = 0;

        [ObservableProperty]
        private int batchReadLength = 100;

        [ObservableProperty]
        private string batchReadResult = "";

        [ObservableProperty]
        private bool batchShowHex = true;

        [ObservableProperty]
        private bool batchShowDecimal = false;

        [ObservableProperty]
        private bool batchShowAscii = false;

        [ObservableProperty]
        private bool showAddress = true;

        [ObservableProperty]
        private bool showBatchDbNumber = true;

        [ObservableProperty]
        private string batchWriteAreaType = "数据块(DB)";

        [ObservableProperty]
        private int batchWriteDbNumber = 1;

        [ObservableProperty]
        private int batchWriteStartAddress = 0;

        [ObservableProperty]
        private string batchWriteData = "";

        [ObservableProperty]
        private bool showBatchWriteDbNumber = true;

        [ObservableProperty]
        private string batchWriteStatus = "";

        [ObservableProperty]
        private string batchWriteStatusColor = "Black";

        // 批量操作
        [ObservableProperty]
        private ObservableCollection<BatchTask> batchTasks = new ObservableCollection<BatchTask>();

        public MainViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReadAreaType))
            {
                ShowDbNumber = ReadAreaType.Contains("DB");
            }
            if (e.PropertyName == nameof(WriteAreaType))
            {
                ShowWriteDbNumber = WriteAreaType.Contains("DB");
            }
            if (e.PropertyName == nameof(IsConnected))
            {
                CanConnect = !IsConnected;
            }
            if (e.PropertyName == nameof(ReadDataType))
            {
                ShowBitAddress = ReadDataType == "Bool";
                UpdateReadLength();
            }
            if (e.PropertyName == nameof(WriteDataType))
            {
                ShowWriteBitAddress = WriteDataType == "Bool";
                UpdateWriteDataHint();
            }
            if (e.PropertyName == nameof(BatchReadAreaType))
            {
                ShowBatchDbNumber = BatchReadAreaType.Contains("DB");
            }
            if (e.PropertyName == nameof(BatchWriteAreaType))
            {
                ShowBatchWriteDbNumber = BatchWriteAreaType.Contains("DB");
            }
        }
        
        private void UpdateReadLength()
        {
            // 根据数据类型自动设置合适的读取长度
            switch (ReadDataType)
            {
                case "Bool":
                case "Byte":
                    ReadLength = 1;
                    break;
                case "Word":
                case "Int":
                    ReadLength = 2;
                    break;
                case "DWord":
                case "DInt":
                case "Real":
                    ReadLength = 4;
                    break;
                case "LReal":
                case "DateTime":
                    ReadLength = 8;
                    break;
                case "String":
                    ReadLength = 256; // 默认字符串长度
                    break;
            }
        }
        
        private void UpdateWriteDataHint()
        {
            // 根据数据类型更新输入提示
            switch (WriteDataType)
            {
                case "Bool":
                    WriteDataHint = "输入: true/false 或 1/0";
                    break;
                case "Byte":
                    WriteDataHint = "输入十六进制值，如: 01 02 03 04 或十进制: 1 2 3 4";
                    break;
                case "Word":
                    WriteDataHint = "输入无符号16位整数 (0-65535)";
                    break;
                case "DWord":
                    WriteDataHint = "输入无符号32位整数 (0-4294967295)";
                    break;
                case "Int":
                    WriteDataHint = "输入有符号16位整数 (-32768 到 32767)";
                    break;
                case "DInt":
                    WriteDataHint = "输入有符号32位整数 (-2147483648 到 2147483647)";
                    break;
                case "Real":
                    WriteDataHint = "输入浮点数，如: 3.14159 或 -123.456";
                    break;
                case "LReal":
                    WriteDataHint = "输入双精度浮点数，如: 3.141592653589793";
                    break;
                case "String":
                    WriteDataHint = "输入字符串文本";
                    break;
                case "DateTime":
                    WriteDataHint = "输入日期时间，格式: yyyy-MM-dd HH:mm:ss 或 yyyy-MM-dd HH:mm:ss.fff";
                    break;
                default:
                    WriteDataHint = "输入数据";
                    break;
            }
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                CanConnect = false;
                AddLog($"正在连接到 {IpAddress}...");
                
                s7Client = new S7Client(IpAddress, Rack, Slot);
                s7Client.LogMessage += OnS7ClientLog;
                
                bool success = await s7Client.ConnectAsync();
                
                if (success)
                {
                    IsConnected = true;
                    AddLog("连接成功！");
                }
                else
                {
                    AddLog("连接失败！");
                    CanConnect = true;
                }
            }
            catch (Exception ex)
            {
                AddLog($"连接错误: {ex.Message}");
                CanConnect = true;
            }
        }

        [RelayCommand]
        private void Disconnect()
        {
            try
            {
                s7Client?.Disconnect();
                s7Client?.Dispose();
                s7Client = null;
                IsConnected = false;
                CanConnect = true;
                AddLog("已断开连接");
            }
            catch (Exception ex)
            {
                AddLog($"断开连接错误: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ReadAsync()
        {
            if (s7Client == null || !IsConnected) return;

            try
            {
                AddLog($"读取 {ReadAreaType} - 地址:{ReadStartAddress} 类型:{ReadDataType}");
                
                // 只支持DB块的数据类型读取
                if (!ReadAreaType.Contains("DB") && !ReadAreaType.Contains("数据块"))
                {
                    // 对于非DB块，使用原始字节读取
                    byte[]? data = null;
                    if (ReadAreaType.Contains("I") || ReadAreaType.Contains("输入"))
                    {
                        data = await Task.Run(() => s7Client.ReadInput(ReadStartAddress, ReadLength));
                    }
                    else if (ReadAreaType.Contains("Q") || ReadAreaType.Contains("输出"))
                    {
                        data = await Task.Run(() => s7Client.ReadOutput(ReadStartAddress, ReadLength));
                    }
                    else if (ReadAreaType.Contains("M") || ReadAreaType.Contains("标志"))
                    {
                        data = await Task.Run(() => s7Client.ReadMemory(ReadStartAddress, ReadLength));
                    }
                    
                    if (data != null)
                    {
                        ReadResult = FormatData(data, ShowHex, ShowDecimal, ShowAscii);
                        AddLog($"读取成功，返回 {data.Length} 字节");
                    }
                    else
                    {
                        ReadResult = "读取失败";
                        AddLog("读取失败");
                    }
                    return;
                }
                
                // DB块支持各种数据类型
                string result = "";
                bool success = false;
                
                switch (ReadDataType)
                {
                    case "Bool":
                        var boolValue = await s7Client.ReadBitAsync(ReadDbNumber, ReadStartAddress, ReadBitAddress);
                        if (boolValue.HasValue)
                        {
                            result = boolValue.Value ? "TRUE" : "FALSE";
                            success = true;
                        }
                        break;
                        
                    case "Byte":
                        var byteData = await s7Client.ReadDBAsync(ReadDbNumber, ReadStartAddress, ReadLength);
                        if (byteData != null)
                        {
                            result = FormatData(byteData, ShowHex, ShowDecimal, ShowAscii);
                            success = true;
                        }
                        break;
                        
                    case "Word":
                        var wordValue = await s7Client.ReadWordAsync(ReadDbNumber, ReadStartAddress);
                        if (wordValue.HasValue)
                        {
                            result = ShowHex ? $"0x{wordValue.Value:X4}" : wordValue.Value.ToString();
                            success = true;
                        }
                        break;
                        
                    case "DWord":
                        var dwordValue = await s7Client.ReadDWordAsync(ReadDbNumber, ReadStartAddress);
                        if (dwordValue.HasValue)
                        {
                            result = ShowHex ? $"0x{dwordValue.Value:X8}" : dwordValue.Value.ToString();
                            success = true;
                        }
                        break;
                        
                    case "Int":
                        var intValue = await s7Client.ReadIntAsync(ReadDbNumber, ReadStartAddress);
                        if (intValue.HasValue)
                        {
                            result = intValue.Value.ToString();
                            success = true;
                        }
                        break;
                        
                    case "DInt":
                        var dintValue = await s7Client.ReadDIntAsync(ReadDbNumber, ReadStartAddress);
                        if (dintValue.HasValue)
                        {
                            result = dintValue.Value.ToString();
                            success = true;
                        }
                        break;
                        
                    case "Real":
                        var realValue = await s7Client.ReadRealAsync(ReadDbNumber, ReadStartAddress);
                        if (realValue.HasValue)
                        {
                            result = realValue.Value.ToString("F6");
                            success = true;
                        }
                        break;
                        
                    case "LReal":
                        var lrealValue = await s7Client.ReadLRealAsync(ReadDbNumber, ReadStartAddress);
                        if (lrealValue.HasValue)
                        {
                            result = lrealValue.Value.ToString("F10");
                            success = true;
                        }
                        break;
                        
                    case "String":
                        var stringValue = await s7Client.ReadStringAsync(ReadDbNumber, ReadStartAddress, ReadLength - 2);
                        if (stringValue != null)
                        {
                            result = $"\"{stringValue}\"";
                            success = true;
                        }
                        break;
                        
                    case "DateTime":
                        var dateValue = await s7Client.ReadDateTimeAsync(ReadDbNumber, ReadStartAddress);
                        if (dateValue.HasValue)
                        {
                            result = dateValue.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            success = true;
                        }
                        break;
                        
                    default:
                        // 默认按字节读取
                        var defaultData = await s7Client.ReadDBAsync(ReadDbNumber, ReadStartAddress, ReadLength);
                        if (defaultData != null)
                        {
                            result = FormatData(defaultData, ShowHex, ShowDecimal, ShowAscii);
                            success = true;
                        }
                        break;
                }
                
                if (success)
                {
                    ReadResult = result;
                    AddLog($"读取成功: {result}");
                }
                else
                {
                    ReadResult = "读取失败";
                    AddLog("读取失败");
                }
            }
            catch (Exception ex)
            {
                AddLog($"读取错误: {ex.Message}");
                ReadResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task WriteAsync()
        {
            if (s7Client == null || !IsConnected) return;

            try
            {
                // 只支持DB块的数据类型写入
                if (!WriteAreaType.Contains("DB") && !WriteAreaType.Contains("数据块"))
                {
                    AddLog("数据类型写入仅支持DB块");
                    return;
                }

                AddLog($"写入 {WriteAreaType} - 地址:{WriteStartAddress} 类型:{WriteDataType} 值:{WriteData}");
                
                bool success = false;
                string inputValue = WriteData.Trim();
                
                switch (WriteDataType)
                {
                    case "Bool":
                        if (TryParseBool(inputValue, out bool boolValue))
                        {
                            success = await s7Client.WriteBitAsync(WriteDbNumber, WriteStartAddress, WriteBitAddress, boolValue);
                        }
                        else
                        {
                            AddLog("无效的布尔值，请输入 true/false 或 1/0");
                            return;
                        }
                        break;
                        
                    case "Byte":
                        byte[] byteData = ParseByteArray(inputValue);
                        if (byteData.Length > 0)
                        {
                            success = await s7Client.WriteDBAsync(WriteDbNumber, WriteStartAddress, byteData);
                        }
                        else
                        {
                            AddLog("无效的字节数据");
                            return;
                        }
                        break;
                        
                    case "Word":
                        if (ushort.TryParse(inputValue, out ushort wordValue))
                        {
                            success = await s7Client.WriteWordAsync(WriteDbNumber, WriteStartAddress, wordValue);
                        }
                        else
                        {
                            AddLog("无效的Word值 (0-65535)");
                            return;
                        }
                        break;
                        
                    case "DWord":
                        if (uint.TryParse(inputValue, out uint dwordValue))
                        {
                            success = await s7Client.WriteDWordAsync(WriteDbNumber, WriteStartAddress, dwordValue);
                        }
                        else
                        {
                            AddLog("无效的DWord值 (0-4294967295)");
                            return;
                        }
                        break;
                        
                    case "Int":
                        if (short.TryParse(inputValue, out short intValue))
                        {
                            success = await s7Client.WriteIntAsync(WriteDbNumber, WriteStartAddress, intValue);
                        }
                        else
                        {
                            AddLog("无效的Int值 (-32768 到 32767)");
                            return;
                        }
                        break;
                        
                    case "DInt":
                        if (int.TryParse(inputValue, out int dintValue))
                        {
                            success = await s7Client.WriteDIntAsync(WriteDbNumber, WriteStartAddress, dintValue);
                        }
                        else
                        {
                            AddLog("无效的DInt值 (-2147483648 到 2147483647)");
                            return;
                        }
                        break;
                        
                    case "Real":
                        if (float.TryParse(inputValue, System.Globalization.NumberStyles.Float, 
                            System.Globalization.CultureInfo.InvariantCulture, out float realValue))
                        {
                            success = await s7Client.WriteRealAsync(WriteDbNumber, WriteStartAddress, realValue);
                        }
                        else
                        {
                            AddLog("无效的Real值");
                            return;
                        }
                        break;
                        
                    case "LReal":
                        if (double.TryParse(inputValue, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double lrealValue))
                        {
                            success = await s7Client.WriteLRealAsync(WriteDbNumber, WriteStartAddress, lrealValue);
                        }
                        else
                        {
                            AddLog("无效的LReal值");
                            return;
                        }
                        break;
                        
                    case "String":
                        // 从输入中提取字符串长度（可选）
                        int maxLength = 254;
                        success = await s7Client.WriteStringAsync(WriteDbNumber, WriteStartAddress, inputValue, maxLength);
                        break;
                        
                    case "DateTime":
                        if (DateTime.TryParse(inputValue, out DateTime dateValue))
                        {
                            success = await s7Client.WriteDateTimeAsync(WriteDbNumber, WriteStartAddress, dateValue);
                        }
                        else
                        {
                            AddLog("无效的日期时间格式");
                            return;
                        }
                        break;
                        
                    default:
                        AddLog($"不支持的数据类型: {WriteDataType}");
                        return;
                }

                if (success)
                {
                    AddLog("写入成功");
                }
                else
                {
                    AddLog("写入失败");
                }
            }
            catch (Exception ex)
            {
                AddLog($"写入错误: {ex.Message}");
            }
        }
        
        private bool TryParseBool(string input, out bool value)
        {
            input = input.ToLower().Trim();
            if (input == "true" || input == "1")
            {
                value = true;
                return true;
            }
            else if (input == "false" || input == "0")
            {
                value = false;
                return true;
            }
            value = false;
            return false;
        }
        
        private byte[] ParseByteArray(string input)
        {
            try
            {
                // 移除所有空格、逗号、破折号
                input = input.Replace(" ", "").Replace(",", "").Replace("-", "");
                
                // 检查是否为十六进制
                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(2);
                }
                
                // 尝试作为十六进制解析
                if (input.All(c => "0123456789ABCDEFabcdef".Contains(c)))
                {
                    // 十六进制字符串
                    if (input.Length % 2 != 0)
                        input = "0" + input; // 补齐为偶数位
                    
                    byte[] result = new byte[input.Length / 2];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
                    }
                    return result;
                }
                
                // 尝试作为十进制数组解析（空格分隔）
                string[] parts = WriteData.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    List<byte> bytes = new List<byte>();
                    foreach (string part in parts)
                    {
                        if (byte.TryParse(part, out byte b))
                        {
                            bytes.Add(b);
                        }
                        else
                        {
                            return Array.Empty<byte>();
                        }
                    }
                    return bytes.ToArray();
                }
            }
            catch
            {
                // 解析失败
            }
            
            return Array.Empty<byte>();
        }

        [RelayCommand]
        private async Task BatchReadAsync()
        {
            if (s7Client == null || !IsConnected) return;

            try
            {
                AddLog($"批量读取 {BatchReadAreaType} - 地址:{BatchReadStartAddress} 长度:{BatchReadLength}");
                
                byte[]? data = null;
                
                if (BatchReadAreaType.Contains("DB") || BatchReadAreaType.Contains("数据块"))
                {
                    data = await s7Client.ReadDBAsync(BatchReadDbNumber, BatchReadStartAddress, BatchReadLength);
                }
                else if (BatchReadAreaType.Contains("I") || BatchReadAreaType.Contains("输入"))
                {
                    data = await Task.Run(() => s7Client.ReadInput(BatchReadStartAddress, BatchReadLength));
                }
                else if (BatchReadAreaType.Contains("Q") || BatchReadAreaType.Contains("输出"))
                {
                    data = await Task.Run(() => s7Client.ReadOutput(BatchReadStartAddress, BatchReadLength));
                }
                else if (BatchReadAreaType.Contains("M") || BatchReadAreaType.Contains("标志"))
                {
                    data = await Task.Run(() => s7Client.ReadMemory(BatchReadStartAddress, BatchReadLength));
                }

                if (data != null)
                {
                    BatchReadResult = FormatBatchData(data, BatchShowHex, BatchShowDecimal, BatchShowAscii, ShowAddress, BatchReadStartAddress);
                    AddLog($"批量读取成功，返回 {data.Length} 字节");
                }
                else
                {
                    BatchReadResult = "读取失败";
                    AddLog("批量读取失败");
                }
            }
            catch (Exception ex)
            {
                AddLog($"批量读取错误: {ex.Message}");
                BatchReadResult = $"错误: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ClearBatchRead()
        {
            BatchReadResult = "";
        }

        [RelayCommand]
        private async Task BatchWriteAsync()
        {
            if (s7Client == null || !IsConnected) return;

            try
            {
                byte[] data = ParseByteArray(BatchWriteData);
                
                if (data.Length == 0)
                {
                    BatchWriteStatus = "数据为空";
                    BatchWriteStatusColor = "Red";
                    AddLog("批量写入数据为空");
                    return;
                }

                AddLog($"批量写入 {BatchWriteAreaType} - 地址:{BatchWriteStartAddress} 长度:{data.Length}");
                
                bool success = false;
                
                if (BatchWriteAreaType.Contains("DB") || BatchWriteAreaType.Contains("数据块"))
                {
                    success = await s7Client.WriteDBAsync(BatchWriteDbNumber, BatchWriteStartAddress, data);
                }
                // 其他区域的写入需要实现

                if (success)
                {
                    BatchWriteStatus = $"写入成功 ({data.Length}字节)";
                    BatchWriteStatusColor = "Green";
                    AddLog($"批量写入成功，{data.Length} 字节");
                }
                else
                {
                    BatchWriteStatus = "写入失败";
                    BatchWriteStatusColor = "Red";
                    AddLog("批量写入失败");
                }
            }
            catch (Exception ex)
            {
                BatchWriteStatus = $"错误: {ex.Message}";
                BatchWriteStatusColor = "Red";
                AddLog($"批量写入错误: {ex.Message}");
            }
        }

        private string FormatBatchData(byte[] data, bool hex, bool dec, bool ascii, bool showAddr, int startAddress)
        {
            StringBuilder sb = new StringBuilder();
            
            if (hex)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i % 16 == 0)
                    {
                        if (i > 0) sb.AppendLine();
                        if (showAddr)
                        {
                            sb.Append($"{(startAddress + i):D6}: ");
                        }
                    }
                    else if (i > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(data[i].ToString("X2"));
                }
            }
            else if (dec)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i % 16 == 0)
                    {
                        if (i > 0) sb.AppendLine();
                        if (showAddr)
                        {
                            sb.Append($"{(startAddress + i):D6}: ");
                        }
                    }
                    else if (i > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(data[i].ToString().PadLeft(3));
                }
            }
            else if (ascii)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (showAddr && i % 32 == 0)
                    {
                        if (i > 0) sb.AppendLine();
                        sb.Append($"{(startAddress + i):D6}: ");
                    }
                    
                    if (data[i] >= 32 && data[i] <= 126)
                    {
                        sb.Append((char)data[i]);
                    }
                    else
                    {
                        sb.Append('.');
                    }
                }
            }
            
            return sb.ToString();
        }

        [RelayCommand]
        private void ClearLog()
        {
            logBuilder.Clear();
            LogText = "";
        }

        [RelayCommand]
        private void AddBatchTask()
        {
            BatchTasks.Add(new BatchTask());
        }

        [RelayCommand]
        private void RemoveBatchTask()
        {
            var selected = BatchTasks.FirstOrDefault(t => t.IsSelected);
            if (selected != null)
            {
                BatchTasks.Remove(selected);
            }
        }

        [RelayCommand]
        private async Task ExecuteBatchAsync()
        {
            if (s7Client == null || !IsConnected) return;

            foreach (var task in BatchTasks.Where(t => t.IsEnabled))
            {
                try
                {
                    if (task.Operation == "读取")
                    {
                        var data = await s7Client.ReadDBAsync(task.DbNumber, task.Address, task.Length);
                        if (data != null)
                        {
                            task.Result = BitConverter.ToString(data);
                        }
                        else
                        {
                            task.Result = "读取失败";
                        }
                    }
                    else if (task.Operation == "写入")
                    {
                        var writeData = ParseData(task.Data, true, false, false);
                        var success = await s7Client.WriteDBAsync(task.DbNumber, task.Address, writeData);
                        task.Result = success ? "写入成功" : "写入失败";
                    }
                }
                catch (Exception ex)
                {
                    task.Result = $"错误: {ex.Message}";
                }
                
                await Task.Delay(100); // 避免过快执行
            }
        }

        [RelayCommand]
        private void ClearBatchResults()
        {
            foreach (var task in BatchTasks)
            {
                task.Result = "";
            }
        }

        private void OnS7ClientLog(object? sender, string message)
        {
            AddLog($"[S7] {message}");
        }

        private void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}\n";
            
            logBuilder.Append(logEntry);
            LogText = logBuilder.ToString();
        }

        private string FormatData(byte[] data, bool hex, bool dec, bool ascii)
        {
            if (hex)
            {
                // Format hex data with line breaks every 16 bytes for better readability
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    if (i > 0 && i % 16 == 0)
                    {
                        sb.AppendLine();  // New line every 16 bytes
                    }
                    else if (i > 0)
                    {
                        sb.Append(" ");   // Space between bytes
                    }
                    sb.Append(data[i].ToString("X2"));
                }
                return sb.ToString();
            }
            else if (dec)
            {
                // Format decimal data with line breaks every 16 values
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    if (i > 0 && i % 16 == 0)
                    {
                        sb.AppendLine();
                    }
                    else if (i > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(data[i].ToString().PadLeft(3));  // Pad to 3 digits for alignment
                }
                return sb.ToString();
            }
            else if (ascii)
            {
                // For ASCII, replace non-printable characters with dots
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                {
                    if (b >= 32 && b <= 126)  // Printable ASCII range
                    {
                        sb.Append((char)b);
                    }
                    else
                    {
                        sb.Append('.');  // Non-printable character
                    }
                }
                return sb.ToString();
            }
            
            return BitConverter.ToString(data);
        }

        private byte[] ParseData(string input, bool hex, bool dec, bool ascii)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return Array.Empty<byte>();

                if (hex)
                {
                    input = input.Replace(" ", "").Replace("-", "");
                    int length = input.Length / 2;
                    byte[] data = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
                    }
                    return data;
                }
                else if (dec)
                {
                    string[] parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Select(p => Convert.ToByte(p)).ToArray();
                }
                else if (ascii)
                {
                    return Encoding.ASCII.GetBytes(input);
                }
            }
            catch
            {
                // 解析失败返回空数组
            }

            return Array.Empty<byte>();
        }
    }
}