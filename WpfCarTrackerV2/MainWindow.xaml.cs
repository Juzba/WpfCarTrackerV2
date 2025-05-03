using CarsPriceXml.Components;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Xml;
using XmlCarsPriceApp.Models;

namespace CarsPriceXml;

public partial class MainWindow : Window
{
    bool _isFileOpen = false;
    List<Car> _carList = new();


    public MainWindow()
    {
        InitializeComponent();
    }


    private void MainProg()
    {

        if (_isFileOpen && _carList.Count > 0)
        {

            // "I used searching with LINQ methods with slight assistance from AI. :)

            var cars = _carList
            .GroupBy(p => p.Name)
            .Select(g =>
            new
            {
                Name = g.Key,
                TotalPrice = g.Sum(p => Functions.SumCondition(this, p) ? p.Price : 0),
                PriceWithDPH = g.Sum(p => Functions.DPHCalc(Functions.SumCondition(this, p) ? p.Price : 0, p.DPH))
            }).ToList();



            DataGridInput.ItemsSource = _carList;
            DataGridResult.ItemsSource = cars;
            Functions.AddMessage(this, "Výpočet hotov.");
        }
    }


    private void ButtonOpenXmlFile_Click(object sender, RoutedEventArgs e)
    {
        _isFileOpen = false;
        int count = 0;
        int countError = 0;
        _carList = new();
        DataGridInput.ItemsSource = null;
        DataGridResult.ItemsSource = null;

        OpenFileDialog openFD = new();
        openFD.InitialDirectory = Directory.GetCurrentDirectory();
        openFD.Filter = "XML Files(*.xml)|*.xml";

        if (openFD.ShowDialog() == true)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNodeList? carNodes = null;
            textBlockPath.Text = openFD.FileName;

            try
            {
                xmlDoc.Load(openFD.FileName);
                carNodes = xmlDoc.DocumentElement?.SelectNodes("/Data/Car");
            }
            catch (Exception)
            {
                Functions.AddMessage(this, $"XML obsahuje chyby.");
                //throw;
            }

            if (carNodes != null)
            {

                foreach (XmlNode node in carNodes)
                {
                    count++;
                    string? name = node?.SelectSingleNode("Name")?.InnerText;
                    string? date = node?.SelectSingleNode("Date")?.InnerText;
                    string? price = node?.SelectSingleNode("Price")?.InnerText;
                    string? dph = node?.SelectSingleNode("DPH")?.InnerText;

                    if (
                        name?.Length > 2
                        && DateTime.TryParse(date, out DateTime parsedDate)
                        && double.TryParse(dph, out double parsedDph)
                        && price?.Length > 3
                        && double.TryParse(price.Remove(price.Length - 2).Replace(".", ""), out double parsedPrice)
                        )
                    {
                        _carList.Add(new Car(name, parsedDate, parsedPrice, parsedDph));
                    }
                    else
                    {
                        Functions.AddMessage(this, $"Chyba zadání v Car číslo{count}!", false);
                        countError++;
                    }
                }


                if (_carList.Count - countError > 0)
                {
                    _isFileOpen = true;
                    Functions.AddMessage(this, $"Načteno {count - countError} položek z XML.", false);
                    MainProg();
                }

            }
        }
    }


    // Combo box change, start sum
    private void ComboBoxClosed(object sender, EventArgs e)
    {
        MainProg();
    }
}
