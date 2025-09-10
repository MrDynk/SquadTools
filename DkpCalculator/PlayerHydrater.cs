public class PlayerHydrater
{
    public void PopulateSquadPlayerDetailsForRaid(SquadSheetContext context)
    {
        //Build Valid Squad Players List and associate Combatant info Timestamps
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

        //associate activity gaps
        foreach (var player in context.SquadPlayers)
        {
            foreach (var alias in player.PlayerAliases)
            {
                if (!context.AliasTimeStamps.TryGetValue(alias, out var times) || times.Count < 2)
                    continue;

                times.Sort();
                for (int i = 1; i < times.Count; i++)
                {
                    var diff = times[i] - times[i - 1];
                    if (diff.TotalMinutes >= ApplicationOptions.InactivityThresholdMinutes)
                    {
                        //Console.WriteLine($"Player {alias} inactive from {times[i - 1]:HH:mm:ss} to {times[i]:HH:mm:ss} ({diff.TotalMinutes:F1} min)");
                        PlayerActivityGap activityGap = new PlayerActivityGap
                        {
                            GapStart = times[i - 1],
                            GapEnd = times[i],
                        };

                        player.ActivityGaps.Add(activityGap);
                    }
                }

            }
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


        foreach (var death in context.Deaths)
        {
            var player = context.SquadPlayers
                .FirstOrDefault(p => p.PlayerAliases.Contains(death.Item2));

            if (player != null)
            {
                player.Deaths.Add(death.Item1);
            }
        }
    }
}