using System;
using System.Linq;
using System.Windows;
using System.Threading;
using ByteBank.Core.Model;
using ByteBank.Core.Service;
using System.Threading.Tasks;
using ByteBank.Core.Repository;
using System.Collections.Generic;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;

            _cts = new CancellationTokenSource();

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            LimparView();

            BtnCancelar.IsEnabled = true;

            try
            {
                var inicio = DateTime.Now;
                var progress = new Progress<string>(str => PgsProgresso.Value++);
                var resultado = await ConsolidarContas(contas, progress, _cts.Token);
                var fim = DateTime.Now;

                AtualizarView(resultado, fim - inicio);
            }
            catch (OperationCanceledException ex)
            {
                TxtTempo.Text = "Operação cancelada!";
            }

            BtnProcessar.IsEnabled = true;
            BtnCancelar.IsEnabled = false;
        } 

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cts.Cancel();
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> progresso, CancellationToken ct)
        {
            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultado = r_Servico.ConsolidarMovimentacao(conta, ct);
                    progresso.Report(resultado);

                    ct.ThrowIfCancellationRequested();

                    return resultado;
                }, ct)
            );

            return await Task.WhenAll(tasks);
        }

        private void AtualizarView(IEnumerable<string> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
            PgsProgresso.Value = 0;
        }
    }
}
