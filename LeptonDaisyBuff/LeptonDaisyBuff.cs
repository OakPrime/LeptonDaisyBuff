using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using R2API;
using R2API.Utils;
using System.Collections.Generic;
using UnityEngine;
using EntityStates;
using IL.RoR2.Projectile;
using RoR2.Projectile;
using On.RoR2.Projectile;
using ProjectileDotZone = RoR2.Projectile.ProjectileDotZone;
using UnityEngine.UIElements;
using BepInEx.Logging;

namespace LeptonDaisyBuff
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class HuntersHarpoonRework : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "LeptonDaisyBuff";
        public const string PluginVersion = "0.1.0";

        private readonly Dictionary<string, string> DefaultLanguage = new Dictionary<string, string>();

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            try
            {
                IL.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.HealPulse.Update += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdarg(out _),
                        x => x.MatchLdloc(out _)
                    );
                    c.Index += 3;
                    c.Emit(OpCodes.Ldloc_2);
                    c.EmitDelegate<Action<RoR2.HealthComponent>>(healthComponent =>
                    {
                        Util.CleanseBody(healthComponent.body, true, false, true, true, true, false);
                    });

                };

                IL.EntityStates.TeleporterHealNovaController.TeleporterHealNovaGeneratorMain.CalculatePulseCount += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdloc(out _),
                        x => x.MatchRet()
                    );
                    c.Index++;
                    c.EmitDelegate<Func<int, int>>(num =>
                    {
                        if (num > 0)
                        {
                            return num + 1;
                        }
                        else
                        {
                            return num;
                        }
                    });
                };

            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            };
        }
        private void ReplaceSecondaryText()
        {
            this.ReplaceString("HUNTRESS_SECONDARY_DESCRIPTION", "Throw a seeking glaive that bounces up to <style=cIsDamage>6</style> times for <style=cIsDamage>250% damage</style>" +
                ". Damage increases by <style=cIsDamage>15%</style> per bounce.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
