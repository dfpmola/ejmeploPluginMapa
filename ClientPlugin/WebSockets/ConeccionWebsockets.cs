using System;
using System.Threading.Tasks;
using SocketIOClient;

namespace ClientPlugin.WebSockets;

public class ConeccionWebsockets
{
    protected SocketIO client;
     public   ConeccionWebsockets  (string url ,string port)
    {
        String uri = String.Format("{0}:{1}",url,port);  
        client = new SocketIO("http://127.0.0.1:3000/");
        
        client.OnConnected += async (sender, e) =>
        {
            // Emit a string
            await client.EmitAsync("connection", "Conexion establecida");
        };
        
    }
     public async void conectarCliente()  
     {       
         await this.client.ConnectAsync();
     } 
     

     public async void enviarDatos(string evento ,string dts)
     {
         await client.EmitAsync(evento, dts);
     }
    
}