﻿using Xamarin.Forms;

namespace PulseOximeterApp
{
    public partial class App : Application
    {
        public App()
        {
            DependencyService.Register<Infrastructure.DependencyServices.IMessageService, Infrastructure.DependencyServices.MessageService>();

            InitializeComponent();
            MainPage = new MainPage();
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
