﻿using System.Collections;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Vector
{
    public class AnticoagulantCardController : CardController
    {
        //==============================================================
        // Increase damage dealt to {Vector} by 1.
        //
        // When {Vector} is dealt damage, destroy this card and {Vector}
        // deals each hero target X toxic damage, where X is the amount
        // of damage that was dealt to {Vector}.
        //==============================================================

        public static readonly string Identifier = "Anticoagulant";

        private const int IncreaseDamageAmount = 1;

        public AnticoagulantCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {

            AddTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DidDealDamage, DealDamageResponse,
                new []{TriggerType.DealDamage, TriggerType.DestroySelf}, timing: TriggerTiming.After);

            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == base.CharacterCard, 1);

            base.AddTriggers();
        }


        private IEnumerator DealDamageResponse(DealDamageAction dda)
        {
            // When {Vector} is dealt damage, destroy this card and {Vector}
            // deals each hero target X toxic damage, where X is the amount
            // of damage that was dealt to {Vector}.

            IEnumerator damageRoutine = base.GameController.DealDamage(base.DecisionMaker, base.CharacterCard, 
                card => card.IsHero && card.IsInPlay,
                dda.Amount, DamageType.Toxic, cardSource: GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageRoutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageRoutine);
            }

            IEnumerator destroyRoutine = base.GameController.DestroyCard(this.DecisionMaker, this.Card);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyRoutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyRoutine);
            }
        }
    }
}