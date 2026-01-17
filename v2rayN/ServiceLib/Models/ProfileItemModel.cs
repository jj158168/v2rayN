namespace ServiceLib.Models;

[Serializable]
public class ProfileItemModel : ProfileItem
{
    public bool IsActive { get; set; }
    public string SubRemarks { get; set; }

    /// <summary>
    /// Whether this profile is currently running (multi-port feature)
    /// </summary>
    [Reactive]
    public bool IsRunning { get; set; }

    /// <summary>
    /// Display text for running status
    /// </summary>
    [Reactive]
    public string RunningStatus { get; set; }

    /// <summary>
    /// Display text for local proxy port
    /// </summary>
    [Reactive]
    public string LocalPortDisplay { get; set; }

    [Reactive]
    public int Delay { get; set; }

    public decimal Speed { get; set; }
    public int Sort { get; set; }

    [Reactive]
    public string DelayVal { get; set; }

    [Reactive]
    public string SpeedVal { get; set; }

    [Reactive]
    public string TodayUp { get; set; }

    [Reactive]
    public string TodayDown { get; set; }

    [Reactive]
    public string TotalUp { get; set; }

    [Reactive]
    public string TotalDown { get; set; }
}
