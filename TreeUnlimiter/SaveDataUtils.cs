﻿using ColossalFramework;
using ColossalFramework.Packaging;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TreeUnlimiter.OptionsFramework;
using UnityEngine;

namespace TreeUnlimiter
{
	internal static class SaveDataUtils
	{

        public static bool ListDataKeysToLog()
        {
            bool result = false;
            try
            {

                SimulationManager SimMgr;
                if (Singleton<SimulationManager>.exists)
                { SimMgr = Singleton<SimulationManager>.instance; }
                else
                {
                    Logger.dbgLog("SimMgr is null or does not exist");
                    return false;
                }
                if (SimMgr.m_serializableDataStorage == null)
                {
                    Logger.dbgLog("m_serializableDataStorage null or does not exist");
                    return false;
                }
                while (!Monitor.TryEnter(SimMgr.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }

                try
                {
                    Logger.dbgLog(string.Format("---  m_serializableDataStorage contains {0} entries: ---", SimMgr.m_serializableDataStorage.Count.ToString()));
                    if (SimMgr.m_serializableDataStorage.Count > 0)
                    {
                        int i = 0;
                        foreach( KeyValuePair<string,byte[]> kvp in SimMgr.m_serializableDataStorage)
                        {
                            Logger.dbgLog(string.Format("entry:{0} name:{1} #bytes:{2}", i.ToString(), kvp.Key.ToString(), kvp.Value.Length.ToString()));
                            i++;
                        }
                    }
                    result = true;
                }
                finally
                {
                    Monitor.Exit(SimMgr.m_serializableDataStorage);
                }
            }
            catch (Exception ex)
            {
                Logger.dbgLog("Error while listing custom data keys.  ",ex);
            }
            return result;
        }


        public static bool EraseBytesFromNamedKey(string id)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    Logger.dbgLog("id was null string.");
                    return false;
                }

                SimulationManager SimMgr;
                if (Singleton<SimulationManager>.exists)
                { SimMgr = Singleton<SimulationManager>.instance; }
                else
                {
                    Logger.dbgLog("SimMgr is null or does not exist");
                    return false;
                }
                if (SimMgr.m_serializableDataStorage == null)
                {
                    Logger.dbgLog("m_serializableDataStorage null or does not exist");
                    return false;
                }

                while (!Monitor.TryEnter(SimMgr.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }

                try
                {
                    if (SimMgr.m_serializableDataStorage != null)
                    {
                        if (SimMgr.m_serializableDataStorage.Remove(id))
                        {
                            result = true;
                            if (OptionsWrapper<Configuration>.Options.IsLoggingEnabled()) { Logger.dbgLog(string.Format("Successfully removed data from  {0} in m_serializableDataStorage dictionary.", id)); }
                        }
                        else
                        {
                            if(OptionsWrapper<Configuration>.Options.IsLoggingEnabled() && OptionsWrapper<Configuration>.Options.DebugLoggingLevel > 1) { Logger.dbgLog(string.Format(".Remove() Could not locate {0} in m_serializableDataStorage dictionary.",id));}
                        }
                    }
                    else
                    { Logger.dbgLog("m_serializableDataStorage was null"); result = false; }
                }
                finally
                {
                    Monitor.Exit(SimMgr.m_serializableDataStorage);
                }
            }
            catch (Exception ex)
            { Logger.dbgLog(ex.ToString());}

            return result;
        }


        public static byte[] ReadBytesFromNamedKey(string id)
        {
//            Logger.dbgLog("threadname: " + Thread.CurrentThread.Name);

            if (string.IsNullOrEmpty(id))
            {
                Logger.dbgLog("id was null string.");
                return null;
            }

            SimulationManager SimMgr;
            if (Singleton<SimulationManager>.exists)
            { SimMgr = Singleton<SimulationManager>.instance; }
            else
            {
                Logger.dbgLog("SimMgr is null or does not exist");
                return null;
            }
            if (SimMgr.m_serializableDataStorage == null)
            {
                Logger.dbgLog("m_serializableDataStorage null or does not exist");
                return null;
            }

            if (OptionsWrapper<Configuration>.Options.IsLoggingEnabled() && OptionsWrapper<Configuration>.Options.DebugLoggingLevel > 1) { Logger.dbgLog("Now Loading " + id); }

            while (!Monitor.TryEnter(SimMgr.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            byte[] result = null ;
            try
            {
                try
                {
                    byte[] array;
                    if (SimMgr.m_serializableDataStorage != null)
                    {
                        if (SimMgr.m_serializableDataStorage.TryGetValue(id, out array))
                        {
                            result = array;
                        }
                        else
                        {
                            result = null;
                            if(OptionsWrapper<Configuration>.Options.IsLoggingEnabled() && OptionsWrapper<Configuration>.Options.DebugLoggingLevel > 1) { Logger.dbgLog(string.Format("Could not locate {0} in m_serializableDataStorage dictionary.",id));}
                        }
                    }
                    else
                    { Logger.dbgLog("m_serializableDataStorage was null"); result = null; }
                }
                finally
                {
                    Monitor.Exit(SimMgr.m_serializableDataStorage);
                }
            }
            catch (Exception ex)
            { Logger.dbgLog(ex.ToString()); result = null; }

            return result;
        }


        public static bool WriteBytesToNamedKey(string id, byte[] theBytes)
        {
  //          Logger.dbgLog("threadname: " + Thread.CurrentThread.Name);
 //           Logger.dbgLog(DateTime.Now.ToString(Mod.DTMilli));
            if (string.IsNullOrEmpty(id))
            {
                Logger.dbgLog("id was null string.");
                return false;
            }

            if (theBytes == null || theBytes.Length > 16700000) //16711680 
            {
                Logger.dbgLog("Error - The array you are trying to save is > than the allowed max size of approximately 16.7 million bytes.");
                return false;
            }
            SimulationManager SimMgr;
            if(Singleton<SimulationManager>.exists)
            { SimMgr = Singleton<SimulationManager>.instance; }
            else
            {
                Logger.dbgLog("SimMgr is null or does not exist");
                return false;
            }
            if (SimMgr.m_serializableDataStorage == null)
            {
                Logger.dbgLog("m_serializableDataStorage null or does not exist");
                return false;
            }

            if (OptionsWrapper<Configuration>.Options.IsLoggingEnabled() && OptionsWrapper<Configuration>.Options.DebugLoggingLevel > 1) { Logger.dbgLog("Now saving " + id); }

            while (!Monitor.TryEnter(SimMgr.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            bool retval = false;
            try
            {
                try
                {
                    SimMgr.m_serializableDataStorage[id] = theBytes;
                    if(SimMgr.m_serializableDataStorage.ContainsKey(id))
                    {
                        if (OptionsWrapper<Configuration>.Options.IsLoggingEnabled()) { Logger.dbgLog("Saved " + id + " to storage. (key exists)"); }
                        retval = true;
                    }
                    else
                    {  Logger.dbgLog("Could not locate " + id + " aftersave.");}
                }
                finally
                {
                    Monitor.Exit(SimMgr.m_serializableDataStorage);
                }
            }
            catch(Exception ex)
            { Logger.dbgLog(ex.ToString()); retval = false; }
            return retval;
        }


	}
}
