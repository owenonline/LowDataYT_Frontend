using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using lowdataYT_APP.Services;
using lowdataYT_APP.Views;

namespace lowdataYT_APP
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new Views.PlayerPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
