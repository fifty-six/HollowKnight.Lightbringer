using System.Collections.Generic;

namespace Lightbringer {
    public partial class Lightbringer
    {
        private readonly Dictionary<string, string> _langDict = new Dictionary<string, string>
        {
            #region Charm Descriptions

            ["CHARM_DESC_13"] =
                "Trophy given to those proven to be worthy. A requirement for any who wish to master the nail.\n\nGreatly increases the range of the bearer's nail and causes it to deal more damage based on what they have accomplished.",
            ["CHARM_DESC_14"] =
                "What is this? Why does it exist? Is it safe to use?\n\nSlashing upwards speeds up the flow of time. Taking damage will break the effect.",
            ["CHARM_DESC_15"] =
                "A small trinket made of valuable metal and precious stone.\n\nLanding five successful nail attacks on an enemy will restore health.",
            ["CHARM_DESC_17"] =
                "Composed of living fungal matter. Scatters spores when exposed to SOUL.\n\nWhen focusing SOUL, emit a spore cloud that slowly damages enemies. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_18"] = "A metal badge that is cold to the touch. It brings back warm memories.\n\nFire lances in both directions when attacking.",
            ["CHARM_DESC_20"] =
                "A token given to those embarking on a great journey.\n\nSplit the lance attack into two projectiles, allowing the user to hit multiple enemies at once (cannot hit the same enemy twice).",
            ["CHARM_DESC_21"] = "A poorly made wallet that seems to lose all the geo you put in it.\n\nPicked up geo is converted directly into SOUL.",
            ["CHARM_DESC_22"] =
                "Drains the SOUL of its bearer and uses it to birth hatchlings.\n\nThe hatchlings have no desire to eat or live, and will sacrifice themselves to protect their parent. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_25"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm is fragile, and will break if its bearer is killed.",
            ["CHARM_DESC_26"] =
                "Contains the passion, skill, and regrets of a Nailmaster.\n\nCharge nail arts faster and summon lances around the user at random.",
            ["CHARM_DESC_28"] =
                "Reveals the form of Unn within the bearer.\n\nWhile focusing SOUL, the bearer will take on a new shape and can move freely to avoid enemies. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_30"] =
                "Transient charm created for those who wield the Dream Nail and collect Essence.\n\nAllows the bearer to charge the Dream Nail faster and collect more SOUL with it. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_32"] =
                "Born from imperfect, discarded nails that have fused together. The nails still long to be wielded.\n\nIncreases attack speed moderately.",
            ["CHARM_DESC_34"] =
                "Naturally formed within a crystal over a long period. Draws in SOUL from the surrounding air.\n\nThe bearer will focus SOUL at a slower rate, but the healing effect will double and holy power is unleashed. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_35"] = "Imbues the lance with holy energy.\n\nGreatly increases damage and lance size.",
            ["CHARM_DESC_25_G"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm is unbreakable.",
            ["CHARM_DESC_6"] =
                "Reserved for only the strongest of challengers.\n\nAmplifies lance damage based on current health, but the user will be destroyed in a single blow.",
            ["CHARM_DESC_25_BROKEN"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm has broken, and the power inside has been silenced. It can not be equipped.",
            ["CHARM_DESC_1"] =
                "A swarm will follow the bearer and gather up any loose Geo.\n\nUseful for those who can't bear to leave anything behind, no matter how insignificant.",
            ["CHARM_DESC_8"] = "A metal shell that vibrates continuously.\n\nAttacking upwards releases a barrage of lances.",
            ["CHARM_DESC_19"] =
                "An artifact said to bring out the true power hidden inside of its user.\n\nIncreases the power of spells, dealing more damage to foes. Replaces some attacks with intense blasts of energy. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_2"] =
                "Whispers its location to the bearer whenever a map is open, allowing wanderers to pinpoint their current location.\n\nIncreases the wearer's movement speed by 3% for each missing mask.",
            ["CHARM_DESC_3"] =
                "A pendant forged from bone and blood. Do you feel guilty, or proud?\n\nGain a chance to make a critical damage attack. This chance increases with each new victim you claim (in your Hunter's Journal).",
            ["CHARM_DESC_4"] =
                "A shell suited for someone tiny yet tough. When recovering from damage, the bearer will remain invulnerable for longer.\n\nDecreases your size by 25%.",

            #endregion

            #region Shop Descriptions 

            ["SHOP_DESC_WAYWARDCOMPASS"] =
                "Highly recommended! If you're having trouble finding your way in the maze of ruins below us, try this charm.\n\nIt will pinpoint your location on your map, and even help you get around a bit quicker!",
            ["SHOP_DESC_SPELLDMGUP"] =
                "Are you a spellcaster, you little scoundrel? Ho ho! I'm only teasing.\n\nIf you ever learn any spells you should buy this charm for yourself. I've heard it can awaken hidden powers in whoever holds it!",
            ["SHOP_DESC_ENEMYRECOILUP"] =
                "Tired of having to heal in the middle of a fight? With this charm equipped, you can just whack enemies with your nail until your health is back!",
            ["SHOP_DESC_BLUEHEALTHSMALL"] = "Having trouble taking out enemies that fly high above you?\n\nWith this, you can fire lances into the sky!",
            ["SHOP_DESC_LONGNAIL"] =
                "Are you tired of only attacking in one direction?\n\nHo! Ho ho ho! Go on, take this charm home with you. It may provide the service you are looking for...",
            ["SHOP_DESC_NORECOIL"] =
                "I don't know where this came from... Was it always here? Should I sell it?\n\nWhatever it is, it should be used with extreme caution.",
            ["SHOP_DESC_STALWARTSHELL"] =
                "Life in Hallownest can be tough, always taking hits and getting knocked around. The bigger you are, the harder you fall. This charm is the solution to that.",

            #endregion

            #region Nail 

            ["INV_NAME_NAIL1"] = "Lance of Light",
            ["INV_NAME_NAIL2"] = "Lance of Light +1",
            ["INV_NAME_NAIL3"] = "Lance of Light +2",
            ["INV_NAME_NAIL4"] = "Lance of Light +3",
            ["INV_NAME_NAIL5"] = "Severance",
            ["INV_DESC_NAIL1"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL2"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL3"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL4"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL5"] = "It is time. Bring forth the light.",

            #endregion

            #region Charm Names 

            ["CHARM_NAME_2"] = "Panic Compass",
            ["CHARM_NAME_3"] = "Bloodsong",
            ["CHARM_NAME_4"] = "Tiny Shell",
            ["CHARM_NAME_6"] = "Glass Soul",
            ["CHARM_NAME_8"] = "Rising Light",
            ["CHARM_NAME_13"] = "Burning Pride",
            ["CHARM_NAME_14"] = "Time Fracture",
            ["CHARM_NAME_15"] = "Bloodlust",
            ["CHARM_NAME_18"] = "Silent Divide",
            ["CHARM_NAME_19"] = "Eye of the Storm",
            ["CHARM_NAME_20"] = "Twin Fangs",
            ["CHARM_NAME_21"] = "Faulty Wallet",
            ["CHARM_NAME_25"] = "Fragile Nightmare",
            ["CHARM_NAME_25_BRK"] = "Fragile Nightmare (Repair)",
            ["CHARM_NAME_25_G"] = "Unbreakable Nightmare",
            ["CHARM_NAME_26"] = "Nailmaster's Passion",
            ["CHARM_NAME_35"] = "Radiant Jewel",

            #endregion

            #region Muzznik 

            ["BIGFLY_MAIN"] = "EMPRESS",
            ["BIGFLY_SUB"] = "MUZZNIK"

            #endregion
        };
    }
}