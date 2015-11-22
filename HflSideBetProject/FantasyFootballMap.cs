namespace CodedUITestProject1.FantasyFootballMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
    
    
    public partial class FantasyFootballMap
    {
        #region Browser Window
        public void NavigateToUrl(string url)
        {
            var numTries = 0;
            while (browserWindow.Uri != new Uri(url) && numTries < 3)
            {
                browserWindow.NavigateToUrl(new Uri(url));
                numTries++;
            }

        }

        public double getHomeTeamScore()
        {
            var homeScorePane = this.UIBridgeBuildersvsFuzzWindow.UIBridgeBuildersvsFuzzDocument.UIContentPane.HomeScorePane;
            return double.Parse(homeScorePane.InnerText);
        }

        public string getHtml()
        {
            HtmlDocument document = new HtmlDocument(browserWindow);
            return document.GetProperty("OuterHtml") as String;
        }

        public double getAwayTeamScore()
        {
            HtmlDocument document = new HtmlDocument(browserWindow);
            var html = document.GetProperty("OuterHtml") as String;
            HtmlAgilityPack.HtmlDocument agileDoc = new HtmlAgilityPack.HtmlDocument();
            agileDoc.LoadHtml(html);
            var nodes = agileDoc.DocumentNode.SelectNodes("//div[@class='danglerBox totalScore']");
            return double.Parse(nodes[0].InnerText);


            //var awayScorePane = this.UIBridgeBuildersvsFuzzWindow.UIBridgeBuildersvsFuzzDocument.UIContentPane.AwayScorePane;
            //return double.Parse(awayScorePane.InnerText);
        }

        public double getHomeDefPts()
        {
            var homeDefPts = this.UIBridgeBuildersvsFuzzWindow.UIBridgeBuildersvsFuzzDocument.UIPlayertable_0Table.HomeDefPtsCell;
            return double.Parse(homeDefPts.InnerText);
        }

        public double getAwayDefPts()
        {
            var awayDefPts = this.UIBridgeBuildersvsFuzzWindow.UIBridgeBuildersvsFuzzDocument.UIPlayertable_2Table.AwayDefPtsCell;
            return double.Parse(awayDefPts.InnerText);
        }

        public BrowserWindow browserWindow
        {
            get
            {
                return (BrowserWindow)UIHawkeyeFootballLeaguWindowProp;
            }
        }

        public UIHawkeyeFootballLeaguWindow UIHawkeyeFootballLeaguWindowProp
        {
            get
            {
                if ((this.mUIHawkeyeFootballLeaguWindow == null))
                {
                    this.mUIHawkeyeFootballLeaguWindow = new UIHawkeyeFootballLeaguWindow();
                }
                return this.mUIHawkeyeFootballLeaguWindow;
            }
        }
        #endregion
    }
}
