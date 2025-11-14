using LabResultsApi.Models.Migration;

namespace LabResultsApi.Services.Migration;

public interface IMigrationNotificationService
{
    // Email notifications
    Task SendMigrationStartedEmailAsync(Guid migrationId, MigrationOptions options, List<string> recipients);
    Task SendMigrationCompletedEmailAsync(Guid migrationId, MigrationResult result, List<string> recipients);
    Task SendMigrationFailedEmailAsync(Guid migrationId, MigrationResult result, List<string> recipients);
    Task SendPerformanceAlertEmailAsync(Guid migrationId, PerformanceAlert alert, List<string> recipients);
    
    // Slack/Teams integration
    Task SendSlackNotificationAsync(string channel, MigrationNotification notification);
    Task SendTeamsNotificationAsync(string webhookUrl, MigrationNotification notification);
    
    // Real-time alerts
    Task SendRealTimeAlertAsync(Guid migrationId, AlertSeverity severity, string message, Dictionary<string, object>? data = null);
    Task<List<RealTimeAlert>> GetActiveRealTimeAlertsAsync(Guid migrationId);
    Task ResolveRealTimeAlertAsync(Guid alertId);
    
    // Escalation procedures
    Task TriggerEscalationAsync(Guid migrationId, EscalationLevel level, string reason);
    Task<List<EscalationRule>> GetEscalationRulesAsync();
    Task UpdateEscalationRulesAsync(List<EscalationRule> rules);
    
    // Notification preferences
    Task<NotificationPreferences> GetNotificationPreferencesAsync(string userId);
    Task UpdateNotificationPreferencesAsync(string userId, NotificationPreferences preferences);
    
    // Notification history
    Task<List<NotificationHistory>> GetNotificationHistoryAsync(Guid migrationId);
    Task<NotificationDeliveryStatus> GetNotificationStatusAsync(Guid notificationId);
}

public class MigrationNotification
{
    public Guid NotificationId { get; set; }
    public Guid MigrationId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
    public List<NotificationChannel> Channels { get; set; } = new();
}

public enum NotificationType
{
    MigrationStarted,
    MigrationCompleted,
    MigrationFailed,
    MigrationPaused,
    PerformanceAlert,
    ErrorAlert,
    ProgressUpdate,
    EscalationTriggered
}

public enum NotificationChannel
{
    Email,
    Slack,
    Teams,
    SMS,
    WebPush,
    InApp
}

public class RealTimeAlert
{
    public Guid AlertId { get; set; }
    public Guid MigrationId { get; set; }
    public DateTime Timestamp { get; set; }
    public AlertSeverity Severity { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class EscalationRule
{
    public Guid RuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<EscalationTrigger> Triggers { get; set; } = new();
    public List<EscalationAction> Actions { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; }
}

public class EscalationTrigger
{
    public string TriggerType { get; set; } = string.Empty; // "AlertCount", "Duration", "ErrorRate", etc.
    public string Condition { get; set; } = string.Empty; // "GreaterThan", "LessThan", "Equals"
    public object Threshold { get; set; } = new();
    public TimeSpan TimeWindow { get; set; }
}

public class EscalationAction
{
    public string ActionType { get; set; } = string.Empty; // "Email", "Slack", "PauseExecution", "CallWebhook"
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
    public int DelayMinutes { get; set; }
}

public enum EscalationLevel
{
    Level1, // Team lead
    Level2, // Manager
    Level3, // Director
    Level4  // Emergency
}

public class NotificationPreferences
{
    public string UserId { get; set; } = string.Empty;
    public List<NotificationChannel> EnabledChannels { get; set; } = new();
    public Dictionary<NotificationType, bool> TypePreferences { get; set; } = new();
    public Dictionary<AlertSeverity, bool> SeverityPreferences { get; set; } = new();
    public string? EmailAddress { get; set; }
    public string? SlackUserId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool QuietHoursEnabled { get; set; }
    public TimeSpan QuietHoursStart { get; set; }
    public TimeSpan QuietHoursEnd { get; set; }
    public List<DayOfWeek> QuietHoursDays { get; set; } = new();
}

public class NotificationHistory
{
    public Guid NotificationId { get; set; }
    public Guid MigrationId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime SentAt { get; set; }
    public List<NotificationDelivery> Deliveries { get; set; } = new();
}

public class NotificationDelivery
{
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public NotificationDeliveryStatus Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

public enum NotificationDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Bounced,
    Unsubscribed
}

public class EmailTemplate
{
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
    public List<string> RequiredVariables { get; set; } = new();
}

public class SlackMessage
{
    public string Channel { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<SlackAttachment> Attachments { get; set; } = new();
    public string? IconEmoji { get; set; }
    public string? Username { get; set; }
}

public class SlackAttachment
{
    public string Color { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<SlackField> Fields { get; set; } = new();
    public long Timestamp { get; set; }
}

public class SlackField
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Short { get; set; }
}

public class TeamsMessage
{
    public string Type { get; set; } = "MessageCard";
    public string Context { get; set; } = "https://schema.org/extensions";
    public string Summary { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = string.Empty;
    public List<TeamsSection> Sections { get; set; } = new();
    public List<TeamsPotentialAction> PotentialAction { get; set; } = new();
}

public class TeamsSection
{
    public string ActivityTitle { get; set; } = string.Empty;
    public string ActivitySubtitle { get; set; } = string.Empty;
    public string ActivityImage { get; set; } = string.Empty;
    public List<TeamsFact> Facts { get; set; } = new();
    public string Text { get; set; } = string.Empty;
}

public class TeamsFact
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class TeamsPotentialAction
{
    public string Type { get; set; } = "OpenUri";
    public string Name { get; set; } = string.Empty;
    public List<TeamsTarget> Targets { get; set; } = new();
}

public class TeamsTarget
{
    public string Os { get; set; } = "default";
    public string Uri { get; set; } = string.Empty;
}