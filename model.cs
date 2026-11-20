/****************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
****************/ 

using System;

#region Mo hinh khung

public interface ISensor{
    double ReadValue();
    string GetUnit();
}

public sealed record AlertEvent(
    string SensorName,
    double Value,
    string Unit,
    string Severity,
    DateTime OccurredAt
);

#endregion

#region Adapter

public sealed class LegacySmokeDetector{

    private int smokeLevel;

    public LegacySmokeDetector(int smokeLevel){
        this.smokeLevel = smokeLevel;
    }

    public int GetSmokeLevel() => smokeLevel; // 0..1000
    public string GetDescription() => "Legacy smoke detector";
    public void SetSmokeLevel(int value){
        smokeLevel = Math.Clamp(value, 0, 1000);
    }
}

public sealed class SmokeSensorAdapter: ISensor
{
    private readonly LegacySmokeDetector legacy;

    public SmokeSensorAdapter(LegacySmokeDetector legacy)
    {
        this.legacy = legacy ??
            throw new ArgumentNullException(nameof(legacy));
    }

    public double ReadValue()
    {
        return legacy.GetSmokeLevel() / 10.0;
    }

    public string GetUnit() => "% smoke";
}
#endregion

#region Observer

public interface IAlertObserver
{
    void Update(AlertEvent alertEvent);
}

public sealed class SensorMonitor
{
    private readonly List<IAlertObserver> observers = new();
    public void Attach(IAlertObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void Detach(IAlertObserver observer)
    {
        observers.Remove(observer);
    }

    public void CheckSensor(string sensorName, ISensor sensor)
    {
        double value = sensor.ReadValue();
        string? severity = value switch
        {
            >= 70 => "CRITICAL",
            >= 50 => "WARNING",
            _ => null
        };

        if (severity is null)
        {
            return;
        }

        var alertEvent = new AlertEvent(
            sensorName,
            value,
            sensor.GetUnit(),
            severity,
            DateTime.Now
        );

        Notify(alertEvent);
    }

    private void Notify(AlertEvent alertEvent)
    {
        foreach (IAlertObserver observer in observers.ToArray())
        {
            try
            {
                observer.Update(alertEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Observer '{observer.GetType().Name}' gặp lỗi: {ex.Message}");
            }
        }
    }
}

public sealed class DashboardObserver : IAlertObserver
{
    public void Update(AlertEvent alertEvent)
    {
        Console.WriteLine(
            $"[DASHBOARD] {alertEvent.SensorName}: " +
            $"{alertEvent.Value:F1}{alertEvent.Unit} " +
            $"[{alertEvent.Severity}]");
    }
}

public sealed class EmailAlertObserver : IAlertObserver
{
    private readonly string recipientEmail;

    public EmailAlertObserver(string recipientEmail = "safety-team@campus.edu.vn")
    {
        this.recipientEmail = recipientEmail;
    }

    public void Update(AlertEvent alertEvent)
    {
        Console.WriteLine(
            $"[EMAIL -> {recipientEmail}] Cảnh báo từ {alertEvent.SensorName}: " +
            $"{alertEvent.Value:F1}{alertEvent.Unit} [{alertEvent.Severity}] lúc {alertEvent.OccurredAt:yyyy-MM-dd HH:mm:ss}");
    }
}

public sealed class AuditLogObserver : IAlertObserver
{
    private readonly List<AlertEvent> logs = new();
    public IReadOnlyList<AlertEvent> Logs => logs.AsReadOnly();

    public void Update(AlertEvent alertEvent)
    {
        logs.Add(alertEvent);
        Console.WriteLine(
            $"[AUDIT LOG] [{alertEvent.OccurredAt:yyyy-MM-dd HH:mm:ss}] " +
            $"Sensor: {alertEvent.SensorName} | Value: {alertEvent.Value:F1}{alertEvent.Unit} | Severity: {alertEvent.Severity}");
    }
}

public sealed class EmergencyBridgeObserver : IAlertObserver
{
    private readonly IEmergencyMediator mediator;

    public EmergencyBridgeObserver(IEmergencyMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public void Update(AlertEvent alertEvent)
    {
        // Khi mức độ cảnh báo là CRITICAL, chuyển thành sự kiện khẩn cấp cho Mediator
        if (string.Equals(alertEvent.Severity, "CRITICAL", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                $"[BRIDGE] Nhận cảnh báo CRITICAL từ {alertEvent.SensorName} ({alertEvent.Value:F1}{alertEvent.Unit}). " +
                "Chuyển tiếp sự kiện 'FIRE_DETECTED' tới EmergencyMediator...");
            mediator.Notify(this, "FIRE_DETECTED");
        }
    }
}

#endregion

#region Mediator

public interface IEmergencyMediator
{
    void Notify(object sender, string eventCode);
}

public sealed class Alarm
{
    public bool IsTriggered { get; private set; }

    public void Trigger()
    {
        IsTriggered = true;
        Console.WriteLine("[ALARM] Kích hoạt còi hú báo cháy khẩn cấp!");
    }

    public void Reset()
    {
        IsTriggered = false;
        Console.WriteLine("[ALARM] Đã tắt còi báo động.");
    }
}

public sealed class DoorController
{
    public bool AreDoorsUnlocked { get; private set; }

    public void UnlockEmergencyDoors()
    {
        AreDoorsUnlocked = true;
        Console.WriteLine("[DOOR] Đã mở khóa toàn bộ cửa thoát hiểm!");
    }

    public void LockDoors()
    {
        AreDoorsUnlocked = false;
        Console.WriteLine("[DOOR] Đã khóa lại các cửa thoát hiểm.");
    }
}

public sealed class VentilationController
{
    public bool IsExhaustActive { get; private set; }

    public void ActivateExhaust()
    {
        IsExhaustActive = true;
        Console.WriteLine("[VENTILATION] Bật hệ thống hút khói và thông gió công suất tối đa!");
    }

    public void StopExhaust()
    {
        IsExhaustActive = false;
        Console.WriteLine("[VENTILATION] Tắt hệ thống hút khói.");
    }
}

public sealed class EmergencyMediator : IEmergencyMediator
{
    private readonly Alarm alarm;
    private readonly DoorController doorController;
    private readonly VentilationController ventilationController;

    public EmergencyMediator(Alarm alarm, DoorController door, VentilationController vent)
    {
        this.alarm = alarm ?? throw new ArgumentNullException(nameof(alarm));
        this.doorController = door ?? throw new ArgumentNullException(nameof(door));
        this.ventilationController = vent ?? throw new ArgumentNullException(nameof(vent));
    }

    public void Notify(object sender, string eventCode)
    {
        Console.WriteLine($"[MEDIATOR] Nhận thông báo sự kiện '{eventCode}' từ {sender.GetType().Name}. Bắt đầu điều phối các thiết bị ứng phó:");

        switch (eventCode)
        {
            case "FIRE_DETECTED":
                alarm.Trigger();
                doorController.UnlockEmergencyDoors();
                ventilationController.ActivateExhaust();
                break;

            case "ALL_CLEAR":
            case "RESET":
                alarm.Reset();
                doorController.LockDoors();
                ventilationController.StopExhaust();
                break;

            default:
                Console.WriteLine($"[MEDIATOR] Không có quy trình xử lý cho sự kiện: '{eventCode}'");
                break;
        }
    }
}

#endregion