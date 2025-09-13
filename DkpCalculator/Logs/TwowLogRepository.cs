
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Presentation;

namespace Logs
{
    public class TwowLogRepository : ILogRepository
    {

        private string _logDirectory;
        public TwowLogRepository(string logDirectory)
        {
            _logDirectory = logDirectory;
        }
        public void GetPriliminaryDataPoints(SquadSheetContext context)
        {
            context.ZoneInfo = new List<Tuple<DateTime, string>>();
            context.CombatantInfo = new List<Tuple<DateTime, string>>();
            context.Deaths = new List<Tuple<DateTime, string>>();
            context.Loot = new List<Loot>();
            context.BossesDefeated = new List<Boss>();

            var allLines = File.ReadAllLines(_logDirectory);
            List<string> zonesEncountered = new List<string>();
            List<(string, string)> potentialBosses = new List<(string, string)>();
            foreach (var line in allLines)
            {
                //9/3 20:47:06.469  ZONE_INFO: 03.09.25 20:47:06&naxxramas&0
                if (line.Contains(" ZONE_INFO") && !line.Contains("LOOT"))
                {
                    string zone = ParseZoneInfo(context, line);
                    if (!string.IsNullOrEmpty(zone) && !zonesEncountered.Contains(zone, StringComparer.OrdinalIgnoreCase))
                    {
                        BuildPotentialBosses(zone, potentialBosses);
                        zonesEncountered.Add(zone);
                    }

                    
                    continue;

                }

                //9/3 20:45:46.437  COMBATANT_INFO: 03.09.25 20:45:46&Holecloser&PRIEST&Human&3&nil&SQUAD&Battle Brother&4&nil&21507:440:0:0&22515:0:0:0&nil&nil&nil&nil&nil&22519:2566:0:0&nil&17110:440:0:0&19863:440:0:0&18469:0:0:0&19950:0:0:0&nil&nil&nil&21801:0:0:0&nil&nil
                if (line.Contains("COMBATANT_INFO"))
                {
                    bool flowControl = ParseCombatantInfo(context, line);
                    if (!flowControl)
                    {
                        continue;
                    }
                    continue;

                }
                //9/3 21:08:15.762  Sharkblood dies.
                if (line.Contains("dies"))
                {
                    // Example line: 9/3 21:08:15.762  Sharkblood dies.
                    // 1. Extract DateTime from the start of the line
                    int spaceIdx = line.IndexOf(' ');
                    if (spaceIdx > 0)
                    {
                        string dateTimeStr = line.Substring(0, line.IndexOf(' ', line.IndexOf(' ') + 1)).Trim();
                        DateTime timestamp;
                        if (DateTime.TryParseExact(dateTimeStr, "M/d HH:mm:ss.fff", null, System.Globalization.DateTimeStyles.None, out timestamp))
                        {

                            string afterDate = line.Substring(dateTimeStr.Length).Trim();

                            // 2. Extract text between end of DateTime and the word 'dies'
                            int diesIdx = afterDate.IndexOf("dies", spaceIdx) - 1;
                            if (diesIdx > 0)
                            {
                                string between = afterDate.Substring(0, diesIdx);
                                //.Replace(".dies", "").Replace("dies", "").Trim();
                                if (!string.IsNullOrEmpty(between))
                                {

                                    var bossTuple = potentialBosses.FirstOrDefault(p => p.Item1.Equals(between, StringComparison.OrdinalIgnoreCase));
                                    if (bossTuple != default)
                                    {
                                        Boss boss = new Boss
                                        {
                                            Zone = bossTuple.Item2,
                                            Name = bossTuple.Item1,
                                            KillTime = timestamp
                                        };
                                        context.BossesDefeated.Add(boss);
                                    }
                                    else
                                    {
                                        context.Deaths.Add(new Tuple<DateTime, string>(timestamp, between));
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }



                //9/3 21:16:23.404  LOOT_TRADE: 03.09.25 21:16:23&Brakin trades item Widow's Remorse to Dwynk.
                if (line.Contains("LOOT_TRADE"))
                {
                    /*Console.WriteLine("Loot Trade logic not yet implemented, need to grab from and too player names and fix player objects accordingly");
                    Console.WriteLine(line);
                    Console.WriteLine("---");
                    */
                    continue;
                }

                //9/3 21:14:54.005  LOOT: 03.09.25 21:14:54&Turthot receives loot: |cffa335ee|Hitem:22362:0:0:0|h[Desecrated Wristguards]|h|rx1.
                //9/3 20:54:59.462  LOOT: 03.09.25 20:54:59&Dwynk receives item: |cffffffff|Hitem:83004:0:0:0|h[Conjured Mana Orange]|h|rx20.
                if (line.Contains("LOOT") && !line.Contains("receives item:"))
                {
                    var tokens = line.Split('&');
                    if (tokens.Length > 1)
                    {
                        var timestampStr = tokens[0].Substring(0, line.IndexOf(" LOOT:")).Trim();
                        DateTime timestamp;
                        if (DateTime.TryParseExact(timestampStr, "M/d HH:mm:ss.fff", null,
                                                    System.Globalization.DateTimeStyles.None, out timestamp))
                        {
                            var playerPart = tokens[1];
                            var playerNameEndIdx = playerPart.IndexOf(" receives loot:");
                            if (playerNameEndIdx > 0)
                            {
                                var playerName = playerPart.Substring(0, playerNameEndIdx).Trim();
                                var itemPart = playerPart.Substring(playerNameEndIdx + " receives loot:".Length).Trim();
                                if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(itemPart))
                                {
                                    string itemName = itemPart.Substring(itemPart.IndexOf('[') + 1, itemPart.IndexOf(']') - itemPart.IndexOf('[') - 1);
                                    context.Loot.Add(new Loot
                                    {
                                        TimeStamp = timestamp,
                                        PlayerName = playerName,
                                        Item = itemName
                                    });
                                }
                            }
                        }
                    }
                }
            }
            AwardZoneDkp(context);
        }
        private void AwardZoneDkp(SquadSheetContext context)
        {
            var uniqueZones = context.ZoneInfo.Select(z => z.Item2).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var zone in uniqueZones)
            {
                // Find all bosses for the given zone
                if (ApplicationOptions.DkpPotential.TryGetValue(zone, out var zoneDkp))
                {
                    context.PotentialDkpEarnedForRaid += zoneDkp;
                }
            }
        }

        private void BuildPotentialBosses(string zone, List<(string,string)> potentialBosses)
        {
                // Find all bosses for the given zone
                if (ApplicationOptions.bossNames.TryGetValue(zone, out var bosses))
                {
                    potentialBosses.AddRange(bosses.Select(b => (b, zone)));
                }
        }

        public void GetPlayerActivity(SquadSheetContext context)
        {
            // Read all log lines
            var allLines = File.ReadAllLines(_logDirectory);
            context.AliasTimeStamps = new Dictionary<string, List<DateTime>>(StringComparer.OrdinalIgnoreCase);
            List<string> allAliases = new List<string>();

            // Build a list of timestamps for each player
            foreach (var player in context.SquadPlayers)
            {
                foreach (var alias in player.PlayerAliases)
                {
                    if (player.AliasTimeStamps[alias].Count > 0)
                    {
                        context.AliasTimeStamps[alias] = new List<DateTime>();
                        allAliases.Add(alias);
                    }
                }
            }

            string dateDelimeter = "  ";
            foreach (var line in allLines)
            {
                //ignore time outside of defined raid start and end   
                var timestampStr = line.Substring(0, line.IndexOf(dateDelimeter)).Trim();
                DateTime timestamp;
                if (!DateTime.TryParseExact(timestampStr, "M/d HH:mm:ss.fff", null,
                                            System.Globalization.DateTimeStyles.None, out timestamp))
                {
                    continue;
                }

                
                foreach (var alias in allAliases)
                    {
                        if (line.Contains(alias, StringComparison.OrdinalIgnoreCase))
                        {
                            context.AliasTimeStamps[alias].Add(timestamp);
                            break;
                        }
                    }
            }
        }

        public void FindFirstActivityPriorToEventTime(string identifier, DateTime eventTime)
        {
            throw new NotImplementedException();
        }

        private static bool ParseCombatantInfo(SquadSheetContext context, string line)
        {
            var timestampStr = line.Substring(0, line.IndexOf(" COMBATANT_INFO:")).Trim();
            DateTime timestamp;
            string playerName = string.Empty;
            if (!DateTime.TryParseExact(timestampStr, "M/d HH:mm:ss.fff", null,
                                        System.Globalization.DateTimeStyles.None, out timestamp))
            {
                Console.WriteLine($"Failed to parse timestamp: {timestampStr}");
            }
            var parts = line.Split('&');
            if (parts.Length > 1)
            {
                playerName = parts[1].Trim().ToLower();
                if (string.IsNullOrEmpty(playerName))
                {
                    Console.WriteLine($"Player name is empty in line: {line}");
                    return false;
                }

            }

            if (!String.IsNullOrEmpty(playerName) && timestamp != default)
            {
                context.CombatantInfo.Add(new Tuple<DateTime, string>(timestamp, playerName));
            }

            return true;
        }
        private static string ParseZoneInfo(SquadSheetContext context, string line)
        {
            var timestampStr = line.Substring(0, line.IndexOf(" ZONE_INFO:")).Trim();
            DateTime timestamp;
            string zoneName = string.Empty;
            if (!DateTime.TryParseExact(timestampStr, "M/d HH:mm:ss.fff", null,
                                        System.Globalization.DateTimeStyles.None, out timestamp))
            {
                Console.WriteLine($"Failed to parse timestamp: {timestampStr}");
            }
            var parts = line.Split('&');
            if (parts.Length > 1)
            {
                zoneName = parts[1].Trim();
                if (string.IsNullOrEmpty(zoneName))
                {
                    //Console.WriteLine($"Zone name is empty in line: {line}");
                    return string.Empty;
                }

            }

            if (!String.IsNullOrEmpty(zoneName) && timestamp != default)
            {

                context.ZoneInfo.Add(new Tuple<DateTime, string>(timestamp, zoneName));
            }

            return zoneName;
        }

    }
}