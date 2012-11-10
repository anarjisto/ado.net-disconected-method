using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        private string activeTableName;
        private int activeId;
        public Form1()
        {
            InitializeComponent();
            var tables = StorageResource.Instance.getListOfTables();
            foreach (var table in tables)
            {
                comboBox1.Items.Add(table);
            }
        }

      

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            activeId = 0;
            string tableName = (string)comboBox1.Items[comboBox1.SelectedIndex];
            activeTableName = tableName;
            var columnNameList = StorageResource.Instance.getAllColumnNamesFromTable(tableName);
           
            Console.WriteLine("nr of subviews before clear is " + flowLayoutPanel1.Controls.Count);
            flowLayoutPanel1.Controls.Clear();



            Console.WriteLine("nr of subviews after clear is " + flowLayoutPanel1.Controls.Count);
            foreach(var str in columnNameList)
            {
                var lbl = new Label();
                lbl.Name = "lbl" + str;
                lbl.Text = str;
                lbl.AutoSize = true;
                lbl.Location = new Point(7, 130);
                lbl.Size = new Size(35, 13);


                var txtBox = new TextBox();
                txtBox.Name = str;
                txtBox.Tag = str;
                txtBox.Text = str;
                Point p = lbl.Location;
                p.X += 170;
                txtBox.Location = p;
                txtBox.Size = new Size(100, 20);


                flowLayoutPanel1.Controls.Add(lbl);
                flowLayoutPanel1.Controls.Add(txtBox);
            }
            var array = StorageResource.Instance.getAllPKFromTable(activeTableName);
            try
            {
                this.updateTextBoxes(array[0]);
            }
            catch (Exception ee)
            {
                Console.WriteLine("exeption =====>"+ee);
            }
            


        }

        void updateTextBoxes(int index)
        {
            var activePk = StorageResource.Instance.getPKColumnFromTable(activeTableName);
            if (index > 0)
            {
                var dict = StorageResource.Instance.getRowValues(index, activeTableName);
              
                        foreach (Control ctrl in flowLayoutPanel1.Controls)
                        {
                            if (ctrl is TextBox)
                            {
                                // Console.WriteLine(reader.ToString() + "\n" + ctrl.ToString());
                                ctrl.Text = dict[ctrl.Tag.ToString()];
                                if (ctrl.Tag.ToString().Equals(activePk))
                                    ctrl.Enabled = false;
                                
                            }

                        }
            }
            else
            {
                foreach (Control ctrl in flowLayoutPanel1.Controls)
                {
                    if (ctrl is TextBox)
                    {
                        // Console.WriteLine(reader.ToString() + "\n" + ctrl.ToString());
                        ctrl.Text = "";
                         if (ctrl.Tag.ToString().Equals(activePk))
                             ctrl.Enabled = false;

                    }

                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dic = new Dictionary<string, string>();
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is TextBox)
                {
                    dic.Add(ctrl.Tag.ToString(),ctrl.Text);
                }
            }

            StorageResource.Instance.updateOrInsertRow(dic,activeTableName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Backward
            try
            {
                var array = StorageResource.Instance.getAllPKFromTable(activeTableName);
                if (activeId > 0)
                    activeId--;
                else
                {
                    activeId = 0;
                }
                this.updateTextBoxes(array[activeId]);
            }
            catch (Exception)
            {
                
               
            }
           

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Forward
            try
            {
                var array = StorageResource.Instance.getAllPKFromTable(activeTableName);
                if (activeId < array.Count - 2)
                    activeId++;
                else
                {
                    activeId = array.Count - 1;
                }
                updateTextBoxes(array[activeId]);
            }
            catch (Exception ee)
            {
                
                
            }
            

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Clear * textviews
            updateTextBoxes(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StorageResource.Instance.deleteRecordFromTableWithIndex(activeTableName,activeId);
            var array = StorageResource.Instance.getAllPKFromTable(activeTableName);
            try
            {
                this.updateTextBoxes(array[0]);
            }
            catch (Exception)
            {

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            StorageResource.Instance.NeedStoredProc = checkBox1.Checked;
        }
    }
}
