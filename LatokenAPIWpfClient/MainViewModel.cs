using Latoken.Api.Client.Library.Utils.Configuration;
using Latoken.Api.Client.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static LatokenAPIWpfClient.MainWindow;
using System.Windows.Data;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LatokenAPIWpfClient
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.LoadKeys();
            walletCollectionViewSource.Source = WalletBalances;
            WalletCollectionView = walletCollectionViewSource.View;

            spotCollectionViewSource.Source = SpotBalances;
            SpotCollectionView = spotCollectionViewSource.View;

            this.IsHideWalletZeroBalances = true;
            this.isHideSpotZeroBalances = true;
            this.FetchTokenBalancesCommand = new RelayCommand((obj) => true, (obj) => this.ExecuteFetchTokenBalancesCommand(obj));
            this.RefreshRatesCommand = new RelayCommand((obj) => true, (obj) => this.ExecuteRefreshRatesCommand(obj));
            this.RowRefreshClickedCommand = new RelayCommand((obj) => true, (obj) => this.ExecuteRowRefreshClickedCommand(obj));
            this.SaveKeysCommand = new RelayCommand((obj) => true, (obj) => this.ExecuteSaveKeysCommand(obj));
            this.AddProfileCommand = new RelayCommand((obj) => true, (obj) => this.ExecuteAddProfileCommand(obj));
            //this.IsBusy = false;
        }

        //public bool IsBusy
        //{
        //    get => isBusy;
        //    set
        //    {
        //        isBusy = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        public ICollectionView WalletCollectionView
        {
            get => walletCollectionView;
            set
            {
                walletCollectionView = value;
                this.OnPropertyChanged();
            }
        }

        public ICollectionView SpotCollectionView
        {
            get => spotCollectionView;
            set
            {
                spotCollectionView = value;
                this.OnPropertyChanged();
            }
        }

        private CollectionViewSource walletCollectionViewSource = new CollectionViewSource();
        private CollectionViewSource spotCollectionViewSource = new CollectionViewSource();

        public List<TokenBalance> WalletBalances { get; set; } = new List<TokenBalance>();
        public List<TokenBalance> SpotBalances { get; set; } = new List<TokenBalance>();

        public ICommand FetchTokenBalancesCommand { get; set; }
        public ICommand RefreshRatesCommand { get; set; }
        public ICommand RowRefreshClickedCommand { get; set; }
        public ICommand SaveKeysCommand { get; set; }
        public ICommand AddProfileCommand { get; set; }

        public bool IsHideWalletZeroBalances
        {
            get => isHideWalletZeroBalances; set
            {
                isHideWalletZeroBalances = value;
                this.ApplyBalancesFilter(BalanceTypes.Wallet);
                this.OnPropertyChanged();
            }
        }

        public bool IsHideSpotZeroBalances
        {
            get => isHideSpotZeroBalances; set
            {
                isHideSpotZeroBalances = value;
                this.ApplyBalancesFilter(BalanceTypes.Spot);
                this.OnPropertyChanged();
            }
        }

        public DateTime? LastRefreshedWalletOn
        {
            get => lastRefreshedWalletOn;
            set
            {
                lastRefreshedWalletOn = value;
                this.OnPropertyChanged();
            }
        }

        public DateTime? LastRefreshedSpotOn
        {
            get => lastRefreshedSpotOn;
            set
            {
                lastRefreshedSpotOn = value;
                this.OnPropertyChanged();
            }
        }

        public UserProfileCollection UserProfiles { get => userProfiles; private set => SetProperty(ref userProfiles, value); }

        public BindingList<string> ProfileNames { get => profileNames; set => SetProperty(ref profileNames, value); }

        public string PublicKey
        {
            get => publicKey;
            set
            {
                //Invalidate the client instance so that new instance is created next time.
                this.latokenRestClient = null;
                SetProperty(ref publicKey, value);
            }
        }

        public string PrivateKey
        {
            get => privateKey;
            set
            {
                //Invalidate the client instance so that new instance is created next time.
                this.latokenRestClient = null;
                SetProperty(ref privateKey, value);
            }
        }
        public bool IsFirstLaunch { get; private set; }
        public string SelectedProfileName
        {
            get => selectedProfileName;
            set
            {
                this.SelectedProfileChanged(value);
                SetProperty(ref selectedProfileName, value);
            }
        }

        private LARestClient latokenRestClient = null;
        private ICollectionView walletCollectionView;
        private ICollectionView spotCollectionView;
        private bool isHideWalletZeroBalances;
        private bool isHideSpotZeroBalances;
        private Currency baseCurrency;
        private List<Balance> balances;
        private List<Currency> currencies;
        private DateTime? lastRefreshedWalletOn;
        private DateTime? lastRefreshedSpotOn;
        private string privateKey;
        private string publicKey;
        BindingList<string> profileNames;
        private UserProfileCollection userProfiles;
        private string selectedProfileName;

        //private bool isBusy;

        public void ExecuteFetchTokenBalancesCommand(object param)
        {
            try
            {
                this.GetBalances();
                this.ApplyBalancesFilter(BalanceTypes.Wallet);
                this.ApplyBalancesFilter(BalanceTypes.Spot);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExecuteRefreshRatesCommand(object param)
        {
            try
            {
                BalanceTypes balanceType = (BalanceTypes)param;
                if (balanceType == BalanceTypes.Wallet)
                {
                    this.UpdateRates(balanceType);
                    this.ApplyBalancesFilter(BalanceTypes.Wallet);
                    this.LastRefreshedWalletOn = DateTime.Now;
                }
                else if (balanceType == BalanceTypes.Spot)
                {
                    this.UpdateRates(balanceType);
                    this.ApplyBalancesFilter(BalanceTypes.Spot);
                    this.LastRefreshedSpotOn = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExecuteRowRefreshClickedCommand(object param)
        {
            try
            {
                var tokenBalance = param as TokenBalance;
                if (tokenBalance != null)
                {
                    this.RefreshRow(tokenBalance);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void GetBalances()
        {
            //this.IsBusy = true;
            if (this.latokenRestClient == null)
            {
                this.latokenRestClient = GetLaTokenRestClient();
            }

            WalletBalances.Clear();
            SpotBalances.Clear();

            this.balances = this.latokenRestClient.GetBalances().Result.ToList();
            this.currencies = this.latokenRestClient.GetCurrencies().Result.ToList();
            Dictionary<string, Currency> currencyMap = new Dictionary<string, Currency>();
            currencies.ForEach(currency => { currencyMap[currency.Id] = currency; });
            this.baseCurrency = currencies?.FirstOrDefault(c => c.Tag == "USDT");//latokenRestClient.GetCurrency("0c3a106d-bde3-4c13-a26e-3fd2394529e5").Result;

            foreach (var balance in balances)
            {
                if (!currencyMap.ContainsKey(balance.CurrencyId))
                {
                    System.Diagnostics.Debug.WriteLine($"Unknown currency with Id '{balance.CurrencyId}' in {balance.Type.Replace("ACCOUNT_TYPE_", "")} AvailableBalance = {balance.Available} BlockedBalance = {balance.Blocked} ");
                    if (balance.Available > 0)
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                }

                var currency = currencyMap[balance.CurrencyId];
                //decimal? availableBaseCurrencyValue = null;
                //if (balance.Available > 0 && baseCurrency != null)
                //{
                //    var rate = latokenRestClient.GetRate(this.baseCurrency.Id, balance.CurrencyId);
                //    if (rate.Result.Value > 0)
                //    {
                //        availableBaseCurrencyValue = balance.Available / rate.Result.Value;
                //    }
                //    else
                //    {
                //        availableBaseCurrencyValue = null;
                //        System.Diagnostics.Debug.WriteLine($"Received currency rate as '{rate.Result.Value}' for {currency.Tag}-{baseCurrency.Tag}");
                //    }
                //}
                if (balance.Type == "ACCOUNT_TYPE_WALLET")
                {
                    var tokenBalance = new TokenBalance
                    {
                        Balance = balance,
                        Token = currency,
                        AvailableCurrencyValueInBase = null,
                        LastRefreshedOn = null,
                        RefreshColumnText = "Refresh",
                    };
                    WalletBalances.Add(tokenBalance);
                }
                else
                {
                    SpotBalances.Add(new TokenBalance
                    {
                        Balance = balance,
                        Token = currency,
                        AvailableCurrencyValueInBase = null,
                        LastRefreshedOn = null,
                        RefreshColumnText = "Refresh",
                    }); ;
                }
            }

            LastRefreshedWalletOn = DateTime.Now;
            LastRefreshedSpotOn = null;
            //this.IsBusy = false;
        }

        public void UpdateRates(BalanceTypes balanceType)
        {
            //this.IsBusy = true;
            if (this.latokenRestClient == null)
            {
                this.latokenRestClient = GetLaTokenRestClient();
            }

            if (this.baseCurrency == null)
            {
                this.baseCurrency = currencies?.FirstOrDefault(c => c.Tag == "USDT");
            }

            List<TokenBalance> balances = new List<TokenBalance>();
            if (balanceType == BalanceTypes.Wallet)
            {
                balances = WalletBalances;
            }

            if (balanceType == BalanceTypes.Spot)
            {
                balances = SpotBalances;
            }

            foreach (TokenBalance tokenBalance in balances)
            {
                this.RefreshRow(tokenBalance);
            }
            //this.IsBusy = false;
        }

        private void RefreshRow(TokenBalance tokenBalance)
        {
            if (tokenBalance != null)
            {
                if (this.latokenRestClient == null)
                {
                    this.latokenRestClient = GetLaTokenRestClient();
                }

                tokenBalance.RefreshColumnText = "Refreshing...";
                decimal? availableBaseCurrencyValue = null;
                if (tokenBalance.Balance.Available > 0 && baseCurrency != null)
                {
                    var rate = this.latokenRestClient.GetRate(this.baseCurrency.Id, tokenBalance.Balance.CurrencyId);
                    if (rate.Result.Value > 0)
                    {
                        availableBaseCurrencyValue = tokenBalance.Balance.Available / rate.Result.Value;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Received currency rate as '{rate.Result.Value}' for {tokenBalance.Token.Tag}-{baseCurrency.Tag}");
                    }
                }

                tokenBalance.AvailableCurrencyValueInBase = availableBaseCurrencyValue;
                tokenBalance.LastRefreshedOn = DateTime.Now;
                tokenBalance.RefreshColumnText = "Refresh";
            }
        }

        private void ApplyBalancesFilter(BalanceTypes balaceType)
        {
            if (balaceType == BalanceTypes.Wallet)
            {
                if (this.IsHideWalletZeroBalances)
                {
                    var yourCostumFilter = new Predicate<object>(item => ((TokenBalance)item).Balance.Available > 0);
                    WalletCollectionView.Filter = yourCostumFilter;
                }
                else
                {
                    WalletCollectionView.Filter = null;
                }
            }
            else if (balaceType == BalanceTypes.Spot)
            {
                if (this.IsHideSpotZeroBalances)
                {
                    var yourCostumFilter = new Predicate<object>(item => ((TokenBalance)item).Balance.Available > 0);
                    SpotCollectionView.Filter = yourCostumFilter;
                }
                else
                {
                    SpotCollectionView.Filter = null;
                }
            }
        }

        private LARestClient GetLaTokenRestClient()
        {
            //Generate your public and private API keys on this page https://latoken.com/account/apikeys
            ClientCredentials credentials = new ClientCredentials
            {
                ApiKey = this.PublicKey, //"Your Public API Key",
                ApiSecret = this.PrivateKey, //"Your Private API Key"
            };

            var latokenRestClient = new LARestClient(credentials, new HttpClient() { BaseAddress = new Uri("https://api.latoken.com") });

            return latokenRestClient;
        }

        private void ExecuteAddProfileCommand(object param)
        {
            string inputRead = new InputBox("Enter the name of the profile", "Add User Profile").ShowDialog();
            if (!string.IsNullOrWhiteSpace(inputRead))
            {
                this.ProfileNames.Add(inputRead);
                this.SelectedProfileName = inputRead;
            }
        }

        private void ExecuteSaveKeysCommand(object param)
        {
            try
            {
                if (AppSettings.Default.UserProfiles == null)
                {
                    AppSettings.Default.UserProfiles = new UserProfileCollection();
                }

                var existingProfile = AppSettings.Default.UserProfiles.UserProfiles.FirstOrDefault(profile => profile.ProfileName == this.SelectedProfileName); ;
                if (existingProfile == null && !string.IsNullOrWhiteSpace(this.PublicKey) && !string.IsNullOrWhiteSpace(this.PrivateKey))
                {
                    existingProfile = new UserProfile
                    {
                        ProfileName = this.SelectedProfileName,
                        ApiKey = this.PublicKey,
                        ApiSecret = this.PrivateKey,
                    };
                    AppSettings.Default.UserProfiles.UserProfiles.Add(existingProfile);
                }

                else if (existingProfile != null)
                {
                    existingProfile.ApiKey = this.PublicKey;
                    existingProfile.ApiSecret = this.PrivateKey;
                    AppSettings.Default.SelectedProfileName = existingProfile.ProfileName;
                }


                //AppSettings.Default.ApiKey = this.PublicKey;
                //AppSettings.Default.ApiSecret = this.PrivateKey;
                if (existingProfile == null)
                {
                    this.IsFirstLaunch = true;
                }
                else
                {
                    this.IsFirstLaunch = false;
                }

                AppSettings.Default.IsFirstLaunch = this.IsFirstLaunch;

                this.latokenRestClient = this.GetLaTokenRestClient();

                if (this.latokenRestClient != null && !string.IsNullOrEmpty(this.latokenRestClient.GetUser().Result.Id))
                {
                    AppSettings.Default.Save();
                }
                else
                {
                    //Invalidate the client instance so that new instance is created next time.
                    this.latokenRestClient = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SelectedProfileChanged(string selectedProfileName)
        {
            var existingProfile = AppSettings.Default.UserProfiles.UserProfiles.FirstOrDefault(profile => profile.ProfileName == selectedProfileName);
            if (existingProfile != null)
            {
                //Invalidate the client instance so that new instance is created next time.
                this.latokenRestClient = null;

                this.PublicKey = existingProfile.ApiKey;
                this.PrivateKey = existingProfile.ApiSecret;
            }
            else
            {
                //Invalidate the client instance so that new instance is created next time.
                this.latokenRestClient = null;

                this.PublicKey = string.Empty;
                this.PrivateKey = string.Empty;
            }
        }

        private void LoadKeys()
        {
            try
            {
                if (AppSettings.Default.UserProfiles == null)
                {
                    AppSettings.Default.UserProfiles = new UserProfileCollection();
                }
                this.UserProfiles = AppSettings.Default.UserProfiles;

                if (this.ProfileNames == null)
                {
                    this.ProfileNames = new BindingList<string>();
                }

                this.userProfiles.UserProfiles.Select(p => p.ProfileName).ToList()
                    .ForEach(profile => this.ProfileNames.Add(profile));

                this.SelectedProfileName = AppSettings.Default.SelectedProfileName;
                this.IsFirstLaunch = AppSettings.Default.IsFirstLaunch;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        #endregion
    }
}
