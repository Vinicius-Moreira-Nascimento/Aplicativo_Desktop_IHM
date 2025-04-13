using System.Text;
using System.Windows;
using System.IO.Ports;
using System.Timers;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;

namespace Aplicativo_Teste
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private System.Timers.Timer _readTimer; // Temporizador para limitar a taxa de amostragem
        private string _bufferedData = string.Empty; // Buffer para armazenar dados recebidos da serial
        private readonly object _lock = new object(); // Lock para acesso ao buffer
        private bool _isPaused = false; // Controla se os dados são exibidos
        private ChartValues<double> _voltageValues; // Dados do gráfico
        private int _timeCounter; // Contador de tempo


        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();

            // Inicializa o temporizador com intervalo de 1 segundo (1000 ms)
            _readTimer = new System.Timers.Timer(1000)
            {
                AutoReset = true // O temporizador reinicia automaticamente após cada intervalo
            };
            _readTimer.Elapsed += ProcessBufferedData;
            _readTimer.Start();


            // Inicializa o gráfico
            _voltageValues = new ChartValues<double>();
            VoltageChart.Series = new SeriesCollection
            {
             new LineSeries
              {
                 Title = "Tensão Total",
                 Values = _voltageValues,
                 LineSmoothness = 0, // 0 para linhas retas; 1 para suavização
                 PointGeometry = null // Remove os pontos para melhor visualização
                }
             };
            _timeCounter = 0;
        }

        private void LoadAvailablePorts()
        {
            // Lista todas as portas COM disponíveis
            PortComboBox.ItemsSource = SerialPort.GetPortNames();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortComboBox.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma porta para conectar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Configura e abre a porta serial
                _serialPort = new SerialPort(PortComboBox.SelectedItem.ToString(), 115200)
                {
                    NewLine = "\r\n" // Define o caractere de fim de linha
                };
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                MessageBox.Show("Conexão estabelecida com sucesso!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Lê os dados da porta serial e armazena no buffer
            string data = _serialPort.ReadExisting();
            lock (_lock)
            {
                _bufferedData += data;
            }
        }

        private void ProcessBufferedData(object sender, ElapsedEventArgs e)
        {
            string dataToProcess;

            // Obtém os dados do buffer
            lock (_lock)
            {
                dataToProcess = _bufferedData;
                _bufferedData = string.Empty; // Limpa o buffer
            }

            if (!string.IsNullOrEmpty(dataToProcess) && !_isPaused)
            {
                // Atualiza a interface no thread principal
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Extração do primeiro valor antes da vírgula
                        string firstValue = dataToProcess.Split('.')[0];
                        if (double.TryParse(firstValue, out double voltage))
                        {
                            _voltageValues.Add(voltage); // Adiciona ao gráfico
                            if (_voltageValues.Count > 50) // Limite de pontos no gráfico
                                _voltageValues.RemoveAt(0); // Remove os mais antigos

                            _timeCounter++;
                        }

                        OutputTextBox.AppendText(dataToProcess + Environment.NewLine);
                        OutputTextBox.ScrollToEnd(); // Rola automaticamente para o final
                    }
                    catch (Exception ex)
                    {
                        OutputTextBox.AppendText($"Erro ao processar dados: {ex.Message}" + Environment.NewLine);
                    }
                });
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                MessageBox.Show("Conecte a uma porta antes de enviar comandos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string message = InputTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                _serialPort.WriteLine(message);
                InputTextBox.Clear();
            }
        }

        //CleartButton_Click
        //OutputTextBox.Clear();

        private void CleartButton_Click(object sender, RoutedEventArgs e)
        {
           OutputTextBox.Clear();
           MessageBox.Show("Limpando tela!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                    MessageBox.Show("Conexão encerrada com sucesso!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao desconectar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Nenhuma conexão ativa para encerrar.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused; // Alterna entre play e pause
            PlayPauseButton.Content = _isPaused ? "Reproduzir" : "Pausar"; // Atualiza o texto do botão
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Fecha a porta serial, se estiver aberta
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            // Para e libera o temporizador
            if (_readTimer != null)
            {
                _readTimer.Stop();
                _readTimer.Dispose();
            }
        }
    }
}
