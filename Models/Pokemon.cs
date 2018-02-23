using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Models
{
    public class Pokemon
    {
      public int Id
      {
         get; set;
      }

      public int PokemonId
      {
         get; set;
      }

      public int Gen
      {
         get; set;
      }

      public string Name
      {
         get; set;
      }

      public bool Active
      {
         get; set;
      }
   }
}
