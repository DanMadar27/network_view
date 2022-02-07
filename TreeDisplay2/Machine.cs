using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeDisplay2
{
    public class Machine
    {
        private string ip; 
        private string mac;
        private string hostname;
        private bool isMyComputer; 
        private bool isRouter;
        
        public Machine(string ip, string mac, string hostname, bool isMyComputer, bool isRouter)
        {
            this.ip = ip;
            this.mac = mac;
            this.hostname = hostname;
            this.isMyComputer = isMyComputer;
            this.isRouter = isRouter;
            
        }

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public string Mac
        {
            get { return mac; }
            set { mac = value; }
        }

        public string Hostname
        {
            get { return hostname; }
            set { hostname = value; }
        }

        public bool IsMyComputer
        {
            get { return isMyComputer; }
            set { isMyComputer = value; }
        }

        public bool IsRouter
        {
            get { return isRouter; }
            set { isRouter = value; }
        }

        public string GetNodeOutput(string status) //returns the output of the machine in the treeView
        {
            if (status == "ip")
            {
                return ip;
            }
            else if (status == "mac")
            {
                return mac;
            }
            return hostname;
        }

        public override string ToString()
        {
            if (isMyComputer)
            {
                return string.Format("My Computer: ip: {0} , mac: {1} , hostname: {2}", ip, mac, hostname);
            }
            if (isRouter)
            {
                return string.Format("Router: ip: {0} , mac: {1} , hostname: {2}", ip, mac, hostname);
            }

            return string.Format("ip: {0} , mac: {1} , hostname: {2}", ip, mac, hostname);
        }
    }
}