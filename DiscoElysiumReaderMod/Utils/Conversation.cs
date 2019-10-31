using System;
using System.Collections.Generic;

namespace DiscoElysiumReader.Mod.Utils
{
    [Serializable]
    public class Conversation
    {
        public Conversation() : this(-1, "unknown", "unknown")
        {

        }

        public Conversation(int conversationId, string actorName, string conversant)
        {
            DialogueEntries = new List<ReaderDialogueEntry>();
            ConversationId = conversationId;
            ActorName = actorName;
            Conversant = conversant;
        }

        public string ActorName { get; set; }
        public string Conversant { get; set; }
        public int ConversationId { get; set; }
        public List<ReaderDialogueEntry> DialogueEntries { get; set; }
    }
}