using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace YinTaohua
{
    enum StoryLineType
    {
        Text,
        Click,
        Choice,
        Choices,
        Collection,
        Jump,
        End,
        Effect
    }
    internal class StoryLine
    {
        public StoryLineType Type;
        public string Text;
        public bool LineBreak;
        public List<StoryLine> Lines;
        public Brush Color;

        public bool Render()
        {
            switch(Type)
            {
                case StoryLineType.Effect:
                    MainWindow.I.BeginHallu();
                    return true;
                case StoryLineType.Text:
                    if (!string.IsNullOrWhiteSpace(Text)) Story.MainParagraph.Add(Text);
                    if (LineBreak) Story.MainParagraph.Add(new LineBreak());
                    return true; ;
                case StoryLineType.Click:
                    Story.Current = this;
                    return false;
                case StoryLineType.Choices:
                    Story.Choices.Clear();
                    foreach(var choice in Lines)
                    {
                        var hyperlink = new Hyperlink(new Run(choice.Text)) { Foreground = choice.Color };
                        if (choice.Color != null) hyperlink.Foreground = choice.Color;
                        else hyperlink.SetBinding(Hyperlink.ForegroundProperty, new Binding("Foreground") { ElementName = "mw" });
                        hyperlink.Click += Story.Choice_Click;
                        Story.Choices.Add(new KeyValuePair<Inline, StoryLine>(hyperlink, choice));
                        Story.ChoiceMenu.Add(hyperlink);
                        Story.ChoiceMenu.Add(new LineBreak());
                    }
                    Story.CurrentCollection = this;
                    Story.Current = null;
                    return false;
                case StoryLineType.Choice:
                    bool first = true;
                    foreach (var text in Lines)
                        if (first)
                        {
                            var hyperlink = new Hyperlink(new Run(text.Text));
                            hyperlink.SetBinding(Hyperlink.ForegroundProperty, new Binding("Foreground") { ElementName = "mw" });
                            hyperlink.Click += Story.ReChoice_Click;
                            Story.ReChoices.Add(new KeyValuePair<Inline, StoryLine>(hyperlink, Story.CurrentCollection));
                            Story.CurrentCollection = this;
                            Story.MainParagraph.Add(hyperlink);
                            if (text.LineBreak) Story.MainParagraph.Add(new LineBreak());
                            first = false;
                        }
                        else if (!text.Render()) break;
                    break;
                case StoryLineType.Collection:
                    Story.CurrentCollection = this;
                    foreach(var line in Lines)
                        if (!line.Render()) break;
                    break;
                case StoryLineType.Jump:
                    Story.Current = null;
                    if (Text == "g") Story.g.Render();
                    break;
                case StoryLineType.End:
                    Story.Current = null;
                    Story.RenderEND(Text);
                    break;
            }
            return false;
        }
    }
}
