using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Hanshek.CustomFunctions;
using static Hanshek.Plugin;
using static Hanshek.DescriptionFunctions;
using static Hanshek.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Hanshek
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // At the start of combat, gain 2 Insulate and 2 Courage.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                _character.SetAuraTrait(_character, "insulate", 2);
                _character.SetAuraTrait(_character, "courage", 2);
            }


            else if (_trait == trait2a)
            {
                // trait2a
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // When you play a Spell, add the Curse Spell type to it.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (_castedCard != null && _castedCard.HasCardType(Enums.CardType.Spell))
                {
                    if (!_castedCard.HasCardType(Enums.CardType.Curse_Spell))
                    {
                        List<Enums.CardType> currentTypes = [.. _castedCard.CardTypeAux];
                        currentTypes.Add(Enums.CardType.Curse_Spell);
                        _castedCard.CardTypeAux = [.. currentTypes];
                        LogDebug($"Added Curse type to {_castedCard.CardName}");
                        Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
                    }
                }

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // At the start of your turn, reduce the cost of all Curse Spells by 1 until discarded.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                ReduceCardTypeCostUntilDiscarded(Enums.CardType.Curse_Spell, 1, ref _character, ref heroHand, ref cardDataList, traitName);
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // Once per combat, when you play the \"Hellfire\" card put a 0 cost copy with Vanish into your deck.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (_castedCard != null && _castedCard.Id.StartsWith("royalmagehellfire") && _character.HeroData != null && _character.HeroData.HeroSubClass != null && !MatchManager.Instance.ItemExecuteForThisCombat(_character.HeroData.HeroSubClass.Id, traitId, 1, ""))
                {

                    {
                        string cardInDictionary = MatchManager.Instance.CreateCardInDictionary(_castedCard.Id);
                        CardData cardData = MatchManager.Instance.GetCardData(cardInDictionary);
                        cardData.EnergyReductionToZeroPermanent = true;
                        MatchManager.Instance.ModifyCardInDictionary(cardInDictionary, cardData);
                        MatchManager.Instance.GenerateNewCard(1, cardInDictionary, false, Enums.CardPlace.RandomDeck, heroIndex: MatchManager.Instance.GetHeroHeroActive().HeroIndex);
                        MatchManager.Instance.SetTraitInfoText();
                        _character.HeroItem.ScrollCombatText(traitName + Functions.TextChargesLeft(MatchManager.Instance.ItemExecutedInThisCombat(MatchManager.Instance.GetHeroHeroActive().SubclassName, traitId), 1), Enums.CombatScrollEffectType.Trait);
                    }
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait2a:
                // Burn on enemies reduces Dark resistance by 0.5% per charge.",

                // trait2b:
                // Stealth on heroes increases All Damage by an additional 15% per charge and All Resistances by an additional 5% per charge.",

                // trait 4a;
                // Evasion on you can't be purged unless specified. 
                // Stealth grants 25% additional damage per charge.",

                // trait 4b:
                // Heroes Only lose 75% stealth charges rounding down when acting in stealth.

                case "burn":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result = AtOManager.Instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Shadow, 0, -0.5f);
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventData), "Init")]
        public static void InitPostfix(ref EventData __instance, ref EventReplyData[] ___replys)
        {
            List<EventReplyData> eventReplyDataList = new List<EventReplyData>();
            // List<string> stringList = new List<string>();
            // for (int index = 0; index < ___replys.Length; ++index)
            // {
            //     EventReplyData reply = ___replys[index];
            //     if (reply != null && reply.RequiredClass != null)
            //         stringList.Add(reply.RequiredClass.Id);
            // }
            for (int index = 0; index < ___replys.Length; ++index)
            {
                EventReplyData reply = ___replys[index];
                if (reply != null && reply.RequiredClass.Id == "warlock")
                {
                    EventReplyData eventReplyData = reply.ShallowCopy();
                    eventReplyData.RequiredClass = Globals.Instance.SubClass[subclassName.ToLower()];
                    eventReplyData.IndexForAnswerTranslation = 175165 + index;
                    eventReplyData.ReplyText.Replace("Zek", "Hanshek");
                    eventReplyData.FlRewardText.Replace("Zek", "Hanshek");
                    eventReplyData.SsRewardText.Replace("Zek", "Hanshek");
                    eventReplyData.FlcRewardText.Replace("Zek", "Hanshek");
                    eventReplyData.SscRewardText.Replace("Zek", "Hanshek");
                    // eventReplyData.ReplyActionText.Replace("Zek","Hanshek");
                    eventReplyDataList.Add(eventReplyData);
                }

            }
            ___replys = eventReplyDataList.ToArray();
        }

    }
}

