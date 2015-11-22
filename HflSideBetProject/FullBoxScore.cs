using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CodedUITestProject1
{
    public class FullBoxScore
    {
        HtmlDocument document;

        public double Turnovers = 0;
        public double MostRushes = 0;
        public string MostRushesPlayer;
        public string MostCatchesPlayer;
        public double MostCatches = 0;

        private int teamId;
        public int week;

        public FullBoxScore() { }

        public FullBoxScore(string html, int teamId, int week)
        {
            Trace.WriteLine(string.Format("Loading full box score for {0} in week {1}", teamId, week));
            this.teamId = teamId;
            this.week = week;
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

            //Trace.WriteLine(string.Format("Turnovers: {0}", Turnovers));
            //Trace.WriteLine(string.Format("Most rushes: {0} ({1})", MostRushes, MostRushesPlayer));
            //Trace.WriteLine(string.Format("Most catches: {0} ({1})", MostCatches, MostCatchesPlayer));
        }
    }
}
