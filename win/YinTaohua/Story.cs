using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace YinTaohua
{
    internal class Story
    {
        static BrushConverter BrushConverter = new BrushConverter();
        public static void ParseFrom(StreamReader sr)
        {
            Main = new StoryLine() { Type = StoryLineType.Collection, Lines = new List<StoryLine>() };
            string line;
            Stack<StoryLine> choiceStack = new Stack<StoryLine>();
            StoryLine CurrentCollection = Main;
            while((line = sr.ReadLine()) != null)
            {
                if (line == "!") CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Click });
                else if (line == "")
                {
                    CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Click });
                    CurrentCollection.Lines.Add(new StoryLine() { LineBreak = true });
                }
                else if (line[0] == '+')
                {
                    int level;
                    for (level = 1; level < 8; ++level)
                        if (line[level] != '+') break;
                    if (level > choiceStack.Count)
                    {
                        var choices = new StoryLine() { Type = StoryLineType.Choices, Lines = new List<StoryLine>() };
                        CurrentCollection.Lines.Add(choices);
                        choiceStack.Push(choices);
                    }
                    else for (; level < choiceStack.Count; choiceStack.Pop()) ;
                    if (level == choiceStack.Count)
                    {
                        var colorIndex = line.IndexOf("#");
                        var hide = line.Contains('[');
                        var text = line.Substring(level + (hide ? 1 : 0), (colorIndex == -1 ? line.Length : colorIndex) - level - (hide ? 2 : 0));
                        CurrentCollection = new StoryLine()
                        {
                            Type = StoryLineType.Choice,
                            Text = text,
                            Color = colorIndex == -1 ? null : (Brush)BrushConverter.ConvertFromString(line.Substring(colorIndex)),
                            Lines = new List<StoryLine>()
                        };
                        if (!hide) CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Text, Text = text, LineBreak = true });
                        choiceStack.Peek().Lines.Add(CurrentCollection);
                    }
                }
                else if (line.StartsWith("->"))
                {
                    if (line.StartsWith("->END")) CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.End, Text = line.Substring(6) });
                    else CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Jump, Text = line.Substring(2) });
                }
                else if (line == "==g==")
                {
                    choiceStack.Clear();
                    CurrentCollection = g = new StoryLine() { Type = StoryLineType.Collection, Lines = new List<StoryLine>() };
                }
                else if (line == "hallu") CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Effect });
                else
                {
                    var lineBreak = !line.EndsWith("_");
                    CurrentCollection.Lines.Add(new StoryLine() { Type = StoryLineType.Text, Text = lineBreak ? line : line.Substring(0, line.Length - 1), LineBreak = lineBreak });
                }
            }
        }
        
        public static StoryLine Main;
        public static StoryLine g;

        public static BlockCollection Doc;
        public static InlineCollection MainParagraph;
        public static InlineCollection ChoiceMenu;
        public static Paragraph End1;
        public static Paragraph End2;

        public static StoryLine CurrentCollection;
        public static StoryLine Current;
        public static List<KeyValuePair<Inline, StoryLine>> Choices = new List<KeyValuePair<Inline, StoryLine>>();
        public static List<KeyValuePair<Inline, StoryLine>> ReChoices = new List<KeyValuePair<Inline, StoryLine>>();

        public static void Restart()
        {
            MainWindow.I.EndHallu();
            Doc.Remove(End1);
            Doc.Remove(End2);
            MainParagraph.Clear();
            ChoiceMenu.Clear();
            Choices.Clear();
            ReChoices.Clear();
            CurrentCollection = Main;
            CurrentCollection.Render();
        }
        public static void Doc_Click(object sender, RoutedEventArgs e)
        {
            if (Current?.Type == StoryLineType.Click)
            {
                var scroll = MainWindow.I.BottomScrolled;
                var index = CurrentCollection.Lines.IndexOf(Current);
                for (int i = index + 1; i < CurrentCollection.Lines.Count; i++)
                    if (!CurrentCollection.Lines[i].Render())
                    {
                        if (scroll)
                            MainWindow.I.BeginAutoScroll();
                        return;
                    }
            }
        }
        public static void Choice_Click(object sender, RoutedEventArgs e)
        {
            var scroll = MainWindow.I.BottomScrolled;
            ChoiceMenu.Clear();
            foreach(var pair in Choices)
                if (pair.Key == sender)
                {
                    pair.Value.Render();
                    if (scroll) MainWindow.I.BeginAutoScroll();
                    break;
                }
        }
        public static void ReChoice_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.I.EndHallu();
            ChoiceMenu.Clear();
            Doc.Remove(End1);
            Doc.Remove(End2);
            int i = 0;
            foreach (var pair in ReChoices)
            {
                if (pair.Key == sender)
                {
                    var main = (System.Collections.IList)MainParagraph;
                    var index = main.IndexOf(pair.Key);
                    for(var j = main.Count - 1; j >= index; j--) main.RemoveAt(j);
                    ReChoices.RemoveRange(i, ReChoices.Count - i);
                    pair.Value.Render();
                    break;
                }
                i++;
            }
        }
        public static void RenderEND(string achievement)
        {
            SteamUserStats.SetAchievement(achievement);
            SteamUserStats.StoreStats();
            Doc.Add(End1);
            Doc.Add(End2);
        }
    }
}
