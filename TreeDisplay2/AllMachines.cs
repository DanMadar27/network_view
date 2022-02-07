using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TreeDisplay2
{
    public class AllMachines
    {
        private List<Machine> machines; 
        private int number_hostname; //being used in Add method
        
        public AllMachines()
        {
            machines = new List<Machine>();
            number_hostname = 1;
        }

        public void Add(Machine m) // adds a machine to the machines list
        {
            if (m.Hostname.StartsWith("Unknown hostname"))
            {
                m.Hostname = "Unknown hostname " + number_hostname; //prevent situation that different machines 
                number_hostname++;                                  //have the same hostname(machine1.hostname="Unknown hostname1"
            }                                                       //,machine2.hostname="Unknown hostname 2")
            machines.Add(m);
        }


        public int Number_Hostname
        {
            get { return number_hostname; }
            set { number_hostname = value; }
        }

        public List<Machine> GetAllMachines()
        {
            return machines;
        }


        public bool IsEmpty() //returns true or false if the machines list is empty
        {
            return machines.Count == 0;
        }

        public bool IsMachineExists(Machine m) //returns true or false if the machines exists
        {
            foreach (Machine m2 in machines)
            {
                if (m2.IP == m.IP)
                {
                    return true;
                }
            }
            return false;

        }


        public Machine MyComputer() //returns my computer from the machines list
        {
            foreach (Machine m in machines)
            {
                if (m.IsMyComputer)
                {
                    return m;
                }
            }
            return null;
        }
        public Machine Router() // returns the router from the machines list
        {
            foreach (Machine m in machines)
            {
                if (m.IsRouter)
                {
                    return m;
                }
            }
            return null;
        }

        public List<SNMP_Machine> SNMP_Machines() //returns list of snmp machines that not my computer or the router
        {
            List<SNMP_Machine> lst = new List<SNMP_Machine>();
            foreach (Machine m in machines)
            {
                if (!(m.IsMyComputer) && !(m.IsRouter) && (m is SNMP_Machine))
                {
                    lst.Add(((SNMP_Machine)m));
                }
            }
            return lst;
        }


        public List<Machine> OtherMachines() //returns list of machines that not snmp machines and not my computer or the router
        {
            List<Machine> lst = new List<Machine>();
            foreach (Machine m in machines)
            {
                if (!(m.IsMyComputer) && !(m.IsRouter) && !(m is SNMP_Machine))
                {
                    lst.Add(m);
                }
            }
            return lst;
        }

        public void UpdateUptime(string ip,string uptime) //update uptime of snmp machine
        {
            for(int i=0;i<machines.Count;i++)
            {
                if(machines[i].IP==ip && machines[i] is SNMP_Machine)
                {
                    ((SNMP_Machine)machines[i]).SysUptime = uptime;
                    break;
                }
            }
        }


        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < machines.Count; i++) 
            {
                s += machines[i].ToString();
                if (i != (machines.Count - 1)) // if not the last element in the list
                {
                    s += "\n";
                }
            }
            return s;
        }
    }
}
