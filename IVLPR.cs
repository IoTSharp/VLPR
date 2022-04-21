
internal interface IVLPR
{
    string IPAddress { get; }
    string Name { get; }

    event EventHandler<VehicleInfo> FoundVehicle;

    bool Capture();
    bool CheckStatus();
    void Dispose();
    void EventHandle();
    bool Init();
}