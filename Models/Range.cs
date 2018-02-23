using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Models
{
    public class Range
    {
      public int Id
      {
         get; set;
      }

      public string UserId
      {
         get; set;
      }

      public string ServerId
      {
         get; set;
      }

      public decimal RangeKm
      {
         get; set;
      }

      public double Latitude
      {
         get; set;
      }

      public double Longitude
      {
         get; set;
      }

      public bool Active
      {
         get; set;
      }

      public bool Rares
      {
         get; set;
      }

      public bool Pokemon
      {
         get; set;
      }

      public bool Starters
      {
         get; set;
      }

      public bool Raids
      {
         get; set;
      }

      public string RaidLevels
      {
         get; set;
      }

      public bool Mentions
      {
         get; set;
      }

      public DateTime CreatedOn
      {
         get; set;
      }
      public DateTime ModifiedOn
      {
         get; set;
      }
   }
}
