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
    public class VanishVariantsTests : BaseTest
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
        [Order(0)]
        public void FirstResponseVanishLoad()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish/FirstResponseVanishCharacter", "Haka", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(vanish);
            Assert.IsInstanceOf(typeof(FirstResponseVanishCharacterCardController), vanish.CharacterCardController);

            foreach (var card in vanish.HeroTurnTaker.GetAllCards())
            {
                var cc = GetCardController(card);
                Assert.IsTrue(cc.GetType() != typeof(CardController), $"{card.Identifier} is does not have a CardController");
            }

            Assert.AreEqual(27, vanish.CharacterCard.HitPoints);
        }


        [Test]
        public void FirstResponseVanishInnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish/FirstResponseVanishCharacter", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            var minion = PlayCard("BladeBattalion");

            AssertNumberOfStatusEffectsInPlay(0);
            UsePower(vanish);
            AssertNumberOfStatusEffectsInPlay(1);

            QuickHPStorage(baron.CharacterCard, vanish.CharacterCard, haka.CharacterCard, bunker.CharacterCard, scholar.CharacterCard, minion);
            DealDamage(baron, haka, 2, DamageType.Cold);
            QuickHPCheck(0, 0, 0, 0, 0, 0);

            AssertNumberOfStatusEffectsInPlay(0);
        }

        [Test]
        public void FirstResponseVanishIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish/FirstResponseVanishCharacter", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);
            DecisionSelectTurnTaker = haka.TurnTaker;
            DecisionSelectTarget = baron.CharacterCard;
            QuickHPStorage(baron.CharacterCard, haka.CharacterCard, bunker.CharacterCard, scholar.CharacterCard);
            UseIncapacitatedAbility(vanish, 0);
            QuickHPCheck(-2, 0, 0, 0);

        }

        [Test]
        public void FirstResponseVanishIncap2()
        {
            //This is a bad test, but the power kinda doesn't do anything either.

            SetupGameController("BaronBlade", "Cauldron.Vanish/FirstResponseVanishCharacter", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);
            DecisionSelectLocations = new[] { new LocationChoice(baron.TurnTaker.Deck), new LocationChoice(haka.TurnTaker.Deck) };

            UseIncapacitatedAbility(vanish, 1);

            AssertNumberOfCardsInRevealed(baron, 0);
            AssertNumberOfCardsInRevealed(haka, 0);
        }

        [Test]
        public void FirstResponseVanishIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Vanish/FirstResponseVanishCharacter", "Ra", "TheWraith", "Megalopolis");
            StartGame();

            DestroyCard("MobileDefensePlatform");

            SetupIncap(baron);
            AssertIncapacitated(vanish);

            GoToUseIncapacitatedAbilityPhase(vanish);

            AssertNumberOfStatusEffectsInPlay(0);
            UseIncapacitatedAbility(vanish, 2);
            AssertNumberOfStatusEffectsInPlay(1);

            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, wraith.CharacterCard);
            DecisionSelectTarget = wraith.CharacterCard;
            DealDamage(baron, ra, 2, DamageType.Infernal);
            QuickHPCheck(0, 0, -2);
            AssertNumberOfStatusEffectsInPlay(0);
        }


    }
}
