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

            if (VoiceList.Count == 0)
            {
                MainWindow.ShowErrorMessage("You have no voices installed, install some.");
            }

            MainWindow.ReaderDataModel.DefaultVoice = VoiceList.FirstOrDefault();
        }

        public bool StopStart()
        {
            if (Watcher == null)
            {
                return Watch();
            }
            else
            {
                StopWatching();
                return false;
            }
        }


        private bool Watch()
        {
            if (!Directory.Exists(MainWindow.ReaderDataModel.GamePath))
            {
                MainWindow.ShowErrorMessage("Specified game path does not exist!");
                return false;
            }

            if (!File.Exists(Path.Combine(MainWindow.ReaderDataModel.GamePath, "disco.exe")))
            {
                MainWindow.ShowErrorMessage("Game executable 'disco.exe' does not exist at the specified path. This may be the incorrect path.\nWe will still start watching, just in case... but you will need to change your inject script!");
                return false;
            }

            StopWatching();

            Watcher = new FileSystemWatcher();
            Watcher.Path = MainWindow.ReaderDataModel.GamePath;
            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Filter = FileName;
            Watcher.Changed += new FileSystemEventHandler(OnChanged);
            Watcher.EnableRaisingEvents = true;

            return true;
        }

        private void StopWatching()
        {
            Watcher?.Dispose();
            synth.SpeakAsyncCancelAll();
            LastConversation = null;
            Watcher = null;
        }

        public void SkipText()
        {
            synth.SpeakAsyncCancelAll();
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
                while (stream == null && i < 5)
                {
                    try
                    {
                        stream = new FileStream(Path.Combine(MainWindow.ReaderDataModel.GamePath, FileName), FileMode.Open, FileAccess.Read);
                        objnew = (Conversation)SerializerObj.Deserialize(stream);
                    }
                    catch (Exception)
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
            {
                MainWindow.ShowErrorMessage($"Failed reading {Path.Combine(MainWindow.ReaderDataModel.GamePath, FileName)}. Disco Elysium may still have a lock on it?");
                return;
            }

            long? lastSpokenUniqueId = LastConversation?.DialogueEntries.LastOrDefault()?.UniqueMessageId;

            if (LastConversation != null && objnew.ConversationId == LastConversation.ConversationId && lastSpokenUniqueId == objnew.DialogueEntries.LastOrDefault()?.UniqueMessageId)
            {
                // We're currently generating multiple file events... ignore if we've already attempted to read this one
                return;
            }


            // Is it our empty conversation?
            if (objnew.ConversationId == -1)
                return;

            IEnumerable<ReaderDialogueEntry> entriesToProcess = null;
            // If our last read is null, only read the last entry
            if (lastSpokenUniqueId == null && objnew.DialogueEntries.Count > 0)
            {
                entriesToProcess = new List<ReaderDialogueEntry>() { objnew.DialogueEntries.OrderByDescending(x => x.UniqueMessageId).First() };
            }
            else
            {
                entriesToProcess = objnew.DialogueEntries.Where(x => x.UniqueMessageId > lastSpokenUniqueId);
            }

            foreach (ReaderDialogueEntry dialogue in entriesToProcess)
            {
                // If we somehow don't have a message, skip this entry
                if (dialogue?.Message == null || String.IsNullOrWhiteSpace(dialogue.Message))
                    continue;

                // Skip the PC's entries. Less immersive
                if (dialogue.IsPlayerCharacter)
                    continue;

                ConversationTextBox?.Dispatcher?.Invoke(() => ConversationTextBox.Text = $"Actor: {objnew.ActorName}\nConversant: {objnew.Conversant}\nText:{dialogue.Message}\nTest:{dialogue.Sequence}");

                string selectedVoice = MainWindow.ReaderDataModel.DefaultVoice;

                foreach (CustomVoiceConfig customVoice in MainWindow.ReaderDataModel.CustomVoices)
                {
                    if (objnew.Conversant == customVoice.ConversantName)
                    {
                        selectedVoice = customVoice.VoiceName;
                        break;
                    }
                }

                synth.Rate = MainWindow.ReaderDataModel.SpeechRate;
                synth.Volume = MainWindow.ReaderDataModel.SpeechVolume;

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
