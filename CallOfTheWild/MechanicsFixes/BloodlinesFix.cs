﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class BloodlinesFix
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass magus = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
        static BlueprintCharacterClass sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
        static BlueprintCharacterClass dragon_disciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
        static BlueprintArchetype eldritch_scion = library.Get<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");

        static public BlueprintFeature blood_havoc;
        static public BlueprintFeature blood_intensity;

        static internal void load()
        {
            fixSorcBloodlineProgression();
            fixBloodlinesUI();
            createBloodHavoc();
            createBloodIntensity();

            addBloodlineMutations();

            fixArcaneBloodline();
        }


        static void fixArcaneBloodline()
        {
            var resource = Helpers.CreateAbilityResource("ArcaneBloodlineMetamagicAdeptResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 3, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { sorcerer, magus}, new BlueprintArchetype[] { eldritch_scion });

            var metamagic_adept = library.Get<BlueprintFeature>("82a966061553ea442b0ce0cdb4e1d49c");
            var icon = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").Icon;
            metamagic_adept.SetNameDescriptionIcon("Metamagic Adept",
                                                   "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell. You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level. At 20th level, this ability is replaced by arcane apotheosis.",
                                                   icon);

            var buff = Helpers.CreateBuff("MetamagicAdeptBuff",
                                          metamagic_adept.Name,
                                          metamagic_adept.Description,
                                          "",
                                          metamagic_adept.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicAdept>(m => m.resource = resource),
                                          Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 1)
                                          );
            var ability = Helpers.CreateActivatableAbility("MetamagicAdeptToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );

            metamagic_adept.ComponentsArray = new BlueprintComponent[] {Helpers.CreateAddAbilityResource(resource),
                                                                        Helpers.CreateAddFact(ability)};

            var arcane_apotheosis = library.Get<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4");
            arcane_apotheosis.SetDescription("At 20th level, your body surges with arcane power. You can add any metamagic feats that you know to your spells without increasing their casting time, although you must still expend higher-level spell slots.");
            arcane_apotheosis.SetComponents(Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 100),
                                            Common.createRemoveFeatureOnApply(metamagic_adept));
        }



        static void createBloodIntensity()
        {
            var resource = Helpers.CreateAbilityResource("BloodIntensityResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 3, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { sorcerer, magus, Bloodrager.bloodrager_class }, new BlueprintArchetype[] { eldritch_scion });

            var buff = Helpers.CreateBuff("BloodIntensityBuff",
                                          "Blood Intensity",
                                          "Whenever you cast a bloodrager or sorcerer spell that deals damage, you can increase its maximum number of damage dice by an amount equal to your Strength or Charisma modifier, whichever is higher. This otherwise functions as —and does not stack with—the Intensified Spell feat. You can use this ability once per day at 3rd level and one additional time per day for every 4 caster levels you have beyond 3rd, up to five times per day at 19th level.",
                                          "",
                                          MetamagicFeats.intensified_metamagic.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                                                                                                      {
                                                                                                                          m.resource = resource;
                                                                                                                          m.spell_descriptor = SpellDescriptor.None;
                                                                                                                          m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.BloodIntensity;
                                                                                                                      })
                                          );

            var ability = Helpers.CreateActivatableAbility("BloodIntensityToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;
                                          
            blood_intensity = Helpers.CreateFeature("BloodIntensityFeature",
                                                "Blood Intensity",
                                                "Whenever you cast a bloodrager or sorcerer spell that deals damage, you can increase its maximum number of damage dice by an amount equal to your Strength or Charisma modifier, whichever is higher. This otherwise functions as —and does not stack with—the Intensified Spell feat. You can use this ability once per day at 3rd level and one additional time per day for every 4 caster levels you have beyond 3rd, up to five times per day at 19th level.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.CreateAddAbilityResource(resource),
                                                Helpers.CreateAddFact(ability),
                                                Helpers.PrerequisiteClassLevel(sorcerer, 3, any: true),
                                                Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 8, any: true),
                                                Common.createPrerequisiteArchetypeLevel(magus, eldritch_scion, 3, any: true)
                                                );
        }
        static void createBloodHavoc()
        {
            blood_havoc = Helpers.CreateFeature("BloodHavocFeature",
                                                "Blood Havoc",
                                                "Whenever you cast a bloodrager or sorcerer spell that deals damage, add 1 point of damage per die rolled. This benefit applies only to damaging spells that belong to schools you have selected with Spell Focus or that are bloodline spells for your bloodline.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.BloodHavoc>(b => b.feature = library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe")),
                                                Helpers.PrerequisiteClassLevel(sorcerer, 1, any: true),
                                                Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 4, any: true),
                                                Common.createPrerequisiteArchetypeLevel(magus, eldritch_scion, 1, any: true)
                                                );

        }


        static void addBloodlineMutations()
        {
            var feats_selections = new BlueprintFeatureSelection[] {library.Get<BlueprintFeatureSelection>("3a60f0c0442acfb419b0c03b584e1394"), //sorcerer
                                                                    Bloodrager.bloodline_feat_selection,
                                                                   };

            foreach (var fs in feats_selections)
            {
                fs.AllFeatures = fs.AllFeatures.AddToArray(blood_havoc, blood_intensity);
            }

            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures.Cast<BlueprintProgression>().ToList();

            foreach (var b in bloodlines)
            {
                var ability1 = b.LevelEntries[0].Features.Where(f => !f.name.Contains("Arcana") && !f.name.Contains("ClassSkill")).FirstOrDefault() as BlueprintFeature;
                var ability3 = b.LevelEntries[1].Features.Where(f => !f.name.Contains("BloodlineBonusSpell")).FirstOrDefault() as BlueprintFeature;

                var selection = Helpers.CreateFeatureSelection(ability1.name + "Selection",
                                                               ability1.Name,
                                                               ability1.Description,
                                                               "",
                                                               ability1.Icon,
                                                               FeatureGroup.None);
                selection.AllFeatures = new BlueprintFeature[] { ability1, blood_havoc };

                var selection3 = Helpers.CreateFeatureSelection(ability3.name + "Selection",
                                               ability3.Name,
                                               ability3.Description,
                                               "",
                                               ability3.Icon,
                                               FeatureGroup.None);
                selection3.AllFeatures = new BlueprintFeature[] { ability3, blood_intensity };
                b.UIGroups[0].Features.Remove(ability1);
                b.UIGroups[0].Features.Add(selection);
                b.UIGroups[0].Features.Remove(ability3);
                b.UIGroups[0].Features.Add(selection3);
                b.LevelEntries[0].Features.Remove(ability1);
                b.LevelEntries[0].Features.Add(selection);
                b.LevelEntries[1].Features.Remove(ability3);
                b.LevelEntries[1].Features.Add(selection3);
            }

            foreach (var b in Bloodrager.bloodlines.Values)
            {
                var ability1 = b.progression.LevelEntries[1].Features[0] as BlueprintFeature;
                var ability2 = b.progression.LevelEntries[3].Features[0] as BlueprintFeature;

                var selection = Helpers.CreateFeatureSelection(ability1.name + "Selection",
                                                               ability1.Name,
                                                               ability1.Description,
                                                               "",
                                                               ability1.Icon,
                                                               FeatureGroup.None);
                selection.AllFeatures = new BlueprintFeature[] { ability1, blood_havoc };
                b.progression.UIGroups[0].Features.Remove(ability1);
                b.progression.UIGroups[0].Features.Add(selection);
                b.progression.LevelEntries[1].Features.Remove(ability1);
                b.progression.LevelEntries[1].Features.Add(selection);

                var selection2 = Helpers.CreateFeatureSelection(ability2.name + "Selection",
                                               ability2.Name,
                                               ability2.Description,
                                               "",
                                               ability2.Icon,
                                               FeatureGroup.None);
                selection.AllFeatures = new BlueprintFeature[] { ability2, blood_intensity };
                b.progression.UIGroups[0].Features.Remove(ability2);
                b.progression.UIGroups[0].Features.Add(selection2);
                b.progression.LevelEntries[3].Features.Remove(ability2);
                b.progression.LevelEntries[3].Features.Add(selection2);
            }
        }

        static void fixBloodlinesUI()
        {
            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures.Cast<BlueprintProgression>().ToList();
            bloodlines.Add(library.Get<BlueprintProgression>("a46d4bd93601427409d034a997673ece")); //sylvan
            bloodlines.Add(library.Get<BlueprintProgression>("7d990675841a7354c957689a6707c6c2")); //sage
            bloodlines.Add(library.Get<BlueprintProgression>("8a95d80a3162d274896d50c2f18bb6b1")); //empyreal

            foreach (var b in bloodlines)
            {
                List<BlueprintFeature> spells = new List<BlueprintFeature>();
                List<BlueprintFeature> abilities = new List<BlueprintFeature>();

                foreach (var le in b.LevelEntries)
                {
                    foreach (var f in le.Features)
                    {
                        if (f.name.Contains("SpellLevel"))
                        {
                            spells.Add(f as BlueprintFeature);
                        }
                        else if (!f.name.Contains("ClassSkill") && !(le.Level == 1 && f.name.Contains("Arcana")))
                        {
                            abilities.Add(f as BlueprintFeature);
                        }
                    }
                }
                b.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(abilities.ToArray()), Helpers.CreateUIGroup(spells.ToArray()) };
            }
        }


        static void fixSorcBloodlineProgression()
        {
            //put draconic claws and breath weapon in one first level feat
            var draconic_progression = library.GetAllBlueprints().Where(b => (b is BlueprintProgression) && b.name.Contains("BloodlineDraconic") && !b.name.Contains("Bloodrager")).Cast<BlueprintProgression>().ToArray();

            foreach (var d in draconic_progression)
            {
                convolveBloodlineFeatures(d, "ClawsFeatureAddLevel1", "ClawsFeatureAddLevel", sorcerer, magus, dragon_disciple);
                convolveBloodlineFeatures(d, "BreathWeaponBaseFeature", "BreathWeaponExtraUse", sorcerer, magus, dragon_disciple);
                convolveBloodlineFeatures(d, "ResistancesAbilityAddLevel1", "ResistancesAbilityAddLevel", sorcerer, magus, dragon_disciple);
                d.LevelEntries[4].Features[0].HideInUI = false;
            }

            var serpentine_bloodline = library.Get<BlueprintProgression>("739c1e842bf77994baf963f4ad964379");
            convolveBloodlineFeatures(serpentine_bloodline, "SerpentsFangBiteFeatureAddLevel1", "SerpentsFangBiteFeatureAddLevel", sorcerer, magus);
            convolveBloodlineFeatures(serpentine_bloodline, "SnakeskinFeatureAddLevel1", "SnakeskinFeatureAddLevel", sorcerer, magus);

            var infernal_bloodline = library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d");
            convolveBloodlineFeatures(infernal_bloodline, "HellfireFeature", "HellfireExtraUse", sorcerer, magus);

            var undead_bloodline = library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e");
            convolveBloodlineFeatures(undead_bloodline, "BloodlineUndeadGraspOfTheDeadFeature", "GraspOfTheDeadExtraUse", sorcerer, magus);
            var deaths_gift = library.Get<BlueprintFeature>("fd5f14d44e82f464196fdf0ea82347cc");
            replaceRepeatingFeatureWithProgression(undead_bloodline, deaths_gift);

            //fix elemental bloodlines
            var elemental_progression = library.GetAllBlueprints().Where(b => (b is BlueprintProgression) && b.name.Contains("BloodlineElemental") && !b.name.Contains("Bloodrager")).Cast<BlueprintProgression>().ToArray();

            foreach (var e in elemental_progression)
            {
                convolveBloodlineFeatures(e, "ElementalBlastFeature", "ElementalBlastExtraUse", sorcerer, magus);
            }

            var water_elemental_movement = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");
            removeExtraEntries(library.Get<BlueprintProgression>("7c692e90592257a4e901d12ae6ec1e41"), water_elemental_movement); //second water elemental movement entry
            replaceFeature(library.Get<BlueprintProgression>("32393034410fb2f4d9c8beaa5c8c8ab7"), library.Get<BlueprintFeature>("277d9c2a2392e8940a452888aa67f32e"), water_elemental_movement); //earth elemental movement

            //celestial or empyreal remove wings at level 15

            var celestial_bloodline = library.Get<BlueprintProgression>("aa79c65fa0e11464d9d100b038c50796");
            var empyreal_bloodline = library.Get<BlueprintProgression>("8a95d80a3162d274896d50c2f18bb6b1");
            var aura = library.Get<BlueprintFeature>("2768c719ee7338c49932358c2c581bba");
            var celestial_wings = library.Get<BlueprintFeature>("894d21ff523481d49a51ab1750fca3a0");

            replaceFeature(celestial_bloodline, celestial_wings, null);
            replaceFeature(empyreal_bloodline, celestial_wings, null);
            replaceFeature(celestial_bloodline, aura, celestial_wings);

            var abyssal_bloodline = library.Get<BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113");
            var demon_wings = library.Get<BlueprintFeature>("36db25d9e0848f04da604ff9e3d931af");
            replaceFeature(abyssal_bloodline, demon_wings, null);
            convolveBloodlineFeatures(abyssal_bloodline, "StrengthAbilityAddLevel1", "StrengthAbilityAddLevel", sorcerer, magus);

            var arcane_bloodline = library.Get<BlueprintProgression>("4d491cf9631f7e9429444f4aed629791");
            var sage_bloodline = library.Get<BlueprintProgression>("7d990675841a7354c957689a6707c6c2");
            convolveBloodlineFeatures(arcane_bloodline, "CombatCastingAdeptFeatureAddLevel1", "CombatCastingAdeptFeatureAddLevel", sorcerer, magus);
            

            var new_arcana = library.Get<BlueprintFeature>("20a2435574bdd7f4e947f405df2b25ce");
            var arcana_progression =  replaceRepeatingFeatureWithProgression(arcane_bloodline, new_arcana);
            removeExtraEntries(sage_bloodline, new_arcana);
            replaceFeature(sage_bloodline, new_arcana, arcana_progression);
            replaceFeature(sage_bloodline, library.Get<BlueprintFeature>("3d7b19c8a1d03464aafeb306342be000"), null);
        }






        static void convolveBloodlineFeatures(BlueprintProgression bloodline, string primary_feature_id, string secondary_feature_id, params BlueprintCharacterClass[] classes)
        {
            BlueprintFeature primary_feature = null;
            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f.name.Contains(primary_feature_id))
                    {
                        primary_feature = f as BlueprintFeature;
                    }
                    else if (f.name.Contains(secondary_feature_id))
                    {
                        primary_feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(f as BlueprintFeature, bloodline.LevelEntries[i].Level,
                                                                                        classes,
                                                                                        new BlueprintArchetype[] { eldritch_scion }));
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
        }


        static void removeExtraEntries(BlueprintProgression bloodline, BlueprintFeature feature)
        {
            bool found = false;
            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f == feature && !found)
                    {
                        found = true;
                    }
                    else if (f == feature)
                    {
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
        }


        static void replaceFeature(BlueprintProgression bloodline, BlueprintFeature feature, BlueprintFeature replacement)
        {

            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                if (replacement == null)
                {
                    bloodline.LevelEntries[i].Features.RemoveAll(f => f == feature);
                }
                else
                {
                    for (int j = 0; j < bloodline.LevelEntries[i].Features.Count; j++)
                    {
                        if (bloodline.LevelEntries[i].Features[j] == feature )
                        {
                            bloodline.LevelEntries[i].Features[j] = replacement;
                        }
                    }
                }
            }
        }


        static BlueprintProgression replaceRepeatingFeatureWithProgression(BlueprintProgression bloodline, BlueprintFeature feature)
        {
            List<LevelEntry> level_entries = new List<LevelEntry>();

            var feature_progression = Helpers.CreateProgression(feature.name + "Progression",
                                                                feature.Name,
                                                                feature.Description,
                                                                "",
                                                                feature.Icon,
                                                                FeatureGroup.None);
            feature_progression.Classes = bloodline.Classes;
            feature_progression.Archetypes = bloodline.Archetypes;
            feature_progression.HideInCharacterSheetAndLevelUp = feature.HideInCharacterSheetAndLevelUp;

            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f == feature)
                    {
                        if (level_entries.Count == 0)
                        {
                            bloodline.LevelEntries[i].Features.Add(feature_progression);
                        }
                        level_entries.Add(Helpers.LevelEntry(bloodline.LevelEntries[i].Level, feature));
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
            feature_progression.LevelEntries = level_entries.ToArray();
            return feature_progression;
        }
    }



}
