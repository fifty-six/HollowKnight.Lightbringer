using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Lightbringer
{
    public class Lightbringer : Mod
    {
        // Empress Muzznik variables
        private GameObject Gruz;

        private bool[] GruzFight;

        private PlayMakerFSM GruzHealth;

        private GameObject GruzMinion;

        private GameObject[] GruzMinions;

        private int HitNumber;

        private int? Inter;

        // Lost Kins variables
        private GameObject Kin;

        private bool[] KinFight;

        private PlayMakerFSM KinHealth;

        private GameObject KinTwo;

        private float _invincibleTime = Time.deltaTime;

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

        private SpriteFlash SpriteFlash => GetAttr<SpriteFlash>(HeroController.instance, "spriteFlash");

        private T GetAttr<T>(object obj, string attr)
        {
            if (FieldInfoDict.ContainsKey(attr))
            {
                return (T) FieldInfoDict[attr]?.GetValue(obj);
            }

            FieldInfoDict[attr] = obj.GetType().GetField(attr, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T) FieldInfoDict[attr]?.GetValue(obj);
        }

        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += Yes;
            On.HeroController.Attack += Attack;
            On.HeroController.DoAttack += DoAttack;
            On.HeroController.SoulGain += SoulGain;
            On.HeroController.Update += Update;
            On.HeroController.Update10 += Update10;
            On.PlayerData.AddGeo += AddGeo;
            On.PlayerData.TakeHealth += TakeHealth;
            On.NailSlash.StartSlash += StartSlash;
            On.HeroController.CharmUpdate += CharmUpdate;
            On.HeroController.Move += Move;
            // UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private Dictionary<string, FieldInfo> FieldInfoDict = new Dictionary<string, FieldInfo>();
        private void SetAttr<T>(object obj, string attr, T val)
        {
            if (FieldInfoDict.ContainsKey(attr))
            {
                FieldInfoDict[attr]?.SetValue(obj, val);
            }
            else
            {
                FieldInfoDict[attr] = obj.GetType().GetField(attr, BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfoDict[attr]?.SetValue(obj, val);
            }
        }

        private void AddGeo(On.PlayerData.orig_AddGeo orig, PlayerData self, int amount)
        {
            // Don't let Faulty Wallet hurt people with full SOUL
            if (PlayerData.instance.equippedCharm_21 && (PlayerData.instance.MPCharge < 99 || PlayerData.instance.MPReserve != PlayerData.instance.MPReserveMax))
            {
                PlayerData.instance.AddMPCharge(amount * 3);
            }
            else
            {
                PlayerData.instance.geo += amount;
                if (PlayerData.instance.geo > 9999999)
                {
                    PlayerData.instance.geo = 9999999;
                }
            }
        }

        private void Attack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            {
                HeroController.instance.cState.altAttack = false;
                HeroController.instance.cState.attacking = true;

                // Damage Controller //////////////////////////////////////////////////////////////////////////
                // NAIL
                HeroController.instance.playerData.nailDamage = 1 + (HeroController.instance.playerData.nailSmithUpgrades * 2);
                if (HeroController.instance.playerData.equippedCharm_13) // Mark of Pride
                {
                    HeroController.instance.playerData.CountGameCompletion();
                    HeroController.instance.playerData.nailDamage += (int)HeroController.instance.playerData.completionPercentage / 8;
                }
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                // LANCE
                HeroController.instance.playerData.beamDamage = 3 + (HeroController.instance.playerData.nailSmithUpgrades * 3);
                if (HeroController.instance.playerData.equippedCharm_35) // Radiant Jewel charm replacing Grubberfly's Elegy
                {
                    HeroController.instance.playerData.beamDamage += 5;
                }
                if (HeroController.instance.playerData.equippedCharm_25 && HeroController.instance.playerData.MPCharge > 3) // Fragile Strength > Fragile Nightmare
                {
                    HeroController.instance.playerData.beamDamage += HeroController.instance.playerData.MPCharge / 20;
                    HeroController.instance.TakeMP(7);
                }
                if (HeroController.instance.playerData.equippedCharm_6) // Glass Soul charm replacing Fury of Fallen
                {
                    HeroController.instance.playerData.beamDamage += HeroController.instance.playerData.health + HeroController.instance.playerData.healthBlue - 3;
                }

                bool critical = false;
                if (HeroController.instance.playerData.equippedCharm_3) // Bloodsong replaces Grubsong
                {
                    Random rnd = new Random(); // CRITICAL HIT CHARM
                    int critChance = rnd.Next(1, 101);
                    HeroController.instance.playerData.CountJournalEntries();
                    int critThreshold = 100 - (HeroController.instance.playerData.journalEntriesCompleted / 10);
                    if (critChance > Math.Min(critThreshold, 96))
                    {
                        critical = true;
                    }
                    if (critical)
                    {
                        HeroController.instance.playerData.beamDamage *= 3;
                        HeroController.instance.shadowRingPrefab.Spawn(HeroController.instance.transform.position);
                        GetAttr<AudioSource>(HeroController.instance, "audioSource").PlayOneShot(HeroController.instance.nailArtChargeComplete, 1f);
                    }
                }

                int lanceDamage = HeroController.instance.playerData.beamDamage;
                // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // QUICK SLASH CHARM #32
                SetAttr(HeroController.instance, "attackDuration", HeroController.instance.playerData.equippedCharm_32
                    ? HeroController.instance.ATTACK_DURATION_CH
                    : HeroController.instance.ATTACK_DURATION);

                if (HeroController.instance.cState.wallSliding)
                {
                    HeroController.instance.playerData.nailDamage = HeroController.instance.playerData.beamDamage; // fix bug
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                    HeroController.instance.playerData.beamDamage = lanceDamage;

                    SetAttr(HeroController.instance, "wallSlashing", true);
                    if (HeroController.instance.cState.facingRight)
                    {
                        GrubberFlyBeam = critical
                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance.transform
                                .position)
                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance.transform
                                .position);
                        GrubberFlyBeam.transform.SetScaleX(1f);
                        GrubberFlyBeam.transform.SetScaleY(1f);
                    }
                    else
                    {
                        GrubberFlyBeam = critical
                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController.instance.transform
                                .position)
                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance.transform
                                .position);
                        GrubberFlyBeam.transform.SetScaleX(-1f);
                        GrubberFlyBeam.transform.SetScaleY(1f);
                    }
                }
                else
                {
                    SetAttr(HeroController.instance, "wallSlashing", false);
                    switch (attackDir)
                    {
                        case AttackDirection.normal:
                            HeroController.instance.playerData.nailDamage = HeroController.instance.playerData.beamDamage; // fix bug
                            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                            HeroController.instance.playerData.beamDamage = lanceDamage;

                            SetAttr(HeroController.instance, "slashComponent", HeroController.instance.normalSlash);
                            SetAttr(HeroController.instance, "slashFsm", HeroController.instance.normalSlashFsm);
                            GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables.GetFsmFloat("direction").Value = HeroController.instance.cState.facingRight
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
                                        HeroController.instance.spell1Prefab.Spawn(HeroController.instance.transform.position + new Vector3(0f, .6f, 0f));
                                    }
                                    else
                                    {
                                        HeroController.instance.spell1Prefab.Spawn(HeroController.instance.transform.position + new Vector3(0f, .3f, 0f));
                                    }
                                }
                                else
                                {
                                    GetAttr<AudioSource>(HeroController.instance, "audioSource").PlayOneShot(HeroController.instance.blockerImpact, 1f);
                                }
                            }
                            else
                            {
                                if (HeroController.instance.playerData.equippedCharm_35) /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Grubberfly's Elegy
                                {
                                    if (HeroController.instance.playerData.equippedCharm_20 && HeroController.instance.playerData.equippedCharm_18) ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Longnail AND Soul Catcher
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                        }

                                        GrubberFlyBeam.transform.SetScaleX(1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);

                                        GrubberFlyBeam = critical ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance.transform.position) : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance.transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.9f);
                                        }

                                        GrubberFlyBeam.transform.SetScaleX(1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);

                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                        }

                                        GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);

                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.9f);
                                        }

                                        GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);
                                    }
                                    else if (HeroController.instance.playerData.equippedCharm_20) ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Soul Catcher
                                    {
                                        if (HeroController.instance.cState.facingRight) // attack rightwards with charm 20
                                        {
                                            GrubberFlyBeam = critical
                                                ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                    .instance.transform.position)
                                                : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController
                                                    .instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);

                                            GrubberFlyBeam = critical ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController.instance.transform.position) : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.9f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);

                                            HeroController.instance.RecoilLeftLong();
                                        }
                                        else // attack leftwards with charm 20
                                        {
                                            GrubberFlyBeam = critical
                                                ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                    .instance.transform.position)
                                                : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController
                                                    .instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);

                                            GrubberFlyBeam = critical ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance.transform.position) : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.9f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);

                                            HeroController.instance.RecoilRightLong();
                                        }
                                    }
                                    else if (HeroController.instance.playerData.equippedCharm_18) // ///////////////////////// Longnail aka Silent Divide
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.1f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.1f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                        GrubberFlyBeam.transform.SetScaleY(1.5f);
                                    }
                                    else
                                    {
                                        if (HeroController.instance.cState.facingRight)
                                        {
                                            GrubberFlyBeam = critical
                                                ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                    .instance.transform.position)
                                                : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController
                                                    .instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.1f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(-1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);
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
                                            GrubberFlyBeam = critical
                                                ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                    .instance.transform.position)
                                                : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController
                                                    .instance.transform.position);
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.1f);
                                            if (HeroController.instance.playerData.equippedCharm_4)
                                            {
                                                GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.2f);
                                            }
                                            GrubberFlyBeam.transform.SetScaleX(1.5f);
                                            GrubberFlyBeam.transform.SetScaleY(1.5f);
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
                                else if (HeroController.instance.playerData.equippedCharm_20 && HeroController.instance.playerData.equippedCharm_18) // ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Longnail AND Soul Catcher
                                {
                                    GrubberFlyBeam = critical
                                        ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance
                                            .transform.position)
                                        : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                            .transform.position);
                                    GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.4f);
                                    if (HeroController.instance.playerData.equippedCharm_4)
                                    {
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.2f);
                                    }

                                    GrubberFlyBeam.transform.SetScaleX(1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);

                                    GrubberFlyBeam = critical
                                        ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance
                                            .transform.position)
                                        : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                            .transform.position);
                                    GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.5f);
                                    if (HeroController.instance.playerData.equippedCharm_4)
                                    {
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                    }

                                    GrubberFlyBeam.transform.SetScaleX(1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);

                                    GrubberFlyBeam = critical ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController.instance.transform.position) : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance.transform.position);
                                    GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.4f);
                                    if (HeroController.instance.playerData.equippedCharm_4)
                                    {
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.2f);
                                    }

                                    GrubberFlyBeam.transform.SetScaleX(-1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);

                                    GrubberFlyBeam = critical
                                        ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController.instance
                                            .transform.position)
                                        : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                            .transform.position);
                                    GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.5f);
                                    if (HeroController.instance.playerData.equippedCharm_4)
                                    {
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                    }

                                    GrubberFlyBeam.transform.SetScaleX(-1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);
                                }
                                else if (HeroController.instance.playerData.equippedCharm_20) // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Soul Catcher
                                {
                                    if (HeroController.instance.cState.facingRight) // attack rightwards with charm 20
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.4f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.2f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(-1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);

                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.5f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(-1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);
                                    }
                                    else // attack leftwards with charm 20
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.4f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.2f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);

                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.5f);
                                        if (HeroController.instance.playerData.equippedCharm_4)
                                        {
                                            GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() + 0.7f);
                                        }
                                        GrubberFlyBeam.transform.SetScaleX(1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);
                                    }
                                }
                                else if (HeroController.instance.playerData.equippedCharm_18) // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Longnail
                                {
                                    GrubberFlyBeam = critical
                                        ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController.instance
                                            .transform.position)
                                        : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                            .transform.position);
                                    GrubberFlyBeam.transform.SetScaleX(1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);
                                    GrubberFlyBeam = critical
                                        ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController.instance
                                            .transform.position)
                                        : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                            .transform.position);
                                    GrubberFlyBeam.transform.SetScaleX(-1f);
                                    GrubberFlyBeam.transform.SetScaleY(1f);
                                }
                                else // player has no charms
                                {
                                    if (HeroController.instance.cState.facingRight) // attack rightwards
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabR_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetScaleX(-1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);
                                    }
                                    else // attack leftwards
                                    {
                                        GrubberFlyBeam = critical
                                            ? HeroController.instance.grubberFlyBeamPrefabL_fury.Spawn(HeroController
                                                .instance.transform.position)
                                            : HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance
                                                .transform.position);
                                        GrubberFlyBeam.transform.SetScaleX(1f);
                                        GrubberFlyBeam.transform.SetScaleY(1f);
                                    }
                                }
                            }

                            break;
                        // attack upwards
                        case AttackDirection.upward:
                            // Timescale Charm #14
                            if (HeroController.instance.playerData.equippedCharm_14 && _timefracture < 2f)
                            {
                                _timefracture += 0.1f;
                                SpriteFlash.flash(Color.white, 0.85f, 0.35f, 0f, 0.35f);
                            }

                            // UP ATTACK CHARM 8 - RISING LIGHT//
                            if (HeroController.instance.playerData.equippedCharm_8)
                            {
                                HeroController.instance.playerData.nailDamage = HeroController.instance.playerData.beamDamage; // fix bug
                                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                                HeroController.instance.playerData.beamDamage = lanceDamage;

                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + 0.5f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + 0.85f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + 1.2f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + 1.55f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + 0.15f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() - 0.2f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() - 0.55f);
                                GrubberFlyBeam = critical
                                    ? HeroController.instance.grubberFlyBeamPrefabU_fury.Spawn(HeroController.instance
                                        .transform.position)
                                    : HeroController.instance.grubberFlyBeamPrefabU.Spawn(HeroController.instance.transform
                                        .position);
                                GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                                GrubberFlyBeam.transform.SetScaleX(0.6f);
                                GrubberFlyBeam.transform.SetScaleY(0.6f);
                                GrubberFlyBeam.transform.SetPositionX(
                                    HeroController.instance.transform.GetPositionX() - 0.9f);
                            }

                            SetAttr(HeroController.instance, "slashComponent", HeroController.instance.upSlash);
                            SetAttr(HeroController.instance, "slashFsm", HeroController.instance.upSlashFsm);
                            HeroController.instance.cState.upAttacking = true;
                            GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables.GetFsmFloat("direction").Value = 90f;
                            GetAttr<NailSlash>(HeroController.instance, "slashComponent").StartSlash();
                            break;
                        // attack downwards
                        case AttackDirection.downward:
                            SetAttr(HeroController.instance, "slashComponent", HeroController.instance.downSlash);
                            SetAttr(HeroController.instance, "slashFsm", HeroController.instance.downSlashFsm);
                            HeroController.instance.cState.downAttacking = true;
                            GetAttr<PlayMakerFSM>(HeroController.instance, "slashFsm").FsmVariables.GetFsmFloat("direction").Value = 270f;
                            GetAttr<NailSlash>(HeroController.instance, "slashComponent").StartSlash();
                            break;
                    }
                }

                if (HeroController.instance.playerData.equippedCharm_38)
                {
                    HeroController.instance.fsm_orbitShield.SendEvent("SLASH");
                }
            }
        }

        private void CharmUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
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

            PlayerData.instance.isInvincible = false;
            Time.timeScale = 1f; // reset time to normal
            _timefracture = 1f; // reset time to normal

            PlayerData.instance.charmCost_21 = 1; // Faulty Wallet update patch
            PlayerData.instance.charmCost_19 = 4; // Eye of the Storm update patch
            PlayerData.instance.charmCost_15 = 3; // Bloodlust update patch
            PlayerData.instance.charmCost_14 = 2; // Glass Soul update patch
            PlayerData.instance.charmCost_8 = 3; // Rising Light update patch
            PlayerData.instance.charmCost_35 = 5; // Radiant Jewel update patch
            PlayerData.instance.charmCost_18 = 3; // Silent Divide update patch
            PlayerData.instance.charmCost_3 = 2; // Bloodsong update patch

            // Respawn all ghosts and pin them!
            PlayerData.instance.galienPinned = true;
            PlayerData.instance.galienDefeated = 0;
            PlayerData.instance.markothPinned = true;
            PlayerData.instance.markothDefeated = 0;
            PlayerData.instance.noEyesPinned = true;
            PlayerData.instance.noEyesDefeated = 0;
            PlayerData.instance.mumCaterpillarPinned = true;
            PlayerData.instance.mumCaterpillarDefeated = 0;
            PlayerData.instance.huPinned = true;
            PlayerData.instance.elderHuDefeated = 0;
            PlayerData.instance.xeroPinned = true;
            PlayerData.instance.xeroDefeated = 0;
            PlayerData.instance.aladarPinned = true;
            PlayerData.instance.aladarSlugDefeated = 0;

            // resets dream boss fights
            PlayerData.instance.falseKnightDreamDefeated = false;
            PlayerData.instance.infectedKnightDreamDefeated = false;
            PlayerData.instance.mageLordDreamDefeated = false;

            // BURNING PRIDE CALCULATIONS
            PlayerData.instance.nailDamage = 1 + (PlayerData.instance.nailSmithUpgrades * 2);
            if (PlayerData.instance.equippedCharm_13) // Mark of Pride
            {
                PlayerData.instance.CountGameCompletion();
                PlayerData.instance.nailDamage += (int)PlayerData.instance.completionPercentage / 6;
            }
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }

        private void DoAttack(On.HeroController.orig_DoAttack orig, HeroController self)
        {
            // HeroController.instance.GetType().GetMethod("ResetLook").Invoke(HeroController.instance, null);
            HeroController.instance.cState.recoiling = false;

            SetAttr(HeroController.instance, "attack_cooldown",
                HeroController.instance.playerData.equippedCharm_32
                    ? HeroController.instance.ATTACK_COOLDOWN_TIME_CH
                    : HeroController.instance.ATTACK_COOLDOWN_TIME);

            if (HeroController.instance.vertical_input > Mathf.Epsilon)
            {
                HeroController.instance.Attack(AttackDirection.upward);
                HeroController.instance.StartCoroutine(HeroController.instance.CheckForTerrainThunk(AttackDirection.upward));
            }
            else if (HeroController.instance.vertical_input < -Mathf.Epsilon)
            {
                if (HeroController.instance.hero_state != ActorStates.idle && HeroController.instance.hero_state != ActorStates.running)
                {
                    HeroController.instance.Attack(AttackDirection.downward);
                    HeroController.instance.StartCoroutine(HeroController.instance.CheckForTerrainThunk(AttackDirection.downward));
                }
                else
                {
                    HeroController.instance.Attack(AttackDirection.normal);
                }
            }
            else
            {
                HeroController.instance.Attack(AttackDirection.normal);
            }
        }

        private void Move(On.HeroController.orig_Move orig, HeroController self, float moveDirection)
        {
            // ReSharper disable once NotAccessedVariable
            float panicSpeed = 1f;
            if (HeroController.instance.playerData.equippedCharm_2)
            {
                int missingHealth = HeroController.instance.playerData.maxHealth - HeroController.instance.playerData.health;
                // ReSharper disable once RedundantAssignment
                panicSpeed += missingHealth * .03f;
            }

            orig(self, moveDirection);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable twice UnusedParameter.Local
        // Unused private types or members should be removed
        private void SceneChanged(Scene arg0, Scene arg1)
        {
            Inter = null;
            // nope
            // breaks on scene entry, etc

            // Text Display code
            //if (c == null)
            //{
            //    c = new GameObject();
            //    UnityEngine.Object.DontDestroyOnLoad(c);
            //    c.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            //    CanvasScaler canvasScaler = c.AddComponent<CanvasScaler>();
            //    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //    canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            //    c.AddComponent<GraphicRaycaster>();
            //    GameObject gameObject = new GameObject();
            //    gameObject.transform.parent = c.transform;
            //    gameObject.AddComponent<CanvasRenderer>();
            //    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            //    rectTransform.anchorMax = new Vector2(1f, 0f);
            //    rectTransform.anchorMin = new Vector2(0f, 0f);
            //    rectTransform.pivot = new Vector2(0.5f, 0.5f);
            //    rectTransform.sizeDelta = new Vector2(0f, 50f);
            //    rectTransform.anchoredPosition = new Vector2(0f, 45f);
            //    textObj = gameObject.AddComponent<Text>();
            //    textObj.font = Modding.CanvasUtil.TrajanBold;
            //    textObj.text = "";
            //    textObj.fontSize = 42;
            //    textObj.alignment = TextAnchor.MiddleCenter;
            //}

            GameManager.instance.sceneName = GameManager.GetBaseSceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            // Empress Muzznik
            PlayerData.instance.CountGameCompletion();
            //if (PlayerData.instance.completionPercentage > 80)
            //{
            //    textObj.text = "You are ready. Empress Muzznik awaits you.";
            //    textObj.CrossFadeAlpha(1f, 0f, false);
            //    textObj.CrossFadeAlpha(0f, 7f, false);
            //}
            //else if (PlayerData.instance.completionPercentage > 60)
            //{
            //    textObj.text = "You might just stand a chance...";
            //    textObj.CrossFadeAlpha(1f, 0f, false);
            //    textObj.CrossFadeAlpha(0f, 7f, false);
            //}
            //else
            //{
            //    textObj.text = "You are unworthy. Come back when you are stronger.";
            //    textObj.CrossFadeAlpha(1f, 0f, false);
            //    textObj.CrossFadeAlpha(0f, 7f, false);
            //}

            // find and save data for Empress Muzznik
            Gruz = GameObject.Find("Giant Fly");
            if (Gruz != null)
            {
                GruzHealth = FSMUtility.LocateFSM(Gruz, "health_manager_enemy");
                FSMUtility.SetInt(GruzHealth, "HP", 1500);
                //gruzFight = new bool[12];
                //gruzMinions = new GameObject[16];
                GruzMinion = GameObject.Find("Fly");
                GruzMinion.transform.SetScaleY(-1f);
                FSMUtility.SetInt(FSMUtility.LocateFSM(GruzMinion, "health_manager_enemy"), "HP", 99999);
            }

            // Lost Kins
            if (GameManager.instance.sceneName == "Dream_03_Infected_Knight")
            {
                // find and save data for Lost Kin
                Kin = GameObject.Find("Lost Kin");
                KinHealth = FSMUtility.LocateFSM(Kin, "health_manager_enemy");
                KinFight = new bool[12];
            }
            Log("ur mum gayer");
        }

        private void SoulGain(On.HeroController.orig_SoulGain orig, HeroController self)
        {
            if (!HeroController.instance.playerData.equippedCharm_15) return;
            HitNumber++;
            if (HitNumber != 5) return;
            HeroController.instance.AddHealth(1);
            HitNumber = 0;
            SpriteFlash.flash(Color.red, 0.7f, 0.45f, 0f, 0.45f);
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

        private void TakeHealth(On.PlayerData.orig_TakeHealth orig, PlayerData self, int amount)
        {
            PlayerData.instance.ghostCoins = 1; // for timefracture

            if (PlayerData.instance.equippedCharm_6)
            {
                PlayerData.instance.health = 0;
                return;
            }

            if (PlayerData.instance.healthBlue > 0)
            {
                PlayerData.instance.damagedBlue = true;
                PlayerData.instance.healthBlue -= amount;
                if (PlayerData.instance.healthBlue < 0)
                {
                    PlayerData.instance.health += PlayerData.instance.healthBlue;
                }
            }
            else
            {
                PlayerData.instance.damagedBlue = false;
                if (PlayerData.instance.health - amount <= 0)
                {
                    PlayerData.instance.health = 0;
                    return;
                }
                PlayerData.instance.health -= amount;
            }
        }

        private void Update(On.HeroController.orig_Update orig, HeroController self)
        {
            // START MOD CODE
            if (_timefracture < 1f || HeroController.instance.playerData.ghostCoins == 1)
            {
                HeroController.instance.playerData.ghostCoins = 0;
                _timefracture = 1f;
            }
            if (_timefracture > .99f && HeroController.instance.playerData.equippedCharm_14 && !HeroController.instance.cState.isPaused)
            {
                Time.timeScale = _timefracture;
            }

            if (Kin != null && PlayerData.instance.geo == 753)
            {
                int kinHp = KinHealth.FsmVariables.GetFsmInt("HP").Value;
                if (!KinFight[0] && kinHp < 400)
                {
                    KinFight[0] = true;
                    HeroController.instance.playerData.isInvincible = true; // temporary invincibility iFrames
                    SpriteFlash.flash(Color.black, 0.6f, 0.15f, 0f, 0.55f);
                    KinFight[5] = true; // iFrames
                    KinTwo = Object.Instantiate(Kin);
                    FSMUtility.SetInt(FSMUtility.LocateFSM(KinTwo, "health_manager_enemy"), "HP", 99999);
                }
                else if (KinFight[5]) // iFrames
                {
                    _invincibleTime += Time.deltaTime;
                    if (_invincibleTime >= 5.5f)
                    {
                        HeroController.instance.playerData.isInvincible = false;
                        KinFight[5] = false;
                    }
                }
                else if (!KinFight[1] && kinHp < 1)
                {
                    KinFight[1] = true;
                    FSMUtility.SetInt(FSMUtility.LocateFSM(KinTwo, "health_manager_enemy"), "HP", 1);
                }
            }

            // EMPRESS MUZZNIK BOSS FIGHT
            if (Gruz != null)
            {
                int gruzHp = GruzHealth.FsmVariables.GetFsmInt("HP").Value;
                if (!GruzFight[0] && gruzHp < 1470)
                {
                    GruzFight[0] = true;
                    GruzMinions[0] = GruzMinion.Spawn(Gruz.transform.position); // dud
                    GruzMinions[1] = GruzMinion.Spawn(Gruz.transform.position);
                    GruzMinions[1].transform.SetScaleX(1.3f);
                    GruzMinions[1].transform.SetScaleY(-1.3f);
                }
                else if (!GruzFight[1] && gruzHp < 1100)
                {
                    GruzFight[1] = true;
                    GruzMinions[2] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[2] && gruzHp < 800)
                {
                    GruzFight[2] = true;
                    GruzMinions[3] = GruzMinion.Spawn(Gruz.transform.position);
                    GruzMinions[3].transform.SetScaleX(.8f);
                    GruzMinions[3].transform.SetScaleY(-.8f);
                }
                else if (!GruzFight[3] && gruzHp < 600)
                {
                    GruzFight[3] = true;
                    GruzMinions[4] = GruzMinion.Spawn(Gruz.transform.position);
                    GruzMinions[4].transform.SetScaleX(.8f);
                    GruzMinions[4].transform.SetScaleY(-.8f);
                }
                else if (!GruzFight[4] && gruzHp < 500)
                {
                    GruzFight[4] = true;
                    GruzMinions[5] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[5] && gruzHp < 400)
                {
                    GruzFight[5] = true;
                    GruzMinions[6] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[6] && gruzHp < 300)
                {
                    GruzFight[6] = true;
                    GruzMinions[7] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[7] && gruzHp < 200)
                {
                    GruzFight[7] = true;
                    GruzMinions[8] = GruzMinion.Spawn(Gruz.transform.position);
                    GruzMinions[9] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[8] && gruzHp < 100)
                {
                    GruzFight[8] = true;
                    GruzMinions[10] = GruzMinion.Spawn(Gruz.transform.position);
                    GruzMinions[11] = GruzMinion.Spawn(Gruz.transform.position);
                }
                else if (!GruzFight[9] && gruzHp < 1)
                {
                    GruzFight[9] = true;
                    FSMUtility.SetInt(FSMUtility.LocateFSM(GruzMinion, "health_manager_enemy"), "HP", 1);
                    for (int i = 0; i < 12; i++)
                    {
                        FSMUtility.SetInt(FSMUtility.LocateFSM(GruzMinions[i], "health_manager_enemy"), "HP", 1);
                    }
                    Gruz = null;
                }
            }

            _manaRegenTime += Time.deltaTime * Time.timeScale;
            if (_manaRegenTime >= 1.11f)
            {
                // Mana regen
                _manaRegenTime -= 1.11f;
                HeroController.instance.AddMPChargeSpa(1);
                if (HeroController.instance.playerData.equippedCharm_17)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_19)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_34)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_30)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_28)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_22)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }
                if (HeroController.instance.playerData.equippedCharm_25 && !HeroController.instance.playerData.brokenCharm_25)
                {
                    HeroController.instance.AddMPChargeSpa(1);
                }

                // Easter Egg
                if (HeroController.instance.playerData.geo == 753)
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
                else if (HeroController.instance.playerData.equippedCharm_6)
                {
                    SpriteFlash.flash(Color.white, 0.6f, 0.45f, 0f, 0.45f);
                }
            }

            if (HeroController.instance.playerData.equippedCharm_26) // Nailmaster's Passion
            {
                _passionTime += Time.deltaTime * Time.timeScale;
                if (_passionTime >= 2f)
                {
                    _passionTime -= 2f;
                    _passionDirection = !_passionDirection;
                    float num2 = new Random().Next(3, 12);
                    if (_passionDirection)
                    {
                        GrubberFlyBeam = HeroController.instance.grubberFlyBeamPrefabR.Spawn(HeroController.instance.transform.position);
                        GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() - num2);
                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.5f + (num2 / 6f));
                        GrubberFlyBeam.transform.SetScaleX(-1f);
                        GrubberFlyBeam.transform.SetScaleY(1f);
                    }
                    else
                    {
                        GrubberFlyBeam = HeroController.instance.grubberFlyBeamPrefabL.Spawn(HeroController.instance.transform.position);
                        GrubberFlyBeam.transform.SetPositionX(HeroController.instance.transform.GetPositionX() + num2);
                        GrubberFlyBeam.transform.SetPositionY(HeroController.instance.transform.GetPositionY() - 0.5f + (num2 / 6f));
                        GrubberFlyBeam.transform.SetScaleX(1f);
                        GrubberFlyBeam.transform.SetScaleY(1f);
                    }
                }
            }

            // END MOD CODE
            orig(self);
        }

        private void Update10(On.HeroController.orig_Update10 orig, HeroController self)
        {
            if (GetAttr<bool>(self, "isGameplayScene"))
            {
                Vector2 vector = self.transform.position;
                if ((vector.y >= -60f && vector.y <= GameManager.instance.sceneHeight + 60f && vector.x >= -60f && vector.x <= GameManager.instance.sceneWidth + 60f) || self.cState.dead || !GetAttr<bool>(self, "boundsChecking"))
                {
                }
            }
            float scaleX = self.transform.GetScaleX();
            if (scaleX < -1f)
            {
                self.transform.SetScaleX(-Math.Abs(self.transform.GetScaleX()));
            }
            if (scaleX > 1f)
            {
                self.transform.SetScaleX(Math.Abs(self.transform.GetScaleX()));
            }
            if (self.transform.position.z != 0.004f)
            {
                self.transform.SetPositionZ(0.004f);
            }
        }

        /*
                public GameObject C;
                public Text TextObj;
        */

        private bool Yes(On.HeroController.orig_CanOpenInventory orig, HeroController self)
        {
            return true;
        }
    }
}