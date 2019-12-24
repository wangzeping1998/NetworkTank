using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Net;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NetManager.StartLoop(8888);
        }
    }
}
