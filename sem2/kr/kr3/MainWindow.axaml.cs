using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpMonitorWpf;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new();
    private readonly ConcurrentDictionary<string, string> _messages = new();
    private readonly List<LogEntry> _logEntries = new();
    private readonly object _logsSync = new();
    private readonly object _fileSync = new();

    private readonly ConcurrentDictionary<DateTime, int> _minuteBuckets = new();
    private readonly ConcurrentDictionary<DateTime, int> _hourBuckets = new();

    private readonly string _logFilePath = System.IO.Path.Combine(AppContext.BaseDirectory, "logs.txt");

    private HttpListener? _listener;
    private CancellationTokenSource? _serverCts;
    private DateTime _serverStartUtc;

    private long _totalRequests;
    private long _getRequests;
    private long _postRequests;
    private long _totalProcessingMs;

    private readonly Timer _uiTimer;

    public MainWindow()
    {
        InitializeComponent();
        _uiTimer = new Timer(_ => Dispatcher.UIThread.Post(UpdateStatsUi), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        UpdateStatsUi();
    }

    private async void StartServerClick(object? sender, RoutedEventArgs e)
    {
        if (_listener is not null)
        {
            return;
        }

        if (!int.TryParse(PortTextBox.Text, out var port) || port < 1 || port > 65535)
        {
            await ShowError("Укажите корректный порт 1..65535");
            return;
        }

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();
            _serverCts = new CancellationTokenSource();
            _serverStartUtc = DateTime.UtcNow;

            StartServerButton.IsEnabled = false;
            StopServerButton.IsEnabled = true;
            ServerStateTextBlock.Text = $"Сервер запущен: http://localhost:{port}/";

            _ = Task.Run(() => ServerLoopAsync(_serverCts.Token));
            AddLog("SERVER", "INFO", $"http://localhost:{port}/", 200, "", "", "Сервер запущен", 0);
        }
        catch (Exception ex)
        {
            _listener = null;
            _serverCts?.Dispose();
            _serverCts = null;
            await ShowError($"Ошибка запуска сервера: {ex.Message}");
        }
    }

    private void StopServerClick(object? sender, RoutedEventArgs e)
    {
        StopServerInternal();
        AddLog("SERVER", "INFO", "local", 200, "", "", "Сервер остановлен", 0);
    }

    private void StopServerInternal()
    {
        try
        {
            _serverCts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
        }
        catch
        {
            // Ignore listener stop race conditions.
        }
        finally
        {
            _serverCts?.Dispose();
            _serverCts = null;
            _listener = null;
        }

        StartServerButton.IsEnabled = true;
        StopServerButton.IsEnabled = false;
        ServerStateTextBlock.Text = "Сервер остановлен";
    }

    private async Task ServerLoopAsync(CancellationToken token)
    {
        var listener = _listener;
        if (listener is null)
        {
            return;
        }

        while (!token.IsCancellationRequested)
        {
            HttpListenerContext? context = null;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }

            if (context is null)
            {
                continue;
            }

            _ = Task.Run(() => HandleContextAsync(context), token);
        }
    }

    private async Task HandleContextAsync(HttpListenerContext context)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;
        var method = request.HttpMethod.ToUpperInvariant();
        var path = request.Url?.AbsolutePath ?? "/";
        var headers = BuildHeadersText(request.Headers);
        var requestBody = await ReadBodyAsync(request);

        AddLog("IN", method, request.Url?.ToString() ?? "", 0, headers, requestBody, "Входящий запрос", 0);

        int statusCode;
        string responseBody;
        string responseContentType = "application/json; charset=utf-8";

        Interlocked.Increment(ref _totalRequests);
        IncrementBuckets(DateTime.Now);

        if (method == "GET")
        {
            Interlocked.Increment(ref _getRequests);
            var uptime = DateTime.UtcNow - _serverStartUtc;
            var total = Interlocked.Read(ref _totalRequests);
            var getCount = Interlocked.Read(ref _getRequests);
            var postCount = Interlocked.Read(ref _postRequests);
            var avg = total == 0 ? 0 : Interlocked.Read(ref _totalProcessingMs) / (double)total;

            responseBody = JsonSerializer.Serialize(new
            {
                status = "ok",
                endpoint = path,
                uptimeSeconds = (int)uptime.TotalSeconds,
                totalRequests = total,
                getRequests = getCount,
                postRequests = postCount,
                averageProcessingMs = Math.Round(avg, 2),
                messagesStored = _messages.Count
            });
            statusCode = 200;
        }
        else if (method == "POST")
        {
            Interlocked.Increment(ref _postRequests);
            var id = Guid.NewGuid().ToString("N");
            var parsedMessage = ExtractMessage(requestBody);
            _messages[id] = parsedMessage;

            responseBody = JsonSerializer.Serialize(new
            {
                status = "created",
                id,
                message = parsedMessage
            });
            statusCode = 201;
        }
        else
        {
            statusCode = 405;
            responseBody = JsonSerializer.Serialize(new
            {
                status = "error",
                message = "Only GET and POST methods are supported"
            });
        }

        sw.Stop();
        Interlocked.Add(ref _totalProcessingMs, sw.ElapsedMilliseconds);

        var responseBytes = Encoding.UTF8.GetBytes(responseBody);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = responseContentType;
        context.Response.ContentLength64 = responseBytes.Length;

        try
        {
            await context.Response.OutputStream.WriteAsync(responseBytes);
        }
        finally
        {
            context.Response.OutputStream.Close();
        }

        AddLog("OUT", method, request.Url?.ToString() ?? "", statusCode, "Content-Type: application/json", responseBody,
            "Ответ отправлен", sw.Elapsed.TotalMilliseconds);
    }

    private async void SendRequestClick(object? sender, RoutedEventArgs e)
    {
        var url = ClientUrlTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            await ShowError("Укажите URL для клиентского запроса");
            return;
        }

        var method = (ClientMethodComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "GET";
        var body = ClientBodyTextBox.Text ?? string.Empty;

        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST")
        {
            request.Content = new StringContent(body, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        var outHeaders = method == "POST" ? "Content-Type: application/json" : "";
        AddLog("OUT", method, url, 0, outHeaders, body, "Клиент отправил запрос", 0);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            sw.Stop();

            ClientResponseTextBox.Text = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n\n{responseText}";
            AddLog("IN", method, url, (int)response.StatusCode, BuildHeadersText(response.Headers), responseText,
                "Клиент получил ответ", sw.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorText = $"Ошибка отправки: {ex.Message}";
            ClientResponseTextBox.Text = errorText;
            AddLog("IN", method, url, 500, "", errorText, "Ошибка клиента", sw.Elapsed.TotalMilliseconds);
        }
    }

    private static async Task<string> ReadBodyAsync(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static string BuildHeadersText(NameValueCollection headers)
    {
        if (headers.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (string? key in headers.AllKeys)
        {
            if (key is null)
            {
                continue;
            }

            sb.Append(key).Append(": ").Append(headers[key]).AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildHeadersText(HttpResponseHeaders headers)
    {
        var sb = new StringBuilder();
        foreach (var pair in headers)
        {
            sb.Append(pair.Key).Append(": ").Append(string.Join(", ", pair.Value)).AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string ExtractMessage(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // Keep original body when JSON is malformed.
        }

        return rawJson;
    }

    private void IncrementBuckets(DateTime now)
    {
        var minuteKey = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
        var hourKey = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

        _minuteBuckets.AddOrUpdate(minuteKey, 1, (_, old) => old + 1);
        _hourBuckets.AddOrUpdate(hourKey, 1, (_, old) => old + 1);

        TrimBuckets();
        Dispatcher.UIThread.Post(RenderLoadChart);
    }

    private void TrimBuckets()
    {
        var minThreshold = DateTime.Now.AddHours(-2);
        foreach (var key in _minuteBuckets.Keys)
        {
            if (key < minThreshold)
            {
                _minuteBuckets.TryRemove(key, out _);
            }
        }

        var hourThreshold = DateTime.Now.AddDays(-3);
        foreach (var key in _hourBuckets.Keys)
        {
            if (key < hourThreshold)
            {
                _hourBuckets.TryRemove(key, out _);
            }
        }
    }

    private void AddLog(string direction, string method, string url, int statusCode, string headers, string body, string summary, double durationMs)
    {
        var entry = new LogEntry(
            DateTime.Now,
            direction,
            method,
            url,
            statusCode,
            headers,
            body,
            summary,
            durationMs);

        lock (_logsSync)
        {
            _logEntries.Add(entry);
            if (_logEntries.Count > 2000)
            {
                _logEntries.RemoveRange(0, _logEntries.Count - 2000);
            }
        }

        WriteLogToFile(entry);
        Dispatcher.UIThread.Post(RefreshLogsView);
    }

    private void WriteLogToFile(LogEntry entry)
    {
        var text = FormatLogEntry(entry) + Environment.NewLine + new string('-', 70) + Environment.NewLine;
        lock (_fileSync)
        {
            File.AppendAllText(_logFilePath, text, Encoding.UTF8);
        }
    }

    private void RefreshLogsView()
    {
        if (MethodFilterComboBox is null || StatusFilterComboBox is null || LogsTextBox is null)
        {
            return;
        }

        string methodFilter = (MethodFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
        string statusFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";

        List<LogEntry> snapshot;
        lock (_logsSync)
        {
            snapshot = _logEntries.ToList();
        }

        IEnumerable<LogEntry> query = snapshot;

        if (methodFilter == "GET" || methodFilter == "POST")
        {
            query = query.Where(x => string.Equals(x.Method, methodFilter, StringComparison.OrdinalIgnoreCase));
        }

        query = statusFilter switch
        {
            "2xx" => query.Where(x => x.StatusCode is >= 200 and < 300),
            "4xx" => query.Where(x => x.StatusCode is >= 400 and < 500),
            "5xx" => query.Where(x => x.StatusCode >= 500),
            _ => query
        };

        var builder = new StringBuilder();
        foreach (var entry in query.OrderByDescending(x => x.Timestamp).Take(200))
        {
            builder.AppendLine(FormatLogEntry(entry));
            builder.AppendLine(new string('-', 70));
        }

        LogsTextBox.Text = builder.ToString();
    }

    private static string FormatLogEntry(LogEntry entry)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Direction} {entry.Method} {entry.Url}");
        sb.AppendLine($"Status: {entry.StatusCode} | Duration: {entry.DurationMs:F1} ms");
        sb.AppendLine($"Summary: {entry.Summary}");

        if (!string.IsNullOrWhiteSpace(entry.Headers))
        {
            sb.AppendLine("Headers:");
            sb.AppendLine(entry.Headers);
        }

        if (!string.IsNullOrWhiteSpace(entry.Body))
        {
            sb.AppendLine("Body:");
            sb.AppendLine(entry.Body);
        }

        return sb.ToString().TrimEnd();
    }

    private void UpdateStatsUi()
    {
        TotalRequestsText.Text = Interlocked.Read(ref _totalRequests).ToString(CultureInfo.InvariantCulture);
        GetRequestsText.Text = Interlocked.Read(ref _getRequests).ToString(CultureInfo.InvariantCulture);
        PostRequestsText.Text = Interlocked.Read(ref _postRequests).ToString(CultureInfo.InvariantCulture);

        var total = Interlocked.Read(ref _totalRequests);
        var avg = total == 0 ? 0 : Interlocked.Read(ref _totalProcessingMs) / (double)total;
        AvgMsText.Text = avg.ToString("F1", CultureInfo.InvariantCulture);

        var uptime = _serverStartUtc == default ? TimeSpan.Zero : DateTime.UtcNow - _serverStartUtc;
        UptimeText.Text = $"Uptime: {uptime:hh\\:mm\\:ss}";

        RenderLoadChart();
    }

    private void RenderLoadChart()
    {
        if (LoadChartCanvas is null)
        {
            return;
        }

        var width = Math.Max(LoadChartCanvas.Bounds.Width, 320);
        var height = Math.Max(LoadChartCanvas.Bounds.Height, 180);

        LoadChartCanvas.Children.Clear();

        var isMinute = ((ChartRangeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "По минутам") == "По минутам";
        var buckets = isMinute ? _minuteBuckets : _hourBuckets;
        var ordered = buckets.OrderBy(x => x.Key).ToList();

        if (ordered.Count == 0)
        {
            ChartSummaryText.Text = "Нет данных для графика";
            LoadChartCanvas.Children.Add(new TextBlock
            {
                Text = "Пока нет входящих запросов",
                Foreground = new SolidColorBrush(Color.Parse("#64748B"))
            });
            return;
        }

        int maxPoints = isMinute ? 30 : 24;
        ordered = ordered.TakeLast(maxPoints).ToList();

        var maxY = Math.Max(1, ordered.Max(x => x.Value));
        var peak = ordered.MaxBy(x => x.Value);
        ChartSummaryText.Text = peak.Key == default
            ? "Нет данных"
            : $"Пик: {peak.Value} запрос(ов) в {(isMinute ? peak.Key.ToString("HH:mm") : peak.Key.ToString("dd.MM HH:00"))}";

        double left = 34;
        double right = 8;
        double top = 8;
        double bottom = 24;

        double plotW = width - left - right;
        double plotH = height - top - bottom;

        var axisColor = new SolidColorBrush(Color.Parse("#94A3B8"));
        var lineColor = new SolidColorBrush(Color.Parse("#0F766E"));
        var fillColor = new SolidColorBrush(Color.Parse("#D1FAE5"));

        LoadChartCanvas.Children.Add(new Line
        {
            StartPoint = new Point(left, top + plotH),
            EndPoint = new Point(left + plotW, top + plotH),
            Stroke = axisColor,
            StrokeThickness = 1
        });

        LoadChartCanvas.Children.Add(new Line
        {
            StartPoint = new Point(left, top),
            EndPoint = new Point(left, top + plotH),
            Stroke = axisColor,
            StrokeThickness = 1
        });

        for (int i = 0; i <= 4; i++)
        {
            var y = top + (plotH / 4) * i;
            LoadChartCanvas.Children.Add(new Line
            {
                StartPoint = new Point(left, y),
                EndPoint = new Point(left + plotW, y),
                Stroke = new SolidColorBrush(Color.Parse("#E2E8F0")),
                StrokeThickness = 1
            });

            var value = (int)Math.Round(maxY - (maxY / 4.0) * i);
            var label = new TextBlock
            {
                Text = value.ToString(CultureInfo.InvariantCulture),
                FontSize = 10,
                Foreground = axisColor
            };
            Canvas.SetLeft(label, 4);
            Canvas.SetTop(label, y - 7);
            LoadChartCanvas.Children.Add(label);
        }

        var polyline = new Polyline
        {
            Stroke = lineColor,
            StrokeThickness = 2
        };

        var area = new Polygon
        {
            Fill = fillColor,
            StrokeThickness = 0
        };

        for (int i = 0; i < ordered.Count; i++)
        {
            double x = left + (ordered.Count == 1 ? plotW / 2 : (plotW * i / (ordered.Count - 1)));
            double y = top + plotH - (plotH * ordered[i].Value / maxY);
            var point = new Point(x, y);
            polyline.Points.Add(point);
            area.Points.Add(point);

            if (i % Math.Max(1, ordered.Count / 6) == 0 || i == ordered.Count - 1)
            {
                var label = new TextBlock
                {
                    Text = isMinute ? ordered[i].Key.ToString("HH:mm") : ordered[i].Key.ToString("HH:00"),
                    FontSize = 10,
                    Foreground = axisColor
                };
                Canvas.SetLeft(label, x - 16);
                Canvas.SetTop(label, top + plotH + 4);
                LoadChartCanvas.Children.Add(label);
            }
        }

        area.Points.Add(new Point(left + plotW, top + plotH));
        area.Points.Add(new Point(left, top + plotH));

        LoadChartCanvas.Children.Add(area);
        LoadChartCanvas.Children.Add(polyline);
    }

    private void FilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshLogsView();
    }

    private void ChartRangeChanged(object? sender, SelectionChangedEventArgs e)
    {
        RenderLoadChart();
    }

    private void LoadChartSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        RenderLoadChart();
    }

    private void ClearLogsClick(object? sender, RoutedEventArgs e)
    {
        lock (_logsSync)
        {
            _logEntries.Clear();
        }

        LogsTextBox.Text = string.Empty;
    }

    private async Task ShowError(string message)
    {
        var errorBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FEE2E2")),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Child = new TextBlock
            {
                Text = "Error: " + message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.Parse("#991B1B")),
                FontSize = 12,
                FontWeight = FontWeight.SemiBold
            }
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 90,
            Padding = new Thickness(12, 8),
            Background = new SolidColorBrush(Color.Parse("#0F766E")),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = FontWeight.SemiBold,
            CornerRadius = new CornerRadius(8)
        };

        var content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 14,
            Children = { errorBorder, okButton }
        };

        var dialog = new Window
        {
            Width = 450,
            Height = 200,
            Title = "Error",
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = new SolidColorBrush(Color.Parse("#F3F6F9")),
            FontFamily = new FontFamily("Inter, Segoe UI"),
            Content = content
        };

        okButton.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        StopServerInternal();
        _uiTimer.Dispose();
        _httpClient.Dispose();
        base.OnClosed(e);
    }

    private sealed record LogEntry(
        DateTime Timestamp,
        string Direction,
        string Method,
        string Url,
        int StatusCode,
        string Headers,
        string Body,
        string Summary,
        double DurationMs);
}
