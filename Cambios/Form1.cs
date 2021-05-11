using Cambios.Servicos;
using Cambios.Modelos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cambios
{
    public partial class Form1 : Form
    {
        #region Atributos

        private List<Rate> Rates;

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;


        #endregion

        #region Propriedades

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            lblResultado.Text = "A atualizar taxas....";

            var connection = networkService.checkConnection();

            if (!connection.IsSucess)
            {

                LoadLocalRates();
                load = false;

            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                lblResultado.Text = "Não há ligação á internet" +
                                        Environment.NewLine + "e não foram prévimente carregadas" +
                                        Environment.NewLine + "Tente novamente mais tarde!";

                lblStatus.Text = "Primeira inicialização deverá ter ligação á internet";

                return;

            }

            comboBoxOrigem.DataSource = Rates;
            comboBoxOrigem.DisplayMember = "Name";

            //Dividir os valores das ComboBox - corrige bug microsoft
            comboBoxDestino.BindingContext = new BindingContext();

            comboBoxDestino.DataSource = Rates;
            comboBoxDestino.DisplayMember = "Name";



            lblResultado.Text = "Taxas atualizadas...";

            if (load)
            {
                lblStatus.Text = string.Format("Taxas carregadas da internet em {0:f}", DateTime.Now);
            }
            else
            {
                lblStatus.Text = string.Format("Taxas carregadas da Base de Dados.");
            }

            progressBar1.Value = 100;

            btnConverter.Enabled = true;
            BtnTroca.Enabled = true;
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);
        }

        private void btnConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(txtValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }

            decimal valor;
            if (!decimal.TryParse(txtValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de conversão", "Valor terá que ser numérico");
                return;
            }

            if (comboBoxOrigem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", " Tem que escolher uma moeda a converter");
                return;
            }
            if (comboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", " Tem que escolher uma moeda de destino para converter");
                return;
            }

            var taxaOrigem = (Rate)comboBoxOrigem.SelectedItem;

            var taxtaDestino = (Rate)comboBoxDestino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxtaDestino.TaxRate;

            lblResultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, valor, taxtaDestino.Code, valorConvertido);
        }

        private void BtnTroca_Click(object sender, EventArgs e)
        {
            Troca();
        }

        private void Troca()
        {
            var aux = comboBoxOrigem.SelectedItem;
            comboBoxOrigem.SelectedItem = comboBoxDestino.SelectedItem;
            comboBoxDestino.SelectedItem = aux;
            Converter();
        }
    }
}
