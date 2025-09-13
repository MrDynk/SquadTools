public class PlayerActivityGap
{
    public DateTime GapStart { get; set; }
    public DateTime GapEnd { get; set; }
    public TimeSpan GapDuration 
    { 
        get
        {
            return GapEnd - GapStart;
        }
    }
}