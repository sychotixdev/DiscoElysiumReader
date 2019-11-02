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

        private string Text { get; set; }
        private DateTime WhenToStopDisplaying { get; set; }

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

            ShowDebugMessage("Sychotix's voice mod successfully injected! :)", 10);
        }

        protected void OnGUI()
        {
            if (String.IsNullOrEmpty(Text))
                return;

            if (WhenToStopDisplaying < DateTime.Now)
            {
                Text = "";
                WhenToStopDisplaying = DateTime.MaxValue;
            }

            //GUI.Label(new Rect(10, 10, 100, 20), "Is this working yet lol");
            UIHelper.Begin(Text, 1, 1, 1000, 100, 5, 100, 100);
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
                    Subtitle dialogueSubtitle = DialogueManager.Instance.currentConversationState.subtitle;

                    if (dialogueSubtitle?.dialogueEntry == null)
                        return;

                    FinalEntry finalEntry = ConversationLogger.AssembleFinalEntry(dialogueSubtitle.dialogueEntry);

                    int conversationId = DialogueManager.Instance.ConversationModel.GetConversationID(DialogueManager.Instance.currentConversationState);
                    if (Conversation == null || Conversation.ConversationId != conversationId)
                    {
                        Conversation = new Conversation(conversationId, DialogueManager.Instance.ConversationModel.ActorInfo.Name, DialogueManager.Instance.ConversationModel.ConversantInfo.Name);
                    }

                    Conversation.DialogueEntries.Add(new ReaderDialogueEntry(dialogueSubtitle.speakerInfo.Name, dialogueSubtitle.speakerInfo.IsPlayer, finalEntry.spokenLine, "", UniqueDialogEntry));


                }
                else
                {
                    Conversation = new Utils.Conversation();
                }


                // Now to handle outputting to the file...
                XmlSerializer serializerObj = new XmlSerializer(typeof(Conversation));
                Stream stream = null;
                try
                {
                    stream = new FileStream(@"WriteText.txt", FileMode.Create, FileAccess.Write);
                    serializerObj.Serialize(stream, Conversation);
                }
                finally
                {
                    stream?.Close();
                }
            }
            catch (Exception e)
            {
                ShowDebugMessage("Exception while writing file... " + e.Message, 30);
                // We never want to throw an exception
            }
        }

        private void ShowDebugMessage(string message, int seconds)
        {
            Text = message;
            WhenToStopDisplaying = DateTime.Now.AddSeconds(seconds);
        }
    }
}
