﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.OblaskCrater
{
    public class IndustriousHulkCardController : OblaskCraterUtilityCardController
    {
        /*
         * Whenever a hero draws a card, this card deals them 1 melee damage.
         * When this card is destroyed, each player may put the top card of 
         * their deck into their hand.
        */
        public IndustriousHulkCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            base.AddTrigger((DrawCardAction drawCard) => drawCard.IsSuccessful && drawCard.DidDrawCard && drawCard.DrawnCard.IsHero, DealDamageResponse, TriggerType.DealDamage, TriggerTiming.After);
            base.AddWhenDestroyedTrigger((dca) => base.EachPlayerDrawsACard(optional: true), TriggerType.DrawCard);
        }

		private IEnumerator DealDamageResponse(DrawCardAction drawCard)
		{
			List<Card> storedCharacter = new List<Card>();
			IEnumerator coroutine = FindCharacterCardToTakeDamage(drawCard.HeroTurnTaker, storedCharacter, base.CharacterCard, 1, DamageType.Melee);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(coroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(coroutine);
			}
			Card card = storedCharacter.FirstOrDefault();
			if (card != null)
			{
				IEnumerator coroutine2 = DealDamage(Card, card, 1, DamageType.Melee, cardSource: GetCardSource());
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(coroutine2);
				}
				else
				{
					base.GameController.ExhaustCoroutine(coroutine2);
				}
			}
		}
	}
}
