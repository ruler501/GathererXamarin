﻿using Gatherer.Models;
using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardPage : ContentPage
	{
        CardsViewModel viewModel;

        public CardPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new CardsViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Card;
            if (item == null)
                return;


            // Manually deselect item.
            CardsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}