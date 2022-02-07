using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeDisplay2
{
    public class SNMP_Machine : Machine
    {
        private string sysUptime; //time passed since the network management portion of the systems was last re-initialized
        private string sysName; // machine name
        private string sysDescription; // machine operating system description
        private string sysContact; // machine contact name
        private string sysLocation; // machine location

        public SNMP_Machine(string ip, string mac, string hostname, bool isMyComputer,
            bool isRouter,string sysUptime ,string sysName, string sysDescription, string sysContact, string sysLocation)
            : base(ip, mac, hostname, isMyComputer, isRouter)
        {
            if(sysUptime=="" || sysUptime==null)

            {
                this.sysUptime = "Unknown";
            }

            else
            {
                this.sysUptime = sysUptime;
            }
            if(sysName=="" || sysName==null)
            {
                this.sysName = "Unknown";
            }
            else
            {
                this.sysName = sysName;
            }

            if (sysDescription == "" || sysDescription==null)
            {
                this.sysDescription = "Unknown";
            }
            else
            {
                this.sysDescription = sysDescription;
            }

            if(sysContact=="" || sysContact==null)
            {
                this.sysContact= "Unknown";
            }
            else
            {
                this.sysContact = sysContact;
            }

            if(sysLocation=="" || sysLocation==null)
            {
                this.sysLocation = "Unknown";
            }
            else
            {
                this.sysLocation = sysLocation;
            }

        }
        
        // Get & Set

        public string SysUptime
        {
            get { return sysUptime; }
            set { sysUptime = value; }
        }

        public string SysName
        {
            get { return sysName; }
        }

        public string SysDescription
        {
            get { return sysDescription; }
        }

        public string SysContact
        {
            get { return sysContact; }
        }

        public string SysLocation
        {
            get { return sysLocation; }
        }


        

        public override string ToString()
        {
            string s = base.ToString() + "\n";
            s += string.Format("Machine Up time: {0}",sysUptime);
            s += string.Format("Machine name: {0}\n", sysName);
            s += string.Format("Machine description: {0}\n", sysDescription);
            s += string.Format("Machine contact name: {0}\n", sysContact);
            s += string.Format("Machine location: {0}", sysLocation);
            return s;
        }


    }
}
