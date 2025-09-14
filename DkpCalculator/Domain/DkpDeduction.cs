public class DkpDeduction
{
    public DeductionReasonEnum Reason { get; set; }
    public int Amount { get; set; }
    public List<DateTime>? RelatedTimeStamps { get; set; }
}