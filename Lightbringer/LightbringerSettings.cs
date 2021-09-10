using Modding;

namespace Lightbringer
{
    public class LightbringerSettings : ModSettings
    {
        // Lance attack config options
        public int BaseBeamDamage
        {
            get => GetInt(3);
            set => SetInt(value);
        }

        public int UpgradeBeamDamage
        {
            get => GetInt(3);
            set => SetInt(value);
        }

        public int RadiantJewelDamage
        {
            get => GetInt(5);
            set => SetInt(value);
        }

        public float FragileNightmareScaleFactor
        {
            get => GetFloat(1 / 20f);
            set => SetFloat(value);
        }

        public int FragileNightmareSoulCost
        {
            get => GetInt(7);
            set => SetInt(value);
        }

        // Nail attack config options
        public int BaseNailDamage
        {
            get => GetInt(1);
            set => SetInt(value);
        }

        public int UpgradeNailDamage
        {
            get => GetInt(2);
            set => SetInt(value);
        }

        public float BurningPrideScaleFactor
        {
            get => GetFloat(1 / 6f);
            set => SetFloat(value);
        }

        // MP regen
        public float SoulRegenRate
        {
            get => GetFloat(1.11f);
            set => SetFloat(value);
        }
    }
}
