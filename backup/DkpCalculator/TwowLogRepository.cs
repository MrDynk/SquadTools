
using System.Runtime.CompilerServices;

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
            context.Loot = new List<Tuple<DateTime, string>>();

            var allLines = File.ReadAllLines(_logDirectory);

            foreach (var line in allLines)
            {
                //9/3 20:47:06.469  ZONE_INFO: 03.09.25 20:47:06&naxxramas&0
                if (line.Contains("ZONE_INFO"))
                {
                    bool flowControl = ParseZoneInfo(context, line);
                    if (!flowControl)
                    {
                        continue;
                    }
                }

                //9/3 20:45:46.437  COMBATANT_INFO: 03.09.25 20:45:46&Holecloser&PRIEST&Human&3&nil&SQUAD&Battle Brother&4&nil&21507:440:0:0&22515:0:0:0&nil&nil&nil&nil&nil&22519:2566:0:0&nil&17110:440:0:0&19863:440:0:0&18469:0:0:0&19950:0:0:0&nil&nil&nil&21801:0:0:0&nil&nil
                if (line.Contains("COMBATANT_INFO"))
                {
                    bool flowControl = ParseCombatantInfo(context, line);
                    if (!flowControl)
                    {
                        continue;
                    }

                }
                //9/3 21:08:15.762  Sharkblood dies.
                if (line.Contains(".dies"))
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
                            // 2. Extract text between end of DateTime and the word 'dies'
                            int diesIdx = line.IndexOf("dies", spaceIdx);
                            if (diesIdx > 0)
                            {
                                // The text between the end of the DateTime and the word 'dies'
                                string afterDate = line.Substring(spaceIdx).Trim();
                                string between = afterDate.Substring(0, diesIdx - spaceIdx).Replace(".dies", "").Replace("dies", "").Trim();
                                if (!string.IsNullOrEmpty(between))
                                {
                                    context.Deaths.Add(new Tuple<DateTime, string>(timestamp, between));
                                }
                            }
                        }
                    }
                }
            }
        }


        public void GetPlayerActivity(SquadSheetContext context)
        {
            // Implementation here
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
                playerName = parts[1].Trim();
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
            private static bool ParseZoneInfo(SquadSheetContext context, string line)
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
                    Console.WriteLine($"Zone name is empty in line: {line}");
                    return false;
                }

            }

            if (!String.IsNullOrEmpty(zoneName) && timestamp != default)
            {
                context.ZoneInfo.Add(new Tuple<DateTime, string>(timestamp, zoneName));
            }

            return true;
        }

    }
}