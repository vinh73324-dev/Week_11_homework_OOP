/****************
Mã sinh viên: 202419018
Họ tên: Phạm Văn Vinh
****************/ 

using System;

class Program
{
    static void Main(string[] args)
    {
        // T1. Kiem tra SmokeSensorAdapter (chuyen doi 0..1000 sang 0..100% va lay don vi)
        Console.WriteLine("T1. Kiem tra SmokeSensorAdapter");
        var legacyDetector = new LegacySmokeDetector(735);
        ISensor sensor = new SmokeSensorAdapter(legacyDetector);
        Console.WriteLine($"Gia tri doc: {sensor.ReadValue():F1}");
        Console.WriteLine($"Don vi do: {sensor.GetUnit()}");

        // T2. Kiem tra ngoai le khi khoi tao Adapter voi doi tuong null
        SeperateLine();
        Console.WriteLine("T2. Kiem tra ngoai le khi khoi tao Adapter voi doi tuong null");
        try
        {
            var invalidAdapter = new SmokeSensorAdapter(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Da bat ngoai le: {ex.GetType().Name}");
            Console.WriteLine($"Thong bao: Tham so khong duoc null ({ex.ParamName})");
        }

        // T3. Kiem tra SensorMonitor voi muc khoi duoi nguong (35.0% - khong phat canh bao)
        SeperateLine();
        Console.WriteLine("T3. Kiem tra SensorMonitor voi muc khoi duoi nguong");
        var monitor = new SensorMonitor();
        var dashboard = new DashboardObserver();
        var email = new EmailAlertObserver("admin-datacenter@campus.edu.vn");
        var audit = new AuditLogObserver();

        monitor.Attach(dashboard);
        monitor.Attach(email);
        monitor.Attach(audit);

        legacyDetector.SetSmokeLevel(350);
        Console.WriteLine($"Kiem tra cam bien voi muc khoi: {sensor.ReadValue():F1}%");
        monitor.CheckSensor("SmokeSensor_RoomA", sensor);
        Console.WriteLine("(Khong phat canh bao)");

        // T4. Kiem tra muc canh bao WARNING (58.0% - tu 50% den duoi 70%)
        SeperateLine();
        Console.WriteLine("T4. Kiem tra muc canh bao WARNING");
        legacyDetector.SetSmokeLevel(580);
        monitor.CheckSensor("SmokeSensor_RoomA", sensor);

        // T5. Kiem tra muc CRITICAL va kich hoat dieu phoi khan cap qua EmergencyBridgeObserver va Mediator
        SeperateLine();
        Console.WriteLine("T5. Kiem tra muc CRITICAL va kich hoat EmergencyMediator");
        var alarm = new Alarm();
        var door = new DoorController();
        var vent = new VentilationController();
        var mediator = new EmergencyMediator(alarm, door, vent);
        var bridge = new EmergencyBridgeObserver(mediator);

        monitor.Attach(bridge);
        legacyDetector.SetSmokeLevel(820);
        monitor.CheckSensor("SmokeSensor_RoomA", sensor);

        // T6. Kiem tra Mediator xu ly su kien ALL_CLEAR de reset thiet bi
        SeperateLine();
        Console.WriteLine("T6. Kiem tra Mediator xu ly su kien ALL_CLEAR");
        mediator.Notify(new object(), "ALL_CLEAR");
    }

    static void SeperateLine()
    {   
        Console.WriteLine(new string('-', 40));
    }
}
