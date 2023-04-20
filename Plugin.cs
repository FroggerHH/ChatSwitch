using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChatSwitch;

[BepInPlugin(ModGUID, ModName, ModVersion)]
internal class Plugin : BaseUnityPlugin
{
    #region values

    internal const string ModName = "ChatSwitch", ModVersion = "1.0.0", ModGUID = "com.Frogger." + ModName;
    internal static Harmony harmony = new(ModGUID);

    internal static Plugin _self;
    internal static bool inSMode = false;
    internal static bool inWMode = false;

    #endregion

    #region ConfigSettings

    #region tools

    static string ConfigFileName = "com.Frogger.ChatSwitch.cfg";
    DateTime LastConfigChange;

    public static readonly ConfigSync configSync = new(ModName)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

    private static ConfigEntry<Toggle> serverConfigLocked = null!;

    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigEntry<T> configEntry = _self.Config.Bind(group, name, value, description);

        SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    private ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    void SetCfgValue<T>(Action<T> setter, ConfigEntry<T> config)
    {
        setter(config.Value);
        config.SettingChanged += (_, _) => setter(config.Value);
    }

    public enum Toggle
    {
        On = 1,
        Off = 0
    }

    #endregion

    #region configs

    static ConfigEntry<string> moderatorUrlConfig;
    static ConfigEntry<string> logrUrlConfig;
    static ConfigEntry<string> languageServerConfig;
    static ConfigEntry<Toggle> preventItemDropPickupConfig;
    static ConfigEntry<Toggle> preventPickablePickupConfig;
    static ConfigEntry<Toggle> preventCraftingConfig;
    static ConfigEntry<float> webHookTimerConfig;
    static ConfigEntry<float> logWebHookTimerConfig;

    #endregion

    #region config values

    internal static string moderatorUrl = "";

    public static string languageServer = "English";

    //public static Localization localization = new();
    internal static string logrUrl = "";
    internal static bool preventItemDropPickup = false;
    internal static bool preventPickablePickup = false;
    internal static bool preventCrafting = false;
    internal static float webHookTimer = 2.5f;
    internal static float logWebHookTimer = 2f;

    #endregion

    #endregion

    #region Config

    private void SetupWatcher()
    {
        FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, ConfigFileName);
        fileSystemWatcher.Changed += ConfigChanged;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - LastConfigChange).TotalSeconds <= 5.0)
        {
            return;
        }

        LastConfigChange = DateTime.Now;
        try
        {
            Config.Reload();
        }
        catch
        {
            DebugError("Can't reload Config", true);
        }
    }

    private void UpdateConfiguration()
    {
        //if(Player.m_localPlayer)
        //{
        moderatorUrl = moderatorUrlConfig.Value;
        //logrUrl = logrUrlConfig.Value;
        languageServer = languageServerConfig.Value;
        //localization.SetupLanguage(languageServer);
        //preventItemDropPickup = preventItemDropPickupConfig.Value == Toggle.On;
        //preventPickablePickup = preventPickablePickupConfig.Value == Toggle.On;
        //preventCrafting = preventCraftingConfig.Value == Toggle.On;
        //webHookTimer = webHookTimerConfig.Value;
        //logWebHookTimer = logWebHookTimerConfig.Value;

        moderatorUrl = moderatorUrl.Replace(" ", "");

        //if (logrUrl != string.Empty) logrUrl = logrUrl.Replace(" ", "");

        //InvokeRepeating("LogWebHookTimer", logWebHookTimer, logWebHookTimer);

        Debug("Configuration Received");
        //}
    }

    #endregion

    #region tools

    public static void Debug(string msg, bool localize = false)
    {
        if (Localization.instance != null && localize)
        {
            _self.Logger.LogInfo(Localization.instance.Localize(msg));
        }
        else
        {
            _self.Logger.LogInfo(msg);
        }
    }

    public void DebugError(string msg, bool showWriteToDev)
    {
        if (showWriteToDev)
        {
            msg += "Write to the developer and moderator if this happens often.";
        }

        Logger.LogError(msg);
    }

    public void DebugWarning(string msg, bool showWriteToDev)
    {
        if (showWriteToDev)
        {
            msg += "Write to the developer and moderator if this happens often.";
        }

        Logger.LogWarning(msg);
    }

    #endregion

    private void Awake()
    {
        _self = this;

        #region config

        Config.SaveOnConfigSet = false;

        configSync.AddLockingConfigEntry(config("Main", "Lock Configuration", Toggle.On,
            "If on, the configuration is locked and can be changed by server admins only."));
        
        Config.SaveOnConfigSet = true;

        #endregion

        SetupWatcher();
        Config.SettingChanged += (_, _) => { UpdateConfiguration(); };
        Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };

        Config.Save();

        harmony.PatchAll();
    }
}