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
        public const string PluginVersion = "1.0.0";

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
                        Util.CleanseBody(healthComponent.body, true, false, true, true, true, true);
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
                this.UpdateText();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            };
        }
        private void UpdateText()
        {
            this.ReplaceString("ITEM_TPHEALINGNOVA_DESC", "Release a <style=cIsHealing>healing nova</style> during the Teleporter event, <style=cIsHealing>cleansing</style>"
                + " and <style=cIsHealing>healing</style> all nearby allies for <style=cIsHealing>50%</style> of their maximum health. Occurs "
                + "<style=cIsHealing>2</style> <style=cStack>(+1 per stack)</style> times.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
