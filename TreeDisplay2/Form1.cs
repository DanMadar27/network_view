using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace TreeDisplay2
{
    public partial class Form1 : Form
    {
        private AllMachines machines; //instance of AllMachine that represent the all machines in the local network
        private Machine myComputer; //instance of Machine that represent the router 
        private Machine router; //instance of Machine that represent the router
        private string status = "hostname"; //status of the nodes output in the tree


        public static List<Dictionary<string, string>> LoadJson(string path) //return dictionary from json file
        {
            try
            {
                using (StreamReader r = new StreamReader(path))
                {

                    string json = r.ReadToEnd();
                    List<Dictionary<string, string>> items =
                        JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
                    return items;
                }
            }
            catch(FileNotFoundException)
            {
                return null;
            }
        }

        

        public static Machine DictionaryToMachine(Dictionary<string, string> dictionary) 
            //convert dictionary to mahchine instance
        {
            string ip = dictionary["ip"];
            string mac = dictionary["mac"];
            string hostname = dictionary["hostname"];
            bool isMyComputer = (dictionary["myComputer"] == "true");
            bool isRouter = (dictionary["router"] == "true");
            bool isUsingSNMP = (dictionary["isUsingSNMP"] =="true");
            if (isUsingSNMP)
            {
                string sysUptime = dictionary["sysUptime"];
                string sysName = dictionary["sysName"];
                string sysDescription = dictionary["sysDescription"];
                string sysContact = dictionary["sysContact"];
                string sysLocation = dictionary["sysLocation"];
                return new SNMP_Machine(ip, mac, hostname, isMyComputer, isRouter,sysUptime ,sysName,
                    sysDescription, sysContact, sysLocation);
            }
            return new Machine(ip, mac, hostname, isMyComputer, isRouter);
            
        }



        public static AllMachines Get_Machines(string path) //returns all machines
        {
            List<Dictionary<string,string>> lst = LoadJson(path);
            if (lst != null && lst.Count!=0)
            {
                AllMachines machines = new AllMachines();
                foreach (Dictionary<string,string> dictionary in lst)
                {
                    machines.Add(DictionaryToMachine(dictionary));
                }
                return machines;
            }
            return null;
        }

        public static AllMachines LoadJsonMyComputer_Router() //return my computer and the router
        {
            return Get_Machines(@"D:\MyFolder\Json and Pickle files\myComputer_router.json");
        }

        public static AllMachines LoadJsonNewComputers() // return new computers that scanned
        {
            return Get_Machines(@"D:\MyFolder\Json and Pickle files\new_computers.json");
        }

        public static List<Dictionary<string, string>> LoadJsonShutdownComputers() //return info about known computers
        {                                                                          // (shutdown: true/false)
            return LoadJson(@"D:\MyFolder\Json and Pickle files\shutdown_computers.json"); ;
        }

        public static List<Dictionary<string,string>> LoadJsonSNMP_UptimeUpdates() //return info about snmp machines uptime
        {
            return LoadJson(@"D:\MyFolder\Json and Pickle files\computers_upTime_updates.json");
        }




        public static Machine NodeToMachine(TreeNode node)  // returns machine instance that represent the node
        {
            return ((Machine)node.Tag);
        }

        public static SNMP_Machine NodeToSNMP_Machine(TreeNode node)
        {
            return ((SNMP_Machine)node.Tag);
        }

        public static TreeNode GetNodeByIP(TreeView treeView1,string ip) //returns the node with the given ip address
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if(myComputer_node.Name==ip)
            {
                return myComputer_node;
            }
            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if(router_node.Name==ip)
            {
                return router_node;
            }
            
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) //nodes of snmp machines
            {
                if (node.Name == ip)
                {
                    return node;
                }
            }

            foreach (TreeNode node in treeView1.Nodes[0].Nodes[2].Nodes) //nodes of other
            {
                if(node.Name==ip)
                {
                    return node;
                }
            }
            return null;

        }

        public static string GetNodeOutput(TreeNode node,string status) //get the node output that need to be in the tree
        {
            if(status=="ip")
            {
                return NodeToMachine(node).IP;
            }
            if(status=="mac")
            {
                return NodeToMachine(node).Mac;
            }
            return NodeToMachine(node).Hostname;

        }

        public static bool NodeExists(TreeView treeView1, TreeNode node) //return true or false if the node exists
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if (myComputer_node.Name == node.Name)
            {
                return true;
            }

            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if (router_node.Name == node.Name)
            {
                return true;
            }

            foreach (TreeNode tn in treeView1.Nodes[0].Nodes[1].Nodes) //nodes of snmp machines
            {
                if (tn.Name == node.Name)
                {
                    return true;
                }
            }

            foreach (TreeNode tn in treeView1.Nodes[0].Nodes[2].Nodes) //nodes of other
            {
                if (tn.Name == node.Name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RunPythonScript(string path, string arguments, bool window_hidden) //run python script
                                                                                              //arguments- path+args
        {
            Process process = new Process();
            process.StartInfo.FileName =path;
            process.StartInfo.Arguments = arguments;
            if (window_hidden)
            {
                //the window of the process will be hidden
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            process.Start();
            process.WaitForExit();
        }

        public static void RunPythonScript(string path, bool window_hidden) //run python script
                                                                            //arguments- path+args
        {
            Process process = new Process();
            process.StartInfo.FileName =path;
            if (window_hidden)
            {
                //the window of the process will be hidden
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            process.Start();
            process.WaitForExit();
        }

        public static void ScanAll(TreeView treeView1,ref AllMachines machines,string status,TextBox textBox
            ,Button button, System.Windows.Forms.Timer timer)
            //running python scan_all.py script (scanning local network)
        {
            RunPythonScript(@"D:\MyFolder\Python Scripts\scan_all.py", false);
            button.Enabled = true; //enabling the button
            timer.Enabled = false; // disabling the timer
            UpdateTree(treeView1, ref machines, status, textBox);
        }

        public static void RunGetShutdownComputers(string all_ip) // running python getShutdownComputers.py script

        {
            RunPythonScript(@"D:\MyFolder\Python Scripts\getShutdownComputers.py", all_ip, true);
        }

        public static void RunMyComputer_Router() 
        {
            RunPythonScript(@"D:\MyFolder\Python Scripts\getMyComputer_router.py", true);
        }

        public static void RunUpdateSNMP_Uptime(string snmp_ip)
        {
            RunPythonScript(@"D:\MyFolder\Python Scripts\updateUptime.py",snmp_ip, true);
        }


        public static void UpdateTree(TreeView treeView1, ref AllMachines machines, string status,TextBox textBox)
            // update the treeview
        {
            foreach(TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) //nodes of snmp machines
            {
                if(node.BackColor==Color.Pink) //if the color of the node is pink
                {
                    node.BackColor = Color.Empty; //set the color to default
                }
            }
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[2].Nodes) //nodes of other
            {
                if (node.BackColor == Color.Pink) //if the color of the node is pink
                {
                    node.BackColor = Color.Empty; //set the color to default
                }
            }
            // get machines from json file
            AllMachines new_machines = LoadJsonNewComputers();
            if (new_machines != null) 
            {
                List<SNMP_Machine> snmp_machines = new_machines.SNMP_Machines(); //list of snmp machines
                List<Machine> other = new_machines.OtherMachines(); //list of other machines
                foreach(SNMP_Machine m in snmp_machines)
                {
                    if(!machines.IsMachineExists(m)) // if machine not in the database
                    {
                        machines.Add(m); //add machine to the database
                        //creating node, his tag is the machine, his name is the machine's ip 
                        //the node's color will be pink
                        TreeNode node = new TreeNode();
                        //machines define m hostname
                        node.Tag = machines.GetAllMachines()[machines.GetAllMachines().Count - 1]; 
                        node.Name = m.IP;
                        node.Text = GetNodeOutput(node, status); //get the node output that need to be in the tree
                        node.BackColor = Color.Pink;
                        treeView1.Nodes[0].Nodes[1].Nodes.Add(node); //add node to snmp machines
                    }
                }

                foreach (Machine m in other)
                {
                    if (!machines.IsMachineExists(m)) // if machine not in the database
                    {
                        machines.Add(m); //add machine to the database
                        //creating node, his tag is the machine, his name is the machine's ip 
                        //the node's color will be pink
                        TreeNode node = new TreeNode();
                        //machines define m hostname
                        node.Tag = machines.GetAllMachines()[machines.GetAllMachines().Count - 1]; 
                        node.Name = m.IP;
                        node.Text = GetNodeOutput(node, status); //get the node output that need to be in the tree
                        node.BackColor = Color.Pink;
                        treeView1.Nodes[0].Nodes[2].Nodes.Add(node); //add node to other machines
                    }
                       
                }    
            }
            // changing text to number of total machines
            textBox.Text = string.Format("{0} Machines", machines.GetAllMachines().Count);

        }

        public static void CheckForShutDownComputers(TreeView treeView1) //colors shutdown computers
        {
            string all_ip = "";
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) //nodes of snmp machines
            {
                all_ip += node.Name + " ";
            }
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[2].Nodes) //nodes of other
            {
                all_ip += node.Name + " ";
            }
            //getting status (up or down) for each node 
            RunGetShutdownComputers(all_ip); // running python script getShutdownComputers.py
            List<Dictionary<string, string>> lst = LoadJsonShutdownComputers();
            if (lst != null)
            {
                foreach (Dictionary<string, string> dictionary in lst) //lst=[{"ip":"0.0.0.0","valid":true/false}]
                {
                    string ip = dictionary["ip"];
                    bool valid = (dictionary["valid"] == "true");
                    TreeNode node = GetNodeByIP(treeView1, ip); //getting the node from the tree with the given ip
                    if (node != null)
                    {
                        if (valid) // if node is up
                        {
                            if (node.BackColor == Color.Red) //if it was in red color
                            {
                                node.BackColor = Color.Green; //turn its color to greeb
                            }
                            else if (node.BackColor == Color.Green) //if it in green color
                            {
                                node.BackColor = Color.Empty; //set the color to the default
                            }
                        }
                        else //if node is down
                        {
                            if (node.BackColor != Color.Red) //if node color is not red
                            {
                                node.BackColor = Color.Red; //set the node color to red
                            }
                        }
                    }
                }
            }

        }


        public static void UpdateSNMP_Uptime(TreeView treeView1,ref AllMachines machines)
        {
            string snmp_ip = "";
            TreeNode myComputer_node = treeView1.Nodes[0]; //my computer node
            if(myComputer_node.Tag is SNMP_Machine ) // if my computer is snmp machine
            {
                snmp_ip += myComputer_node.Name+" ";
            }
            TreeNode router_node = treeView1.Nodes[0].Nodes[0]; //router node
            if(router_node.Tag is SNMP_Machine)
            {
                snmp_ip += router_node.Name+" ";
            }

            foreach(TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) // nodes of snmp machines
            {
                snmp_ip += node.Name+" ";
            }
            
            // running python script that writes snmp machines uptime to json file
            RunUpdateSNMP_Uptime(snmp_ip);
            //get the info from json file
            List<Dictionary<string, string>> lst = LoadJsonSNMP_UptimeUpdates();
            if(lst!=null)
            {
                foreach (Dictionary<string, string> dictionary in lst) //lst=[{"ip":"0.0.0.0","upTime":"15 minutes"}]
                {
                    string ip = dictionary["ip"];
                    string uptime = dictionary["upTime"];
                    TreeNode node = GetNodeByIP(treeView1, ip); // get node with the given ip address
                    if(node.Text==NodeToSNMP_Machine(node).SysUptime) //if node text is his uptime
                    {
                        node.Text = uptime;
                    }
                    machines.UpdateUptime(ip, uptime); // update the machine uptime in the database
                    NodeToSNMP_Machine(node).SysUptime = uptime; // update the node's tag uptime
                }
            }

        }


        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen; // the form will be in the center of the screen
        }

        private void Form1_Load(object sender, EventArgs e) // this function start when loading the form
        {
            //get my computer and the router from json file
            AllMachines myComputer_router = LoadJsonMyComputer_Router();
            if (myComputer_router == null || myComputer_router.IsEmpty()) // if there isn't json file with information about my computer or the router
            {
                RunMyComputer_Router(); // running python script getMyComputer_router.py
                myComputer_router = LoadJsonMyComputer_Router();
            }
            myComputer = myComputer_router.MyComputer();
            router = myComputer_router.Router();
            machines = new AllMachines(); //define the database
            //adding my computer and the router to the database
            machines.Add(myComputer); 
            machines.Add(router);
            // creating nodes that represent my computer and the router
            TreeNode myComputer_node = new TreeNode();
            myComputer_node.Tag = myComputer;
            myComputer_node.Name = myComputer.IP;
            myComputer_node.Text = myComputer.Hostname;
            treeView1.Nodes.Add(myComputer_node); // adding my computer's node to the root of the tree
            TreeNode router_node = new TreeNode();
            router_node.Tag = router;
            router_node.Name = router.IP;
            router_node.Text = router.Hostname;
            treeView1.Nodes[0].Nodes.Add(router_node); // adding router's node to be the child of my computer's node
            // adding two categories of machines
            treeView1.Nodes[0].Nodes.Add("SNMP Computers"); 
            treeView1.Nodes[0].Nodes.Add("Other");
            // set colors of the treeview, my computer's node and the router's node
            treeView1.BackColor = Color.Yellow; 
            treeView1.Nodes[0].BackColor = Color.LightSkyBlue; 
            treeView1.Nodes[0].Nodes[0].BackColor = Color.LightSteelBlue;
            UpdateTree(treeView1, ref machines, status, textBox2);//Updating the tree

            // disables pink colors due to UpdateTree function
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) //nodes of snmp machines
            {
                if (node.BackColor == Color.Pink) //if the color of the node is pink
                {
                    node.BackColor = Color.Empty; //set the color to default
                }
            }
            foreach (TreeNode node in treeView1.Nodes[0].Nodes[2].Nodes) //nodes of other
            {
                if (node.BackColor == Color.Pink) //if the color of the node is pink
                {
                    node.BackColor = Color.Empty; //set the color to default
                }
            }


        }

        private void button1_Click(object sender, EventArgs e) //show ip button
        {
            status = "ip";
            ChangeNodesTextToIP(treeView1.Nodes[0], machines, status);
        }

        public static void ChangeNodesTextToIP(TreeNode tn, AllMachines machines, string status)
            //changing the nodes text to ip addresses
        {                                                                                       
                                                                                                
            if (tn.Name!="") // if the node represent machine
            {
                tn.Text = tn.Name; // the text will be the node's name (node's name is the ip of the machine)
            }
            foreach (TreeNode tn2 in tn.Nodes) // going through all nodes of the tree
            {
                ChangeNodesTextToIP(tn2, machines, status);
            }

        }

        private void button2_Click(object sender, EventArgs e) //show mac button
        {
            status = "mac";
            ChangeNodesTextToMac(treeView1.Nodes[0], machines, status);
        }

        public static void ChangeNodesTextToMac(TreeNode tn, AllMachines machines, string status)
        //changing the nodes text to mac addresses
        {
            if (tn.Name!="") // if the node represent machine
            {
                tn.Text = NodeToMachine(tn).Mac; // the text will be the node's mac address
            }
            foreach (TreeNode tn2 in tn.Nodes) // going through all nodes of the tree
            {
                ChangeNodesTextToMac(tn2, machines, status);
            }
        }

        private void button3_Click(object sender, EventArgs e) //show hostnames button
        {
            status = "hostname";
            ChangeNodesTextToHostname(treeView1.Nodes[0], machines, status);
        }

        public static void ChangeNodesTextToHostname(TreeNode tn, AllMachines machines, string status)
        //changing the nodes text to hostnames
        {
            if (tn.Name!="") // if the node represent machine
            {
                tn.Text = NodeToMachine(tn).Hostname; //the text will be the node's hostname
            }
            foreach (TreeNode tn2 in tn.Nodes) // going through all nodes of the tree
            {
                ChangeNodesTextToHostname(tn2, machines, status);
            }
        }



        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        //showing the selected node information
        {
            TreeNode node = treeView1.SelectedNode; // get the selected node
            if (node.Name!="") // if the node represent machine
            {
                MessageBox.Show(node.Tag.ToString()); // showing node information
            }
        }

        private void button4_Click(object sender, EventArgs e) // turn on computer button
        {
            TreeNode node = treeView1.SelectedNode; // get the selected node
            if(node.Name!="") // if the node represent machine
            {
                string mac = NodeToMachine(node).Mac; // get the node's mac address
                TurnOnComputer(mac); // turning on the computer with this mac address
            }
        }


        public static void TurnOnComputer(string mac)
        // run python script that turn on computer with the given mac address
        {
            RunPythonScript(@"D:\MyFolder\Python Scripts\file_wakeonlan.py", mac, true);
        }

        

        private void button5_Click(object sender, EventArgs e) 
            // changing snmp machines nodes text to the time in minutes since they started using snmp services
        {
            TreeNode myComputer_node = treeView1.Nodes[0]; // get the node of my computer 
            if(myComputer_node.Tag is SNMP_Machine) // if my computer is snmp_machine
            {
                myComputer_node.Text = ((SNMP_Machine)myComputer_node.Tag).SysUptime;
            }
            TreeNode router_node = treeView1.Nodes[0].Nodes[0]; // get the node of my router
            if(router_node.Tag is SNMP_Machine) // if my router is snmp_machine
            {
                router_node.Text = ((SNMP_Machine)router_node.Tag).SysUptime;
            }

            foreach(TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes) // child nodes of snmp machines
            {
                node.Text = ((SNMP_Machine)node.Tag).SysUptime;
            }
        }

        private void button6_Click(object sender, EventArgs e) 
            // changing snmp machines nodes text to their system name
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if (myComputer_node.Tag is SNMP_Machine)
            {
                myComputer_node.Text = ((SNMP_Machine)myComputer_node.Tag).SysName;
            }
            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if (router_node.Tag is SNMP_Machine)
            {
                router_node.Text = ((SNMP_Machine)router_node.Tag).SysName;
            }

            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes)
            {
                node.Text = ((SNMP_Machine)node.Tag).SysName;
            }
        }

        private void button7_Click(object sender, EventArgs e) 
            // changing snmp machines nodes text to their system description
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if (myComputer_node.Tag is SNMP_Machine)
            {
                myComputer_node.Text = ((SNMP_Machine)myComputer_node.Tag).SysDescription;
            }

            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if (router_node.Tag is SNMP_Machine)
            {
                router_node.Text = ((SNMP_Machine)router_node.Tag).SysDescription;
            }

            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes)
            {
                node.Text = ((SNMP_Machine)node.Tag).SysDescription;
            }
        }

        private void button8_Click(object sender, EventArgs e) //Machines contact name button
            // changing snmp machines nodes text to their contact name
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if (myComputer_node.Tag is SNMP_Machine)
            {
                myComputer_node.Text = ((SNMP_Machine)myComputer_node.Tag).SysContact;
            }

            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if (router_node.Tag is SNMP_Machine)
            {
                router_node.Text = ((SNMP_Machine)router_node.Tag).SysContact;
            }

            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes)
            {
                node.Text = ((SNMP_Machine)node.Tag).SysContact;
            }
        }

        private void button9_Click(object sender, EventArgs e) 
            // changing snmp machines nodes text to their location
        {
            TreeNode myComputer_node = treeView1.Nodes[0];
            if (myComputer_node.Tag is SNMP_Machine)
            {
                myComputer_node.Text = ((SNMP_Machine)myComputer_node.Tag).SysLocation;
            }

            TreeNode router_node = treeView1.Nodes[0].Nodes[0];
            if (router_node.Tag is SNMP_Machine)
            {
                router_node.Text = ((SNMP_Machine)router_node.Tag).SysLocation;
            }

            foreach (TreeNode node in treeView1.Nodes[0].Nodes[1].Nodes)
            {
                node.Text = ((SNMP_Machine)node.Tag).SysLocation;
            }
        }

        private void button12_Click(object sender, EventArgs e) // start scan button
        {
            button12.Enabled = false; //disabling the button
            timer1.Enabled = true;
            Thread thread = new Thread(()=>ScanAll(treeView1,ref machines,
                status,textBox2,button12,timer1)); // running python script "scan_all.py"
            thread.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) //timer for checking new computers
        {
            timer1.Stop(); 
            UpdateTree(treeView1, ref machines, status,textBox2);
            timer1.Start();
            
        }


        private void timer2_Tick(object sender, EventArgs e) //timer for checking shutdown computers
        {
            timer2.Stop();
            CheckForShutDownComputers(treeView1);
            timer2.Start();
        }

        private void timer3_Tick(object sender, EventArgs e) //timer for updating snmp machines uptime
        {
            timer3.Stop();
            UpdateSNMP_Uptime(treeView1, ref machines);
            timer3.Start();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) // set the textbox size to fit its text
        {
            Size size = TextRenderer.MeasureText(textBox2.Text, textBox2.Font);
            textBox2.Width = size.Width;
            textBox2.Height = size.Height;
        }

       
    }
}
