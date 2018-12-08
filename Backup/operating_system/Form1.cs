using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace operating_system
{
    public partial class Form1 : Form
    {
        /*文件的结构
         *标志位1个字节
         * 文件名3
         * 扩展名3
         * 物理地址1
         * 长度1
         * 索引标志位
         */
        public Form1()
        {
            InitializeComponent();
        }
        public struct PCB
        {
            public int index;
            public string command;
            public int PC;
            public int start;
            public char instructment;
            public int time;
            public int result;
        };//------------------------------------------------------------进程控制块儿
        public struct Instruct
        {
            public char name;
            public int state;
            public int index;
            public int time;
        };
        public static byte[] Bitmapc = new byte[128];//---------------------------C盘位示图
        public static byte[] Bitmapd = new byte[128];//---------------------------D盘位示图
        public static int disk_countc, disk_countd;//-------------------------------C、D盘使用块数
        public static PCB[] PCB_ready = new PCB[10];
        public static PCB[] PCB_block = new PCB[10];
        public static PCB[] PCB_wait = new PCB[10];
        public static Instruct[] instructment = new Instruct[6];  
        public static string path1 = "disk1.txt";
        public static string path2 = "disk2.txt";
        public static string road;
        public static int count_process,count_processes;//------------------------------------------记录进程数目
        public static int time;//----------------------------------------------------记录时间

        private void Form1_Load(object sender, EventArgs e)//-------------------Form1_Load进行初始化工作
        {
            initiate_lable();
            initiate_treeview();
            FileInfo TheFile = new FileInfo(path1);
            if (TheFile.Exists == false)
            {
                initiate_disc();
                initiate_catalog();
            }
            else
            {
                deduce_disc();
                deduce_treeview("c", 1);
                deduce_treeview("d", 1);
            }
            initiate_PCB();
            initiate_mainmomory();
            initiate_instructment();
            textBox20.Text ="闲逛进程";
            timer1.Enabled = true;
            timer2.Enabled = true;
            textBox1.Focus();
            button2.Enabled = false;
            textBox2.Enabled = false;
        }
        public void initiate_treeview()//--------------------------------------------------treeview初始化
        {
            TreeNode tmp0 = new TreeNode("我的电脑");
            tmp0.ImageIndex = 0;
            tmp0.SelectedImageIndex = 0;
            this.treeView1.Nodes.Add(tmp0);
            this.treeView1.SelectedNode = tmp0;
            TreeNode tmp1 = new TreeNode("磁盘C");
            tmp1.ImageIndex = 1;
            tmp1.SelectedImageIndex = 1;
            TreeNode tmp2 = new TreeNode("磁盘D");
            tmp2.ImageIndex = 1;
            tmp2.SelectedImageIndex = 1;
            this.treeView1.SelectedNode.Nodes.Add(tmp1);
            this.treeView1.SelectedNode.Nodes.Add(tmp2);
        }
        public void initiate_disc()//--------------------------------------------------------初始化磁盘
        {
            for (int i = 0; i < Bitmapc.Length; i++)
                if (i <= 1)
                {
                    Bitmapc[i] = 1;
                    Bitmapd[i] = 1;
                }
                else
                {
                    Bitmapc[i] = 0;
                    Bitmapd[i] = 0;
                }
            disk_countc = 2;
            disk_countd = 2;
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox3.Text += disk_countc;
            textBox4.Text += (128 - disk_countc);
            textBox5.Text += disk_countd;
            textBox6.Text += (128 - disk_countd);
        }
        public void initiate_lable()
        {
            for (int i = 0; i < c_lable .GetLength(0); i++)
                for (int j = 0; j < c_lable .GetLength(1); j++)
                {
                    c_lable[i, j] = new Label();
                    c_lable[i, j].BackColor = Color.White;
                    c_lable[i, j].BorderStyle = BorderStyle.FixedSingle;
                    c_lable[i, j].Height = 16;
                    c_lable[i, j].Width = 16;
                    c_lable[i, j].Location = new Point(10 +i * 16, 10 +j * 16);
                    groupBox3.Controls.Add(c_lable [i,j]);
                }
            c_lable[0,0].BackColor = Color.Red;
            c_lable[1,0].BackColor =Color .Red ;
            for (int i = 0; i < d_lable.GetLength(0); i++)
                for (int j = 0; j < d_lable.GetLength(1); j++)
                {
                    d_lable[i, j] = new Label();
                    d_lable[i, j].BackColor = Color.White;
                    d_lable[i, j].BorderStyle = BorderStyle.FixedSingle;
                    d_lable[i, j].Height = 16;
                    d_lable[i, j].Width = 16;
                    d_lable[i, j].Location = new Point(10 + i* 16, 10 + j * 16);
                    groupBox4.Controls.Add(d_lable[i, j]);
                }
            d_lable[0,0].BackColor = Color.Red;
            d_lable[1,0].BackColor = Color.Red;
        }//-----------------------------------------------------初始化lable
        public void initiate_catalog()//----------------------------------------------------初始化子目录
        {
            byte[] one =new byte[16384];
            for (int i = 0; i <one.Length; i++)
                one[i]=0;
            FileStream file1 =new FileStream(path1,FileMode.CreateNew);
            file1.Seek(0,SeekOrigin.Begin);
            file1.Write(one,0,one.Length);
            for (int i = 0; i < 8; i++)
            {
                file1.Seek(i*16+128,SeekOrigin.Begin);
                file1.WriteByte(0);
            }
            file1.Close();
            FileStream file2 =new FileStream(path2,FileMode.CreateNew);
            file2.Seek(0,SeekOrigin.Begin);
            file2.Write(one,0,one.Length);
            for (int i = 0; i < 8; i++)
            {
                file2.Seek(i*16+128, SeekOrigin.Begin);
                file2.WriteByte(0);
            }
            file2.Close();
        }
        public byte apply_blank(string disk)//-----------------------------------------审请空白块
        {
            int k, x, y;
            if(disk.StartsWith("c") || disk.StartsWith("C"))
            {
                for (k = 2; k < Bitmapc.Length; k++)
                    if (Bitmapc[k] == 0)
                        break;
                Bitmapc[k] = 1;
                disk_countc++;
                textBox3.Text = "";
                textBox4.Text = "";
                textBox3.Text += disk_countc;
                textBox4.Text += (128 - disk_countc);
                x = k  % 16;
                y =k/16 ;
                c_lable[x, y].BackColor = Color.Red;
            }
            else
            {
                for (k = 0; k < Bitmapd.Length; k++)
                    if (Bitmapd[k] == 0)
                        break;
                Bitmapd[k] = 1;
                disk_countd++;
                textBox5.Text = "";
                textBox6.Text = "";
                textBox5.Text += disk_countd;
                textBox6.Text += (128 - disk_countd);
                x = k %16;
                y =k/16 ;
                d_lable[x, y].BackColor = Color.Red;;
            }
            return Convert.ToByte(k) ;
        }
        public void return_blank(string disk,byte n)//---------------------------------还磁盘块
        {
            int k, x, y;
            k = Convert.ToByte(n);
            if (disk.StartsWith("c") || disk.StartsWith("C"))
            {
                Bitmapc[k] = 0;
                disk_countc--;
                textBox3.Text = "";
                textBox4.Text = "";
                textBox3.Text += disk_countc;
                textBox4.Text += (128 - disk_countc);
                x =k% 16 ;
                y =k / 16 ;
                c_lable[x, y].BackColor = Color.White;
            }
            else
            {
                Bitmapd[k] = 0;
                disk_countd--;
                textBox5.Text = "";
                textBox6.Text = "";
                textBox5.Text += disk_countd;
                textBox6.Text += (128 - disk_countd);
                x = k% 16;
                y =k /  16 ;
                d_lable[x, y].BackColor = Color.White;
            }
        }
        public void deduce_disc()
        {
            int x, y;
            disk_countc = 2;
            disk_countd = 2;
            FileStream fs = new FileStream(path1, FileMode.Open, FileAccess.Read);
            fs.Seek(0,SeekOrigin.Begin);
            fs.Read(Bitmapc, 0, 128);
            fs.Close();
            for (int i = 2; i <Bitmapc.Length; i++)
                if (Bitmapc[i]==1)
                {
                    x =Convert.ToInt32( i) % 16;
                    y =Convert.ToInt32(i) / 16 ;
                    c_lable[x, y].BackColor = Color.Red; 
                    disk_countc++;
                }
            textBox3.Text = "";
            textBox4.Text = "";
            textBox3.Text += disk_countc;
            textBox4.Text += (128 - disk_countc);
            fs = new FileStream(path2, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(Bitmapd, 0,128);
            fs.Close();
            for (int i = 2; i <Bitmapd.Length; i++)
                if (Bitmapd[i]==1)
                {
                    x = Convert.ToInt32(i) % 16;
                    y = Convert.ToInt32(i)/16;
                   d_lable[x, y].BackColor = Color.Red;
                    disk_countd++;
                }
            textBox5.Text = "";
            textBox6.Text = "";
            textBox5.Text += disk_countd;
            textBox6.Text += (128 - disk_countd);
        }
        public void deduce_treeview(string path, byte current)
        {
            byte[] three = new byte[3];
            string[] paths = path.Split(new char[] { '\\'});
            string path0,path00;
            if (paths[0].StartsWith("c"))
                path0=path1;
            else
                path0=path2;
            for (int i = 0; i <8; i++)
                if (read_one(path0,current,i)==1)
                    if (read_expandname(path0, current, i) == "fil")
                    {
                        path00 = path + '\\' + read_name(path0, current, i) + '\\' + read_expandname(path0, current, i);
                        create_Node(path00);
                        three = read_add_lth(path0, current, i);
                        deduce_treeview(path + '\\' + read_name(path0, current, i), three[0]);
                    }
                    else
                    {
                        path00 = path + '\\' + read_name(path0, current, i) + '\\' + read_expandname(path0, current, i);
                        create_Node(path00);
                    }
        }

        public byte read_one(string path0, byte current, int x)
        {
            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(current *128+x*16,SeekOrigin.Begin);
            byte one;
            one=Convert.ToByte(fs.ReadByte());
            fs.Close();
            return one;
        }
        public string read_name(string path0, byte current, int x)
        {
            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(current * 128 + x * 16 + 1, SeekOrigin.Begin);
            byte[] names=new byte[3];
            fs.Read(names,0,3);
            string name = System.Text.Encoding.Default.GetString(names);
            name = name.Trim();
            fs.Close();
            return name;
        }
        public string read_expandname(string path0, byte current, int x)
        {
            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(current * 128 + x * 16 + 4, SeekOrigin.Begin);
            byte[] names = new byte[3];
            fs.Read(names, 0, 3);
            string name = System.Text.Encoding.Default.GetString(names);
            fs.Close();
            return name;
        }
        public byte[] read_add_lth(string path0, byte current, int x)
        {
            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
            byte[] three = new byte[3];
            fs.Read(three, 0, 3);
            fs.Close();
            return three;
        }

        public void add_file(string path)
        {
            string[] paths = path.Split(new char[] { '\\', '.' });
            if (paths.Length > 5 || (paths[paths.Length - 1] == "fil" & paths.Length == 5))
            {
                return;
            }
            int number = paths.Length;
            int flag = 0, k = 1, x = 0;
            byte current = 1;
            byte[] three=new byte[3];
            string path0;
            if (path.StartsWith("c") || path.StartsWith("C"))
                path0 = path1;
            else
                path0 = path2;
            while (k < number - 2)
            {
                x = 0;
                flag = 0;
                while (flag == 0 & x < 8)
                {
                    if (read_one(path0, current, x) == 1)
                    {
                        if (read_name(path0, current, x) == paths[k] & read_expandname(path0, current, x)=="fil")
                        {
                            three = read_add_lth(path0, current, x);
                            current = three[0];
                            flag = 1;
                        }
                        else
                            x++;
                    }
                    else
                        x++;
                }
                if (flag==1)
                    k++;
                else
                {
                    MessageBox.Show("路径错误");
                    return;
                }
            }
            for(k=0;k<8;k++)
            {
                if (read_one(path0, current, k) == 0)
                    break;
                else
                {
                    if (read_name(path0, current, k) == paths[number - 2] & read_expandname(path0, current, k) == paths[number - 1])
                    {
                        MessageBox.Show("存在同名文件");
                        return;
                    }
                }
            }
            if (k >= 8)
            {
                return;
            }
            else
            {
                FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(current * 128 + k * 16, SeekOrigin.Begin);
                fs.WriteByte(1);
                fs.Seek(current * 128 + k * 16 + 1, SeekOrigin.Begin);
                for (int ii = paths[number - 2].Length; ii < 3; ii++)
                    paths[number - 2] += ' ';
                byte[] tmp0 = new byte[3];
                tmp0 = System.Text.Encoding.Default.GetBytes(paths[number - 2]);
                fs.Write(tmp0, 0, 3);
                paths[number - 2] = paths[number - 2].Trim();
                fs.Seek(current * 128 + k * 16 + 4, SeekOrigin.Begin);
                tmp0 = System.Text.Encoding.Default.GetBytes(paths[number - 1]);
                fs.Write(tmp0, 0, 3);
                if (paths[number - 1] == "fil")
                    three[0] = apply_blank(paths[0]);
                else
                    three[0] = 0;
                three[1] = three[2] = 0;
                fs.Seek(current * 128 + k * 16 + 7, SeekOrigin.Begin);
                fs.Write(three, 0, 3);
                byte[] tt = new byte[8];
                for (int iii = 0; iii < tt.Length; iii++)
                {
                    tt[iii] = 0;
                    fs.Seek(three[0] * 128 + iii * 16, SeekOrigin.Begin);
                    fs.WriteByte(tt[iii]);
                }
                fs.Close();
            }
        }
        public void create_Node(string path)
        {
            treeView1.SelectedNode = treeView1.Nodes[0];
            string[] paths = path.Split(new char[] { '\\', '.' });
            if (paths.Length > 5 || (paths[paths.Length - 1] == "fil" & paths.Length == 5))
            {
                return;
            }
            int number = paths.Length;
            if (paths[0]=="c" || paths[0]=="C")
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[0];
            else
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[1];
            int tag = 0;
            int i = 1;
            TreeNode tmp1 = new TreeNode();
            while (i < number - 2)
            {
                int j = 0;
                tag = 0;
                while ((tag == 0) && (j < treeView1.SelectedNode.Nodes.Count))
                {
                    tmp1 = treeView1.SelectedNode;
                    if (paths[i].Equals(treeView1.SelectedNode.Nodes[j].Text) & treeView1.SelectedNode.Nodes[j].ImageIndex == 4)
                    {
                        treeView1.SelectedNode = treeView1.SelectedNode.Nodes[j];
                        tag = 1;
                    }
                    else
                        j++;
                }
                if (tag==0)
                {
                    return;
                }
                i++;
            }
            int tmp00 = 0;
            if (paths[number - 1] == "fil")
                tmp00 = 4;
            else if (paths[number - 1] == "txt")
                tmp00 = 3;
            else if (paths[number - 1] == "exe")
                tmp00 = 2;
            i = 0;
            while (i < treeView1.SelectedNode.Nodes.Count)
            {
                if (treeView1.SelectedNode.Nodes[i].Text.Equals(paths[number - 2])
                   & treeView1.SelectedNode.Nodes[i].ImageIndex == tmp00)
                    return;
                i++;
            }
            TreeNode tmp;
            tmp = new TreeNode(paths[number - 2]);
            if (paths[number - 1] == "fil")
            {
                tmp.ImageIndex = 4;
                tmp.SelectedImageIndex = 4;
            }
            else if (paths[number - 1] == "txt")
            {
                tmp.ImageIndex = 3;
                tmp.SelectedImageIndex = 3;
            }
            else if (paths[number - 1] == "exe")
            {
                tmp.ImageIndex = 2;
                tmp.SelectedImageIndex = 2;
            }
            treeView1.SelectedNode.Nodes.Add(tmp);
            treeView1.ExpandAll();
        }//------------------------------------添加文件，无物理地址则为0，无内容长度为0

        public void delete_file(string path0,string path00,byte current)
        {
            int x = 0;
            byte[] three = new byte[3];
            for (x = 0; x < 8; x++)
            {
                if (read_one(path0, current, x) == 1)
                    if (read_name(path0, current, x) != "fil")
                    {
                        FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current * 128 + x * 16, SeekOrigin.Begin);
                        fs.WriteByte(0);
                        fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                        fs.Read(three, 0,3);
                        if (three[0] != 0)
                            if (three[2] == 0)
                            {
                                return_blank(path00, three[0]);
                            }
                            else
                            {
                                fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                byte[] tmp01 = new byte[three[1]];
                                fs.Read(tmp01, 0, three[1]);
                                for (int ii = 0; ii < three[1]; ii++)
                                    return_blank(path00, tmp01[ii]);
                                return_blank(path00, three[0]);
                            }
                        fs.Close();
                        break;
                    }
                    else
                    {
                        FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                        fs.Read(three, 0, 3);
                        fs.Close();
                        delete_file(path0, path00, three[0]);
                        fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current * 128 + x * 16, SeekOrigin.Begin);
                        fs.WriteByte(0);
                        return_blank(path00, three[0]);
                        fs.Close();
                    }
            }
        }
        public void delete_File(string path)
        {
            string[] paths = path.Split(new char[] { '\\', '.' });
            int number = paths.Length;
            int flag = 0, k = 1, x = 0;
            byte current = 1;
            byte[] three = new byte[3];
            string path0;
            if (path.StartsWith("c") || path.StartsWith("C"))
                path0 = path1;
            else
                path0 = path2;
            while (k < number - 2)
            {
                x = 0;
                flag = 0;
                while (flag == 0 & x < 8)
                {
                    if (read_one(path0, current, x) == 1)
                    {
                        if (read_name(path0, current, x) == paths[k] & read_expandname(path0, current, x) == "fil")
                        {
                            three = read_add_lth(path0, current, x);
                            current = three[0];
                            flag = 1;
                        }
                        else
                            x++;
                    }
                    else
                        x++;
                }
                if (flag == 1)
                    k++;
                else
                {
                    return;
                }
            }
            for (x = 0; x < 8; x++)
            {
                if (read_one(path0, current, x) == 1)
                    if (read_name(path0, current, x) == paths[number - 2] & read_expandname(path0, current, x) == paths[number - 1])
                    {
                        if (paths[number - 1] != "fil")
                        {
                            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                            fs.Seek(current * 128 + x * 16, SeekOrigin.Begin);
                            fs.WriteByte(0);
                            fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                            fs.Read(three, 0, 3);
                            if (three[0]!=0)
                                if (three[2] == 0)
                                {
                                    return_blank(paths[0], three[0]);
                                }
                                else
                                {
                                    fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                    byte[] tmp01 = new byte[three[1]];
                                    fs.Read(tmp01, 0, three[1]);
                                    for (int ii = 0; ii < three[1]; ii++)
                                        return_blank(paths[0], tmp01[ii]);
                                    return_blank(paths[0], three[0]);
                                }                                
                            fs.Close();
                            break;
                        }
                        else
                        {
                            FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                            fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                            fs.Read(three, 0, 3);
                            fs.Close();
                            delete_file(path0, paths[0], three[0]);
                            fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                            fs.Seek(current * 128 + x * 16, SeekOrigin.Begin);
                            fs.WriteByte(0);
                            return_blank(paths[0], three[0]);
                            fs.Close();
                        }
                    }
            }
        }
        public void delete_node()
        {
            TreeNode t;
            for (int i = 0; i < treeView1.SelectedNode.Nodes.Count; i++)
            {
                if (treeView1.SelectedNode.Nodes[i].Nodes.Count == 0)
                {
                    treeView1.SelectedNode.Nodes[i].Remove();
                    i--;
                }
                else
                {
                    t = treeView1.SelectedNode;
                    treeView1.SelectedNode = treeView1.SelectedNode.Nodes[i];
                    delete_node();
                    treeView1.SelectedNode = t;
                    i--;
                }
            }
            treeView1.SelectedNode.Remove();
        }
        public void delete_Node(string path)//-------------------------------------删除文件
        {
            treeView1.SelectedNode = treeView1.Nodes[0];
            string[] paths = path.Split(new char[] { '\\', '.' });
            int number = paths.Length;
            if (paths.Length > 5 || (paths[paths.Length - 1] == "fil" & paths.Length == 5))
            {
                MessageBox.Show("目录太长");
                return;
            }
            if (path.StartsWith("c") || path.StartsWith("C"))
            {
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[0];
            }
            else
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[1];
            int tag = 0;
            int i = 1,j=0;
            int index = 0;
            if (paths[number - 1] == "fil")
                index = 4;
            else if (paths[number - 1] == "txt")
                index = 3;
            else if (paths[number - 1] == "exe")
                index = 2;
            j = 0;
            while (i < number - 2)
            {
                j = 0;
                tag = 0;
                while ((tag == 0) && (j < treeView1.SelectedNode.Nodes.Count))
                {
                    if (paths[i].Equals(treeView1.SelectedNode.Nodes[j].Text)
                         & treeView1.SelectedNode.Nodes[j].ImageIndex == 4)
                    {
                        treeView1.SelectedNode = treeView1.SelectedNode.Nodes[j];
                        tag = 1;
                        break;
                    }
                    else
                        j++;
                }
                if (tag == 1)
                    i++;
                else
                {
                    MessageBox.Show("路径错误！");
                    return;
                }
            }
            j=0;
            while (j < treeView1.SelectedNode.Nodes.Count)
            {
                if (treeView1.SelectedNode.Nodes[j].Text == paths[number - 2]
                    & treeView1.SelectedNode.Nodes[j].ImageIndex == index)
                {
                    string name = paths[number - 2];
                    if (name == "fil")
                        delete_node();
                    else
                    {
                        treeView1.SelectedNode.Nodes[j].Remove();
                        return;
                    }
                }
                j++;
            }
            if (j == treeView1.SelectedNode.Nodes.Count)
                MessageBox.Show("没有此文件");
        }

        public void edit_file(string path)
        {
            string contents=textBox2.Text ;
            textBox2.Text ="";
            string[] paths = path.Split(new char[] { '\\', '.' });
            string path0;
            if (paths[0].StartsWith( "c") || paths[0].StartsWith ("C"))
                path0 = path1;
            else
                path0 = path2;
            int number = paths.Length;
            if (paths[number-1]=="fil"& paths[number-1]!="txt" & paths[number-1]!="exe")
            {
                return;
            }
            byte current = 1;
            byte[] three = new byte[3];
            int k=1, x, flag;
            while (k < number - 2)
            {
                x = 0;
                flag = 0;
                while (flag == 0 & x < 8)
                {
                    if (read_one(path0, current, x) == 1)
                    {
                        if (read_name(path0, current, x) == paths[k] & read_expandname(path0, current, x) == "fil")
                        {
                            three = read_add_lth(path0, current, x);
                            current = three[0];
                            flag = 1;
                        }
                        else
                            x++;
                    }
                    else
                        x++;
                }
                if (flag == 1)
                    k++;
                else
                {
                    return;
                }
            }
            for (x = 0; x < 8; x++)
            {
                if (read_one(path0, current, x) == 1)
                    if (read_name(path0, current, x) == paths[number - 2] & read_expandname(path0, current, x) == paths[number - 1])
                    {
                        FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                        fs.Read (three, 0, 3);
                        if (three[0] == 0)
                        {
                            if (contents =="")
                            {
                                fs.Close();
                                return;
                            }
                            three[0] = apply_blank(paths[0]);
                            if (contents.Length <= 128)
                            {
                                three[1] = Convert.ToByte(contents.Length);
                                byte[] tmp00 = System.Text.Encoding.Default.GetBytes(contents);
                                three[2] = 0;
                                fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                                fs.Write(three, 0, 3);
                                fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                fs.Write(tmp00, 0, tmp00.Length);
                            }
                            else
                            {
                                three[1] = Convert.ToByte(contents.Length / 128);
                                if (contents.Length % 128 == 0)
                                    three[2] = 128;
                                else
                                {
                                    three[2] = Convert.ToByte(contents.Length % 128);
                                    three[1] = Convert.ToByte(three[1] + 1);
                                }
                                if (paths[0].StartsWith("c") & paths[0].StartsWith("C"))
                                {
                                    if (three[2] > (128 - disk_countc))
                                    {
                                        fs.Close();
                                        return;
                                    }
                                }
                                else
                                {
                                    if (three[2] > (128 - disk_countd))
                                    {
                                        fs.Close();
                                        return;
                                    }
                                }
                                fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                                fs.Write(three, 0, 3);
                                byte[] one_address = new byte[three[1]];
                                for (int ii = 0; ii < three[1]; ii++)
                                    one_address[ii] = apply_blank(paths[0]);
                                fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                fs.Write(one_address, 0, three[1]);
                                string cont;
                                for (int ii = 0; ii < three[1]; ii++)
                                    if (ii < three[1] - 1)
                                    {
                                        cont = contents.Substring(ii * 128, 128);
                                        byte[] tmp11 = System.Text.Encoding.Default.GetBytes(cont);
                                        fs.Seek(one_address[ii] * 128, SeekOrigin.Begin);
                                        fs.Write(tmp11, 0, 128);
                                    }
                                    else if (ii == three[1] - 1)
                                    {
                                        cont = contents.Substring(ii * 128, three[2]);
                                        byte[] tmp12 = System.Text.Encoding.Default.GetBytes(cont);
                                        fs.Seek(one_address[ii] * 128, SeekOrigin.Begin);
                                        fs.Write(tmp12, 0, three[2]);
                                    }
                            }
                        }
                        else
                        {
                            if (contents =="")
                            {
                                if (three[2] == 0)
                                {
                                    return_blank(paths[0], three[0]);
                                }
                                else
                                {
                                    byte[] tmp02 = new byte[three[1]];
                                    fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                    fs.Read(tmp02, 0, three[1]);
                                    for (int ia = 0; ia < three[1]; ia++)
                                        return_blank(paths[0], tmp02[ia]);
                                    return_blank(paths[0], three[0]);
                                }
                                three[0] = three[1] = three[2] = 0;
                                fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                                fs.Write(three, 0, 3);
                                fs.Close();
                                return;
                            }
                            string cont = "";
                            if (three[2] == 0)
                            {
                                byte[] tmp00 = new byte[three[1]];
                                fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                fs.Read(tmp00, 0, three[1]);
                                cont = System.Text.Encoding.Default.GetString(tmp00);
                            }
                            else
                            {
                                byte[] tmp01 = new byte[three[1]];
                                fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                fs.Read(tmp01, 0, three[1]);
                                for (int ii = 0; ii < three[1]; ii++)
                                    if (ii < three[1] - 1)
                                    {
                                        byte[] content = new byte[128];
                                        fs.Seek(tmp01[ii] * 128, SeekOrigin.Begin);
                                        fs.Read(content, 0, 128);
                                        cont += System.Text.Encoding.Default.GetString(content);
                                    }
                                    else if (ii == three[1] - 1)
                                    {
                                        byte[] content = new byte[three[2]];
                                        fs.Seek(tmp01[ii] * 128, SeekOrigin.Begin);
                                        fs.Read(content, 0, three[2]);
                                        cont += System.Text.Encoding.Default.GetString(content);
                                    }
                            }
                            if (contents == cont)
                            {
                                return;
                            }
                            else
                            {
                                if (three[2] == 0)
                                {
                                    return_blank(paths[0], three[0]);
                                }
                                else
                                {
                                    byte[] tmp02 = new byte[three[1]];
                                    fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                    fs.Read(tmp02, 0, three[1]);
                                    for (int ia = 0; ia < three[1]; ia++)
                                        return_blank(paths[0], tmp02[ia]);
                                    return_blank(paths[0], three[0]);
                                }
                                three[0] = apply_blank(paths[0]);
                                if (contents.Length <= 128)
                                {
                                    three[1] = Convert.ToByte(contents.Length);
                                    byte[] tmp00 = System.Text.Encoding.Default.GetBytes(contents);
                                    three[2] = 0;
                                    fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                                    fs.Write(three, 0, 3);
                                    fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                    fs.Write(tmp00, 0, tmp00.Length);
                                }
                                else
                                {
                                    three[1] = Convert.ToByte(contents.Length / 128);
                                    if (contents.Length % 128 == 0)
                                        three[2] = 128;
                                    else
                                    {
                                        three[2] = Convert.ToByte(contents.Length % 128);
                                        three[1] = Convert.ToByte(three[1] + 1);
                                    }
                                    fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                                    fs.Write(three, 0, 3);
                                    byte[] one_address = new byte[three[1]];
                                    for (int ii = 0; ii < three[1]; ii++)
                                        one_address[ii] = apply_blank(paths[0]);
                                    fs.Seek(three[0] * 128, SeekOrigin.Begin);
                                    fs.Write(one_address, 0, three[1]);
                                    for (int ii = 0; ii < three[1]; ii++)
                                        if (ii < three[1] - 1)
                                        {
                                            cont = contents.Substring(ii * 128, 128);
                                            byte[] tmp11 = System.Text.Encoding.Default.GetBytes(cont);
                                            fs.Seek(one_address[ii] * 128, SeekOrigin.Begin);
                                            fs.Write(tmp11, 0, 128);
                                        }
                                        else if (ii == three[1] - 1)
                                        {
                                            cont = contents.Substring(ii * 128, three[2]);
                                            byte[] tmp12 = System.Text.Encoding.Default.GetBytes(cont);
                                            fs.Seek(one_address[ii] * 128, SeekOrigin.Begin);
                                            fs.Write(tmp12, 0, three[2]);
                                        }
                                }
                            }
                        }//判断文件有无
                        fs.Close();
                        break;
                    }
            }//查找文件的for循环结束
        }//----------------------------------------编辑文件

        public void type_file(string path)//----------------------------------------显示文件内容
        {
            textBox2.Enabled = true;
            string[] paths = path.Split(new char[] { '\\', '.' });
            string path0;
            if (paths[0].StartsWith("c")  ||  paths[0].StartsWith("C"))
                path0 = path1;
            else
                path0 = path2;
            int number = paths.Length;
            if (paths[number-1]=="fil")
            {
                MessageBox.Show("不可显示");
                textBox2.Enabled = false;
                return;
            }
            byte current = 1;
            byte[] three= new byte[3];
            int k=1, x, flag;
            while (k < number - 2)
            {
                x = 0;
                flag = 0;
                while (flag == 0 & x < 8)
                {
                    if (read_one(path0, current, x) == 1)
                    {
                        if (read_name(path0, current, x) == paths[k] & read_expandname(path0, current, x) == "fil")
                        {
                            three = read_add_lth(path0, current, x);
                            current = three[0];
                            flag = 1;
                        }
                        else
                            x++;
                    }
                    else
                        x++;
                }
                if (flag == 1)
                    k++;
                else
                {
                    MessageBox.Show("路径错误");
                    textBox2.Enabled = false;
                    return;
                }
            }
            for (x = 0; x < 8; x++)
            {
                if (read_one(path0, current, x) == 1)
                    if (read_name(path0, current, x) == paths[number - 2] & read_expandname(path0, current, x) == paths[number - 1])
                    {
                        FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current* 128 + x * 16 + 7, SeekOrigin.Begin);
                        fs.Read(three, 0, 3);
                        if (three[2] == 0)
                        {
                            byte[] tmp00 = new byte[three[1]];
                            fs.Seek(three[0] * 128, SeekOrigin.Begin);
                            fs.Read(tmp00, 0, three[1]);
                            textBox2.Text = System.Text.Encoding.Default.GetString(tmp00);
                            fs.Close();
                        }
                        else
                        {
                            byte[] tmp01=new byte[three[1]];   
                            fs.Seek(three[0] * 128, SeekOrigin.Begin);
                            fs.Read(tmp01, 0, three[1]);
                            string cont="";
                            for (int ii = 0; ii <three[1]; ii++)
                                if (ii < three[1] - 1)
                                {
                                    byte[] content=new byte[128];
                                    fs.Seek(tmp01[ii] * 128, SeekOrigin.Begin);
                                    fs.Read(content, 0, 128);
                                    cont += System.Text.Encoding.Default.GetString(content);
                                }
                                else if(ii==three[1]-1)
                                {
                                    byte[] content = new byte[three[2]];
                                    fs.Seek(tmp01[ii] * 128, SeekOrigin.Begin);
                                    fs.Read(content, 0, three[2]);
                                    cont += System.Text.Encoding.Default.GetString(content);
                                }
                            textBox2.Text = cont;
                            fs.Close();
                        }
                    }
            }
        }

        public void format(string path)
        {
            string path0;
            if (path.StartsWith("c") || path.StartsWith("C"))
                path0 = path1;
            else
                path0 = path2;
            byte[] one = new byte[16384];
            for (int i = 0; i < one.Length; i++)
                one[i] = 0;
            FileStream file1 = new FileStream(path0, FileMode.Open,FileAccess.Write);
            file1.Seek(0, SeekOrigin.Begin);
            file1.Write(one, 0, one.Length);
            file1.Close();
            if (path.StartsWith("c") || path.StartsWith("C"))
            {
                for (int i = 2; i < Bitmapc.Length; i++)
                    if (Bitmapc[i] != 0)
                    {
                        return_blank(path, Convert.ToByte(i));
                        Bitmapc[i] = 0;
                    }
                for (int i = 0; i < c_lable.GetLength(0); i++)
                    for (int j = 0; j < c_lable.GetLength(1); j++)
                    {
                        c_lable[i, j].BackColor = Color.White;
                    }
                c_lable[0, 0].BackColor = Color.Red;
                c_lable[1, 0].BackColor = Color.Red;
                treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0];
                TreeNode t;
                for (int i = 0; i < treeView1.SelectedNode.Nodes.Count; i++)
                {
                    if (treeView1.SelectedNode.Nodes[i].Nodes.Count == 0)
                    {
                        treeView1.SelectedNode.Nodes[i].Remove();
                        i--;
                    }
                    else
                    {
                        t = treeView1.SelectedNode;
                        treeView1.SelectedNode = treeView1.SelectedNode.Nodes[i];
                        delete_node();
                        treeView1.SelectedNode = t;
                        i--;
                    }
                }
            }
            else
            {
                for (int i = 2; i < Bitmapd.Length; i++)
                    if (Bitmapd[i] != 0)
                    {
                        return_blank(path, Convert.ToByte(i));
                        Bitmapd[i] = 0;
                    }
                for (int i = 0; i < d_lable.GetLength(0); i++)
                    for (int j = 0; j < d_lable.GetLength(1); j++)
                    {
                        d_lable[i, j].BackColor = Color.White;
                    }
                d_lable[0, 0].BackColor = Color.Red;
                d_lable[1, 0].BackColor = Color.Red;
                treeView1.SelectedNode = treeView1.Nodes[0].Nodes[1];
                TreeNode t;
                for (int i = 0; i < treeView1.SelectedNode.Nodes.Count; i++)
                {
                    if (treeView1.SelectedNode.Nodes[i].Nodes.Count == 0)
                    {
                        treeView1.SelectedNode.Nodes[i].Remove();
                        i--;
                    }
                    else
                    {
                        t = treeView1.SelectedNode;
                        treeView1.SelectedNode = treeView1.SelectedNode.Nodes[i];
                        delete_node();
                        treeView1.SelectedNode = t;
                        i--;
                    }
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode ==Keys.Enter )
            {
                string command = textBox1.Text;
                textBox1.Text = "";
                command = command.Trim();
                string[] array = command.Split(' ');
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[1]!=string.Empty)
                    {
                        break;
                    }
                    if (array[i] == "")
                        continue;
                    else
                    {
                        array[1] = array[i];
                        break;
                    }
                }
                for (int i = 2; i < array.Length; i++)
                {
                    if (array[2] != string.Empty)
                    {
                        break;
                    }
                    if (array[i] == "")
                        continue;
                    else
                    {
                        array[2] = array[i];
                        break;
                    }
                }
                string[] paths=array[1].Split(new char[]{'\\','.'});
                if (!(array[1].StartsWith("c") || array[1].StartsWith("C") || array[1].StartsWith("d") || array[1].StartsWith("D")))
                {
                    MessageBox.Show("只支持绝对路径");
                    return;
                }
                if (array.Length < 1)
                    MessageBox.Show("请输入正确的指令！");
                else
                {
                    if (array[0] == "create")
                    {
                        if (!(paths[paths.Length-1].Equals("exe") || paths[paths.Length-1].Equals("txt")))
                        {
                            MessageBox.Show("请输入正确的带有扩展名的指令");
                            return;
                        }
                        add_file(array[1]);
                        create_Node(array[1]);
                    }
                    else if (array[0] == "delete")
                    {
                        if (!(paths[paths.Length - 1].Equals("exe") || paths[paths.Length - 1].Equals("txt")))
                        {
                            MessageBox.Show("请输入正确的带有扩展名的指令");
                            return;
                        }
                        delete_Node(array[1]);
                        delete_File(array[1]);
                    }
                    else if (array[0] == "edit")
                    {
                        if (!(paths[paths.Length - 1].Equals("exe") || paths[paths.Length - 1].Equals("txt")))
                        {
                            MessageBox.Show("请输入正确的带有扩展名的指令");
                            return;
                        }
                        button2.Enabled = true;
                        textBox2.Enabled = true;
                        road = array[1];
                        type_file(array[1]);
                    }
                    else if (array[0] == "type")
                    {
                        if (!(paths[paths.Length - 1].Equals("exe") || paths[paths.Length - 1].Equals("txt")))
                        {
                            MessageBox.Show("请输入正确的带有扩展名的指令");
                            return;
                        }
                        type_file(array[1]);
                    }
                    else if (array[0] == "makdir")
                    {
                        if (paths[paths.Length - 1].Equals("exe") ||
                            paths[paths.Length - 1].Equals("txt") || 
                            paths[paths.Length-1].Equals("fil"))
                        {
                            MessageBox.Show("创建目录不需要扩展名");
                            return;
                        }
                        array[1] += ".fil";
                        create_Node(array[1]);
                        add_file(array[1]);
                    }
                    else if (array[0] == "deldir")
                    {
                        if (paths[paths.Length - 1].Equals("exe")
                            ||paths[paths.Length - 1].Equals("txt") 
                            ||paths[paths.Length - 1].Equals("fil"))
                        {
                            MessageBox.Show("创建目录不需要扩展名");
                            return;
                        }
                        array[1] += ".fil";
                        delete_Node(array[1]);
                        delete_File(array[1]);
                    }
                    else if (array[0] == "format")
                    {
                        format(array[1]);
                    }
                    else
                        MessageBox.Show("请输入正确的指令，不支持模糊指令！");
                }
            }
        }//----------文件编辑命令接口
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox2.Enabled = false;
            button2.Enabled = false;
        }//----------------清除textbox2内容
        private void button2_Click(object sender, EventArgs e)
        {
            edit_file(road);
            textBox2.Enabled = false;
            button2.Enabled = false;
        }

        public void initiate_PCB()//--------------------------------------------------初始化PCB
        {
            for (int i = 0; i < PCB_ready.Length ; i++)
            {
                PCB_ready[i].index = 0;
                PCB_ready[i].PC = 0;
                PCB_ready[i].result = 0;
                PCB_ready[i].start = 4;
                PCB_ready[i].time = 0;
                PCB_ready[i].instructment = '\\';

                PCB_block[i].index = 0;
                PCB_block[i].PC = 0;                
                PCB_block[i].result = 0;                
                PCB_block[i].start = 4;
                PCB_block[i].time = 0;
                PCB_block[i].instructment = '\\';

                PCB_wait[i].index = 0;
                PCB_wait[i].PC = 0;
                PCB_wait[i].result = 0;
                PCB_wait[i].start = 4;
                PCB_wait[i].time = 0;
                PCB_wait[i].instructment = '\\';

            }
            for (int i = 0; i < instructment.Length; i++)
            {
                instructment[i].state = 0;
                instructment[i].index = 0;
            }
            count_process = 0;
            count_processes = 0;
            time = 0;
        }
        public void initiate_mainmomory()
        {
            for (int i = 0; i < main_memory.Length ; i++)
            {
                main_memory[i] = new Label();
                main_memory[i].Height = 3;
                main_memory[i].Width = 78;
                main_memory[i].Location = new Point(8, 16 + i * 6);
                main_memory[i].BackColor = Color.GreenYellow;
                groupBox8.Controls.Add(main_memory[i]);
            }
            main_memory[0].BackColor = Color.Black;
            main_memory[1].BackColor = Color.Black;
            main_memory[2].BackColor = Color.Black;
            main_memory[3].BackColor = Color.Black;
        }//-------------------------------------初始化内存
        public void initiate_instructment()//----------------------------------------初始化设备
        {
            for (int i = 0; i < instructment.Length; i++)
            {
                instructment[i].index = -1;
                instructment[i].state = 0;
                instructment[i].time = 0;
                if (i < 3)
                {
                    instructment[i].name = 'A';
                }
                else if (i >= 3 & i <5)
                {
                    instructment[i].name = 'B';
                }
                else
                    instructment[i].name = 'C';
            }
            update_instructment();
        }

        public void apply_instructment(char tmp)
        {
            int i;
            for (i = 0; i < instructment.Length; i++)
                if (instructment[i].state == 0 & instructment[i].name == tmp)
                {
                    instructment[i].state = 1;
                    instructment[i].index = PCB_ready[0].index;
                    instructment[i].time = PCB_ready[0].time;
                    break;
                }
            //update_instructment();
            if (i < instructment.Length)
                block_process();
            else
                block_wait_process();
            //update_instructment();
        }//------------------------------申请设备
        public void update_instructment()
        {
            if (instructment[0].state == 0)
            {
                textBox8.Text = "空闲";
                textBox14.Text = "";
                
            }
            else
            {
                textBox8.Text = "占用";
                textBox14.Text = "进程" + instructment[0].index;
                if (instructment[0].time != 0)
                    textBox26.Text = Convert.ToString(instructment[0].time);
                else
                    textBox26.Text = "";
            }
            if (instructment[1].state == 0)
            {
                textBox9.Text = "空闲";
                textBox15.Text = "";
            }
            else
            {
                textBox9.Text = "占用";
                textBox15.Text = "进程" + instructment[1].index;
                if (instructment[1].time != 0)
                    textBox27.Text = Convert.ToString(instructment[1].time);
                else
                    textBox27.Text = "";
            }
            if (instructment[2].state == 0)
            {
                textBox10.Text = "空闲";
                textBox16.Text = "";
            }
            else
            {
                textBox10.Text = "占用";
                textBox16.Text = "进程" + instructment[2].index;
                if (instructment[2].time != 0)
                    textBox28.Text = Convert.ToString(instructment[2].time);
                else
                    textBox28.Text = "";
            }
            if (instructment[3].state == 0)
            {
                textBox11.Text = "空闲";
                textBox17.Text = "";
            }
            else
            {
                textBox11.Text = "占用";
                textBox17.Text = "进程" + instructment[3].index;
                if (instructment[3].time != 0)
                    textBox29.Text = Convert.ToString(instructment[3].time);
                else
                    textBox29.Text = "";
            }
            if (instructment[4].state == 0)
            {
                textBox12.Text = "空闲";
                textBox18.Text = "";
            }
            else
            {
                textBox12.Text = "占用";
                textBox18.Text = "进程" + instructment[4].index;
                if (instructment[4].time != 0)
                    textBox30.Text = Convert.ToString(instructment[4].time);
                else
                    textBox30.Text = "";
            }
            if (instructment[5].state == 0)
            {
                textBox13.Text = "空闲";
                textBox19.Text = "";
            }
            else
            {
                textBox13.Text = "占用";
                textBox19.Text = "进程" + instructment[5].index;
                if (instructment[5].time != 0)
                    textBox31.Text = Convert.ToString(instructment[5].time);
                else
                    textBox31.Text = "";
            }
        }//---------------------------------------更新设备使用情况
        public void return_instructment(char tmp,int index)
        {
            int i;
            for (i = 0; i <instructment.Length; i++)
                if (instructment[i].state ==1)
                    if (instructment[i].index ==index & instructment[i].name ==tmp)
                    {
                        instructment[i].index = 0;
                        instructment[i].state = 0;
                        //update_instructment();
                        break;
                    }
            for (int j = 0; j <PCB_wait.Length; j++)
                if (PCB_wait[j].index !=0)
                    if (PCB_wait[j].instructment ==tmp)
                    {
                        instructment[i].index = PCB_wait[j].index;
                        instructment[i].state = 1;
                        wake_wait_process(j); 
                        //update_instructment();
                        break;
                    }           
        }//------------------归还设备

        public void block_process()
        {
            int i;
            for (i = 0; i < PCB_block.Length; i++)
                if (PCB_block[i].index == 0)
                    break;
            PCB_block[i].index = PCB_ready[0].index;
            PCB_block[i].instructment = PCB_ready[0].instructment;
            PCB_block[i].PC = PCB_ready[0].PC;
            PCB_block[i].result = PCB_ready[0].result;
            PCB_block[i].start = PCB_ready[0].start; 
            PCB_block[i].command = PCB_ready[0].command;
            PCB_block[i].time = PCB_ready[0].time;
        }//---------------------------------------------阻塞进程
        public void block_wait_process()//----------------------------------------等待设备阻塞
        {
            int i;
            for (i = 0; i < PCB_wait.Length; i++)
                if (PCB_wait[i].index == 0)
                    break;
            PCB_wait[i].index = PCB_ready[0].index;
            PCB_wait[i].instructment = PCB_ready[0].instructment;
            PCB_wait[i].PC = PCB_ready[0].PC;
            PCB_wait[i].result = PCB_ready[0].result;
            PCB_wait[i].start = PCB_ready[0].start;
            PCB_wait[i].command = PCB_ready[0].command;
            PCB_wait[i].time = PCB_ready[0].time; 
        }
        public void wake_process(int  i)
        {
            int j;
            for (j = 0; j < PCB_ready.Length; j++)
                if (PCB_ready[j].index == 0)
                    break;
            PCB_ready[j].index =PCB_block[i].index;
            PCB_ready[j].instructment= PCB_block[i].instructment;
            PCB_ready[j].PC =PCB_block[i].PC ;
            PCB_ready[j].result =PCB_block[i].result;
            PCB_ready[j].start = PCB_block[i].start; 
            PCB_ready[j].command=PCB_block[i].command ;
            PCB_ready[j].time = PCB_block[i].time;
            PCB_block[i].index = 0;
        }//----------------------------------------唤醒进程
        public void wake_wait_process(int i)
        {
            int j;
            for (j = 0; j < PCB_block.Length; j++)
                if (PCB_block[j].index == 0)
                    break;
            PCB_block[j].index = PCB_wait[i].index;
            PCB_block[j].instructment = PCB_wait[i].instructment;
            PCB_block[j].PC = PCB_wait[i].PC;
            PCB_block[j].result = PCB_wait[i].result;
            PCB_block[j].start = PCB_wait[i].start;
            PCB_block[j].command = PCB_wait[i].command;
            PCB_block[j].time = PCB_wait[i].time;
            PCB_wait[i].index = 0;
        }//-----------------------------------唤醒等待设备的进程
        public void cope_block()
        {
            for (int i = 0; i < instructment.Length; i++)
                if (instructment[i].index != -1)
                {
                    instructment[i].time--;
                    update_instructment();
                }
            for (int i = 0; i <PCB_block.Length; i++)
                if (PCB_block[i].index!=0)
                {
                    PCB_block[i].time--;
                    if (PCB_block[i].time == 0)
                    {
                        return_instructment(PCB_block[i].instructment, PCB_block[i].index);
                        wake_process(i);
                        updata_process(0);
                        update_instructment();
                    }
                }
        }

        public int apply_memory(int number)
        {
            int flag=1,i,ii;
            for (i = 4; i <= 63 - (number - 1); i++)
            {
                flag = 1;
                for (ii = i; ii <i+number; ii++)
                    if (main_memory[ii].BackColor != Color.GreenYellow)
                        flag = 0;
                if (flag == 1)
                {
                    break;
                }
                else
                    continue;
            }
            if (flag == 1)
            {
                return i;
            }
            else
                return 0;
        }
        public void create_process(string path)
        {
            string[] paths = path.Split(new char[] { '\\', '.' });
            string path0;
            if (paths[0] == "c" || paths[0] == "C")
                path0 = path1;
            else
                path0 = path2;
            int number = paths.Length;
            if (paths[number - 1] != "exe")
            {
                MessageBox.Show("非可执行文件");
                return;
            }
            byte current = 1;
            byte[] three = new byte[3];
            int k = 1, x, flag,main_counter=4;
            while (k < number - 2)
            {
                x = 0;
                flag = 0;
                while (flag == 0 & x < 8)
                {
                    if (read_one(path0, current, x) == 1)
                    {
                        if (read_name(path0, current, x) == paths[k] & read_expandname(path0, current, x) == "fil")
                        {
                            three = read_add_lth(path0, current, x);
                            current = three[0];
                            flag = 1;
                        }
                        else
                            x++;
                    }
                    else
                        x++;
                }
                if (flag == 1)
                    k++;
                else
                {
                    MessageBox.Show("路径错误");
                    return;
                }
            }
            for (x = 0; x < 8; x++)
            {
                if (read_one(path0, current, x) == 1)
                    if (read_name(path0, current, x) == paths[number - 2] & read_expandname(path0, current, x) == paths[number - 1])
                    {
                        FileStream fs = new FileStream(path0, FileMode.Open, FileAccess.ReadWrite);
                        fs.Seek(current * 128 + x * 16 + 7, SeekOrigin.Begin);
                        fs.Read(three, 0, 3);
                        if (three[0] != 0)
                        {
                            fs.Seek(three[0] * 128, SeekOrigin.Begin);
                            byte[] tmp00 = new byte[three[1]];
                            fs.Read(tmp00, 0, three[1]);
                            fs.Close();
                            int ii;
                            for (ii = 0; ii < PCB_ready.Length; ii++)
                                if (PCB_ready[ii].index == 0)
                                    break;
                            count_process++;
                            count_processes++;
                            PCB_ready[ii].command = System.Text.Encoding.Default.GetString(tmp00);
                            main_counter = apply_memory(PCB_ready[ii].command.Length / 4);
                            if (main_counter ==0)
                            {
                                MessageBox.Show("内存已满");
                                return;
                            }
                            PCB_ready[ii].index = count_processes;
                            PCB_ready[ii].PC = 0;
                            PCB_ready[ii].start = main_counter;
                            updata_process(0);
                            for (int i = PCB_ready[ii].start; i <main_counter+PCB_ready[ii].command.Length/4; i++)
                                main_memory[i].BackColor = Color.Blue;
                            return;
                        }
                        else
                        {
                            timer2.Enabled = false;
                            MessageBox.Show("创建失败，没有执行内容");
                            fs.Close();
                            timer2.Enabled = true;
                            return;
                        }
                    }
            }
        }//-------------------------------------------创建进程
        public void excute_process()
        {
            updata_process(1);
            update_instructment();
            textBox7.Enabled = false;
            textBox20.Text = "";
            textBox21.Text = "";
            textBox24.Text = "";
            if (PCB_ready[0].index != 0)
            {
                textBox20.Text = "进程" + PCB_ready[0].index;
                PCB_ready[0].PC++;
                string command = PCB_ready[0].command.Substring((PCB_ready[0].PC - 1) * 4, 4);
                textBox21.Text = command;
                if (command[1] == '=')
                {
                    PCB_ready[0].result = Convert.ToInt32(command[2]) - Convert.ToInt32('0');
                    textBox24.Text = "";
                    textBox24.Text += PCB_ready[0].result;
                }
                else if (command[1] == '+')
                {
                    PCB_ready[0].result++;
                    textBox24.Text = "";
                    textBox24.Text += PCB_ready[0].result;
                }
                else if (command[1] == '-')
                {
                    PCB_ready[0].result--;
                    textBox24.Text = "";
                    textBox24.Text += PCB_ready[0].result;
                }
                else if (command[1] == 'A' || command[1] == 'B' || command[1] == 'C' || command[1] == 'a' || command[1] == 'b' || command[1] == 'c')
                {
                    textBox24.Text = "";
                    textBox24.Text = "申请设备";
                    PCB_ready[0].instructment = command[1];
                    PCB_ready[0].time = Convert.ToInt32(command[2]) - Convert.ToInt32('0')+1;
                    apply_instructment(command[1]);
                    PCB_ready[0].index = 0;
                }
                else if (command[1] == 'n')
                {
                    count_process--;
                    for (int i = PCB_ready[0].start; i < PCB_ready[0].start + PCB_ready[0].command.Length / 4; i++)
                        main_memory[i].BackColor = Color.GreenYellow;
                    PCB_ready[0].index = 0;
                }
                if (PCB_ready[0].index == 0)
                {
                    int t;
                    for (t = 1; t < PCB_ready.Length; t++)
                        if (PCB_ready[t].index == 0)
                            break;
                    if (t<PCB_ready.Length)
                    {
                        for (int i = 0; i < t; i++)
                        {
                            PCB_ready[i].index = PCB_ready[i + 1].index;
                            PCB_ready[i].command = PCB_ready[i + 1].command;
                            PCB_ready[i].instructment = PCB_ready[i + 1].instructment;
                            PCB_ready[i].start = PCB_ready[i + 1].start;
                            PCB_ready[i].PC = PCB_ready[i + 1].PC;
                            PCB_ready[i].result = PCB_ready[i + 1].result;
                        }
                    }
                    else if (t==PCB_ready.Length)
                    {
                        for (int i = 0; i < PCB_ready.Length-1; i++)
                        {
                            PCB_ready[i].index = PCB_ready[i + 1].index;
                            PCB_ready[i].command = PCB_ready[i + 1].command;
                            PCB_ready[i].instructment = PCB_ready[i + 1].instructment;
                            PCB_ready[i].start = PCB_ready[i + 1].start;
                            PCB_ready[i].PC = PCB_ready[i + 1].PC;
                            PCB_ready[i].result = PCB_ready[i + 1].result;
                        }
                        PCB_ready[PCB_ready.Length - 1].index = 0;
                    }
                }
                else
                {
                    int t;
                    for (t = 1; t < PCB_ready.Length; t++)
                        if (PCB_ready[t].index == 0)
                            break;
                    if (t<PCB_ready.Length)
                    {
                        PCB_ready[t].index = PCB_ready[0].index;
                        PCB_ready[t].command = PCB_ready[0].command;
                        PCB_ready[t].instructment = PCB_ready[0].instructment;
                        PCB_ready[t].PC = PCB_ready[0].PC;
                        PCB_ready[t].start = PCB_ready[0].start;
                        PCB_ready[t].result = PCB_ready[0].result;
                        PCB_ready[t].time = PCB_ready[0].time;
                        for (int i = 0; i < t; i++)
                        {
                            PCB_ready[i].index = PCB_ready[i + 1].index;
                            PCB_ready[i].command = PCB_ready[i + 1].command;
                            PCB_ready[i].instructment = PCB_ready[i + 1].instructment;
                            PCB_ready[i].start = PCB_ready[i + 1].start;
                            PCB_ready[i].PC = PCB_ready[i + 1].PC;
                            PCB_ready[i].result = PCB_ready[i + 1].result;
                            PCB_ready[i].time = PCB_ready[i + 1].time;
                        }
                        PCB_ready[t].index = 0;
                        PCB_ready[t].command = "";
                        PCB_ready[t].instructment = '\\';
                        PCB_ready[t].PC = 0;
                        PCB_ready[t].start = 4;
                        PCB_ready[t].result = 0;
                        PCB_ready[t].time = 0;
                    }
                    else if (t==PCB_ready.Length)
                    {
                        int index = PCB_ready[0].index;
                        string command00 = PCB_ready[0].command;
                       char instructment = PCB_ready[0].instructment;
                        int PC = PCB_ready[0].PC;
                        int start = PCB_ready[0].start;
                        int result = PCB_ready[0].result;
                        int time = PCB_ready[0].time;
                        for (int i = 0; i < PCB_ready.Length-1; i++)
                        {
                            PCB_ready[i].index = PCB_ready[i + 1].index;
                            PCB_ready[i].command = PCB_ready[i + 1].command;
                            PCB_ready[i].instructment = PCB_ready[i + 1].instructment;
                            PCB_ready[i].start = PCB_ready[i + 1].start;
                            PCB_ready[i].PC = PCB_ready[i + 1].PC;
                            PCB_ready[i].result = PCB_ready[i + 1].result;
                            PCB_ready[i].time = PCB_ready[i + 1].time;
                        }
                        PCB_ready[PCB_ready.Length - 1].index = index ;
                        PCB_ready[PCB_ready.Length - 1].command = command00;
                        PCB_ready[PCB_ready.Length - 1].instructment =instructment;
                        PCB_ready[PCB_ready.Length - 1].PC = PC;
                        PCB_ready[PCB_ready.Length - 1].start = start;
                        PCB_ready[PCB_ready.Length - 1].result = result;
                        PCB_ready[PCB_ready.Length - 1].time = time;
                    }
                }
            }
            cope_block();
            textBox7.Enabled = true;
            textBox7.Focus();
        }//-------------------------------------------执行进程
        public void updata_process(int x)
        {
            textBox22.Text = "";
            for (int i = x; i < PCB_ready.Length; i++)
                if (PCB_ready[i].index != 0)
                    textBox22.Text += "进程" + PCB_ready[i].index+"\r\n";
            textBox23.Text = "";
            int flag1=0, flag2=0;
            for (int i = 0; i < PCB_block.Length; i++)
                if (PCB_block[i].index != 0)
                {
                    flag1 =1;
                    break;
                }
            if (flag1==1)
            {
                textBox23.Text = "占用设备阻塞" + "\r\n";
                for (int i = 0; i < PCB_block.Length; i++)
                    if (PCB_block[i].index != 0)
                        textBox23.Text += "进程" + PCB_block[i].index+"\r\n";
            }
            for (int i = 0; i <PCB_wait.Length; i++)
                if (PCB_wait[i].index !=0)
                {
                    flag2 = 1;
                    break;
                }
            if (flag2 == 1)
            {
                textBox23.Text += "等待设备阻塞" + "\r\n";
                for (int i = 0; i < PCB_wait.Length; i++)
                    if (PCB_wait[i].index != 0)
                        textBox23.Text += "进程" +PCB_wait[i].index+"\r\n";
            }
        }//--------------------------------------更新进程运行情况
        public void CPU()
        {
            textBox20.Text = "闲逛进程";
        }//---------------------------------------------------------CPU

        private void timer1_Tick(object sender, EventArgs e)
        {
            int hour, minute, second;
            time++;
            hour = time / 3600;
            minute = (time - hour * 3600) / 60;
            second = time - hour * 3600 - minute * 60;
            textBox25.Text  = "";
            textBox25.Text = string.Format("{0:00}:{1:00}:{2:00}", hour,minute, second);
        }//------------------系统时间
        private void timer2_Tick(object sender, EventArgs e)
        { 
            textBox20.Text = "";
            textBox21.Text = "";
            textBox24.Text = ""; 
            updata_process(0);
            if (count_process == 0)
            {
                CPU();
            }
            else
            {
                excute_process();
            }
        }//------------------时间片时间
        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = textBox7.Text;
                textBox7.Text = "";
                path = path.Trim();
                create_process(path);
            }
        }//------执行进程接口

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileStream fs = new FileStream(path1,FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(Bitmapc, 0, Bitmapc.Length);
            fs.Close();
            fs = new FileStream(path2, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(Bitmapd, 0, Bitmapd.Length);
        }
        //-----------------------------------------------------------------------------图形接口
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode.ImageIndex == 4 || treeView1.SelectedNode.ImageIndex==1))
            {
                return;
            }
            string paths="";
            TreeNode tmp00=treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            string name= Microsoft.VisualBasic.Interaction.InputBox("请输入文件名及扩展名", "创建文件", "例如：***.txt", 0, 0);
            if (!(name == "" || name== "例如：***.txt" || name.Length < 4))
            {
                string[] tmp02 = name.Split('.');
                if (tmp02.Length == 1)
                {
                    return;
                }
                else
                {
                    if (!(tmp02[1]=="fil" || tmp02[1]=="exe" || tmp02[1]=="txt"))
                    {
                        return;
                    }
                }
                paths = paths + "\\" + name;
                paths = paths.Substring(2, paths.Length - 2);
                create_Node(paths);
                add_file(paths);
            }
            else
            {
                MessageBox.Show("请按照格式输入");
                return;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex == 0 || treeView1.SelectedNode.ImageIndex == 1)
            {
                return;
            }
            string paths = "";
            TreeNode tmp00 = treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            if (treeView1.SelectedNode.ImageIndex == 2)
                paths += ".exe";
            else if (treeView1.SelectedNode.ImageIndex == 3)
                paths += ".txt";
            else
                paths += ".fil";
            paths = paths.Substring(2, paths.Length - 2);
            delete_Node(paths);
            delete_File(paths);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex == 0 || treeView1.SelectedNode.ImageIndex == 1|| treeView1.SelectedNode.ImageIndex ==4)
            {
                return;
            }
            string paths = "";
            TreeNode tmp00 = treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            if (treeView1.SelectedNode.ImageIndex == 2)
                paths += ".exe";
            else if (treeView1.SelectedNode.ImageIndex == 3)
                paths += ".txt";
            else
                paths += ".fil";
            textBox2.Enabled = true;
            button2.Enabled = true;
            paths = paths.Substring(2, paths.Length - 2);
            road = paths;
            type_file(paths);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex == 0 || treeView1.SelectedNode.ImageIndex == 1||treeView1.SelectedNode.ImageIndex ==4)
            {
                return;
            }
            string paths = "";
            TreeNode tmp00 = treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            if (treeView1.SelectedNode.ImageIndex == 2)
                paths += ".exe";
            else if (treeView1.SelectedNode.ImageIndex == 3)
                paths += ".txt";
            else
                paths += ".fil";
            paths = paths.Substring(2, paths.Length - 2);
            type_file(paths);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex == 0
                || treeView1.SelectedNode.ImageIndex == 1
                || treeView1.SelectedNode.ImageIndex == 3
                || treeView1.SelectedNode.ImageIndex == 4)
            {
                return;
            }
            string paths = "";
            TreeNode tmp00 = treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            if (treeView1.SelectedNode.ImageIndex == 2)
                paths += ".exe";
            else if (treeView1.SelectedNode.ImageIndex == 3)
                paths += ".txt";
            else
                paths += ".fil";
            paths = paths.Substring(2, paths.Length - 2);
            create_process(paths);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex != 1)
                return;
            string path = treeView1.SelectedNode.Text;
            path = path.Substring(2, path.Length - 2);
            format(path);
        }    

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.ImageIndex == 0
                || treeView1.SelectedNode.ImageIndex == 1
                || treeView1.SelectedNode.ImageIndex == 4)
            {
                return;
            }
            string paths = "";
            TreeNode tmp00 = treeView1.SelectedNode;
            while (tmp00.Text != "我的电脑")
            {
                string tmp01 = tmp00.Text;
                if (paths == "")
                    paths = tmp01;
                else
                {
                    paths = tmp01 + "\\" + paths;
                }
                tmp00 = tmp00.Parent;
            }
            if (treeView1.SelectedNode.ImageIndex == 2)
                paths += ".exe";
            else if (treeView1.SelectedNode.ImageIndex == 3)
                paths += ".txt";
            else
                paths += ".fil";
            if (treeView1.SelectedNode.ImageIndex ==2)
            {
                paths = paths.Substring(2, paths.Length - 2);
                create_process(paths);
            }
            else if (treeView1.SelectedNode.ImageIndex == 3)
            {
                textBox2.Enabled = true;
                button2.Enabled = true;
                paths = paths.Substring(2, paths.Length - 2);
                type_file(paths);
                road = paths;
            }
        }    
    }
}