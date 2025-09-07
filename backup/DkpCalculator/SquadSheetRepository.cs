namespace SquadSheets
{
    public class SquadSheetRepository: ISquadSheetRepository
{
        private string squadSheetPath;

        public SquadSheetRepository(string squadSheetPath)
        {
            this.squadSheetPath = squadSheetPath;
        }

        // Retrieves squad sheet data and updates the context
        public void GetRosterDetails(SquadSheetContext context)
        {
        
        }

        public void UpdateDkp(SquadSheetContext context){
            
        }
    }
}