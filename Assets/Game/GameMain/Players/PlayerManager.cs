using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JSDK.Misc;
using External.JEichner.ooo;
using UnityEngine.Assertions;
using UnityEngine;

namespace Game.GameMain.Players
{
    class PlayerManager : Singleton<PlayerManager>, IBaseManager
    {
        private Player[] m_Players = new Player[(int) PlayerID.Count];

        ////////////////////////////////////////////////////////////////
        
        public ref Player CreatePlayer(PlayerID playerID)
        {
            Debug.Log("PlayerManager: Create Player " + (int) playerID);

            m_Players[(int) playerID] = new Player();
            m_Players[(int) playerID].Init(playerID);
            return ref m_Players[(int) playerID];
        }
        
        ////////////////////////////////////////////////////////////////

        public void DestroyPlayer(PlayerID playerID)
        {
            Debug.Log("PlayerManager: Destroy Player " + (int) playerID);

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
            List<Player> playerList = m_Players.ToList();
            io.Serialize("Players", ref playerList, false);
            m_Players               = playerList.ToArray();
        }

        ////////////////////////////////////////////////////////////////
        // Initialization & Destruction
        ////////////////////////////////////////////////////////////////

        public void OnInitialize()
        {
            Debug.Log("Initializing Player Manager ...");

            ////////////////////////////////////////////////////////////////
            
            m_Players = new Player[(int) PlayerID.Count];
        }

        ////////////////////////////////////////////////////////////////

        public void OnDestroy()
        {
            Debug.Log("Destructing Player Manager ...");
        }
    }
}
