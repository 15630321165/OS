using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace operating_system
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
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
        public static byte[] Bitmapc = new byte[128];//---------------------------磁盘位示图

        public static int disk_countc;//-------------------------------磁盘使用块数
        public static PCB[] PCB_ready = new PCB[10];
        public static PCB[] PCB_block = new PCB[10];
        public static PCB[] PCB_wait = new PCB[10];
        public static Instruct[] instructment = new Instruct[6];  
        public static string path1 = "disk1.txt";
        public static string path2 = "disk2.txt";
        public static string road;
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
 
            }
            initiate_PCB();
            
            timer1.Enabled = true;
            timer2.Enabled = true;
           
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

            this.treeView1.SelectedNode.Nodes.Add(tmp1);
        }

        public void initiate_disc()//--------------------------------------------------------初始化磁盘
        {
            for (int i = 0; i < Bitmapc.Length; i++)
                if (i <= 1) // 0,1存放文件分配表
                {
                    Bitmapc[i] = 1;
 
                }
                else
                {
                    Bitmapc[i] = 0;

                }
            disk_countc = 2;
 
            textBox3.Text = "";
            textBox4.Text = "";
           
            textBox3.Text += disk_countc;
            textBox4.Text += (128 - disk_countc);

           

        }
        public void initiate_lable()
        {
            for (int i = 0; i < c_lable .GetLength(0); i++)
                for (int j = 0; j < c_lable .GetLength(1); j++)
                {
                    c_lable[i, j] = new Label();
                    c_lable[i, j].BackColor = Color.White;
                    c_lable[i, j].Height = 16;
                    c_lable[i, j].Width = 16;
                    c_lable[i, j].Location = new Point(25 +i * 16, 25 +j * 16);
                    groupBox3.Controls.Add(c_lable [i,j]);
                }
            c_lable[0,0].BackColor = Color.CornflowerBlue;
            c_lable[1,0].BackColor = Color.CornflowerBlue;
 
        }//-----------------------------------------------------初始化lable

        public void initiate_catalog()//----------------------------------------------------初始化子目录
        {
            byte[] one =new byte[16384];
            for (int i = 0; i <one.Length; i++)
                one[i]=0;
            FileStream file1 =new FileStream(path1,FileMode.CreateNew);// FileStream用于文件任何位置的读写
            file1.Seek(0,SeekOrigin.Begin);
            file1.Write(one,0,one.Length);
            for (int i = 0; i < 8; i++)
            {
                // 将文件指针移动到 i*16+128处，起始位置是文件的开始
                file1.Seek(i*16+128,SeekOrigin.Begin);
                // 写入一个字节
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
            if (disk.StartsWith("c") || disk.StartsWith("C"))
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
                x = k % 16;
                y = k / 16;
                c_lable[x, y].BackColor = Color.Red;
            }
        
            else
            {
                for (k = 0; k < Bitmapc.Length; k++)
                    if (Bitmapc[k] == 0)
                        break;
                Bitmapc[k] = 1;
                disk_countc++;
               
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
 
        }
        public void deduce_disc()
        {
            int x, y;
            disk_countc = 2;

            FileStream fs = new FileStream(path1, FileMode.Open, FileAccess.Read);
            fs.Seek(0,SeekOrigin.Begin);
            fs.Read(Bitmapc, 0, 128);
            fs.Close();
            for (int i = 2; i <Bitmapc.Length; i++)
                if (Bitmapc[i]==1)
                {
                    x =Convert.ToInt32(i) % 16;
                    y =Convert.ToInt32(i) / 16 ;
                    c_lable[x, y].BackColor = Color.LightSalmon; 
                    disk_countc++;
                }
            textBox3.Text = "";
            textBox4.Text = "";
            textBox3.Text += disk_countc;
            textBox4.Text += (128 - disk_countc);
            fs = new FileStream(path2, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);

            fs.Close();
 
           
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

        }

      
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox2.Enabled = false;
      
        }//----------------清除textbox2内容

        private void button2_Click(object sender, EventArgs e)
        {
            edit_file(road);
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
            //count_process = 0;
            //count_processes = 0;
            time = 0;
        }
        
        

       
       

        
        
      

        
       

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
        
      
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileStream fs = new FileStream(path1,FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(Bitmapc, 0, Bitmapc.Length);
            fs.Close();
            fs = new FileStream(path2, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0, SeekOrigin.Begin);
//            fs.Write(Bitmapd, 0, Bitmapd.Length);
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
            //create_process(paths);
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
                //create_process(paths);
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


        private void button3_Click(object sender, EventArgs e)
        {

        }

    }
}