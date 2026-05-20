using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class AttendenceOption : IAttendenceOption
    {
        public AttendenceOption(int iD, string altText, IsPresent? value, int orchestraID, string colorLight, string colorDark, string symbolName, string comment, bool isMandatory, int order, bool canBeFilled)
        {
            ID = iD;
            AltText = altText;
            Value = value;
            OrchestraID = orchestraID;
            ColorLight = colorLight;
            ColorDark = colorDark;
            SymbolName = symbolName;
            Comment = comment;
            IsMandatory = isMandatory;
            Order = order;
            CanBeFilled = canBeFilled;
        }

        public int ID { get; }
        public string AltText { get; }
        public IsPresent? Value { get; }
        public int OrchestraID { get; }
        public string ColorLight { get; }
        public string ColorDark { get; }
        public string SymbolName { get; }
        public string Comment { get; }
        public bool IsMandatory { get; }
        public int Order { get; }
        public bool CanBeFilled { get; }
    }
}