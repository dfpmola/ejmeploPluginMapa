using System;
using System.Text;
using ParallelTasks;
using SuperSimpleTcp;
using VRage.Collections;
using VRage.Game.ModAPI;

namespace ClientPlugin.ObjectosParalell;

public class SSTConnection
{
    static public string ipClinet;
    public static void DoWork(WorkData workData)
    {
        var data = workData as SSTConnectionData;
        if (data == null)
            return;
        data.server = new SimpleTcpServer("127.0.0.1:9000");
        data.server.Events.ClientConnected += ClientConnected;
        data.server.Events.ClientDisconnected += ClientDisconnected;
        data.server.Events.DataReceived += DataReceived;
        
        data.server.Start();
        
    }
    public class SSTConnectionData : WorkData
    {
        public SimpleTcpServer server;

        public SSTConnectionData(SimpleTcpServer server)
        {
            this.server = server;
        }
    }
    
    static void ClientConnected(object sender, ConnectionEventArgs e)
    {
        ipClinet = e.IpPort.ToString();
    }

    static void ClientDisconnected(object sender, ConnectionEventArgs e)
    {
    }

    static void DataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine($"[{e.IpPort}]: {Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count)}");
    }
}