using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using ModCommon;
using Modding;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Lightbringer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Lightbringer : Mod, ITogglableMod
    {
        private readonly Dictionary<string, string> _langDict = new Dictionary<string, string>
        {
            ["CHARM_DESC_13"] =
                "Trophy given to those proven to be worthy. A requirement for any who wish to master the nail.\n\nGreatly increases the range of the bearer's nail and causes it to deal more damage based on what they have accomplished.",
            ["CHARM_DESC_14"] =
                "What is this? Why does it exist? Is it safe to use?\n\nSlashing upwards speeds up the flow of time. Taking damage will break the effect.",
            ["CHARM_DESC_15"] =
                "A small trinket made of valuable metal and precious stone.\n\nLanding five successful nail attacks on an enemy will restore health.",
            ["CHARM_DESC_17"] =
                "Composed of living fungal matter. Scatters spores when exposed to SOUL.\n\nWhen focusing SOUL, emit a spore cloud that slowly damages enemies. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_18"] =
                "A metal badge that is cold to the touch. It brings back warm memories.\n\nFire lances in both directions when attacking.",
            ["CHARM_NAME_25_BRK"] = "Fragile Nightmare (Repair)",
            ["CHARM_DESC_20"] =
                "A token given to those embarking on a great journey.\n\nSplit the lance attack into two projectiles, allowing the user to hit multiple enemies at once (cannot hit the same enemy twice).",
            ["CHARM_DESC_21"] =
                "A poorly made wallet that seems to lose all the geo you put in it.\n\nPicked up geo is converted directly into SOUL.",
            ["CHARM_DESC_22"] =
                "Drains the SOUL of its bearer and uses it to birth hatchlings.\n\nThe hatchlings have no desire to eat or live, and will sacrifice themselves to protect their parent. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_25"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm is fragile, and will break if its bearer is killed.",
            ["CHARM_DESC_26"] =
                "Contains the passion, skill, and regrets of a Nailmaster.\n\nCharge nail arts faster and summon lances around the user at random.",
            ["CHARM_DESC_28"] =
                "Reveals the form of Unn within the bearer.\n\nWhile focusing SOUL, the bearer will take on a new shape and can move freely to avoid enemies. Increases SOUL regeneration rate slightly.",
            ["CHARM_NAME_13"] = "Burning Pride",
            ["INV_NAME_NAIL4"] = "Lance of Light +3",
            ["INV_NAME_NAIL5"] = "Severance",
            ["CHARM_DESC_30"] =
                "Transient charm created for those who wield the Dream Nail and collect Essence.\n\nAllows the bearer to charge the Dream Nail faster and collect more SOUL with it. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_32"] =
                "Born from imperfect, discarded nails that have fused together. The nails still long to be wielded.\n\nIncreases attack speed moderately.",
            ["CHARM_DESC_34"] =
                "Naturally formed within a crystal over a long period. Draws in SOUL from the surrounding air.\n\nThe bearer will focus SOUL at a slower rate, but the healing effect will double and holy power is unleashed. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_35"] =
                "Imbues the lance with holy energy.\n\nGreatly increases damage and lance size.",
            ["CHARM_DESC_25_G"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm is unbreakable.",
            ["CHARM_NAME_25"] = "Fragile Nightmare",
            ["SHOP_DESC_SPELLDMGUP"] =
                "Are you a spellcaster, you little scoundrel? Ho ho! I'm only teasing.\n\nIf you ever learn any spells you should buy this charm for yourself. I've heard it can awaken hidden powers in whoever holds it!",
            ["SHOP_DESC_ENEMYRECOILUP"] =
                "Tired of having to heal in the middle of a fight? With this charm equipped, you can just whack enemies with your nail until your health is back!",
            ["INV_NAME_NAIL1"] = "Lance of Light",
            ["INV_NAME_NAIL2"] = "Lance of Light +1",
            ["INV_NAME_NAIL3"] = "Lance of Light +2",
            ["CHARM_DESC_6"] =
                "Reserved for only the strongest of challengers.\n\nAmplifies lance damage based on current health, but the user will be destroyed in a single blow.",
            ["INV_DESC_NAIL1"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL2"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL3"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL4"] = "Summons a lance of light from thin air to destroy your enemies.",
            ["INV_DESC_NAIL5"] = "It is time. Bring forth the light.",
            ["CHARM_NAME_25_G"] = "Unbreakable Nightmare",
            ["CHARM_DESC_25_BROKEN"] =
                "Attacks cost a small amount of SOUL but lance damage is increased based on current SOUL (up to +10 damage). Increases SOUL regeneration rate slightly.\n\nThis charm has broken, and the power inside has been silenced. It can not be equipped.",
            ["SHOP_DESC_BLUEHEALTHSMALL"] =
                "Having trouble taking out enemies that fly high above you?\n\nWith this, you can fire lances into the sky!",
            ["SHOP_DESC_LONGNAIL"] =
                "Are you tired of only attacking in one direction?\n\nHo! Ho ho ho! Go on, take this charm home with you. It may provide the service you are looking for...",
            ["CHARM_NAME_2"] = "Panic Compass",
            ["CHARM_NAME_3"] = "Bloodsong",
            ["CHARM_NAME_4"] = "Tiny Shell",
            ["CHARM_NAME_6"] = "Glass Soul",
            ["CHARM_NAME_8"] = "Rising Light",
            ["SHOP_DESC_NORECOIL"] =
                "I don't know where this came from... Was it always here? Should I sell it?\n\nWhatever it is, it should be used with extreme caution.",
            ["SHOP_DESC_STALWARTSHELL"] =
                "Life in Hallownest can be tough, always taking hits and getting knocked around. The bigger you are, the harder you fall. This charm is the solution to that.",
            ["CHARM_DESC_1"] =
                "A swarm will follow the bearer and gather up any loose Geo.\n\nUseful for those who can't bear to leave anything behind, no matter how insignificant.",
            ["CHARM_NAME_14"] = "Time Fracture",
            ["CHARM_NAME_15"] = "Bloodlust",
            ["CHARM_NAME_18"] = "Silent Divide",
            ["CHARM_NAME_19"] = "Eye of the Storm",
            ["CHARM_DESC_8"] =
                "A metal shell that vibrates continuously.\n\nAttacking upwards releases a barrage of lances.",
            ["CHARM_NAME_20"] = "Twin Fangs",
            ["CHARM_NAME_21"] = "Faulty Wallet",
            ["CHARM_NAME_26"] = "Nailmaster's Passion",
            ["CHARM_NAME_35"] = "Radiant Jewel",
            ["CHARM_DESC_19"] =
                "An artifact said to bring out the true power hidden inside of its user.\n\nIncreases the power of spells, dealing more damage to foes. Replaces some attacks with intense blasts of energy. Increases SOUL regeneration rate slightly.",
            ["CHARM_DESC_2"] =
                "Whispers its location to the bearer whenever a map is open, allowing wanderers to pinpoint their current location.\n\nIncreases the wearer's movement speed by 3% for each missing mask.",
            ["CHARM_DESC_3"] =
                "A pendant forged from bone and blood. Do you feel guilty, or proud?\n\nGain a chance to make a critical damage attack. This chance increases with each new victim you claim (in your Hunter's Journal).",
            ["SHOP_DESC_WAYWARDCOMPASS"] =
                "Highly recommended! If you're having trouble finding your way in the maze of ruins below us, try this charm.\n\nIt will pinpoint your location on your map, and even help you get around a bit quicker!",
            ["CHARM_DESC_4"] =
                "A shell suited for someone tiny yet tough. When recovering from damage, the bearer will remain invulnerable for longer.\n\nDecreases your size by 25%.",
            ["BIGFLY_MAIN"] = "EMPRESS",
            ["BIGFLY_SUB"] = "MUZZNIK"
        };

        private GameObject _gruz;

        private GameObject _kin;

        private int _hitNumber;

        internal static Lightbringer Instance;

        // mod vars
        private float _manaRegenTime = Time.deltaTime;

        private bool _passionDirection = true;

        private float _passionTime = Time.deltaTime;

        private float _timefracture;

        private GameObject GrubberFlyBeam
        {
            get => GetAttr<GameObject>(HeroController.instance, "grubberFlyBeam");
            set => SetAttr(HeroController.instance, "grubberFlyBeam", value);
        }

        internal SpriteFlash SpriteFlash => GetAttr<SpriteFlash>(HeroController.instance, "spriteFlash");

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private T GetAttr<T>(object obj, string name, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return default(T);

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                    t.GetField(name,
                        BindingFlags.NonPublic | BindingFlags.Public |
                        (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            return (T) typeFields[name]?.GetValue(obj);
        }

        internal Dictionary<string, Sprite> Sprites;

        // down isn't here cause elegy doesn't have it
        private enum BeamDirection
        {
            Up,
            Down,
            Right,
            Left
        }

        private void SpawnBeam(BeamDirection dir, float scaleX, float scaleY, bool critical = false,
            float? positionX = null, float? positionY = null, bool offset = false, bool rightNegative = true)
        {
            string beamPrefab = "grubberFlyBeamPrefab";
            switch (dir)
            {
                case BeamDirection.Up:
                    beamPrefab += "U";
                    break;
                case BeamDirection.Down:
                    beamPrefab += "D";
                    break;
                case BeamDirection.Right:
                    beamPrefab += "R";
                    break;
                case BeamDirection.Left:
                    beamPrefab += "L";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            beamPrefab += critical ? "_fury" : "";

            GrubberFlyBeam = GetAttr<GameObject>(HeroController.instance, beamPrefab)
                .Spawn(HeroController.instance.transform.position);
            Transform t = HeroController.instance.transform;
            if (positionX != null)
                GrubberFlyBeam.transform.SetPositionX((float) (positionX + (offset ? t.GetPositionX() : 0)));
            if (positionY != null)
                GrubberFlyBeam.transform.SetPositionY((float) (positionY + (offset ? t.GetPositionY() : 0)));
            GrubberFlyBeam.transform.SetScaleX((rightNegative && dir == BeamDirection.Right ? -1 : 1) * scaleX);
            GrubberFlyBeam.transform.SetScaleY(scaleY);
        }

        public override string GetVersion() => "shitpost";

        private void CreateCanvas()
        {
            if (_canvas != null) return;
            CanvasUtil.CreateFonts();
            _canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            Object.DontDestroyOnLoad(_canvas);
            GameObject gameObject =
                CanvasUtil.CreateTextPanel(_canvas, "", 27, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(
                        new Vector2(0, 50),
                        new Vector2(0, 45),
                        new Vector2(0, 0),
                        new Vector2(1, 0),
                        new Vector2(0.5f, 0.5f)));
            _textObj = gameObject.GetComponent<Text>();
            _textObj.font = CanvasUtil.TrajanBold;
            _textObj.text = "";
            _textObj.fontSize = 42;
        }
        

        public override void Initialize()
        {
            Instance = this;

            try
            {
                RegisterCallbacks();
            }
            catch
            {
                CreateCanvas();
                _textObj.text = "Lightbringer requires ModCommon to function! Install it!";
                _textObj.CrossFadeAlpha(1f, 0f, false);
            }
        }

        private void RegisterCallbacks() 
        {

        // Sprites!
            On.ShopItemStats.Awake += Awake;

            // Lance Spawn
            On.HeroController.Attack += Attack;

            // Faulty Wallet
            On.PlayerData.AddGeo += AddGeo;

            // Burning Blade, Fury
            On.NailSlash.StartSlash += StartSlash;

            // Charm Values
            // Restore Nail Damage
            // SPRITES!
            ModHooks.Instance.BeforeSavegameSaveHook += BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.Instance.SavegameSaveHook += SaveGameSave;

            // Panic Compass
            ModHooks.Instance.BeforeAddHealthHook += Health;
            ModHooks.Instance.TakeHealthHook += Health;

            // Don't hit walls w/ lances
            ModHooks.Instance.DoAttackHook += DoAttack;
            ModHooks.Instance.AfterAttackHook += AfterAttack;

            // Glass Soul
            ModHooks.Instance.TakeHealthHook += TakeHealth;

            // Disable Soul Gain
            // Bloodlust
            ModHooks.Instance.SoulGainHook += SoulGain;

            // Soul Gen
            // 753 Easter Egg
            // Nailmaster's Passion
            // Add Muzznik & DoubleKin Behaviours
            ModHooks.Instance.HeroUpdateHook += Update;

            // Beam Damage
            // Ghosts
            // Timescale
            // Panic Compass
            // Tiny Shell
            ModHooks.Instance.CharmUpdateHook += CharmUpdate;

            // Custom Text
            ModHooks.Instance.LanguageGetHook += LangGet;

            // Ascending Light won't give 2 hearts
            ModHooks.Instance.BlueHealthHook += BlueHealth;

            // Lance Textures
            // Canvas for Muzznik Text
            // Soul Orb FSM
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoadedHook;

            Assembly asm = Assembly.GetExecutingAssembly();

            Sprites = new Dictionary<string, Sprite>();
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (!res.EndsWith(".png"))
                {
                    Log("Unknown resource: " + res);
                    continue;
                }

                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();

                    //Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer);

                    //Create sprite from texture
                    //Substring is to cut off the Lightbringer. and the .png
                    Sprites.Add(res.Substring(23, res.Length - 27),
                        Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));

                    Log("Created sprite from embedded image: " + res);
                }
            }
        }

        public void Unload()
        {
            On.ShopItemStats.Awake -= Awake;
            On.HeroController.Attack -= Attack;
            On.PlayerData.AddGeo -= AddGeo;
            On.NailSlash.StartSlash -= StartSlash;
            ModHooks.Instance.BeforeSavegameSaveHook -= BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.SavegameSaveHook -= SaveGameSave;
            ModHooks.Instance.TakeHealthHook -= Health;
            ModHooks.Instance.DoAttackHook -= DoAttack;
            ModHooks.Instance.AfterAttackHook -= AfterAttack;
            ModHooks.Instance.TakeHealthHook -= TakeHealth;
            ModHooks.Instance.SoulGainHook -= SoulGain;
            ModHooks.Instance.HeroUpdateHook -= Update;
            ModHooks.Instance.CharmUpdateHook -= CharmUpdate;
            ModHooks.Instance.LanguageGetHook -= LangGet;
            ModHooks.Instance.BlueHealthHook -= BlueHealth;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoadedHook;

            if (PlayerData.instance != null)
                BeforeSaveGameSave();
        }


        private void Awake(On.ShopItemStats.orig_Awake orig, ShopItemStats self)
        {
            orig(self);
            
            string pdbool = self.playerDataBoolName;
            if (!pdbool.StartsWith("gotCharm_")) return;
            
            string key = "Charms." + pdbool.Substring(9, pdbool.Length - 9);
            if (Sprites.ContainsKey(key))
                GetAttr<GameObject>(self, "itemSprite").GetComponent<SpriteRenderer>().sprite = Sprites[key];
        }

        private void AfterSaveGameLoad(SaveGameData data)
        {
            SaveGameSave();
            GameManager.instance.StartCoroutine(ChangeSprites());
        }

        private IEnumerator ChangeSprites()
        {
            while (CharmIconList.Instance == null || GameManager.instance == null || HeroController.instance == null || Sprites.Count < 20)
            {
                yield return null;
            }

            foreach (int i in new int[] {2, 3, 4, 6, 8, 13, 14, 15, 18, 19, 20, 21, 25, 26, 35})
            {
                CharmIconList.Instance.spriteList[i] = Sprites["Charms." + i];
            }

            CharmIconList.Instance.unbreakableStrength = Sprites["Charms.ustr"];

            GameManager.instance.inventoryFSM.gameObject.FindGameObjectInChildren("25")
                .LocateMyFSM("charm_show_if_collected").GetAction<SetSpriteRendererSprite>("Glass Attack", 2).sprite
                .Value = Sprites["Charms.brokestr"];

            HeroController.instance.grubberFlyBeamPrefabL.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material
                .mainTexture = Sprites["Lances"].texture;

            InvNailSprite invNailSprite = GameManager.instance.inventoryFSM.gameObject.FindGameObjectInChildren("Nail")
                .GetComponent<InvNailSprite>();
            invNailSprite.level1 = Sprites["LanceInv"];
            invNailSprite.level2 = Sprites["LanceInv"];
            invNailSprite.level3 = Sprites["LanceInv"];
            invNailSprite.level4 = Sprites["LanceInv"];
            invNailSprite.level5 = Sprites["LanceInv"];

            Log("Changed Sprites!");
        }


        private void SaveGameSave(int id = 0)
        {
            PlayerData.instance.charmCost_21 = 1; // Faulty Wallet update patch
            PlayerData.instance.charmCost_19 = 4; // Eye of the Storm update patch
            PlayerData.instance.charmCost_15 = 3; // Bloodlust update patch
            PlayerData.instance.charmCost_14 = 2; // Glass Soul update patch
            PlayerData.instance.charmCost_8 = 3; // Rising Light update patch
            PlayerData.instance.charmCost_35 = 5; // Radiant Jewel update patch
            PlayerData.instance.charmCost_18 = 3; // Silent Divide update patch
            PlayerData.instance.charmCost_3 = 2; // Bloodsong update patch
        }

        private void BeforeSaveGameSave(SaveGameData data = null)
        {
            PlayerData.instance.charmCost_21 = 4;
            PlayerData.instance.charmCost_19 = 3;
            PlayerData.instance.charmCost_15 = 2;
            PlayerData.instance.charmCost_14 = 1;
            PlayerData.instance.charmCost_8 = 2;
            PlayerData.instance.charmCost_35 = 3;
            PlayerData.instance.charmCost_18 = 2;
            PlayerData.instance.charmCost_3 = 1;
            PlayerData.instance.nailDamage = PlayerData.instance.nailSmithUpgrades * 4 + 5; // protect the player from hurting their save data
        }

        private const float ORIG_RUN_SPEED = 8.3f;
        private const float ORIG_RUN_SPEED_CH = 10f;
        private const float ORIG_RUN_SPEED_CH_COMBO = 11.5f;

        private int Health(int amount)
        {
            float panicSpeed = 1f;
            if (HeroController.instance.playerData.equippedCharm_2)
            {
                int missingHealth = HeroController.instance.playerData.maxHealth -
                                    HeroController.instance.playerData.health;
                panicSpeed += missingHealth * .03f;
                HeroController.instance.RUN_SPEED = ORIG_RUN_SPEED * panicSpeed;
                HeroController.instance.RUN_SPEED_CH = ORIG_RUN_SPEED_CH * panicSpeed;
                HeroController.instance.RUN_SPEED_CH_COMBO = ORIG_RUN_SPEED_CH_COMBO * panicSpeed;
            }
            else
            {
                HeroController.instance.RUN_SPEED = ORIG_RUN_SPEED;
                HeroController.instance.RUN_SPEED_CH = ORIG_RUN_SPEED_CH;
                HeroController.instance.RUN_SPEED_CH_COMBO = ORIG_RUN_SPEED_CH_COMBO;
            }

            return amount;
        }

        private float _origNailTerrainCheckTime;

        private void DoAttack()
        {
            if (_origNailTerrainCheckTime == 0)
            {
                _origNailTerrainCheckTime = GetAttr<float>(HeroController.instance, "NAIL_TERRAIN_CHECK_TIME");
            }

            if (!(HeroController.instance.vertical_input < Mathf.Epsilon) &&
                !(HeroController.instance.vertical_input < -Mathf.Epsilon &&
                  HeroController.instance.hero_state != ActorStates.idle &&
                  HeroController.instance.hero_state != ActorStates.running))
            {
                SetAttr(HeroController.instance, "NAIL_TERRAIN_CHECK_TIME", 0f);
            }
        }

        private void AfterAttack(AttackDirection dir)
        {
            SetAttr(HeroController.instance, "NAIL_TERRAIN_CHECK_TIME", _origNailTerrainCheckTime);
        }

        private int BlueHealth()
        {
            // Make Rising Light not give 2 blue health.
            return PlayerData.instance.equippedCharm_8 ? -2 : 0;
        }

        private string LangGet(string key, string sheetTitle)
        {
            return _langDict.TryGetValue(key, out string val) ? val : Language.Language.GetInternal(key, sheetTitle);
        }

        private void SetAttr<T>(object obj, string name, T val, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return;

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                    t.GetField(name,
                        BindingFlags.NonPublic | BindingFlags.Public |
                        (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            typeFields[name]?.SetValue(obj, val);
        }


        private void AddGeo(On.PlayerData.orig_AddGeo orig, PlayerData self, int amount)
        {
            // Don't let Faulty Wallet hurt people with full SOUL
            if (PlayerData.instance.equippedCharm_21 && (PlayerData.instance.MPCharge < PlayerData.instance.maxMP ||
                                                         PlayerData.instance.MPReserve !=
                                                         PlayerData.instance.MPReserveMax))
            {
                int lostGeo = (PlayerData.instance.maxMP - 1 - PlayerData.instance.MPCharge) / 3 +
                              (PlayerData.instance.MPReserveMax - PlayerData.instance.MPReserve) / 3 + 1;
                PlayerData.instance.AddMPCharge(lostGeo > amount ? amount * 3 : lostGeo * 3);
                orig(self, lostGeo > amount ? 0 : amount - lostGeo);
            }
            else
            {
                orig(self, amount);
            }
        }

        private void Attack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            HeroController.instance.cState.altAttack = false;
            HeroController.instance.cState.attacking = true;

            // Damage Controller //////////////////////////////////////////////////////////////////////////
            // NAIL
            HeroController.instance.playerData.nailDamage =
                1 + HeroController.instance.playerData.nailSmithUpgrades * 2;
            if (HeroController.instance.playerData.equippedCharm_13) // Mark of Pride
            {
                HeroController.instance.playerData.CountGameCompletion();
                HeroController.instance.playerData.nailDamage +=
                    (int) HeroController.instance.playerData.completionPercentage / 8;
            }

            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            // LANCE
            HeroController.instance.playerData.beamDamage =
                3 + HeroController.instance.playerData.nailSmithUpgrades * 3;
            if (HeroController.instance.playerData.equippedCharm_35
            ) // Radiant Jewel charm replacing Grubberfly's Elegy
            {
                HeroController.instance.playerData.beamDamage += 5;
            }

            if (HeroController.instance.playerData.equippedCharm_25 &&
                HeroController.instance.playerData.MPCharge > 3) // Fragile Strength > Fragile Nightmare
            {
                HeroController.instance.playerData.beamDamage += HeroController.instance.playerData.MPCharge / 20;
                HeroController.instance.TakeMP(7);
            }

            if (HeroController.instance.playerData.equippedCharm_6) // Glass Soul charm replacing Fury of Fallen
            {
                HeroController.instance.playerData.beamDamage +=
                    HeroController.instance.playerData.health + HeroController.instance.playerData.healthBlue - 3;
            }

            bool critical = false;
            if (HeroController.instance.playerData.equippedCharm_3) // Bloodsong replaces Grubsong
            {
                Random rnd = new Random(); // CRITICAL HIT CHARM
                int critChance = rnd.Next(1, 101);
                HeroController.instance.playerData.CountJournalEntries();
                int critThreshold = 100 - HeroController.instance.playerData.journalEntriesCompleted / 10;
                if (critChance > Math.Min(critThreshold, 96))
                {
                    critical = true;
                }

                if (critical)
                {
                    HeroController.instance.playerData.beamDamage *= 3;
                    HeroController.instance.shadowRingPrefab.Spawn(HeroController.instance.transform.position);
                    GetAttr<AudioSource>(HeroController.instance, "audioSource")
                        .PlayOneShot(HeroController.instance.nailArtChargeComplete, 1f);
                }
            }

            int lanceDamage = HeroController.instance.playerData.beamDamage;
            // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // QUICK SLASH CHARM #REEE32
            SetAttr(HeroController.instance, "attackDuration", HeroController.instance.playerData.equippedCharm_32
                ? HeroController.instance.ATTACK_DURATION_CH
                : HeroController.instance.ATTACK_DURATION);

            if (HeroController.instance.cState.wallSliding)
            {
                HeroController.instance.playerData.nailDamage =
                    HeroController.instance.playerData.beamDamage; // fix bug
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                HeroController.instance.playerData.beamDamage = lanceDamage;

                SetAttr(HeroController.instance, "wallSlashing", true);
                bool x = HeroController.instance.cState.facingRight;
                SpawnBeam(x ? BeamDirection.Left : BeamDirection.Right, 1f, 1f, critical);
            }
            else
            {
                SetAttr(HeroController.instance, "wallSlashing", false);
                switch (attackDir)
                {
                    #region Normal Attack

                    case AttackDirection.normal:
                        HeroController.instance.playerData.nailDamage =
                            HeroController.instance.playerData.beamDamage; // fix bug
                        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                        HeroController.instance.playerData.beamDamage = lanceDamage;

                        SetAttr(HeroController.instance, "slashComponent", HeroController.instance.normalSlash);
                        SetAttr(HeroController.instance, "slashFsm", HeroController.instance.normalSlashFsm);
                        GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables
                            .GetFsmFloat("direction").Value = HeroController.instance.cState.facingRight
                            ? 0f
                            : 180f;
                        GetAttr<NailSlash>(HeroController.instance, "slashComponent").StartSlash();

                        if (HeroController.instance.playerData.equippedCharm_19)
                        {
                            if (HeroController.instance.playerData.MPCharge > 10)
                            {
                                if (!critical)
                                {
                                    HeroController.instance.TakeMP(10);
                                }

                                if (HeroController.instance.playerData.equippedCharm_4)
                                {
                                    HeroController.instance.spell1Prefab.Spawn(
                                        HeroController.instance.transform.position + new Vector3(0f, .6f, 0f));
                                }
                                else
                                {
                                    HeroController.instance.spell1Prefab.Spawn(
                                        HeroController.instance.transform.position + new Vector3(0f, .3f, 0f));
                                }
                            }
                            else
                            {
                                GetAttr<AudioSource>(HeroController.instance, "audioSource")
                                    .PlayOneShot(HeroController.instance.blockerImpact, 1f);
                            }
                        }
                        else
                        {
                            // Grubberfly's Elegy
                            if (HeroController.instance.playerData.equippedCharm_35)
                            {
                                // Longnail AND Soul Catcher
                                if (HeroController.instance.playerData.equippedCharm_20 &&
                                    HeroController.instance.playerData.equippedCharm_18)
                                {
                                    foreach (BeamDirection x in new BeamDirection[]
                                        {BeamDirection.Left, BeamDirection.Right})
                                    {
                                        SpawnBeam(x, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .2f : 0, offset: true);

                                        SpawnBeam(x, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .9f : .7f, offset: true);
                                    }
                                }
                                // Soul Catcher
                                else if (HeroController.instance.playerData.equippedCharm_20)
                                {
                                    // attack rightwards with charm 20
                                    if (HeroController.instance.cState.facingRight)
                                    {
                                        SpawnBeam(BeamDirection.Right, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .2f : 0f, offset: true);
                                        SpawnBeam(BeamDirection.Right, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .7f : .9f, offset: true);
                                        HeroController.instance.RecoilLeftLong();
                                    }
                                    else // attack leftwards with charm 20
                                    {
                                        SpawnBeam(BeamDirection.Left, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .2f : 0f, offset: true);
                                        SpawnBeam(BeamDirection.Left, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .7f : .9f, offset: true);
                                        HeroController.instance.RecoilRightLong();
                                    }
                                }
                                else if (HeroController.instance.playerData.equippedCharm_18
                                ) // ///////////////////////// Longnail aka Silent Divide
                                {
                                    SpawnBeam(BeamDirection.Left, 1.5f, 1.5f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? .2f : .1f, offset: true);
                                    SpawnBeam(BeamDirection.Right, 1.5f, 1.5f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? .2f : .1f, offset: true);
                                }
                                else
                                {
                                    if (HeroController.instance.cState.facingRight)
                                    {
                                        SpawnBeam(BeamDirection.Right, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .2f : .1f, offset: true);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            HeroController.instance.RecoilLeftLong();
                                        }
                                        else
                                        {
                                            HeroController.instance.RecoilLeft();
                                        }
                                    }
                                    else
                                    {
                                        SpawnBeam(BeamDirection.Left, 1.5f, 1.5f, critical,
                                            positionY: PlayerData.instance.equippedCharm_4 ? .2f : .1f, offset: true);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            HeroController.instance.RecoilRightLong();
                                        }
                                        else
                                        {
                                            HeroController.instance.RecoilRight();
                                        }
                                    }
                                }
                            }
                            else if (HeroController.instance.playerData.equippedCharm_20 &&
                                     HeroController.instance.playerData.equippedCharm_18
                            ) // ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Longnail AND Soul Catcher
                            {
                                SpawnBeam(BeamDirection.Left, 1f, 1f, critical,
                                    positionY: PlayerData.instance.equippedCharm_4 ? -.2f : -.4f, offset: true);
                                SpawnBeam(BeamDirection.Left, 1f, 1f, critical,
                                    positionY: PlayerData.instance.equippedCharm_4 ? .7f : .5f, offset: true);
                                SpawnBeam(BeamDirection.Right, 1f, 1f, critical,
                                    positionY: PlayerData.instance.equippedCharm_4 ? -.2f : -.4f, offset: true);
                                SpawnBeam(BeamDirection.Right, 1f, 1f, critical,
                                    positionY: PlayerData.instance.equippedCharm_4 ? .7f : .5f, offset: true);
                            }
                            else if (HeroController.instance.playerData.equippedCharm_20
                            ) // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Soul Catcher
                            {
                                if (HeroController.instance.cState.facingRight) // attack rightwards with charm 20
                                {
                                    SpawnBeam(BeamDirection.Right, 1f, 1f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? -.2f : -.4f, offset: true);
                                    SpawnBeam(BeamDirection.Right, 1f, 1f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? .7f : .5f, offset: true);
                                }
                                else // attack leftwards with charm 20
                                {
                                    SpawnBeam(BeamDirection.Left, 1f, 1f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? -.2f : -.4f, offset: true);
                                    SpawnBeam(BeamDirection.Left, 1f, 1f, critical,
                                        positionY: PlayerData.instance.equippedCharm_4 ? .7f : .5f, offset: true);
                                }
                            }
                            // Longnail
                            else if (HeroController.instance.playerData.equippedCharm_18)
                            {
                                SpawnBeam(BeamDirection.Left, 1f, 1f, critical);
                                SpawnBeam(BeamDirection.Right, 1f, 1f, critical);
                            }
                            else // player has no charms
                            {
                                SpawnBeam(
                                    HeroController.instance.cState.facingRight
                                        ? BeamDirection.Right
                                        : BeamDirection.Left, 1f, 1f, critical);
                            }
                        }

                        break;
                    // attack upwards

                    #endregion

                    #region Upwards Attack

                    case AttackDirection.upward:
                        // Timescale Charm #14 - TIME FRACTURE //
                        if (HeroController.instance.playerData.equippedCharm_14 && _timefracture < 2f)
                        {
                            _timefracture += 0.1f;
                            SpriteFlash.flash(Color.white, 0.85f, 0.35f, 0f, 0.35f);
                        }

                        // Upward Attack Charm #8 - RISING LIGHT //
                        if (HeroController.instance.playerData.equippedCharm_8)
                        {
                            HeroController.instance.playerData.nailDamage =
                                HeroController.instance.playerData.beamDamage; // fix bug
                            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                            HeroController.instance.playerData.beamDamage = lanceDamage;

                            foreach (float i in new float[] {0.5f, 0.85f, 1.2f, 1.55f, .15f, -.2f, -.55f, -.9f})
                            {
                                SpawnBeam(BeamDirection.Up, 0.6f, 0.6f, critical, positionX: i, offset: true);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                            }
                        }

                        SetAttr(HeroController.instance, "slashComponent", HeroController.instance.upSlash);
                        SetAttr(HeroController.instance, "slashFsm", HeroController.instance.upSlashFsm);
                        HeroController.instance.cState.upAttacking = true;
                        GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables
                            .GetFsmFloat("direction").Value = 90f;
                        GetAttr<NailSlash>(HeroController.instance, "slashComponent").StartSlash();
                        break;
                    // attack downwards

                    #endregion

                    case AttackDirection.downward:
                        SetAttr(HeroController.instance, "slashComponent", HeroController.instance.downSlash);
                        SetAttr(HeroController.instance, "slashFsm", HeroController.instance.downSlashFsm);
                        HeroController.instance.cState.downAttacking = true;
                        GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables
                            .GetFsmFloat("direction").Value = 270f;
                        GetAttr<NailSlash>(HeroController.instance, "slashComponent").StartSlash();
                        break;
                }
            }

            if (HeroController.instance.playerData.equippedCharm_38)
            {
                HeroController.instance.fsm_orbitShield.SendEvent("SLASH");
            }
        }

        private void CharmUpdate(PlayerData pd, HeroController self)
        {
            // Charm Costs
            SaveGameSave();

            GameManager.instance.StartCoroutine(ChangeSprites());

            // Tiny Shell charm
            if (PlayerData.instance.equippedCharm_4)
            {
                self.transform.SetScaleX(.75f * Math.Sign(self.transform.GetScaleX()));
                self.transform.SetScaleY(.75f * Math.Sign(self.transform.GetScaleY()));
            }
            else
            {
                self.transform.SetScaleX(1f * Math.Sign(self.transform.GetScaleX()));
                self.transform.SetScaleY(1f * Math.Sign(self.transform.GetScaleY()));
            }

            if (!PlayerData.instance.equippedCharm_2)
            {
                HeroController.instance.RUN_SPEED = ORIG_RUN_SPEED;
                HeroController.instance.RUN_SPEED_CH = ORIG_RUN_SPEED_CH;
                HeroController.instance.RUN_SPEED_CH_COMBO = ORIG_RUN_SPEED_CH_COMBO;
            }

            pd.isInvincible = false;
            Time.timeScale = 1f; // reset time to normal
            _timefracture = 1f; // reset time to normal

            /*  NO LONGER NECESSARY WITH GODMASTER UPDATE
            // Respawn all ghosts and pin them!
            pd.galienPinned = true;
            pd.galienDefeated = 0;
            pd.markothPinned = true;
            pd.markothDefeated = 0;
            pd.noEyesPinned = true;
            pd.noEyesDefeated = 0;
            pd.mumCaterpillarPinned = true;
            pd.mumCaterpillarDefeated = 0;
            pd.huPinned = true;
            pd.elderHuDefeated = 0;
            pd.xeroPinned = true;
            pd.xeroDefeated = 0;
            pd.aladarPinned = true;
            pd.aladarSlugDefeated = 0;

            // resets dream boss fights
            pd.falseKnightDreamDefeated = false;
            pd.infectedKnightDreamDefeated = false;
            pd.mageLordDreamDefeated = false;
            */

            // BURNING PRIDE CALCULATIONS
            pd.nailDamage = 1 + pd.nailSmithUpgrades * 2;
            if (pd.equippedCharm_13) // Mark of Pride
            {
                pd.CountGameCompletion();
                pd.nailDamage += (int) pd.completionPercentage / 6;
            }

            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }

        private void Move(On.HeroController.orig_Move orig, HeroController self, float moveDirection)
        {
            float panicSpeed = 1f;
            if (HeroController.instance.playerData.equippedCharm_2)
            {
                int missingHealth = HeroController.instance.playerData.maxHealth -
                                    HeroController.instance.playerData.health;
                panicSpeed += missingHealth * .03f;
            }

            orig(self, !HeroController.instance.cState.inWalkZone && HeroController.instance.inAcid
                ? moveDirection * panicSpeed
                : moveDirection
            );
        }


        private Text _textObj;
        private GameObject _canvas;

        // Unused private types or members should be removed

        private void SceneLoadedHook(Scene arg0, LoadSceneMode lsm)
        {
            // Without this your shade doesn't go away when you die.
            if (GameManager.instance == null) return;
            GameManager.instance.StartCoroutine(SceneLoaded(arg0));
        }
        
        private IEnumerator<YieldInstruction> SceneLoaded(Scene arg0)
        {
            yield return null;
            yield return null;
            
            if (arg0.name == "Knight_Pickup")
            {
                PlayerData.instance.maxHealthBase = PlayerData.instance.maxHealth = PlayerData.instance.health = 4;
                PlayerData.instance.charmSlots += 1;
            }

            FsmEvent a = GameManager.instance.soulOrb_fsm.FsmEvents.FirstOrDefault(x => x.Name == "MP GAIN");
            if (a != null)
            {
                a.Name = "no";
            }
            
            GameManager.instance.StartCoroutine(ChangeSprites());

            CreateCanvas();

            // Empress Muzznik
            PlayerData.instance.CountGameCompletion();
            if (arg0.name != "Crossroads_04" || PlayerData.instance.killedBigFly) yield break;
            if (PlayerData.instance.completionPercentage > 80)
            {
                _textObj.text = "You are ready. Empress Muzznik awaits you.";
                _textObj.CrossFadeAlpha(1f, 0f, false);
                _textObj.CrossFadeAlpha(0f, 7f, false);
            }
            else if (PlayerData.instance.completionPercentage > 60)
            {
                _textObj.text = "You might just stand a chance...";
                _textObj.CrossFadeAlpha(1f, 0f, false);
                _textObj.CrossFadeAlpha(0f, 7f, false);
            }
            else
            {
                _textObj.text = "You are unworthy. Come back when you are stronger.";
                _textObj.CrossFadeAlpha(1f, 0f, false);
                _textObj.CrossFadeAlpha(0f, 7f, false);
            }
        }

        private int SoulGain(int amount)
        {
            if (!HeroController.instance.playerData.equippedCharm_15) return 0;
            _hitNumber++;
            if (_hitNumber != 5) return 0;
            HeroController.instance.AddHealth(1);
            _hitNumber = 0;
            SpriteFlash.flash(Color.red, 0.7f, 0.45f, 0f, 0.45f);
            return 0;
        }

        private void StartSlash(On.NailSlash.orig_StartSlash orig, NailSlash self)
        {
            orig(self);
            PlayMakerFSM slashFsm = GetAttr<PlayMakerFSM>(self, "slashFsm");
            float slashAngle = slashFsm.FsmVariables.FindFsmFloat("direction").Value;
            tk2dSpriteAnimator anim = GetAttr<tk2dSpriteAnimator>(self, "anim");
            if (slashAngle == 0f || slashAngle == 180f)
            {
                self.transform.localScale = new Vector3(self.scale.x * 0.32f, self.scale.y * 0.32f, self.scale.z);
                self.transform.SetPositionZ(9999f);
                anim.Play(self.animName); // nope
                return;
            }

            if (GetAttr<bool>(self, "mantis")) // burning blade
            {
                self.transform.localScale = new Vector3(self.scale.x * 1.35f, self.scale.y * 1.35f, self.scale.z);
                anim.Play(self.animName + " F");
            }
            else
            {
                self.transform.localScale = self.scale;
                anim.Play(self.animName);
            }

            if (GetAttr<bool>(self, "fury"))
            {
                anim.Play(self.animName + " F");
            }
        }

        private int TakeHealth(int amount)
        {
            PlayerData.instance.ghostCoins = 1; // for timefracture

            if (!PlayerData.instance.equippedCharm_6) return amount;
            PlayerData.instance.health = 0;
            return 0;
        }

        private void Update()
        {
            if (_timefracture < 1f || HeroController.instance.playerData.ghostCoins == 1)
            {
                HeroController.instance.playerData.ghostCoins = 0;
                _timefracture = 1f;
            }

            if (_timefracture > .99f && HeroController.instance.playerData.equippedCharm_14 &&
                !HeroController.instance.cState.isPaused)
            {
                Time.timeScale = _timefracture;
            }

            // Double Kin
            if (_kin == null && (PlayerData.instance.geo == 753 || PlayerData.instance.geo == 56))
            {
                _kin = GameObject.Find("Lost Kin");
                if (_kin != null)
                {
                    _kin.AddComponent<DoubleKin>();
                }
            }

            // EMPRESS MUZZNIK BOSS FIGHT
            if (_gruz == null)
            {
                _gruz = GameObject.Find("Giant Fly");
                if (_gruz != null)
                {
                    _gruz.AddComponent<Muzznik>();
                }
            }

            _manaRegenTime += Time.deltaTime * Time.timeScale;
            if (_manaRegenTime >= 1.11f && GameManager.instance.soulOrb_fsm != null)
            {
                // Mana regen
                _manaRegenTime -= 1.11f;
                HeroController.instance.AddMPChargeSpa(1);
                foreach (int i in new int[] {17, 19, 34, 30, 28, 22, 25})
                {
                    //if (PlayerData.instance.GetBool("equippedCharm_" + i) &&
                    if (GetAttr<bool>(PlayerData.instance, "equippedCharm_" + i) &&
                        (i != 25 || !PlayerData.instance.brokenCharm_25))
                    {
                        HeroController.instance.AddMPChargeSpa(1);
                    }
                }

                // Easter Eg.11g
                if (PlayerData.instance.geo == 753)
                {
                    HeroController.instance.AddMPChargeSpa(3);
                    int num = new Random().Next(1, 6);
                    switch (num)
                    {
                        case 1:
                            SpriteFlash.flash(Color.green, 0.6f, 0.45f, 0f, 0.45f);
                            break;
                        case 2:
                            SpriteFlash.flash(Color.red, 0.6f, 0.45f, 0f, 0.45f);
                            break;
                        case 3:
                            SpriteFlash.flash(Color.magenta, 0.6f, 0.45f, 0f, 0.45f);
                            break;
                        case 4:
                            SpriteFlash.flash(Color.yellow, 0.6f, 0.45f, 0f, 0.45f);
                            break;
                        default:
                            SpriteFlash.flash(Color.blue, 0.6f, 0.45f, 0f, 0.45f);
                            break;
                    }
                } 
                else if (PlayerData.instance.geo == 56)
                {
                    HeroController.instance.AddMPChargeSpa(3);
                    SpriteFlash.flash(Color.black, 1.11f, 0f, 1.11f, 0f);
                }
                else if (PlayerData.instance.equippedCharm_6)
                {
                    SpriteFlash.flash(Color.white, 0.6f, 0.45f, 0f, 0.45f);
                }
            }

            if (!HeroController.instance.playerData.equippedCharm_26) return;
            _passionTime += Time.deltaTime * Time.timeScale;
            if (!(_passionTime >= 2f)) return;
            _passionTime -= 2f;
            _passionDirection = !_passionDirection;
            float num2 = new Random().Next(3, 12);
            SpawnBeam(_passionDirection ? BeamDirection.Right : BeamDirection.Left, 1f, 1f,
                positionX: _passionDirection ? -num2 : num2, positionY: -0.5f + num2 / 6f, offset: true);

            // END MOD CODE
            //orig(self);
        }
    }
}
