# v2rayN Multi-Port Edition

基于 [v2rayN](https://github.com/2dust/v2rayN) 修改，添加多节点同时运行功能。

[![Build Status](https://github.com/YOUR_USERNAME/v2rayN-fork/actions/workflows/test-multiport.yml/badge.svg)](https://github.com/YOUR_USERNAME/v2rayN-fork/actions)
[![GitHub Releases](https://img.shields.io/github/downloads/YOUR_USERNAME/v2rayN-fork/latest/total?logo=github)](https://github.com/YOUR_USERNAME/v2rayN-fork/releases)

> **Note**: 请将上面的 `YOUR_USERNAME` 替换为你的 GitHub 用户名

## ✨ 新增功能

### 多节点同时运行
- 支持同时启动多个代理节点
- 每个节点使用独立的本地端口
- 互不干扰，可同时提供服务

### 自定义本地端口
- 可为每个节点单独设置本地监听端口
- 支持在编辑服务器时配置
- 未设置时自动分配可用端口

### 运行状态显示
- 服务器列表新增"状态"列
- 显示"运行中"或空（已停止）
- 新增"本地端口"列显示当前使用的端口

### 右键菜单操作
- **启动节点**: 启动选中的服务器
- **停止节点**: 停止选中的服务器

## 📦 下载

从 [Releases](../../releases) 页面下载最新版本。

| 平台 | 架构 | 说明 |
|------|------|------|
| Windows | x64 | 大多数 Windows 电脑 |
| Windows | x86 | 32位 Windows 系统 |
| Windows | ARM64 | Surface Pro X, Windows on ARM |
| Linux | x64 | 大多数 Linux 系统 |
| Linux | ARM64 | 树莓派4, ARM 服务器 |
| macOS | x64 | Intel Mac |
| macOS | ARM64 | Apple Silicon (M1/M2/M3) |

## 🔧 使用方法

### 设置自定义本地端口
1. 双击或右键编辑服务器
2. 在"自定义本地端口"字段填入端口号（如 10801）
3. 点击确定保存

### 启动/停止节点
1. 在服务器列表中右键点击目标服务器
2. 选择"启动节点"或"停止节点"
3. 查看"状态"列确认运行状态

### 使用多节点
例如，配置多个节点分别监听不同端口：
- 节点 A: 本地端口 10801 (用于浏览器)
- 节点 B: 本地端口 10802 (用于开发工具)
- 节点 C: 本地端口 10803 (用于其他应用)

各应用配置对应的代理端口即可使用不同节点。

## ⚠️ 注意事项

1. **端口冲突**: 确保每个同时运行的节点使用不同的本地端口
2. **与主节点独立**: 多节点功能与主节点（系统代理）相互独立
3. **资源占用**: 同时运行多个节点会增加系统资源占用

## 🔄 自动更新

本项目会自动检测上游 v2rayN 的新版本发布，并在同步后自动构建新版本。

### GitHub Actions 工作流

| 工作流 | 说明 |
|--------|------|
| `check-upstream-release.yml` | 每天检测上游新版本 |
| `sync-upstream.yml` | 手动触发同步上游代码 |
| `test-multiport.yml` | PR 和推送时自动测试 |
| `release-multiport.yml` | 创建 Release 时自动构建 |

## 🛠️ 本地构建

```bash
# 克隆仓库
git clone https://github.com/YOUR_USERNAME/v2rayN-fork.git
cd v2rayN-fork/v2rayN

# 构建 Windows 版本
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-x64 --self-contained -o publish/win-x64

# 构建 Linux 版本
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r linux-x64 --self-contained -o publish/linux-x64

# 构建 macOS 版本
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r osx-arm64 --self-contained -o publish/osx-arm64
```

## 📝 变更日志

查看 [Releases](../../releases) 了解各版本变更。

## 🔗 相关链接

- [上游项目 v2rayN](https://github.com/2dust/v2rayN)
- [Xray-core](https://github.com/XTLS/Xray-core)
- [sing-box](https://github.com/SagerNet/sing-box)

## 📄 许可证

本项目遵循上游项目的许可证。
