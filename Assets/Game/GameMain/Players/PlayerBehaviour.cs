using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMain.Players
{
    public class PlayerBehaviour : MonoBehaviour
    {
        public PlayerID             m_PlayerID = PlayerID.Invalid;
        [HideInInspector] Player    m_Player;

        ////////////////////////////////////////////////////////////////
        
        void Start()
        {
            PlayerManager.Instance.CreatePlayer(m_PlayerID);
        }
        
    }
}
