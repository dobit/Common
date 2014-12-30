using System.Text;

namespace LFNet.Common.Extensions
{
    public class PersonNameInfo
    {
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.Salutation.IsNullOrWhiteSpace())
            {
                builder.Append(this.Salutation).Append(" ");
            }
            if (this.FirstName.IsNullOrWhiteSpace())
            {
                builder.Append(this.FirstName).Append(" ");
            }
            if (this.MiddleName.IsNullOrWhiteSpace())
            {
                builder.Append(this.MiddleName).Append(" ");
            }
            if (this.LastName.IsNullOrWhiteSpace())
            {
                builder.Append(this.LastName).Append(" ");
            }
            if (this.Suffix.IsNullOrWhiteSpace())
            {
                builder.Append(this.Suffix).Append(" ");
            }
            return builder.ToString().Trim();
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleInitial
        {
            get
            {
                if (!string.IsNullOrEmpty(this.MiddleName))
                {
                    char ch = this.MiddleName[0];
                    return ch.ToString();
                }
                return this.MiddleName;
            }
        }

        public string MiddleName { get; set; }

        public string Salutation { get; set; }

        public string Suffix { get; set; }
    }
}

