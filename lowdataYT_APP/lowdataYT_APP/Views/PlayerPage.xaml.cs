using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lowdataYT_APP.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerPage : ContentPage
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private HttpWebRequest webRequest;
        private VolumeWaveProvider16 volumeProvider;

        static HttpClient client = new HttpClient();

        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        public PlayerPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            HttpResponseMessage response = client.GetAsync("http://lowdatayt.ddns.net:8000/api/search/"+searchbar.Text).Result;
            List<Models.SearchResult> list = new List<Models.SearchResult>();
            var json = response.Content.ReadAsStringAsync().Result;
            list = JsonConvert.DeserializeObject<List<Models.SearchResult>>(json);
            createbuttons(list);
        }
        private void createbuttons(List<Models.SearchResult> list)
        {
            foreach (Models.SearchResult x in list)
            {
                Button resultbutton = new Button();
                resultbutton.Clicked += (sender, e) => Result_Button_Clicked(sender, e, x.vidid, x.title);
                resultbutton.Text = x.title;
                stack.Children.Add(resultbutton);
            }
        }
        private void Result_Button_Clicked(object sender, EventArgs e, String id, String title)
        {
            fullyDownloaded = false;
            webRequest = (HttpWebRequest)WebRequest.Create("http://lowdatayt.ddns.net:8000/api/search/"+id+", '"+title+"'");
            //test with definitively working mp3 stream
            //webRequest = (HttpWebRequest)WebRequest.Create("http://listen.openstream.co/3162/audio");
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException x)
            {
                if (x.Status != WebExceptionStatus.RequestCanceled)
                {
                    Console.WriteLine(x);
                }
                return;
            }
            var buffer = new byte[16384 * 4];
            IMp3FrameDecompressor decompressor = null;
            try{
                using (var responsestream = resp.GetResponseStream())
                {
                    var readFullyStream = new Services.ReadFullyStream(responsestream);
                    do
                    {
                        if (IsBufferNearlyFull)
                        {
                            Console.WriteLine("buffer getting full");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                fullyDownloaded = true;
                                break;
                            }
                            catch (WebException)
                            {
                                break;
                            }
                            if (frame == null) break;
                            if (decompressor == null)
                            {
                                decompressor = CreateFrameDecompressor(frame);
                                bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20);
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                        }
                    } while (playbackState != StreamingPlaybackState.Stopped);
                        Console.WriteLine("exiting");
                        decompressor.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }
        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }
        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }
    }
}