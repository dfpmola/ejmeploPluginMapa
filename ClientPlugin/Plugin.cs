using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ClientPlugin.GUI;
using ClientPlugin.WebSockets;
using HarmonyLib;
using ParallelTasks;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using Shared.Config;
using Shared.Logging;
using Shared.Patches;
using Shared.Plugin;
using VRage.FileSystem;
using VRage.Game.ModAPI;
using VRage.Plugins;
using VRageMath;

using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using ClientPlugin.ObjectosParalell;

using SuperSimpleTcp;


namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, ICommonPlugin
    {
        public const string Name = "PluginTemplate";
        public static Plugin Instance { get; private set; }

        public long Tick { get; private set; }
        public long TickControll { get; private set; }
        public IPluginLogger Log => Logger;
        private static readonly IPluginLogger Logger = new PluginLogger(Name);

        public IPluginConfig Config => config?.Data;
        private PersistentConfig<PluginConfig> config;
        private static readonly string ConfigFileName = $"{Name}.cfg";
        
        private IMyPlayer jugadorEntidad;
        private Boolean primerInicio = true;
        public ConeccionWebsockets conectionWeb;
        public SimpleTcpServer server;
        public Process myProcess;
        

        private static bool initialized;
        private static bool failed;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            Log.Info("Loading");

            var configPath = Path.Combine(MyFileSystem.UserDataPath, ConfigFileName);
            config = PersistentConfig<PluginConfig>.Load(Log, configPath);

            Common.SetPlugin(this);

            if (!PatchHelpers.HarmonyPatchAll(Log, new Harmony(Name)))
            {
                failed = true;
                return;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {
            try
            {
                // TODO: Save state and close resources here, called when the game exists (not guaranteed!)
                // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.
                server.Send(SSTConnection.ipClinet, "Cerrando");
                myProcess.Kill();

                
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Dispose failed");
            }

            Instance = null;
        }

        public void Update()
        {
            EnsureInitialized();
            try
            {
                if (!failed)
                {
                    CustomUpdate();
                    Tick++;
                }
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Update failed");
                failed = true;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized || failed)
                return;

            Log.Info("Initializing");
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Log.Critical(ex, "Failed to initialize plugin");
                failed = true;
                return;
            }

            Log.Debug("Successfully initialized");
            initialized = true;
        }

        private  void Initialize()
        {
            

        }
        
        
        public  void  conectarCliente(WorkData workData)
        {
            // TODO: Put your one time initialization code here. It is executed on first update, not on loading the plugin.
            
        }
        private void CustomUpdate()
        {
            if ( MySession.Static == null)
            {
                return;
            } // Ha entrado en partida, porque se puede parar
            if (!MySession.Static.IsPausable())
            {
                return;
            }
            if (primerInicio == true)
            {
                TickControll = Tick;
                jugadorEntidad = MyAPIGateway.Session.Player;
                primerInicio = false;
                
                var datos = new SSTConnection.SSTConnectionData(server);
                MyAPIGateway.Parallel.Start(SSTConnection.DoWork, Callback, datos);
                
                
            }

            if ((Tick-TickControll) <= 300)
            {
                return;
            }
            TickControll = Tick;
            // TODO: Put your update code here. It is called on every simulation frame!
            Vector3D gpsJugador = jugadorEntidad.GetPosition();

            Vector3D camara = MyAPIGateway.Session.Player.Controller.ControlledEntity.GetHeadMatrix(true,true).GetOrientation().Forward;
            
            Vector3D camaraItento2 = MyAPIGateway.Session.Player.Character.GetHeadMatrix(true,true,true).Forward;
            
            var myObject = new ObjectoEjemplo(camaraItento2.X + "|" + camaraItento2.Y + "|" + camaraItento2.Z,gpsJugador.X + "|" + gpsJugador.Y + "|" + gpsJugador.Z ,camara.X + "|" + camara.Y + "|" + camara.Z);
            
            string jsonString = JsonSerializer.Serialize(myObject);
        
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            server.Send(SSTConnection.ipClinet, "Hello, world!2");

                //nviarDatos("actualizacion",new DateTime().ToString() + "|" + gpsJugador.X + "|" + gpsJugador.Y + "|" + gpsJugador.Z );

        }
        private void Callback(WorkData workData)
        {
            var data = workData as SSTConnection.SSTConnectionData;
            if (data == null)
                return;
            
             myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = "C:\\Users\\Usuario\\RiderProjects\\ejmeploPluginMapa\\Bin64\\Plugins\\Local\\WebApplication1.exe";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();
            this.server = data.server;
        }
        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginConfigDialog());
        }
    }
    public class ObjectoEjemplo
    {
        public string data1;
        public string data2;
        public string data3;
        public ObjectoEjemplo(string data1,string data2, string data3)
        {
            this.data1 = data1;
            this.data2 = data2;
            this.data3 = data3;
        }

        public string Data1
        {
            get => data1;
            set => data1 = value;
        }

        public string Data2
        {
            get => data2;
            set => data2 = value;
        }

        public string Data3
        {
            get => data3;
            set => data3 = value;
        }
    }
}

