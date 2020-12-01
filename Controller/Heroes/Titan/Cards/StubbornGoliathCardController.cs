﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Titan
{
    public class StubbornGoliathCardController : CardController
    {
        public StubbornGoliathCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<SelectCardDecision> storedSelect = new List<SelectCardDecision>();
            //{Titan} deals up to 2 non-hero targets 2 infernal damage each.
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Infernal, 2, false, 0, storedResultsDecisions: storedSelect, cardSource: base.GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //Until the start of your next turn, when those targets would deal damage, you may redirect that damage to {Titan}.
            if (storedSelect.FirstOrDefault() != null && storedSelect.FirstOrDefault().SelectedCard != null)
            {
                Card firstCard = storedSelect.FirstOrDefault().SelectedCard;
                Card secondCard = storedSelect.LastOrDefault().SelectedCard;
                List<Card> cards = new List<Card>() { firstCard };
                if (firstCard != secondCard)
                {
                    cards.Add(secondCard);
                }
                RedirectDamageStatusEffect redirectDamageStatusEffect = new RedirectDamageStatusEffect();
                //Until the start of your next turn...
                redirectDamageStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
                //...when those targets would deal damage...
                redirectDamageStatusEffect.SourceCriteria.IsOneOfTheseCards = cards;
                //...you may redirect...
                redirectDamageStatusEffect.IsOptional = true;
                //...that damage to {Titan}.
                redirectDamageStatusEffect.RedirectTarget = base.CharacterCard;

                coroutine = base.AddStatusEffect(redirectDamageStatusEffect);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }
    }
}