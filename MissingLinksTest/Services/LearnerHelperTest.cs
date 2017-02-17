using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissingLinks.Controllers;
using MissingLinks.Models;
using MissingLinks.Services;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MissingLinksTest.Services
{
    [TestFixture]
    public class LearnerHelperTest
    {
        private LearnerHelper _testObject;
        private Mock<IPokeApiService> _pokeService;

        [SetUp]
        public void SetUp()
        {
            _pokeService = new Mock<IPokeApiService>();
            _testObject = new LearnerHelper(_pokeService.Object);
        }

        [Test]
        public void GetApiResults_Returns_Snark_If_Pokemon_Is_Smeargle_And_Move_Not_Sketch()
        {
            var input = new InputModel {Pokemon = "Smeargle", Move = "whatever"};
            _pokeService.Setup(x => x.GetAllPokemon())
                .Returns(new List<ApiPokemon>
                {
                    new ApiPokemon
                    {
                        Name = "smeargle",
                        Moves = new List<Move> {new Move {Name = "sketch", LevelUp = true}}
                    }
                });

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("Go sketch it, nerd.", actual);
            _pokeService.Verify(x => x.GetPokemonWithMove(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetApiResults_Returns_Message_If_Pokemon_Not_Given()
        {
            var input = new InputModel { Pokemon = "", Move = "whatever" };

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("I can't tell you about a Pokemon if you haven't given one!", actual);
            _pokeService.Verify(x => x.GetPokemonWithMove(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetApiResults_Returns_Message_If_Move_Not_Given()
        {
            var input = new InputModel { Pokemon = "stuff", Move = "" };

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("I can't tell you about a move if you haven't given one!", actual);
            _pokeService.Verify(x => x.GetPokemonWithMove(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetApiResults_Returns_Message_If_Pokemon_Doesnt_Learn_Move()
        {
            var input = new InputModel { Pokemon = "Charmnder", Move = "Water gun" };

            _pokeService.Setup(x => x.GetPokemonWithMove("water gun")).Returns(new List<ApiPokemon> ());

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("Charmnder doesn't seem to learn Water Gun. Did you spell something wrong?", actual);
        }

        [Test]
        public void GetApiResults_Returns_Correct_Result_If_Pokemon_Learns_Move()
        {
            var input = new InputModel { Pokemon = "Smeargle", Move = "Sketch" };
            _pokeService.Setup(x => x.GetPokemonWithMove("sketch"))
                .Returns(new List<ApiPokemon>
                {
                    new ApiPokemon
                    {
                        Name = "smeargle",
                        Moves = new List<Move> {new Move {Name = "sketch", LevelUp = true}}
                    }
                });

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("Smeargle learns Sketch on its own, or can be taught the move. No breeding necessary!", actual);
        }

        [Test]
        public void GetApiResults_Returns_Compatible_Direct_Learners()
        {
            var input = new InputModel { Pokemon = "ferroseed", Move = "spikes" };
            var ferroseed = new ApiPokemon {Name = "ferroseed", Moves = new List<Move> { new Move {Name = "spikes", Breed = true } }, EggGroups = new List<string> { "plant", "mineral" } };
            var klefki = new ApiPokemon {Name = "klefki", Moves = new List<Move> { new Move {Name = "spikes", LevelUp = true } }, EggGroups = new List<string> { "mineral" } };
            var cacnea = new ApiPokemon {Name = "cacnea", Moves = new List<Move> { new Move { Name = "spikes", LevelUp = true } }, EggGroups = new List<string> {"plant", "humanshape" } };
            var charmander = new ApiPokemon {Name = "charmander"};
            _pokeService.Setup(x => x.GetPokemonWithMove("spikes")).Returns(new List<ApiPokemon> { klefki, cacnea, charmander, ferroseed });

            var actual = _testObject.GetApiResults(input);

            Assert.AreEqual("Learn directly from:  Klefki Cacnea", actual);
        }
    }
}
