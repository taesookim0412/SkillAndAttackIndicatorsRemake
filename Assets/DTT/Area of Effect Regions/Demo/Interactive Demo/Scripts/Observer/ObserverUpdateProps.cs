using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.DTT.Area_of_Effect_Regions.Demo.Interactive_Demo.Scripts.Observer
{
    public class ObserverUpdateProps
    {
        public long UpdateTickTime;
        public long UpdateTickTimeDeltaTime;

        public ObserverUpdateProps(long updateTickTime)
        {
            UpdateTickTime = updateTickTime;
        }

        public void Update_MainThread() 
        { 
            long newUpdateTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            UpdateTickTimeDeltaTime = newUpdateTickTime - UpdateTickTime;
            UpdateTickTime = newUpdateTickTime;
        }
    }
}
