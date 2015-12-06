using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CodedUITestProject1
{
    public static class BoxScoreData
    {
        private static string _dataPath = @"C:\hflscores\";
        private static string _typeFbs = "fbs";
        private static string _typeBs = "bs";
        
        public static BoxScore getBoxScore(int year, int week, int teamId)
        {
            var fileName = buildFilePath(year, week, teamId, _typeBs);
            if (!File.Exists(fileName)) { return null; }

            var serializer = new XmlSerializer(typeof(BoxScore));
            var fileStream = new FileStream(fileName, FileMode.Open);
            return (BoxScore)serializer.Deserialize(fileStream);
        }

        public static FullBoxScore getFullBoxScore(int year, int week, int teamId)
        {
            var fileName = buildFilePath(year, week, teamId, _typeFbs);
            if (!File.Exists(fileName)) { return null; }

            var serializer = new XmlSerializer(typeof(FullBoxScore));
            var fileStream = new FileStream(fileName, FileMode.Open);
            return (FullBoxScore)serializer.Deserialize(fileStream);
        }

        public static void saveBoxScore(BoxScore boxScore)
        {
            var fileName = buildFilePath(boxScore.Year, boxScore.Week, boxScore.TeamId, _typeBs);
            var serializer = new XmlSerializer(typeof(BoxScore));
            var writer = new StreamWriter(fileName);
            serializer.Serialize(writer, boxScore);
            writer.Close();
        }

        public static void saveFullBoxScore(FullBoxScore fullBoxScore)
        {
            var fileName = buildFilePath(fullBoxScore.Year, fullBoxScore.Week, fullBoxScore.TeamId, _typeFbs);
            var serializer = new XmlSerializer(typeof(FullBoxScore));
            var writer = new StreamWriter(fileName);
            serializer.Serialize(writer, fullBoxScore);
            writer.Close();
        }

        private static string buildFilePath(int year, int week, int teamId, string type)
        {
            var fileName = String.Format("{0}_{1}_{2}_{3}.xml", year, week, teamId, type);
            return String.Format("{0}{1}", _dataPath, fileName);
        }
    }
}
