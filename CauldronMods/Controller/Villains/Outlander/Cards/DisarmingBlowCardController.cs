﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Outlander
{
    public class DisarmingBlowCardController : OutlanderUtilityCardController
    {
        public DisarmingBlowCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNonVillainTargetWithHighestHP(numberOfTargets: 2);
        }

        public override IEnumerator Play()
        {
            //{Outlander} deals the 2 non-villain targets with the highest HP 3 melee damage each.
            //Any hero damaged this way discards 1 card.
            IEnumerator coroutine = DealDamageToHighestHP(CharacterCard, 1, (Card c) => !IsVillain(c) && c.IsTarget, (Card c) => 3, DamageType.Melee, numberOfTargets: () => 2, addStatusEffect: (DealDamageAction action) => GameController.SelectAndDiscardCard(FindHeroTurnTakerController(action.Target.Owner.ToHero()), additionalCriteria: (Card c) => action.Target.IsCharacter && action.Amount > 0, cardSource: GetCardSource()));
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}
