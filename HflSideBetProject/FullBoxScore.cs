using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CodedUITestProject1
{
    [Serializable]
    public class FullBoxScore
    {
        HtmlDocument document;

        public double Turnovers = 0;
        public double MostRushes = 0;
        public string MostRushesPlayer;
        public string MostCatchesPlayer;
        public double MostCatches = 0;

        public int Year { get; set; }
        public int TeamId { get; set; }
        public int Week { get; set; }

        public FullBoxScore() { }

        public FullBoxScore(string html, int teamId, int week, int year)
        {
            Trace.WriteLine(string.Format("Loading full box score for {0} in week {1}", teamId, week));
            this.TeamId = teamId;
            this.Week = week;
            this.Year = year;
            document = new HtmlDocument();
            document.LoadHtml(html);
            LoadCatches();
        }

        private void LoadCatches()
        {
            var playerTable = document.DocumentNode.SelectSingleNode("//table[@id='playertable_0']");
            var playerRows = playerTable.SelectNodes(".//tr[contains(@class, 'pncPlayerRow')]");
            foreach (var playerRow in playerRows)
            {
                var playerName = playerRow.SelectNodes(".//td[contains(@class, 'playertablePlayerName')]//a[1]")[0].InnerText;
                var cells = playerRow.SelectNodes(".//td[@class='playertableStat ']");
                var receptions = cells[7].getDoubleFromInnerText();
                var rushes = cells[4].getDoubleFromInnerText();
                var fumbles = cells[12].getDoubleFromInnerText();
                var interceptions = cells[3].getDoubleFromInnerText();

                if (rushes > MostRushes)
                {
                    MostRushes = rushes;
                    MostRushesPlayer = playerName;
                }

                if (receptions > MostCatches)
                {
                    MostCatches = receptions;
                    MostCatchesPlayer = playerName;
                }
                
                Turnovers += fumbles + interceptions;
            }
        }
    }
}
