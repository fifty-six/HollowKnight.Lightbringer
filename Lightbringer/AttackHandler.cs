using System;
using System.Collections.Generic;
using GlobalEnums;
using ModCommon.Util;
using UnityEngine;
using static Lightbringer.AttackHandler.BeamDirection;
using Random = System.Random;

namespace Lightbringer
{
    public class AttackHandler
    {
        private static GameObject GrubberFlyBeam
        {
            get => HeroController.instance.GetAttr<GameObject>("grubberFlyBeam");
            set => HeroController.instance.SetAttr("grubberFlyBeam", value);
        }
        
        private float _timeFracture;
        private bool _crit;

        public AttackHandler() {}

        public AttackHandler(float fracture)
        {
            _timeFracture = fracture;
        }

        private static Random _rand => Lightbringer.Random;

        private static SpriteFlash SpriteFlash => Lightbringer.SpriteFlash;

        public void Attack(HeroController hc, AttackDirection dir)
        {
            PlayerData pd = PlayerData.instance;

            hc.cState.altAttack = false;
            hc.cState.attacking = true;

            #region Damage Controller

            // NAIL
            pd.nailDamage = 1 + pd.nailSmithUpgrades * 2;
            // Mark of Pride
            if (pd.equippedCharm_13)
            {
                pd.CountGameCompletion();
                pd.nailDamage += (int) pd.completionPercentage / 8;
            }

            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");

            // LANCE
            pd.beamDamage = 3 + pd.nailSmithUpgrades * 3;

            // Radiant Jewel (Elegy)
            if (pd.equippedCharm_35) pd.beamDamage += 5;

            // Fragile Nightmare damage will be factored in only when firing lances
            if (pd.equippedCharm_6) // Glass Soul charm replacing Fury of Fallen
                pd.beamDamage += pd.health + pd.healthBlue - 3;

            #endregion

            _crit = CalculateCrit(hc, pd);

            int lanceDamage = pd.beamDamage;

            // QUICK SLASH CHARM #REEE32
            hc.SetAttr("attackDuration", pd.equippedCharm_32
                             ? hc.ATTACK_DURATION_CH
                             : hc.ATTACK_DURATION);

            // Fragile Nightmare damage calculations
            if (pd.equippedCharm_25 &&
                pd.MPCharge > 3) // Fragile Strength > Fragile Nightmare
            {
                pd.beamDamage += pd.MPCharge / 20;
                hc.TakeMP(7);
            }

            if (pd.equippedCharm_38) hc.fsm_orbitShield.SendEvent("SLASH");

            if (hc.cState.wallSliding)
            {
                pd.nailDamage = pd.beamDamage; 
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                pd.beamDamage = lanceDamage;
                hc.SetAttr("wallSlashing", true);
                SpawnBeam(!hc.cState.facingRight, 1f, 1f);
                return;
            }

            hc.SetAttr("wallSlashing", false);
            switch (dir)
            {
                #region Normal Attack

                case AttackDirection.normal:
                    // fix bug
                    pd.nailDamage = pd.beamDamage;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                    pd.beamDamage = lanceDamage;

                    hc.SetAttr("slashComponent", hc.normalSlash);

                    hc.normalSlashFsm.FsmVariables.GetFsmFloat("direction").Value = hc.cState.facingRight
                        ? 0f
                        : 180f;
                    HeroController.instance.SetAttr("slashFsm", hc.normalSlashFsm);

                    hc.normalSlash.StartSlash();

                    bool tShell = pd.equippedCharm_4;

                    if (pd.equippedCharm_19)
                    {
                        if (pd.MPCharge > 10)
                        {
                            if (!_crit) hc.TakeMP(10);

                            hc.spell1Prefab.Spawn(
                                hc.transform.position +
                                (tShell
                                    ? new Vector3(0f, .6f)
                                    : new Vector3(0f, .3f)));
                        }
                        else
                        {
                            hc.GetAttr<AudioSource>("audioSource")
                                .PlayOneShot(hc.blockerImpact, 1f);
                        }

                        return;
                    }

                    // Grubberfly's Elegy
                    if (pd.equippedCharm_35)
                    {
                        // Longnail AND/OR Soul Catcher
                        if (pd.equippedCharm_20)
                        {
                            bool longnail = pd.equippedCharm_18;

                            if (hc.cState.facingRight || longnail)
                                SpawnBeams(Right, 1.5f, 1.5f, positionY: tShell ? new[] {.2f, .7f} : new[] {0f, .9f});

                            if (!hc.cState.facingRight || longnail)
                                SpawnBeams(Left, 1.5f, 1.5f, positionY: tShell ? new[] {.2f, .7f} : new[] {0f, .9f});
                        }
                        // Longnail
                        else if (pd.equippedCharm_18)
                        {
                            SpawnBeams(1.5f, 1.5f, positionY: tShell ? .2f : .1f);
                        }
                        else
                        {
                            SpawnBeam(hc.cState.facingRight, 1.5f, 1.5f, positionY: tShell ? .2f : .1f, recoil: tShell);
                        }
                    }
                    // Longnail AND Soul Catcher
                    else if (pd.equippedCharm_20 && pd.equippedCharm_18)
                    {
                        SpawnBeams(1f, 1f, positionY: tShell ? new float[] {-.2f, .7f} : new float[] {.5f, -.4f});
                    }
                    // Soul Catcher
                    else if (pd.equippedCharm_20)
                    {
                        SpawnBeams(hc.cState.facingRight, 1f, 1f, positionY: tShell ? new[] {-.2f, .7f} : new[] {.5f, -.4f}, recoils: Recoils.None);
                    }
                    // Longnail
                    else if (pd.equippedCharm_18)
                    {
                        SpawnBeams(1f, 1f);
                    }
                    else // player has no charms
                    {
                        SpawnBeam(hc.cState.facingRight, 1f, 1f);
                    }

                    break;
                // attack upwards

                #endregion

                #region Upwards Attack

                case AttackDirection.upward:
                    // Timescale Charm #14 - TIME FRACTURE //
                    if (pd.equippedCharm_14 && _timeFracture < 2f)
                    {
                        _timeFracture += 0.1f;
                        SpriteFlash.flash(Color.white, 0.85f, 0.35f, 0f, 0.35f);
                    }

                    // Upward Attack Charm #8 - RISING LIGHT //
                    if (pd.equippedCharm_8)
                    {
                        // Fragile Nightmare damage calculations
                        if (pd.equippedCharm_25 &&
                            pd.MPCharge > 3)
                        {
                            pd.beamDamage += pd.MPCharge / 20;
                            hc.TakeMP(7);
                        }


                        pd.nailDamage = pd.beamDamage;
                        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                        pd.beamDamage = lanceDamage;

                        foreach (float i in new float[] {0.5f, 0.85f, 1.2f, 1.55f, .15f, -.2f, -.55f, -.9f})
                        {
                            SpawnBeam(Up, 0.6f, 0.6f, i);
                            GrubberFlyBeam.transform.Rotate(0f, 0f, -90f);
                        }
                    }

                    hc.SetAttr("slashComponent", hc.upSlash);
                    hc.SetAttr("slashFsm", hc.upSlashFsm);
                    hc.cState.upAttacking = true;
                    hc.GetAttr<PlayMakerFSM>("slashFsm")
                        .FsmVariables.GetFsmFloat("direction")
                        .Value = 90f;
                    hc.GetAttr<NailSlash>("slashComponent").StartSlash();
                    break;

                #endregion

                #region Down

                case AttackDirection.downward:
                    hc.SetAttr("slashComponent", hc.downSlash);
                    hc.SetAttr("slashFsm", hc.downSlashFsm);
                    hc.cState.downAttacking = true;
                    hc.downSlashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
                    hc.GetAttr<NailSlash>("slashComponent").StartSlash();
                    break;

                #endregion

                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        private static bool CalculateCrit(HeroController hc, PlayerData pd)
        {
            if (!pd.equippedCharm_3) return false;
            
            int critChance = _rand.Next(1, 101);
            
            pd.CountJournalEntries();
            int critThreshold = 100 - pd.journalEntriesCompleted / 10;

            if (critChance <= Math.Min(critThreshold, 96)) return false;
            
            pd.beamDamage *= 3;
            hc.shadowRingPrefab.Spawn(hc.transform.position);
            hc.GetAttr<AudioSource>("audioSource")
              .PlayOneShot(hc.nailArtChargeComplete, 1f);

            return true;
        }

        #region Recoil
        internal enum Recoils
        {
            None,
            Normal,
            Long
        }

        private static void Recoil(BeamDirection dir, bool @long)
        {
            // The directions are flipped cause you recoil the opposite of the direction you attack
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (dir)
            {
                case Right:
                    if (@long)
                    {
                        HeroController.instance.RecoilLeftLong();
                        break;
                    }

                    HeroController.instance.RecoilLeft();
                    break;
                case Left:
                    if (@long)
                    {
                        HeroController.instance.RecoilRightLong();
                        break;
                    }

                    HeroController.instance.RecoilRight();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }
        #endregion

        #region SpawnBeam
        internal enum BeamDirection
        {
            Up,
            Down,
            Right,
            Left
        }

        private void SpawnBeams
        (
            bool         dir,
            float        scaleX,
            float        scaleY,
            float?       positionX     = null,
            IList<float> positionY     = null,
            bool         offset        = true,
            bool         rightNegative = true,
            bool?        recoil        = null,
            Recoils      recoils       = Recoils.Long
        )
        {
            SpawnBeams(dir ? Right : Left, scaleX, scaleY, positionX, positionY, offset, rightNegative, recoil, recoils);
        }

        private void SpawnBeams
        (
            BeamDirection dir,
            float         scaleX,
            float         scaleY,
            float?        positionX     = null,
            IList<float>  positionY     = null,
            bool          offset        = true,
            bool          rightNegative = true,
            bool?         recoil        = null,
            Recoils       recoils       = Recoils.Long
        )
        {
            SpawnBeam(dir, scaleX, scaleY, positionX, positionY?[0], offset, rightNegative, recoil, recoils);
            SpawnBeam(dir, scaleX, scaleY, positionX, positionY?[1], offset, rightNegative, recoil, recoils);
        }

        private void SpawnBeams
        (
            float   scaleX,
            float   scaleY,
            float?  positionX     = null,
            object  positionY     = null,
            bool    offset        = true,
            bool    rightNegative = true,
            bool?   recoil        = null,
            Recoils recoils       = Recoils.None
        )
        {
            switch (positionY)
            {
                case float posY:
                    SpawnBeam(Left, scaleX, scaleY, positionX, posY, offset, rightNegative, recoil, recoils);
                    SpawnBeam(Right, scaleX, scaleY, positionX, posY, offset, rightNegative, recoil, recoils);
                    break;
                case float[] posYs:
                    foreach (float y in posYs)
                    {
                        SpawnBeams(scaleX, scaleY, positionX, y, offset, rightNegative, recoil, recoils);
                        SpawnBeams(scaleX, scaleY, positionX, y, offset, rightNegative, recoil, recoils);
                    }

                    break;
                case null:
                    SpawnBeam(Left, scaleX, scaleY, positionX, null, offset, rightNegative, recoil, recoils);
                    SpawnBeam(Right, scaleX, scaleY, positionX, null, offset, rightNegative, recoil, recoils);
                    break;
            }
        }

        internal void SpawnBeam
        (
            bool    dir,
            float   scaleX,
            float   scaleY,
            float?  positionX     = null,
            float?  positionY     = null,
            bool    offset        = true,
            bool    rightNegative = true,
            bool?   recoil        = null,
            Recoils recoils       = Recoils.None
        )
        {
            SpawnBeam(dir ? Right : Left, scaleX, scaleY, positionX, positionY, offset, rightNegative, recoil, recoils);
        }

        private void SpawnBeam
        (
            BeamDirection dir,
            float         scaleX,
            float         scaleY,
            float?        positionX     = null,
            float?        positionY     = null,
            bool          offset        = true,
            bool          rightNegative = true,
            bool?         recoil        = null,
            Recoils       recoils       = Recoils.None
        )
        {
            string beamPrefab = "grubberFlyBeamPrefab";
            switch (dir)
            {
                case Up:
                    beamPrefab += "U";
                    break;
                case Down:
                    beamPrefab += "D";
                    break;
                case Right:
                    beamPrefab += "R";
                    break;
                case Left:
                    beamPrefab += "L";
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            beamPrefab += _crit ? "_fury" : "";

            HeroController hc = HeroController.instance;

            GrubberFlyBeam = hc.GetAttr<GameObject>(beamPrefab).Spawn(hc.transform.position);
            Transform t = hc.transform;

            if (positionX != null)
                GrubberFlyBeam.transform.SetPositionX((float) (positionX + (offset ? t.GetPositionX() : 0)));
            if (positionY != null)
                GrubberFlyBeam.transform.SetPositionY((float) (positionY + (offset ? t.GetPositionY() : 0)));

            GrubberFlyBeam.transform.SetScaleX((rightNegative && dir == Right ? -1 : 1) * scaleX);
            GrubberFlyBeam.transform.SetScaleY(scaleY);

            // Switch because we can pass *either* a bool or a enum
            switch (recoil)
            {
                case true:
                    recoils = Recoils.Long;
                    break;
                case false:
                    recoils = Recoils.Normal;
                    break;
            }

            if (recoils != Recoils.None) Recoil(dir, recoils == Recoils.Long);
        }

        #endregion
    }
}