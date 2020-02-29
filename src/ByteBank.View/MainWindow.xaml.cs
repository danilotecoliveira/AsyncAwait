using System;
using System.Windows;
using ByteBank.Core.Service;
using ByteBank.Core.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var contas = r_Repositorio.GetContaClientes();

            var contas_parte1 = contas.Take(contas.Count() / 2);
            var contas_parte2 = contas.Skip(contas.Count() / 2);

            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            Thread thread_parte1 = new Thread(() => 
            {
                foreach (var conta in contas_parte1)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            Thread thread_parte2 = new Thread(() =>
            {
                foreach (var conta in contas_parte2)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            thread_parte1.Start();
            thread_parte2.Start();

            //thread_parte1.IsAlive;

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
