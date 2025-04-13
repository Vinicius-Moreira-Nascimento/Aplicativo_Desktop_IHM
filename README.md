# Aplicativo de Comunicação Serial com Gráficos.
## Autor: Vinicius Moreira Nascimento.

Aplicativo WPF para monitoramento de dados via porta serial com visualização gráfica em tempo real.

## Funcionalidades
- Conexão com dispositivos via porta serial
- Visualização de dados em texto e gráfico
- Controle de fluxo de dados (play/pause)
- Envio de comandos para o dispositivo
- Limpeza da tela de exibição

## Tecnologias
- C#/.NET
- WPF (Windows Presentation Foundation)
- LiveCharts (gráficos)
- SerialPort (comunicação serial)

## Como Executar
1. Clone o repositório
2. Abra o projeto no Visual Studio
3. Compile e execute (F5)

## Requisitos
- Visual Studio 2019+
- .NET Framework 4.7.2
- Dispositivo serial configurado (opcional para testes)


## 📦 Dependências do Projeto

Este projeto utiliza os seguintes pacotes NuGet que devem ser instalados:

### Instalação das Dependências

1. **LiveCharts.Wpf** (para os gráficos):
   - No Visual Studio, clique com o botão direito no projeto no Solution Explorer
   - Selecione "Manage NuGet Packages..."
   - Na aba "Browse", pesquise por "LiveCharts.Wpf"
   - Selecione a versão mais recente e clique em "Install"

2. **Porta Serial** (já incluída no .NET):
   - `System.IO.Ports` é parte do .NET Framework
   - Nenhuma instalação adicional é necessária

### Como adicionar via Console do NuGet (alternativo):
```bash
Install-Package LiveCharts.Wpf -Version 0.7.7

## Configuração
Edite no código:
- Baud rate (padrão: 115200)
- Limite de pontos no gráfico (padrão: 50)
- Intervalo de atualização (padrão: 1000ms)
