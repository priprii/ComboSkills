using System.Collections.Generic;

namespace ComboSkills {
    public class Combo {
        public string Class { get; set; }
        public List<string> Skills { get; set; }
        public int Expiration { get; set; }

        public int Index { get; set; }
        public bool Triggered { get; set; }
        public KeyState State { get; set; }
        public long ExpirationTimestamp { get; set; }
    }
}
