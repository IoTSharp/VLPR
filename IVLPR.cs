﻿
internal interface IVLPR
{
    string IPAddress { get; }
    string Name { get; }

    event EventHandler<VehicleInfo> FoundVehicle;

    bool Capture(int laneid,int index);
    bool Capture();
    bool CheckStatus();
    void Dispose();
    bool Load();
}