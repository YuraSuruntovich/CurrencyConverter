using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using System.Runtime;
using System.Runtime.InteropServices;
using CurrencyConverter.Models;
using Windows.Storage;
using System.Threading.Tasks;


// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CurrencyConverter
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        const string fileName = "FavoriteReservedFile.txt";
        List<Currency> AllCurrency { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            AllCurrency = new List<Currency>
            {
                new Currency { Cur_Id=191, Cur_Name="Болгарский лев", Cur_Abbreviation="BGN", ImagePath="Assets/Favorite/BGN.jpg"},
                new Currency { Cur_Id=170, Cur_Name="Австралийский доллар", Cur_Abbreviation="AUD", ImagePath="Assets/Favorite/AUD.jpg"},
                new Currency { Cur_Id=291, Cur_Name="Датских крон", Cur_Abbreviation="DKK", ImagePath="Assets/Favorite/DKK.jpg"},
                new Currency { Cur_Id=143, Cur_Name="Фунт стерлингов", Cur_Abbreviation="GBP", ImagePath="Assets/Favorite/GBP.jpg"},
                new Currency { Cur_Id=130, Cur_Name="Швейцарский франк", Cur_Abbreviation="CHF", ImagePath="Assets/Favorite/CHF.jpg"},
                new Currency { Cur_Id=295, Cur_Name="Японская Йена", Cur_Abbreviation="JPY", ImagePath="Assets/Favorite/JPY.jpg"},
                new Currency { Cur_Id=23, Cur_Name="Канадский доллар", Cur_Abbreviation="CAD", ImagePath="Assets/Favorite/CAD.jpg"},
                new Currency { Cur_Id=306, Cur_Name="Шведский крон", Cur_Abbreviation="SEK", ImagePath="Assets/Favorite/SEK.jpg"}
            };

            for(int i=0; i<AllCurrency.Count; i++)
                elemList.ItemsSource = AllCurrency.ToList();
        }

        int[] CurID = { 145, 292, 290, 298, 293 };
        double USD;
        double EUR;
        double UAH;
        double RUB;
        double PLN;
        double FVT;
        int FVT_Scale;
        int FVT_Id;

        private bool IsConnect()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }
        private async void GetDataInternet()
        {
            DateRefresh.Text = "Загрузка...";
            DateTime thisDay = DateTime.Today;
            string d = thisDay.ToString("yyyy") + "-" + thisDay.ToString("MM") + "-" + thisDay.ToString("dd");

            var client = new HttpClient();            

            for(int i=0; i<CurID.Length;i++)
            {
                var resVal = await client.GetStringAsync(new Uri("http://www.nbrb.by/API/ExRates/Rates/" + CurID[i] + "?onDate=" + d + "&Periodicity=0"));
                dynamic y = Newtonsoft.Json.JsonConvert.DeserializeObject(resVal);

                switch(i)
                {
                    case 0: USD = y.Cur_OfficialRate;
                        break;
                    case 1: EUR = y.Cur_OfficialRate;
                        break;
                    case 2: UAH = y.Cur_OfficialRate;
                        break;
                    case 3: RUB = y.Cur_OfficialRate;
                        break;
                    case 4: PLN = y.Cur_OfficialRate;
                        break;
                    default:
                        break;
                }
                            
            }
            
           DateRefresh.Text = "Обновлено: " + thisDay.ToString("dd/MM/yyyy");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsConnect())
            {
                GetDataInternet();
                GetSavedFavorite();

                //FavoriteResult
                var client = new HttpClient();
                DateTime thisDay = DateTime.Today;
                string d = thisDay.ToString("yyyy") + "-" + thisDay.ToString("MM") + "-" + thisDay.ToString("dd");

                var resVal = await client.GetStringAsync(new Uri("http://www.nbrb.by/API/ExRates/Rates/" + FVT_Id + "?onDate=" + d + "&Periodicity=0"));
                dynamic y = Newtonsoft.Json.JsonConvert.DeserializeObject(resVal);

                FVT = y.Cur_OfficialRate;
                FVT_Scale = y.Cur_Scale;
                //***********************************************************
                FavoriteChamge();
                FillFavoriteDataToXML(AllCurrency);
            }
            else
                DateRefresh.Text = "Нет соединения с интернетом!";
        }
        public void FillFavoriteDataToXML(List<Currency> ls)
        {
            foreach (Currency c in ls)
            {
                if (c.Cur_Id == FVT_Id)
                {
                    favorite_text.Text = c.Cur_Abbreviation;
                    inputFavorite.PlaceholderText = c.Cur_Abbreviation;

                    Image image = new Image();
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                    bitmap.UriSource = new Uri("ms-appx:///" + c.ImagePath);
                    image.Source = bitmap;
                    favorite_img.Source = image.Source;

                    break;
                }
                else
                    continue;
            }

        }
      
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnect())
            {
                GetDataInternet();
                GetSavedFavorite();

                var client = new HttpClient();
                DateTime thisDay = DateTime.Today;
                string d = thisDay.ToString("yyyy") + "-" + thisDay.ToString("MM") + "-" + thisDay.ToString("dd");

                var resVal = await client.GetStringAsync(new Uri("http://www.nbrb.by/API/ExRates/Rates/" + FVT_Id + "?onDate=" + d + "&Periodicity=0"));
                dynamic y = Newtonsoft.Json.JsonConvert.DeserializeObject(resVal);

                FVT = y.Cur_OfficialRate;
                FVT_Scale = y.Cur_Scale;
                //----------------------------
                FillFavoriteDataToXML(AllCurrency);
            }
            else
                DateRefresh.Text = "Нет соединения с интернетом!";

        }


        private async void elemList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (IsConnect())
            {
                Currency selectedCurrency = (Currency)e.ClickedItem;
                favorite_text.Text = selectedCurrency.Cur_Abbreviation;
                inputFavorite.PlaceholderText = selectedCurrency.Cur_Abbreviation;

                Image image = new Image();
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                bitmap.UriSource = new Uri("ms-appx:///" + selectedCurrency.ImagePath);
                image.Source = bitmap;
                favorite_img.Source = image.Source;

                FavoriteResult(selectedCurrency.Cur_Id);
                FavoriteChamge();

                if (USD == 0 || EUR == 0 || UAH == 0 || RUB == 0 || PLN == 0)
                    GetDataInternet();

                //Запись в файл
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                StorageFile FavFile = await localFolder.GetFileAsync(fileName);
                // запись в файл
                await FileIO.WriteTextAsync(FavFile, selectedCurrency.Cur_Id.ToString());

                FVT_Id = selectedCurrency.Cur_Id;
                dialogBox.Hide();
            }
            else
            {
                await new Windows.UI.Popups.MessageDialog("* Убедитесь, что не включен режим 'в самолете'.\n" + "* Проверьте, включен ли беспроводной коммутатор.", "Вы не подключены к Интернету").ShowAsync();
            }

        }

        public async void FavoriteResult(int cur_id)
        {
            var client = new HttpClient();
            DateTime thisDay = DateTime.Today;
            string d = thisDay.ToString("yyyy") + "-" + thisDay.ToString("MM") + "-" + thisDay.ToString("dd");

            var resVal = await client.GetStringAsync(new Uri("http://www.nbrb.by/API/ExRates/Rates/" + cur_id + "?onDate=" + d + "&Periodicity=0"));
            dynamic y = Newtonsoft.Json.JsonConvert.DeserializeObject(resVal);

            FVT = y.Cur_OfficialRate;
            FVT_Scale = y.Cur_Scale;
            FavoriteChamge();
        }
       

        public async void GetSavedFavorite()
        {
            int intDataFromFile;
            int idWriteInFile = 170;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        
            try
            {
                StorageFile fileFavorite = await localFolder.GetFileAsync(fileName);
                string dataFromFile = await FileIO.ReadTextAsync(fileFavorite);
                if (int.TryParse(dataFromFile, out intDataFromFile))
                {
                    FVT_Id = intDataFromFile;
                }
                else
                    await new Windows.UI.Popups.MessageDialog("Ошибка чтения файла.", "Ошибка").ShowAsync();
            }
            catch
            {
                FVT_Id = idWriteInFile;
                await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                StorageFile fileFavorite = await localFolder.GetFileAsync(fileName);
                await FileIO.WriteTextAsync(fileFavorite, idWriteInFile.ToString());//запись в файл

                GetSavedFavorite();
                //await new Windows.UI.Popups.MessageDialog("Файл не существует", "Ок").ShowAsync();
                //if (IsConnect()) //попробовать try catch вместо if-elso
                //{
                //    if(FVT_Id != 0)
                //    {
                //        await new Windows.UI.Popups.MessageDialog("Вызов FavoriteResult. FVT_ID != 0", "Ok").ShowAsync();
                //        FavoriteResult(FVT_Id);
                //    }
                //    else
                //        await new Windows.UI.Popups.MessageDialog("FVT_Id = 0", "Ошибка").ShowAsync();
                //    FillFavoriteDataToXML(AllCurrency);
                //}
                //else
                //    DateRefresh.Text = "Нет соединения с интернетом!";
            }
            #region comments
            //if (File.Exists(fileName))
            //{
            //    StorageFile fileFavorite = await localFolder.GetFileAsync(fileName);
            //    string dataFromFile = await FileIO.ReadTextAsync(fileFavorite);
            //    if (int.TryParse(dataFromFile, out intDataFromFile))
            //    {
            //        FVT_Id = intDataFromFile;
            //    }
            //    else
            //        await new Windows.UI.Popups.MessageDialog("Ошибка чтения файла", "Ошибка").ShowAsync();

            //}
            //else
            //{
            //    await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            //    StorageFile fileFavorite = await localFolder.GetFileAsync(fileName);
            //    await FileIO.WriteTextAsync(fileFavorite, idWriteInFile.ToString());//запись в файл
            //    FVT_Id = idWriteInFile;
            //}
            #endregion
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            dialogBox.ShowAt((AppBarButton)sender);
        }

        private void favorite_text_Tapped(object sender, TappedRoutedEventArgs e)
        {
            dialogBox.ShowAt((TextBlock)sender);
        }

        private void CancelDialogButton_Click(object sender, RoutedEventArgs e)
        {
            dialogBox.Hide();
        }
        #region Inputs
        private void inputBYN_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputBYN.Text;

            if (Double.TryParse(value, out byn))
            {
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputUAH.Text = Convert.ToString(uah);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputUAH.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputBYR_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputBYR.Text;

            if (Double.TryParse(value, out byr))
            {
                byn = byr / 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYN.Text = Convert.ToString(byn);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputUAH.Text = Convert.ToString(uah);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputUAH.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputUSD_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputUSD.Text;

            if (Double.TryParse(value, out usd))
            {
                byn = Math.Round(usd * USD, 2);
                byr = byn * 10000;
                eur = Math.Round(byn / EUR, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputEUR.Text = Convert.ToString(eur);
                inputUAH.Text = Convert.ToString(uah);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputEUR.Text = "";
                inputUAH.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputEUR_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputEUR.Text;

            if (Double.TryParse(value, out eur))
            {
                byn = Math.Round(eur * EUR, 2);
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputUAH.Text = Convert.ToString(uah);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputUAH.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputUAH_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputUAH.Text;

            if (Double.TryParse(value, out uah))
            {
                byn = Math.Round(uah * UAH / 100, 2);
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputRUB_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputRUB.Text;

            if (Double.TryParse(value, out rub))
            {
                byn = Math.Round(rub * RUB / 100, 2);
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);

                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputUAH.Text = Convert.ToString(uah);
                inputPLN.Text = Convert.ToString(pln);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputUAH.Text = "";
                inputPLN.Text = "";
                inputFavorite.Text = "";
            }
        }

        private void inputPLN_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputPLN.Text;

            if (Double.TryParse(value, out pln))
            {
                byn = Math.Round(pln * PLN / 10, 2);
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                uah = Math.Round(byn / UAH * 100, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                fvt = Math.Round(byn / FVT * FVT_Scale, 2);


                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputUAH.Text = Convert.ToString(uah);
                inputRUB.Text = Convert.ToString(rub);
                inputFavorite.Text = Convert.ToString(fvt);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputUAH.Text = "";
                inputRUB.Text = "";
                inputFavorite.Text = "";
            }

        }

        public void FavoriteChamge()
        {
            string value;
            double byn;
            double byr;
            double usd;
            double eur;
            double uah;
            double rub;
            double pln;
            double fvt;

            value = inputFavorite.Text;

            if (Double.TryParse(value, out fvt))
            {
                byn = Math.Round(fvt * FVT / FVT_Scale, 2);
                byr = byn * 10000;
                usd = Math.Round(byn / USD, 2);
                eur = Math.Round(byn / EUR, 2);
                rub = Math.Round(byn / RUB * 100, 2);
                pln = Math.Round(byn / PLN * 10, 2);
                uah = Math.Round(byn / UAH * 100, 2);


                inputBYN.Text = Convert.ToString(byn);
                inputBYR.Text = Convert.ToString(byr);
                inputUSD.Text = Convert.ToString(usd);
                inputEUR.Text = Convert.ToString(eur);
                inputRUB.Text = Convert.ToString(rub);
                inputPLN.Text = Convert.ToString(pln);
                inputUAH.Text = Convert.ToString(uah);
            }
            else
            {
                inputBYN.Text = "";
                inputBYR.Text = "";
                inputUSD.Text = "";
                inputEUR.Text = "";
                inputRUB.Text = "";
                inputPLN.Text = "";
                inputUAH.Text = "";
            }
        }

        private void inputFavorite_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            FavoriteChamge();
        }
        #endregion
    }
}
