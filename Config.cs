using System.Collections.Generic;
using System.IO;
using System.Threading;
using Advanced_Combat_Tracker;
using Newtonsoft.Json;

namespace ComboSkills {
    public static class Config {
        private static Timer ConfigTimer;
        private static string ConfigPath = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, @"Config\Primu.ComboSkills.config.json");

        public static Options Options { get; set; } = new Options();

        public static Options.LocalizationOptions Localization { get { return Options.Localization; } set { Options.Localization = value; } }
        public static Options.SkillComboOptions SkillCombo { get { return Options.SkillCombo; } set { Options.SkillCombo = value; } }

        public static void Load() {
            if(!File.Exists(ConfigPath)) {
                Save(false);
            } else {
                Options = JsonConvert.DeserializeObject<Options>(File.ReadAllText(ConfigPath), new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.Auto });
            }
        }

        public static void Save(bool onTimer) {
            if(onTimer) {
                if(ConfigTimer == null) {
                    ConfigTimer = new Timer(new TimerCallback((state) => {
                        ConfigTimer.Dispose();
                        ConfigTimer = null;
                        Save(false);
                    }));
                }
                ConfigTimer.Change(2000, 0);
            } else {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Options, Formatting.Indented));
            }
        }
    }

    public class Options {
        public LocalizationOptions Localization { get; set; } = new LocalizationOptions();
        public SkillComboOptions SkillCombo { get; set; } = new SkillComboOptions();

        public class LocalizationOptions {
            public string PlayerName { get; set; } = "YOU";
            public string PlayerClass { get; set; }
        }

        public class SkillComboOptions {
            public List<Combo> Combos { get; set; } = new List<Combo>();
            public List<Skill> Skills { get; set; } = new List<Skill>();
        }
    }
}
