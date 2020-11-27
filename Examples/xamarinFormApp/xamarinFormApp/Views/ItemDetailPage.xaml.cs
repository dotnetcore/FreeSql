using System.ComponentModel;
using Xamarin.Forms;
using xamarinFormApp.ViewModels;

namespace xamarinFormApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}