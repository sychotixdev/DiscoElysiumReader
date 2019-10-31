using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;
using DiscoElysiumReader.Mod.Utils;

namespace DiscoElysiumVoice
{
    public class Reader
    {
        private string FileName { get; set; }
        private TextBlock ConversationTextBox { get; }
        private SpeechSynthesizer synth { get; }
        public ObservableCollection<string> VoiceList { get; private set; }

        private FileSystemWatcher Watcher { get; set; }

        private Conversation LastConversation { get; set; }

        public Reader(TextBlock conversationTextBox)
        {
            FileName = "WriteText.txt";
            ConversationTextBox = conversationTextBox;

            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();

            initializeVoiceList();
        }

        private void initializeVoiceList()
        {
            VoiceList = new ObservableCollection<string>();

            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                VoiceList.Add(voice.VoiceInfo.Name);
            }

            MainWindow.ReaderDataModel.DefaultVoice = VoiceList.FirstOrDefault();
        }


        public void watch()
        {
            Watcher?.Dispose();

            Watcher = new FileSystemWatcher();
            Watcher.Path = MainWindow.ReaderDataModel.GamePath;
            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Filter = FileName;
            Watcher.Changed += new FileSystemEventHandler(OnChanged);
            Watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //Copies file to another directory.

            XmlSerializer SerializerObj = new XmlSerializer(typeof(Conversation));
            Stream stream = null;
            Conversation objnew = null;
            try
            {
                int i = 0;
                while (stream == null && i < 3)
                {
                    try
                    {
                        stream = new FileStream(Path.Combine(MainWindow.ReaderDataModel.GamePath, FileName), FileMode.Open, FileAccess.Read);
                        objnew = (Conversation)SerializerObj.Deserialize(stream);
                    }
                    catch (Exception exception)
                    {
                        // Do nothing, retry pattern
                        Thread.Sleep(50);
                    }
                }
            }
            finally
            {
                stream?.Close();
            }

            if (objnew == null)
                return;

            long? lastSpokenUniqueId = LastConversation?.DialogueEntries.LastOrDefault()?.UniqueMessageId;

            if (LastConversation != null && objnew.ConversationId == LastConversation.ConversationId && lastSpokenUniqueId == objnew.DialogueEntries.LastOrDefault()?.UniqueMessageId)
            {
                // We're currently generating multiple file events... ignore if we've already attempted to read this one
                return;
            }


            // Is it our empty conversation?
            if (objnew.ConversationId == -1)
                return;

            foreach (ReaderDialogueEntry dialogue in objnew.DialogueEntries.Where(x => lastSpokenUniqueId == null || x.UniqueMessageId > lastSpokenUniqueId))
            {
                if (dialogue?.Message == null)
                    return;

                ConversationTextBox?.Dispatcher.Invoke(() => ConversationTextBox.Text = $"Actor: {objnew.ActorName}\nConversant: {objnew.Conversant}\nText:{dialogue.Message}");

                string selectedVoice = MainWindow.ReaderDataModel.DefaultVoice;

                foreach (CustomVoiceConfig customVoice in MainWindow.ReaderDataModel.CustomVoices)
                {
                    if (objnew.Conversant == customVoice.ConversantName)
                    {
                        selectedVoice = customVoice.VoiceName;
                        break;
                    }
                }

                synth.SelectVoice(selectedVoice);

                synth.SpeakAsync(dialogue.Message);
            }

            


            LastConversation = objnew;

        }

        public void Dispose()
        {
            // avoiding resource leak
            Watcher.Changed -= OnChanged;
            this.Watcher.Dispose();
        }

    }
}
