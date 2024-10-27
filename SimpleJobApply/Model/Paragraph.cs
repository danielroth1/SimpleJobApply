using SimpleJobApply.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleJobApply.Model
{
    public class Paragraph : ViewModelBase
    {
        private bool isSelected;
        private string? keywords;
        private string? content;
        private List<HighlightTextColorPair>? highlighTextColorPairs = new List<HighlightTextColorPair>();
        private int groupNumber;
        private ICommand moveUpCommand;
        private ICommand moveDownCommand;
        private Brush backgroundColorBrush;
        private bool isAlwaysShow;
        private bool isNoLineBreak;
        private List<HighlightTextColorPair>? paragraphHighlighTextColorPairs = new List<HighlightTextColorPair>();
        private ICommand addParagraphBelowCommand;
        private ICommand removeParagraphCommand;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
        public string? Keywords { get => keywords; set => SetProperty(ref keywords, value); }
        public string? Content { get => content; set => SetProperty(ref content, value); }
        public int GroupNumber { get => groupNumber; set => SetProperty(ref groupNumber, value); }
        public bool IsAlwaysShow { get => isAlwaysShow; set => SetProperty(ref isAlwaysShow, value); }
        public bool IsNoLineBreak { get => isNoLineBreak; set => SetProperty(ref isNoLineBreak, value); }
        [JsonIgnore]
        public Brush BackgroundColorBrush { get => backgroundColorBrush; set => SetProperty(ref backgroundColorBrush, value); }
        [JsonIgnore]
        public List<HighlightTextColorPair>? HighlighTextColorPairs { get => highlighTextColorPairs; set => SetProperty(ref highlighTextColorPairs, value); }
        [JsonIgnore]
        public List<HighlightTextColorPair>? ParagraphHighlighTextColorPairs { get => paragraphHighlighTextColorPairs; set => SetProperty(ref paragraphHighlighTextColorPairs, value); }

        [JsonIgnore]
        public ICommand MoveUpCommand { get => moveUpCommand; set => SetProperty(ref moveUpCommand, value); }
        [JsonIgnore]
        public ICommand MoveDownCommand { get => moveDownCommand; set => SetProperty(ref moveDownCommand, value); }
        [JsonIgnore]
        public ICommand AddParagraphBelowCommand { get => addParagraphBelowCommand; set => SetProperty(ref addParagraphBelowCommand, value); }
        [JsonIgnore]
        public ICommand RemoveParagraphCommand { get => removeParagraphCommand; set => SetProperty(ref removeParagraphCommand, value); }



    }
}
