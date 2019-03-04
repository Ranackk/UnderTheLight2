using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using External.JEichner.ooo;
using Game.GameMain.Bridges;
using Game.GameMain.Players;

namespace Game.Core
{
    /// <summary>
    /// The Games Heart.
    /// Needs to be Initialized from the earliest script in execution order!
    /// </summary>
    public static class GameManager
    {
        static volatile bool m_Initialized = false;
        static string saveGameFilePath = @"C:\Users\Ranack\Desktop\UTL_SaveGame.txt";

        static GameManager() { }

        #region Entry & Exit
                
        ////////////////////////////////////////////////////////////////
        // Entry Point, Exit Point
        ////////////////////////////////////////////////////////////////
        
        public static void OnStart()
        {
            AssureInitialization();
        }

        public static void OnDestroy()
        {
            Destroy();
        }
        
        #endregion

        #region Initialize & Destroy
            #region Events

        ////////////////////////////////////////////////////////////////
        // Events
        ////////////////////////////////////////////////////////////////
        
        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnUnityReloadScripts()
        {
            if (m_Initialized)
            {
                Destroy();
            }

            ////////////////////////////////////////////////////////////////
            
            AssureInitialization();
        }

        ////////////////////////////////////////////////////////////////
        
        static void OnDomainUnload(object sender, EventArgs e)
        {
            AssureDestroy();
        }

            #endregion
            #region Assurance Helpers
        ////////////////////////////////////////////////////////////////
        /// Initialize & Destroy - Helper
        ////////////////////////////////////////////////////////////////

        static void AssureInitialization()
        {
            if (m_Initialized)
            {
                return;
            }

            ////////////////////////////////////////////////////////////////
            
            Initialize();
        }

        ////////////////////////////////////////////////////////////////
       
        static void AssureDestroy()
        {
            if (!m_Initialized)
            {
                return;
            }

            ////////////////////////////////////////////////////////////////
            
            Destroy();
        }
        
            #endregion
            #region Main
        ////////////////////////////////////////////////////////////////
        /// Initialize & Destroy - Main
        ////////////////////////////////////////////////////////////////
        
        static void Initialize()
        {
            Debug.Assert(!m_Initialized, "GameManager already initialized!");

            ////////////////////////////////////////////////////////////////
            
            Debug.Log("Initializing Game Manager ...");
            
            ////////////////////////////////////////////////////////////////
            /// Callbacks
            ////////////////////////////////////////////////////////////////
            
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
            
            ////////////////////////////////////////////////////////////////
            /// Managers
            ////////////////////////////////////////////////////////////////
           
            InitializeManagers();

            m_Initialized = true;

            ////////////////////////////////////////////////////////////////
            
            Debug.Log("... Game Manager initialized!");
        }

        ////////////////////////////////////////////////////////////////

        static void Destroy()
        {            
            Debug.Assert(m_Initialized, "GameManager already destroyed!");

            ////////////////////////////////////////////////////////////////
            
            Debug.Log("Destructing Game Manager ...");

            ////////////////////////////////////////////////////////////////
            /// Callbacks
            ////////////////////////////////////////////////////////////////
            
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            
            ////////////////////////////////////////////////////////////////
            /// Managers
            ////////////////////////////////////////////////////////////////
           
            DestroyManagers();

            m_Initialized = false;

            ////////////////////////////////////////////////////////////////
            
            Debug.Log("... Game Manager destroyed!");
        }
        
        private static void InitializeManagers()
        {
            BridgeManager.Instance.OnInitialize();
        }

        ////////////////////////////////////////////////////////////////

        private static void DestroyManagers()
        {
            BridgeManager.Instance.OnDestroy();
        }

            #endregion
        #endregion

        #region Serialization
        ////////////////////////////////////////////////////////////////
        // Serialization
        ////////////////////////////////////////////////////////////////

        public static void SaveGame()
        {
            Serializer serializer = new Serializer();
            serializer.BeginSaving();

            ////////////////////////////////////////////////////////////////

            BridgeManager.Instance.Serialize(serializer);
            PlayerManager.Instance.Serialize(serializer);

            ////////////////////////////////////////////////////////////////

            byte[] saveGameBlob = serializer.EndSaving();
            
            ////////////////////////////////////////////////////////////////

            System.IO.File.WriteAllBytes(saveGameFilePath, saveGameBlob);
            Debug.Log("Saving finished!");
        }

        public static void LoadGame()
        {
            if (!System.IO.File.Exists(saveGameFilePath))
            {
                Debug.Log("No save game found!");
                return;
            }

            byte[] saveGameBlob = System.IO.File.ReadAllBytes(saveGameFilePath);

            ////////////////////////////////////////////////////////////////

            Serializer serializer = new Serializer();
            serializer.BeginLoading(saveGameBlob);

            ////////////////////////////////////////////////////////////////

            PlayerManager.Instance.Serialize(serializer);
            BridgeManager.Instance.Serialize(serializer);

            ////////////////////////////////////////////////////////////////

            serializer.EndLoading();
        }

        #endregion
    }
}

