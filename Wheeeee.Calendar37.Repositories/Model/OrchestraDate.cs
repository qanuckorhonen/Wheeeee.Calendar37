using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    public class OrchestraDate : IOrchestraDate
    {
        public OrchestraDate(IOrchestra orchestra, IDate date)
        {
            Orchestra = orchestra;
            Date = date;
        }

        public IOrchestra Orchestra { get; }
        public IDate Date { get; }

        public override string ToString()
        {
            return $"{Orchestra} - {Date}";
        }
    }
}
