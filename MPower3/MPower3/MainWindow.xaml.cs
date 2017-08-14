using System;
using System.Collections.Generic;
using System.Linq;
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
using MahApps.Metro.Controls;
using System.Windows.Threading;
using System.IO;

namespace MPower3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MediaPlayer m; //Music player class
        private bool Bstate; //state of the play button
        DispatcherTimer timer;
        private bool isDragging = false;
        public SortedList<string, string> musicas; //chave-valor nome-localizacao
        private int lastIndexPlayed = 0;
        private bool wasHumanTouch = false;

        public MainWindow()
        {
            InitializeComponent();
            musicas = new SortedList<string, string>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += Timer_Tick;
            timer.Start();
            m = new MediaPlayer();
            Bstate = false;
            m.MediaOpened += M_MediaOpened;
        }

        private void M_MediaOpened(object sender, EventArgs e)
        {
            if(!wasHumanTouch)
            {
                lastIndexPlayed++;
            }
            else
            {
                wasHumanTouch = false;
            }
            slider.Maximum = m.NaturalDuration.TimeSpan.Ticks;
            totalDuration.Content = m.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            name.Content = System.IO.Path.GetFileName(m.Source.LocalPath).Replace(".mp3", "");
            m.Play();
            if(!Bstate)
            changeBstate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (m.Source != null)
            {
                currentDuration.Content = new TimeSpan((long)slider.Value).ToString(@"hh\:mm\:ss");
                if(!isDragging)
                slider.Value = m.Position.Ticks;
                if(slider.Value == slider.Maximum)
                {
                    m.Open(new Uri(getNextSong().Value));
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if(!Bstate)
            {
                m.Play();
            }
            else
            {
                m.Pause();
            }
            changeBstate();
        }

        private void changeBstate()
        {
            Bstate = !Bstate;
            if(Bstate)
            {
                button.Content = "❚❚";
                return;
            }
            button.Content = " ▶";
        }

        private void slider_DragCompleted(object sender, RoutedEventArgs e)
        {
            m.Position = new TimeSpan((long)slider.Value); //such an ugly hack
            isDragging = false;
        }

        private void slider_DragStarted(object sender, RoutedEventArgs e)
        {
            isDragging = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog d = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            d.Description = "Pick a folder with mp3 files to be played";
            d.ShowDialog();
            if(d.SelectedPath != null)
            {
                musicas = getFileList(d.SelectedPath);
                listBox.ItemsSource = musicas;
            }
        }

        private SortedList<string, string> getFileList(string path)
        {
            SortedList<string, string> lista = new SortedList<string, string>();
            if(path != null)
            {
                foreach(string file in Directory.GetFiles(path))
                {
                    FileInfo i = new FileInfo(file);
                    if(i.Extension == ".mp3")
                    {
                        lista.Add(i.Name.Replace(i.Extension, ""), file);
                        //listBox.Items.Add(i.Name.Replace(i.Extension, ""));
                        
                    }
                    
                }
            }
            return lista;
        }


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
            {
                string path = "";
                musicas.TryGetValue(((KeyValuePair<string, string>)listBox.SelectedValue).Key, out path);
                lastIndexPlayed = listBox.SelectedIndex;
                wasHumanTouch = true;
                m.Open(new Uri(path));
            }
        }

        private KeyValuePair<string, string> getNextSong()
        {
            if(lastIndexPlayed+1 < listBox.Items.Count)
            {
                KeyValuePair<string, string> next = (KeyValuePair<string, string>)listBox.Items.GetItemAt(lastIndexPlayed + 1);
                return next;
            }
            else
            {
                return (KeyValuePair<string, string>)listBox.Items.GetItemAt(0);
            }
            
        }
    }
}
