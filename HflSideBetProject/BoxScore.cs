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
    public class BoxScore
    {
        HtmlDocument document;

        public double HomeScore { get; set; }
        public double AwayScore { get; set; }
        public double HomeDefensePts { get; set; }
        public string HomeDefense { get; set; }
        public double AwayDefensePts { get; set; }
        public double HomeQBPts { get; set; }
        public double HomeRB1Pts { get; set; }
        public double HomeRB2Pts { get; set; }
        public double HomeWR1Pts { get; set; }
        public double HomeWR2Pts { get; set; }
        public double HomeTEPts { get; set; }
        public double HomeFLEXPts { get; set; }
        public double HomeKPts { get; set; }
        public double AwayQBPts { get; set; }
        public double AwayRB1Pts { get; set; }
        public double AwayRB2Pts { get; set; }
        public double AwayWR1Pts { get; set; }
        public double AwayWR2Pts { get; set; }
        public double AwayTEPts { get; set; }
        public double AwayFLEXPts { get; set; }
        public double AwayKPts { get; set; }
        public double TotalSacks { get; set; }
        public double LongPunt { get; set; }
        public double LongestFieldGoal { get; set; }
        public string LongestFieldGoalKicker { get; set; }
        public double LongestPlay { get; set; }
        public string LongestPlayPlayer { get; set; }
        public double MaxPlayerPoints { get; set; }
        public string MaxPlayerPointsPlayer { get; set; }
        public int TeamId { get; set; }
        public int Week { get; set; }
        public int Year { get; set; }

        public BoxScore() { }

        public BoxScore(string html, int teamId, int week, int year)
        {
            this.TeamId = teamId;
            this.Week = week;
            this.Year = year;
            document = new HtmlDocument();
            document.LoadHtml(html);
            ParseDocument();
        }

        void ParseDocument()
        {
            LoadPlayerPoints();
            LoadTotalPoints();
            LoadDefStats();
            LoadKickerStats();
            LoadLongestRunCatchStats();
        }

        private void LoadLongestRunCatchStats()
        {
            var rows = document.DocumentNode.SelectNodes("//tr[contains(@class, 'pncPlayerRow')]");

            for (var i = 1; i < 8; i++)
            {
                var player = rows[i];
                if (player.SelectNodes(".//a") == null) return;
                var playerName = player.SelectNodes(".//a")[0].InnerText;

                var playerNode = player.SelectNodes(".//a[contains(@class, 'gamestatus')]//@href");
                if (playerNode == null) return;
                var url = playerNode[0].GetAttributeValue("href", "");
                var urlParams = url.Split('?');
                var gameId = urlParams.Last();
                url = url.Replace("game?", "boxscore?");

                var html = BoxScoreData.getGameStatHtml(gameId);
                if (html == null)
                {
                    var web = new HtmlWeb();
                    html = web.Load(url).DocumentNode.OuterHtml;
                    BoxScoreData.saveGameStatHtml(gameId, html);
                }
                
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var longPlayNodes = doc.DocumentNode.SelectNodes("//tr[td//text()[contains(., \"" + playerName.Replace("'", "\'") + "\")]]//td[contains(@class, 'long')]");
                if(longPlayNodes == null) continue;
                foreach(var longPlayNode in longPlayNodes) 
                {
                    var longPlay = longPlayNode.getDoubleFromInnerText();
                    if(longPlay > LongestPlay) 
                    {
                        LongestPlay = longPlay;
                        LongestPlayPlayer = playerName;
                    }
                }
            }
        }

        private void LoadKickerStats()
        {
            var rows = document.DocumentNode.SelectNodes("//tr[contains(@class, 'pncPlayerRow')]");
            var kicker = rows[8];
            if (kicker.SelectNodes(".//a") == null) return;
            var kickerName = kicker.SelectNodes(".//a")[0].InnerText;

            var kickerNode = kicker.SelectNodes(".//a[contains(@class, 'gamestatus')]//@href");
            if (kickerNode == null) return;
            var url = kickerNode[0].GetAttributeValue("href", "");
            url = url.Replace("game?", "boxscore?");
            var web = new HtmlWeb();
            var html = web.Load(url).DocumentNode.OuterHtml;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var longFg = doc.DocumentNode.SelectNodes("//tr[td//text()[contains(., '"+kickerName+"')]]//td[contains(@class, 'long')]");
            LongestFieldGoalKicker = kickerName;
            LongestFieldGoal = longFg == null ? 0 : longFg[0].getDoubleFromInnerText();
        }

        private void LoadDefStats()
        {
            var rows = document.DocumentNode.SelectNodes("//tr[contains(@class, 'pncPlayerRow')]");
            var def = rows[7].SelectNodes(".//a[contains(@class, 'gamestatus')]//@href");
            if (def == null) return;

            var url = def[0].GetAttributeValue("href", "");
            url = url.Replace("game?", "boxscore?");
            var web = new HtmlWeb();
            var html = web.Load(url).DocumentNode.OuterHtml;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var defLinks = document.DocumentNode.SelectNodes("//td[@class='playertablePlayerName']");
            var defName = defLinks[7].InnerText.Replace(" D/ST&nbsp;D/ST", "");
            var isAway = doc.DocumentNode.SelectSingleNode("//div[@class='team away']").InnerText.Contains(defName);
            
            var sacks = doc.DocumentNode.SelectNodes(".//tr[contains(@class, 'highlight')]//td[contains(@class, 'sacks')]");
            var punts = doc.DocumentNode.SelectNodes(".//tr[contains(@class, 'highlight')]//td[contains(@class, 'long')]");
            if (isAway)
            {
                TotalSacks = sacks[2].getDoubleFromInnerText();
                LongPunt = punts[punts.Count - 2].getDoubleFromInnerText();
            }
            else
            {
                TotalSacks = sacks[3].getDoubleFromInnerText();
                LongPunt = punts[punts.Count - 1].getDoubleFromInnerText();
            }
        }

        void LoadPlayerPoints()
        {
            var nodes = document.DocumentNode.SelectNodes("//tr[contains(@class, 'pncPlayerRow')]");
            HomeQBPts = loadPlayerRowPoints("QB", true);
            HomeRB1Pts = loadPlayerRowPoints("RB1", true);
            HomeRB2Pts = loadPlayerRowPoints("RB2", true);
            HomeWR1Pts = loadPlayerRowPoints("WR1", true);
            HomeWR2Pts = loadPlayerRowPoints("WR2", true);
            HomeTEPts = loadPlayerRowPoints("TE", true);
            HomeFLEXPts = loadPlayerRowPoints("FLEX", true);
            HomeDefensePts = loadPlayerRowPoints("D/ST", true);
            HomeKPts = loadPlayerRowPoints("K", true);

            AwayQBPts = loadPlayerRowPoints("QB", false);
            AwayRB1Pts = loadPlayerRowPoints("RB1", false);
            AwayRB2Pts = loadPlayerRowPoints("RB2", false);
            AwayWR1Pts = loadPlayerRowPoints("WR1", false);
            AwayWR2Pts = loadPlayerRowPoints("WR2", false);
            AwayTEPts = loadPlayerRowPoints("TE", false);
            AwayFLEXPts = loadPlayerRowPoints("FLEX", false);
            AwayDefensePts = loadPlayerRowPoints("D/ST", false);
            AwayKPts = loadPlayerRowPoints("K", false);
        }

        double loadPlayerRowPoints(string position, bool isHome)
        {
            var index = isHome ? 1 : 2;
            if (position == "RB1")
            {
                position = "RB";
                index = isHome ? 1 : 3;
            } 
            else if (position == "RB2")
            {
                position = "RB";
                index = isHome ? 2 : 4;
            }
            else if (position == "WR1")
            {
                position = "WR";
                index = isHome ? 1 : 3;
            }
            else if (position == "WR2")
            {
                position = "WR";
                index = isHome ? 2 : 4;
            }

            var points = document.DocumentNode.SelectSingleNode("(//tr[contains(@class, 'pncPlayerRow') and td[1]//text()[contains(., '"+position+"')]])["+index+"]//td[last()]").getDoubleFromInnerText();

            if (isHome)
            {
                var playerNode = document.DocumentNode.SelectSingleNode("(//tr[contains(@class, 'pncPlayerRow') and td[1]//text()[contains(., '" + position + "')]])[" + index + "]//a");
                if (playerNode == null) return 0;
                
                var playerName = playerNode.InnerText;
                if (points > MaxPlayerPoints)
                {
                    MaxPlayerPoints = points;
                    MaxPlayerPointsPlayer = playerName;
                }

                if (position == "D/ST")
                {
                    HomeDefense = playerName;
                }                
            }

            return points;
        }

        void LoadTotalPoints()
        {
            var nodes = document.DocumentNode.SelectNodes("//div[@class='danglerBox totalScore']");
            HomeScore = nodes[0].getDoubleFromInnerText();
            AwayScore = nodes[1].getDoubleFromInnerText();
        }
    }

    public static class MyExtensions
    {
        public static double getDoubleFromInnerText(this HtmlNode node) 
        {
            double outVal = 0;
            if(node != null) double.TryParse(node.InnerText, out outVal);
            return outVal;
        }
    }
}
