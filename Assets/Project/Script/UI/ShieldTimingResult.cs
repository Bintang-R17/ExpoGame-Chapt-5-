/// <summary>
/// Timing window result for shield/parry system
/// </summary>
public enum ShieldTimingResult
{
    Miss,      // Outside timing window or too early/late
    Good,      // Close to perfect zone
    Perfect    // Inside perfect zone
}
