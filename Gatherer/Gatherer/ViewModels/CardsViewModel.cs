﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using Gatherer.Models;
using Gatherer.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gatherer.ViewModels
{
    public class CardsViewModel : INotifyPropertyChanged
    {
        public CardDataStore DataStore;

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public ObservableCollection<Card> Items { get; set; }
        public int CardCount => Items.Count;
        public Command LoadItemsCommand { get; set; }

        public CardsViewModel()
        {
            Title = "Cards";
            Items = new ObservableCollection<Card>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public CardsViewModel(CardDataStore store)
        {
            Title = "Cards";
            Items = new ObservableCollection<Card>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            this.DataStore = store;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}