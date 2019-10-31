using System;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DiscoElysiumReader.Mod.Utils;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using Conversation = DiscoElysiumReader.Mod.Utils.Conversation;

namespace DiscoElysiumReader.Mod
{
    class DialogReader : MonoBehaviour
    {
        private string Text = "";

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

            Text = "StartedNew";

        }
        protected void Update()
        {
            // Do nothing for now
        }

        protected void OnGUI()
        {
            if (String.IsNullOrEmpty(Text))
                return;
            //GUI.Label(new Rect(10, 10, 100, 20), "Is this working yet lol");
            //UIHelper.Begin($"Testing Text: {Text} Thing: {UniqueDialogEntry} .", 1,1,1000,100,5,100,100);
        }

        private static long UniqueDialogEntry = 0;
        public static long DialogReaderAlwaysChanging()
        {
            return UniqueDialogEntry++;
        }

        private void DiscoReaderHandleNewDialogueEntry(LuaWatchItem luaWatchItem, Lua.Result newResult)
        {


            if (DialogueManager.IsConversationActive && DialogueManager.Instance.ConversationModel.HasValidEntry)
            {
                int conversationId =
                    DialogueManager.Instance.ConversationModel.GetConversationID(DialogueManager.Instance
                        .currentConversationState);
                if (Conversation == null || Conversation.ConversationId != conversationId)
                {
                    Conversation = new Conversation(conversationId, DialogueManager.Instance.ConversationModel.ActorInfo.Name, DialogueManager.Instance.ConversationModel.ConversantInfo.Name);
                }

                Subtitle dialogueSubtitle = DialogueManager.Instance.currentConversationState.subtitle;
                Conversation.DialogueEntries.Add(new ReaderDialogueEntry(dialogueSubtitle.speakerInfo.Name, dialogueSubtitle.speakerInfo.IsPlayer, dialogueSubtitle.dialogueEntry.DialogueText, $"{dialogueSubtitle.sequence} {dialogueSubtitle.responseMenuSequence} {dialogueSubtitle.entrytag}", UniqueDialogEntry));

                
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
    }
}
