# S7协议原生通信实现指南

## 协议概述

S7协议是西门子专有的工业通信协议，用于与S7系列PLC（S7-300/400/1200/1500）进行通信。该协议基于ISO-on-TCP（RFC1006）实现，使用TCP端口102进行通信。

## 协议栈结构

```
┌─────────────────┐
│   S7 Protocol   │  应用层：S7命令和数据
├─────────────────┤
│   ISO-COTP      │  传输层：连接导向传输协议
├─────────────────┤
│     TPKT        │  会话层：ISO传输服务
├─────────────────┤
│    TCP/IP       │  网络层：TCP端口102
└─────────────────┘
```

## 连接建立流程

### 第1步：TCP连接
- 连接到PLC的IP地址，端口102

### 第2步：COTP连接请求
- 发送COTP CR（Connection Request）包
- 接收COTP CC（Connection Confirm）包

### 第3步：S7通信设置
- 发送S7 Communication Setup请求
- 协商PDU大小和并行作业数

## 数据包结构

### TPKT头部（4字节）
```
字节0: 版本号 (0x03)
字节1: 保留 (0x00)
字节2-3: 包长度（包括TPKT头）
```

### COTP头部
```
字节0: 长度指示器
字节1: PDU类型
  - 0xE0: 连接请求(CR)
  - 0xD0: 连接确认(CC)
  - 0xF0: 数据传输(DT)
  - 0x80: 断开请求(DR)
后续字节: 根据PDU类型变化
```

### S7协议头部
```
字节0: 协议标识符 (0x32)
字节1: 消息类型
  - 0x01: Job Request
  - 0x02: Ack
  - 0x03: Ack_Data
  - 0x07: Userdata
字节2-3: 保留 (0x0000)
字节4-5: PDU参考
字节6-7: 参数长度
字节8-9: 数据长度
```

## 基本操作实现

### 1. 读取数据 (Read Variable)

功能码：0x04

参数结构：
- Function Code: 0x04
- Item Count: 项目数量
- Item结构：
  - Variable Specification: 0x12
  - Length: 0x0A
  - Syntax ID: 0x10 (S7ANY)
  - Transport Size: 数据类型
  - Length: 数据长度
  - DB Number: DB块号
  - Area: 内存区域 (0x84=DB, 0x81=I, 0x82=Q, 0x83=M)
  - Address: 起始地址

### 2. 写入数据 (Write Variable)

功能码：0x05

参数结构类似读取，但需要附加数据段：
- Return Code: 返回码
- Transport Size: 传输大小
- Data: 实际数据

### 3. 内存区域代码

```
0x81: 输入区 (I)
0x82: 输出区 (Q)
0x83: 标志位 (M)
0x84: 数据块 (DB)
0x85: 实例数据块 (DI)
0x86: 本地数据 (L)
0x87: 前一个本地数据 (V)
```

### 4. 数据类型代码

```
0x01: BIT
0x02: BYTE
0x03: CHAR
0x04: WORD
0x05: INT
0x06: DWORD
0x07: DINT
0x08: REAL
```

## 通信示例流程

```
客户端                          PLC
  |                              |
  |-------- TCP SYN ------------>|
  |<------- TCP SYN-ACK ---------|
  |-------- TCP ACK ------------>|
  |                              |
  |-------- COTP CR ------------>|
  |<------- COTP CC -------------|
  |                              |
  |---- S7 Comm Setup Req ------>|
  |<--- S7 Comm Setup Ack -------|
  |                              |
  |---- S7 Read Request -------->|
  |<--- S7 Read Response ---------|
  |                              |
  |---- S7 Write Request ------->|
  |<--- S7 Write Response -------|
  |                              |
```

## 实现注意事项

1. **字节序**：S7协议使用大端字节序（Big-Endian）
2. **PDU大小**：默认240字节，最大960字节，需在连接时协商
3. **并行作业**：可同时发送多个请求，数量在连接时协商
4. **错误处理**：检查每个响应的错误码字段
5. **连接保持**：定期发送心跳包维持连接

## 推荐的开源实现

1. **Snap7**：C/C++实现，支持多平台
2. **Sharp7**：C#实现，适合.NET开发
3. **node-s7**：Node.js实现
4. **python-snap7**：Python封装

## 调试工具

- **Wireshark**：内置S7comm解析器
- **S7 Client Demo**：测试连接和基本操作
- **PLC模拟器**：用于开发测试

## 安全建议

1. 使用防火墙限制访问
2. 启用PLC的访问控制功能
3. 监控异常通信模式
4. 定期更新PLC固件
5. 使用VPN或专用网络

通过理解这些基础知识，你可以实现自己的S7协议通信程序，与西门子PLC进行原生通信。