﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Cauldron.Quicksilver
{
    public class ForestOfNeedlesCardController : CardController
    {
        public ForestOfNeedlesCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }
        //{Quicksilver} may deal 6 melee damage to a target with more than 8HP, or 3 melee damage to a target with 8 or fewer HP.
        public override IEnumerator Play()
        {
            var a = new Func<Card, int>((Card c) => this.DynamicDamage(c));
            Func<Card, int> damage = (Card c) => this.DynamicDamage(c);
            Func<int> targets = new Func<int>(() => 1);
            IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), (Card c) => this.DynamicDamage(c), DamageType.Melee, () => 1, false, 1, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        private int DynamicDamage(Card c)
        {
            int amount = 0;
            if (c.HitPoints > 8)
            {
                amount = 6;
            }
            else
            {
                amount = 3;
            }
            return amount;
        }
    }
}