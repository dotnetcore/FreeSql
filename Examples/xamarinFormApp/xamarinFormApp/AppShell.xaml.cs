using System;
using System.Collections.Generic;
using Xamarin.Forms;
using xamarinFormApp.ViewModels;
using xamarinFormApp.Views;

namespace xamarinFormApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
