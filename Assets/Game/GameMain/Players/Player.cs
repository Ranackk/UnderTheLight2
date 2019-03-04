using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using External.JEichner.ooo;

namespace Game.GameMain.Players
{
    public enum PlayerID
    {
        Invalid = -1,

        Blue    = 0,
        Red     = 1,
        Green   = 2,
        Yellow  = 3,

        Count   = 4
    }

    ////////////////////////////////////////////////////////////////

    public class Player
    {
        PlayerID m_PlayerID;
    
        ////////////////////////////////////////////////////////////////
    
        public void Init(PlayerID playerID)
        {
            m_PlayerID = playerID;
        }

        ////////////////////////////////////////////////////////////////
        
        public void Destroy()
        {

        }
               
        ////////////////////////////////////////////////////////////////
        // Serialization
        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("PlayerID", ref m_PlayerID, PlayerID.Invalid);
        }

        ////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////
    
}
