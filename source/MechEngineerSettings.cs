﻿using System.Collections.Generic;
using BattleTech;
using CustomComponents;
using HBS.Logging;

namespace MechEngineer
{
    public class AddHelper
    {
        public string ComponentDefId { get; set; }
        public ChassisLocations ChassisLocation { get; set; }
        public ComponentType ComponentType { get; set; }
    }

    public class MechEngineerSettings
    {
        #region misc

        public bool HeatDamageInjuryEnabled = true;
        public bool ShutdownInjuryEnabled = true;

        public int MinimumHeatSinksOnMech = 10; // minimum heatsinks a mech requires
        public bool EnforceRulesForAdditionalInternalHeatSinks = true; // can't have those juicy ++ cooling systems with smaller fusion cores than the rules allow it
        public bool AllowMixingHeatSinkTypes = false; // only useful for patchwork like behavior
        public float FractionalAccountingPrecision = 0.5f; // change to 0.001 for kg fractional accounting precision

        public bool ArmorStructureRatioEnforcement = true;
        public string[] ArmorStructureRatioEnforcementSkipMechDefs = { }; // mech defs to skip

        public bool SaveMechDefOnMechLabConfirm = false;

        public string DefaultEngineHeatSinkId = "Gear_HeatSink_Generic_Standard"; // default heat sink type for engines without a kit

        public int EngineMissingFallbackHeatSinkCapacity = 30; // for stuff that wasn't auto fixed and still missing an engine, use a fallback

        // MWO does not allow to drop if that would mean to go overweight
        // battletech allows overweight, to stay consistent so we also allow overspace usage by default
        // set to true to switch to MWO style
        public bool DynamicSlotsValidateDropEnabled = false;

        public float? ArmorRoundingPrecision = null; // default is ARMOR_PER_STEP * TONNAGE_PER_ARMOR_POINT

        public float? MechLabScale = 0.85f;
        public float? MechLabPositionY = 40;

        #endregion

        #region bonus descriptions

        public string BonusDescriptionsBonusTemplate = " <indent=10%><line-indent=-5%><line-height=65%>{{bonus}}</line-height></line-indent></indent>\r\n";
        public string BonusDescriptionsDescriptionTemplate = "Traits:<b><color=#F79B26FF>\r\n{{bonuses}}</color></b>\r\n{{originalDescription}}";

        #endregion

        #region engine critical hit states

        public CriticalHitStates EngineCriticalHitStates = new CriticalHitStates
        {
            MaxStates = 2,
            DeathMethod = DeathMethod.EngineDestroyed,
            HitEffects = new[]
            {
                new CriticalHitEffect
                {
                    State = 1,
                    StatusEffect = new EffectData
                    {
                        durationData = new EffectDurationData(),
                        targetingData = new EffectTargetingData
                        {
                            effectTriggerType = EffectTriggerType.Passive,
                            effectTargetType = EffectTargetType.Creator,
                            showInTargetPreview = true,
                            showInStatusPanel = true
                        },
                        effectType = EffectType.StatisticEffect,
                        Description = new BaseDescriptionDef(
                            Id:"StatusEffect-EngineCrit1",
                            Name:"Engine Crit (1)",
                            Details:"Engine was hit, additional 15 Heat / Turn",
                            Icon:"uixSvgIcon_equipment_ThermalExchanger"
                            ),
                        nature = EffectNature.Debuff,
                        statisticData = new StatisticEffectData
                        {
                            statName = "HeatSinkCapacity",
                            operation = StatCollection.StatOperation.Int_Subtract,
                            modValue = "15",
                            modType = "System.Int32"
                        }
                    }
                },
                new CriticalHitEffect
                {
                    State = 2,
                    StatusEffect = new EffectData
                    {
                        durationData = new EffectDurationData(),
                        targetingData = new EffectTargetingData
                        {
                            effectTriggerType = EffectTriggerType.Passive,
                            effectTargetType = EffectTargetType.Creator,
                            showInTargetPreview = true,
                            showInStatusPanel = true
                        },
                        effectType = EffectType.StatisticEffect,
                        Description = new BaseDescriptionDef(
                            Id:"StatusEffect-EngineCrit2",
                            Name:"Engine Crit (2)",
                            Details:"Engine was hit, additional 30 Heat / Turn",
                            Icon:"uixSvgIcon_equipment_ThermalExchanger"
                        ),
                        nature = EffectNature.Debuff,
                        statisticData = new StatisticEffectData
                        {
                            statName = "HeatSinkCapacity",
                            operation = StatCollection.StatOperation.Int_Subtract,
                            modValue = "30",
                            modType = "System.Int32"
                        }
                    }
                }
            }
        };

        #endregion

        #region logging

        public class LoggerLogLevel
        {
            public string Name;
            public LogLevel Level;
        }
        public LoggerLogLevel[] LogLevels = {
            new LoggerLogLevel
            {
                Name = "MechEngineer",
                Level = LogLevel.Debug
            },
            new LoggerLogLevel
            {
                Name = Mech_InitStats_Patch.LOGGER_NAME,
                Level = LogLevel.Log
            }
        };

        #endregion

        #region auto fixes

        public string[] AutoFixMechDefSkip = { }; // mech defs to skip for AutoFixMechDef*
        public string[] AutoFixUpgradeDefSkip =
        {
            "Gear_Cockpit_SLDF_Custom",
            "Gear_Cockpit_Generic_Small",
            "Gear_Cockpit_Generic_Standard",
            "Gear_Cockpit_LifeSupportA_Standard",
            "Gear_Cockpit_LifeSupportB_Standard",
            "Gear_Cockpit_SensorsA_Standard",
            "Gear_Cockpit_SensorsB_Standard",
            "Gear_Gyro_Generic_Standard",
            "emod_arm_shoulder",
            "emod_arm_upper",
            "emod_arm_lower",
            "emod_arm_hand",
            "emod_leg_hip",
            "emod_leg_upper",
            "emod_leg_lower",
            "emod_leg_foot",
        }; // upgrades to not autofix

        public bool AutoFixMechDefEngine = true; // adds missing engine and removes too many jump jets
        public string AutoFixMechDefCoolingDef = "emod_kit_shs";
        public string AutoFixMechDefHeatBlockDef = "emod_engine_cooling";
        public string AutoFixMechDefCoreDummy = "emod_engine_dummy";

        public IdentityHelper AutoFixGyroCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.CenterTorso, // optional if category is properly setup
            ComponentType = ComponentType.Upgrade, // optional if category is properly setup
            Prefix = "Gear_Gyro_", // optional if category is properly setup
            CategoryId = "Gyro", // required
            AutoAddCategoryIdIfMissing = true // adds category id to items matched by optional filters
        };
        public ValueChange<int> AutoFixGyroSlotChange = new ValueChange<int> {From = 3, By = 1};

        public IdentityHelper AutoFixCockpitCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.Head,
            ComponentType = ComponentType.Upgrade,
            Prefix = "Gear_Cockpit_",
            CategoryId = "Cockpit",
            AutoAddCategoryIdIfMissing = true
        };
        public ValueChange<float> AutoFixCockpitTonnageChange = new ValueChange<float> {From = 0, By = 3};
        public ValueChange<int> AutoFixCockpitSlotChange = new ValueChange<int> {From = 1, By = 5};


        public bool UseCBTActuators = true;
        public string DefaultCBTShoulder = "emod_armcbt_shoulder";
        public string DefaultCBTLower = "emod_armcbt_lower";
        public string DefaultCBTUppder = "emod_armcbt_upper";
        public string DefaultCBTHand = "emod_armcbt_hand";


        public bool AutoFixMechDefArmActuator = true;
        public IdentityHelper AutoFixArmActuatorCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.Arms,
            ComponentType = ComponentType.Upgrade,
            Prefix = null, //"Gear_Actuator_",
            CategoryId = "ArmActuator",
            AutoAddCategoryIdIfMissing = true
        };
        public ValueChange<int> AutoFixArmActuatorSlotChange = new ValueChange<int> {From = 2, By = 2};

        public IdentityHelper AutoFixLegUpgradesCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.Legs,
            ComponentType = ComponentType.Upgrade,
            Prefix = null, //"Gear_Actuator_";
            CategoryId = "LegFootActuator",
            AutoAddCategoryIdIfMissing = true
        };
        public ValueChange<int> AutoFixLegUpgradesSlotChange = new ValueChange<int>  {From = 3, By = -2, FromIsMin = true, NewMin = 1};

        public string[] AutoFixChassisDefSkip = { };

        public ChassisSlotsChange[] AutoFixChassisDefSlotsChanges =
        {
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftTorso,
                Change = new ValueChange<int> {From = 10, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightTorso,
                Change = new ValueChange<int> {From = 10, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftLeg,
                Change = new ValueChange<int> {From = 4, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightLeg,
                Change = new ValueChange<int> {From = 4, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.Head,
                Change = new ValueChange<int> {From = 1, By = 5}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.CenterTorso,
                Change = new ValueChange<int> {From = 4, By = 11}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftArm,
                Change = new ValueChange<int> {From = 8, By = 4}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightArm,
                Change = new ValueChange<int> {From = 8, By = 4}
            }
        };
        public class ChassisSlotsChange
        {
            public ChassisLocations Location;
            public ValueChange<int> Change;
        }

        public bool AutoFixChassisDefInitialTonnage = true;
        public float AutoFixChassisDefInitialToTotalTonnageFactor = 0.1f; // 10% structure weight
        public bool AutoFixChassisDefMaxJumpjets = true;
        public int AutoFixChassisDefMaxJumpjetsCount = 8;
        public int AutoFixChassisDefMaxJumpjetsRating = 400;

        public WeaponSlotChange[] AutoFixWeaponDefSlotsChanges =
        {
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC5,
                Change = new ValueChange<int> {From = 2, By = 2}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC10,
                Change = new ValueChange<int> {From = 3, By = 4}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC20,
                Change = new ValueChange<int> {From = 4, By = 6}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.Gauss,
                Change = new ValueChange<int> {From = 5, By = 2}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.LargeLaserPulse,
                Change = new ValueChange<int> {From = 2, By = 1}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.LRM20,
                Change = new ValueChange<int> {From = 4, By = 1}
            }
        };
        public class WeaponSlotChange
        {
            public WeaponSubType Type;
            public ValueChange<int> Change;
        }

        #endregion

        #region movement

        public CBTMovementSettings CBTMovement = new CBTMovementSettings();
        public class CBTMovementSettings
        {
            /* 
            set to false to use TT walk values
            using the default game values, slow mechs move a bit faster, and fast mechs move a bit slower
            Examples if set to true:
                Walk 2  70 / 125
                Walk 3  95 / 165
                Walk 4 120 / 200
                Walk 5 140 / 240
                Walk 6 165 / 275
                Walk 7 190 / 315
                Walk 8 210 / 350
            */
            public bool UseGameWalkValues = true;

            //// set to false to only allow engines that produce integer walk values
            //public bool AllowNonIntWalkValues = true;

            // this setting controls if the allowed number of jump jets is rounded up or down
            // example: if false, TT walk speed of 2.1 allows 2 jump jets, if true, it allows 3 jump jets
            public bool JJRoundUp = false;

            /*
            not sure why you would want to change these, but they are set here
            they are the multiples that translate TT movement values to game movement values
            Example:
                A griffin that walks 5 would walk 5 * 30 = 150 and sprint 5 * 50 = 250
            NOTE: if you have the UseGameWalkValues set, the exact values are then changed based on a linear equasion
            */
            public float TTWalkMultiplier = 30f;
            public float TTSprintMultiplier = 50f;
        }

        #endregion

        #region hardpoint fixes

        public HardpointFixSettings HardpointFix = new HardpointFixSettings();
        public class HardpointFixSettings
        {
            // TODO add set to 4 slots per chassis location autofix variant
            // TODO make enum so we have: set to 4, set to encountered prefabs, disabled
            public bool AutoFixChassisDefWeaponHardpointCounts = false; // true = hardpoint counts derived from prefab hardpoints
            public bool EnforceHardpointLimits = false; // true = use prefab hardpoints
            public bool AllowDefaultLoadoutWeapons = false;
            public bool AllowLRMInSmallerSlotsForAll = false;
            public string[] AllowLRMInSmallerSlotsForMechs = { "atlas" };
            public bool AllowLRMInLargerSlotsForAll = true;
        }

        #endregion
    }
}
