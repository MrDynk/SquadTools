public class BossHydrater
{
    public void PopulatBossDetailsForRaid(SquadSheetContext context)
    {
        foreach(var death in context.Deaths)
        {
            var player = context.SquadPlayers
                .FirstOrDefault(p => p.PlayerAliases.Contains(death.Item2));

            if (player == null)
            {
                Console.WriteLine($"Death recorded for non squad player {death.Item2} Time: {death.Item1}");
                continue;
            }
            //player.Deaths.Add(death.Item1);
        }
    }
}