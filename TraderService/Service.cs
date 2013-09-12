using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Trader.Server;

namespace TraderService
{
    partial class Service : ServiceBase
    {
        private TraderServer _TraderServer = new TraderServer();
        public Service()
        {
            InitializeComponent();
        }
        static void Main()
        {
            ServiceBase.Run(new Service());
        }

        protected override void OnStart(string[] args)
        {
            this._TraderServer.Start();
        }

        protected override void OnStop()
        {
            this._TraderServer.Stop();
        }
    }
}
