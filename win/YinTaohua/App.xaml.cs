using Steamworks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace YinTaohua
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Timer timer;
        protected override void OnStartup(StartupEventArgs e)
        {
            if (SteamAPI.RestartAppIfNecessary((AppId_t)2116000)) Shutdown();
            SteamAPI.Init();
            timer = new Timer((s) => { SteamAPI.RunCallbacks(); }, null, 30, 30);
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            timer.Dispose();
            SteamAPI.Shutdown();
            base.OnExit(e);
        }
    }
}
