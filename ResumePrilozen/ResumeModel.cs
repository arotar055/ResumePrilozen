using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumePrilozen
{
    public class ResumeModel
    {
        public string UserFullName { get; set; }
        public int UserAge { get; set; }
        public string MaritalStat { get; set; }
        public string UserAddress { get; set; }
        public string UserEmail { get; set; }
        public string UserSkills { get; set; }

        public override string ToString()
        {
            // Разделитель — точка с запятой
            return $"{UserFullName};{UserAge};{MaritalStat};{UserAddress};{UserEmail};{UserSkills}";
        }

        public static ResumeModel FromLine(string line)
        {
            var parts = line.Split(';');
            if (parts.Length < 6) return null;
            int parsedAge = 0;
            int.TryParse(parts[1], out parsedAge);

            return new ResumeModel
            {
                UserFullName = parts[0],
                UserAge = parsedAge,
                MaritalStat = parts[2],
                UserAddress = parts[3],
                UserEmail = parts[4],
                UserSkills = parts[5]
            };
        }
    }
}
