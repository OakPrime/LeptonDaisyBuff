using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using R2API;

namespace LeptonDaisyBuff
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class LeptonDaisyBuff : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "LeptonDaisyBuff";
        public const string PluginVersion = "1.1.2";

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
                IL.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdcR4(out _)
                    );
                    c.Next.Operand = 0.35f;
                };
                IL.EntityStates.TeleporterHealNovaController.TeleporterHealNovaGeneratorMain.CalculateNextPulseFraction += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(x => x.MatchLdcI4(out _));
                    c.Emit(OpCodes.Ldc_I4_2);
                    c.Emit(OpCodes.Mul);
                    c.TryGotoNext(x => x.MatchMul());
                    c.Index++;
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<float, float, float>>((nextPulseFraction, prevPulseFraction) =>
                    {
                        //Log.LogInfo("PREVIOUS FRAC: " + prevPulseFraction);
                        if (prevPulseFraction < 0.01f)
                        {
                            return 0.01f;
                        }
                        //Log.LogInfo("NEXT FRAC: " + nextPulseFraction);
                        return nextPulseFraction;
                    });
                    c.TryGotoNext(x => x.MatchLdarg(0));
                    c.Index++;
                    c.Emit(OpCodes.Ldc_I4_2);
                    c.Emit(OpCodes.Mul);
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
                + " and <style=cIsHealing>healing</style> all nearby allies for <style=cIsHealing>35%</style> of their maximum health. Occurs "
                + "<style=cIsHealing>3</style> <style=cStack>(+2 per stack)</style> times.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
