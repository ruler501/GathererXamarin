﻿using Acr.UserDialogs;
using MtSparked.FilePicker;
using MtSparked.Models;
using MtSparked.Services;
using MtSparked.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static MtSparked.ViewModels.DeckViewModel;

namespace MtSparked.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeckPage : ContentPage
	{
        Deck Deck;
        DeckViewModel viewModel;
        bool Active = false;
        IUserDialogs Dialogs { get; set; }

        public DeckPage()
            : this(ConfigurationManager.ActiveDeck)
        { }

        public DeckPage (Deck deck, bool active = true)
		{
            InitializeComponent ();

            this.Dialogs = UserDialogs.Instance;

            this.Active = active;
            if (active)
            {
                Deck = ConfigurationManager.ActiveDeck = deck;
            }
            else
            {
                Deck = deck;
                this.ManageToolbarItem.Text = "Save As";
            }

            this.BindingContext = viewModel = new DeckViewModel(Deck);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem is null)
            {
                return;
            }
           // Manually deselect item.
           ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem is CardWithBoard cwb)
            {
                Board board = viewModel.BoardByName[cwb.Board];
                int index = board.IndexOf(cwb);
                List<Card> cards = board.Select(icwb => Deck.Boards[icwb.Board][icwb.Id].Card).ToList();

                await Navigation.PushAsync(new CardCarousel(cards, index));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Strangeness is happeneing");
            }
        }

        async void ManageDeck(object sender, EventArgs args)
        {
            if (!Active)
            {
                this.Deck.SaveDeckAs();
                return;
            }
            const string NEW_DECK = "New Deck";
            const string NAME_DECK = "Name Deck";
            const string OPEN_DECK = "Open Deck";
            const string SAVE_DECK_AS = "Save Deck As";
            // const string SHARE_DECK = "Share Deck";
            const string IMPORT_FROM_DEC = "Import from .dec";
            const string EXPORT_TO_DEC = "Export to .dec";
            // const string SHARE_AS_DEC = "Share as .dec(Unsupported)";
            const string MANAGE_BOARDS = "Manage Visible Boards";
            const string ADD_BOARD = "Add Board";
            const string REMOVE_BOARD_PREFIX = "Remove Board: ";
            List<string> actions = new List<string>()
            {
                NAME_DECK, OPEN_DECK, SAVE_DECK_AS, IMPORT_FROM_DEC, EXPORT_TO_DEC, MANAGE_BOARDS, ADD_BOARD
            };
            foreach(string name in Deck.BoardNames.Where(n => n != Deck.MASTER))
            {
                actions.Add(REMOVE_BOARD_PREFIX + name);
            }
            string action = await this.Dialogs.ActionSheetAsync("Manage Deck", "Cancel", NEW_DECK, null, actions.ToArray());
            // string action = await DisplayActionSheet("Manage Deck", "Cancel", NEW_DECK, actions.ToArray());

            if(action == NEW_DECK)
            {
                this.Deck = ConfigurationManager.ActiveDeck = new Deck();
                this.BindingContext = this.viewModel = new DeckViewModel(this.Deck);
            }
            else if(action == NAME_DECK)
            {
                PromptResult name = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Deck Name").SetCancellable(true));
                if (name.Ok)
                {
                    this.Deck.Name = name.Text;
                }
            }
            else if(action == OPEN_DECK)
            {
                FileData fileData = await ConfigurationManager.FilePicker.OpenFileAs();
                if(fileData is null)
                {
                    await DisplayAlert("Error", "Failed to open Deck File", "Okay");
                }
                else
                {
                    if (fileData.FilePath != this.Deck.StoragePath)
                    {
                        string toRelease = this.Deck.StoragePath;
                        ConfigurationManager.ActiveDeck = this.Deck = Deck.FromJdec(fileData.FilePath);
                        this.BindingContext = viewModel = new DeckViewModel(this.Deck);
                        ConfigurationManager.FilePicker.ReleaseFile(toRelease);
                    }
                }
            }
            else if(action == SAVE_DECK_AS)
            {
                this.Deck.SaveDeckAs();
            }
            else if(action == IMPORT_FROM_DEC)
            {
                FileData fileData = await ConfigurationManager.FilePicker.OpenFileAs();
                if (fileData is null)
                {
                    await DisplayAlert("Error", "Failed to import .dec File", "Okay");
                }
                else
                {
                    if (fileData.FilePath != this.Deck.StoragePath)
                    {
                        string toRelease = this.Deck.StoragePath;
                        ConfigurationManager.ActiveDeck = this.Deck = Deck.FromDec(fileData.FilePath);
                        this.BindingContext = viewModel = new DeckViewModel(this.Deck);
                        ConfigurationManager.FilePicker.ReleaseFile(toRelease);
                    }
                }
            }
            else if(action == EXPORT_TO_DEC)
            {
                this.Deck.SaveAsDec();
            }
            else if (action == MANAGE_BOARDS)
            {
                await Navigation.PushAsync(new BoardEditing(this.Deck));
            }
            else if(action == ADD_BOARD)
            {
                PromptResult result = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Board Name"));
                if (result.Ok)
                {
                    this.Deck.AddBoard(result.Text);
                }
            }
            else if (action.StartsWith(REMOVE_BOARD_PREFIX))
            {
                string name = action.Substring(REMOVE_BOARD_PREFIX.Length);
                this.Deck.RemoveBoard(name);
            }
        }

        public void ToggleUnique(object sender, EventArgs args)
        {
            ConfigurationManager.ShowUnique = !ConfigurationManager.ShowUnique;
        }

        public async void OpenStats(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new StatsPage(this.Deck));
        }
    }
}