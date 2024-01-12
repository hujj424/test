using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace CommandDemo2
{
    public partial class Form1 : Form
    {
        private BackgroundWorker dataLoaderWorker;
        private PictureBox pictureBox;
        private float originalWidth, originalHeight;
        private const float ZoomFactor = 3f; // 設定放大倍率
        public Form1()
        {
            InitializeComponent();
        }

        string cnstr = @"Data Source=127.0.0.1;Initial Catalog=Eye;
                        User ID=sa;Password=sa123";
        //int sqlStr ;
        void ShowData()
        {

            using (SqlConnection cn = new SqlConnection())
            {
                // sqlStr = 1;
                cn.ConnectionString = cnstr;
                SqlDataAdapter Eyes = new SqlDataAdapter
                    ("SELECT [M_ID],[Patient_ID],[Sex],CONVERT(varchar, [Birth],111) AS [Birth], [Age],[DateTime],[CD],[SD],[CV],[6A],[AVE],[MAX],[MIN],[NUM],[PACHY],[MEMO]  FROM [Eye].[dbo].[Eye] ORDER BY CAST([M_ID] AS INT)", cn);
                DataSet ds = new DataSet();
                Eyes.Fill(ds, "Eye");
                dataGridView1.DataSource = ds.Tables["Eye"];
            }

        }

        // 表單載入時執行
        private void Form1_Load(object sender, EventArgs e)
        {
            ShowData();
            this.WindowState = FormWindowState.Maximized;
            originalPictureBoxWIdth = pictureBox1.Width;
            originalPictureBoxHIdth = pictureBox1.Height;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            dataLoaderWorker = new BackgroundWorker();
            dataLoaderWorker.DoWork += DataLoaderWorker_DoWork;
            dataLoaderWorker.RunWorkerCompleted += DataLoaderWorker_RunWorkerCompleted;
        }



        // 按下 [新增] 鈕執行
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try  //使用try...catch...敘述來補捉異動資料可能發生的例外 
            {
                using (SqlConnection cn = new SqlConnection())
                {

                    cn.ConnectionString = cnstr;
                    cn.Open();
                    string checkDuplicateQuery = $"SELECT COUNT(*) FROM Eye WHERE [M_ID] = {int.Parse(M_ID.Text)}";
                    SqlCommand checkCmd = new SqlCommand(checkDuplicateQuery, cn);
                    int duplicateCount = (int)checkCmd.ExecuteScalar();

                    if (duplicateCount > 0)
                    {
                        MessageBox.Show("M_ID 已存在，請輸入不同的 M_ID。");
                        return; // 不執行新增操作
                    }
                    string sqlStr;
                    //SET IDENTITY_INSERT Eye ON
                    sqlStr = $@"  INSERT INTO Eye([M_ID],[Patient_ID],[Sex],[Birth],[Age],[DateTime],[CD],[SD],[CV],[6A],[AVE],[MAX],[MIN],[NUM],[PACHY],[MEMO]) 
                    VALUES( {int.Parse(M_ID.Text)},
                            '{P_ID.Text.Replace("'", "*")}',
                            '{Sex.Text.Replace("'", "*")}',
                             {(string.IsNullOrEmpty(Birth.Text) ? "NULL" : $"'{DateTime.Parse(Birth.Text).ToString("yyyy-MM-dd")}'")}, -- 可以为空的日期值
                             '{Age.Text.Replace("'", "*")}',
                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',  --病人看診時的時間
                             '{CD.Text.Replace("'", "*")}',
                             '{SD.Text.Replace("'", "*")}',
                             '{CV.Text.Replace("'", "*")}',
                             '{IS_6A.Text.Replace("'", "*")}',
                             '{AVE.Text.Replace("'", "*")}',
                             '{MAX.Text.Replace("'", "*")}',
                             '{MIN.Text.Replace("'", "*")}',
                             '{NUM.Text.Replace("'", "*")}',
                             '{PACHY.Text.Replace("'", "*")}',
                             '{MEMO.Text.Replace("'", "*")}')";




                    SqlCommand Cmd = new SqlCommand(sqlStr, cn);
                    Cmd.ExecuteNonQuery();
                    ShowData();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", 新增資料發生錯誤");
            }
        }
        // 按下 [更新] 鈕執行 
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try	//使用try...catch...敘述來補捉異動資料可能發生的例外
            {
                using (SqlConnection cn = new SqlConnection())
                {
                    cn.ConnectionString = cnstr;
                    cn.Open();
                    string sqlStr = $@"UPDATE Eye SET 
                    [P_ID]= '{P_ID.Text.Replace("'", " * ")}',
                    [Sex]='{Sex.Text.Replace("'", " * ")}',
                    [Birth]='{Birth.Text.Replace("'", " * ")}',
                    [Age]='{Age.Text.Replace("'", " * ")}',
                    [CD]='{CD.Text.Replace("'", " * ")}',
                    [SD]='{SD.Text.Replace("'", " * ")}',
                    [CV]='{CV.Text.Replace("'", " * ")}',
                    [6A]='{IS_6A.Text.Replace("'", " * ")}',
                    [AVE]='{AVE.Text.Replace("'", " * ")}',
                    [MAX]='{MAX.Text.Replace("'", " * ")}',
                    [MIN]='{MIN.Text.Replace("'", " * ")}',
                    [NUM]='{NUM.Text.Replace("'", " * ")}',
                    [PACHY]='{PACHY.Text.Replace("'", " * ")}',
                    [MEMO]='{MEMO.Text.Replace("'", " * ")}'
                    WHERE [M_ID]= {int.Parse(M_ID.Text)}";
                    SqlCommand Cmd = new SqlCommand(sqlStr, cn);
                    Cmd.ExecuteNonQuery();


                    ShowData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", 修改資料發生錯誤");
            }
        }
        // 按下 [刪除] 鈕 
        private void btnDel_Click(object sender, EventArgs e)
        {
            using (SqlConnection cn = new SqlConnection())
            { 
                cn.ConnectionString = cnstr;
                cn.Open();
                
                    string sqlStr = $"DELETE FROM Eye WHERE [M_ID] =  {int.Parse(M_ID.Text)}";
                    SqlCommand Cmd = new SqlCommand(sqlStr, cn);
                    Cmd.ExecuteNonQuery();
                

            }

            
            ShowData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // 將列名映射到TextBox控件
                M_ID.Text = row.Cells["M_ID"].Value.ToString();
                P_ID.Text = row.Cells["Patient_ID"].Value.ToString();
                Sex.Text = row.Cells["Sex"].Value.ToString();
                Birth.Text = row.Cells["Birth"].Value.ToString();
                Age.Text = row.Cells["Age"].Value.ToString();
                CD.Text = row.Cells["CD"].Value.ToString();
                SD.Text = row.Cells["SD"].Value.ToString();
                CV.Text = row.Cells["CV"].Value.ToString();
                IS_6A.Text = row.Cells["6A"].Value.ToString();
                AVE.Text = row.Cells["AVE"].Value.ToString();
                MAX.Text = row.Cells["MAX"].Value.ToString();
                MIN.Text = row.Cells["MIN"].Value.ToString();
                NUM.Text = row.Cells["NUM"].Value.ToString();
                PACHY.Text = row.Cells["PACHY"].Value.ToString();
                MEMO.Text = row.Cells["MEMO"].Value.ToString();


                // 任何列中的點擊
                string text = row.Cells[1].Value.ToString();

                // 保留僅數字字符
                string resultText = new string(text.Where(c => char.IsDigit(c) || c == '-').ToArray());

                // 使用結果更新TextBox
                P_ID.Text = resultText;

                // 在所有子文件夾中搜索圖片文件
                string pictureFileName = $"{resultText}.jpg";
                string pictureDirectory = @"C:\Users\胡家傑\Desktop\2023_11_24專題\2023_11_24專題\CommandDemo2\pictures\";

                // 獲取所有匹配的文件（包括子文件夾）
                string[] matchingFiles = Directory.GetFiles(pictureDirectory, pictureFileName, SearchOption.AllDirectories);

                if (matchingFiles.Length > 0)
                {
                    // 在pictureBox1中顯示第一個匹配的圖片文件
                    pictureBox1.ImageLocation = matchingFiles[0];

                }
                else
                {
                    pictureBox1.ImageLocation = string.Empty;
                    MessageBox.Show($"未找到圖片文件：{pictureFileName}", "未找到圖片文件！");

                    
                }

            }

        }
        


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(cnstr))
                {
                    cn.Open();

                    string query = "SELECT [M_ID],[Patient_ID] as Patient_ID,[Sex],CONVERT(varchar, [Birth],111) AS [Birth], [Age],[DateTime],[CD],[SD],[CV],[6A],[AVE],[MAX],[MIN],[NUM],[PACHY],[MEMO] FROM Eye WHERE [Patient_ID] =@Patient_ID";

                    //"SELECT [emp001] as 編號, [emp002] as 姓名, [emp003] as 身分證字號, [emp004] as 性別, [emp005] as 生日, [emp006] as 職位編號, [emp008] as 住址, [emp009] as 上級主管編號 FROM Employee WHERE [emp001] = @編號";
                    SqlCommand command = new SqlCommand(query, cn);
                    command.Parameters.AddWithValue("@Patient_ID", P_ID.Text.ToString());
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "查詢資料發生錯誤");
            }
        }
        private bool isCustomerTableDisplayed = false;
        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            M_ID.Text = "";
            P_ID.Text = "";
            Sex.Text = "";
            Birth.Text = "";
            Age.Text = "";
            CD.Text = "";
            SD.Text = "";
            CV.Text = "";
            IS_6A.Text = "";
            AVE.Text = "";
            MAX.Text = "";
            MIN.Text = "";
            NUM.Text = "";
            PACHY.Text = "";
            MEMO.Text = "";
            pictureBox1.ImageLocation = string.Empty;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void DataLoaderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 在背景執行緒中執行耗時操作
            ShowData();
        }

        private void DataLoaderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 背景執行緒完成時的處理
            if (e.Error != null)
            {
                MessageBox.Show($"發生錯誤: {e.Error.Message}");
            }
            else
            {
                // 處理完成後的任何操作
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataLoaderWorker.RunWorkerAsync();
        }
        private int originalPictureBoxWIdth;
        private int originalPictureBoxHIdth;
        private void PictureBox1_MouseEnter(object sender, EventArgs e)
        {
            

            // 放大PictureBox
            pictureBox1.Width = (int)(pictureBox1.Width * 1.2);
            pictureBox1.Height = (int)(pictureBox1.Height* 1.2);
        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            // 恢復PictureBox的原始大小
            pictureBox1.Width = originalPictureBoxWIdth;
            pictureBox1.Height = originalPictureBoxHIdth; 
        }
    }

}
