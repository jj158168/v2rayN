using System.Collections.Concurrent;

namespace ServiceLib.Manager;

/// <summary>
/// Manages multiple core instances for running multiple nodes simultaneously
/// Each node runs on its own local port
/// </summary>
public class MultiCoreManager
{
    private static readonly Lazy<MultiCoreManager> _instance = new(() => new());
    public static MultiCoreManager Instance => _instance.Value;

    private Config _config;
    private Func<bool, string, Task>? _updateFunc;
    private const string _tag = "MultiCoreManager";

    /// <summary>
    /// Dictionary to track running nodes: IndexId -> (ProcessService, LocalPort)
    /// </summary>
    private readonly ConcurrentDictionary<string, RunningNodeInfo> _runningNodes = new();

    /// <summary>
    /// Event raised when a node's running status changes
    /// </summary>
    public event Action<string, bool, int>? OnNodeStatusChanged;

    public class RunningNodeInfo
    {
        public ProcessService Process { get; set; }
        public int LocalPort { get; set; }
        public ProfileItem Node { get; set; }
        public ECoreType CoreType { get; set; }
    }

    public async Task Init(Config config, Func<bool, string, Task>? updateFunc = null)
    {
        _config = config;
        _updateFunc = updateFunc;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Check if a node is currently running
    /// </summary>
    public bool IsNodeRunning(string indexId)
    {
        return _runningNodes.ContainsKey(indexId) &&
               _runningNodes[indexId].Process != null &&
               !_runningNodes[indexId].Process.HasExited;
    }

    /// <summary>
    /// Get the local port for a running node
    /// </summary>
    public int? GetNodeLocalPort(string indexId)
    {
        if (_runningNodes.TryGetValue(indexId, out var info))
        {
            return info.LocalPort;
        }
        return null;
    }

    /// <summary>
    /// Get all running node IDs
    /// </summary>
    public IEnumerable<string> GetRunningNodeIds()
    {
        return _runningNodes.Keys.ToList();
    }

    /// <summary>
    /// Start a node with its configured custom local port
    /// </summary>
    public async Task<bool> StartNode(ProfileItem node)
    {
        if (node == null || string.IsNullOrEmpty(node.IndexId))
        {
            await UpdateFunc(false, ResUI.CheckServerSettings);
            return false;
        }

        // Check if already running
        if (IsNodeRunning(node.IndexId))
        {
            await UpdateFunc(false, $"Node {node.GetSummary()} is already running");
            return false;
        }

        // Determine local port
        int localPort = node.CustomLocalPort ?? 0;
        if (localPort <= 0)
        {
            // Auto-assign a port if not specified
            localPort = await FindAvailablePort();
            if (localPort <= 0)
            {
                await UpdateFunc(false, "Failed to find available port");
                return false;
            }
        }

        // Check if port is already in use by another running node
        foreach (var kvp in _runningNodes)
        {
            if (kvp.Value.LocalPort == localPort && !kvp.Value.Process.HasExited)
            {
                await UpdateFunc(false, $"Port {localPort} is already in use by another node");
                return false;
            }
        }

        // Generate simplified config for multi-node (no routing rules)
        var fileName = string.Format("multicore_{0}.json", node.IndexId);
        var configPath = Utils.GetBinConfigPath(fileName);

        // Use simplified config generation that doesn't include routing rules
        // This allows each node to work independently as a proxy server
        var result = await CoreConfigHandler.GenerateClientMultiNodeConfig(_config, node, localPort, configPath);

        if (result.Success != true)
        {
            await UpdateFunc(false, result.Msg);
            return false;
        }

        // Start process
        var coreType = AppManager.Instance.GetCoreType(node, node.ConfigType);
        var coreInfo = CoreInfoManager.Instance.GetCoreInfo(coreType);
        var exeFile = CoreInfoManager.Instance.GetCoreExecFile(coreInfo, out var msg);

        if (exeFile.IsNullOrEmpty())
        {
            await UpdateFunc(false, msg);
            return false;
        }

        try
        {
            var environmentVars = new Dictionary<string, string>();
            foreach (var kv in coreInfo.Environment)
            {
                environmentVars[kv.Key] = string.Format(kv.Value, coreInfo.AbsolutePath ? configPath.AppendQuotes() : fileName);
            }

            // Create a wrapper updateFunc that uses NoticeManager if _updateFunc is null
            Func<bool, string, Task> processUpdateFunc = async (notify, msg) =>
            {
                await UpdateFunc(notify, msg);
            };

            var procService = new ProcessService(
                fileName: exeFile,
                arguments: string.Format(coreInfo.Arguments, coreInfo.AbsolutePath ? configPath.AppendQuotes() : fileName),
                workingDirectory: Utils.GetBinConfigPath(),
                displayLog: true,
                redirectInput: false,
                environmentVars: environmentVars,
                updateFunc: processUpdateFunc
            );

            await procService.StartAsync();
            await Task.Delay(500);  // Wait longer to ensure process has time to initialize or fail

            if (procService is null or { HasExited: true })
            {
                await UpdateFunc(true, $"Failed to start node: {node.GetSummary()} - Core exited immediately");
                return false;
            }

            // Store running node info
            _runningNodes[node.IndexId] = new RunningNodeInfo
            {
                Process = procService,
                LocalPort = localPort,
                Node = node,
                CoreType = coreType
            };

            await UpdateFunc(false, $"Started node: {node.GetSummary()} on port {localPort}");

            // Notify status change
            OnNodeStatusChanged?.Invoke(node.IndexId, true, localPort);

            return true;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            await UpdateFunc(false, $"Error starting node: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stop a running node
    /// </summary>
    public async Task<bool> StopNode(string indexId)
    {
        if (!_runningNodes.TryGetValue(indexId, out var info))
        {
            return false;
        }

        try
        {
            if (info.Process != null && !info.Process.HasExited)
            {
                await info.Process.StopAsync();
                info.Process.Dispose();
            }

            _runningNodes.TryRemove(indexId, out _);

            // Clean up config file
            var fileName = string.Format("multicore_{0}.json", indexId);
            var configPath = Utils.GetBinConfigPath(fileName);
            if (File.Exists(configPath))
            {
                try { File.Delete(configPath); } catch { }
            }

            await UpdateFunc(false, $"Stopped node: {info.Node?.GetSummary()}");

            // Notify status change
            OnNodeStatusChanged?.Invoke(indexId, false, 0);

            return true;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return false;
        }
    }

    /// <summary>
    /// Stop all running nodes
    /// </summary>
    public async Task StopAllNodes()
    {
        var nodeIds = _runningNodes.Keys.ToList();
        foreach (var nodeId in nodeIds)
        {
            await StopNode(nodeId);
        }
    }

    /// <summary>
    /// Start multiple nodes at once
    /// </summary>
    public async Task<(int success, int failed)> StartNodes(IEnumerable<ProfileItem> nodes)
    {
        int success = 0;
        int failed = 0;

        foreach (var node in nodes)
        {
            if (await StartNode(node))
            {
                success++;
            }
            else
            {
                failed++;
            }
        }

        return (success, failed);
    }

    /// <summary>
    /// Stop multiple nodes at once
    /// </summary>
    public async Task<(int success, int failed)> StopNodes(IEnumerable<string> indexIds)
    {
        int success = 0;
        int failed = 0;

        foreach (var indexId in indexIds)
        {
            if (await StopNode(indexId))
            {
                success++;
            }
            else
            {
                failed++;
            }
        }

        return (success, failed);
    }

    /// <summary>
    /// Find an available port for a new node
    /// </summary>
    private async Task<int> FindAvailablePort()
    {
        var usedPorts = _runningNodes.Values.Select(x => x.LocalPort).ToHashSet();
        var basePort = AppManager.Instance.GetLocalPort(EInboundProtocol.socks);

        // Start from base port + 100 to avoid conflicts with main instance
        for (int port = basePort + 100; port < Global.MaxPort; port++)
        {
            if (!usedPorts.Contains(port) && !IsPortInUse(port))
            {
                return await Task.FromResult(port);
            }
        }

        return -1;
    }

    /// <summary>
    /// Check if a port is in use
    /// </summary>
    private bool IsPortInUse(int port)
    {
        try
        {
            var ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ipProperties.GetActiveTcpListeners();
            return tcpListeners.Any(ep => ep.Port == port);
        }
        catch
        {
            return false;
        }
    }

    private async Task UpdateFunc(bool notify, string msg)
    {
        if (_updateFunc != null)
        {
            await _updateFunc(notify, msg);
        }
        else
        {
            // Fallback to NoticeManager if no updateFunc provided
            NoticeManager.Instance.SendMessage(msg);
            if (notify)
            {
                NoticeManager.Instance.Enqueue(msg);
            }
        }
    }
}
