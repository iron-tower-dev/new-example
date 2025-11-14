using LabResultsApi.Models.Migration;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Mail;
using System.Net;

namespace LabResultsApi.Services.Migration;

public class MigrationNotificationService : IMigrationNotificationService
{
    private readonly ILogger<MigrationNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMigrationLoggingService _loggingService;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<Guid, List<RealTimeAlert>> _realTimeAlerts;
    private readonly ConcurrentDictionary<string, NotificationPreferences> _userPreferences;
    private readonly ConcurrentDictionary<Guid, List<NotificationHistory>> _notificationHistory;
    private readonly List<EscalationRule> _escalationRules;
    private readonly NotificationConfiguration _config;

    public MigrationNotificationService(
        ILogger<MigrationNotificationService> logger,
        IConfiguration configuration,
        IMigrationLoggingService loggingService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _loggingService = loggingService;
        _httpClient = httpClientFactory.CreateClient();
        _realTimeAlerts = new ConcurrentDictionary<Guid, List<RealTimeAlert>>();
        _userPreferences = new ConcurrentDictionary<string, NotificationPreferences>();
        _notificationHistory = new ConcurrentDictionary<Guid, List<NotificationHistory>>();
        _escalationRules = new List<EscalationRule>();
        _config = _configuration.GetSection("Migration:Notifications").Get<NotificationConfiguration>() 
            ?? new NotificationConfiguration();

        InitializeDefaultEscalationRules();
        LoadUserPreferences();
    }

    public async Task SendMigrationStartedEmailAsync(Guid migrationId, MigrationOptions options, List<string> recipients)
    {
        var notification = new MigrationNotification
        {
            NotificationId = Guid.NewGuid(),
            MigrationId = migrationId,
            Type = NotificationType.MigrationStarted,
            Title = "Migration Started",
            Message = $"Database migration {migrationId} has started",
            Severity = AlertSeverity.Info,
            Timestamp = DateTime.UtcNow,
            Recipients = recipients,
            Channels = new List<NotificationChannel> { NotificationChannel.Email },
            Data = new Dictionary<string, object>
            {
                ["MigrationId"] = migrationId,
                ["Options"] = options,
                ["StartTime"] = DateTime.UtcNow
            }
        };

        await SendNotificationAsync(notification);
    }

    public async Task SendMigrationCompletedEmailAsync(Guid migrationId, MigrationResult result, List<string> recipients)
    {
        var notification = new MigrationNotification
        {
            NotificationId = Guid.NewGuid(),
            MigrationId = migrationId,
            Type = NotificationType.MigrationCompleted,
            Title = "Migration Completed Successfully",
            Message = $"Database migration {migrationId} has completed successfully",
            Severity = AlertSeverity.Info,
            Timestamp = DateTime.UtcNow,
            Recipients = recipients,
            Channels = new List<NotificationChannel> { NotificationChannel.Email },
            Data = new Dictionary<string, object>
            {
                ["MigrationId"] = migrationId,
                ["Result"] = result,
                ["Duration"] = result.Duration?.ToString() ?? "Unknown",
                ["RecordsProcessed"] = result.Statistics.RecordsProcessed,
                ["ErrorCount"] = result.Errors.Count
            }
        };

        await SendNotificationAsync(notification);
    }

    public async Task SendMigrationFailedEmailAsync(Guid migrationId, MigrationResult result, List<string> recipients)
    {
        var notification = new MigrationNotification
        {
            NotificationId = Guid.NewGuid(),
            MigrationId = migrationId,
            Type = NotificationType.MigrationFailed,
            Title = "Migration Failed",
            Message = $"Database migration {migrationId} has failed",
            Severity = AlertSeverity.Critical,
            Timestamp = DateTime.UtcNow,
            Recipients = recipients,
            Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Slack },
            Data = new Dictionary<string, object>
            {
                ["MigrationId"] = migrationId,
                ["Result"] = result,
                ["ErrorCount"] = result.Errors.Count,
                ["LastError"] = result.Errors.LastOrDefault()?.Message ?? "Unknown error"
            }
        };

        await SendNotificationAsync(notification);
        await TriggerEscalationAsync(migrationId, EscalationLevel.Level1, "Migration failed");
    }

    public async Task SendPerformanceAlertEmailAsync(Guid migrationId, PerformanceAlert alert, List<string> recipients)
    {
        var notification = new MigrationNotification
        {
            NotificationId = Guid.NewGuid(),
            MigrationId = migrationId,
            Type = NotificationType.PerformanceAlert,
            Title = $"Performance Alert: {alert.AlertType}",
            Message = alert.Message,
            Severity = alert.Severity,
            Timestamp = DateTime.UtcNow,
            Recipients = recipients,
            Channels = new List<NotificationChannel> { NotificationChannel.Email },
            Data = new Dictionary<string, object>
            {
                ["MigrationId"] = migrationId,
                ["Alert"] = alert,
                ["Component"] = alert.Component,
                ["Metrics"] = alert.Metrics
            }
        };

        await SendNotificationAsync(notification);

        if (alert.Severity == AlertSeverity.Critical)
        {
            await TriggerEscalationAsync(migrationId, EscalationLevel.Level2, $"Critical performance alert: {alert.AlertType}");
        }
    }

    public async Task SendSlackNotificationAsync(string channel, MigrationNotification notification)
    {
        if (string.IsNullOrEmpty(_config.SlackWebhookUrl))
        {
            _logger.LogWarning("Slack webhook URL not configured");
            return;
        }

        try
        {
            var slackMessage = CreateSlackMessage(channel, notification);
            var json = JsonSerializer.Serialize(slackMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_config.SlackWebhookUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                await _loggingService.LogStructuredAsync(notification.MigrationId, LabResultsApi.Models.Migration.LogLevel.Information,
                    "SlackNotification", $"Slack notification sent to {channel}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send Slack notification: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack notification to {Channel}", channel);
        }
    }

    public async Task SendTeamsNotificationAsync(string webhookUrl, MigrationNotification notification)
    {
        if (string.IsNullOrEmpty(webhookUrl))
        {
            _logger.LogWarning("Teams webhook URL not provided");
            return;
        }

        try
        {
            var teamsMessage = CreateTeamsMessage(notification);
            var json = JsonSerializer.Serialize(teamsMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                await _loggingService.LogStructuredAsync(notification.MigrationId, LabResultsApi.Models.Migration.LogLevel.Information,
                    "TeamsNotification", "Teams notification sent");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send Teams notification: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Teams notification");
        }
    }

    public async Task SendRealTimeAlertAsync(Guid migrationId, AlertSeverity severity, string message, Dictionary<string, object>? data = null)
    {
        var alert = new RealTimeAlert
        {
            AlertId = Guid.NewGuid(),
            MigrationId = migrationId,
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            AlertType = "RealTimeAlert",
            Message = message,
            Component = "MigrationMonitoring",
            Data = data ?? new Dictionary<string, object>(),
            IsResolved = false
        };

        _realTimeAlerts.AddOrUpdate(migrationId, 
            new List<RealTimeAlert> { alert },
            (key, existing) => 
            {
                existing.Add(alert);
                // Keep only the last 100 alerts per migration
                if (existing.Count > 100)
                {
                    existing.RemoveRange(0, existing.Count - 100);
                }
                return existing;
            });

        await _loggingService.LogStructuredAsync(migrationId, 
            severity == AlertSeverity.Critical ? LabResultsApi.Models.Migration.LogLevel.Error : LabResultsApi.Models.Migration.LogLevel.Warning,
            "RealTimeAlert", message, data);

        // Send immediate notifications for critical alerts
        if (severity == AlertSeverity.Critical)
        {
            var notification = new MigrationNotification
            {
                NotificationId = Guid.NewGuid(),
                MigrationId = migrationId,
                Type = NotificationType.ErrorAlert,
                Title = "Critical Alert",
                Message = message,
                Severity = severity,
                Timestamp = DateTime.UtcNow,
                Recipients = _config.CriticalAlertRecipients,
                Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Slack },
                Data = data ?? new Dictionary<string, object>()
            };

            await SendNotificationAsync(notification);
        }
    }

    public async Task<List<RealTimeAlert>> GetActiveRealTimeAlertsAsync(Guid migrationId)
    {
        var alerts = _realTimeAlerts.GetValueOrDefault(migrationId, new List<RealTimeAlert>());
        return await Task.FromResult(alerts.Where(a => !a.IsResolved).OrderByDescending(a => a.Timestamp).ToList());
    }

    public async Task ResolveRealTimeAlertAsync(Guid alertId)
    {
        foreach (var migrationAlerts in _realTimeAlerts.Values)
        {
            var alert = migrationAlerts.FirstOrDefault(a => a.AlertId == alertId);
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedAt = DateTime.UtcNow;
                alert.ResolvedBy = "System"; // In production, would get from current user context
                
                await _loggingService.LogStructuredAsync(alert.MigrationId, LabResultsApi.Models.Migration.LogLevel.Information,
                    "AlertResolution", $"Alert {alertId} resolved", 
                    new Dictionary<string, object> { ["AlertId"] = alertId });
                break;
            }
        }
    }

    public async Task TriggerEscalationAsync(Guid migrationId, EscalationLevel level, string reason)
    {
        var escalationRules = _escalationRules.Where(r => r.IsEnabled).OrderBy(r => r.Priority).ToList();
        
        foreach (var rule in escalationRules)
        {
            if (await ShouldTriggerEscalation(migrationId, rule))
            {
                await ExecuteEscalationActions(migrationId, rule, level, reason);
            }
        }

        await _loggingService.LogStructuredAsync(migrationId, LabResultsApi.Models.Migration.LogLevel.Warning,
            "Escalation", $"Escalation triggered: {level} - {reason}",
            new Dictionary<string, object> 
            { 
                ["EscalationLevel"] = level.ToString(),
                ["Reason"] = reason
            });
    }

    public async Task<List<EscalationRule>> GetEscalationRulesAsync()
    {
        return await Task.FromResult(_escalationRules.ToList());
    }

    public async Task UpdateEscalationRulesAsync(List<EscalationRule> rules)
    {
        _escalationRules.Clear();
        _escalationRules.AddRange(rules);
        
        // In production, would persist to database
        _logger.LogInformation("Escalation rules updated: {RuleCount} rules", rules.Count);
        await Task.CompletedTask;
    }

    public async Task<NotificationPreferences> GetNotificationPreferencesAsync(string userId)
    {
        return await Task.FromResult(_userPreferences.GetValueOrDefault(userId, CreateDefaultPreferences(userId)));
    }

    public async Task UpdateNotificationPreferencesAsync(string userId, NotificationPreferences preferences)
    {
        _userPreferences.AddOrUpdate(userId, preferences, (key, existing) => preferences);
        
        // In production, would persist to database
        _logger.LogInformation("Notification preferences updated for user {UserId}", userId);
        await Task.CompletedTask;
    }

    public async Task<List<NotificationHistory>> GetNotificationHistoryAsync(Guid migrationId)
    {
        var history = _notificationHistory.GetValueOrDefault(migrationId, new List<NotificationHistory>());
        return await Task.FromResult(history.OrderByDescending(h => h.SentAt).ToList());
    }

    public async Task<NotificationDeliveryStatus> GetNotificationStatusAsync(Guid notificationId)
    {
        foreach (var history in _notificationHistory.Values)
        {
            var notification = history.FirstOrDefault(h => h.NotificationId == notificationId);
            if (notification != null)
            {
                var overallStatus = notification.Deliveries.Any() 
                    ? notification.Deliveries.Min(d => d.Status)
                    : NotificationDeliveryStatus.Pending;
                return await Task.FromResult(overallStatus);
            }
        }
        
        return await Task.FromResult(NotificationDeliveryStatus.Failed);
    }

    private async Task SendNotificationAsync(MigrationNotification notification)
    {
        var deliveries = new List<NotificationDelivery>();

        foreach (var channel in notification.Channels)
        {
            foreach (var recipient in notification.Recipients)
            {
                var delivery = new NotificationDelivery
                {
                    Channel = channel,
                    Recipient = recipient,
                    Status = NotificationDeliveryStatus.Pending,
                    RetryCount = 0
                };

                try
                {
                    switch (channel)
                    {
                        case NotificationChannel.Email:
                            await SendEmailNotificationAsync(recipient, notification);
                            delivery.Status = NotificationDeliveryStatus.Sent;
                            delivery.DeliveredAt = DateTime.UtcNow;
                            break;
                        
                        case NotificationChannel.Slack:
                            await SendSlackNotificationAsync(_config.DefaultSlackChannel, notification);
                            delivery.Status = NotificationDeliveryStatus.Sent;
                            delivery.DeliveredAt = DateTime.UtcNow;
                            break;
                        
                        case NotificationChannel.Teams:
                            await SendTeamsNotificationAsync(_config.TeamsWebhookUrl, notification);
                            delivery.Status = NotificationDeliveryStatus.Sent;
                            delivery.DeliveredAt = DateTime.UtcNow;
                            break;
                        
                        default:
                            delivery.Status = NotificationDeliveryStatus.Failed;
                            delivery.ErrorMessage = $"Unsupported channel: {channel}";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    delivery.Status = NotificationDeliveryStatus.Failed;
                    delivery.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Failed to send notification via {Channel} to {Recipient}", channel, recipient);
                }

                deliveries.Add(delivery);
            }
        }

        // Record notification history
        var history = new NotificationHistory
        {
            NotificationId = notification.NotificationId,
            MigrationId = notification.MigrationId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Severity = notification.Severity,
            SentAt = DateTime.UtcNow,
            Deliveries = deliveries
        };

        _notificationHistory.AddOrUpdate(notification.MigrationId,
            new List<NotificationHistory> { history },
            (key, existing) => 
            {
                existing.Add(history);
                return existing;
            });
    }

    private async Task SendEmailNotificationAsync(string recipient, MigrationNotification notification)
    {
        if (string.IsNullOrEmpty(_config.SmtpHost))
        {
            _logger.LogWarning("SMTP configuration not found, skipping email notification");
            return;
        }

        try
        {
            using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort);
            client.EnableSsl = _config.SmtpUseSsl;
            
            if (!string.IsNullOrEmpty(_config.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.FromEmail, _config.FromName),
                Subject = notification.Title,
                Body = CreateEmailBody(notification),
                IsBodyHtml = true
            };

            mailMessage.To.Add(recipient);

            await client.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", recipient);
            throw;
        }
    }

    private string CreateEmailBody(MigrationNotification notification)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine($"<h2>{notification.Title}</h2>");
        sb.AppendLine($"<p><strong>Migration ID:</strong> {notification.MigrationId}</p>");
        sb.AppendLine($"<p><strong>Timestamp:</strong> {notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine($"<p><strong>Severity:</strong> {notification.Severity}</p>");
        sb.AppendLine($"<p>{notification.Message}</p>");
        
        if (notification.Data.Any())
        {
            sb.AppendLine("<h3>Additional Details:</h3>");
            sb.AppendLine("<ul>");
            foreach (var kvp in notification.Data)
            {
                sb.AppendLine($"<li><strong>{kvp.Key}:</strong> {kvp.Value}</li>");
            }
            sb.AppendLine("</ul>");
        }
        
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private SlackMessage CreateSlackMessage(string channel, MigrationNotification notification)
    {
        var color = notification.Severity switch
        {
            AlertSeverity.Critical => "danger",
            AlertSeverity.Warning => "warning",
            _ => "good"
        };

        var attachment = new SlackAttachment
        {
            Color = color,
            Title = notification.Title,
            Text = notification.Message,
            Timestamp = ((DateTimeOffset)notification.Timestamp).ToUnixTimeSeconds(),
            Fields = new List<SlackField>
            {
                new SlackField { Title = "Migration ID", Value = notification.MigrationId.ToString(), Short = true },
                new SlackField { Title = "Severity", Value = notification.Severity.ToString(), Short = true }
            }
        };

        return new SlackMessage
        {
            Channel = channel,
            Text = notification.Title,
            Attachments = new List<SlackAttachment> { attachment },
            IconEmoji = ":warning:",
            Username = "Migration Bot"
        };
    }

    private TeamsMessage CreateTeamsMessage(MigrationNotification notification)
    {
        var themeColor = notification.Severity switch
        {
            AlertSeverity.Critical => "FF0000",
            AlertSeverity.Warning => "FFA500",
            _ => "00FF00"
        };

        var facts = new List<TeamsFact>
        {
            new TeamsFact { Name = "Migration ID", Value = notification.MigrationId.ToString() },
            new TeamsFact { Name = "Timestamp", Value = notification.Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC") },
            new TeamsFact { Name = "Severity", Value = notification.Severity.ToString() }
        };

        var section = new TeamsSection
        {
            ActivityTitle = notification.Title,
            ActivitySubtitle = $"Migration Notification - {notification.Type}",
            Text = notification.Message,
            Facts = facts
        };

        return new TeamsMessage
        {
            Summary = notification.Title,
            ThemeColor = themeColor,
            Sections = new List<TeamsSection> { section }
        };
    }

    private async Task<bool> ShouldTriggerEscalation(Guid migrationId, EscalationRule rule)
    {
        // Simplified escalation logic - in production would be more sophisticated
        var alerts = _realTimeAlerts.GetValueOrDefault(migrationId, new List<RealTimeAlert>());
        var recentAlerts = alerts.Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-10)).ToList();
        
        foreach (var trigger in rule.Triggers)
        {
            switch (trigger.TriggerType)
            {
                case "AlertCount":
                    if (recentAlerts.Count > Convert.ToInt32(trigger.Threshold))
                        return true;
                    break;
                
                case "CriticalAlertCount":
                    var criticalCount = recentAlerts.Count(a => a.Severity == AlertSeverity.Critical);
                    if (criticalCount > Convert.ToInt32(trigger.Threshold))
                        return true;
                    break;
            }
        }
        
        return await Task.FromResult(false);
    }

    private async Task ExecuteEscalationActions(Guid migrationId, EscalationRule rule, EscalationLevel level, string reason)
    {
        foreach (var action in rule.Actions)
        {
            try
            {
                switch (action.ActionType)
                {
                    case "Email":
                        var notification = new MigrationNotification
                        {
                            NotificationId = Guid.NewGuid(),
                            MigrationId = migrationId,
                            Type = NotificationType.EscalationTriggered,
                            Title = $"Escalation Triggered: {level}",
                            Message = $"Escalation rule '{rule.Name}' triggered: {reason}",
                            Severity = AlertSeverity.Critical,
                            Timestamp = DateTime.UtcNow,
                            Recipients = action.Recipients,
                            Channels = new List<NotificationChannel> { NotificationChannel.Email }
                        };
                        await SendNotificationAsync(notification);
                        break;
                    
                    case "Slack":
                        var slackNotification = new MigrationNotification
                        {
                            NotificationId = Guid.NewGuid(),
                            MigrationId = migrationId,
                            Type = NotificationType.EscalationTriggered,
                            Title = $"🚨 Escalation: {level}",
                            Message = $"Rule: {rule.Name}\nReason: {reason}",
                            Severity = AlertSeverity.Critical,
                            Timestamp = DateTime.UtcNow
                        };
                        await SendSlackNotificationAsync(_config.EscalationSlackChannel, slackNotification);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute escalation action {ActionType} for rule {RuleName}", 
                    action.ActionType, rule.Name);
            }
        }
    }

    private void InitializeDefaultEscalationRules()
    {
        _escalationRules.AddRange(new[]
        {
            new EscalationRule
            {
                RuleId = Guid.NewGuid(),
                Name = "High Alert Volume",
                Triggers = new List<EscalationTrigger>
                {
                    new EscalationTrigger
                    {
                        TriggerType = "AlertCount",
                        Condition = "GreaterThan",
                        Threshold = 10,
                        TimeWindow = TimeSpan.FromMinutes(10)
                    }
                },
                Actions = new List<EscalationAction>
                {
                    new EscalationAction
                    {
                        ActionType = "Email",
                        Recipients = _config.EscalationRecipients,
                        DelayMinutes = 0
                    }
                },
                IsEnabled = true,
                Priority = 1
            },
            new EscalationRule
            {
                RuleId = Guid.NewGuid(),
                Name = "Critical Alert Escalation",
                Triggers = new List<EscalationTrigger>
                {
                    new EscalationTrigger
                    {
                        TriggerType = "CriticalAlertCount",
                        Condition = "GreaterThan",
                        Threshold = 3,
                        TimeWindow = TimeSpan.FromMinutes(5)
                    }
                },
                Actions = new List<EscalationAction>
                {
                    new EscalationAction
                    {
                        ActionType = "Email",
                        Recipients = _config.CriticalAlertRecipients,
                        DelayMinutes = 0
                    },
                    new EscalationAction
                    {
                        ActionType = "Slack",
                        Recipients = new List<string>(),
                        DelayMinutes = 2
                    }
                },
                IsEnabled = true,
                Priority = 0
            }
        });
    }

    private void LoadUserPreferences()
    {
        // In production, would load from database
        // For now, create some default preferences
        var defaultPrefs = CreateDefaultPreferences("admin");
        _userPreferences.TryAdd("admin", defaultPrefs);
    }

    private NotificationPreferences CreateDefaultPreferences(string userId)
    {
        return new NotificationPreferences
        {
            UserId = userId,
            EnabledChannels = new List<NotificationChannel> 
            { 
                NotificationChannel.Email, 
                NotificationChannel.Slack 
            },
            TypePreferences = Enum.GetValues<NotificationType>().ToDictionary(t => t, t => true),
            SeverityPreferences = Enum.GetValues<AlertSeverity>().ToDictionary(s => s, s => true),
            EmailAddress = _config.DefaultEmail,
            QuietHoursEnabled = false
        };
    }
}

public class NotificationConfiguration
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@company.com";
    public string FromName { get; set; } = "Migration System";
    public string DefaultEmail { get; set; } = "admin@company.com";
    
    public string SlackWebhookUrl { get; set; } = string.Empty;
    public string DefaultSlackChannel { get; set; } = "#migrations";
    public string EscalationSlackChannel { get; set; } = "#alerts";
    
    public string TeamsWebhookUrl { get; set; } = string.Empty;
    
    public List<string> CriticalAlertRecipients { get; set; } = new();
    public List<string> EscalationRecipients { get; set; } = new();
}