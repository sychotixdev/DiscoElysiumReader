using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoElysiumVoice
{
    public class ReaderDataModel
    {
        public ReaderDataModel()
        {
            CustomVoices = new List<CustomVoiceConfig>();
        }

        public string DefaultVoice { get; set; }
        public string GamePath { get; set; }
        public List<CustomVoiceConfig> CustomVoices { get; set; }
    }
}
