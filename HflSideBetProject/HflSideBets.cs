using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using CodedUITestProject1.FantasyFootballMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using System.Diagnostics;


namespace CodedUITestProject1
{
    /// <summary>
    /// Summary description for HflSideBets
    /// </summary>
    [CodedUITest]
    public class HflSideBets
    {
        public const string url_scoreboard = @"http://games.espn.go.com/ffl/scoreboard?leagueId=481687&seasonId=2015";
        public const string url_boxscore = "http://games.espn.go.com/ffl/boxscorequick?leagueId=481687&teamId={0}&scoringPeriodId={1}&seasonId=2015&view=scoringperiod&version=quick";
        public const string url_fullscore = "http://games.espn.go.com/ffl/boxscorefull?leagueId=481687&teamId={0}&scoringPeriodId={1}&seasonId=2015&view=scoringperiod&version=full";
        
        public const int chrisId = 1;
        public const int ryanId = 3;
        public const int ericId = 14;
        public const int erikId = 9;
        public const int courtId = 5;
        public const int nateId = 4;
        public const int daneId = 19;
        public const int shawnId = 20;

        public List<int> teams = new List<int> 
        { 
            ryanId, 
            courtId, 
            erikId, 
            chrisId, 
            nateId, 
            ericId, 
            daneId, 
            shawnId 
        };

        public Dictionary<int, string> owners = new Dictionary<int, string>
        {
            {ryanId, "Ryan"},
            {courtId, "Court"},
            {erikId, "EC"},
            {chrisId, "Chris"},
            {nateId, "Nate"},
            {ericId, "Eric"},
            {daneId, "Dane"},
            {shawnId, "Shawn"}
        };

        public Dictionary<int, BoxScore> mostTeamPtsInLoss = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> mostDefPts       = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> mostPlyPoints = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> longestPunt      = new Dictionary<int, BoxScore>();
        public Dictionary<int, double> mostSacks        = new Dictionary<int, double>();
        public Dictionary<int, BoxScore> longestFGs = new Dictionary<int, BoxScore>();
        public Dictionary<int, FullBoxScore> mostCatches      = new Dictionary<int, FullBoxScore>();
        public Dictionary<int, FullBoxScore> mostRushes = new Dictionary<int, FullBoxScore>();
        public Dictionary<int, FullBoxScore> mostTOsInWin = new Dictionary<int, FullBoxScore>();

        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void NavigateAllWeeks()
        {
            //var weeks = 14;
            var weeks = 10;
            initDictionaries();
            Trace.WriteLine("NavigateAllWeeks");
            var map = new FantasyFootballMap();
            for (int week = 1; week <= weeks; week++)
            {
                Trace.WriteLine(string.Format("Processing week {0}", week));
                foreach (var team in teams)
                {
                    Trace.WriteLine(string.Format("Processing team {0} in week {1}", team, week));
                    map.NavigateToUrl(getBoxScore(team, week));
                    var boxScore = new BoxScore(map.getHtml(), team, week);
                    map.NavigateToUrl(getFullBoxScore(team, week));
                    var fullBoxScore = new FullBoxScore(map.getHtml(), team, week);

                    if (mostDefPts[team] == null || mostDefPts[team].HomeDefensePts < boxScore.HomeDefensePts)
                    {
                        mostDefPts[team] = boxScore;
                        Trace.WriteLine("Most def points: " + boxScore.HomeDefensePts);
                    }

                    if ((longestPunt[team] == null || longestPunt[team].LongPunt < boxScore.LongPunt))
                    {
                        longestPunt[team] = boxScore;
                        Trace.WriteLine("Longest punt: " + boxScore.LongPunt);
                    }

                    mostSacks[team] += boxScore.TotalSacks;
                    Trace.WriteLine("Total sacks: " + mostSacks[team]);

                    if (longestFGs[team] == null || longestFGs[team].LongestFieldGoal < boxScore.LongestFieldGoal)
                    {
                        longestFGs[team] = boxScore;
                        Trace.WriteLine(string.Format("Longest FG: {0} ({1})", boxScore.LongestFieldGoalKicker, boxScore.LongestFieldGoal));
                    }

                    if (mostCatches[team] == null || mostCatches[team].MostCatches < fullBoxScore.MostCatches)
                    {
                        mostCatches[team] = fullBoxScore;
                        Trace.WriteLine(string.Format("Most catches: {0} ({1})", mostCatches[team].MostCatchesPlayer, mostCatches[team].MostCatches));
                    }

                    if (mostRushes[team] == null || mostRushes[team].MostRushes < fullBoxScore.MostRushes)
                    {
                        mostRushes[team] = fullBoxScore;
                        Trace.WriteLine(string.Format("Most rushes: {0} ({1})", mostRushes[team].MostRushesPlayer, mostRushes[team].MostRushes));
                    }

                    var teamWon = boxScore.HomeScore > boxScore.AwayScore;
                    if (teamWon && (mostTOsInWin[team] == null || mostTOsInWin[team].Turnovers < fullBoxScore.Turnovers))
                    {
                        mostTOsInWin[team] = fullBoxScore;
                        Trace.WriteLine("Most TOs in a win: " + mostTOsInWin[team].Turnovers);
                    }

                    if (!teamWon && (mostPlyPoints[team] == null || mostPlyPoints[team].MaxPlayerPoints < boxScore.MaxPlayerPoints))
                    {
                        mostPlyPoints[team] = boxScore;
                        Trace.WriteLine(string.Format("Most player points in loss: {0} ({1}, wk{2})", mostPlyPoints[team].MaxPlayerPoints, mostPlyPoints[team].MaxPlayerPointsPlayer, mostPlyPoints[team].week));
                    }

                    if (!teamWon && (mostTeamPtsInLoss[team] == null || mostTeamPtsInLoss[team].HomeScore < boxScore.HomeScore))
                    {
                        mostTeamPtsInLoss[team] = boxScore;
                        Trace.WriteLine(string.Format("Most team points in loss: {0} (wk{1})", mostTeamPtsInLoss[team].HomeScore, mostTeamPtsInLoss[team].week));
                    }
                }
            }

            Console.WriteLine("Longest FG:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], longestFGs[t].LongestFieldGoal, longestFGs[t].LongestFieldGoalKicker, longestFGs[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Most DEF sacks:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1}", owners[t], mostSacks[t])));
            Console.WriteLine("");

            Console.WriteLine("Most catches in a single game:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostCatches[t].MostCatches, mostCatches[t].MostCatchesPlayer, mostCatches[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Most team points scored in a loss:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} wk{2}", owners[t], mostTeamPtsInLoss[t].HomeScore, mostTeamPtsInLoss[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Longest TD run or catch (calculated manually)");
            Console.WriteLine("");

            Console.WriteLine("Most player points in a loss:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostPlyPoints[t].MaxPlayerPoints, mostPlyPoints[t].MaxPlayerPointsPlayer, mostPlyPoints[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Most DEF points in a single game:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostDefPts[t].HomeDefensePts, mostDefPts[t].HomeDefense, mostDefPts[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Longest punt:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], longestPunt[t].LongPunt, longestPunt[t].HomeDefense, longestPunt[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Most rushes in a single game:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostRushes[t].MostRushes, mostRushes[t].MostRushesPlayer, mostRushes[t].week)));
            Console.WriteLine("");

            Console.WriteLine("Most TOs in a win:");
            teams.ForEach(t => Console.WriteLine(string.Format("{0}: {1} wk{2}", owners[t], mostTOsInWin[t].Turnovers, mostTOsInWin[t].week)));
            Console.WriteLine("");
        }

        [TestMethod]
        public void ECWeeks1And2BoxScore()
        {
            var map = new FantasyFootballMap();

            // week 1
            map.NavigateToUrl(getBoxScore(erikId, 1));
            var boxScore = new BoxScore(map.getHtml(), erikId, 1);
            Assert.AreEqual(5, boxScore.TotalSacks);
            Assert.AreEqual(56, boxScore.LongPunt);
            Assert.AreEqual("Dan Bailey", boxScore.LongestFieldGoalKicker);
            Assert.AreEqual(32, boxScore.LongestFieldGoal);
            Assert.AreEqual(14.3, boxScore.HomeQBPts);
            Assert.AreEqual(12.4, boxScore.HomeRB1Pts);
            Assert.AreEqual(13.1, boxScore.HomeRB2Pts);
            Assert.AreEqual(4.8,  boxScore.HomeWR1Pts);
            Assert.AreEqual(13.2, boxScore.HomeWR2Pts);
            Assert.AreEqual(12.3, boxScore.HomeTEPts);
            Assert.AreEqual(10.7, boxScore.HomeFLEXPts);
            Assert.AreEqual(27,   boxScore.HomeDefensePts);
            Assert.AreEqual(9,    boxScore.HomeKPts);
            Assert.AreEqual(27,   boxScore.MaxPlayerPoints);
            Assert.AreEqual("Panthers D/ST", boxScore.MaxPlayerPointsPlayer);
            Assert.AreEqual("Panthers D/ST", boxScore.HomeDefense);

            Assert.AreEqual(25,   boxScore.AwayQBPts);
            Assert.AreEqual(18.3, boxScore.AwayRB1Pts);
            Assert.AreEqual(4.1,  boxScore.AwayRB2Pts);
            Assert.AreEqual(4.4,  boxScore.AwayWR1Pts);
            Assert.AreEqual(12.6, boxScore.AwayWR2Pts);
            Assert.AreEqual(7.3,  boxScore.AwayTEPts);
            Assert.AreEqual(4.7,  boxScore.AwayFLEXPts);
            Assert.AreEqual(4,    boxScore.AwayDefensePts);
            Assert.AreEqual(11,   boxScore.AwayKPts);

            Assert.AreEqual(116.8, boxScore.HomeScore);
            Assert.AreEqual(91.4, boxScore.AwayScore);


            // week 2
            map.NavigateToUrl(getBoxScore(erikId, 2));
            boxScore = new BoxScore(map.getHtml(), erikId, 2);
            Assert.AreEqual(1, boxScore.TotalSacks);
            Assert.AreEqual(65, boxScore.LongPunt);
            Assert.AreEqual("Dan Bailey", boxScore.LongestFieldGoalKicker);
            Assert.AreEqual(28, boxScore.LongestFieldGoal);

            Assert.AreEqual(29.4, boxScore.HomeQBPts);
            Assert.AreEqual(6.2, boxScore.HomeRB1Pts);
            Assert.AreEqual(7.2, boxScore.HomeRB2Pts);
            Assert.AreEqual(14, boxScore.HomeWR1Pts);
            Assert.AreEqual(2.7, boxScore.HomeWR2Pts);
            Assert.AreEqual(8.2, boxScore.HomeTEPts);
            Assert.AreEqual(6.5, boxScore.HomeFLEXPts);
            Assert.AreEqual(6, boxScore.HomeDefensePts);
            Assert.AreEqual(8, boxScore.HomeKPts);
            Assert.AreEqual(29.4, boxScore.MaxPlayerPoints);
            Assert.AreEqual("Cam Newton", boxScore.MaxPlayerPointsPlayer);
            Assert.AreEqual("Panthers D/ST", boxScore.HomeDefense);

            Assert.AreEqual(14.7, boxScore.AwayQBPts);
            Assert.AreEqual(8, boxScore.AwayRB1Pts);
            Assert.AreEqual(14.7, boxScore.AwayRB2Pts);
            Assert.AreEqual(12.6, boxScore.AwayWR1Pts);
            Assert.AreEqual(5.8, boxScore.AwayWR2Pts);
            Assert.AreEqual(10.9, boxScore.AwayTEPts);
            Assert.AreEqual(8.3, boxScore.AwayFLEXPts);
            Assert.AreEqual(4, boxScore.AwayDefensePts);
            Assert.AreEqual(5, boxScore.AwayKPts);

            Assert.AreEqual(88.2, boxScore.HomeScore);
            Assert.AreEqual(83.9, boxScore.AwayScore);

            // week 9
            map.NavigateToUrl(getBoxScore(erikId, 9));
            boxScore = new BoxScore(map.getHtml(), erikId, 9);
            Assert.AreEqual(0, boxScore.AwayKPts);
        }

        [TestMethod]
        public void RyanWeek1FullBox()
        {
            var week = 1;
            var map = new FantasyFootballMap();
            map.NavigateToUrl(getFullBoxScore(ryanId, week));
            
            var boxScore = new FullBoxScore(map.getHtml(), ryanId, week);

            Assert.AreEqual(2, boxScore.Turnovers);
            Assert.AreEqual(15, boxScore.MostCatches);
            Assert.AreEqual("Keenan Allen", boxScore.MostCatchesPlayer);
            Assert.AreEqual(12, boxScore.MostRushes);
            Assert.AreEqual("C.J. Anderson", boxScore.MostRushesPlayer);
        }

        private void initDictionaries()
        {
            foreach(var team in teams) 
            {
                mostTeamPtsInLoss.Add(team, new BoxScore());
                mostDefPts.Add(team, new BoxScore());
                longestPunt.Add(team, new BoxScore());
                mostSacks.Add(team, 0);
                longestFGs.Add(team, new BoxScore());
                mostCatches.Add(team, new FullBoxScore());
                mostRushes.Add(team, new FullBoxScore());
                mostTOsInWin.Add(team, new FullBoxScore());
                mostPlyPoints.Add(team, new BoxScore());
            }                   
        }

        private string getBoxScore(int teamId, int week)
        {
            return string.Format(url_boxscore, teamId, week);
        }

        private string getFullBoxScore(int teamId, int week)
        {
            return string.Format(url_fullscore, teamId, week);            
        }
    }
}
