using System;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal abstract class DateBase : IDate
    {
        protected DateBase(int iD, DateTime dateAt, string locationAt)
        {
            ID = iD;
            DateAt = dateAt;
            LocationAt = locationAt;
        }

        public int ID { get; }
        public DateTime DateAt { get; }
        public string LocationAt { get; }

        public override string ToString()
        {
            using (CultureRange.German())
            {
                return $"{DateAt:d}";
            }
        }
    }
}