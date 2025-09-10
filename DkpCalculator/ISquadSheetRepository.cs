namespace SquadSheets
{
public interface ISquadSheetRepository
{
    // Retrieves squad sheet data and updates the context
    void GetRosterDetails(SquadSheetContext context);

    void UpdateDkp(SquadSheetContext context);
    
}
}