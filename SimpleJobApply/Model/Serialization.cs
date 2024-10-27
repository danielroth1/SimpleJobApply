using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJobApply.Model
{
    internal class Serialization
    {
        public List<Paragraph>? ParagraphDetails { get; set; }
        public string? JobAd { get; set; }
    }
}
