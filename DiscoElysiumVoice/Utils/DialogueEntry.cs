using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoElysiumReader.Mod.Utils
{
    [Serializable]
    public class ReaderDialogueEntry
    {
        public ReaderDialogueEntry()
        {

        }

        public ReaderDialogueEntry(string speakerName, bool isPlayerCharacter, string message, string sequence, long uniqueMessageId)
        {
            this.SpeakerName = speakerName;
            this.IsPlayerCharacter = isPlayerCharacter;
            this.Message = message;
            this.Sequence = sequence;
            this.UniqueMessageId = uniqueMessageId;
        }

        public string SpeakerName { get; set; }
        public bool IsPlayerCharacter { get; set; }
        public string Message { get; set; }
        public string Sequence { get; set; }
        public long UniqueMessageId { get; set; }
    }
}