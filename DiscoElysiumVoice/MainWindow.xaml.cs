using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using DiscoElysiumReader.Mod.Utils;

namespace DiscoElysiumVoice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static ReaderDataModel ReaderDataModel = new ReaderDataModel();

        private Reader Reader { get; }

        public MainWindow()
        {
            InitializeComponent();

            Reader = new Reader(ConversationTextBlock);

            initVoiceLists();
        }

        private void initVoiceLists()
        {
            

            defaultVoiceComboBox.ItemsSource = Reader.VoiceList;
            defaultVoiceComboBox.SelectedItem = Reader.VoiceList.FirstOrDefault();
            customConversantSelectedVoice.ItemsSource = Reader.VoiceList;
            customConversantSelectedVoice.SelectedItem = Reader.VoiceList.FirstOrDefault();

            customConversantVoiceBox.DisplayMemberPath = "DisplayText";
            customConversantVoiceBox.SelectedValuePath = "ConversantName";

        }

        private void DoWorkButton_OnClick(object sender, RoutedEventArgs e)
        {
            
           
        }

        private void CustomVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            string customConversant = customConversantName.Text;
            string selectedVoice = (String) customConversantSelectedVoice.SelectedItem;

            ListBoxItem itm = new ListBoxItem();
            itm.Content = "some text";

            CustomVoiceConfig customVoice = new CustomVoiceConfig
            {
                ConversantName = customConversant,
                VoiceName = selectedVoice
            };

            customConversantVoiceBox.Items.Add(customVoice);
            ReaderDataModel.CustomVoices.Add(customVoice);

            customConversantName.Text = "";
            customConversantSelectedVoice.SelectedItem = null;
        }

        private void GameLocationTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ReaderDataModel.GamePath = gameLocationTextBox.Text;
        }

        private void DefaultVoiceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty((String)defaultVoiceComboBox.SelectedItem))
                ReaderDataModel.DefaultVoice = (String)defaultVoiceComboBox.SelectedItem;
        }
    }
}
