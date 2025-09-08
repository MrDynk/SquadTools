public class PlayerHydrater
{
      public void PopulateSquadPlayerDetailsForRaid(SquadSheetContext context)
    {
        //Build Valid Squad Players List and associate timestamps
        foreach (var logCombatant in context.CombatantInfo)
        {
            var combatantName = logCombatant.Item2;
            var player = context.SquadPlayers
                .FirstOrDefault(p => p.PlayerAliases.Contains(combatantName));

            if (player == null)
            {
                Console.WriteLine($"Detected non squad player in Log {combatantName}");
                continue;
            }

            player.AliasTimeStamps[combatantName].Add(logCombatant.Item1);
            player.PresentInRaid = true;
        }

        //remove players without timestamps
        context.SquadPlayers = context.SquadPlayers
            .Where(p => p.PresentInRaid).ToList();

        //Associate Loot with Squad Players
        //associate loot
        foreach (var item in context.Loot)
        {
            //Console.WriteLine($"Time Looted: {item.TimeStamp} Player: {item.PlayerName} Item: {item.Item} Cost: Could Be Read from discord bot");
            var player = context.SquadPlayers
                .FirstOrDefault(p => p.PlayerAliases.Contains(item.PlayerName));

            if (player == null)
            {
                Console.WriteLine($"Looted item for non squad player {item.PlayerName} Item: {item.Item} Time: {item.TimeStamp}");
                continue;
            }
            player.FatLoot.Add(item);
        }
    }
}