using Unity.Netcode;

namespace AC
{
    public class Potion : NetworkBehaviour
    {
        public PotionType potionType;
        public int duration;

        public enum PotionType
        {
            Speed,
            Healing,
            Invincibility,
            Strength,
            Jumping
        }
    }
}
