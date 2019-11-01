using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DiscoElysiumReader.Mod.Utils;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using Voidforge;
using Conversation = DiscoElysiumReader.Mod.Utils.Conversation;

namespace DiscoElysiumReader.Mod
{
    class DialogReader : MonoBehaviour
    {
        private Sunshine.ConversationLogger ConversationLogger { get; set; }

        public Conversation Conversation;

        private void OnEnable()
        {
            Lua.RegisterFunction("DialogReaderAlwaysChanging", null, typeof(DialogReader).GetMethod("DialogReaderAlwaysChanging"));
        }

        private void OnDisable()
        {
            Lua.UnregisterFunction("DialogReaderAlwaysChanging");
        }

        protected void Start()
        {
            DialogueManager.AddLuaObserver("return DialogReaderAlwaysChanging();", LuaWatchFrequency.EveryDialogueEntry, new LuaChangedDelegate(this.DiscoReaderHandleNewDialogueEntry));
            ConversationLogger = SingletonComponent<Sunshine.ConversationLogger>.Singleton;
        }

        private static long UniqueDialogEntry = 0;
        public static long DialogReaderAlwaysChanging()
        {
            return UniqueDialogEntry++;
        }

        private void DiscoReaderHandleNewDialogueEntry(LuaWatchItem luaWatchItem, Lua.Result newResult)
        {
            try
            {
                if (DialogueManager.IsConversationActive && DialogueManager.Instance.ConversationModel.HasValidEntry)
                {
                    int conversationId = DialogueManager.Instance.ConversationModel.GetConversationID(DialogueManager.Instance.currentConversationState);
                    if (Conversation == null || Conversation.ConversationId != conversationId)
                    {
                        Conversation = new Conversation(conversationId, DialogueManager.Instance.ConversationModel.ActorInfo.Name, DialogueManager.Instance.ConversationModel.ConversantInfo.Name);
                    }

                    Subtitle dialogueSubtitle = DialogueManager.Instance.currentConversationState.subtitle;

                    if (dialogueSubtitle == null || dialogueSubtitle.dialogueEntry == null)
                        return;

                    FinalEntry finalEntry = ConversationLogger.AssembleFinalEntry(dialogueSubtitle.dialogueEntry);

                    Conversation.DialogueEntries.Add(new ReaderDialogueEntry(dialogueSubtitle.speakerInfo.Name, dialogueSubtitle.speakerInfo.IsPlayer, finalEntry.spokenLine, "", UniqueDialogEntry));


                }
                else
                {
                    Conversation = new Utils.Conversation();
                }


                // Now to handle outputting to the file...
                XmlSerializer SerializerObj = new XmlSerializer(typeof(Conversation));
                Stream stream = null;
                try
                {
                    stream = new FileStream(@"WriteText.txt", FileMode.Create, FileAccess.Write);
                    SerializerObj.Serialize(stream, Conversation);
                }
                finally
                {
                    stream?.Close();
                }
            }
            catch (Exception e)
            {
                // We never want to throw an exception
            }

            
        }
    }
}
