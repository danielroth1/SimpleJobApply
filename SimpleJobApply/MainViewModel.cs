using SimpleJobApply.Model;
using SimpleJobApply.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleJobApply
{
    internal class MainViewModel : ViewModelBase
    {
        public static string ApplicationName = "Simple Job Apply";

        private ObservableCollection<Paragraph>? paragraphDetails;
        private string? jobAdText;
        private List<HighlightTextColorPair> highlighTextColorPairs;
        private string? generatedText;
        private List<HighlightTextColorPair> highlighGeneratedTextColorPairs;
        private string windowTitle;
        private string lastLoadedFilename;

        public string WindowTitle { get => windowTitle; set => SetProperty(ref windowTitle, value); }
        public ObservableCollection<Paragraph>? ParagraphDetails { get => paragraphDetails; set => SetProperty(ref paragraphDetails, value); }
        public string? JobAdText { get => jobAdText; set => SetProperty(ref jobAdText, value); }
        public string? GeneratedText { get => generatedText; set => SetProperty(ref generatedText, value); }
        public List<HighlightTextColorPair> HighlighTextColorPairs { get => highlighTextColorPairs; set => SetProperty(ref highlighTextColorPairs, value); }
        public List<HighlightTextColorPair> HighlighGeneratedTextColorPairs { get => highlighGeneratedTextColorPairs; set => SetProperty(ref highlighGeneratedTextColorPairs, value); }

        public ICommand SaveCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand PasteJobAdCommand { get; set; }
        public ICommand GenerateCommand { get; set; }
        public ICommand AddParagraphCommand { get; set; }
        public ICommand RemoveLastParagraphCommand { get; set; }
        public ICommand ScanAdCommand { get; set; }

        public MainViewModel()
        {
            WindowTitle = ApplicationName;
            HighlighTextColorPairs = new List<HighlightTextColorPair>();
            ParagraphDetails = new ObservableCollection<Paragraph>()
            {
                new Paragraph()
                {
                    Content = "(Replace this text with a paragraph.)",
                    IsSelected = true,
                    Keywords = "(Replace this with comma separated keywords.)"
                }
            };
            InitializeParagraphs();
            JobAdText = "(Select and copy the job ad (ctrl + c) and the press the \"PasteJobAd\" button below. This will replace this text with the job ad, scan it for keywords, and generate the paragraphs for the cover letter. Load example-full-stack.sja in the installation directory of this tool as an example.)";

            SaveCommand = new RelayCommand(p => Save());
            SaveAsCommand = new RelayCommand(p => SaveWithDialog());
            LoadCommand = new RelayCommand(p => LoadWithDialog());
            PasteJobAdCommand = new RelayCommand(p => PasteJobAd());
            GenerateCommand = new RelayCommand(p => GenerateAndCopyToClipboard());
            AddParagraphCommand = new RelayCommand(p => AddParagraph());
            RemoveLastParagraphCommand = new RelayCommand(p => RemoveLastParagraph());
            ScanAdCommand = new RelayCommand(p => ScanAd());

            if (App.Args.Length > 0)
            {
                string filename = App.Args[0];
                Load(filename);
            }
        }

        private void PasteJobAd()
        {
            JobAdText = Clipboard.GetText();
            ScanAd();
            GenerateAndCopyToClipboard();
        }

        private System.Windows.Media.Color GetParagraphColor(int index)
        {
            System.Windows.Media.Color[] basicColors = new System.Windows.Media.Color[]
            {
                Colors.Green,
                Colors.Red,
                Colors.Blue,
                Colors.Magenta,
                Colors.Yellow,
                Colors.Orange,
                Colors.Teal,
                Colors.Cyan,
                Colors.Brown,
                Colors.Pink,
                Colors.Lime,
                Colors.Navy,
                Colors.Maroon,
                Colors.Olive,
                Colors.Silver,
                Colors.Gold,
                Colors.Purple,
            };
            basicColors[index % basicColors.Length].A = 50;
            return basicColors[index % basicColors.Length];
        }

        private void ScanAd()
        {
            HighlighTextColorPairs = new List<HighlightTextColorPair>();
            int paragraphIndex = 0;
            foreach (Paragraph p in paragraphDetails)
            {
                p.IsSelected = p.IsAlwaysShow;
                if (p.Keywords != null)
                {
                    List<HighlightTextColorPair> paragraphHighlights = new List<HighlightTextColorPair>();
                    List<string> keywords = p.Keywords.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string keyword in keywords)
                    {
                        string keywordTemp = keyword;
                        if (keywordTemp.Length > 0 && keywordTemp.ElementAt(0) == ' ')
                            keywordTemp = keywordTemp.Substring(1);
                        keywordTemp = keywordTemp.ToLower();
                        bool contains = JobAdText.ToLower().Contains(keywordTemp);
                        if (contains)
                        {
                            p.IsSelected = true;
                            System.Windows.Media.Color c = GetParagraphColor(paragraphIndex);
                            if (!HighlighTextColorPairs.Any(p => p.HighlightText == keywordTemp))
                            {
                                HighlighTextColorPairs.Add(new HighlightTextColorPair()
                                {
                                    HighlightText = keywordTemp,
                                    HighlightBrushBackground = new SolidColorBrush(c)
                                });
                            }
                            paragraphHighlights.Add(new HighlightTextColorPair()
                            {
                                HighlightText = keywordTemp,
                                HighlightBrushBackground = new SolidColorBrush(c)
                            });
                        }
                    }
                    p.HighlighTextColorPairs = paragraphHighlights;
                    if (p.IsSelected)
                    {
                        p.ParagraphHighlighTextColorPairs = new List<HighlightTextColorPair>()
                        {
                            new HighlightTextColorPair()
                            {
                                HighlightText = p.Content,
                                HighlightBrushBackground = new SolidColorBrush(GetParagraphColor(paragraphIndex))
                            }
                        };
                    }
                    else
                    {
                        p.ParagraphHighlighTextColorPairs = new List<HighlightTextColorPair>();
                    }
                }
                paragraphIndex++;

            }
            HighlighTextColorPairs = new List<HighlightTextColorPair>(HighlighTextColorPairs);
            //OnPropertyChanged(nameof(HighlighTextColorPairs));
            //foreach (Paragraph paragraph in paragraphDetails)
            //{
            //    paragraph.HighlighTextColorPairs = HighlighTextColorPairs;
            //}
        }

        private void RemoveLastParagraph()
        {
            if (ParagraphDetails.Count > 0)
                ParagraphDetails.RemoveAt(ParagraphDetails.Count - 1);
        }

        private void AddParagraph()
        {
            ParagraphDetails.Add(new Paragraph());
            InitializeParagraphs();
        }

        private void InitializeParagraphs()
        {
            foreach (Paragraph paragraph in  ParagraphDetails)
            {
                paragraph.MoveUpCommand = new RelayCommand(p => MoveParagraphGroup(paragraph, true));
                paragraph.MoveDownCommand = new RelayCommand(p => MoveParagraphGroup(paragraph, false));
                paragraph.AddParagraphBelowCommand = new RelayCommand(p => AddParagraphBelow(paragraph));
                paragraph.RemoveParagraphCommand = new RelayCommand(p => RemoveParagraph(paragraph));
                paragraph.PropertyChanged += Paragraph_PropertyChanged;
            }
            RefreshParagraphColors();
        }

        private void RefreshColors()
        {
            HighlighTextColorPairs = new List<HighlightTextColorPair>();
            for (int i = 0; i < ParagraphDetails.Count; i++)
            {
                Paragraph p = ParagraphDetails[i];
                if (p.Keywords != null)
                {
                    List<HighlightTextColorPair> paragraphHighlights = new List<HighlightTextColorPair>();
                    List<string> keywords = p.Keywords.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string keyword in keywords)
                    {
                        string keywordTemp = keyword;
                        if (keywordTemp.Length > 0 && keywordTemp.ElementAt(0) == ' ')
                            keywordTemp = keywordTemp.Substring(1);
                        keywordTemp = keywordTemp.ToLower();
                        bool contains = JobAdText.ToLower().Contains(keywordTemp);
                        if (contains)
                        {
                            System.Windows.Media.Color c = GetParagraphColor(i);
                            if (!HighlighTextColorPairs.Any(p => p.HighlightText == keywordTemp))
                            {
                                HighlighTextColorPairs.Add(new HighlightTextColorPair()
                                {
                                    HighlightText = keywordTemp,
                                    HighlightBrushBackground = new SolidColorBrush(c)
                                });
                            }
                            paragraphHighlights.Add(new HighlightTextColorPair()
                            {
                                HighlightText = keywordTemp,
                                HighlightBrushBackground = new SolidColorBrush(c)
                            });
                        }
                    }
                    p.HighlighTextColorPairs = paragraphHighlights;
                    if (p.IsSelected)
                    {
                        p.ParagraphHighlighTextColorPairs = new List<HighlightTextColorPair>()
                        {
                            new HighlightTextColorPair()
                            {
                                HighlightText = p.Content,
                                HighlightBrushBackground = new SolidColorBrush(GetParagraphColor(i))
                            }
                        };
                    }
                    else
                    {
                        p.ParagraphHighlighTextColorPairs = new List<HighlightTextColorPair>();
                    }
                }

            }
            HighlighTextColorPairs = new List<HighlightTextColorPair>(HighlighTextColorPairs);
        }

        private void RefreshParagraphColors()
        {
            foreach (Paragraph paragraph in ParagraphDetails)
            {
                if (paragraph.GroupNumber != 0)
                {
                    paragraph.BackgroundColorBrush = new SolidColorBrush(GetParagraphColor(paragraph.GroupNumber));
                }
            }
        }

        private void Paragraph_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Paragraph.GroupNumber))
            {
                RefreshParagraphColors();
            }
            else if (e.PropertyName == nameof(Paragraph.IsSelected))
            {
                GenerateAndCopyToClipboard();
            }
        }

        private void AddParagraphBelow(Paragraph p)
        {
            int index = ParagraphDetails.IndexOf(p);
            if (index >= 0)
            {
                ParagraphDetails.Insert(index + 1, new Paragraph()
                {
                    GroupNumber = p.GroupNumber
                });
                InitializeParagraphs();
            }
        }

        private void RemoveParagraph(Paragraph p)
        {
            MessageBoxResult result = MessageBox.Show("Do you really wish to remove this paragraph?", "Remove Paragraph", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (ParagraphDetails.Contains(p))
                {
                    ParagraphDetails.Remove(p);
                    InitializeParagraphs();
                }
            }
        }

        private void MoveParagraphGroup(Paragraph p, bool up)
        {
            int index = ParagraphDetails.IndexOf(p);
            if (index < 0)
                return;

            List<Paragraph> toMove = new List<Paragraph>();
            toMove.Add(p);
            if (p.GroupNumber != 0)
            {
                if (up && index > 0 && ParagraphDetails[index - 1].GroupNumber != p.GroupNumber)
                {
                    for (int i = index + 1; i < ParagraphDetails.Count; i++)
                    {
                        if (i > ParagraphDetails.Count - 1 || p.GroupNumber != ParagraphDetails[i].GroupNumber)
                            break;
                        toMove.Add(ParagraphDetails[i]);
                    }
                }
                else if (!up && index < ParagraphDetails.Count - 1 && ParagraphDetails[index + 1].GroupNumber != p.GroupNumber)
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        if (i < 0 || p.GroupNumber != ParagraphDetails[i].GroupNumber)
                            break;
                        toMove.Add(ParagraphDetails[i]);
                    }
                }
            }
            foreach (Paragraph paragraph in toMove)
            {
                MoveParagraph(paragraph, up);
            }

            RefreshColors();
            GenerateAndCopyToClipboard();
        }

        private void MoveParagraph(Paragraph p, bool up)
        {
            int indexSource = ParagraphDetails.IndexOf(p);
            if (indexSource >= 0)
            {
                int indexTarget = indexSource;
                if (up && indexTarget - 1 >= 0)
                {
                    for (int i = indexTarget - 1; i >= 0; i--)
                    {
                        if (indexTarget - 1 >= 0 && ParagraphDetails[indexTarget - 1].GroupNumber != 0 && p.GroupNumber != ParagraphDetails[indexTarget - 1].GroupNumber && indexTarget - 2 >= 0 && ParagraphDetails[indexTarget - 1].GroupNumber == ParagraphDetails[indexTarget - 2].GroupNumber)
                            indexTarget--;
                    }
                    ParagraphDetails.Move(indexSource, indexTarget - 1);
                }
                else if (!up && indexTarget + 1 < ParagraphDetails.Count)
                {
                    for (int i = indexTarget + 1; i < ParagraphDetails.Count; i++)
                    {
                        if (indexTarget + 1 < ParagraphDetails.Count && ParagraphDetails[indexTarget + 1].GroupNumber != 0 && p.GroupNumber != ParagraphDetails[indexTarget + 1].GroupNumber && indexTarget + 2 < ParagraphDetails.Count && ParagraphDetails[indexTarget + 1].GroupNumber == ParagraphDetails[indexTarget + 2].GroupNumber)
                            indexTarget++;
                    }
                    ParagraphDetails.Move(indexSource, indexTarget + 1);
                }
            }
        }

        private void ClearParagraphs()
        {

        }

        private string CreateCombinedParagraphs()
        {
            string combined = "";
            foreach (Paragraph p in ParagraphDetails)
            {
                if (p.IsSelected)
                    combined += p.Content + (p.IsNoLineBreak ? "" : "\n");
            }
            return combined;
        }

        private void GenerateAndCopyToClipboard()
        {
            GeneratedText = CreateCombinedParagraphs();
            Clipboard.SetText(GeneratedText);
            HighlighGeneratedTextColorPairs = new List<HighlightTextColorPair>();
            int paragraphIndex = 0;
            foreach (Paragraph p in ParagraphDetails)
            {
                if (p.IsSelected)
                {
                    HighlighGeneratedTextColorPairs.Add(new HighlightTextColorPair()
                    {
                        HighlightText = p.Content,
                        HighlightBrushBackground = new SolidColorBrush(GetParagraphColor(paragraphIndex))
                    });
                }
                paragraphIndex++;
            }
            HighlighGeneratedTextColorPairs = new List<HighlightTextColorPair>(HighlighGeneratedTextColorPairs);
            //highlighGeneratedTextColorPairs = new ObservableCollection<HighlightTextColorPair>(HighlighGeneratedTextColorPairs);
            //OnPropertyChanged(nameof(HighlighGeneratedTextColorPairs));
        }

        private string OpenDialog(bool isSave)
        {
            Microsoft.Win32.FileDialog dialog;
            if (isSave)
                dialog = new Microsoft.Win32.SaveFileDialog();
            else
                dialog = new Microsoft.Win32.OpenFileDialog();
            string lastLoadedFilename = Path.GetFileName(LastLoadedFilename);
            dialog.FileName = string.IsNullOrEmpty(lastLoadedFilename) ? "Document" : lastLoadedFilename; // Default file name
            dialog.DefaultExt = ".sja"; // Default file extension
            dialog.Filter = "Simple Job Ad File (.sja)|*.sja|Json File (.json)|*.json"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                return filename;
            }
            return "";
        }

        private string LastLoadedFilename
        {
            get => lastLoadedFilename;
            set
            {
                lastLoadedFilename = value;
                string filename = Path.GetFileName(value);
                WindowTitle = string.IsNullOrEmpty(filename) ? ApplicationName : ApplicationName + " - " + filename;
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(LastLoadedFilename))
            {
                SaveWithDialog();
            }
            else
            {
                Save(LastLoadedFilename);
            }
        }

        public void SaveWithDialog()
        {
            string filename = OpenDialog(true);
            Save(filename);
        }

        public void LoadWithDialog()
        {
            string filename = OpenDialog(false);
            Load(filename);
        }

        public void Save(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;
            string jsonString = JsonSerializer.Serialize(new Serialization()
            {
                ParagraphDetails = new List<Paragraph>(ParagraphDetails),
                JobAd = JobAdText
            });
            File.WriteAllText(filename, jsonString);
            LastLoadedFilename = filename;
        }

        public void Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;
            LastLoadedFilename = filename;
            HighlighTextColorPairs = new List<HighlightTextColorPair>();
            GeneratedText = "";
            ClearParagraphs();
            try
            {
                using (StreamReader r = new StreamReader(filename))
                {
                    string json = r.ReadToEnd();
                    Serialization? serialization = JsonSerializer.Deserialize<Serialization>(json);
                    ParagraphDetails = new ObservableCollection<Paragraph>(serialization.ParagraphDetails);
                    JobAdText = serialization.JobAd;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Something went wrong reading the file: " + ex.Message);
            }
            InitializeParagraphs();
        }
    }
}
