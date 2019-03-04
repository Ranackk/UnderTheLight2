using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JSDK.Misc;
using External.JEichner.ooo;
using UnityEngine.Assertions;

namespace Game.GameMain.Players
{
    class PlayerManager : Singleton<PlayerManager>, IBaseManager
    {
        private Player[] m_Players = new Player[(int) PlayerID.Count];

        ////////////////////////////////////////////////////////////////
        
        public ref Player CreatePlayer(PlayerID playerID)
        {
            m_Players[(int) playerID].Init(playerID);
            return ref m_Players[(int) playerID];
        }
        
        ////////////////////////////////////////////////////////////////

        public void RemovePlayer(PlayerID playerID)
        {
            m_Players[(int) playerID].Destroy();
        }

        ////////////////////////////////////////////////////////////////
        
        public ref Player GetPlayer(PlayerID playerID)
        {
            Assert.IsTrue((int) playerID >= 0);   
            return ref m_Players[(int) playerID];
        }
        
        ////////////////////////////////////////////////////////////////
        // Serialization
        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("Players", ref m_Players);
        }

        ////////////////////////////////////////////////////////////////

        public void OnInitialize()
        {
        }

        ////////////////////////////////////////////////////////////////

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }
       
        ////////////////////////////////////////////////////////////////

        public string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
