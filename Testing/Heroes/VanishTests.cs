﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Cauldron.Vanish;

namespace CauldronTests
{
    [TestFixture()]
    public class VanishTests : BaseTest
    {
        #region HelperFunctions
        protected HeroTurnTakerController vanish { get { return FindHero("Vanish"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(vanish.CharacterCard, 1);
            DealDamage(villain, vanish, 2, DamageType.Melee);
        }

        #endregion HelperFunctions

        [Test()]
        public void VanishLoad()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Haka", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(vanish);
            Assert.IsInstanceOf(typeof(VanishCharacterCardController), vanish.CharacterCardController);

            foreach (var card in vanish.HeroTurnTaker.GetAllCards())
            {
                var cc = GetCardController(card);
                Assert.IsTrue(cc.GetType() != typeof(CardController), $"{card.Identifier} is does not have a CardController");
            }

            Assert.AreEqual(26, vanish.CharacterCard.HitPoints);
        }

        [Test]
        public void VanishInnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            var minion = PlayCard("BladeBattalion");

            var mode = PlayCard("TurretMode");
            AssertInPlayArea(bunker, mode);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, haka.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, minion);
            DecisionSelectTargets = new Card[] { haka.CharacterCard, minion };
            UsePower(vanish);

            QuickHPCheck(0, 0, 0, 0, 0, -1);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, haka.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, minion);
            DecisionSelectTargets = new Card[] { bunker.CharacterCard, minion };
            UsePower(vanish);
            //check that bunker dealt the damage, if he did it will be increased by 1
            QuickHPCheck(0, 0, 0, 0, 0, -2);
        }

        [Test]
        public void VanishIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);

            QuickHandStorage(haka, bunker, scholar);
            DecisionSelectTurnTaker = haka.TurnTaker;
            UseIncapacitatedAbility(vanish, 0);

            QuickHandCheck(1, 0, 0);
        }

        [Test]
        public void VanishIncap2_Replace()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);

            var topCard = baron.TurnTaker.Deck.TopCard;
            var bottomCard = baron.TurnTaker.Deck.BottomCard;
            DecisionSelectLocation = new LocationChoice(baron.TurnTaker.Deck);
            DecisionMoveCardDestination = new MoveCardDestination(baron.TurnTaker.Deck, true);

            UseIncapacitatedAbility(vanish, 1);

            AssertNumberOfCardsInRevealed(baron, 0);

            AssertOnTopOfDeck(topCard);
            AssertOnBottomOfDeck(bottomCard);
        }


        [Test]
        public void VanishIncap2_MoveToTop()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);

            var topCard = haka.TurnTaker.Deck.TopCard;
            var bottomCard = haka.TurnTaker.Deck.BottomCard;
            DecisionSelectLocation = new LocationChoice(haka.TurnTaker.Deck);
            DecisionMoveCardDestination = new MoveCardDestination(haka.TurnTaker.Deck, false);

            UseIncapacitatedAbility(vanish, 1);

            AssertNumberOfCardsInRevealed(haka, 0);
            AssertOnTopOfDeck(bottomCard);
            AssertOnTopOfDeck(topCard, 1);

        }

        [Test]
        public void VanishIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");

            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);

            AssertNumberOfStatusEffectsInPlay(0);

            DecisionSelectCard = wraith.CharacterCard;
            UseIncapacitatedAbility(vanish, 2);

            string messageText = $"Reduce damage dealt to {wraith.Name} by 1.";

            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, wraith.TurnTaker);
            AssertStatusEffectsContains(messageText);

            //Test that the reducing effect works as expected
            QuickHPStorage(wraith);
            DealDamage(baron, wraith, 3, DamageType.Melee);
            //should have been reduced by 1
            QuickHPCheck(-2);

            PrintSeparator("Change turns");
            GoToEndOfTurn(wraith);

            PrintSeparator("Effect still applied");
            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, wraith.TurnTaker);
            AssertStatusEffectsContains(messageText);
            //Test that the reducing effect works as expected
            QuickHPStorage(wraith);
            DealDamage(baron, wraith, 3, DamageType.Melee);
            //should have been reduced by 1
            QuickHPCheck(-2);

            PrintSeparator("Effect expires");
            AssertNextMessageContains(messageText);
            GoToStartOfTurn(vanish);
            AssertNumberOfStatusEffectsInPlay(0);
            //Test that the reducing effect has disappeared
            QuickHPStorage(wraith);
            DealDamage(baron, wraith, 3, DamageType.Melee);
            //should have not have been reduced
            QuickHPCheck(-3);
        }


        [Test]
        public void ConcussiveBurst_EffectApplied()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");

            GoToPlayCardPhase(vanish);
            var card = PlayCard("ConcussiveBurst");
            AssertInPlayArea(vanish, card);

            AssertNumberOfStatusEffectsInPlay(0);

            DealDamage(vanish, baron.CharacterCard, 1, DamageType.Melee);

            string messageText = $"Reduce damage dealt by {baron.Name} by 1.";

            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, baron.TurnTaker);
            AssertStatusEffectsContains(messageText);

            //Test that the reducing effect works as expected
            QuickHPStorage(wraith);
            DealDamage(baron, wraith, 3, DamageType.Melee);
            //should have been reduced by 1
            QuickHPCheck(-2);

            PrintSeparator("Change turns");
            GoToEndOfTurn(wraith);

            PrintSeparator("Effect still applied");
            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, baron.TurnTaker);
            AssertStatusEffectsContains(messageText);
            //Test that the reducing effect works as expected
            QuickHPStorage(ra);
            DealDamage(baron, ra, 3, DamageType.Melee);
            //should have been reduced by 1
            QuickHPCheck(-2);

            PrintSeparator("Effect expires");
            AssertNextMessageContains(messageText);
            GoToStartOfTurn(vanish);
            AssertNumberOfStatusEffectsInPlay(0);
            //Test that the reducing effect has disappeared
            QuickHPStorage(vanish);
            DealDamage(baron, vanish, 3, DamageType.Melee);
            //should have not have been reduced
            QuickHPCheck(-3);
        }

        [Test]
        public void ConcussiveBurst_OnlyFirstDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            var minion = PlayCard("BladeBattalion");

            GoToPlayCardPhase(vanish);
            var card = PlayCard("ConcussiveBurst");
            AssertInPlayArea(vanish, card);

            AssertNumberOfStatusEffectsInPlay(0);

            DealDamage(vanish, baron.CharacterCard, 1, DamageType.Melee);

            string messageText = $"Reduce damage dealt by {baron.Name} by 1.";

            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, baron.TurnTaker);
            AssertStatusEffectsContains(messageText);

            DealDamage(vanish, minion, 1, DamageType.Melee);

            //no status effect applied
            AssertNumberOfStatusEffectsInPlay(1);
        }

        [Test]
        public void ConcussiveBurst_NotAppliedOnNoDamage()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(vanish);
            var card = PlayCard("ConcussiveBurst");
            AssertInPlayArea(vanish, card);

            AssertNumberOfStatusEffectsInPlay(0);

            DealDamage(vanish, baron.CharacterCard, 1, DamageType.Melee);

            //baron immune to damage, no status effect
            AssertNumberOfStatusEffectsInPlay(0);

            DestroyCard("MobileDefensePlatform");

            //first damage dealt, apply
            DealDamage(vanish, baron.CharacterCard, 1, DamageType.Melee);

            string messageText = $"Reduce damage dealt by {baron.Name} by 1.";

            AssertNumberOfStatusEffectsInPlay(1);
            AssertStatusEffectAssociatedTurnTaker(0, baron.TurnTaker);
            AssertStatusEffectsContains(messageText);
        }

        [Test]
        public void ConcussiveBurst_NotAppliedToHeroTargets()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(vanish);
            var card = PlayCard("ConcussiveBurst");
            AssertInPlayArea(vanish, card);

            AssertNumberOfStatusEffectsInPlay(0);

            DealDamage(vanish, ra.CharacterCard, 1, DamageType.Melee);

            //baron immune to damage, no status effect
            AssertNumberOfStatusEffectsInPlay(0);
        }

        [Test]
        public void FlickeringStrike_Discard0()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();
            var mdp = GetCardInPlay("MobileDefensePlatform");

            GoToPlayCardPhase(vanish);


            DecisionSelectWordSkip = true;
            DecisionDoNotSelectCard = SelectionType.DiscardCard;
            QuickHandStorage(vanish, ra, wraith);
            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            var card = PlayCard("FlickeringStrike");
            AssertInTrash(vanish, card);

            //two cards drawn
            QuickHandCheck(2, 0, 0);
            //no damage dealt
            QuickHPCheck(0, 0, 0, 0, 0);
        }

        [Test]
        public void FlickeringStrike_Discard1()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();
            var mdp = GetCardInPlay("MobileDefensePlatform");

            GoToPlayCardPhase(vanish);

            DecisionSelectCards = new Card[] { vanish.HeroTurnTaker.Hand.Cards.First(), mdp, null };
            QuickHandStorage(vanish, ra, wraith);
            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            var card = PlayCard("FlickeringStrike");
            AssertInTrash(vanish, card);

            //two cards drawn, 1 discarded
            QuickHandCheck(1, 0, 0);
            //1 unit of damage dealt
            QuickHPCheck(0, 0, 0, 0, -1);
        }

        [Test]
        public void FlickeringStrike_DiscardAll()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();
            var mdp = GetCardInPlay("MobileDefensePlatform");

            //stack deck & hand
            var card = GetCard("FlickeringStrike");
            MoveCard(vanish, card, vanish.HeroTurnTaker.Deck, true);
            AssertInDeck(card); //just to ensure our setup is working

            var sequence = new Card[]
            {
                GetTopCardOfDeck(vanish, 0),
                ra.CharacterCard,
                GetTopCardOfDeck(vanish, 1),
                wraith.CharacterCard,
                GetCardFromHand(vanish, 0),
                mdp,
                GetCardFromHand(vanish, 1),
                mdp,
                GetCardFromHand(vanish, 2),
                mdp,
                GetCardFromHand(vanish, 3),
                mdp,
                null
            };

            GoToPlayCardPhase(vanish);

            DecisionSelectCards = sequence;
            QuickHandStorage(vanish, ra, wraith);
            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            PlayCard(card);
            AssertInTrash(vanish, card);

            //two cards drawn, 6 discarded
            QuickHandCheck(-4, 0, 0);
            //1 unit of damage dealt
            QuickHPCheck(0, 0, -1, -1, -4);
        }

        [Test]
        public void FocusingGauntlet()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();
            var mdp = GetCardInPlay("MobileDefensePlatform");

            var card = PlayCard("FocusingGauntlet");
            AssertInPlayArea(vanish, card);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            DealDamage(vanish, mdp, 1, DamageType.Energy);
            QuickHPCheck(0, 0, 0, 0, -2);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            DealDamage(vanish, mdp, 1, DamageType.Melee);
            QuickHPCheck(0, 0, 0, 0, -1);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, ra.CharacterCard, wraith.CharacterCard, mdp);
            DealDamage(baron, vanish, 1, DamageType.Energy);
            QuickHPCheck(0, -1, 0, 0, 0);
        }

        [Test]
        public void FlashRecon()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            //stack decks with harmless cards
            var played = StackDeck(baron, "MobileDefensePlatform");
            StackDeck(vanish, "FocusingGauntlet");
            StackDeck(ra, "FleshOfTheSunGod");
            StackDeck(wraith, "StunBolt");
            StackDeck(env, "PoliceBackup");

            DecisionSelectLocations = new[]
            {
                new LocationChoice(baron.TurnTaker.Deck),
                new LocationChoice(vanish.TurnTaker.Deck),
                new LocationChoice(ra.TurnTaker.Deck),
                new LocationChoice(wraith.TurnTaker.Deck),
                //new LocationChoice(env.TurnTaker.Deck), env deck is selected automatically since it's the last selection, yuck
                new LocationChoice(baron.TurnTaker.Deck)
            };
            var card = PlayCard("FlashRecon");
            AssertInTrash(vanish, card);

            AssertInPlayArea(baron, played);
            AssertNumberOfCardsInRevealed(baron, 0);
            AssertNumberOfCardsInRevealed(vanish, 0);
            AssertNumberOfCardsInRevealed(ra, 0);
            AssertNumberOfCardsInRevealed(wraith, 0);
            AssertNumberOfCardsInRevealed(env, 0);
        }



        [Test]
        public void Blink()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            var drawn = vanish.TurnTaker.Deck.TopCard;
            var played = PutInHand("ConcussiveBurst");

            DecisionSelectCardToPlay = played;

            //will use the base power and deal some damage or something, don't matter. we just check it was used.
            var card = PlayCard("Blink");
            AssertInTrash(vanish, card);

            AssertNotUsablePower(vanish, vanish.CharacterCard);
            AssertInHand(drawn);
            AssertInPlayArea(vanish, played);
        }
    }
}
