using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mars_Restaurant
{
    // This class retains the name for an employee
    public class NameOfPerson
    {
        private string prefix;
        private string firstName;
        private string middleName;
        private string lastName;
        private string affix;

        public string getPrefix()
        {
            return prefix;
        }

        public void setPrefix(string pref)
        {
            prefix = pref;
        }

        public string getFirstName()
        {
            return firstName;
        }

        public void setFirstName(string fname)
        {
            firstName = fname;
        }

        public string getMiddleName()
        {
            return middleName;
        }

        public void setMiddleName(string mname)
        {
            middleName = mname;
        }

        public string getLastName()
        {
            return lastName;
        }

        public void setLastName(string lname)
        {
            lastName = lname;
        }

        public string getAffix()
        {
            return affix;
        }

        public void setAffix(string aname)
        {
            affix = aname;
        }

        public NameOfPerson()
        {
            prefix = "";
            firstName = "";
            middleName = "";
            lastName = "";
            affix = "";
        }

        public NameOfPerson nameOfPersonCopyThis()
        {
            return this;
        }
    }
}
