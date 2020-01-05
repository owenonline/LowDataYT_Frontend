using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lowdataYT_APP.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerPage : ContentPage
    {
        static HttpClient client = new HttpClient();

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
                resultbutton.Clicked += (sender, e) => Result_Button_Clicked(sender, e, x.vidid);
                resultbutton.Text = x.title;
                stack.Children.Add(resultbutton);
            }
        }
        private void Result_Button_Clicked(object sender, EventArgs e, String id)
        {
        }
    }
}