using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
namespace GameServer.Net
{
    class ClientState
    {
        public Socket socket;
        public ByteArray readBuff = new ByteArray();
        public long lastPingTime = 0;

        public ClientState()
        {
            lastPingTime = NetManager.GetTimeStamp();
        }
    }
}
