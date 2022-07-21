using Latoken.Api.Client.Library;
using System;
using System.ComponentModel;

namespace LatokenAPIWpfClient
{
    public class TokenBalance : INotifyPropertyChanged
    {
        private Balance balance;
        private Currency token;
        private decimal? availableCurrencyValueInBase;
        private DateTime? lastRefreshedOn;
        private string refreshColumnText;

        public Balance Balance
        {
            get => balance;
            set
            {
                balance = value;
                this.OnPropertyChanged(nameof(Balance));
            }
        }

        public Currency Token
        {
            get => token;
            set
            {
                token = value;
                this.OnPropertyChanged(nameof(Token));
            }
        }

        public decimal? AvailableCurrencyValueInBase
        {
            get => availableCurrencyValueInBase;
            set
            {
                availableCurrencyValueInBase = value;
                this.OnPropertyChanged(nameof(AvailableCurrencyValueInBase));
            }
        }

        public string RefreshColumnText
        {
            get => refreshColumnText;
            set
            {
                refreshColumnText = value;
                this.OnPropertyChanged(nameof(RefreshColumnText));
            }
        }

        public DateTime? LastRefreshedOn
        {
            get => lastRefreshedOn;
            set
            {
                lastRefreshedOn = value;
                this.OnPropertyChanged(nameof(LastRefreshedOn));
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string peropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(peropertyName));
        }
        #endregion
    }
}
