using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using CodedUITestProject1.FantasyFootballMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using System.Diagnostics;


namespace CodedUITestProject1
{
    [CodedUITest]
    public class HflSideBets
    {
        public const int year = 2017;
        public const string url_scoreboard = @"http://games.espn.go.com/ffl/scoreboard?leagueId=481687&scoringPeriodId=16";
        public const string url_boxscore = "http://games.espn.go.com/ffl/boxscorequick?leagueId=481687&teamId={0}&scoringPeriodId={1}&seasonId={2}&view=scoringperiod&version=quick";
        public const string url_fullscore = "http://games.espn.go.com/ffl/boxscorefull?leagueId=481687&teamId={0}&scoringPeriodId={1}&seasonId={2}&view=scoringperiod&version=full";
        
        public Dictionary<int, string> owners = new Dictionary<int, string>
        {
            {3, "Ryan"},
            {5, "Court"},
            {9, "EC"},
            {1, "Chris"},
            {4, "Nate"},
            {14, "Eric"},
            // {19, "Dane"},
            // {21, "Joel"},
            {22, "Sean"},
            {23, "Scott"}
        };

        private List<int> teamIds {
            get {
                return new List<int>(owners.Keys.ToArray<int>());
            }
        }

        public Dictionary<int, BoxScore> mostTeamPtsInLoss = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> mostDefPts        = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> mostPlyPoints     = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> longestPunt       = new Dictionary<int, BoxScore>();
        public Dictionary<int, BoxScore> longestPlay = new Dictionary<int, BoxScore>();
        public Dictionary<int, double> mostSacks           = new Dictionary<int, double>();
        public Dictionary<int, BoxScore> longestFGs        = new Dictionary<int, BoxScore>();
        public Dictionary<int, FullBoxScore> mostCatches   = new Dictionary<int, FullBoxScore>();
        public Dictionary<int, FullBoxScore> mostRushes    = new Dictionary<int, FullBoxScore>();
        public Dictionary<int, FullBoxScore> mostTOsInWin  = new Dictionary<int, FullBoxScore>();

        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void NavigateAllWeeks()
        {
            // var weeks = 6;
            var weeks = 13;
            initDictionaries();
            Logger.log("NavigateAllWeeks");
            var map = new FantasyFootballMap();
            var totalSteps = weeks * teamIds.Count;
            var stepsCompleted = 0.0;
            for (int week = 1; week <= weeks; week++)
            {
                Logger.log(string.Format("Processing week {0}", week));
                foreach (var team in teamIds)
                {
                    Logger.log(string.Format("Processing team {0} in week {1}", owners[team], week));
                    var boxScore = BoxScoreData.getBoxScore(year, week, team);
                    if (boxScore == null)
                    {
                        map.NavigateToUrl(getBoxScoreUrl(team, week));
                        boxScore = new BoxScore(map.getHtml(), team, week, year);
                        BoxScoreData.saveBoxScore(boxScore);
                    }

                    var fullBoxScore = BoxScoreData.getFullBoxScore(year, week, team);
                    if (fullBoxScore == null)
                    {
                        map.NavigateToUrl(getFullBoxScoreUrl(team, week));
                        fullBoxScore = new FullBoxScore(map.getHtml(), team, week, year);
                        BoxScoreData.saveFullBoxScore(fullBoxScore);
                    }
                    
                    if (mostDefPts[team] == null || mostDefPts[team].HomeDefensePts < boxScore.HomeDefensePts)
                    {
                        mostDefPts[team] = boxScore;
                        Logger.log("Most def points: " + boxScore.HomeDefensePts);
                    }

                    if ((longestPunt[team] == null || longestPunt[team].LongPunt < boxScore.LongPunt))
                    {
                        longestPunt[team] = boxScore;
                        Logger.log("Longest punt: " + boxScore.LongPunt);
                    }

                    mostSacks[team] += boxScore.TotalSacks;
                    Logger.log("Total sacks: " + mostSacks[team]);

                    if (longestFGs[team] == null || longestFGs[team].LongestFieldGoal < boxScore.LongestFieldGoal)
                    {
                        longestFGs[team] = boxScore;
                        Logger.log(string.Format("Longest FG: {0} ({1})", boxScore.LongestFieldGoalKicker, boxScore.LongestFieldGoal));
                    }

                    if (longestPlay[team] == null || longestPlay[team].LongestPlay < boxScore.LongestPlay)
                    {
                        longestPlay[team] = boxScore;
                        Logger.log(string.Format("Longest Play: {0} ({1})", boxScore.LongestPlayPlayer, boxScore.LongestPlay));
                    }

                    if (mostCatches[team] == null || mostCatches[team].MostCatches < fullBoxScore.MostCatches)
                    {
                        mostCatches[team] = fullBoxScore;
                        Logger.log(string.Format("Most catches: {0} ({1})", mostCatches[team].MostCatchesPlayer, mostCatches[team].MostCatches));
                    }

                    if (mostRushes[team] == null || mostRushes[team].MostRushes < fullBoxScore.MostRushes)
                    {
                        mostRushes[team] = fullBoxScore;
                        Logger.log(string.Format("Most rushes: {0} ({1})", mostRushes[team].MostRushesPlayer, mostRushes[team].MostRushes));
                    }

                    var teamWon = boxScore.HomeScore > boxScore.AwayScore;
                    if (teamWon && (mostTOsInWin[team] == null || mostTOsInWin[team].Turnovers < fullBoxScore.Turnovers))
                    {
                        mostTOsInWin[team] = fullBoxScore;
                        Logger.log("Most TOs in a win: " + mostTOsInWin[team].Turnovers);
                    }

                    if (!teamWon && (mostPlyPoints[team] == null || mostPlyPoints[team].MaxPlayerPoints < boxScore.MaxPlayerPoints))
                    {
                        mostPlyPoints[team] = boxScore;
                        Logger.log(string.Format("Most player points in loss: {0} ({1}, wk{2})", mostPlyPoints[team].MaxPlayerPoints, mostPlyPoints[team].MaxPlayerPointsPlayer, mostPlyPoints[team].Week));
                    }

                    if (!teamWon && (mostTeamPtsInLoss[team] == null || mostTeamPtsInLoss[team].HomeScore < boxScore.HomeScore))
                    {
                        mostTeamPtsInLoss[team] = boxScore;
                        Logger.log(string.Format("Most team points in loss: {0} (wk{1})", mostTeamPtsInLoss[team].HomeScore, mostTeamPtsInLoss[team].Week));
                    }

                    double percentDone = (stepsCompleted++ / totalSteps) * 100.0;
                    Logger.log(string.Format("{0}% complete", Math.Round(percentDone, 2)));
                }
            }

            PrintSummary();
        }

        private void PrintSummary()
        {
            List<int> teams = teamIds;
            Logger.log("Longest FG:");
            teams.Sort(delegate(int t1, int t2) { return longestFGs[t1].LongestFieldGoal > longestFGs[t2].LongestFieldGoal ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], longestFGs[t].LongestFieldGoal, longestFGs[t].LongestFieldGoalKicker, longestFGs[t].Week)));
            Logger.log("");

            Logger.log("Most DEF sacks:");
            teams.Sort(delegate(int t1, int t2) { return mostSacks[t1] > mostSacks[t2] ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1}", owners[t], mostSacks[t])));
            Logger.log("");

            Logger.log("Most catches in a single game:");
            teams.Sort(delegate(int t1, int t2) { return mostCatches[t1].MostCatches > mostCatches[t2].MostCatches ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostCatches[t].MostCatches, mostCatches[t].MostCatchesPlayer, mostCatches[t].Week)));
            Logger.log("");

            Logger.log("Most team points scored in a loss:");
            teams.Sort(delegate(int t1, int t2) { return mostTeamPtsInLoss[t1].HomeScore > mostTeamPtsInLoss[t2].HomeScore ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} wk{2}", owners[t], mostTeamPtsInLoss[t].HomeScore, mostTeamPtsInLoss[t].Week)));
            Logger.log("");

            Logger.log("Longest TD run or catch (make sure it's a TD!)");
            teams.Sort(delegate(int t1, int t2) { return longestPlay[t1].LongestPlay > longestPlay[t2].LongestPlay ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], longestPlay[t].LongestPlay, longestPlay[t].LongestPlayPlayer, longestPlay[t].Week)));
            Logger.log("");

            Logger.log("Most player points in a loss:");
            teams.Sort(delegate(int t1, int t2) { return mostPlyPoints[t1].MaxPlayerPoints > mostPlyPoints[t2].MaxPlayerPoints ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostPlyPoints[t].MaxPlayerPoints, mostPlyPoints[t].MaxPlayerPointsPlayer, mostPlyPoints[t].Week)));
            Logger.log("");

            Logger.log("Most DEF points in a single game:");
            teams.Sort(delegate(int t1, int t2) { return mostDefPts[t1].HomeDefensePts > mostDefPts[t2].HomeDefensePts ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostDefPts[t].HomeDefensePts, mostDefPts[t].HomeDefense, mostDefPts[t].Week)));
            Logger.log("");

            Logger.log("Longest punt:");
            teams.Sort(delegate(int t1, int t2) { return longestPunt[t1].LongPunt > longestPunt[t2].LongPunt ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], longestPunt[t].LongPunt, longestPunt[t].HomeDefense, longestPunt[t].Week)));
            Logger.log("");

            Logger.log("Most rushes in a single game:");
            teams.Sort(delegate(int t1, int t2) { return mostRushes[t1].MostRushes > mostRushes[t2].MostRushes ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} ({2}, wk{3})", owners[t], mostRushes[t].MostRushes, mostRushes[t].MostRushesPlayer, mostRushes[t].Week)));
            Logger.log("");

            Logger.log("Most TOs in a win:");
            teams.Sort(delegate(int t1, int t2) { return mostTOsInWin[t1].Turnovers > mostTOsInWin[t2].Turnovers ? -1 : 1; });
            teams.ForEach(t => Logger.log(string.Format("{0}: {1} wk{2}", owners[t], mostTOsInWin[t].Turnovers, mostTOsInWin[t].Week)));
            Logger.log("");
            Trace.Flush();
        }

        [TestMethod, Ignore]
        public void ECWeeks1And2BoxScore()
        {
            var map = new FantasyFootballMap();
            var erikId = 9;
            // week 1
            map.NavigateToUrl(getBoxScoreUrl(erikId, 1));
            var boxScore = new BoxScore(map.getHtml(), erikId, 1, year);
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

            // test storing the data
            BoxScoreData.saveBoxScore(boxScore);
            var boxScore2 = BoxScoreData.getBoxScore(year, 1, erikId);
            Assert.AreEqual(5, boxScore2.TotalSacks);
            Assert.AreEqual(56, boxScore2.LongPunt);
            Assert.AreEqual("Dan Bailey", boxScore2.LongestFieldGoalKicker);
            Assert.AreEqual(32, boxScore2.LongestFieldGoal);
            Assert.AreEqual(14.3, boxScore2.HomeQBPts);
            Assert.AreEqual(12.4, boxScore2.HomeRB1Pts);
            Assert.AreEqual(13.1, boxScore2.HomeRB2Pts);
            Assert.AreEqual(4.8, boxScore2.HomeWR1Pts);
            Assert.AreEqual(13.2, boxScore2.HomeWR2Pts);
            Assert.AreEqual(12.3, boxScore2.HomeTEPts);
            Assert.AreEqual(10.7, boxScore2.HomeFLEXPts);
            Assert.AreEqual(27, boxScore2.HomeDefensePts);
            Assert.AreEqual(9, boxScore2.HomeKPts);
            Assert.AreEqual(27, boxScore2.MaxPlayerPoints);
            Assert.AreEqual("Panthers D/ST", boxScore2.MaxPlayerPointsPlayer);
            Assert.AreEqual("Panthers D/ST", boxScore2.HomeDefense);
            Assert.AreEqual(25, boxScore2.AwayQBPts);
            Assert.AreEqual(18.3, boxScore2.AwayRB1Pts);
            Assert.AreEqual(4.1, boxScore2.AwayRB2Pts);
            Assert.AreEqual(4.4, boxScore2.AwayWR1Pts);
            Assert.AreEqual(12.6, boxScore2.AwayWR2Pts);
            Assert.AreEqual(7.3, boxScore2.AwayTEPts);
            Assert.AreEqual(4.7, boxScore2.AwayFLEXPts);
            Assert.AreEqual(4, boxScore2.AwayDefensePts);
            Assert.AreEqual(11, boxScore2.AwayKPts);
            Assert.AreEqual(116.8, boxScore2.HomeScore);
            Assert.AreEqual(91.4, boxScore2.AwayScore);
            
            // week 2
            map.NavigateToUrl(getBoxScoreUrl(erikId, 2));
            boxScore = new BoxScore(map.getHtml(), erikId, 2, year);
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
            map.NavigateToUrl(getBoxScoreUrl(erikId, 9));
            boxScore = new BoxScore(map.getHtml(), erikId, 9, year);
            Assert.AreEqual(0, boxScore.AwayKPts);
        }

        [TestMethod, Ignore]
        public void RyanWeek1FullBox()
        {
            var ryanId = 3;
            var week = 1;
            var map = new FantasyFootballMap();
            map.NavigateToUrl(getFullBoxScoreUrl(ryanId, week));
            
            var boxScore = new FullBoxScore(map.getHtml(), ryanId, week, year);

            Assert.AreEqual(2, boxScore.Turnovers);
            Assert.AreEqual(15, boxScore.MostCatches);
            Assert.AreEqual("Keenan Allen", boxScore.MostCatchesPlayer);
            Assert.AreEqual(12, boxScore.MostRushes);
            Assert.AreEqual("C.J. Anderson", boxScore.MostRushesPlayer);

            BoxScoreData.saveFullBoxScore(boxScore);
            var boxScore2 = BoxScoreData.getFullBoxScore(year, week, ryanId);
            Assert.IsNotNull(boxScore2);

            Assert.AreEqual(2, boxScore2.Turnovers);
            Assert.AreEqual(15, boxScore2.MostCatches);
            Assert.AreEqual("Keenan Allen", boxScore2.MostCatchesPlayer);
            Assert.AreEqual(12, boxScore2.MostRushes);
            Assert.AreEqual("C.J. Anderson", boxScore2.MostRushesPlayer);
        }

        private void initDictionaries()
        {
            foreach(var teamId in teamIds) 
            {
                mostTeamPtsInLoss.Add(teamId, new BoxScore());
                mostDefPts.Add(teamId, new BoxScore());
                longestPunt.Add(teamId, new BoxScore());
                mostSacks.Add(teamId, 0);
                longestFGs.Add(teamId, new BoxScore());
                longestPlay.Add(teamId, new BoxScore());
                mostCatches.Add(teamId, new FullBoxScore());
                mostRushes.Add(teamId, new FullBoxScore());
                mostTOsInWin.Add(teamId, new FullBoxScore());
                mostPlyPoints.Add(teamId, new BoxScore());
            }                   
        }

        private string getBoxScoreUrl(int teamId, int week)
        {
            return string.Format(url_boxscore, teamId, week, year);
        }

        private string getFullBoxScoreUrl(int teamId, int week)
        {
            return string.Format(url_fullscore, teamId, week, year);            
        }
    }
}
