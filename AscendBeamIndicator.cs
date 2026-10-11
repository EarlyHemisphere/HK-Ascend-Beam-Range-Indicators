using Modding;
using Satchel.BetterMenus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AscendBeamRangeIndicators {
    public class AscendBeamRangeIndicators : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod {
        internal static AscendBeamRangeIndicators instance;

        public AscendBeamRangeIndicators() : base("Ascend Beam Range Indicators") {
           instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            USceneManager.activeSceneChanged += CheckForRadiance;
            Log("Initialized.");
        }

        public override string GetVersion() {
            return "1.0.0.0";
        }

        public void Unload() {
            USceneManager.activeSceneChanged -= CheckForRadiance;
        }

        private static void CheckForRadiance(Scene from, Scene to) {
            if (to.name != "GG_Radiance") {
                return;
            }

            new GameObject("AbsFinder", typeof(AbsFinder));
        }

        public static GlobalSettings settings { get; private set; } = new GlobalSettings();

        public void OnLoadGlobal(GlobalSettings s) => settings = s;

        public GlobalSettings OnSaveGlobal() => settings;

        public bool ToggleButtonInsideMenu => false;

        private Menu menuRef;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) {
            menuRef ??= new Menu(
                "Ascend Beam Range Indicators",
                new Element[] {
                    new HorizontalOption(
                        name: "Show Beam Indicators",
                        description: "Toggles the beam range indicator lines shown during Radiance's climb phase.",
                        values: new string[] {"False", "True"},
                        applySetting: val => settings.showIndicators = val == 1,
                        loadSetting: () => settings.showIndicators ? 1 : 0
                    ),
                    new KeyBind(
                        name: "Force Beam At Player",
                        playerAction: settings.keybinds.ForceBeamAtPlayer
                    )
                }
            );

            return menuRef.GetMenuScreen(modListMenu);
        }
    }
}