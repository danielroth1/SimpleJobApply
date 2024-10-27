using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Data.SqlTypes;
using System.IO;
using System.Windows.Markup;
using System.Printing;
using System.Reflection;

namespace SimpleJobApply.View
{
    public class HighlightTextBlock
    {
        public static string GetTextContent(DependencyObject obj)
        {
            return (string)obj.GetValue(TextContentProperty);
        }

        public static void SetTextContent(DependencyObject obj, string value)
        {
            obj.SetValue(TextContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Bold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.RegisterAttached("TextContent", typeof(string), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RefreshTextContent));

        #region Bold

        public static bool GetBold(DependencyObject obj)
        {
            return (bool)obj.GetValue(BoldProperty);
        }

        public static void SetBold(DependencyObject obj, bool value)
        {
            obj.SetValue(BoldProperty, value);
        }

        // Using a DependencyProperty as the backing store for Bold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoldProperty =
            DependencyProperty.RegisterAttached("Bold", typeof(bool), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));

        #endregion

        #region Italic

        public static bool GetItalic(DependencyObject obj)
        {
            return (bool)obj.GetValue(ItalicProperty);
        }

        public static void SetItalic(DependencyObject obj, bool value)
        {
            obj.SetValue(ItalicProperty, value);
        }

        // Using a DependencyProperty as the backing store for Italic.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItalicProperty =
            DependencyProperty.RegisterAttached("Italic", typeof(bool), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));

        #endregion

        #region Underline

        public static bool GetUnderline(DependencyObject obj)
        {
            return (bool)obj.GetValue(UnderlineProperty);
        }

        public static void SetUnderline(DependencyObject obj, bool value)
        {
            obj.SetValue(UnderlineProperty, value);
        }

        // Using a DependencyProperty as the backing store for Underline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnderlineProperty =
            DependencyProperty.RegisterAttached("Underline", typeof(bool), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));

        #endregion

        #region HighlightTexts

        public static ICollection<HighlightTextColorPair> GetHightlightTextColorPairs(DependencyObject obj)
        {
            return (ICollection<HighlightTextColorPair>)obj.GetValue(HightlightTextColorPairsProperty);
        }

        public static void SetHightlightTextColorPairs(DependencyObject obj, ICollection<HighlightTextColorPair> value)
        {
            obj.SetValue(HightlightTextColorPairsProperty, value);
        }

        // Using a DependencyProperty as the backing store for HightlightText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HightlightTextColorPairsProperty =
            DependencyProperty.RegisterAttached("HightlightTextColorPairs", typeof(ICollection<HighlightTextColorPair>), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));

        #endregion

        #region InternalText

        protected static string GetInternalText(DependencyObject obj)
        {
            return (string)obj.GetValue(InternalTextProperty);
        }

        protected static void SetInternalText(DependencyObject obj, string value)
        {
            obj.SetValue(InternalTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalText.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty InternalTextProperty =
            DependencyProperty.RegisterAttached("InternalText", typeof(string), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnInternalTextChanged));

        private static void OnInternalTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as RichTextBox;

            if (textBox != null)
            {
                //textBox.Text = e.NewValue as string;
                Highlight(textBox);
            }
        }

        #endregion

        #region  IsBusy 

        private static bool GetIsBusy(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBusyProperty);
        }

        private static void SetIsBusy(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBusyProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsBusy.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached("IsBusy", typeof(bool), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Document Xaml

        private static HashSet<Thread> _recursionProtection = new HashSet<Thread>();

        public static string GetDocumentXaml(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentXamlProperty);
        }

        public static void SetDocumentXaml(DependencyObject obj, string value)
        {
            _recursionProtection.Add(Thread.CurrentThread);
            obj.SetValue(DocumentXamlProperty, value);
            _recursionProtection.Remove(Thread.CurrentThread);
        }

        public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
            "DocumentXaml",
            typeof(string),
            typeof(HighlightTextBlock),
            new FrameworkPropertyMetadata(
                "",
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (obj, e) => {
                    if (_recursionProtection.Contains(Thread.CurrentThread))
                        return;

                    var richTextBox = (RichTextBox)obj;
                    try
                    {
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(GetDocumentXaml(richTextBox)));
                        var doc = (FlowDocument)XamlReader.Load(stream);

                        richTextBox.Document = doc;
                    }
                    catch (Exception)
                    {
                        richTextBox.Document = new FlowDocument();
                    }

                    richTextBox.TextChanged += (obj2, e2) =>
                    {
                        RichTextBox richTextBox2 = obj2 as RichTextBox;
                        if (richTextBox2 != null)
                        {
                            SetDocumentXaml(richTextBox, XamlWriter.Save(richTextBox2.Document));
                        }
                    };
                }
            )
        );

        #endregion

        #region Methods

        private static void RefreshTextContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Highlight(d as RichTextBox);
        }

        private static string Convert(Block block)
        {
            string content = "";
            if (block is System.Windows.Documents.List list)
            {
                int index = 0;
                foreach (ListItem item in list.ListItems)
                {
                    foreach (Block b2 in item.Blocks)
                    {
                        content += Convert(b2);
                    }
                    if (index < list.ListItems.Count - 1)
                        content += "\n";
                    index++;
                }
            }
            else if (block is Paragraph p)
            {
                int index = 0;
                foreach (Inline inline in p.Inlines)
                {
                    content += ConvertToText(inline);
                    //if (index < p.Inlines.Count - 1 && ConvertToText(p.Inlines.ElementAt(index + 1)) != "")
                    //    content += "\n";
                    index++;
                }
            }
            return content;
        }

        private static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox textBox = d as RichTextBox;
            if (textBox == null)
                return;
            string content = "";
            int index = 0;
            foreach (Block block in textBox.Document.Blocks)
            {
                //if (block is System.Windows.Documents.List list)
                //{
                //    foreach (ListItem item in list.ListItems)
                //    {
                //        item.Blocks
                //    }
                //}
                //else if (block is Paragraph p)
                //{
                //    foreach (Inline inline in p.Inlines)
                //    {
                //        content += ConvertToText(inline);
                //    }
                   
                //}
                content += Convert(block);
                if (index < textBox.Document.Blocks.Count - 1)
                    content += "\n";
                index++;
            }
            // Alternative way. Problem: adds empty spaces at end of paragraphs
            //string content = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text;
            //if (content != "")
            SetTextContent(textBox, content);

            Highlight(d as RichTextBox);
        }

        private static string ConvertToText(Inline inline)
        {
            string text = "";
            if (inline is Span span)
            {
                foreach (Inline i in span.Inlines)
                {
                    text += ConvertToText(i);
                }
            }
            else if (inline is Run run)
            {
                text += run.Text;
            }
            else if (inline is LineBreak lineBreak)
            {
                text += "\n";
            }
            return text;
        }

        private static void Highlight(RichTextBox textBox)
        {
            if (textBox == null) return;

            //string content = "";
            //foreach (Block block in textBox.Document.Blocks)
            //{
            //    if (block is Paragraph p)
            //    {
            //        foreach (Inline inline in p.Inlines)
            //        {
            //            if (inline is Run run)
            //            {
            //                content += run.Text;
            //            }
            //        }
            //    }
            //}
            //if (content != "")
            //    SetTextContent(textBox, content);
            string text = GetTextContent(textBox);

            //if (textBox.GetBindingExpression(InternalTextProperty) == null)
            //{
            //    var textBinding = textBox.GetBindingExpression(TextBlock.TextProperty);

            //    if (textBinding != null)
            //    {
            //        textBox.SetBinding(InternalTextProperty, textBinding.ParentBindingBase);

            //        var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

            //        propertyDescriptor.RemoveValueChanged(textBox, OnTextChanged);
            //    }
            //    else
            //    {
            //        var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

            //        propertyDescriptor.AddValueChanged(textBox, OnTextChanged);

            //        //textBox.Unloaded -= Textblock_Unloaded;
            //        //textBox.Unloaded += Textblock_Unloaded;
            //    }
            //}
            
            textBox.Loaded -= TextBox_Loaded;
            textBox.Loaded += TextBox_Loaded;
            //textBox.Unloaded -= Textblock_Unloaded;
            //textBox.Unloaded += Textblock_Unloaded;
            //textBox.KeyUp -= TextBox_KeyUp;
            //textBox.KeyUp += TextBox_KeyUp;
            if (true || !string.IsNullOrEmpty(text))
            {
                SetIsBusy(textBox, true);

                ICollection<HighlightTextColorPair> toHighlights = GetHightlightTextColorPairs(textBox);
                textBox.Document = new FlowDocument();
                textBox.Document.Blocks.Add(new Paragraph());

                if (toHighlights == null || toHighlights.Count == 0)
                {
                    AddInline(textBox, GetTextContent(textBox), toHighlights);
                }
                else
                {
                    string regex = "(";
                    for (int i = 0; i < toHighlights.Count; i++)
                    {
                        HighlightTextColorPair pair = toHighlights.ElementAt(i);
                        //pair.HighlightText.Replace(" ", "");
                        string expression = Regex.Escape(pair.HighlightText);
                        if (expression != "")
                        {
                            regex += Regex.Escape(pair.HighlightText);
                            if (i != toHighlights.Count - 1)
                                regex += "|";
                        }
                    }
                    regex += ")";

                    var matches = Regex.Split(text, regex, RegexOptions.IgnoreCase);

                    foreach (var subString in matches)
                    {
                        AddInline(textBox, subString, toHighlights);
                    }
                }
                
                SetIsBusy(textBox, false);
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            RichTextBox textBox = (RichTextBox)sender;
            textBox.LostKeyboardFocus -= TextBox_LostKeyboardFocus;
            textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.GotFocus -= TextBox_GotFocus;
            textBox.GotFocus += TextBox_GotFocus;
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private static void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Refresh((RichTextBox)sender, new DependencyPropertyChangedEventArgs());
        }

        private static void TextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Refresh((RichTextBox)sender, new DependencyPropertyChangedEventArgs());
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Refresh((RichTextBox)sender, new DependencyPropertyChangedEventArgs());
        }

        private static void TextBox_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh((RichTextBox)sender, new DependencyPropertyChangedEventArgs());
        }

        private static void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Refresh((RichTextBox)sender, new DependencyPropertyChangedEventArgs());
        }

        private static void AddInline(RichTextBox textBox, string subString, ICollection<HighlightTextColorPair> highlightPairs)
        {
            bool success = false;
            if (highlightPairs != null)
            {
                foreach (HighlightTextColorPair pair in highlightPairs)
                {
                    if (string.Compare(subString, pair.HighlightText, true) == 0)
                    {
                        var formattedText = new Run(subString);
                        if (pair.HighlightBrushBackground != null)
                            formattedText.Background = pair.HighlightBrushBackground;
                        if (pair.HighlightBrushForeground != null)
                            formattedText.Foreground = pair.HighlightBrushForeground;

                        if (GetBold(textBox))
                        {
                            formattedText.FontWeight = FontWeights.Bold;
                        }

                        if (GetItalic(textBox))
                        {
                            formattedText.FontStyle = FontStyles.Italic;
                        }

                        if (GetUnderline(textBox))
                        {
                            formattedText.TextDecorations.Add(TextDecorations.Underline);
                        }

                        (textBox.Document.Blocks.First() as Paragraph).Inlines.Add(formattedText);
                        success = true;
                        break;
                    }
                }
            }
            if (!success)
            {
                (textBox.Document.Blocks.First() as Paragraph).Inlines.Add(subString);
            }
        }

        private static void Textblock_Unloaded(object sender, RoutedEventArgs e)
        {
            //var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(System.Windows.Controls.RichTextBox.DocumentXaml, typeof(TextBlock));

            //propertyDescriptor.RemoveValueChanged(sender as RichTextBox, OnTextChanged);
            ((RichTextBox) sender).KeyDown -= TextBox_KeyDown;
            //((RichTextBox)sender).LostFocus -= ;
        }

        private static void OnTextChanged(object sender, EventArgs e)
        {
            var textBox = sender as RichTextBox;

            if (textBox != null &&
                !GetIsBusy(textBox))
            {
                Highlight(textBox);
            }
        }

        #endregion
    }
}
