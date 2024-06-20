using UnityEngine;
using Unity.NetCode;
using UnityEngine.Scripting;

[Preserve]//make sure Unity wont ignore the code on build
public class AutoConnectBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 0;//make sure autoConnect wont happen
        return false;
    }
}