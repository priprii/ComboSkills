using System.Windows.Forms;

namespace ComboSkills {
    public class Skill {
        public string Name { get; set; }
        public string AltName { get; set; }
        public Keys Key { get; set; }
        public bool CtrlMod { get; set; }
        public bool ShiftMod { get; set; }
        public bool AltMod { get; set; }
    }
}
