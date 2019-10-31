using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoElysiumVoice
{
    public class CustomVoiceConfig
    {
        public string ConversantName { get; set; }
        public string VoiceName { get; set; }
        public string DisplayText => $"{ConversantName} - {VoiceName}";
    }
}
