using System.Collections.Generic;
using MissingLinks.Models;

namespace MissingLinks.Services
{
    public interface IPokeApiService
    {
        List<ApiPokemon> GetAllPokemon();
        List<ApiPokemon> GetPokemonWithMove(string move);
    }
}