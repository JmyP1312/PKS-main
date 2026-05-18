using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetworkAnalyzer;

public partial class MainWindow : Window
{
    private readonly string _historyFilePath;

    public MainWindow()
    {
        InitializeComponent();

        _historyFilePath = BuildHistoryFilePath();
        LoadInterfaces();
        LoadHistory();
    }

    private void RefreshInterfacesClick(object? sender, RoutedEventArgs e)
    {
        LoadInterfaces();
    }

    private void InterfacesSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (InterfacesListBox.SelectedItem is not InterfaceDisplay selected)
        {
            SelectedInterfaceDetails.Text = "Выберите интерфейс из списка.";
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Имя: {selected.Name}");
        builder.AppendLine($"Описание: {selected.Description}");
        builder.AppendLine($"Тип интерфейса: {selected.InterfaceType}");
        builder.AppendLine($"Статус: {selected.OperationalStatus}");
        builder.AppendLine($"Скорость: {selected.SpeedMbps} Мбит/с");
        builder.AppendLine($"MAC: {selected.MacAddress}");
        builder.AppendLine($"IP-адреса: {string.Join(", ", selected.IpAddresses)}");
        builder.AppendLine($"Маски подсети: {string.Join(", ", selected.SubnetMasks)}");

        SelectedInterfaceDetails.Text = builder.ToString();
    }

    private void AnalyzeUrlClick(object? sender, RoutedEventArgs e)
    {
        if (!TryParseInputUri(out var uri, out var inputError))
        {
            ResultTextBox.Text = inputError;
            return;
        }

        AddToHistory(uri!.ToString());

        var result = new StringBuilder();
        var effectivePort = GetEffectivePort(uri);
        result.AppendLine("Анализ URL завершен:");
        result.AppendLine($"Схема: {uri.Scheme}");
        result.AppendLine($"Хост: {uri.Host}");
        result.AppendLine($"Порт: {(uri.IsDefaultPort ? $"{effectivePort} (по умолчанию для схемы)" : effectivePort)}");
        result.AppendLine($"Путь: {uri.AbsolutePath}");
        result.AppendLine($"Параметры запроса: {(string.IsNullOrWhiteSpace(uri.Query) ? "(нет)" : uri.Query)}");
        result.AppendLine($"Фрагмент: {(string.IsNullOrWhiteSpace(uri.Fragment) ? "(нет)" : uri.Fragment)}");
        result.AppendLine($"Тип адреса: {DetermineAddressType(uri.Host)}");

        ResultTextBox.Text = result.ToString();
    }

    private async void PingHostClick(object? sender, RoutedEventArgs e)
    {
        if (!TryParseInputUri(out var uri, out var inputError))
        {
            ResultTextBox.Text = inputError;
            return;
        }

        AddToHistory(uri!.ToString());

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(uri.Host, 4000);
            var result = new StringBuilder();
            result.AppendLine($"Ping {uri.Host}:");
            result.AppendLine($"Статус: {reply.Status}");
            if (reply.Status == IPStatus.Success)
            {
                result.AppendLine($"IP: {reply.Address}");
                result.AppendLine($"Время: {reply.RoundtripTime} мс");
                result.AppendLine($"TTL: {reply.Options?.Ttl}");
            }
            else if (reply.Status == IPStatus.TimedOut)
            {
                var port = GetEffectivePort(uri);
                var tcpCheck = await TryTcpConnectAsync(uri.Host, port, 4000);

                result.AppendLine("ICMP может быть заблокирован на стороне узла или в сети.");
                result.AppendLine($"TCP-проверка {uri.Host}:{port}: {(tcpCheck.Success ? "доступен" : "недоступен")}, {tcpCheck.ElapsedMs} мс");

                if (!tcpCheck.Success && !string.IsNullOrWhiteSpace(tcpCheck.Error))
                {
                    result.AppendLine($"Причина TCP: {tcpCheck.Error}");
                }
            }

            ResultTextBox.Text = result.ToString();
        }
        catch (Exception ex)
        {
            ResultTextBox.Text = $"Ошибка Ping: {ex.Message}";
        }
    }

    private async void ResolveDnsClick(object? sender, RoutedEventArgs e)
    {
        if (!TryParseInputUri(out var uri, out var inputError))
        {
            ResultTextBox.Text = inputError;
            return;
        }

        AddToHistory(uri!.ToString());

        try
        {
            var dnsEntry = await Dns.GetHostEntryAsync(uri.Host);
            var result = new StringBuilder();
            result.AppendLine($"DNS для {uri.Host}:");
            result.AppendLine($"Каноническое имя: {dnsEntry.HostName}");
            result.AppendLine("IP-адреса:");

            foreach (var address in dnsEntry.AddressList)
            {
                result.AppendLine($"- {address} ({DetermineAddressType(address)})");
            }

            ResultTextBox.Text = result.ToString();
        }
        catch (Exception ex)
        {
            ResultTextBox.Text = $"Ошибка DNS-запроса: {ex.Message}";
        }
    }

    private static async Task<(bool Success, long ElapsedMs, string Error)> TryTcpConnectAsync(string host, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        var sw = Stopwatch.StartNew();

        var connectTask = client.ConnectAsync(host, port);
        var timeoutTask = Task.Delay(timeoutMs);
        var completed = await Task.WhenAny(connectTask, timeoutTask);

        sw.Stop();

        if (completed == timeoutTask)
        {
            return (false, sw.ElapsedMilliseconds, $"таймаут {timeoutMs} мс");
        }

        try
        {
            await connectTask;
            return (true, sw.ElapsedMilliseconds, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, sw.ElapsedMilliseconds, ex.Message);
        }
    }

    private static int GetEffectivePort(Uri uri)
    {
        if (!uri.IsDefaultPort)
        {
            return uri.Port;
        }

        return uri.Scheme.ToLowerInvariant() switch
        {
            "https" => 443,
            "http" => 80,
            _ => uri.Port
        };
    }

    private void ClearInputClick(object? sender, RoutedEventArgs e)
    {
        UrlInputTextBox.Text = string.Empty;
        UrlInputTextBox.Focus();
    }

    private void ClearHistoryClick(object? sender, RoutedEventArgs e)
    {
        HistoryListBox.ItemsSource = Array.Empty<string>();
        SaveHistory(Array.Empty<string>());
    }

    private void HistorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (HistoryListBox.SelectedItem is not string entry || string.IsNullOrWhiteSpace(entry))
        {
            return;
        }

        var separator = " | ";
        var separatorIndex = entry.IndexOf(separator, StringComparison.Ordinal);
        var url = separatorIndex >= 0
            ? entry[(separatorIndex + separator.Length)..].Trim()
            : entry.Trim();

        if (!string.IsNullOrWhiteSpace(url))
        {
            UrlInputTextBox.Text = url;
            UrlInputTextBox.CaretIndex = url.Length;
        }
    }

    private void LoadInterfaces()
    {
        var interfaces = NetworkInterface
            .GetAllNetworkInterfaces()
            .Select(ToInterfaceDisplay)
            .OrderBy(i => i.Name)
            .ToList();

        InterfacesListBox.ItemsSource = interfaces;
        SelectedInterfaceDetails.Text = interfaces.Count == 0
            ? "Интерфейсы не найдены."
            : "Выберите интерфейс из списка.";
    }

    private static InterfaceDisplay ToInterfaceDisplay(NetworkInterface ni)
    {
        var ipProps = ni.GetIPProperties();
        var unicast = ipProps.UnicastAddresses;

        var ipAddresses = unicast
            .Select(a => a.Address)
            .Where(a => a.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
            .Select(a => a.ToString())
            .Distinct()
            .ToList();

        var subnetMasks = unicast
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && a.IPv4Mask != null)
            .Select(a => a.IPv4Mask!.ToString())
            .Distinct()
            .ToList();

        var speedMbps = ni.Speed > 0 ? ni.Speed / 1_000_000 : 0;

        return new InterfaceDisplay
        {
            Name = ni.Name,
            Description = ni.Description,
            InterfaceType = ni.NetworkInterfaceType.ToString(),
            OperationalStatus = ni.OperationalStatus.ToString(),
            SpeedMbps = speedMbps,
            MacAddress = FormatMac(ni.GetPhysicalAddress()),
            IpAddresses = ipAddresses,
            SubnetMasks = subnetMasks,
            QuickInfo = $"{ni.NetworkInterfaceType} | {ni.OperationalStatus}"
        };
    }

    private static string FormatMac(PhysicalAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes.Length == 0 ? "(нет)" : string.Join(":", bytes.Select(b => b.ToString("X2")));
    }

    private bool TryParseInputUri(out Uri? uri, out string error)
    {
        var input = UrlInputTextBox.Text?.Trim();
        uri = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Введите URL для анализа.";
            return false;
        }

        if (!input.Contains("://", StringComparison.Ordinal))
        {
            input = $"https://{input}";
        }

        if (!Uri.TryCreate(input, UriKind.Absolute, out uri) || string.IsNullOrWhiteSpace(uri.Host))
        {
            error = "Некорректный URL. Пример: https://example.com:8080/path?q=1#hash";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string DetermineAddressType(string host)
    {
        if (IPAddress.TryParse(host, out var ip))
        {
            return DetermineAddressType(ip);
        }

        return "доменное имя";
    }

    private static string DetermineAddressType(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return "loopback";
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            var isPrivate =
                bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168) ||
                (bytes[0] == 169 && bytes[1] == 254);

            return isPrivate ? "локальный" : "публичный";
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();
            var isLocalV6 = (bytes[0] & 0xFE) == 0xFC || (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80);
            return isLocalV6 ? "локальный" : "публичный";
        }

        return "неизвестный";
    }

    private void AddToHistory(string url)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var existing = LoadHistoryEntries();

        existing.Insert(0, $"{timestamp} | {url}");
        var trimmed = existing.Distinct().Take(30).ToList();

        SaveHistory(trimmed);
        HistoryListBox.ItemsSource = trimmed;
    }

    private void LoadHistory()
    {
        HistoryListBox.ItemsSource = LoadHistoryEntries();
    }

    private List<string> LoadHistoryEntries()
    {
        if (!File.Exists(_historyFilePath))
        {
            return new List<string>();
        }

        try
        {
            var json = File.ReadAllText(_historyFilePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private void SaveHistory(IEnumerable<string> entries)
    {
        var directory = Path.GetDirectoryName(_historyFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_historyFilePath, json);
    }

    private static string BuildHistoryFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "NetworkAnalyzer", "url_history.json");
    }

    private sealed class InterfaceDisplay
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string InterfaceType { get; init; } = string.Empty;
        public string OperationalStatus { get; init; } = string.Empty;
        public long SpeedMbps { get; init; }
        public string MacAddress { get; init; } = string.Empty;
        public List<string> IpAddresses { get; init; } = new();
        public List<string> SubnetMasks { get; init; } = new();
        public string QuickInfo { get; init; } = string.Empty;
    }
}