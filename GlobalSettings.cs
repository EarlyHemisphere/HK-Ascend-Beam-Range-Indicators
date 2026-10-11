using InControl;
using Modding.Converters;
using Newtonsoft.Json;

namespace AscendBeamRangeIndicators {
    public class GlobalSettings {
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();

        public bool showIndicators = true;
    }

    public class KeyBinds : PlayerActionSet {
        public PlayerAction ForceBeamAtPlayer;

        public KeyBinds() {
            ForceBeamAtPlayer = CreatePlayerAction("ForceBeamAtPlayer");
        }
    }
}
