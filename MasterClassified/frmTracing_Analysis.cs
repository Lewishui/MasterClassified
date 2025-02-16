﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using MC.Buiness;
using MC.DB;
using System.Reflection;
using System.IO;
using MC.Common;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;

namespace MasterClassified
{
    public partial class frmTracing_Analysis : DockContent
    {
        private List<int> JIDTA = new List<int>();
        private Thread GetDataforOutlookThread;
        public log4net.ILog ProcessLogger { get; set; }
        public log4net.ILog ExceptionLogger { get; set; }
        private frmSetConfig frmSetConfig;
        private frmSetConfigList frmSetConfigList;
        private int isnewfanganlist;
        private frmUDF frmUDF;
        private List<int> UDF;//选择要分析的基数条目
        private List<int> InitialUDF;//按照彩票的Xuan 添加基数列的列数多少
        private List<int> changeInitialUDF;
        List<inputCaipiaoDATA> ClaimReport_Server;
        // 后台执行控件
        private BackgroundWorker bgWorker;
        // 消息显示窗体
        private frmMessageShow frmMessageShow;
        // 后台操作是否正常完成
        private bool blnBackGroundWorkIsOK = false;
        //后加的后台属性显
        private bool backGroundRunResult;
        private frmImport_MCleixing_Data frmImport_MCleixing_Data;
        private frmQianQiFenXi_Zidingyifenxi frmQianQiFenXi_Zidingyifenxi;
        List<int> newi;
        List<int> qianqi_newi;
        List<string> showSuijiResultlist = new List<string>();
        int qianqiqishu = 0;
        List<int> newlist;
        private SortableBindingList<inputCaipiaoDATA> sortablePendingOrderList;
        //private var Combox_qtyTable;
        DataTable Combox_qtyTable;
        bool tab3shuiji = false;
        private frmSelfPiLiangFangAN frmSelfPiLiangFangAN;
        List<string> checkfanganindex;

        List<FangAnLieBiaoDATA> piliangfangan_Result;
        List<inputCaipiaoDATA> ClaimReport_Server_Initial;
        string xiangtongxingfenxi = "";

        int suiji20_30 = 0;

        public frmTracing_Analysis()
        {
            InitializeComponent();

            InitialSystemInfo();

            changeInitialUDF = new List<int>();
            changeInitialUDF = InitialUDF;

            for (int m = 1; m <= InitialUDF[InitialUDF.Count - 1]; m++)
            {
                this.comboBox3.Items.Add("随机 " + m + " 位");
                this.checkedListBox2.Items.Add("基" + m);
            }
            //  this.checkedListBox2.Items.Add("特别号");
            this.comboBox3.SelectedIndex = 0;
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
            isnewfanganlist = 0;
            checkfanganindex = new List<string>();
            piliangfangan_Result = new List<FangAnLieBiaoDATA>();

        }

        private void InitialSystemInfo()
        {
            try
            {
                #region 初始化配置
                ProcessLogger = log4net.LogManager.GetLogger("ProcessLogger");
                ExceptionLogger = log4net.LogManager.GetLogger("SystemExceptionLogger");
                ProcessLogger.Fatal("System Start " + DateTime.Now.ToString());
                #endregion
                //按照彩票的Xuan 添加基数列的列数多少
                InitialUDF = new List<int>();
                UDF = new List<int>();
                clsAllnew BusinessHelp = new clsAllnew();
                List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");
                if (CaipiaozhongleiResult.Count != 0)
                    this.label1.Text = CaipiaozhongleiResult[0].Name;//"当前彩票类型：" 
                else
                {
                    MessageBox.Show("错误：请选择默认的彩票类型，再继续本界面的操作", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                //+"如数据或设置不能刷新请关闭本界面并重新在主界面打开"
                ClaimReport_Server = new List<inputCaipiaoDATA>();
                ClaimReport_Server = BusinessHelp.ReadclaimreportfromServerBy_Xuan(CaipiaozhongleiResult[0].Name);
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {
                    bool ischina = HasChineseTest(item.QiHao);
                    if (ischina == true || Regex.Matches(item.QiHao, "[a-zA-Z]").Count > 0)
                    {
                        MessageBox.Show("EX:异常类型,请修改或删除，不然会影响正常的数据判断，期号 ：" + item.QiHao);
                        return;
                    }
                }
                ClaimReport_Server.Sort(new Comp());
                List<inputCaipiaoDATA> ClaimReport_Server1 = new List<inputCaipiaoDATA>();
                ClaimReport_Server1 = ClaimReport_Server;
                ClaimReport_Server_Initial = new List<inputCaipiaoDATA>();
                ClaimReport_Server_Initial = ClaimReport_Server;
                var counties = ClaimReport_Server1.Select(s => new MockEntity { ShortName = s.QiHao, FullName = s.QiHao }).Distinct().ToList();

                this.toolStripComboBox2.ComboBox.DisplayMember = "FullName";
                this.toolStripComboBox2.ComboBox.ValueMember = "ShortName";
                this.toolStripComboBox2.ComboBox.DataSource = counties;


                var counties1 = ClaimReport_Server1.Select(s => new MockEntity { ShortName = s.QiHao, FullName = s.QiHao }).Distinct().ToList();
                //   counties.Insert(0, new MockEntity { ShortName = "すべて", FullName = "すべて" });
                this.toolStripComboBox1.ComboBox.DisplayMember = "FullName";
                this.toolStripComboBox1.ComboBox.ValueMember = "ShortName";
                this.toolStripComboBox1.ComboBox.DataSource = counties1;

                if (ClaimReport_Server.Count != 0)
                {
                    this.toolStripComboBox1.SelectedIndex = 0;
                    this.toolStripComboBox2.SelectedIndex = counties.Count - 1;
                    //this.toolStripComboBox3.SelectedIndex = 2;
                    this.toolStripComboBox4.SelectedIndex = 2;
                }

                this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;

                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";

                //按照彩票的Xuan 添加基数列的列数多少
                InitialUDF = new List<int>();
                if (CaipiaozhongleiResult[0].Xuan != null)
                {
                    InitialUDF.Add(Convert.ToInt32(CaipiaozhongleiResult[0].Xuan));
                }
                else
                {
                    MessageBox.Show("彩票数据缺失,请维护完整!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                NewMethodtab1();

            }
            catch (Exception ex)
            {
                ProcessLogger.Fatal("System Error 60239 " + ex + DateTime.Now.ToString());

                MessageBox.Show("系统初始化失败或找不到服务器,请关闭当前界面并重新尝试!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                throw;
            }

        }

        private void InitialBackGroundWorker()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bgWorker.ProgressChanged +=
                new ProgressChangedEventHandler(bgWorker_ProgressChanged);
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                blnBackGroundWorkIsOK = false;
            }
            else if (e.Cancelled)
            {
                blnBackGroundWorkIsOK = true;
            }
            else
            {
                blnBackGroundWorkIsOK = true;
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (frmMessageShow != null && frmMessageShow.Visible == true)
            {
                //设置显示的消息
                frmMessageShow.setMessage(e.UserState.ToString());
                //设置显示的按钮文字
                if (e.ProgressPercentage == clsConstant.Thread_Progress_OK)
                {
                    frmMessageShow.setStatus(clsConstant.Dialog_Status_Enable);
                }
            }
        }


        private void AutoSizeColumn(DataGridView dgViewFiles)
        {
            int width = 0;
            //使列自使用宽度
            //对于DataGridView的每一个列都调整
            for (int i = 0; i < dgViewFiles.Columns.Count; i++)
            {
                //将每一列都调整为自动适应模式
                dgViewFiles.AutoResizeColumn(i, DataGridViewAutoSizeColumnMode.AllCells);
                //记录整个DataGridView的宽度
                width += dgViewFiles.Columns[i].Width;
            }
            //判断调整后的宽度与原来设定的宽度的关系，如果是调整后的宽度大于原来设定的宽度，
            //则将DataGridView的列自动调整模式设置为显示的列即可，
            //如果是小于原来设定的宽度，将模式改为填充。
            if (width > dgViewFiles.Size.Width)
            {
                dgViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            }
            else
            {
                dgViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            //冻结某列 从左开始 0，1，2
            dgViewFiles.Columns[1].Frozen = true;
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {
            try
            {
                toolStripLabel9.Text = "";




                clsAllnew BusinessHelp = new clsAllnew();
                //ClaimReport_Server = new List<inputCaipiaoDATA>();

                int s = this.tabControl1.SelectedIndex;
                if (s == 0)
                {
                    toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";
                    //GetDataforOutlookThread = new Thread(NewMethodtab1);
                    //GetDataforOutlookThread.Start();
                    // this.checkedListBox2.Items.Clear();
                    NewMethodtab1();

                }
                else if (s == 1)
                {
                    toolStripComboBox4.Items.Clear();
                    for (int i = 1; i <= 2000; i++)
                    {
                        toolStripComboBox4.Items.Add(i);

                    }
                    toolStripComboBox4.SelectedIndex = 4;

                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

                    toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";

                    //GetDataforOutlookThread = new Thread(tab2);
                    //GetDataforOutlookThread.Start();
                    // tab2(BusinessHelp);
                    tab2();

                    QianQI_Zidingyi_InitialSystemInfo();

                    this.toolStripComboBox5.SelectedIndex = 0;
                    this.toolStripComboBox6.SelectedIndex = 0;
                    this.comboBox1.SelectedIndex = 0;
                    this.comboBox2.SelectedIndex = 0;

                    for (int i = 0; i < clbStatus.Items.Count; i++)
                    {
                        clbStatus.SetItemChecked(i, false);
                    }
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                    }
                }
                else if (s == 2)
                {
                    tab3shuiji = false;
                    RunTAB3();
                    //  button5_Click(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return;

                throw;
            }

        }

        private void RunTAB3()
        {
            if (qianqiqishu == 0)
            {
                toolStripComboBox4.Items.Clear();
                for (int i = 1; i <= 2000; i++)
                {
                    toolStripComboBox4.Items.Add(i);

                }
                toolStripComboBox4.SelectedIndex = 4;
            }
            qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
            if (tab3shuiji == false)
                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";


            tab3();



            //this.toolStripComboBox5.SelectedIndex = 0;
            //this.toolStripComboBox6.SelectedIndex = 0;
            if (tab3shuiji == false)
            {
                JISHU_Zidingyi_InitialSystemInfo();
                Chushihuatab3kongjian();

            }
        }

        private void Chushihuatab3kongjian()
        {
            this.comboBox5.SelectedIndex = 0;
            this.comboBox4.SelectedIndex = 0;

            for (int i = 0; i < checkedListBox4.Items.Count; i++)
            {
                checkedListBox4.SetItemChecked(i, false);
            }
            for (int i = 0; i < checkedListBox3.Items.Count; i++)
            {
                checkedListBox3.SetItemChecked(i, false);
            }
        }

        private void tab2()
        {
            clsAllnew BusinessHelp = new clsAllnew();
            List<string> qianmingcheng = new List<string>();
            //ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();
            ClaimReport_Server.Sort(new CompsSmall());
            int indexing = 0;
            foreach (inputCaipiaoDATA item in ClaimReport_Server)
            {
                item.qianAll = "";
                item.qianMingcheng = "";
                item.TongAll = "";
                indexing = 0;
                string text = "";

                foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                {
                    if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao) && indexing < Convert.ToInt32(qianqiqishu))
                    {
                        //new 20191120  找出哪些 基数相等
                        List<string> same_list = new List<string>();

                        indexing++;
                        int xiangtongindex = 0;
                        if (item.KaiJianHaoMa == null || temp.KaiJianHaoMa == null)
                            continue;

                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                        string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.KaiJianHaoMa, " ");
                        #region 匹配相同次数
                        string shifouyijingpanduanguozhegeshuzi = "";
                        for (int i = 0; i < temp3.Length; i++)
                        {
                            for (int j1 = 0; j1 < temp1.Length; j1++)
                            {
                                string[] tempi = System.Text.RegularExpressions.Regex.Split(shifouyijingpanduanguozhegeshuzi, " ");
                                int isruns = 0;

                                for (int ih = 0; ih < tempi.Length; ih++)
                                {
                                    if (temp3[i] == tempi[ih])
                                    {
                                        isruns++;
                                        break;

                                    }
                                }
                                if (isruns > 0)
                                    break;


                                if (temp3[i] == temp1[j1])
                                {
                                    shifouyijingpanduanguozhegeshuzi = temp3[i] + " " + shifouyijingpanduanguozhegeshuzi;
                                    xiangtongindex++;
                                    same_list.Add("JiShu" + (i + 1).ToString());
                                }
                            }
                        }

                        #endregion
                        //item.qianAll = item.qianAll + "\r\n前" + indexing + " " + xiangtongindex.ToString();

                        // 20191121 判断基数 有3个以内的做 前期分析用
                        if (xiangtongxingfenxi != null && xiangtongxingfenxi == "YES")
                            same_qianqi_tab2mark(item, same_list, xiangtongindex);
                        else
                            item.qianAll = item.qianAll + " " + xiangtongindex.ToString();
                        text = text + " " + xiangtongindex.ToString();
                        //item.qianAll = item.qianAll + " " + xiangtongindex.ToString();
                        item.qianMingcheng = item.qianMingcheng + "\r\n前" + indexing;
                        //  qianmingcheng = item.qianMingcheng + "\r\n前" + indexing; ;
                        int isrun = 0;
                        for (int m = 0; m < qianmingcheng.Count; m++)
                        {
                            if (qianmingcheng[m] == "前" + indexing)
                                isrun++;
                        }
                        if (isrun == 0)
                            qianmingcheng.Add("前" + indexing);
                    }
                    else if (indexing > Convert.ToInt32(qianqiqishu))
                    {
                        break;

                    }
                }
                string[] temptong = System.Text.RegularExpressions.Regex.Split(text, " ");

                //     for (int j = 0; j < 30; j++)
                //20240328
                for (int j = 0; j < 50; j++)
                {
                    int xiangtongindex = 0;

                    for (int i = 1; i < temptong.Length; i++)
                    {
                        if (j.ToString() == temptong[i])
                        {
                            xiangtongindex++;
                        }

                    }
                    item.TongAll = item.TongAll + "\r\n同" + j + " " + xiangtongindex.ToString();

                }

            }
            var qtyTable = new DataTable();
            //foreach (var igrouping in ClaimReport_Server)
            //{
            //    // 生成 ioTable, use c{j}  instead of igrouping.Key, datagridview required
            //    //qtyTable.Columns.Add(igrouping._id, System.Type.GetType("System.String"));

            //    // qtyTable.Columns.Add(igrouping._id, System.Type.GetType("System.Int32"));
            //}
            int l = 0;
            //添加 抬头名称，如果 选中了前几期的combox 
            indexing = 1;
            qianmingcheng = new List<string>();
            for (int i = 1; i <= qianqiqishu; i++)
            {
                qianmingcheng.Add("前" + indexing);
                indexing++;
            }

            qtyTable.Columns.Add("期号", System.Type.GetType("System.Int32"));
            qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));

            for (int m = 0; m < qianmingcheng.Count; m++)
            {
                qtyTable.Columns.Add(qianmingcheng[m], System.Type.GetType("System.String"));

            }
            //  qtyTable.Rows.Add(qtyTable.NewRow());
            foreach (var k in ClaimReport_Server)
            {
                qtyTable.Rows.Add(qtyTable.NewRow());
            }

            int jk = 0;
            int cindex = 0;

            foreach (var item in ClaimReport_Server)
            {
                cindex = 0;

                if (item.qianAll != null)
                {
                    string[] temp1 = System.Text.RegularExpressions.Regex.Split(item.qianAll, " ");
                    for (int i = 0; i < temp1.Length; i++)
                    {
                        cindex++;

                        if (i == 0 || i >= temp1.Length)
                            continue;

                        qtyTable.Rows[jk][cindex] = temp1[i];
                    }
                }
                qtyTable.Rows[jk][0] = item.QiHao;
                qtyTable.Rows[jk][1] = item.KaiJianHaoMa;
                // qtyTable.Rows[1][4] = item.QiHao;
                jk++;
            }

            //   sortablePendingOrderList = new SortableBindingList<inputCaipiaoDATA>(qtyTable);
            //qtyTable.Sort(new Comp());
            //  this.bindingSource1.DataSource = null;
            this.bindingSource1.DataSource = qtyTable;
            bindingSource1.Sort = "期号  ASC";

            // this.dataGridView1.DataSource = this.bindingSource1;

            dataGridView2.DataSource = qtyTable;

            string width = "";

            for (int j = 2; j < dataGridView2.ColumnCount; j++)
            {

                dataGridView2.Columns[j].Width = 40;
            }
            if (dataGridView2.Rows.Count != 0)
            {
                int ii = dataGridView2.Rows.Count - 1;
                dataGridView2.CurrentCell = dataGridView2[0, ii]; // 强制将光标指向i行
                dataGridView2.Rows[ii].Selected = true;   //光标显示至i行 
            }

            toolStripLabel8.Text = "结束";
        }
        private void ZidingYi_tab2()
        {
            {
                clsAllnew BusinessHelp = new clsAllnew();
                List<string> qianmingcheng = new List<string>();
                //ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();

                ClaimReport_Server.Sort(new CompsSmall());
                int indexing = 0;
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {

                    item.qianAll = "";
                    item.qianMingcheng = "";
                    item.TongAll = "";
                    indexing = 0;
                    string text = "";

                    // List<inputCaipiaoDATA> filtered = ClaimReport_Server.FindAll(s => Convert.ToInt32(s.QiHao) > Convert.ToInt32(item.QiHao));

                    foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                    {
                        string shifouyijingpanduanguozhegeshuzi = "";
                        if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao) && indexing < Convert.ToInt32(qianqiqishu))
                        {
                            //new 20191120  找出哪些 基数相等
                            List<string> same_list = new List<string>();

                            indexing++;
                            int xiangtongindex = 0;
                            string[] temp3 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.KaiJianHaoMa, " ");

                            #region 匹配相同次数
                            for (int i = 0; i < temp3.Length; i++)
                            {
                                //判断是否在自定义范围内的数据
                                bool next = false;
                                for (int oi = 0; oi < newi.Count; oi++)
                                {
                                    if (newi[oi] == i + 1)
                                        next = true;
                                }
                                if (next == false)
                                    continue;
                                //前期数据的 分析数据的位置索引

                                for (int j1 = 0; j1 < temp1.Length; j1++)
                                {
                                    //判断是否在自定义范围内的数据
                                    bool nexti = false;
                                    for (int oi = 0; oi < qianqi_newi.Count; oi++)
                                    {
                                        if (qianqi_newi[oi] == j1 + 1)
                                        {
                                            nexti = true;
                                            break;
                                        }
                                    }
                                    if (nexti == false)
                                        continue;
                                    //判断一组号码内相同数字只判断一次
                                    string[] tempi = System.Text.RegularExpressions.Regex.Split(shifouyijingpanduanguozhegeshuzi, " ");
                                    int isruns = 0;

                                    for (int ih = 0; ih < tempi.Length; ih++)
                                    {
                                        if (temp3[i] == tempi[ih])
                                        {
                                            isruns++;
                                            break;

                                        }
                                    }
                                    if (isruns > 0)
                                        break;

                                    if (temp3[i] == temp1[j1])
                                    {

                                        shifouyijingpanduanguozhegeshuzi = temp3[i] + " " + shifouyijingpanduanguozhegeshuzi;
                                        xiangtongindex++;

                                        same_list.Add("JiShu" + (i + 1).ToString());
                                    }
                                }
                            }

                            #endregion
                            // 20191121 判断基数 有3个以内的做 前期分析用
                            if (xiangtongxingfenxi != null && xiangtongxingfenxi == "YES")
                                same_qianqi_tab2mark(item, same_list, xiangtongindex);
                            else
                                item.qianAll = item.qianAll + " " + xiangtongindex.ToString();

                            text = text + " " + xiangtongindex.ToString();
                            item.qianMingcheng = item.qianMingcheng + "\r\n前" + indexing;
                            //  qianmingcheng = item.qianMingcheng + "\r\n前" + indexing; ;
                            int isrun = 0;
                            for (int m = 0; m < qianmingcheng.Count; m++)
                            {
                                if (qianmingcheng[m] == "前" + indexing)
                                    isrun++;
                            }
                            if (isrun == 0)
                                qianmingcheng.Add("前" + indexing);

                        }
                        else if (indexing > Convert.ToInt32(qianqiqishu))
                        {
                            break;

                        }


                    }
                    string[] temptong = System.Text.RegularExpressions.Regex.Split(text, " ");

                    //for (int j = 0; j < 30; j++)
                    //     //20240328
                    for (int j = 0; j < 50; j++)
                    {
                        int xiangtongindex = 0;

                        for (int i = 1; i < temptong.Length; i++)
                        {
                            if (j.ToString() == temptong[i])
                            {
                                xiangtongindex++;
                            }

                        }
                        item.TongAll = item.TongAll + "\r\n同" + j + " " + xiangtongindex.ToString();

                    }

                }
                var qtyTable = new DataTable();
                //foreach (var igrouping in ClaimReport_Server)
                //{
                //    // 生成 ioTable, use c{j}  instead of igrouping.Key, datagridview required
                //    //qtyTable.Columns.Add(igrouping._id, System.Type.GetType("System.String"));

                //    // qtyTable.Columns.Add(igrouping._id, System.Type.GetType("System.Int32"));
                //}
                int l = 0;
                //添加 抬头名称，如果 选中了前几期的combox 
                indexing = 1;
                qianmingcheng = new List<string>();
                for (int i = 1; i <= qianqiqishu; i++)
                {
                    qianmingcheng.Add("前" + indexing);
                    indexing++;
                }

                qtyTable.Columns.Add("期号", System.Type.GetType("System.Int32"));
                qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));

                for (int m = 0; m < qianmingcheng.Count; m++)
                {
                    qtyTable.Columns.Add(qianmingcheng[m], System.Type.GetType("System.String"));

                }
                //  qtyTable.Rows.Add(qtyTable.NewRow());
                foreach (var k in ClaimReport_Server)
                {
                    qtyTable.Rows.Add(qtyTable.NewRow());
                }

                int jk = 0;
                int cindex = 0;

                foreach (var item in ClaimReport_Server)
                {
                    cindex = 0;

                    if (item.qianAll != null)
                    {
                        string[] temp1 = System.Text.RegularExpressions.Regex.Split(item.qianAll, " ");
                        for (int i = 0; i < temp1.Length; i++)
                        {
                            cindex++;

                            if (i == 0 || i >= temp1.Length)
                                continue;

                            qtyTable.Rows[jk][cindex] = temp1[i];
                        }
                    }
                    qtyTable.Rows[jk][0] = item.QiHao;
                    qtyTable.Rows[jk][1] = item.KaiJianHaoMa;
                    // qtyTable.Rows[1][4] = item.QiHao;
                    jk++;
                }

                //   sortablePendingOrderList = new SortableBindingList<inputCaipiaoDATA>(qtyTable);
                //qtyTable.Sort(new Comp());
                //  this.bindingSource1.DataSource = null;
                this.bindingSource1.DataSource = qtyTable;
                bindingSource1.Sort = "期号  ASC";

                this.dataGridView1.DataSource = this.bindingSource1;

                dataGridView2.DataSource = qtyTable;

                string width = "";

                for (int j = 2; j < dataGridView2.ColumnCount; j++)
                {

                    dataGridView2.Columns[j].Width = 50;
                }
                if (dataGridView2.Rows.Count != 0)
                {
                    int ii = dataGridView2.Rows.Count - 1;
                    dataGridView2.CurrentCell = dataGridView2[0, ii]; // 强制将光标指向i行
                    dataGridView2.Rows[ii].Selected = true;   //光标显示至i行 
                }
                toolStripLabel8.Text = "结束";
            }
        }

        private void same_qianqi_tab2mark(inputCaipiaoDATA item, List<string> same_list, int xiangtongindex)
        {
            // same_qian3mark(indexing, item, xiangtongindex, same_list);
            //列出相同内容
            string xiangtogn = "";
            //for (int yx = 0; yx < same_list.Count; yx++)
            //{
            //    xiangtogn += same_list[yx].Replace("JiShu", "第");
            //}
            ////item.qianAll = item.qianAll + "\r\n前" + indexing + " " + xiangtongindex.ToString();

            if (newi.Count < 5)
            {
                xiangtogn = "";
                for (int j = 0; j < newi.Count; j++)
                {
                    bool isrun = false;
                    for (int yx = 0; yx < same_list.Count; yx++)
                    {
                        //if (JIDTA.Count > 0)
                        {
                            if (("基" + newi[j].ToString()) == same_list[yx].Replace("JiShu", "基"))
                                isrun = true;


                            if (isrun == true)
                                break;

                        }
                    }
                    if (isrun == true)
                        xiangtogn += "" + 1;
                    else
                        xiangtogn += "" + 0;
                }
            }


            if (same_list != null && same_list.Count > 0)
                item.qianAll = item.qianAll + " " + xiangtongindex.ToString() + "-" + xiangtogn;
            else
                item.qianAll = item.qianAll + " " + xiangtongindex.ToString() + "-" + xiangtogn; ;
            // item.qianAll = item.qianAll + "-" + xiangtongindex.ToString();//20230516变更

        }

        private void NewMethodtab1()
        {
            try
            {



                //ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();
                clsAllnew BusinessHelp = new clsAllnew();

                #region 添加 基数 和前几期对比

                List<FangAnLieBiaoDATA> Result = BusinessHelp.Read_FangAn("YES");
                if (Result.Count != 0)
                {
                    this.label4.Text = "当前方案名称：　" + Result[0].Name;
                    if (Result[0].Data != null)
                    {
                        toolStripLabel7.Text = Result[0].Data.Replace("\r\n", "* ").Replace("=   ", "=");
                        //toolStripLabel9.Text = Result[0].Data.Replace("\r\n", "* ").Replace("=   ", "=");

                    }
                }
                //showSuijiResultlist = new List<string>();

                //foreach (FangAnLieBiaoDATA item in Result)
                //{
                //    string[] temp1 = System.Text.RegularExpressions.Regex.Split(item.Data, "\r\n");

                //    for (int i = 1; i < temp1.Length; i++)
                //    {
                //        showSuijiResultlist.Add(temp1[i]);
                //    }

                //}
                ClaimReport_Server.Sort(new CompsSmall());
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {

                    foreach (FangAnLieBiaoDATA temp in Result)
                    {
                        if (temp.xiangtongxingfenxi != null && temp.xiangtongxingfenxi == "YES")
                            xiangtongxingfenxi = "YES";

                        if (temp.Data == null)
                            continue;

                        string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.Data, "\r\n");
                        if (item.KaiJianHaoMa == null)
                            continue;

                        string[] temp2 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                        for (int ii = 0; ii < temp2.Length; ii++)
                        {
                            for (int i = 1; i < temp1.Length; i++)
                            {
                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp1[i], "段");
                                int ss = ii + 1;

                                //if (temp1[i].Contains(temp2[ii]))
                                if (temp3[1].Contains(temp2[ii]))
                                {
                                    //例如 27 包含7  所以容易出错再次切分判断一次  20240314
                                    string[] temp3_1 = System.Text.RegularExpressions.Regex.Split(temp3[1], " ");
                                    int ishanv = 0;
                                    for (int ip = 0; ip < temp3_1.Length; ip++)
                                    {
                                        if (temp3_1[ip] == temp2[ii])
                                            ishanv++;

                                    }
                                    if (ishanv == 0)
                                        continue;

                                    item.JiShu = item.JiShu + "基" + ss.ToString() + " " + temp3[0];
                                    if (ss == 1)
                                        item.JiShu1 = temp3[0];
                                    else if (ss == 2)
                                        item.JiShu2 = temp3[0];
                                    else if (ss == 3)
                                        item.JiShu3 = temp3[0];
                                    else if (ss == 4)
                                        item.JiShu4 = temp3[0];
                                    else if (ss == 5)
                                        item.JiShu5 = temp3[0];
                                    else if (ss == 6)
                                        item.JiShu6 = temp3[0];
                                    else if (ss == 7)
                                        item.JiShu7 = temp3[0];
                                    else if (ss == 8)
                                        item.JiShu8 = temp3[0];
                                    else if (ss == 9)
                                        item.JiShu9 = temp3[0];

                                    //new 

                                    else if (ss == 10)
                                        item.JiShu10 = temp3[0];

                                    else if (ss == 11)
                                        item.JiShu11 = temp3[0];

                                    else if (ss == 12)
                                        item.JiShu12 = temp3[0];

                                    else if (ss == 13)
                                        item.JiShu13 = temp3[0];

                                    else if (ss == 14)
                                        item.JiShu14 = temp3[0];

                                    else if (ss == 15)
                                        item.JiShu15 = temp3[0];
                                    //new 0621

                                    else if (ss == 16)
                                        item.JiShu16 = temp3[0];

                                    else if (ss == 17)
                                        item.JiShu17 = temp3[0];

                                    else if (ss == 18)
                                        item.JiShu18 = temp3[0];

                                    else if (ss == 19)
                                        item.JiShu19 = temp3[0];

                                    else if (ss == 20)
                                        item.JiShu20 = temp3[0];

                                    else if (ss == 21)
                                        item.JiShu21 = temp3[0];

                                    else if (ss == 22)
                                        item.JiShu22 = temp3[0];

                                    else if (ss == 23)
                                        item.JiShu23 = temp3[0];

                                    else if (ss == 24)
                                        item.JiShu24 = temp3[0];

                                    else if (ss == 25)
                                        item.JiShu25 = temp3[0];

                                    else if (ss == 26)
                                        item.JiShu26 = temp3[0];

                                    else if (ss == 27)
                                        item.JiShu27 = temp3[0];

                                    else if (ss == 28)
                                        item.JiShu28 = temp3[0];

                                    else if (ss == 29)
                                        item.JiShu29 = temp3[0];

                                    else if (ss == 30)
                                        item.JiShu30 = temp3[0];

                                    else if (ss == 31)
                                        item.JiShu31 = temp3[0];

                                    else if (ss == 32)
                                        item.JiShu32 = temp3[0];

                                    else if (ss == 33)
                                        item.JiShu33 = temp3[0];

                                    else if (ss == 34)
                                        item.JiShu34 = temp3[0];

                                    else if (ss == 35)
                                        item.JiShu35 = temp3[0];

                                    else if (ss == 36)
                                        item.JiShu36 = temp3[0];

                                    else if (ss == 37)
                                        item.JiShu37 = temp3[0];

                                    else if (ss == 38)
                                        item.JiShu38 = temp3[0];

                                    else if (ss == 39)
                                        item.JiShu39 = temp3[0];

                                    else if (ss == 40)
                                        item.JiShu40 = temp3[0];

                                    else if (ss == 41)
                                        item.JiShu41 = temp3[0];

                                    else if (ss == 42)
                                        item.JiShu42 = temp3[0];

                                    else if (ss == 43)
                                        item.JiShu43 = temp3[0];

                                    else if (ss == 44)
                                        item.JiShu44 = temp3[0];

                                    else if (ss == 45)
                                        item.JiShu45 = temp3[0];

                                    else if (ss == 46)
                                        item.JiShu46 = temp3[0];

                                    else if (ss == 47)
                                        item.JiShu47 = temp3[0];

                                    else if (ss == 48)
                                        item.JiShu48 = temp3[0];

                                    else if (ss == 49)
                                        item.JiShu49 = temp3[0];

                                    else if (ss == 50)
                                        item.JiShu50 = temp3[0];

                                    break;

                                }
                            }

                        }
                    }
                }

                #endregion

                // ClaimReport_Server.Sort(new Comp());
                int indexing = 0;
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {
                    indexing = 0;
                    foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                    {
                        if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao))
                        {
                            indexing++;
                            int xiangtongindex = 0;

                            //new 20191120  找出哪些 基数相等
                            List<string> same_list = new List<string>();

                            #region 匹配相同次数
                            if (item.JiShu1 != null && item.JiShu1 == temp.JiShu1)
                            {
                                same_list.Add("JiShu1");
                                xiangtongindex++;

                            }
                            if (item.JiShu2 != null && item.JiShu2 == temp.JiShu2)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu2");
                            }
                            if (item.JiShu3 != null && item.JiShu3 == temp.JiShu3)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu3");
                            }
                            if (item.JiShu4 != null && item.JiShu4 == temp.JiShu4)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu4");

                            }
                            if (item.JiShu5 != null && item.JiShu5 == temp.JiShu5)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu5");
                            }
                            if (item.JiShu6 != null && item.JiShu6 == temp.JiShu6)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu6");
                            }
                            if (item.JiShu7 != null && item.JiShu7 == temp.JiShu7)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu7");
                            }
                            if (item.JiShu8 != null && item.JiShu8 == temp.JiShu8)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu8");
                            }
                            if (item.JiShu9 != null && item.JiShu9 == temp.JiShu9)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu9");
                            }
                            //new
                            if (item.JiShu10 != null && item.JiShu10 == temp.JiShu10)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu10");
                            }
                            if (item.JiShu11 != null && item.JiShu11 == temp.JiShu11)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu11");
                            }
                            if (item.JiShu12 != null && item.JiShu12 == temp.JiShu12)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu12");
                            }
                            if (item.JiShu13 != null && item.JiShu13 == temp.JiShu13)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu13");
                            }
                            if (item.JiShu14 != null && item.JiShu14 == temp.JiShu14)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu14");
                            }
                            if (item.JiShu15 != null && item.JiShu15 == temp.JiShu15)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu15");
                            }
                            //new 0621
                            if (item.JiShu16 != null && item.JiShu16 == temp.JiShu16)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu16");
                            }
                            if (item.JiShu17 != null && item.JiShu17 == temp.JiShu17)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu17");
                            }
                            if (item.JiShu18 != null && item.JiShu18 == temp.JiShu18)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu18");
                            }
                            if (item.JiShu19 != null && item.JiShu19 == temp.JiShu19)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu19");
                            }
                            if (item.JiShu20 != null && item.JiShu20 == temp.JiShu20)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu20");

                            }
                            if (item.JiShu21 != null && item.JiShu21 == temp.JiShu21)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu21");
                            }
                            if (item.JiShu22 != null && item.JiShu22 == temp.JiShu22)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu22");
                            }
                            if (item.JiShu23 != null && item.JiShu23 == temp.JiShu23)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu23");
                            }
                            if (item.JiShu24 != null && item.JiShu24 == temp.JiShu24)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu24");

                            }
                            if (item.JiShu25 != null && item.JiShu25 == temp.JiShu25)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu25");
                            }
                            if (item.JiShu26 != null && item.JiShu26 == temp.JiShu26)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu26");

                            }
                            if (item.JiShu27 != null && item.JiShu27 == temp.JiShu27)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu27");
                            }
                            if (item.JiShu28 != null && item.JiShu28 == temp.JiShu28)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu28");
                            }
                            if (item.JiShu29 != null && item.JiShu29 == temp.JiShu29)
                            {
                                xiangtongindex++;
                                same_list.Add("JiShu29");
                            }
                            if (item.JiShu30 != null && item.JiShu30 == temp.JiShu30)
                            {
                                same_list.Add("JiShu30");
                                xiangtongindex++;
                            }
                            if (item.JiShu31 != null && item.JiShu31 == temp.JiShu31)
                            {
                                same_list.Add("JiShu31");
                                xiangtongindex++;
                            }
                            if (item.JiShu32 != null && item.JiShu32 == temp.JiShu32)
                            {
                                same_list.Add("JiShu32");
                                xiangtongindex++;
                            }
                            if (item.JiShu33 != null && item.JiShu33 == temp.JiShu33)
                            {
                                same_list.Add("JiShu33");
                                xiangtongindex++;
                            }
                            if (item.JiShu34 != null && item.JiShu34 == temp.JiShu34)
                            {
                                same_list.Add("JiShu34");
                                xiangtongindex++;
                            }
                            if (item.JiShu35 != null && item.JiShu35 == temp.JiShu35)
                            {
                                same_list.Add("JiShu35");
                                xiangtongindex++;
                            }
                            if (item.JiShu36 != null && item.JiShu36 == temp.JiShu36)
                            {
                                same_list.Add("JiShu36");
                                xiangtongindex++;
                            }
                            if (item.JiShu37 != null && item.JiShu37 == temp.JiShu37)
                            {
                                same_list.Add("JiShu37");
                                xiangtongindex++;
                            }
                            if (item.JiShu38 != null && item.JiShu38 == temp.JiShu38)
                            {
                                same_list.Add("JiShu38");
                                xiangtongindex++;
                            }
                            if (item.JiShu39 != null && item.JiShu39 == temp.JiShu39)
                            {
                                same_list.Add("JiShu39");
                                xiangtongindex++;
                            }
                            if (item.JiShu40 != null && item.JiShu40 == temp.JiShu40)
                            {
                                same_list.Add("JiShu40");
                                xiangtongindex++;
                            }
                            if (item.JiShu41 != null && item.JiShu41 == temp.JiShu41)
                            {
                                same_list.Add("JiShu41");
                                xiangtongindex++;
                            }
                            if (item.JiShu42 != null && item.JiShu42 == temp.JiShu42)
                            {
                                same_list.Add("JiShu42");
                                xiangtongindex++;
                            }
                            if (item.JiShu43 != null && item.JiShu43 == temp.JiShu43)
                            {
                                same_list.Add("JiShu43");
                                xiangtongindex++;
                            }
                            if (item.JiShu44 != null && item.JiShu44 == temp.JiShu44)
                            {
                                same_list.Add("JiShu44");
                                xiangtongindex++;
                            }
                            if (item.JiShu45 != null && item.JiShu45 == temp.JiShu45)
                            {
                                same_list.Add("JiShu45");
                                xiangtongindex++;
                            }
                            if (item.JiShu46 != null && item.JiShu46 == temp.JiShu46)
                            {
                                same_list.Add("JiShu46");
                                xiangtongindex++;
                            }
                            if (item.JiShu47 != null && item.JiShu47 == temp.JiShu47)
                            {
                                same_list.Add("JiShu47");
                                xiangtongindex++;
                            }
                            if (item.JiShu48 != null && item.JiShu48 == temp.JiShu48)
                            {
                                same_list.Add("JiShu48");
                                xiangtongindex++;
                            }
                            if (item.JiShu49 != null && item.JiShu49 == temp.JiShu49)
                            {
                                same_list.Add("JiShu49");
                                xiangtongindex++;
                            }
                            if (item.JiShu50 != null && item.JiShu50 == temp.JiShu50)
                            {
                                same_list.Add("JiShu50");
                                xiangtongindex++;
                            }

                            #endregion
                            #region MyRegion
                            if (indexing == 1)
                                item.qian1 = xiangtongindex.ToString();
                            else if (indexing == 2) item.qian2 = xiangtongindex.ToString();
                            else if (indexing == 3) item.qian3 = xiangtongindex.ToString();
                            else if (indexing == 4) item.qian4 = xiangtongindex.ToString();
                            else if (indexing == 5) item.qian5 = xiangtongindex.ToString();
                            else if (indexing == 6) item.qian6 = xiangtongindex.ToString();
                            else if (indexing == 7) item.qian7 = xiangtongindex.ToString();
                            else if (indexing == 8) item.qian8 = xiangtongindex.ToString();
                            else if (indexing == 9) item.qian9 = xiangtongindex.ToString();
                            else if (indexing == 10) item.qian10 = xiangtongindex.ToString();
                            else if (indexing == 11) item.qian11 = xiangtongindex.ToString();
                            else if (indexing == 12) item.qian12 = xiangtongindex.ToString();
                            else if (indexing == 13) item.qian13 = xiangtongindex.ToString();
                            else if (indexing == 14) item.qian14 = xiangtongindex.ToString();
                            else if (indexing == 15) item.qian15 = xiangtongindex.ToString();
                            else if (indexing == 16) item.qian16 = xiangtongindex.ToString();
                            else if (indexing == 17) item.qian17 = xiangtongindex.ToString();
                            else if (indexing == 18) item.qian18 = xiangtongindex.ToString();
                            else if (indexing == 19) item.qian19 = xiangtongindex.ToString();
                            else if (indexing == 20) item.qian20 = xiangtongindex.ToString();
                            else if (indexing == 21) item.qian21 = xiangtongindex.ToString();
                            else if (indexing == 22) item.qian22 = xiangtongindex.ToString();
                            else if (indexing == 23) item.qian23 = xiangtongindex.ToString();
                            else if (indexing == 24) item.qian24 = xiangtongindex.ToString();
                            else if (indexing == 25) item.qian25 = xiangtongindex.ToString();
                            else if (indexing == 26) item.qian26 = xiangtongindex.ToString();
                            else if (indexing == 27) item.qian27 = xiangtongindex.ToString();
                            else if (indexing == 28) item.qian28 = xiangtongindex.ToString();
                            else if (indexing == 29) item.qian29 = xiangtongindex.ToString();
                            else if (indexing == 30) item.qian30 = xiangtongindex.ToString();
                            else if (indexing == 31) item.qian31 = xiangtongindex.ToString();
                            else if (indexing == 32) item.qian32 = xiangtongindex.ToString();
                            else if (indexing == 33) item.qian33 = xiangtongindex.ToString();
                            else if (indexing == 34) item.qian34 = xiangtongindex.ToString();
                            else if (indexing == 35) item.qian35 = xiangtongindex.ToString();
                            else if (indexing == 36) item.qian36 = xiangtongindex.ToString();
                            else if (indexing == 37) item.qian37 = xiangtongindex.ToString();
                            else if (indexing == 38) item.qian38 = xiangtongindex.ToString();
                            else if (indexing == 39) item.qian39 = xiangtongindex.ToString();
                            else if (indexing == 40) item.qian40 = xiangtongindex.ToString();
                            else if (indexing == 41) item.qian41 = xiangtongindex.ToString();
                            else if (indexing == 42) item.qian42 = xiangtongindex.ToString();
                            else if (indexing == 43) item.qian43 = xiangtongindex.ToString();
                            else if (indexing == 44) item.qian44 = xiangtongindex.ToString();
                            else if (indexing == 45) item.qian45 = xiangtongindex.ToString();
                            else if (indexing == 46) item.qian46 = xiangtongindex.ToString();
                            else if (indexing == 47) item.qian47 = xiangtongindex.ToString();
                            else if (indexing == 48) item.qian48 = xiangtongindex.ToString();
                            else if (indexing == 49) item.qian49 = xiangtongindex.ToString();
                            else if (indexing == 50) item.qian50 = xiangtongindex.ToString();
                            else if (indexing == 51) item.qian51 = xiangtongindex.ToString();
                            else if (indexing == 52) item.qian52 = xiangtongindex.ToString();
                            else if (indexing == 53) item.qian53 = xiangtongindex.ToString();
                            else if (indexing == 54) item.qian54 = xiangtongindex.ToString();
                            else if (indexing == 55) item.qian55 = xiangtongindex.ToString();
                            else if (indexing == 56) item.qian56 = xiangtongindex.ToString();
                            else if (indexing == 57) item.qian57 = xiangtongindex.ToString();
                            else if (indexing == 58) item.qian58 = xiangtongindex.ToString();
                            else if (indexing == 59) item.qian59 = xiangtongindex.ToString();
                            else if (indexing == 60) item.qian60 = xiangtongindex.ToString();
                            else if (indexing == 61) item.qian61 = xiangtongindex.ToString();
                            else if (indexing == 62) item.qian62 = xiangtongindex.ToString();
                            else if (indexing == 63) item.qian63 = xiangtongindex.ToString();
                            else if (indexing == 64) item.qian64 = xiangtongindex.ToString();
                            else if (indexing == 65) item.qian65 = xiangtongindex.ToString();
                            else if (indexing == 66) item.qian66 = xiangtongindex.ToString();
                            else if (indexing == 67) item.qian67 = xiangtongindex.ToString();
                            else if (indexing == 68) item.qian68 = xiangtongindex.ToString();
                            else if (indexing == 69) item.qian69 = xiangtongindex.ToString();
                            else if (indexing == 70) item.qian70 = xiangtongindex.ToString();
                            else if (indexing == 71) item.qian71 = xiangtongindex.ToString();
                            else if (indexing == 72) item.qian72 = xiangtongindex.ToString();
                            else if (indexing == 73) item.qian73 = xiangtongindex.ToString();
                            else if (indexing == 74) item.qian74 = xiangtongindex.ToString();
                            else if (indexing == 75) item.qian75 = xiangtongindex.ToString();
                            else if (indexing == 76) item.qian76 = xiangtongindex.ToString();
                            else if (indexing == 77) item.qian77 = xiangtongindex.ToString();
                            else if (indexing == 78) item.qian78 = xiangtongindex.ToString();
                            else if (indexing == 79) item.qian79 = xiangtongindex.ToString();
                            else if (indexing == 80) item.qian80 = xiangtongindex.ToString();
                            else if (indexing == 81) item.qian81 = xiangtongindex.ToString();
                            else if (indexing == 82) item.qian82 = xiangtongindex.ToString();
                            else if (indexing == 83) item.qian83 = xiangtongindex.ToString();
                            else if (indexing == 84) item.qian84 = xiangtongindex.ToString();
                            else if (indexing == 85) item.qian85 = xiangtongindex.ToString();
                            else if (indexing == 86) item.qian86 = xiangtongindex.ToString();
                            else if (indexing == 87) item.qian87 = xiangtongindex.ToString();
                            else if (indexing == 88) item.qian88 = xiangtongindex.ToString();
                            else if (indexing == 89) item.qian89 = xiangtongindex.ToString();
                            else if (indexing == 90) item.qian90 = xiangtongindex.ToString();
                            else if (indexing == 91) item.qian91 = xiangtongindex.ToString();
                            else if (indexing == 92) item.qian92 = xiangtongindex.ToString();
                            else if (indexing == 93) item.qian93 = xiangtongindex.ToString();
                            else if (indexing == 94) item.qian94 = xiangtongindex.ToString();
                            else if (indexing == 95) item.qian95 = xiangtongindex.ToString();
                            else if (indexing == 96) item.qian96 = xiangtongindex.ToString();
                            else if (indexing == 97) item.qian97 = xiangtongindex.ToString();
                            else if (indexing == 98) item.qian98 = xiangtongindex.ToString();
                            else if (indexing == 99) item.qian99 = xiangtongindex.ToString();
                            //20180630
                            else if (indexing == 100) item.qian100 = xiangtongindex.ToString();
                            else if (indexing == 101) item.qian101 = xiangtongindex.ToString();
                            else if (indexing == 102) item.qian102 = xiangtongindex.ToString();
                            else if (indexing == 103) item.qian103 = xiangtongindex.ToString();
                            else if (indexing == 104) item.qian104 = xiangtongindex.ToString();
                            else if (indexing == 105) item.qian105 = xiangtongindex.ToString();
                            else if (indexing == 106) item.qian106 = xiangtongindex.ToString();
                            else if (indexing == 107) item.qian107 = xiangtongindex.ToString();
                            else if (indexing == 108) item.qian108 = xiangtongindex.ToString();
                            else if (indexing == 109) item.qian109 = xiangtongindex.ToString();
                            else if (indexing == 110) item.qian110 = xiangtongindex.ToString();
                            else if (indexing == 111) item.qian111 = xiangtongindex.ToString();
                            else if (indexing == 112) item.qian112 = xiangtongindex.ToString();
                            else if (indexing == 113) item.qian113 = xiangtongindex.ToString();
                            else if (indexing == 114) item.qian114 = xiangtongindex.ToString();
                            else if (indexing == 115) item.qian115 = xiangtongindex.ToString();
                            else if (indexing == 116) item.qian116 = xiangtongindex.ToString();
                            else if (indexing == 117) item.qian117 = xiangtongindex.ToString();
                            else if (indexing == 118) item.qian118 = xiangtongindex.ToString();
                            else if (indexing == 119) item.qian119 = xiangtongindex.ToString();
                            else if (indexing == 120) item.qian120 = xiangtongindex.ToString();
                            else if (indexing == 121) item.qian121 = xiangtongindex.ToString();
                            else if (indexing == 122) item.qian122 = xiangtongindex.ToString();
                            else if (indexing == 123) item.qian123 = xiangtongindex.ToString();
                            else if (indexing == 124) item.qian124 = xiangtongindex.ToString();
                            else if (indexing == 125) item.qian125 = xiangtongindex.ToString();
                            else if (indexing == 126) item.qian126 = xiangtongindex.ToString();
                            else if (indexing == 127) item.qian127 = xiangtongindex.ToString();
                            else if (indexing == 128) item.qian128 = xiangtongindex.ToString();
                            else if (indexing == 129) item.qian129 = xiangtongindex.ToString();
                            else if (indexing == 130) item.qian130 = xiangtongindex.ToString();
                            else if (indexing == 131) item.qian131 = xiangtongindex.ToString();
                            else if (indexing == 132) item.qian132 = xiangtongindex.ToString();
                            else if (indexing == 133) item.qian133 = xiangtongindex.ToString();
                            else if (indexing == 134) item.qian134 = xiangtongindex.ToString();
                            else if (indexing == 135) item.qian135 = xiangtongindex.ToString();
                            else if (indexing == 136) item.qian136 = xiangtongindex.ToString();
                            else if (indexing == 137) item.qian137 = xiangtongindex.ToString();
                            else if (indexing == 138) item.qian138 = xiangtongindex.ToString();
                            else if (indexing == 139) item.qian139 = xiangtongindex.ToString();
                            else if (indexing == 140) item.qian140 = xiangtongindex.ToString();
                            else if (indexing == 141) item.qian141 = xiangtongindex.ToString();
                            else if (indexing == 142) item.qian142 = xiangtongindex.ToString();
                            else if (indexing == 143) item.qian143 = xiangtongindex.ToString();
                            else if (indexing == 144) item.qian144 = xiangtongindex.ToString();
                            else if (indexing == 145) item.qian145 = xiangtongindex.ToString();
                            else if (indexing == 146) item.qian146 = xiangtongindex.ToString();
                            else if (indexing == 147) item.qian147 = xiangtongindex.ToString();
                            else if (indexing == 148) item.qian148 = xiangtongindex.ToString();
                            else if (indexing == 149) item.qian149 = xiangtongindex.ToString();
                            else if (indexing == 150) item.qian150 = xiangtongindex.ToString();
                            else if (indexing == 151) item.qian151 = xiangtongindex.ToString();
                            else if (indexing == 152) item.qian152 = xiangtongindex.ToString();
                            else if (indexing == 153) item.qian153 = xiangtongindex.ToString();
                            else if (indexing == 154) item.qian154 = xiangtongindex.ToString();
                            else if (indexing == 155) item.qian155 = xiangtongindex.ToString();
                            else if (indexing == 156) item.qian156 = xiangtongindex.ToString();
                            else if (indexing == 157) item.qian157 = xiangtongindex.ToString();
                            else if (indexing == 158) item.qian158 = xiangtongindex.ToString();
                            else if (indexing == 159) item.qian159 = xiangtongindex.ToString();
                            else if (indexing == 160) item.qian160 = xiangtongindex.ToString();
                            else if (indexing == 161) item.qian161 = xiangtongindex.ToString();
                            else if (indexing == 162) item.qian162 = xiangtongindex.ToString();
                            else if (indexing == 163) item.qian163 = xiangtongindex.ToString();
                            else if (indexing == 164) item.qian164 = xiangtongindex.ToString();
                            else if (indexing == 165) item.qian165 = xiangtongindex.ToString();
                            else if (indexing == 166) item.qian166 = xiangtongindex.ToString();
                            else if (indexing == 167) item.qian167 = xiangtongindex.ToString();
                            else if (indexing == 168) item.qian168 = xiangtongindex.ToString();
                            else if (indexing == 169) item.qian169 = xiangtongindex.ToString();
                            else if (indexing == 170) item.qian170 = xiangtongindex.ToString();
                            else if (indexing == 171) item.qian171 = xiangtongindex.ToString();
                            else if (indexing == 172) item.qian172 = xiangtongindex.ToString();
                            else if (indexing == 173) item.qian173 = xiangtongindex.ToString();
                            else if (indexing == 174) item.qian174 = xiangtongindex.ToString();
                            else if (indexing == 175) item.qian175 = xiangtongindex.ToString();
                            else if (indexing == 176) item.qian176 = xiangtongindex.ToString();
                            else if (indexing == 177) item.qian177 = xiangtongindex.ToString();
                            else if (indexing == 178) item.qian178 = xiangtongindex.ToString();
                            else if (indexing == 179) item.qian179 = xiangtongindex.ToString();
                            else if (indexing == 180) item.qian180 = xiangtongindex.ToString();
                            else if (indexing == 181) item.qian181 = xiangtongindex.ToString();
                            else if (indexing == 182) item.qian182 = xiangtongindex.ToString();
                            else if (indexing == 183) item.qian183 = xiangtongindex.ToString();
                            else if (indexing == 184) item.qian184 = xiangtongindex.ToString();
                            else if (indexing == 185) item.qian185 = xiangtongindex.ToString();
                            else if (indexing == 186) item.qian186 = xiangtongindex.ToString();
                            else if (indexing == 187) item.qian187 = xiangtongindex.ToString();
                            else if (indexing == 188) item.qian188 = xiangtongindex.ToString();
                            else if (indexing == 189) item.qian189 = xiangtongindex.ToString();
                            else if (indexing == 190) item.qian190 = xiangtongindex.ToString();
                            else if (indexing == 191) item.qian191 = xiangtongindex.ToString();
                            else if (indexing == 192) item.qian192 = xiangtongindex.ToString();
                            else if (indexing == 193) item.qian193 = xiangtongindex.ToString();
                            else if (indexing == 194) item.qian194 = xiangtongindex.ToString();
                            else if (indexing == 195) item.qian195 = xiangtongindex.ToString();
                            else if (indexing == 196) item.qian196 = xiangtongindex.ToString();
                            else if (indexing == 197) item.qian197 = xiangtongindex.ToString();
                            else if (indexing == 198) item.qian198 = xiangtongindex.ToString();
                            else if (indexing == 199) item.qian199 = xiangtongindex.ToString();
                            else if (indexing == 200) item.qian200 = xiangtongindex.ToString();
                            #endregion
                            if (indexing < 4 && xiangtongxingfenxi == "YES")
                                same_qian3mark(indexing, item, xiangtongindex, same_list);
                        }
                    }
                }
                #region 显示信息

                if (ClaimReport_Server.Count != 0)
                {
                    NewMethod();

                }

                toolStripLabel8.Text = "运行结束";
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

                throw;
            }
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            if (frmSetConfig == null)
            {
                frmSetConfig = new frmSetConfig();
                frmSetConfig.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmSetConfig == null)
            {
                frmSetConfig = new frmSetConfig();
            }
            frmSetConfig.ShowDialog();


            int s = this.tabControl1.SelectedIndex;
            if (s == 0)
            {
                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";
                //GetDataforOutlookThread = new Thread(NewMethodtab1);
                //GetDataforOutlookThread.Start();
                NewMethodtab1();
            }
        }
        void FrmOMS_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sender is frmSetConfig)
            {
                frmSetConfig = null;
            }
            if (sender is frmUDF)
            {

                UDF = new List<int>();
                UDF = frmUDF.JIDTA;

                frmUDF = null;
            }
            if (sender is frmQianQiFenXi_Zidingyifenxi)
            {
                newi = new List<int>();
                newi = frmQianQiFenXi_Zidingyifenxi.newi;

                frmQianQiFenXi_Zidingyifenxi = null;
            }
            if (sender is frmSetConfigList)
            {
                frmSetConfigList = null;
            }
            if (sender is frmSelfPiLiangFangAN)
            {

                checkfanganindex = frmSelfPiLiangFangAN.oncheckinfo;
                frmSelfPiLiangFangAN = null;

            }

        }

        #region 排序
        private class Comp : Comparer<inputCaipiaoDATA>
        {
            public override int Compare(inputCaipiaoDATA item, inputCaipiaoDATA iten1)
            {
                #region 判断是否为汉字
                if (iten1.QiHao != null && iten1.QiHao != "")
                {
                    char[] c = iten1.QiHao.ToCharArray();
                    bool ischina = false;

                    for (int i = 0; i < c.Length; i++)
                    {
                        if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                            ischina = true;
                    }

                    if (ischina == true || Regex.Matches(iten1.QiHao, "[a-zA-Z]").Count > 0)
                    {
                        return 0;
                    }
                }
                else
                    return 0;

                if (iten1.QiHao != null && iten1.QiHao != "")
                {
                    char[] c = item.QiHao.ToCharArray();
                    bool ischina = false;
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                            ischina = true;
                    }
                    if (ischina == true || Regex.Matches(item.QiHao, "[a-zA-Z]").Count > 0)
                    {
                        return 0;
                    }
                }
                else
                    return 0;
                #endregion
                if (item.QiHao == null && item.QiHao == "")
                {
                    //  item.DO_NO = "1";
                    //  return 0;
                    if (iten1.QiHao == null || !iten1.QiHao.Contains("DO"))
                        return int.Parse("0") - int.Parse("0");

                    return int.Parse("0") - int.Parse("0");
                }
                return int.Parse(item.QiHao.Replace("2000", "")) - int.Parse(iten1.QiHao.Replace("2000", ""));
                ;

            }
        }
        private class CompsSmall : Comparer<inputCaipiaoDATA>
        {
            public override int Compare(inputCaipiaoDATA iten1, inputCaipiaoDATA item)
            {
                #region 判断是否为汉字
                if (iten1.QiHao != null && iten1.QiHao != "")
                {
                    char[] c = iten1.QiHao.ToCharArray();
                    bool ischina = false;

                    for (int i = 0; i < c.Length; i++)
                    {
                        if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                            ischina = true;
                    }

                    if (ischina == true || Regex.Matches(iten1.QiHao, "[a-zA-Z]").Count > 0)
                    {
                        return 0;
                    }
                }
                else
                    return 0;

                if (iten1.QiHao != null && iten1.QiHao != "")
                {
                    char[] c = item.QiHao.ToCharArray();
                    bool ischina = false;
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                            ischina = true;
                    }
                    if (ischina == true || Regex.Matches(item.QiHao, "[a-zA-Z]").Count > 0)
                    {
                        return 0;
                    }
                }
                else
                    return 0;
                #endregion
                if (item.QiHao == null && item.QiHao == "")
                {
                    //  item.DO_NO = "1";
                    //  return 0;
                    if (iten1.QiHao == null || !iten1.QiHao.Contains("DO"))
                        return int.Parse("0") - int.Parse("0");

                    return int.Parse("0") - int.Parse("0");
                }
                return int.Parse(item.QiHao.Replace("2000", "")) - int.Parse(iten1.QiHao.Replace("2000", ""));
                ;

            }
        }
        public class SortableBindingList<T> : BindingList<T>
        {
            private bool isSortedCore = true;
            private ListSortDirection sortDirectionCore = ListSortDirection.Ascending;
            private PropertyDescriptor sortPropertyCore = null;
            private string defaultSortItem;

            public SortableBindingList() : base() { }

            public SortableBindingList(IList<T> list) : base(list) { }

            protected override bool SupportsSortingCore
            {
                get { return true; }
            }

            protected override bool SupportsSearchingCore
            {
                get { return true; }
            }

            protected override bool IsSortedCore
            {
                get { return isSortedCore; }
            }

            protected override ListSortDirection SortDirectionCore
            {
                get { return sortDirectionCore; }
            }

            protected override PropertyDescriptor SortPropertyCore
            {
                get { return sortPropertyCore; }
            }

            protected override int FindCore(PropertyDescriptor prop, object key)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (Equals(prop.GetValue(this[i]), key)) return i;
                }
                return -1;
            }

            protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
            {
                isSortedCore = true;
                sortPropertyCore = prop;
                sortDirectionCore = direction;
                Sort();
            }

            protected override void RemoveSortCore()
            {
                if (isSortedCore)
                {
                    isSortedCore = false;
                    sortPropertyCore = null;
                    sortDirectionCore = ListSortDirection.Ascending;
                    Sort();
                }
            }

            public string DefaultSortItem
            {
                get { return defaultSortItem; }
                set
                {
                    if (defaultSortItem != value)
                    {
                        defaultSortItem = value;
                        Sort();
                    }
                }
            }

            private void Sort()
            {
                List<T> list = (this.Items as List<T>);
                list.Sort(CompareCore);
                ResetBindings();
            }

            private int CompareCore(T o1, T o2)
            {
                int ret = 0;
                if (SortPropertyCore != null)
                {
                    ret = CompareValue(SortPropertyCore.GetValue(o1), SortPropertyCore.GetValue(o2), SortPropertyCore.PropertyType);
                }
                if (ret == 0 && DefaultSortItem != null)
                {
                    PropertyInfo property = typeof(T).GetProperty(DefaultSortItem, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.IgnoreCase, null, null, new Type[0], null);
                    if (property != null)
                    {
                        ret = CompareValue(property.GetValue(o1, null), property.GetValue(o2, null), property.PropertyType);
                    }
                }
                if (SortDirectionCore == ListSortDirection.Descending) ret = -ret;
                return ret;
            }

            private static int CompareValue(object o1, object o2, Type type)
            {
                if (o1 == null) return o2 == null ? 0 : -1;
                else if (o2 == null) return 1;
                else if (type.IsPrimitive || type.IsEnum) return Convert.ToDouble(o1).CompareTo(Convert.ToDouble(o2));
                else if (type == typeof(DateTime)) return Convert.ToDateTime(o1).CompareTo(o2);
                else return String.Compare(o1.ToString().Trim(), o2.ToString().Trim());
            }
        }

        #endregion

        private void 自定义分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int s2 = this.tabControl1.SelectedIndex;

            if (s2 == 0)
            {
                if (frmUDF == null)
                {
                    frmUDF = new frmUDF();
                    frmUDF.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
                }
                if (frmUDF == null)
                {
                    frmUDF = new frmUDF();
                }
                frmUDF.ShowDialog();

                if (UDF.Count != 0)
                {


                    int s = this.tabControl1.SelectedIndex;
                    if (s == 0)
                    {
                        clsAllnew BusinessHelp = new clsAllnew();
                        List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");
                        ClaimReport_Server = new List<inputCaipiaoDATA>();
                        ClaimReport_Server = BusinessHelp.ReadclaimreportfromServerBy_Xuan(CaipiaozhongleiResult[0].Name);
                        ClaimReport_Server.Sort(new Comp());

                        // InitialSystemInfo();
                        #region 原始 用  Dav 筛选
                        //   List<inputCaipiaoDATA> ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();
                        #region 添加 基数 和前几期对比

                        List<FangAnLieBiaoDATA> Result = BusinessHelp.Read_FangAn("YES");
                        ClaimReport_Server.Sort(new CompsSmall());
                        foreach (inputCaipiaoDATA item in ClaimReport_Server)
                        {
                            foreach (FangAnLieBiaoDATA temp in Result)
                            {
                                string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.Data, "\r\n");

                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                                for (int ii = 0; ii < temp2.Length; ii++)
                                {
                                    for (int i = 1; i < temp1.Length; i++)
                                    {
                                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp1[i], "段");
                                        int ss = ii + 1;
                                        bool isrun = false;

                                        for (int j = 0; j < UDF.Count; j++)
                                        {
                                            if (UDF[j] == ss)
                                                isrun = true;

                                        }
                                        if (isrun == false)
                                            continue;

                                        //if (temp1[i].Contains(temp2[ii]))
                                        if (temp3[1].Contains(temp2[ii]))
                                        {
                                            item.JiShu = item.JiShu + "基" + ss.ToString() + " " + temp3[0];
                                            if (ss == 1)
                                                item.JiShu1 = temp3[0];
                                            else if (ss == 2)
                                                item.JiShu2 = temp3[0];
                                            else if (ss == 3)
                                                item.JiShu3 = temp3[0];
                                            else if (ss == 4)
                                                item.JiShu4 = temp3[0];
                                            else if (ss == 5)
                                                item.JiShu5 = temp3[0];
                                            else if (ss == 6)
                                                item.JiShu6 = temp3[0];
                                            else if (ss == 7)
                                                item.JiShu7 = temp3[0];
                                            else if (ss == 8)
                                                item.JiShu8 = temp3[0];
                                            else if (ss == 9)
                                                item.JiShu9 = temp3[0];
                                            break;
                                        }
                                    }

                                }
                            }
                        }

                        #endregion

                        //  ClaimReport_Server = new List<inputCaipiaoDATA>();

                        //  ClaimReport_Server.Sort(new Comp());
                        int indexing = 0;
                        foreach (inputCaipiaoDATA item in ClaimReport_Server)
                        {
                            indexing = 0;

                            foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                            {
                                if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao))
                                {
                                    indexing++;
                                    int xiangtongindex = 0;

                                    #region 匹配相同次数
                                    for (int j = 0; j < UDF.Count; j++)
                                    {
                                        if (item.JiShu1 != null && item.JiShu1 == temp.JiShu1 && UDF[j] == 1)
                                            xiangtongindex++;
                                        if (item.JiShu2 != null && item.JiShu2 == temp.JiShu2 && UDF[j] == 2)
                                            xiangtongindex++;
                                        if (item.JiShu3 != null && item.JiShu3 == temp.JiShu3 && UDF[j] == 3)
                                            xiangtongindex++;
                                        if (item.JiShu4 != null && item.JiShu4 == temp.JiShu4 && UDF[j] == 4)
                                            xiangtongindex++;
                                        if (item.JiShu5 != null && item.JiShu5 == temp.JiShu5 && UDF[j] == 5)
                                            xiangtongindex++;
                                        if (item.JiShu6 != null && item.JiShu6 == temp.JiShu6 && UDF[j] == 6)
                                            xiangtongindex++;
                                        if (item.JiShu7 != null && item.JiShu7 == temp.JiShu7 && UDF[j] == 7)
                                            xiangtongindex++;
                                        if (item.JiShu8 != null && item.JiShu8 == temp.JiShu8 && UDF[j] == 8)
                                            xiangtongindex++;
                                        if (item.JiShu9 != null && item.JiShu9 == temp.JiShu9 && UDF[j] == 9)
                                            xiangtongindex++;
                                    }
                                    #endregion

                                    #region MyRegion
                                    if (indexing == 1)
                                        item.qian1 = xiangtongindex.ToString();

                                    else if (indexing == 2) item.qian2 = xiangtongindex.ToString();
                                    else if (indexing == 3) item.qian3 = xiangtongindex.ToString();
                                    else if (indexing == 4) item.qian4 = xiangtongindex.ToString();
                                    else if (indexing == 5) item.qian5 = xiangtongindex.ToString();
                                    else if (indexing == 6) item.qian6 = xiangtongindex.ToString();
                                    else if (indexing == 7) item.qian7 = xiangtongindex.ToString();
                                    else if (indexing == 8) item.qian8 = xiangtongindex.ToString();
                                    else if (indexing == 9) item.qian9 = xiangtongindex.ToString();
                                    else if (indexing == 10) item.qian10 = xiangtongindex.ToString();
                                    else if (indexing == 11) item.qian11 = xiangtongindex.ToString();
                                    else if (indexing == 12) item.qian12 = xiangtongindex.ToString();
                                    else if (indexing == 13) item.qian13 = xiangtongindex.ToString();
                                    else if (indexing == 14) item.qian14 = xiangtongindex.ToString();
                                    else if (indexing == 15) item.qian15 = xiangtongindex.ToString();
                                    else if (indexing == 16) item.qian16 = xiangtongindex.ToString();
                                    else if (indexing == 17) item.qian17 = xiangtongindex.ToString();
                                    else if (indexing == 18) item.qian18 = xiangtongindex.ToString();
                                    else if (indexing == 19) item.qian19 = xiangtongindex.ToString();
                                    else if (indexing == 20) item.qian20 = xiangtongindex.ToString();
                                    else if (indexing == 21) item.qian21 = xiangtongindex.ToString();
                                    else if (indexing == 22) item.qian22 = xiangtongindex.ToString();
                                    else if (indexing == 23) item.qian23 = xiangtongindex.ToString();
                                    else if (indexing == 24) item.qian24 = xiangtongindex.ToString();
                                    else if (indexing == 25) item.qian25 = xiangtongindex.ToString();
                                    else if (indexing == 26) item.qian26 = xiangtongindex.ToString();
                                    else if (indexing == 27) item.qian27 = xiangtongindex.ToString();
                                    else if (indexing == 28) item.qian28 = xiangtongindex.ToString();
                                    else if (indexing == 29) item.qian29 = xiangtongindex.ToString();
                                    else if (indexing == 30) item.qian30 = xiangtongindex.ToString();
                                    else if (indexing == 31) item.qian31 = xiangtongindex.ToString();
                                    else if (indexing == 32) item.qian32 = xiangtongindex.ToString();
                                    else if (indexing == 33) item.qian33 = xiangtongindex.ToString();
                                    else if (indexing == 34) item.qian34 = xiangtongindex.ToString();
                                    else if (indexing == 35) item.qian35 = xiangtongindex.ToString();
                                    else if (indexing == 36) item.qian36 = xiangtongindex.ToString();
                                    else if (indexing == 37) item.qian37 = xiangtongindex.ToString();
                                    else if (indexing == 38) item.qian38 = xiangtongindex.ToString();
                                    else if (indexing == 39) item.qian39 = xiangtongindex.ToString();
                                    else if (indexing == 40) item.qian40 = xiangtongindex.ToString();
                                    else if (indexing == 41) item.qian41 = xiangtongindex.ToString();
                                    else if (indexing == 42) item.qian42 = xiangtongindex.ToString();
                                    else if (indexing == 43) item.qian43 = xiangtongindex.ToString();
                                    else if (indexing == 44) item.qian44 = xiangtongindex.ToString();
                                    else if (indexing == 45) item.qian45 = xiangtongindex.ToString();
                                    else if (indexing == 46) item.qian46 = xiangtongindex.ToString();
                                    else if (indexing == 47) item.qian47 = xiangtongindex.ToString();
                                    else if (indexing == 48) item.qian48 = xiangtongindex.ToString();
                                    else if (indexing == 49) item.qian49 = xiangtongindex.ToString();
                                    else if (indexing == 50) item.qian50 = xiangtongindex.ToString();
                                    else if (indexing == 51) item.qian51 = xiangtongindex.ToString();
                                    else if (indexing == 52) item.qian52 = xiangtongindex.ToString();
                                    else if (indexing == 53) item.qian53 = xiangtongindex.ToString();
                                    else if (indexing == 54) item.qian54 = xiangtongindex.ToString();
                                    else if (indexing == 55) item.qian55 = xiangtongindex.ToString();
                                    else if (indexing == 56) item.qian56 = xiangtongindex.ToString();
                                    else if (indexing == 57) item.qian57 = xiangtongindex.ToString();
                                    else if (indexing == 58) item.qian58 = xiangtongindex.ToString();
                                    else if (indexing == 59) item.qian59 = xiangtongindex.ToString();
                                    else if (indexing == 60) item.qian60 = xiangtongindex.ToString();
                                    else if (indexing == 61) item.qian61 = xiangtongindex.ToString();
                                    else if (indexing == 62) item.qian62 = xiangtongindex.ToString();
                                    else if (indexing == 63) item.qian63 = xiangtongindex.ToString();
                                    else if (indexing == 64) item.qian64 = xiangtongindex.ToString();
                                    else if (indexing == 65) item.qian65 = xiangtongindex.ToString();
                                    else if (indexing == 66) item.qian66 = xiangtongindex.ToString();
                                    else if (indexing == 67) item.qian67 = xiangtongindex.ToString();
                                    else if (indexing == 68) item.qian68 = xiangtongindex.ToString();
                                    else if (indexing == 69) item.qian69 = xiangtongindex.ToString();
                                    else if (indexing == 70) item.qian70 = xiangtongindex.ToString();
                                    else if (indexing == 71) item.qian71 = xiangtongindex.ToString();
                                    else if (indexing == 72) item.qian72 = xiangtongindex.ToString();
                                    else if (indexing == 73) item.qian73 = xiangtongindex.ToString();
                                    else if (indexing == 74) item.qian74 = xiangtongindex.ToString();
                                    else if (indexing == 75) item.qian75 = xiangtongindex.ToString();
                                    else if (indexing == 76) item.qian76 = xiangtongindex.ToString();
                                    else if (indexing == 77) item.qian77 = xiangtongindex.ToString();
                                    else if (indexing == 78) item.qian78 = xiangtongindex.ToString();
                                    else if (indexing == 79) item.qian79 = xiangtongindex.ToString();
                                    else if (indexing == 80) item.qian80 = xiangtongindex.ToString();
                                    else if (indexing == 81) item.qian81 = xiangtongindex.ToString();
                                    else if (indexing == 82) item.qian82 = xiangtongindex.ToString();
                                    else if (indexing == 83) item.qian83 = xiangtongindex.ToString();
                                    else if (indexing == 84) item.qian84 = xiangtongindex.ToString();
                                    else if (indexing == 85) item.qian85 = xiangtongindex.ToString();
                                    else if (indexing == 86) item.qian86 = xiangtongindex.ToString();
                                    else if (indexing == 87) item.qian87 = xiangtongindex.ToString();
                                    else if (indexing == 88) item.qian88 = xiangtongindex.ToString();
                                    else if (indexing == 89) item.qian89 = xiangtongindex.ToString();
                                    else if (indexing == 90) item.qian90 = xiangtongindex.ToString();
                                    else if (indexing == 91) item.qian91 = xiangtongindex.ToString();
                                    else if (indexing == 92) item.qian92 = xiangtongindex.ToString();
                                    else if (indexing == 93) item.qian93 = xiangtongindex.ToString();
                                    else if (indexing == 94) item.qian94 = xiangtongindex.ToString();
                                    else if (indexing == 95) item.qian95 = xiangtongindex.ToString();
                                    else if (indexing == 96) item.qian96 = xiangtongindex.ToString();
                                    else if (indexing == 97) item.qian97 = xiangtongindex.ToString();
                                    else if (indexing == 98) item.qian98 = xiangtongindex.ToString();
                                    else if (indexing == 99) item.qian99 = xiangtongindex.ToString();
                                    //20180630
                                    else if (indexing == 100) item.qian100 = xiangtongindex.ToString();
                                    else if (indexing == 101) item.qian101 = xiangtongindex.ToString();
                                    else if (indexing == 102) item.qian102 = xiangtongindex.ToString();
                                    else if (indexing == 103) item.qian103 = xiangtongindex.ToString();
                                    else if (indexing == 104) item.qian104 = xiangtongindex.ToString();
                                    else if (indexing == 105) item.qian105 = xiangtongindex.ToString();
                                    else if (indexing == 106) item.qian106 = xiangtongindex.ToString();
                                    else if (indexing == 107) item.qian107 = xiangtongindex.ToString();
                                    else if (indexing == 108) item.qian108 = xiangtongindex.ToString();
                                    else if (indexing == 109) item.qian109 = xiangtongindex.ToString();
                                    else if (indexing == 110) item.qian110 = xiangtongindex.ToString();
                                    else if (indexing == 111) item.qian111 = xiangtongindex.ToString();
                                    else if (indexing == 112) item.qian112 = xiangtongindex.ToString();
                                    else if (indexing == 113) item.qian113 = xiangtongindex.ToString();
                                    else if (indexing == 114) item.qian114 = xiangtongindex.ToString();
                                    else if (indexing == 115) item.qian115 = xiangtongindex.ToString();
                                    else if (indexing == 116) item.qian116 = xiangtongindex.ToString();
                                    else if (indexing == 117) item.qian117 = xiangtongindex.ToString();
                                    else if (indexing == 118) item.qian118 = xiangtongindex.ToString();
                                    else if (indexing == 119) item.qian119 = xiangtongindex.ToString();
                                    else if (indexing == 120) item.qian120 = xiangtongindex.ToString();
                                    else if (indexing == 121) item.qian121 = xiangtongindex.ToString();
                                    else if (indexing == 122) item.qian122 = xiangtongindex.ToString();
                                    else if (indexing == 123) item.qian123 = xiangtongindex.ToString();
                                    else if (indexing == 124) item.qian124 = xiangtongindex.ToString();
                                    else if (indexing == 125) item.qian125 = xiangtongindex.ToString();
                                    else if (indexing == 126) item.qian126 = xiangtongindex.ToString();
                                    else if (indexing == 127) item.qian127 = xiangtongindex.ToString();
                                    else if (indexing == 128) item.qian128 = xiangtongindex.ToString();
                                    else if (indexing == 129) item.qian129 = xiangtongindex.ToString();
                                    else if (indexing == 130) item.qian130 = xiangtongindex.ToString();
                                    else if (indexing == 131) item.qian131 = xiangtongindex.ToString();
                                    else if (indexing == 132) item.qian132 = xiangtongindex.ToString();
                                    else if (indexing == 133) item.qian133 = xiangtongindex.ToString();
                                    else if (indexing == 134) item.qian134 = xiangtongindex.ToString();
                                    else if (indexing == 135) item.qian135 = xiangtongindex.ToString();
                                    else if (indexing == 136) item.qian136 = xiangtongindex.ToString();
                                    else if (indexing == 137) item.qian137 = xiangtongindex.ToString();
                                    else if (indexing == 138) item.qian138 = xiangtongindex.ToString();
                                    else if (indexing == 139) item.qian139 = xiangtongindex.ToString();
                                    else if (indexing == 140) item.qian140 = xiangtongindex.ToString();
                                    else if (indexing == 141) item.qian141 = xiangtongindex.ToString();
                                    else if (indexing == 142) item.qian142 = xiangtongindex.ToString();
                                    else if (indexing == 143) item.qian143 = xiangtongindex.ToString();
                                    else if (indexing == 144) item.qian144 = xiangtongindex.ToString();
                                    else if (indexing == 145) item.qian145 = xiangtongindex.ToString();
                                    else if (indexing == 146) item.qian146 = xiangtongindex.ToString();
                                    else if (indexing == 147) item.qian147 = xiangtongindex.ToString();
                                    else if (indexing == 148) item.qian148 = xiangtongindex.ToString();
                                    else if (indexing == 149) item.qian149 = xiangtongindex.ToString();
                                    else if (indexing == 150) item.qian150 = xiangtongindex.ToString();
                                    else if (indexing == 151) item.qian151 = xiangtongindex.ToString();
                                    else if (indexing == 152) item.qian152 = xiangtongindex.ToString();
                                    else if (indexing == 153) item.qian153 = xiangtongindex.ToString();
                                    else if (indexing == 154) item.qian154 = xiangtongindex.ToString();
                                    else if (indexing == 155) item.qian155 = xiangtongindex.ToString();
                                    else if (indexing == 156) item.qian156 = xiangtongindex.ToString();
                                    else if (indexing == 157) item.qian157 = xiangtongindex.ToString();
                                    else if (indexing == 158) item.qian158 = xiangtongindex.ToString();
                                    else if (indexing == 159) item.qian159 = xiangtongindex.ToString();
                                    else if (indexing == 160) item.qian160 = xiangtongindex.ToString();
                                    else if (indexing == 161) item.qian161 = xiangtongindex.ToString();
                                    else if (indexing == 162) item.qian162 = xiangtongindex.ToString();
                                    else if (indexing == 163) item.qian163 = xiangtongindex.ToString();
                                    else if (indexing == 164) item.qian164 = xiangtongindex.ToString();
                                    else if (indexing == 165) item.qian165 = xiangtongindex.ToString();
                                    else if (indexing == 166) item.qian166 = xiangtongindex.ToString();
                                    else if (indexing == 167) item.qian167 = xiangtongindex.ToString();
                                    else if (indexing == 168) item.qian168 = xiangtongindex.ToString();
                                    else if (indexing == 169) item.qian169 = xiangtongindex.ToString();
                                    else if (indexing == 170) item.qian170 = xiangtongindex.ToString();
                                    else if (indexing == 171) item.qian171 = xiangtongindex.ToString();
                                    else if (indexing == 172) item.qian172 = xiangtongindex.ToString();
                                    else if (indexing == 173) item.qian173 = xiangtongindex.ToString();
                                    else if (indexing == 174) item.qian174 = xiangtongindex.ToString();
                                    else if (indexing == 175) item.qian175 = xiangtongindex.ToString();
                                    else if (indexing == 176) item.qian176 = xiangtongindex.ToString();
                                    else if (indexing == 177) item.qian177 = xiangtongindex.ToString();
                                    else if (indexing == 178) item.qian178 = xiangtongindex.ToString();
                                    else if (indexing == 179) item.qian179 = xiangtongindex.ToString();
                                    else if (indexing == 180) item.qian180 = xiangtongindex.ToString();
                                    else if (indexing == 181) item.qian181 = xiangtongindex.ToString();
                                    else if (indexing == 182) item.qian182 = xiangtongindex.ToString();
                                    else if (indexing == 183) item.qian183 = xiangtongindex.ToString();
                                    else if (indexing == 184) item.qian184 = xiangtongindex.ToString();
                                    else if (indexing == 185) item.qian185 = xiangtongindex.ToString();
                                    else if (indexing == 186) item.qian186 = xiangtongindex.ToString();
                                    else if (indexing == 187) item.qian187 = xiangtongindex.ToString();
                                    else if (indexing == 188) item.qian188 = xiangtongindex.ToString();
                                    else if (indexing == 189) item.qian189 = xiangtongindex.ToString();
                                    else if (indexing == 190) item.qian190 = xiangtongindex.ToString();
                                    else if (indexing == 191) item.qian191 = xiangtongindex.ToString();
                                    else if (indexing == 192) item.qian192 = xiangtongindex.ToString();
                                    else if (indexing == 193) item.qian193 = xiangtongindex.ToString();
                                    else if (indexing == 194) item.qian194 = xiangtongindex.ToString();
                                    else if (indexing == 195) item.qian195 = xiangtongindex.ToString();
                                    else if (indexing == 196) item.qian196 = xiangtongindex.ToString();
                                    else if (indexing == 197) item.qian197 = xiangtongindex.ToString();
                                    else if (indexing == 198) item.qian198 = xiangtongindex.ToString();
                                    else if (indexing == 199) item.qian199 = xiangtongindex.ToString();
                                    else if (indexing == 200) item.qian200 = xiangtongindex.ToString();

                                    #endregion

                                }
                            }
                        }
                        #endregion


                        NewMethod();

                        //this.dataGridView1.DataSource = null;
                        //this.dataGridView1.AutoGenerateColumns = false;
                        //if (ClaimReport_Server.Count != 0)
                        //{
                        //    this.dataGridView1.DataSource = ClaimReport_Server;
                        //}
                        //this.toolStripComboBox1.ComboBox.DisplayMember = "QiHao";
                        //this.toolStripComboBox1.ComboBox.ValueMember = "QiHao";
                        //this.toolStripComboBox1.ComboBox.DataSource = ClaimReport_Server;

                        //this.toolStripComboBox2.ComboBox.DisplayMember = "QiHao";
                        //this.toolStripComboBox2.ComboBox.ValueMember = "QiHao";
                        //this.toolStripComboBox2.ComboBox.DataSource = ClaimReport_Server;
                        //this.toolStripComboBox1.SelectedIndex = 0;
                        //this.toolStripComboBox2.SelectedIndex = ClaimReport_Server.Count - 1;
                        //this.toolStripComboBox3.SelectedIndex = 2;
                        //this.toolStripComboBox4.SelectedIndex = 2;


                    }
                }
            }
            else if (s2 == 1)
            {
                if (frmQianQiFenXi_Zidingyifenxi == null)
                {
                    frmQianQiFenXi_Zidingyifenxi = new frmQianQiFenXi_Zidingyifenxi();
                    frmQianQiFenXi_Zidingyifenxi.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
                }
                if (frmQianQiFenXi_Zidingyifenxi == null)
                {
                    frmQianQiFenXi_Zidingyifenxi = new frmQianQiFenXi_Zidingyifenxi();
                }
                frmQianQiFenXi_Zidingyifenxi.ShowDialog();

                ZidingYi_tab2();




            }
        }

        private void toolStripComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            newi = new List<int>();
            qianqi_newi = new List<int>();
            //int s = this.tabControl1.SelectedIndex;
            //if (s == 1)
            int s2 = this.tabControl1.SelectedIndex;

            if (s2 == 1)
            {
                if (clbStatus.CheckedItems.Count > 0)
                {
                    foreach (string status in this.clbStatus.CheckedItems)
                    {
                        newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                    }
                    if (this.checkedListBox1.CheckedItems.Count > 0)
                    {
                        foreach (string status in this.checkedListBox1.CheckedItems)
                        {
                            qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                        }
                    }
                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
                    ZidingYi_tab2();
                }
                else
                {
                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

                    tab2();

                    QianQI_Zidingyi_InitialSystemInfo();


                }
            }
            if (s2 == 2)
            {
                if (checkedListBox4.CheckedItems.Count > 0)
                {
                    foreach (string status in this.checkedListBox4.CheckedItems)
                    {
                        newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                    }
                    if (this.checkedListBox3.CheckedItems.Count > 0)
                    {
                        foreach (string status in this.checkedListBox3.CheckedItems)
                        {
                            qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                        }
                    }
                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
                    //ZidingYi_tab2();
                    chufafenxijishu_ZidingYi_tab3();
                }
                else
                {
                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

                    tab3();

                    JISHU_Zidingyi_InitialSystemInfo();


                    // this.toolStripLabel8.Text = "请选中基数";

                }

                //newi = new List<int>();
                //if (checkedListBox4.CheckedItems.Count > 0)
                //{
                //    foreach (string status in this.checkedListBox4.CheckedItems)
                //    {
                //        newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                //    }
                //}
                //qianqi_newi = new List<int>();
                //if (this.checkedListBox3.CheckedItems.Count > 0)
                //{
                //    foreach (string status in this.checkedListBox3.CheckedItems)
                //    {
                //        qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                //    }
                //}



            }
            else if (s2 == 0)
            {
                //  QihaoCombox_NewMethod();
                // return;

                NewMethod();

            }

        }

        private void NewMethod()
        {
            try
            {
                int s = this.tabControl1.SelectedIndex;
                if (s == 0)
                {
                    //for (int j = 11; j < dataGridView1.ColumnCount; j++)
                    //{
                    //    dataGridView1.Columns[j].Visible = true;
                    //}
                    //int i = 100 - Convert.ToInt32(toolStripComboBox4.Text);

                    ////for (int j = Convert.ToInt32(toolStripComboBox4.Text) + 11; j < i + 14; j++)
                    ////{
                    ////    dataGridView1.Columns[j].Visible = false;

                    ////}
                    //int startHidecloumn = Convert.ToInt32(toolStripComboBox4.Text) + 11;
                    //int totalcloumn = i + startHidecloumn - 1;
                    //for (int j = startHidecloumn; j <= totalcloumn; j++)
                    //{
                    //    dataGridView1.Columns[j].Visible = false;
                    //}

                    //自构造table
                    if (toolStripComboBox4.Text == null || toolStripComboBox4.Text == "")
                        return;

                    var qtyTable = new DataTable();
                    int comvalue = Convert.ToInt32(toolStripComboBox4.Text);

                    qtyTable.Columns.Add("期号", System.Type.GetType("System.String"));
                    qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));

                    int JISHUIN = 0;
                    if (UDF != null && UDF.Count != 0)
                    {
                        UDF.Sort();
                        for (int m = 1; m <= UDF[UDF.Count - 1]; m++)
                        {
                            JISHUIN++;
                            qtyTable.Columns.Add("基" + m, System.Type.GetType("System.String"));
                            //  dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Width = 30, DataPropertyName = "基" + m });

                        }
                        //最新的基数列数
                        changeInitialUDF = new List<int>();
                        changeInitialUDF = UDF;

                    }
                    else
                    {
                        if (InitialUDF.Count == 0)
                            return;

                        if (InitialUDF != null && InitialUDF.Count != 0)
                        {

                            InitialUDF.Sort();
                            for (int m = 1; m <= InitialUDF[InitialUDF.Count - 1]; m++)
                            {

                                qtyTable.Columns.Add("基" + m, System.Type.GetType("System.String"));
                                //  dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Width = 30, DataPropertyName = "基" + m });
                                //   this.checkedListBox2.Items.Add("基" + m);
                            }
                        }

                        // return;

                        //for (int m = 1; m <= 9; m++)
                        //{
                        //    JISHUIN++;
                        //    qtyTable.Columns.Add("基" + m, System.Type.GetType("System.String"));
                        //    dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Width = 30, DataPropertyName = "基" + m });

                        //}
                    }

                    for (int m = 0; m < Convert.ToInt32(toolStripComboBox4.Text); m++)
                    {
                        int ss = m + 1;

                        //qtyTable.Columns.Add("前第+" + ss + "期相同位置", System.Type.GetType("System.String"));
                        //0322 改名
                        qtyTable.Columns.Add("前" + ss, System.Type.GetType("System.String"));
                        //this.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Width = 80, DataPropertyName = "前" + ss });

                    }
                    //  qtyTable.Rows.Add(qtyTable.NewRow());
                    foreach (var k in ClaimReport_Server)
                    {
                        qtyTable.Rows.Add(qtyTable.NewRow());
                    }

                    int jk = 0;
                    int cindex = 12;
                    int jicloumn = 0;
                    //if (UDF != null && UDF.Count != 0)
                    //    jicloumn = 9 - UDF.Count;
                    UDF.Sort();
                    if (UDF != null && UDF.Count != 0)
                        jicloumn = Convert.ToInt32(UDF[UDF.Count - 1]) + 1;
                    else if (InitialUDF != null && InitialUDF.Count != 0)
                        jicloumn = Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) + 1;

                    foreach (var item in ClaimReport_Server)
                    {
                        //cindex = 10 - jicloumn;
                        cindex = jicloumn;
                        if (qtyTable.Columns.Count != 0 && jicloumn == 0)
                            cindex = qtyTable.Columns.Count - 1 - Convert.ToInt32(toolStripComboBox4.Text);
                        {
                            #region MyRegion
                            string allqian = item.qian1 + " " + item.qian2 + " " + item.qian3 + " " + item.qian4 + " " + item.qian5 + " " + item.qian6 + " " + item.qian7 + " " + item.qian8 + " " + item.qian9 + " " + item.qian10 + " " + item.qian11 + " " + item.qian12 + " " + item.qian13 + " " + item.qian14 + " " + item.qian15 + " " + item.qian16 + " " + item.qian17 + " " + item.qian18 + " " + item.qian19 + " " + item.qian20 + " " + item.qian21 + " " + item.qian22 + " " + item.qian23 + " " + item.qian24 + " " + item.qian25 + " " + item.qian26 + " " + item.qian27 + " " + item.qian28 + " " + item.qian29 + " " + item.qian30 + " " + item.qian31 + " " + item.qian32 + " " + item.qian33 + " " + item.qian34 + " " + item.qian35 + " " + item.qian36 + " " + item.qian37 + " " + item.qian38 + " " + item.qian39 + " " + item.qian40 + " " + item.qian41 + " " + item.qian42 + " " + item.qian43 + " " + item.qian44 + " " + item.qian45 + " " + item.qian46 + " " + item.qian47 + " " + item.qian48 + " " + item.qian49 + " " + item.qian50 + " " + item.qian51 + " " + item.qian52 + " " + item.qian53 + " " + item.qian54 + " " + item.qian55 + " " + item.qian56 + " " + item.qian57 + " " + item.qian58 + " " + item.qian59 + " " + item.qian60 + " " + item.qian61 + " " + item.qian62 + " " + item.qian63 + " " + item.qian64 + " " + item.qian65 + " " + item.qian66 + " " + item.qian67 + " " + item.qian68 + " " + item.qian69 + " " + item.qian70 + " " + item.qian71 + " " + item.qian72 + " " + item.qian73 + " " + item.qian74 + " " + item.qian75 + " " + item.qian76 + " " + item.qian77 + " " + item.qian78 + " " + item.qian79 + " " + item.qian80 + " " + item.qian81 + " " + item.qian82 + " " + item.qian83 + " " + item.qian84 + " " + item.qian85 + " " + item.qian86 + " " + item.qian87 + " " + item.qian88 + " " + item.qian89 + " " + item.qian90 + " " + item.qian91 + " " + item.qian92 + " " + item.qian93 + " " + item.qian94 + " " + item.qian95 + " " + item.qian96 + " " + item.qian97 + " " + item.qian98 + " " + item.qian99 + " " + item.qian100 + " "
                                                + item.qian101 + " "
                                                + item.qian102 + " "
                                                + item.qian103 + " "
                                                + item.qian104 + " "
                                                + item.qian105 + " "
                                                + item.qian106 + " "
                                                + item.qian107 + " "
                                                + item.qian108 + " "
                                                + item.qian109 + " "
                                                + item.qian110 + " "
                                                + item.qian111 + " "
                                                + item.qian112 + " "
                                                + item.qian113 + " "
                                                + item.qian114 + " "
                                                + item.qian115 + " "
                                                + item.qian116 + " "
                                                + item.qian117 + " "
                                                + item.qian118 + " "
                                                + item.qian119 + " "
                                                + item.qian120 + " "
                                                + item.qian121 + " "
                                                + item.qian122 + " "
                                                + item.qian123 + " "
                                                + item.qian124 + " "
                                                + item.qian125 + " "
                                                + item.qian126 + " "
                                                + item.qian127 + " "
                                                + item.qian128 + " "
                                                + item.qian129 + " "
                                                + item.qian130 + " "
                                                + item.qian131 + " "
                                                + item.qian132 + " "
                                                + item.qian133 + " "
                                                + item.qian134 + " "
                                                + item.qian135 + " "
                                                + item.qian136 + " "
                                                + item.qian137 + " "
                                                + item.qian138 + " "
                                                + item.qian139 + " "
                                                + item.qian140 + " "
                                                + item.qian141 + " "
                                                + item.qian142 + " "
                                                + item.qian143 + " "
                                                + item.qian144 + " "
                                                + item.qian145 + " "
                                                + item.qian146 + " "
                                                + item.qian147 + " "
                                                + item.qian148 + " "
                                                + item.qian149 + " "
                                                + item.qian150 + " "
                                                + item.qian151 + " "
                                                + item.qian152 + " "
                                                + item.qian153 + " "
                                                + item.qian154 + " "
                                                + item.qian155 + " "
                                                + item.qian156 + " "
                                                + item.qian157 + " "
                                                + item.qian158 + " "
                                                + item.qian159 + " "
                                                + item.qian160 + " "
                                                + item.qian161 + " "
                                                + item.qian162 + " "
                                                + item.qian163 + " "
                                                + item.qian164 + " "
                                                + item.qian165 + " "
                                                + item.qian166 + " "
                                                + item.qian167 + " "
                                                + item.qian168 + " "
                                                + item.qian169 + " "
                                                + item.qian170 + " "
                                                + item.qian171 + " "
                                                + item.qian172 + " "
                                                + item.qian173 + " "
                                                + item.qian174 + " "
                                                + item.qian175 + " "
                                                + item.qian176 + " "
                                                + item.qian177 + " "
                                                + item.qian178 + " "
                                                + item.qian179 + " "
                                                + item.qian180 + " "
                                                + item.qian181 + " "
                                                + item.qian182 + " "
                                                + item.qian183 + " "
                                                + item.qian184 + " "
                                                + item.qian185 + " "
                                                + item.qian186 + " "
                                                + item.qian187 + " "
                                                + item.qian188 + " "
                                                + item.qian189 + " "
                                                + item.qian190 + " "
                                                + item.qian191 + " "
                                                + item.qian192 + " "
                                                + item.qian193 + " "
                                                + item.qian194 + " "
                                                + item.qian195 + " "
                                                + item.qian196 + " "
                                                + item.qian197 + " "
                                                + item.qian198 + " "
                                                + item.qian199 + " "
                                                + item.qian200 + " "
                                                ;
                            #endregion


                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(allqian, " ");
                            for (int i = 0; i < temp1.Length; i++)
                            {
                                cindex++;
                                if (i >= comvalue)
                                    break;
                                qtyTable.Rows[jk][cindex] = temp1[i];
                            }
                        }
                        qtyTable.Rows[jk][0] = item.QiHao;
                        qtyTable.Rows[jk][1] = item.KaiJianHaoMa;
                        if (UDF != null && UDF.Count != 0)
                        {
                            for (int m = 0; m < UDF.Count; m++)
                            {
                                if (UDF[m] == 1)
                                    qtyTable.Rows[jk][2] = item.JiShu1;
                                if (UDF[m] == 2)
                                    qtyTable.Rows[jk][3] = item.JiShu2;
                                if (UDF[m] == 3)
                                    qtyTable.Rows[jk][4] = item.JiShu3;
                                if (UDF[m] == 4)
                                    qtyTable.Rows[jk][5] = item.JiShu4;
                                if (UDF[m] == 5)
                                    qtyTable.Rows[jk][6] = item.JiShu5;
                                if (UDF[m] == 6)
                                    qtyTable.Rows[jk][7] = item.JiShu6;
                                if (UDF[m] == 7)
                                    qtyTable.Rows[jk][8] = item.JiShu7;
                                if (UDF[m] == 8)
                                    qtyTable.Rows[jk][9] = item.JiShu8;
                                if (UDF[m] == 9)
                                    qtyTable.Rows[jk][10] = item.JiShu9;

                                if (UDF[m] == 10)
                                    qtyTable.Rows[jk][11] = item.JiShu10;

                                if (UDF[m] == 11)
                                    qtyTable.Rows[jk][12] = item.JiShu11;

                                if (UDF[m] == 12)
                                    qtyTable.Rows[jk][13] = item.JiShu12;

                                if (UDF[m] == 13)
                                    qtyTable.Rows[jk][14] = item.JiShu13;

                                if (UDF[m] == 14)
                                    qtyTable.Rows[jk][15] = item.JiShu14;

                                if (UDF[m] == 15)
                                    qtyTable.Rows[jk][16] = item.JiShu15;
                                //new0621
                                if (UDF[m] == 16)
                                    qtyTable.Rows[jk][17] = item.JiShu16;

                                if (UDF[m] == 17)
                                    qtyTable.Rows[jk][18] = item.JiShu17;

                                if (UDF[m] == 18)
                                    qtyTable.Rows[jk][19] = item.JiShu18;

                                if (UDF[m] == 19)
                                    qtyTable.Rows[jk][20] = item.JiShu19;

                                if (UDF[m] == 20)
                                    qtyTable.Rows[jk][21] = item.JiShu20;

                                if (UDF[m] == 21)
                                    qtyTable.Rows[jk][22] = item.JiShu21;

                                if (UDF[m] == 22)
                                    qtyTable.Rows[jk][23] = item.JiShu22;

                                if (UDF[m] == 23)
                                    qtyTable.Rows[jk][24] = item.JiShu23;

                                if (UDF[m] == 24)
                                    qtyTable.Rows[jk][25] = item.JiShu24;

                                if (UDF[m] == 25)
                                    qtyTable.Rows[jk][26] = item.JiShu25;

                                if (UDF[m] == 26)
                                    qtyTable.Rows[jk][27] = item.JiShu26;

                                if (UDF[m] == 27)
                                    qtyTable.Rows[jk][28] = item.JiShu27;

                                if (UDF[m] == 28)
                                    qtyTable.Rows[jk][29] = item.JiShu28;

                                if (UDF[m] == 29)
                                    qtyTable.Rows[jk][30] = item.JiShu29;

                                if (UDF[m] == 30)
                                    qtyTable.Rows[jk][31] = item.JiShu30;

                                if (UDF[m] == 31)
                                    qtyTable.Rows[jk][32] = item.JiShu31;
                                if (UDF[m] == 32)
                                    qtyTable.Rows[jk][33] = item.JiShu32;
                                if (UDF[m] == 33)
                                    qtyTable.Rows[jk][34] = item.JiShu33;
                                if (UDF[m] == 34)
                                    qtyTable.Rows[jk][35] = item.JiShu34;
                                if (UDF[m] == 35)
                                    qtyTable.Rows[jk][36] = item.JiShu35;
                                if (UDF[m] == 36)
                                    qtyTable.Rows[jk][37] = item.JiShu36;
                                if (UDF[m] == 37)
                                    qtyTable.Rows[jk][38] = item.JiShu37;
                                if (UDF[m] == 38)
                                    qtyTable.Rows[jk][39] = item.JiShu38;
                                if (UDF[m] == 39)
                                    qtyTable.Rows[jk][40] = item.JiShu39;
                                if (UDF[m] == 40)
                                    qtyTable.Rows[jk][41] = item.JiShu40;
                                if (UDF[m] == 41)
                                    qtyTable.Rows[jk][42] = item.JiShu41;
                                if (UDF[m] == 42)
                                    qtyTable.Rows[jk][43] = item.JiShu42;
                                if (UDF[m] == 43)
                                    qtyTable.Rows[jk][44] = item.JiShu43;
                                if (UDF[m] == 44)
                                    qtyTable.Rows[jk][45] = item.JiShu44;
                                if (UDF[m] == 45)
                                    qtyTable.Rows[jk][46] = item.JiShu45;
                                if (UDF[m] == 46)
                                    qtyTable.Rows[jk][47] = item.JiShu46;
                                if (UDF[m] == 47)
                                    qtyTable.Rows[jk][48] = item.JiShu47;
                                if (UDF[m] == 48)
                                    qtyTable.Rows[jk][49] = item.JiShu48;
                                if (UDF[m] == 49)
                                    qtyTable.Rows[jk][50] = item.JiShu49;
                                if (UDF[m] == 50)
                                    qtyTable.Rows[jk][51] = item.JiShu50;


                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 0)
                                qtyTable.Rows[jk][2] = item.JiShu1;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 1)
                                qtyTable.Rows[jk][3] = item.JiShu2;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 2)
                                qtyTable.Rows[jk][4] = item.JiShu3;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 3)
                                qtyTable.Rows[jk][5] = item.JiShu4;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 4)
                                qtyTable.Rows[jk][6] = item.JiShu5;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 5)
                                qtyTable.Rows[jk][7] = item.JiShu6;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 6)
                                qtyTable.Rows[jk][8] = item.JiShu7;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 7)
                                qtyTable.Rows[jk][9] = item.JiShu8;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 8)
                                qtyTable.Rows[jk][10] = item.JiShu9;
                            //new 
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 9)
                                qtyTable.Rows[jk][11] = item.JiShu10;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 10)
                                qtyTable.Rows[jk][12] = item.JiShu11;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 11)
                                qtyTable.Rows[jk][13] = item.JiShu12;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 12)
                                qtyTable.Rows[jk][14] = item.JiShu13;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 13)
                                qtyTable.Rows[jk][15] = item.JiShu14;


                            //new 0621
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 14)
                                qtyTable.Rows[jk][16] = item.JiShu15;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 15)
                                qtyTable.Rows[jk][17] = item.JiShu16;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 16)
                                qtyTable.Rows[jk][18] = item.JiShu17;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 17)
                                qtyTable.Rows[jk][19] = item.JiShu18;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 18)
                                qtyTable.Rows[jk][20] = item.JiShu19;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 19)
                                qtyTable.Rows[jk][21] = item.JiShu20;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 20)
                                qtyTable.Rows[jk][22] = item.JiShu21;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 21)
                                qtyTable.Rows[jk][23] = item.JiShu22;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 22)
                                qtyTable.Rows[jk][24] = item.JiShu23;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 23)
                                qtyTable.Rows[jk][25] = item.JiShu24;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 24)
                                qtyTable.Rows[jk][26] = item.JiShu25;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 25)
                                qtyTable.Rows[jk][27] = item.JiShu26;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 26)
                                qtyTable.Rows[jk][28] = item.JiShu27;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 27)
                                qtyTable.Rows[jk][29] = item.JiShu28;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 28)
                                qtyTable.Rows[jk][30] = item.JiShu29;
                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 29)
                                qtyTable.Rows[jk][31] = item.JiShu30;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 30)
                                qtyTable.Rows[jk][32] = item.JiShu31;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 31)
                                qtyTable.Rows[jk][33] = item.JiShu32;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 32)
                                qtyTable.Rows[jk][34] = item.JiShu33;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 33)
                                qtyTable.Rows[jk][35] = item.JiShu34;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 34)
                                qtyTable.Rows[jk][36] = item.JiShu35;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 35)
                                qtyTable.Rows[jk][37] = item.JiShu36;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 36)
                                qtyTable.Rows[jk][38] = item.JiShu37;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 37)
                                qtyTable.Rows[jk][39] = item.JiShu38;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 38)
                                qtyTable.Rows[jk][40] = item.JiShu39;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 39)
                                qtyTable.Rows[jk][41] = item.JiShu40;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 40)
                                qtyTable.Rows[jk][42] = item.JiShu41;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 41)
                                qtyTable.Rows[jk][43] = item.JiShu42;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 42)
                                qtyTable.Rows[jk][44] = item.JiShu43;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 43)
                                qtyTable.Rows[jk][45] = item.JiShu44;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 44)
                                qtyTable.Rows[jk][46] = item.JiShu45;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 45)
                                qtyTable.Rows[jk][47] = item.JiShu46;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 46)
                                qtyTable.Rows[jk][48] = item.JiShu47;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 47)
                                qtyTable.Rows[jk][49] = item.JiShu48;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 48)
                                qtyTable.Rows[jk][50] = item.JiShu49;

                            if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 49)
                                qtyTable.Rows[jk][51] = item.JiShu50;





                        }
                        // qtyTable.Rows[1][4] = item.QiHao;
                        jk++;
                    }
                    //公有变量
                    Combox_qtyTable = qtyTable;

                    //清空自定义的列数
                    this.dataGridView1.DataSource = null;
                    //  this.dataGridView1.AutoGenerateColumns = false;
                    this.bindingSource2.DataSource = qtyTable;
                    bindingSource2.Sort = "期号  ASC";
                    this.dataGridView1.DataSource = this.bindingSource2;
                    ////设置【前*】列宽
                    int qiancout = Convert.ToInt32(toolStripComboBox4.Text);

                    #region 按照固定宽度值设置

                    //if (UDF != null && UDF.Count != 0)
                    //{

                    //    for (int j = 2; j < UDF[UDF.Count - 1] + 2 + qiancout; j++)
                    //    {

                    //        dataGridView1.Columns[j].Width = 30;
                    //    }
                    //}
                    //else if (InitialUDF != null && InitialUDF.Count != 0)
                    //{
                    //    for (int j = 2; j < InitialUDF[InitialUDF.Count - 1] + 2 + qiancout; j++)
                    //    {

                    //        dataGridView1.Columns[j].Width = 30;
                    //    }
                    //} 
                    #endregion

                    #region 按照抬头设置宽度  &第一二 列 按照 内同定义宽度
                    this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    this.dataGridView1.MultiSelect = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    #endregion
                    if (dataGridView1.Rows.Count != 0)
                    {
                        int ii = dataGridView1.Rows.Count - 1;
                        dataGridView1.CurrentCell = dataGridView1[0, ii]; // 强制将光标指向i行
                        dataGridView1.Rows[ii].Selected = true;   //光标显示至i行 
                    }

                    //else
                    //{
                    //    for (int j = 2; j < 11; j++)
                    //    {
                    //        if (j < dataGridView1.ColumnCount - Convert.ToInt32(toolStripComboBox4.Text))
                    //            dataGridView1.Columns[j].Width = 30;
                    //    }
                    //}
                    UDF = new List<int>();
                }
                else if (s == 1)
                {

                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
                    tab2();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据构造失败，请检查数据源", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
                throw;
            }
        }

        private void 下载当前界面数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int s = this.tabControl1.SelectedIndex;
            if (s == 0)
            {

                {
                    if (this.dataGridView1.Rows.Count == 0)
                    {
                        MessageBox.Show("当前界面没有数据，请确认 !", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = ".csv";
                    saveFileDialog.Filter = "csv|*.csv";
                    string strFileName = "Data " + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    saveFileDialog.FileName = strFileName;
                    if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        strFileName = saveFileDialog.FileName.ToString();
                    }
                    else
                    {
                        return;
                    }
                    FileStream fa = new FileStream(strFileName, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fa, Encoding.Unicode);
                    string delimiter = "\t";
                    string strHeader = "";
                    for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                    {
                        strHeader += this.dataGridView1.Columns[i].HeaderText + delimiter;
                    }
                    sw.WriteLine(strHeader);

                    //output rows data
                    for (int j = 0; j < this.dataGridView1.Rows.Count; j++)
                    {
                        string strRowValue = "";

                        for (int k = 0; k < this.dataGridView1.Columns.Count; k++)
                        {
                            if (this.dataGridView1.Rows[j].Cells[k].Value != null)
                            {
                                strRowValue += this.dataGridView1.Rows[j].Cells[k].Value.ToString().Replace("\r\n", " ").Replace("\n", "") + delimiter;
                                if (this.dataGridView1.Rows[j].Cells[k].Value.ToString() == "LIP201507-35")
                                {

                                }

                            }
                            else
                            {
                                strRowValue += this.dataGridView1.Rows[j].Cells[k].Value + delimiter;
                            }
                        }
                        sw.WriteLine(strRowValue);
                    }
                    sw.Close();
                    fa.Close();
                    MessageBox.Show("下载完成！", "保存", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

            }
            else if (s == 1)
            {
                {
                    if (this.dataGridView2.Rows.Count == 0)
                    {
                        MessageBox.Show("当前界面没有数据，请确认  !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = ".csv";
                    saveFileDialog.Filter = "csv|*.csv";
                    string strFileName = "Data" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    saveFileDialog.FileName = strFileName;
                    if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        strFileName = saveFileDialog.FileName.ToString();
                    }
                    else
                    {
                        return;
                    }
                    FileStream fa = new FileStream(strFileName, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fa, Encoding.Unicode);
                    string delimiter = "\t";
                    string strHeader = "";
                    for (int i = 0; i < this.dataGridView2.Columns.Count; i++)
                    {
                        strHeader += this.dataGridView2.Columns[i].HeaderText + delimiter;
                    }
                    sw.WriteLine(strHeader);

                    //output rows data
                    for (int j = 0; j < this.dataGridView2.Rows.Count; j++)
                    {
                        string strRowValue = "";

                        for (int k = 0; k < this.dataGridView2.Columns.Count; k++)
                        {
                            if (this.dataGridView2.Rows[j].Cells[k].Value != null)
                                strRowValue += this.dataGridView2.Rows[j].Cells[k].Value.ToString().Replace("\r\n", " ") + delimiter;
                            else
                                strRowValue += this.dataGridView2.Rows[j].Cells[k].Value + delimiter;
                        }
                        sw.WriteLine(strRowValue);
                    }

                    sw.Close();
                    fa.Close();
                    MessageBox.Show("下载完成！", "保存", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "")
                return;

            clsAllnew BusinessHelp = new clsAllnew();

            ClaimReport_Server = BusinessHelp.Fast_FindData(textBox1.Text.Trim().ToString(), this.label1.Text);


            try
            {

                //ClaimReport_Server = new List<inputCaipiaoDATA>();
                int s = this.tabControl1.SelectedIndex;
                //if (s == 0)
                //{
                //    NewMethodtab1(BusinessHelp);

                //}
                //else if (s == 2)
                //{
                //    tab2(BusinessHelp);
                //}
                if (s == 0)
                {
                    //toolStripLabel7.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";
                    //GetDataforOutlookThread = new Thread(NewMethodtab1);
                    //GetDataforOutlookThread.Start();
                    NewMethodtab1();

                }
                else if (s == 1)
                {
                    tab2();
                    //toolStripLabel7.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";
                    //GetDataforOutlookThread = new Thread(tab2);
                    //GetDataforOutlookThread.Start();
                    //// tab2(BusinessHelp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return;

                throw;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripLabel8.Text = "系统在拼命刷新......";
            Refreshdata();
            toolStripLabel8.Text = "刷新完成";
            return;

            {
                try
                {

                    InitialBackGroundWorker();
                    //    bgWorker.DoWork += new DoWorkEventHandler(Refreshdata);

                    bgWorker.RunWorkerAsync();

                    // 启动消息显示画面
                    frmMessageShow = new frmMessageShow(clsShowMessage.MSG_001,
                                                        clsShowMessage.MSG_007,
                                                        clsConstant.Dialog_Status_Disable);
                    frmMessageShow.ShowDialog();

                    // 数据读取成功后在画面显示
                    if (blnBackGroundWorkIsOK)
                    {

                    }
                }
                catch (Exception ex)
                {
                    return;
                    throw ex;
                }
            }

        }
        private void Refreshdata()
        {
            ClaimReport_Server = new List<inputCaipiaoDATA>();

            clsAllnew BusinessHelp = new clsAllnew();

            DateTime oldDate = DateTime.Now;

            // InitialSystemInfo();
            //ClaimReport_Server = new List<inputCaipiaoDATA>();
            //ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();
            //ClaimReport_Server.Sort(new Comp());

            //sortablePendingOrderList = new SortableBindingList<inputCaipiaoDATA>(ClaimReport_Server);
            List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");
            this.label1.Text = CaipiaozhongleiResult[0].Name;//"当前彩票类型：" + 
            //+"如数据或设置不能刷新请关闭本界面并重新在主界面打开"


            ClaimReport_Server = new List<inputCaipiaoDATA>();
            ClaimReport_Server = BusinessHelp.ReadclaimreportfromServerBy_Xuan(CaipiaozhongleiResult[0].Name);
            ClaimReport_Server.Sort(new Comp());
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 0)
            {
                checkedListBox2.Items.Clear();
                comboBox3.Items.Clear();
                InitialSystemInfo();

                this.checkedListBox2.Items.Clear();
                changeInitialUDF = new List<int>();
                changeInitialUDF = InitialUDF;

                for (int m = 1; m <= InitialUDF[InitialUDF.Count - 1]; m++)
                {
                    this.comboBox3.Items.Add("随机 " + m + " 位");
                    this.checkedListBox2.Items.Add("基" + m);
                }
                //  this.checkedListBox2.Items.Add("特别号");
                this.comboBox3.SelectedIndex = 0;
                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, false);
                }
            }
            else if (sq == 1)
            {
                this.clbStatus.Items.Clear();
                this.checkedListBox1.Items.Clear();
                toolStripComboBox4.Items.Clear();
                comboBox1.Items.Clear();
                comboBox2.Items.Clear();
                for (int i = 1; i <= 2000; i++)
                {
                    toolStripComboBox4.Items.Add(i);

                }
                toolStripComboBox4.SelectedIndex = 4;

                qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";

                //GetDataforOutlookThread = new Thread(tab2);
                //GetDataforOutlookThread.Start();
                // tab2(BusinessHelp);
                tab2();

                QianQI_Zidingyi_InitialSystemInfo();

                //new  
                this.toolStripComboBox5.SelectedIndex = 0;
                this.toolStripComboBox6.SelectedIndex = 0;
                this.comboBox1.SelectedIndex = 0;
                this.comboBox2.SelectedIndex = 0;

                for (int i = 0; i < clbStatus.Items.Count; i++)
                {
                    clbStatus.SetItemChecked(i, false);
                }
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }

            }
            else if (sq == 2)
            {
                tab3shuiji = false;
                InitialSystemInfo();
                this.checkedListBox4.Items.Clear();
                this.checkedListBox3.Items.Clear();

                comboBox5.Items.Clear();
                comboBox4.Items.Clear();


                toolStripComboBox4.Items.Clear();
                for (int i = 1; i <= 2000; i++)
                {
                    toolStripComboBox4.Items.Add(i);

                }
                toolStripComboBox4.SelectedIndex = 4;

                qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";


                tab3();

                JISHU_Zidingyi_InitialSystemInfo();

                this.toolStripComboBox5.SelectedIndex = 0;
                this.toolStripComboBox6.SelectedIndex = 0;
                this.comboBox5.SelectedIndex = 0;
                this.comboBox4.SelectedIndex = 0;

                for (int i = 0; i < checkedListBox4.Items.Count; i++)
                {
                    checkedListBox4.SetItemChecked(i, false);
                }
                for (int i = 0; i < checkedListBox3.Items.Count; i++)
                {
                    checkedListBox3.SetItemChecked(i, false);
                }
            }

            return;



            DateTime FinishTime = DateTime.Now;
            TimeSpan s = DateTime.Now - oldDate;
            string timei = s.Minutes.ToString() + ":" + s.Seconds.ToString();
            string Showtime = clsShowMessage.MSG_029 + timei.ToString();
            bgWorker.ReportProgress(clsConstant.Thread_Progress_OK, clsShowMessage.MSG_009 + "\r\n" + Showtime);
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            int s = this.tabControl1.SelectedIndex;

            if (s == 0)
            {

            }
        }

        private void toolStripComboBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int s = this.tabControl1.SelectedIndex;
            string shipper = this.toolStripComboBox1.Text;
            string county = toolStripComboBox2.Text;

            if (s == 0)
            {

                ApplyBindSourceFilter(shipper, county);
                //string s = this.toolStripComboBox2.Text;
                //string s1 = this.toolStripComboBox1.Text;
                //if (s == "全部" || s1 == "全部")
                //{
                //    this.dataGridView1.DataSource = this.bindingSource1;
                //}
                //else
                //{
                //    //bindingSource1.Filter = "QiHao >=" + s1 + "QiHao <=" + s;
                //    bindingSource1.Filter = "期号='20161216'";
                //    //sortablePendingOrderList.Where(s3 => Convert.ToInt32(s3.QiHao) >= Convert.ToInt32(s1) && Convert.ToInt32(s3.QiHao) <= Convert.ToInt32(s));
                //    dataGridView1.DataSource = bindingSource1;

                //}
            }
            else if (s == 1)
                ApplyBindSourceFilter1(shipper, county);
            else if (s == 2)
                ApplyBindSourceFilter2(shipper, county);
        }
        private void ApplyBindSourceFilter(string shipper, string county = "", string store = "")
        {
            try
            {

                //if (bindingSource1.Count > 0)
                {
                    string filter = "";
                    if (shipper.Length > 0)
                    {
                        filter += " (期号>='" + shipper + "')";
                    }

                    if (county.Length > 0 && county != "")
                    {
                        if (filter.Length > 0)
                        {
                            filter += " AND ";
                        }
                        filter += "(期号<=" + "'" + county + "'" + ")";
                    }
                    if (ClaimReport_Server.Count > 0)
                    {
                        this.dataGridView1.DataSource = null;

                        bindingSource2.Filter = filter;
                        this.dataGridView1.DataSource = bindingSource2;
                        if (dataGridView1.Rows.Count != 0)
                        {
                            int ii = dataGridView1.Rows.Count - 1;
                            dataGridView1.CurrentCell = dataGridView1[0, ii]; // 强制将光标指向i行
                            dataGridView1.Rows[ii].Selected = true;   //光标显示至i行 
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("刷新异常或数据有误，请关闭当前页面重新尝试", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;


                throw;
            }
        }
        private void ApplyBindSourceFilter1(string shipper, string county = "", string store = "")
        {
            try
            {

                //if (bindingSource1.Count > 0)
                {
                    string filter = "";
                    if (shipper.Length > 0)
                    {
                        filter += " (期号>='" + shipper + "')";
                    }

                    if (county.Length > 0 && county != "")
                    {
                        if (filter.Length > 0)
                        {
                            filter += " AND ";
                        }
                        filter += "(期号<=" + "'" + county + "'" + ")";
                    }
                    if (ClaimReport_Server.Count > 0)
                    {
                        this.dataGridView2.DataSource = null;

                        bindingSource1.Filter = filter;
                        this.dataGridView2.DataSource = bindingSource1;
                        if (dataGridView2.Rows.Count != 0)
                        {
                            int ii = dataGridView2.Rows.Count - 1;
                            dataGridView2.CurrentCell = dataGridView2[0, ii]; // 强制将光标指向i行
                            dataGridView2.Rows[ii].Selected = true;   //光标显示至i行 
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("刷新异常或数据有误，请关闭当前页面重新尝试", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;


                throw;
            }
        }
        private void ApplyBindSourceFilter2(string shipper, string county = "", string store = "")
        {
            try
            {

                //if (bindingSource1.Count > 0)
                {
                    string filter = "";
                    if (shipper.Length > 0)
                    {
                        filter += " (期号>='" + shipper + "')";
                    }

                    if (county.Length > 0 && county != "")
                    {
                        if (filter.Length > 0)
                        {
                            filter += " AND ";
                        }
                        filter += "(期号<=" + "'" + county + "'" + ")";
                    }
                    if (ClaimReport_Server.Count > 0)
                    {
                        this.dataGridView3.DataSource = null;

                        bindingSource1.Filter = filter;
                        this.dataGridView3.DataSource = bindingSource1;
                        for (int j = 3; j < dataGridView3.ColumnCount; j++)
                        {

                            dataGridView3.Columns[j].Width = 30;
                        }


                        if (dataGridView3.Rows.Count != 0)
                        {
                            int ii = dataGridView3.Rows.Count - 1;
                            dataGridView3.CurrentCell = dataGridView3[0, ii]; // 强制将光标指向i行
                            dataGridView3.Rows[ii].Selected = true;   //光标显示至i行 
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("刷新异常或数据有误，请关闭当前页面重新尝试", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;


                throw;
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //this.checkBox1.Checked = false;
            //this.checkBox2.Checked = false;
            //this.checkBox3.Checked = false;
            //this.checkBox4.Checked = false;
            //this.checkBox5.Checked = false;
            //this.checkBox6.Checked = false;
            //this.checkBox7.Checked = false;
            //this.checkBox8.Checked = false;
            //this.checkBox9.Checked = false;
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, false);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            JIDTA = new List<int>();

            //if (checkBox1.Checked == true)
            //    JIDTA.Add(1);
            //if (checkBox3.Checked == true)
            //    JIDTA.Add(2);
            //if (checkBox2.Checked == true)
            //    JIDTA.Add(3);
            //if (checkBox4.Checked == true)
            //    JIDTA.Add(4);
            //if (checkBox5.Checked == true)
            //    JIDTA.Add(5);
            //if (checkBox6.Checked == true)
            //    JIDTA.Add(6);
            //if (checkBox7.Checked == true)
            //    JIDTA.Add(7);
            //if (checkBox8.Checked == true)
            //    JIDTA.Add(8);
            //if (checkBox9.Checked == true)
            //    JIDTA.Add(9);
            //if (checkBox10.Checked == true)
            //    JIDTA.Add(10);
            if (checkedListBox2.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox2.CheckedItems)
                {
                    if (status.Contains("基"))
                    {
                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, "基");
                        JIDTA.Add(Convert.ToInt32(temp3[1]));
                    }
                    else if (status.Contains("特"))
                    {
                        //string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, "特别号");
                        JIDTA.Add(10);
                    }
                }


            }

            //newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
            //string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox2.Text, " ");
            //string index = temp3[1];
            //for (int i = 0; i < Convert.ToInt32(index); i++)
            //{

            //    checkedListBox1.SetItemChecked(newlist[i], true);
            //}


            if (JIDTA.Count > 0)
            {
                UDF = new List<int>();
                UDF = JIDTA;

                if (UDF.Count != 0)
                {


                    int s = this.tabControl1.SelectedIndex;
                    if (s == 0)
                    {
                        clsAllnew BusinessHelp = new clsAllnew();
                        List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");
                        ClaimReport_Server = new List<inputCaipiaoDATA>();
                        ClaimReport_Server = BusinessHelp.ReadclaimreportfromServerBy_Xuan(CaipiaozhongleiResult[0].Name);
                        ClaimReport_Server.Sort(new Comp());

                        // InitialSystemInfo();
                        #region 原始 用  Dav 筛选
                        //   List<inputCaipiaoDATA> ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();
                        #region 添加 基数 和前几期对比



                        List<FangAnLieBiaoDATA> Result = BusinessHelp.Read_FangAn("YES");
                        ClaimReport_Server.Sort(new CompsSmall());
                        foreach (inputCaipiaoDATA item in ClaimReport_Server)
                        {
                            foreach (FangAnLieBiaoDATA temp in Result)
                            {
                                if (temp.xiangtongxingfenxi != null && temp.xiangtongxingfenxi == "YES")
                                    xiangtongxingfenxi = "YES";

                                string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.Data, "\r\n");

                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                                for (int ii = 0; ii < temp2.Length; ii++)
                                {
                                    for (int i = 1; i < temp1.Length; i++)
                                    {
                                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp1[i], "段");
                                        int ss = ii + 1;
                                        bool isrun = false;

                                        for (int j = 0; j < UDF.Count; j++)
                                        {
                                            if (UDF[j] == ss)
                                                isrun = true;

                                        }
                                        if (isrun == false)
                                            continue;

                                        //if (temp1[i].Contains(temp2[ii]))
                                        if (temp3[1].Contains(temp2[ii]))
                                        {
                                            //例如 27 包含7  所以容易出错再次切分判断一次  20240314
                                            string[] temp3_1 = System.Text.RegularExpressions.Regex.Split(temp3[1], " ");
                                            int ishanv = 0;
                                            for (int ip = 0; ip < temp3_1.Length; ip++)
                                            {
                                                if (temp3_1[ip] == temp2[ii])
                                                    ishanv++;

                                            }
                                            if (ishanv == 0)
                                                continue;


                                            item.JiShu = item.JiShu + "基" + ss.ToString() + " " + temp3[0];
                                            if (ss == 1)
                                                item.JiShu1 = temp3[0];
                                            else if (ss == 2)
                                                item.JiShu2 = temp3[0];
                                            else if (ss == 3)
                                                item.JiShu3 = temp3[0];
                                            else if (ss == 4)
                                                item.JiShu4 = temp3[0];
                                            else if (ss == 5)
                                                item.JiShu5 = temp3[0];
                                            else if (ss == 6)
                                                item.JiShu6 = temp3[0];
                                            else if (ss == 7)
                                                item.JiShu7 = temp3[0];
                                            else if (ss == 8)
                                                item.JiShu8 = temp3[0];
                                            else if (ss == 9)
                                                item.JiShu9 = temp3[0];

                                            else if (ss == 10)
                                                item.JiShu10 = temp3[0];

                                            else if (ss == 11)
                                                item.JiShu11 = temp3[0];

                                            else if (ss == 12)
                                                item.JiShu12 = temp3[0];

                                            else if (ss == 13)
                                                item.JiShu13 = temp3[0];

                                            else if (ss == 14)
                                                item.JiShu14 = temp3[0];

                                            else if (ss == 15)
                                                item.JiShu15 = temp3[0];

                                            //new0621

                                            else if (ss == 16)
                                                item.JiShu16 = temp3[0];


                                            else if (ss == 17)
                                                item.JiShu17 = temp3[0];


                                            else if (ss == 18)
                                                item.JiShu18 = temp3[0];

                                            else if (ss == 19)
                                                item.JiShu19 = temp3[0];

                                            else if (ss == 20)
                                                item.JiShu20 = temp3[0];

                                            else if (ss == 21)
                                                item.JiShu21 = temp3[0];

                                            else if (ss == 22)
                                                item.JiShu22 = temp3[0];

                                            else if (ss == 23)
                                                item.JiShu23 = temp3[0];

                                            else if (ss == 24)
                                                item.JiShu24 = temp3[0];

                                            else if (ss == 25)
                                                item.JiShu25 = temp3[0];

                                            else if (ss == 26)
                                                item.JiShu26 = temp3[0];

                                            else if (ss == 27)
                                                item.JiShu27 = temp3[0];

                                            else if (ss == 28)
                                                item.JiShu28 = temp3[0];

                                            else if (ss == 29)
                                                item.JiShu29 = temp3[0];

                                            else if (ss == 30)
                                                item.JiShu30 = temp3[0];

                                            else if (ss == 31)
                                                item.JiShu31 = temp3[0];

                                            else if (ss == 32)
                                                item.JiShu32 = temp3[0];

                                            else if (ss == 33)
                                                item.JiShu33 = temp3[0];

                                            else if (ss == 34)
                                                item.JiShu34 = temp3[0];

                                            else if (ss == 35)
                                                item.JiShu35 = temp3[0];

                                            else if (ss == 36)
                                                item.JiShu36 = temp3[0];

                                            else if (ss == 37)
                                                item.JiShu37 = temp3[0];

                                            else if (ss == 38)
                                                item.JiShu38 = temp3[0];

                                            else if (ss == 39)
                                                item.JiShu39 = temp3[0];

                                            else if (ss == 40)
                                                item.JiShu40 = temp3[0];

                                            else if (ss == 41)
                                                item.JiShu41 = temp3[0];

                                            else if (ss == 42)
                                                item.JiShu42 = temp3[0];

                                            else if (ss == 43)
                                                item.JiShu43 = temp3[0];

                                            else if (ss == 44)
                                                item.JiShu44 = temp3[0];

                                            else if (ss == 45)
                                                item.JiShu45 = temp3[0];

                                            else if (ss == 46)
                                                item.JiShu46 = temp3[0];

                                            else if (ss == 47)
                                                item.JiShu47 = temp3[0];

                                            else if (ss == 48)
                                                item.JiShu48 = temp3[0];

                                            else if (ss == 49)
                                                item.JiShu49 = temp3[0];

                                            else if (ss == 50)
                                                item.JiShu50 = temp3[0];




                                            break;
                                        }
                                    }

                                }
                            }
                        }

                        #endregion

                        //  ClaimReport_Server = new List<inputCaipiaoDATA>();

                        //  ClaimReport_Server.Sort(new Comp());
                        int indexing = 0;
                        foreach (inputCaipiaoDATA item in ClaimReport_Server)
                        {
                            indexing = 0;

                            foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                            {
                                if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao))
                                {
                                    indexing++;
                                    int xiangtongindex = 0;

                                    //new 20191120  找出哪些 基数相等
                                    List<string> same_list = new List<string>();


                                    #region 匹配相同次数
                                    for (int j = 0; j < UDF.Count; j++)
                                    {

                                        //f000
                                        if (item.JiShu1 != null && item.JiShu1 == temp.JiShu1 && UDF[j] == 1)
                                        {
                                            same_list.Add("JiShu1");
                                            xiangtongindex++;
                                        }
                                        if (item.JiShu2 != null && item.JiShu2 == temp.JiShu2 && UDF[j] == 2)
                                        {
                                            same_list.Add("JiShu2");
                                            xiangtongindex++;
                                        }
                                        if (item.JiShu3 != null && item.JiShu3 == temp.JiShu3 && UDF[j] == 3)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu3");
                                        }
                                        if (item.JiShu4 != null && item.JiShu4 == temp.JiShu4 && UDF[j] == 4)
                                        {
                                            xiangtongindex++;

                                            same_list.Add("JiShu4");
                                        }
                                        if (item.JiShu5 != null && item.JiShu5 == temp.JiShu5 && UDF[j] == 5)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu5");
                                        }
                                        if (item.JiShu6 != null && item.JiShu6 == temp.JiShu6 && UDF[j] == 6)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu6");
                                        }
                                        if (item.JiShu7 != null && item.JiShu7 == temp.JiShu7 && UDF[j] == 7)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu7");
                                        }
                                        if (item.JiShu8 != null && item.JiShu8 == temp.JiShu8 && UDF[j] == 8)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu8");
                                        }
                                        if (item.JiShu9 != null && item.JiShu9 == temp.JiShu9 && UDF[j] == 9)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu9");
                                        }

                                        if (item.JiShu10 != null && item.JiShu10 == temp.JiShu10 && UDF[j] == 10)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu10");
                                        }
                                        if (item.JiShu11 != null && item.JiShu11 == temp.JiShu11 && UDF[j] == 11)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu11");
                                        }
                                        if (item.JiShu12 != null && item.JiShu12 == temp.JiShu12 && UDF[j] == 12)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu12");

                                        }
                                        if (item.JiShu13 != null && item.JiShu13 == temp.JiShu13 && UDF[j] == 13)
                                        {
                                            xiangtongindex++;

                                            same_list.Add("JiShu13");
                                        }
                                        if (item.JiShu14 != null && item.JiShu14 == temp.JiShu14 && UDF[j] == 14)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu14");
                                        }
                                        if (item.JiShu15 != null && item.JiShu15 == temp.JiShu15 && UDF[j] == 15)
                                        {
                                            xiangtongindex++;

                                            same_list.Add("JiShu15");
                                        }

                                        //new 0621
                                        if (item.JiShu16 != null && item.JiShu16 == temp.JiShu16 && UDF[j] == 16)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu16");
                                        }
                                        if (item.JiShu17 != null && item.JiShu17 == temp.JiShu17 && UDF[j] == 17)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu17");
                                        }
                                        if (item.JiShu18 != null && item.JiShu18 == temp.JiShu18 && UDF[j] == 18)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu18");
                                        }
                                        if (item.JiShu19 != null && item.JiShu19 == temp.JiShu19 && UDF[j] == 19)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu19");
                                        }
                                        if (item.JiShu20 != null && item.JiShu20 == temp.JiShu20 && UDF[j] == 20)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu20");
                                        }
                                        if (item.JiShu21 != null && item.JiShu21 == temp.JiShu21 && UDF[j] == 21)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu21");
                                        }
                                        if (item.JiShu22 != null && item.JiShu22 == temp.JiShu22 && UDF[j] == 22)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu22");
                                        }
                                        if (item.JiShu23 != null && item.JiShu23 == temp.JiShu23 && UDF[j] == 23)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu23");
                                        }
                                        if (item.JiShu24 != null && item.JiShu24 == temp.JiShu24 && UDF[j] == 24)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu24");
                                        }
                                        if (item.JiShu25 != null && item.JiShu25 == temp.JiShu25 && UDF[j] == 25)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu25");
                                        }
                                        if (item.JiShu26 != null && item.JiShu26 == temp.JiShu26 && UDF[j] == 26)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu26");
                                        }
                                        if (item.JiShu27 != null && item.JiShu27 == temp.JiShu27 && UDF[j] == 27)
                                        {
                                            xiangtongindex++;

                                            same_list.Add("JiShu27");
                                        }
                                        if (item.JiShu28 != null && item.JiShu28 == temp.JiShu28 && UDF[j] == 28)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu28");
                                        }
                                        if (item.JiShu29 != null && item.JiShu29 == temp.JiShu29 && UDF[j] == 29)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu29");
                                        }
                                        if (item.JiShu30 != null && item.JiShu30 == temp.JiShu30 && UDF[j] == 30)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu30");
                                        }
                                        if (item.JiShu31 != null && item.JiShu31 == temp.JiShu31 && UDF[j] == 31)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu31");
                                        }
                                        if (item.JiShu32 != null && item.JiShu32 == temp.JiShu32 && UDF[j] == 32)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu32");
                                        }
                                        if (item.JiShu33 != null && item.JiShu33 == temp.JiShu33 && UDF[j] == 33)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu33");
                                        }
                                        if (item.JiShu34 != null && item.JiShu34 == temp.JiShu34 && UDF[j] == 34)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu34");
                                        }
                                        if (item.JiShu35 != null && item.JiShu35 == temp.JiShu35 && UDF[j] == 35)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu35");
                                        }
                                        if (item.JiShu36 != null && item.JiShu36 == temp.JiShu36 && UDF[j] == 36)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu36");
                                        }
                                        if (item.JiShu37 != null && item.JiShu37 == temp.JiShu37 && UDF[j] == 37)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu37");
                                        }
                                        if (item.JiShu38 != null && item.JiShu38 == temp.JiShu38 && UDF[j] == 38)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu38");
                                        }
                                        if (item.JiShu39 != null && item.JiShu39 == temp.JiShu39 && UDF[j] == 39)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu39");
                                        }
                                        if (item.JiShu40 != null && item.JiShu40 == temp.JiShu40 && UDF[j] == 40)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu40");
                                        }
                                        if (item.JiShu41 != null && item.JiShu41 == temp.JiShu41 && UDF[j] == 41)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu41");
                                        }
                                        if (item.JiShu42 != null && item.JiShu42 == temp.JiShu42 && UDF[j] == 42)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu42");
                                        }
                                        if (item.JiShu43 != null && item.JiShu43 == temp.JiShu43 && UDF[j] == 43)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu43");
                                        }
                                        if (item.JiShu44 != null && item.JiShu44 == temp.JiShu44 && UDF[j] == 44)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu44");
                                        }
                                        if (item.JiShu45 != null && item.JiShu45 == temp.JiShu45 && UDF[j] == 45)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu45");
                                        }
                                        if (item.JiShu46 != null && item.JiShu46 == temp.JiShu46 && UDF[j] == 46)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu46");
                                        }
                                        if (item.JiShu47 != null && item.JiShu47 == temp.JiShu47 && UDF[j] == 47)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu47");
                                        }
                                        if (item.JiShu48 != null && item.JiShu48 == temp.JiShu48 && UDF[j] == 48)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu48");
                                        }
                                        if (item.JiShu49 != null && item.JiShu49 == temp.JiShu49 && UDF[j] == 49)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu49");
                                        }
                                        if (item.JiShu50 != null && item.JiShu50 == temp.JiShu50 && UDF[j] == 50)
                                        {
                                            xiangtongindex++;
                                            same_list.Add("JiShu50");
                                        }


                                    }
                                    #endregion


                                    #region MyRegion
                                    if (indexing == 1)
                                        item.qian1 = xiangtongindex.ToString();

                                    else if (indexing == 2) item.qian2 = xiangtongindex.ToString();
                                    else if (indexing == 3) item.qian3 = xiangtongindex.ToString();
                                    else if (indexing == 4) item.qian4 = xiangtongindex.ToString();
                                    else if (indexing == 5) item.qian5 = xiangtongindex.ToString();
                                    else if (indexing == 6) item.qian6 = xiangtongindex.ToString();
                                    else if (indexing == 7) item.qian7 = xiangtongindex.ToString();
                                    else if (indexing == 8) item.qian8 = xiangtongindex.ToString();
                                    else if (indexing == 9) item.qian9 = xiangtongindex.ToString();
                                    else if (indexing == 10) item.qian10 = xiangtongindex.ToString();
                                    else if (indexing == 11) item.qian11 = xiangtongindex.ToString();
                                    else if (indexing == 12) item.qian12 = xiangtongindex.ToString();
                                    else if (indexing == 13) item.qian13 = xiangtongindex.ToString();
                                    else if (indexing == 14) item.qian14 = xiangtongindex.ToString();
                                    else if (indexing == 15) item.qian15 = xiangtongindex.ToString();
                                    else if (indexing == 16) item.qian16 = xiangtongindex.ToString();
                                    else if (indexing == 17) item.qian17 = xiangtongindex.ToString();
                                    else if (indexing == 18) item.qian18 = xiangtongindex.ToString();
                                    else if (indexing == 19) item.qian19 = xiangtongindex.ToString();
                                    else if (indexing == 20) item.qian20 = xiangtongindex.ToString();
                                    else if (indexing == 21) item.qian21 = xiangtongindex.ToString();
                                    else if (indexing == 22) item.qian22 = xiangtongindex.ToString();
                                    else if (indexing == 23) item.qian23 = xiangtongindex.ToString();
                                    else if (indexing == 24) item.qian24 = xiangtongindex.ToString();
                                    else if (indexing == 25) item.qian25 = xiangtongindex.ToString();
                                    else if (indexing == 26) item.qian26 = xiangtongindex.ToString();
                                    else if (indexing == 27) item.qian27 = xiangtongindex.ToString();
                                    else if (indexing == 28) item.qian28 = xiangtongindex.ToString();
                                    else if (indexing == 29) item.qian29 = xiangtongindex.ToString();
                                    else if (indexing == 30) item.qian30 = xiangtongindex.ToString();
                                    else if (indexing == 31) item.qian31 = xiangtongindex.ToString();
                                    else if (indexing == 32) item.qian32 = xiangtongindex.ToString();
                                    else if (indexing == 33) item.qian33 = xiangtongindex.ToString();
                                    else if (indexing == 34) item.qian34 = xiangtongindex.ToString();
                                    else if (indexing == 35) item.qian35 = xiangtongindex.ToString();
                                    else if (indexing == 36) item.qian36 = xiangtongindex.ToString();
                                    else if (indexing == 37) item.qian37 = xiangtongindex.ToString();
                                    else if (indexing == 38) item.qian38 = xiangtongindex.ToString();
                                    else if (indexing == 39) item.qian39 = xiangtongindex.ToString();
                                    else if (indexing == 40) item.qian40 = xiangtongindex.ToString();
                                    else if (indexing == 41) item.qian41 = xiangtongindex.ToString();
                                    else if (indexing == 42) item.qian42 = xiangtongindex.ToString();
                                    else if (indexing == 43) item.qian43 = xiangtongindex.ToString();
                                    else if (indexing == 44) item.qian44 = xiangtongindex.ToString();
                                    else if (indexing == 45) item.qian45 = xiangtongindex.ToString();
                                    else if (indexing == 46) item.qian46 = xiangtongindex.ToString();
                                    else if (indexing == 47) item.qian47 = xiangtongindex.ToString();
                                    else if (indexing == 48) item.qian48 = xiangtongindex.ToString();
                                    else if (indexing == 49) item.qian49 = xiangtongindex.ToString();
                                    else if (indexing == 50) item.qian50 = xiangtongindex.ToString();
                                    else if (indexing == 51) item.qian51 = xiangtongindex.ToString();
                                    else if (indexing == 52) item.qian52 = xiangtongindex.ToString();
                                    else if (indexing == 53) item.qian53 = xiangtongindex.ToString();
                                    else if (indexing == 54) item.qian54 = xiangtongindex.ToString();
                                    else if (indexing == 55) item.qian55 = xiangtongindex.ToString();
                                    else if (indexing == 56) item.qian56 = xiangtongindex.ToString();
                                    else if (indexing == 57) item.qian57 = xiangtongindex.ToString();
                                    else if (indexing == 58) item.qian58 = xiangtongindex.ToString();
                                    else if (indexing == 59) item.qian59 = xiangtongindex.ToString();
                                    else if (indexing == 60) item.qian60 = xiangtongindex.ToString();
                                    else if (indexing == 61) item.qian61 = xiangtongindex.ToString();
                                    else if (indexing == 62) item.qian62 = xiangtongindex.ToString();
                                    else if (indexing == 63) item.qian63 = xiangtongindex.ToString();
                                    else if (indexing == 64) item.qian64 = xiangtongindex.ToString();
                                    else if (indexing == 65) item.qian65 = xiangtongindex.ToString();
                                    else if (indexing == 66) item.qian66 = xiangtongindex.ToString();
                                    else if (indexing == 67) item.qian67 = xiangtongindex.ToString();
                                    else if (indexing == 68) item.qian68 = xiangtongindex.ToString();
                                    else if (indexing == 69) item.qian69 = xiangtongindex.ToString();
                                    else if (indexing == 70) item.qian70 = xiangtongindex.ToString();
                                    else if (indexing == 71) item.qian71 = xiangtongindex.ToString();
                                    else if (indexing == 72) item.qian72 = xiangtongindex.ToString();
                                    else if (indexing == 73) item.qian73 = xiangtongindex.ToString();
                                    else if (indexing == 74) item.qian74 = xiangtongindex.ToString();
                                    else if (indexing == 75) item.qian75 = xiangtongindex.ToString();
                                    else if (indexing == 76) item.qian76 = xiangtongindex.ToString();
                                    else if (indexing == 77) item.qian77 = xiangtongindex.ToString();
                                    else if (indexing == 78) item.qian78 = xiangtongindex.ToString();
                                    else if (indexing == 79) item.qian79 = xiangtongindex.ToString();
                                    else if (indexing == 80) item.qian80 = xiangtongindex.ToString();
                                    else if (indexing == 81) item.qian81 = xiangtongindex.ToString();
                                    else if (indexing == 82) item.qian82 = xiangtongindex.ToString();
                                    else if (indexing == 83) item.qian83 = xiangtongindex.ToString();
                                    else if (indexing == 84) item.qian84 = xiangtongindex.ToString();
                                    else if (indexing == 85) item.qian85 = xiangtongindex.ToString();
                                    else if (indexing == 86) item.qian86 = xiangtongindex.ToString();
                                    else if (indexing == 87) item.qian87 = xiangtongindex.ToString();
                                    else if (indexing == 88) item.qian88 = xiangtongindex.ToString();
                                    else if (indexing == 89) item.qian89 = xiangtongindex.ToString();
                                    else if (indexing == 90) item.qian90 = xiangtongindex.ToString();
                                    else if (indexing == 91) item.qian91 = xiangtongindex.ToString();
                                    else if (indexing == 92) item.qian92 = xiangtongindex.ToString();
                                    else if (indexing == 93) item.qian93 = xiangtongindex.ToString();
                                    else if (indexing == 94) item.qian94 = xiangtongindex.ToString();
                                    else if (indexing == 95) item.qian95 = xiangtongindex.ToString();
                                    else if (indexing == 96) item.qian96 = xiangtongindex.ToString();
                                    else if (indexing == 97) item.qian97 = xiangtongindex.ToString();
                                    else if (indexing == 98) item.qian98 = xiangtongindex.ToString();
                                    else if (indexing == 99) item.qian99 = xiangtongindex.ToString();
                                    //20180630
                                    else if (indexing == 100) item.qian100 = xiangtongindex.ToString();
                                    else if (indexing == 101) item.qian101 = xiangtongindex.ToString();
                                    else if (indexing == 102) item.qian102 = xiangtongindex.ToString();
                                    else if (indexing == 103) item.qian103 = xiangtongindex.ToString();
                                    else if (indexing == 104) item.qian104 = xiangtongindex.ToString();
                                    else if (indexing == 105) item.qian105 = xiangtongindex.ToString();
                                    else if (indexing == 106) item.qian106 = xiangtongindex.ToString();
                                    else if (indexing == 107) item.qian107 = xiangtongindex.ToString();
                                    else if (indexing == 108) item.qian108 = xiangtongindex.ToString();
                                    else if (indexing == 109) item.qian109 = xiangtongindex.ToString();
                                    else if (indexing == 110) item.qian110 = xiangtongindex.ToString();
                                    else if (indexing == 111) item.qian111 = xiangtongindex.ToString();
                                    else if (indexing == 112) item.qian112 = xiangtongindex.ToString();
                                    else if (indexing == 113) item.qian113 = xiangtongindex.ToString();
                                    else if (indexing == 114) item.qian114 = xiangtongindex.ToString();
                                    else if (indexing == 115) item.qian115 = xiangtongindex.ToString();
                                    else if (indexing == 116) item.qian116 = xiangtongindex.ToString();
                                    else if (indexing == 117) item.qian117 = xiangtongindex.ToString();
                                    else if (indexing == 118) item.qian118 = xiangtongindex.ToString();
                                    else if (indexing == 119) item.qian119 = xiangtongindex.ToString();
                                    else if (indexing == 120) item.qian120 = xiangtongindex.ToString();
                                    else if (indexing == 121) item.qian121 = xiangtongindex.ToString();
                                    else if (indexing == 122) item.qian122 = xiangtongindex.ToString();
                                    else if (indexing == 123) item.qian123 = xiangtongindex.ToString();
                                    else if (indexing == 124) item.qian124 = xiangtongindex.ToString();
                                    else if (indexing == 125) item.qian125 = xiangtongindex.ToString();
                                    else if (indexing == 126) item.qian126 = xiangtongindex.ToString();
                                    else if (indexing == 127) item.qian127 = xiangtongindex.ToString();
                                    else if (indexing == 128) item.qian128 = xiangtongindex.ToString();
                                    else if (indexing == 129) item.qian129 = xiangtongindex.ToString();
                                    else if (indexing == 130) item.qian130 = xiangtongindex.ToString();
                                    else if (indexing == 131) item.qian131 = xiangtongindex.ToString();
                                    else if (indexing == 132) item.qian132 = xiangtongindex.ToString();
                                    else if (indexing == 133) item.qian133 = xiangtongindex.ToString();
                                    else if (indexing == 134) item.qian134 = xiangtongindex.ToString();
                                    else if (indexing == 135) item.qian135 = xiangtongindex.ToString();
                                    else if (indexing == 136) item.qian136 = xiangtongindex.ToString();
                                    else if (indexing == 137) item.qian137 = xiangtongindex.ToString();
                                    else if (indexing == 138) item.qian138 = xiangtongindex.ToString();
                                    else if (indexing == 139) item.qian139 = xiangtongindex.ToString();
                                    else if (indexing == 140) item.qian140 = xiangtongindex.ToString();
                                    else if (indexing == 141) item.qian141 = xiangtongindex.ToString();
                                    else if (indexing == 142) item.qian142 = xiangtongindex.ToString();
                                    else if (indexing == 143) item.qian143 = xiangtongindex.ToString();
                                    else if (indexing == 144) item.qian144 = xiangtongindex.ToString();
                                    else if (indexing == 145) item.qian145 = xiangtongindex.ToString();
                                    else if (indexing == 146) item.qian146 = xiangtongindex.ToString();
                                    else if (indexing == 147) item.qian147 = xiangtongindex.ToString();
                                    else if (indexing == 148) item.qian148 = xiangtongindex.ToString();
                                    else if (indexing == 149) item.qian149 = xiangtongindex.ToString();
                                    else if (indexing == 150) item.qian150 = xiangtongindex.ToString();
                                    else if (indexing == 151) item.qian151 = xiangtongindex.ToString();
                                    else if (indexing == 152) item.qian152 = xiangtongindex.ToString();
                                    else if (indexing == 153) item.qian153 = xiangtongindex.ToString();
                                    else if (indexing == 154) item.qian154 = xiangtongindex.ToString();
                                    else if (indexing == 155) item.qian155 = xiangtongindex.ToString();
                                    else if (indexing == 156) item.qian156 = xiangtongindex.ToString();
                                    else if (indexing == 157) item.qian157 = xiangtongindex.ToString();
                                    else if (indexing == 158) item.qian158 = xiangtongindex.ToString();
                                    else if (indexing == 159) item.qian159 = xiangtongindex.ToString();
                                    else if (indexing == 160) item.qian160 = xiangtongindex.ToString();
                                    else if (indexing == 161) item.qian161 = xiangtongindex.ToString();
                                    else if (indexing == 162) item.qian162 = xiangtongindex.ToString();
                                    else if (indexing == 163) item.qian163 = xiangtongindex.ToString();
                                    else if (indexing == 164) item.qian164 = xiangtongindex.ToString();
                                    else if (indexing == 165) item.qian165 = xiangtongindex.ToString();
                                    else if (indexing == 166) item.qian166 = xiangtongindex.ToString();
                                    else if (indexing == 167) item.qian167 = xiangtongindex.ToString();
                                    else if (indexing == 168) item.qian168 = xiangtongindex.ToString();
                                    else if (indexing == 169) item.qian169 = xiangtongindex.ToString();
                                    else if (indexing == 170) item.qian170 = xiangtongindex.ToString();
                                    else if (indexing == 171) item.qian171 = xiangtongindex.ToString();
                                    else if (indexing == 172) item.qian172 = xiangtongindex.ToString();
                                    else if (indexing == 173) item.qian173 = xiangtongindex.ToString();
                                    else if (indexing == 174) item.qian174 = xiangtongindex.ToString();
                                    else if (indexing == 175) item.qian175 = xiangtongindex.ToString();
                                    else if (indexing == 176) item.qian176 = xiangtongindex.ToString();
                                    else if (indexing == 177) item.qian177 = xiangtongindex.ToString();
                                    else if (indexing == 178) item.qian178 = xiangtongindex.ToString();
                                    else if (indexing == 179) item.qian179 = xiangtongindex.ToString();
                                    else if (indexing == 180) item.qian180 = xiangtongindex.ToString();
                                    else if (indexing == 181) item.qian181 = xiangtongindex.ToString();
                                    else if (indexing == 182) item.qian182 = xiangtongindex.ToString();
                                    else if (indexing == 183) item.qian183 = xiangtongindex.ToString();
                                    else if (indexing == 184) item.qian184 = xiangtongindex.ToString();
                                    else if (indexing == 185) item.qian185 = xiangtongindex.ToString();
                                    else if (indexing == 186) item.qian186 = xiangtongindex.ToString();
                                    else if (indexing == 187) item.qian187 = xiangtongindex.ToString();
                                    else if (indexing == 188) item.qian188 = xiangtongindex.ToString();
                                    else if (indexing == 189) item.qian189 = xiangtongindex.ToString();
                                    else if (indexing == 190) item.qian190 = xiangtongindex.ToString();
                                    else if (indexing == 191) item.qian191 = xiangtongindex.ToString();
                                    else if (indexing == 192) item.qian192 = xiangtongindex.ToString();
                                    else if (indexing == 193) item.qian193 = xiangtongindex.ToString();
                                    else if (indexing == 194) item.qian194 = xiangtongindex.ToString();
                                    else if (indexing == 195) item.qian195 = xiangtongindex.ToString();
                                    else if (indexing == 196) item.qian196 = xiangtongindex.ToString();
                                    else if (indexing == 197) item.qian197 = xiangtongindex.ToString();
                                    else if (indexing == 198) item.qian198 = xiangtongindex.ToString();
                                    else if (indexing == 199) item.qian199 = xiangtongindex.ToString();
                                    else if (indexing == 200) item.qian200 = xiangtongindex.ToString();
                                    #endregion

                                    // 20191121 判断基数 有3个以内的做 前期分析用
                                    if (xiangtongxingfenxi == "YES")
                                        same_qian3mark(indexing, item, xiangtongindex, same_list);
                                }
                            }
                        }
                        #endregion
                        NewMethod();
                    }
                }


            }
            else
                MessageBox.Show("请选择要分析的条目，否则请点击取消关闭窗口", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private void same_qian3mark(int indexing, inputCaipiaoDATA item, int xiangtongindex, List<string> same_list)
        {
            #region 20191121 判断基数 有3个以内的做 前期分析用

            if (UDF.Count < 5)
            {
                string xiangtogn = "";
                for (int j = 0; j < UDF.Count; j++)
                {
                    bool isrun = false;
                    for (int yx = 0; yx < same_list.Count; yx++)
                    {


                        //if (JIDTA.Count > 0)
                        {



                            if (("基" + UDF[j].ToString()) == same_list[yx].Replace("JiShu", "基"))
                                isrun = true;


                            if (isrun == true)
                                break;

                        }
                    }
                    if (isrun == true)
                        xiangtogn += "" + 1;
                    else
                        xiangtogn += "" + 0;
                }
                if (indexing == 1)
                    item.qian1 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 2) item.qian2 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 3) item.qian3 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 4) item.qian4 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 5) item.qian5 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 6) item.qian6 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 7) item.qian7 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 8) item.qian8 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 9) item.qian9 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 10) item.qian10 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 11) item.qian11 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 12) item.qian12 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 13) item.qian13 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 14) item.qian14 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 15) item.qian15 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 16) item.qian16 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 17) item.qian17 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 18) item.qian18 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 19) item.qian19 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 20) item.qian20 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 21) item.qian21 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 22) item.qian22 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 23) item.qian23 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 24) item.qian24 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 25) item.qian25 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 26) item.qian26 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 27) item.qian27 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 28) item.qian28 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 29) item.qian29 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 30) item.qian30 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 31) item.qian31 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 32) item.qian32 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 33) item.qian33 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 34) item.qian34 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 35) item.qian35 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 36) item.qian36 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 37) item.qian37 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 38) item.qian38 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 39) item.qian39 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 40) item.qian40 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 41) item.qian41 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 42) item.qian42 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 43) item.qian43 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 44) item.qian44 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 45) item.qian45 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 46) item.qian46 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 47) item.qian47 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 48) item.qian48 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 49) item.qian49 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 50) item.qian50 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 51) item.qian51 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 52) item.qian52 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 53) item.qian53 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 54) item.qian54 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 55) item.qian55 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 56) item.qian56 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 57) item.qian57 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 58) item.qian58 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 59) item.qian59 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 60) item.qian60 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 61) item.qian61 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 62) item.qian62 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 63) item.qian63 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 64) item.qian64 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 65) item.qian65 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 66) item.qian66 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 67) item.qian67 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 68) item.qian68 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 69) item.qian69 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 70) item.qian70 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 71) item.qian71 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 72) item.qian72 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 73) item.qian73 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 74) item.qian74 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 75) item.qian75 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 76) item.qian76 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 77) item.qian77 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 78) item.qian78 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 79) item.qian79 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 80) item.qian80 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 81) item.qian81 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 82) item.qian82 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 83) item.qian83 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 84) item.qian84 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 85) item.qian85 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 86) item.qian86 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 87) item.qian87 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 88) item.qian88 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 89) item.qian89 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 90) item.qian90 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 91) item.qian91 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 92) item.qian92 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 93) item.qian93 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 94) item.qian94 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 95) item.qian95 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 96) item.qian96 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 97) item.qian97 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 98) item.qian98 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 99) item.qian99 = xiangtongindex.ToString() + "-" + xiangtogn;
                //20180630
                else if (indexing == 100) item.qian100 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 101) item.qian101 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 102) item.qian102 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 103) item.qian103 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 104) item.qian104 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 105) item.qian105 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 106) item.qian106 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 107) item.qian107 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 108) item.qian108 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 109) item.qian109 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 110) item.qian110 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 111) item.qian111 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 112) item.qian112 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 113) item.qian113 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 114) item.qian114 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 115) item.qian115 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 116) item.qian116 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 117) item.qian117 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 118) item.qian118 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 119) item.qian119 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 120) item.qian120 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 121) item.qian121 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 122) item.qian122 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 123) item.qian123 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 124) item.qian124 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 125) item.qian125 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 126) item.qian126 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 127) item.qian127 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 128) item.qian128 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 129) item.qian129 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 130) item.qian130 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 131) item.qian131 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 132) item.qian132 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 133) item.qian133 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 134) item.qian134 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 135) item.qian135 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 136) item.qian136 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 137) item.qian137 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 138) item.qian138 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 139) item.qian139 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 140) item.qian140 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 141) item.qian141 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 142) item.qian142 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 143) item.qian143 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 144) item.qian144 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 145) item.qian145 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 146) item.qian146 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 147) item.qian147 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 148) item.qian148 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 149) item.qian149 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 150) item.qian150 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 151) item.qian151 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 152) item.qian152 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 153) item.qian153 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 154) item.qian154 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 155) item.qian155 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 156) item.qian156 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 157) item.qian157 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 158) item.qian158 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 159) item.qian159 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 160) item.qian160 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 161) item.qian161 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 162) item.qian162 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 163) item.qian163 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 164) item.qian164 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 165) item.qian165 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 166) item.qian166 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 167) item.qian167 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 168) item.qian168 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 169) item.qian169 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 170) item.qian170 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 171) item.qian171 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 172) item.qian172 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 173) item.qian173 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 174) item.qian174 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 175) item.qian175 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 176) item.qian176 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 177) item.qian177 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 178) item.qian178 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 179) item.qian179 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 180) item.qian180 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 181) item.qian181 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 182) item.qian182 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 183) item.qian183 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 184) item.qian184 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 185) item.qian185 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 186) item.qian186 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 187) item.qian187 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 188) item.qian188 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 189) item.qian189 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 190) item.qian190 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 191) item.qian191 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 192) item.qian192 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 193) item.qian193 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 194) item.qian194 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 195) item.qian195 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 196) item.qian196 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 197) item.qian197 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 198) item.qian198 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 199) item.qian199 = xiangtongindex.ToString() + "-" + xiangtogn;
                else if (indexing == 200) item.qian200 = xiangtongindex.ToString() + "-" + xiangtogn;



            }


            #endregion
        }


        private void QianQI_Zidingyi_InitialSystemInfo()
        {

            int vony = this.clbStatus.Items.Count;
            for (int i = 0; i < vony; i++)
            {
                clbStatus.Items.Remove(clbStatus.Items[0]);
                this.checkedListBox1.Items.Remove(checkedListBox1.Items[0]);
            }
            clsAllnew BusinessHelp = new clsAllnew();

            List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");

            if (CaipiaozhongleiResult.Count == 0)
            {
                MessageBox.Show("彩票默认运行类型没有选中,请到【彩票类型界面】选中彩票类型，点击确认", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //this.label2.Text = CaipiaozhongleiResult[0].Name;
            ////this.label4.Text = CaipiaozhongleiResult[0].Name;
            //this.label6.Text = CaipiaozhongleiResult[0].JiBenHaoMaS + "-" + CaipiaozhongleiResult[0].JiBenHaoMaT;
            string len = CaipiaozhongleiResult[0].Xuan;
            toolStripComboBox5.Items.Clear();
            toolStripComboBox6.Items.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            for (int i = 0; i < Convert.ToInt32(len); i++)
            {
                int con = i + 1;

                clbStatus.Items.Add("第 " + con + " 位");
                this.checkedListBox1.Items.Add("第 " + con + " 位");

                toolStripComboBox5.Items.Add("随机 " + con + " 位");
                toolStripComboBox6.Items.Add("随机 " + con + " 位");
                this.comboBox1.Items.Add("随机 " + con + " 位");
                this.comboBox2.Items.Add("随机 " + con + " 位");
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            suiji20_30 = 0;
            //20230427 添加随机

            comboBox2_SelectedIndexChanged(this, EventArgs.Empty);
            toolStripButton3_Click(this, EventArgs.Empty);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbStatus.Items.Count; i++)
            {
                clbStatus.SetItemChecked(i, false);
                this.checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

            newi = new List<int>();
            if (clbStatus.CheckedItems.Count > 0)
            {
                foreach (string status in this.clbStatus.CheckedItems)
                {
                    newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                }
            }
            qianqi_newi = new List<int>();
            if (this.checkedListBox1.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox1.CheckedItems)
                {
                    qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                }
            }

            ZidingYi_tab2();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            JIDTA = new List<int>();

            //if (checkBox1.Checked == true)
            //    JIDTA.Add(1);
            //if (checkBox3.Checked == true)
            //    JIDTA.Add(2);
            //if (checkBox2.Checked == true)
            //    JIDTA.Add(3);
            //if (checkBox4.Checked == true)
            //    JIDTA.Add(4);
            //if (checkBox5.Checked == true)
            //    JIDTA.Add(5);
            //if (checkBox6.Checked == true)
            //    JIDTA.Add(6);
            //if (checkBox7.Checked == true)
            //    JIDTA.Add(7);
            //if (checkBox8.Checked == true)
            //    JIDTA.Add(8);
            //if (checkBox9.Checked == true)
            //    JIDTA.Add(9);
            //if (checkBox10.Checked == true)
            //    JIDTA.Add(10);
            if (checkedListBox2.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox2.CheckedItems)
                {
                    if (status.Contains("基"))
                    {
                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, "基");
                        JIDTA.Add(Convert.ToInt32(temp3[1]));
                    }
                    else if (status.Contains("特"))
                    {
                        //string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, "特别号");
                        JIDTA.Add(10);
                    }
                }
            }

            if (JIDTA.Count > 0)
            {
                //Suiji_NewMethod1();
                Suiji_NewMethodNEW();

                //button2_Click(object sender, EventArgs e);
                button2.PerformClick();
            }
            else
            {
                // Suiji_NewMethod1();
                Suiji_NewMethodNEW();
            }
        }

        private void Suiji_NewMethod1()
        {
            try
            {
                newlist = new List<int>();
                showSuijiResultlist = new List<string>();

                newlist.Add(0);
                newlist.Add(1);
                newlist.Add(2);
                newlist.Add(3);
                newlist.Add(4);
                newlist.Add(5);
                newlist.Add(6);
                newlist.Add(7);
                newlist.Add(8);
                newlist.Add(9);
                //  newlist.Add(10);

                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();

                clsAllnew BusinessHelp = new clsAllnew();
                List<FangAnLieBiaoDATA> Result12 = BusinessHelp.Read_FangAn("YES");
                if (Result12[0].MorenDuanShu != null && Result12[0].MorenDuanShu != "")
                {

                }
                else
                {
                    MessageBox.Show("请设置中计选择默认段数，否则无法分配数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }

                ////随机分段取位数
                //List<int> newlist1 = new List<int>();
                //newlist1.Add(2);
                //newlist1.Add(3);
                //newlist1.Add(4);
                //newlist1.Add(5);
                //newlist1.Add(6);
                //newlist1 = newlist1.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();

                int duan = Convert.ToInt32(Result12[0].MorenDuanShu);
                int evertduan = 10 / duan;
                int ilast = 0;
                ilast = duan * evertduan;


                string first = "";
                showSuijiResultlist = new List<string>();
                for (int iq = 1; iq <= duan; iq++)
                {
                    string num = "";
                    int ago = 0;

                    for (int i = 0; i <= evertduan; i++)
                    {
                        ago++;
                        if (ago > evertduan)
                            break;

                        num = num + " " + newlist[0];
                        newlist.RemoveAt(0);

                    }
                    first = first + "\r\n" + iq.ToString() + "段=" + " " + num;

                    showSuijiResultlist.Add(iq.ToString() + " 段= " + " " + num);

                }
                List<string> showSuijiResultlist1 = new List<string>();

                for (int ii = 0; ii < showSuijiResultlist.Count; ii++)
                {
                    for (int i = 0; i < newlist.Count; i++)
                    {
                        showSuijiResultlist[ii] = showSuijiResultlist[ii] + " " + newlist[i];
                        newlist.RemoveAt(i);
                        break;
                    }
                }

                List<FangAnLieBiaoDATA> Result = new List<FangAnLieBiaoDATA>();
                FangAnLieBiaoDATA item = new FangAnLieBiaoDATA();


                for (int i = 0; i < showSuijiResultlist.Count; i++)
                {
                    string[] temp1 = System.Text.RegularExpressions.Regex.Split(showSuijiResultlist[i], "=");
                    if (i == 0)
                        item.DuanWei1 = temp1[1].Trim();
                    else if (i == 1)
                        item.DuanWei2 = temp1[1].Trim();
                    else if (i == 2)
                        item.DuanWei3 = temp1[1].Trim();
                    else if (i == 3)
                        item.DuanWei4 = temp1[1].Trim();
                    else if (i == 4)
                        item.DuanWei5 = temp1[1].Trim();
                    else if (i == 5)
                        item.DuanWei6 = temp1[1].Trim();
                    else if (i == 6)
                        item.DuanWei7 = temp1[1].Trim();
                    else if (i == 7)
                        item.DuanWei8 = temp1[1].Trim();
                    else if (i == 8)
                        item.DuanWei9 = temp1[1].Trim();
                    else if (i == 9)
                        item.DuanWei10 = temp1[1].Trim();

                        //20240311
                    else if (i == 10)
                        item.DuanWei11 = temp1[1].Trim();
                    else if (i == 11)
                        item.DuanWei12 = temp1[1].Trim();
                    else if (i == 12)
                        item.DuanWei13 = temp1[1].Trim();
                    else if (i == 13)
                        item.DuanWei14 = temp1[1].Trim();
                    else if (i == 14)
                        item.DuanWei15 = temp1[1].Trim();
                    else if (i == 15)
                        item.DuanWei16 = temp1[1].Trim();
                    else if (i == 16)
                        item.DuanWei17 = temp1[1].Trim();

                    item.Data = item.Data + "\r\n" + showSuijiResultlist[i];
                }
                item.ZhuJian = "YES";
                if (Result12[0].MorenDuanShu != null && Result12[0].MorenDuanShu != "")
                    item.MorenDuanShu = Result12[0].MorenDuanShu;//保存名称
                item.Name = "默认方案";//保存名称
                item.DuanShu = showSuijiResultlist.Count.ToString();
                Result.Add(item);

                BusinessHelp.Save_FangAn(Result);
                NewMethodtab1();

                toolStripLabel7.Text = item.Data.Replace("\r\n", "*");
                //toolStripLabel9.Text = item.Data.Replace("\r\n", "* ").Replace("=   ", "=");

            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return;

                throw;
            }
        }

        private void Suiji_NewMethodNEW()
        {
            try
            {
                ArrayList CharList = new ArrayList();
                newlist = new List<int>();
                showSuijiResultlist = new List<string>();

                newlist.Add(0);
                newlist.Add(1);
                newlist.Add(2);
                newlist.Add(3);
                newlist.Add(4);
                newlist.Add(5);
                newlist.Add(6);
                newlist.Add(7);
                newlist.Add(8);
                newlist.Add(9);
                // newlist.Add(10);
                clsAllnew BusinessHelp1 = new clsAllnew();
                List<FangAnLieBiaoDATA> Result121 = BusinessHelp1.Read_FangAn("YES");
                if (Result121[0].Mobanleibie != null && Result121[0].Mobanleibie.Contains("_"))
                {
                    //要求 1-33 不含0 
                    newlist = new List<int>();
                    newlist.Add(1);
                    newlist.Add(2);
                    newlist.Add(3);
                    newlist.Add(4);
                    newlist.Add(5);
                    newlist.Add(6);
                    newlist.Add(7);
                    newlist.Add(8);
                    newlist.Add(9);

                    newlist.Add(10);
                    newlist.Add(11);
                    newlist.Add(12);
                    newlist.Add(13);
                    newlist.Add(14);
                    newlist.Add(15);
                    newlist.Add(16);
                    newlist.Add(17);
                    newlist.Add(18);
                    newlist.Add(19);
                    newlist.Add(20);
                    newlist.Add(21);
                    newlist.Add(22);
                    newlist.Add(23);
                    newlist.Add(24);
                    newlist.Add(25);
                    newlist.Add(26);
                    newlist.Add(27);
                    newlist.Add(28);
                    newlist.Add(29);
                    newlist.Add(30);
                    newlist.Add(31);
                    newlist.Add(32);
                    newlist.Add(33);
                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                for (int i = 0; i < newlist.Count; i++)
                {
                    CharList.Add(newlist[i].ToString());
                }
                clsAllnew BusinessHelp = new clsAllnew();
                List<FangAnLieBiaoDATA> Result12 = BusinessHelp.Read_FangAn("YES");
                if (Result12.Count > 0 && Result12[0].MorenDuanShu != null && Result12[0].MorenDuanShu != "")
                {
                    if (Result12[0].Data == null)
                    {

                        MessageBox.Show("方案数据填写缺失 ，请检查操作步骤是否保存上数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;

                    }
                }
                else
                {
                    MessageBox.Show("请设置中计选择默认段数，否则无法分配数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int duan = Convert.ToInt32(Result12[0].MorenDuanShu);
                int evertduan = 10 / duan;
                int ilast = 0;
                ilast = duan * evertduan;
                List<string> SelfNo = new List<string>();
                #region 判断自定义段位模板按其分配每段的数字个数

                if (Result12[0].Mobanleibie != "" && Result12[0].Mobanleibie != "默认")
                {
                    List<int> EverDuanList = ZidingyiMeiDuanGeshu(Result12[0].Mobanleibie);
                    string first = "";
                    showSuijiResultlist = new List<string>();
                    //for (int iq = 1; iq <= duan; iq++)
                    {
                        int iq = 0;

                        for (int i = 0; i < EverDuanList.Count; i++)
                        {
                            string num = "";
                            int ago = 0;
                            //如果有自定义的数字则重新计算当前段数的所添加数字个数
                            int newEverDuanList = EverDuanList[i];
                            string newaddselfn0 = "";
                            #region  //如果有自定义的数字则重新计算当前段数的所添加数字个数

                            for (int ii = 0; ii < SelfNo.Count; ii++)
                            {

                                if (SelfNo[ii].Contains("一") && i == 0)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[0] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("二") && i == 1)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[1] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("三") && i == 2)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[2] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("四") && i == 3)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[3] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("五") && i == 4)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[4] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("六") && i == 5)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[5] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("七") && i == 6)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[6] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("八") && i == 7)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[7] - temp3.Length;
                                    break;

                                }
                                else if (SelfNo[ii].Contains("九") && i == 8)
                                {
                                    string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                    newaddselfn0 = temp2[1];

                                    string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                    newEverDuanList = EverDuanList[8] - temp3.Length;
                                    break;

                                }


                            }
                            #endregion


                            //for (int j = 0; j <= EverDuanList[i]; j++)
                            for (int j = 0; j <= newEverDuanList; j++)
                            {
                                ago++;
                                if (ago > newEverDuanList)
                                    break;
                                num = num + " " + newlist[0];
                                newlist.RemoveAt(0);
                            }
                            iq = i + 1;
                            if (newEverDuanList != EverDuanList[i])
                                num = newaddselfn0 + num;

                            first = first + "\r\n" + iq.ToString() + "段=" + " " + num;

                            showSuijiResultlist.Add(iq.ToString() + " 段= " + " " + num);
                        }


                    }
                    List<string> showSuijiResultlist1 = new List<string>();
                    for (int ii = 0; ii < showSuijiResultlist.Count; ii++)
                    {
                        for (int i = 0; i < newlist.Count; i++)
                        {
                            showSuijiResultlist[ii] = showSuijiResultlist[ii] + " " + newlist[i];
                            newlist.RemoveAt(i);

                            break;
                        }
                    }
                }
                #endregion
                else
                {
                    //if (ilast > 0)
                    //{

                    string first = "";
                    showSuijiResultlist = new List<string>();
                    for (int iq = 1; iq <= duan; iq++)
                    {
                        string num = "";
                        int ago = 0;

                        //如果有自定义的数字则重新计算当前段数的所添加数字个数
                        int newEverDuanList = evertduan;
                        string newaddselfn0 = "";
                        #region  //如果有自定义的数字则重新计算当前段数的所添加数字个数

                        for (int ii = 0; ii < SelfNo.Count; ii++)
                        {

                            if (SelfNo[ii].Contains("一") && iq == 1)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("二") && iq == 2)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("三") && iq == 3)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("四") && iq == 4)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("五") && iq == 5)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("六") && iq == 6)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("七") && iq == 7)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("八") && iq == 8)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;

                            }
                            else if (SelfNo[ii].Contains("九") && iq == 9)
                            {
                                string[] temp2 = System.Text.RegularExpressions.Regex.Split(SelfNo[ii], "\t");
                                newaddselfn0 = temp2[1];

                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp2[1], " ");
                                newEverDuanList = evertduan - temp3.Length;
                                break;
                            }
                        }
                        #endregion
                        //  for (int i = 0; i <= newlist.Count; i++)
                        for (int i = 0; i <= newEverDuanList; i++)
                        {
                            ago++;
                            if (ago > newEverDuanList)
                                break;

                            num = num + " " + newlist[0];
                            newlist.RemoveAt(0);

                        }
                        if (newEverDuanList != evertduan)
                            num = newaddselfn0 + num;
                        first = first + "\r\n" + iq.ToString() + "段=" + " " + num;

                        showSuijiResultlist.Add(iq.ToString() + " 段= " + " " + num);

                    }
                    List<string> showSuijiResultlist1 = new List<string>();

                    for (int ii = 0; ii < showSuijiResultlist.Count; ii++)
                    {
                        for (int i = 0; i < newlist.Count; i++)
                        {
                            showSuijiResultlist[ii] = showSuijiResultlist[ii] + " " + newlist[i];
                            newlist.RemoveAt(i);
                            //  showSuijiResultlist1.Add(showSuijiResultlist[ii]);

                            break;
                        }
                    }
                }
                //this.listBox3.DataSource = showSuijiResultlist;
                List<FangAnLieBiaoDATA> Result = new List<FangAnLieBiaoDATA>();
                FangAnLieBiaoDATA item = new FangAnLieBiaoDATA();


                for (int i = 0; i < showSuijiResultlist.Count; i++)
                {
                    string[] temp1 = System.Text.RegularExpressions.Regex.Split(showSuijiResultlist[i], "=");
                    if (i == 0)
                        item.DuanWei1 = temp1[1].Trim();
                    else if (i == 1)
                        item.DuanWei2 = temp1[1].Trim();
                    else if (i == 2)
                        item.DuanWei3 = temp1[1].Trim();
                    else if (i == 3)
                        item.DuanWei4 = temp1[1].Trim();
                    else if (i == 4)
                        item.DuanWei5 = temp1[1].Trim();
                    else if (i == 5)
                        item.DuanWei6 = temp1[1].Trim();
                    else if (i == 6)
                        item.DuanWei7 = temp1[1].Trim();
                    else if (i == 7)
                        item.DuanWei8 = temp1[1].Trim();
                    else if (i == 8)
                        item.DuanWei9 = temp1[1].Trim();
                    else if (i == 9)
                        item.DuanWei10 = temp1[1].Trim();

                             //20240311
                    else if (i == 10)
                        item.DuanWei11 = temp1[1].Trim();
                    else if (i == 11)
                        item.DuanWei12 = temp1[1].Trim();
                    else if (i == 12)
                        item.DuanWei13 = temp1[1].Trim();
                    else if (i == 13)
                        item.DuanWei14 = temp1[1].Trim();
                    else if (i == 14)
                        item.DuanWei15 = temp1[1].Trim();
                    else if (i == 15)
                        item.DuanWei16 = temp1[1].Trim();
                    else if (i == 16)
                        item.DuanWei17 = temp1[1].Trim();


                    item.Data = item.Data + "\r\n" + showSuijiResultlist[i];
                }
                item.ZhuJian = "YES";
                if (Result12[0].MorenDuanShu != null && Result12[0].MorenDuanShu != "")
                    item.MorenDuanShu = Result12[0].MorenDuanShu;//保存名称
                if (Result12[0].Mobanleibie != null && Result12[0].Mobanleibie != "")
                    item.Mobanleibie = Result12[0].Mobanleibie;//保存名称

                item.Name = "默认方案";//保存名称
                item.DuanShu = showSuijiResultlist.Count.ToString();
                Result.Add(item);

                BusinessHelp.Save_FangAn(Result);
                NewMethodtab1();
                if (item.Data != null)
                {
                    toolStripLabel7.Text = item.Data.Replace("\r\n", "*");
                    //toolStripLabel9.Text = item.Data.Replace("\r\n", "* ").Replace("=   ", "=");

                }


                else
                    toolStripLabel7.Text = "检查方案数据是否准确";


            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return;

                throw;
            }
        }

        private List<int> ZidingyiMeiDuanGeshu(string NAME)
        {
            List<int> EverDuanList = new List<int>();

            if (NAME == "46 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(6);
            }
            else if (NAME == "28 模板")
            {
                EverDuanList.Add(2);
                EverDuanList.Add(8);
            }
            else if (NAME == "37 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(7);
            }
            else if (NAME == "64 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(4);
            }
            else if (NAME == "82 模板")
            {
                EverDuanList.Add(8);
                EverDuanList.Add(2);
            }
            else if (NAME == "73 模板")
            {
                EverDuanList.Add(7);
                EverDuanList.Add(3);
            }
            else if (NAME == "532 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
            }
            else if (NAME == "622 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            else if (NAME == "442 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(2);
            }


            else if (NAME == "4222 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            //0409
            else if (NAME == "541 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(4);
                EverDuanList.Add(1);
            }
            else if (NAME == "631 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(3);
                EverDuanList.Add(1);
            }
            if (NAME == "4411 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "3331 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(1);
            }
            if (NAME == "4321 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
            }
            //20230427
            if (NAME == "5221 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
            }


            if (NAME == "5311 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(3);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            //new 621
            if (NAME == "721 模板")
            {
                EverDuanList.Add(7);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
            }
            //new 0621
            if (NAME == "7111 模板")
            {
                EverDuanList.Add(7);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "6211 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            //new 0621
            if (NAME == "52111 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "61111 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }

            if (NAME == "42211 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "32221 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
            }
            if (NAME == "43111 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(3);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "511111 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "421111 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            if (NAME == "331111 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            //7 段
            if (NAME == "4111111 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            //new 0621 8段
            if (NAME == "31111111 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
                EverDuanList.Add(1);
            }
            //20240311

            if (NAME == "17_16 模板")
            {
                EverDuanList.Add(17);
                EverDuanList.Add(16);
            }
            if (NAME == "11_11_11 模板")
            {
                EverDuanList.Add(11);
                EverDuanList.Add(11);
                EverDuanList.Add(11);
            }

            if (NAME == "9_8_8_8 模板")
            {
                EverDuanList.Add(9);
                EverDuanList.Add(8);
                EverDuanList.Add(8);
                EverDuanList.Add(8);
            }

            if (NAME == "7_7_7_6_6 模板")
            {
                EverDuanList.Add(7);
                EverDuanList.Add(7);
                EverDuanList.Add(7);
                EverDuanList.Add(6);
                EverDuanList.Add(6);
            }

            if (NAME == "666_555 模板")
            {
                EverDuanList.Add(6);
                EverDuanList.Add(6);
                EverDuanList.Add(6);
                EverDuanList.Add(5);
                EverDuanList.Add(5);
                EverDuanList.Add(5);
            }
            if (NAME == "55555_44 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(5);
                EverDuanList.Add(5);
                EverDuanList.Add(5);
                EverDuanList.Add(5);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
            }

            if (NAME == "5_4444444 模板")
            {
                EverDuanList.Add(5);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
            }
            if (NAME == "444444_333 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
            }
            if (NAME == "444_3333333 模板")
            {
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(4);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
            }
            if (NAME == "33333333333_ 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
            }

            if (NAME == "333333333_222 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            if (NAME == "3333333_222222 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            if (NAME == "33333_222222222 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            if (NAME == "333_222222222222 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            if (NAME == "3_222222222222222 模板")
            {
                EverDuanList.Add(3);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
            }
            if (NAME == "2222222222222222_1 模板")
            {
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(2);
                EverDuanList.Add(1);
            }


            return EverDuanList;

        }

        //判断是否为汉字
        public bool HasChineseTest(string text)
        {
            //string text = "是不是汉字，ABC,keleyi.com";
            char[] c = text.ToCharArray();
            bool ischina = false;

            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                {
                    ischina = true;

                }
                //else
                //{
                //    ischina = false;
                //}
            }
            return ischina;

        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int s = this.tabControl1.SelectedIndex;
            string shipper = this.toolStripComboBox1.Text;
            string county = toolStripComboBox2.Text;
            if (s == 0)
            {
                ApplyBindSourceFilter(shipper, county);

            }
            else if (s == 1)
                ApplyBindSourceFilter1(shipper, county);
            else if (s == 2)
                ApplyBindSourceFilter2(shipper, county);
        }

        private void toolStripComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 1)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < clbStatus.Items.Count; i++)
                {
                    clbStatus.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(toolStripComboBox5.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    clbStatus.SetItemChecked(newlist[i], true);
                }
            }
        }

        private void toolStripComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 1)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(toolStripComboBox6.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    checkedListBox1.SetItemChecked(newlist[i], true);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 1)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < clbStatus.Items.Count; i++)
                {
                    clbStatus.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox1.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    clbStatus.SetItemChecked(newlist[i], true);
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 1)
            {
                List<int> newlist = new List<int>();
                //在1-20选项区间随机选择几位   20240314
                if (suiji20_30 == 2)
                {
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                        if (i >= 0 && i <  20)
                        {

                            newlist.Add(i);
                        }
                    }
                }
                //在21-30选项区间随机选择几位   20240311
                else if (suiji20_30 == 1)
                {
                    for (int i = 0; i <  checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                        if (i > 19 && i <  30)
                        {

                            newlist.Add(i);
                        }
                    }

                }
                else if (suiji20_30 == 0)
                {
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                        newlist.Add(i);

                    }
                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox2.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    checkedListBox1.SetItemChecked(newlist[i], true);
                }
            }
            // suiji20_30 = 0;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 0)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox3.Text, " ");
                string index = temp3[1];

                {
                    for (int i = 0; i < Convert.ToInt32(index); i++)
                    {

                        checkedListBox2.SetItemChecked(newlist[i], true);
                    }
                }
            }
        }


        private void QihaoCombox_NewMethod()
        {
            try
            {
                int s = this.tabControl1.SelectedIndex;
                if (s == 0)
                {

                    //自构造table
                    if (toolStripComboBox4.Text == null || toolStripComboBox4.Text == "")
                        return;

                    var qtyTable = Combox_qtyTable;

                    int comvalue = Convert.ToInt32(toolStripComboBox4.Text);

                    //qtyTable.Columns.Add("期号", System.Type.GetType("System.String"));
                    //qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));

                    int JISHUIN = 0;
                    if (UDF != null && UDF.Count != 0)
                    {
                        JISHUIN = UDF.Count;
                    }
                    else
                    {
                        if (InitialUDF.Count == 0)
                            return;
                        if (InitialUDF != null && InitialUDF.Count != 0)
                        {
                            JISHUIN = InitialUDF[0];
                        }

                    }
                    int add_qianCount = Convert.ToInt32(toolStripComboBox4.Text) - qtyTable.Columns.Count + 2 + JISHUIN;

                    int nowfirtcloumnname = qtyTable.Columns.Count - 2 - JISHUIN;

                    for (int m = 0; m <= add_qianCount; m++)
                    {
                        int ss = nowfirtcloumnname + 1;
                        //0322 改名
                        if (add_qianCount > 0)
                            qtyTable.Columns.Add("前" + ss, System.Type.GetType("System.String"));
                        else
                            qtyTable.Columns.Remove("前" + add_qianCount);
                        nowfirtcloumnname++;

                    }
                    //  qtyTable.Rows.Add(qtyTable.NewRow());
                    //foreach (var k in ClaimReport_Server)
                    //{
                    //    qtyTable.Rows.Add(qtyTable.NewRow());
                    //}

                    int jk = 0;
                    int cindex = 12;
                    int jicloumn = 0;
                    //if (UDF != null && UDF.Count != 0)
                    //    jicloumn = 9 - UDF.Count;
                    UDF.Sort();
                    if (UDF != null && UDF.Count != 0)
                        jicloumn = Convert.ToInt32(UDF[UDF.Count - 1]) + 1;
                    else if (InitialUDF != null && InitialUDF.Count != 0)
                        jicloumn = Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) + 1;

                    int firtinsterValue = qtyTable.Columns.Count - add_qianCount;

                    foreach (var item in ClaimReport_Server)
                    {
                        //cindex = 10 - jicloumn;
                        cindex = jicloumn;
                        if (qtyTable.Columns.Count != 0 && jicloumn == 0)
                            cindex = qtyTable.Columns.Count - 1 - Convert.ToInt32(toolStripComboBox4.Text);
                        if (add_qianCount > 0)
                            cindex = firtinsterValue;

                        {
                            string allqian = item.qian1 + " " + item.qian2 + " " + item.qian3 + " " + item.qian4 + " " + item.qian5 + " " + item.qian6 + " " + item.qian7 + " " + item.qian8 + " " + item.qian9 + " " + item.qian10 + " " + item.qian11 + " " + item.qian12 + " " + item.qian13 + " " + item.qian14 + " " + item.qian15 + " " + item.qian16 + " " + item.qian17 + " " + item.qian18 + " " + item.qian19 + " " + item.qian20 + " " + item.qian21 + " " + item.qian22 + " " + item.qian23 + " " + item.qian24 + " " + item.qian25 + " " + item.qian26 + " " + item.qian27 + " " + item.qian28 + " " + item.qian29 + " " + item.qian30 + " " + item.qian31 + " " + item.qian32 + " " + item.qian33 + " " + item.qian34 + " " + item.qian35 + " " + item.qian36 + " " + item.qian37 + " " + item.qian38 + " " + item.qian39 + " " + item.qian40 + " " + item.qian41 + " " + item.qian42 + " " + item.qian43 + " " + item.qian44 + " " + item.qian45 + " " + item.qian46 + " " + item.qian47 + " " + item.qian48 + " " + item.qian49 + " " + item.qian50 + " " + item.qian51 + " " + item.qian52 + " " + item.qian53 + " " + item.qian54 + " " + item.qian55 + " " + item.qian56 + " " + item.qian57 + " " + item.qian58 + " " + item.qian59 + " " + item.qian60 + " " + item.qian61 + " " + item.qian62 + " " + item.qian63 + " " + item.qian64 + " " + item.qian65 + " " + item.qian66 + " " + item.qian67 + " " + item.qian68 + " " + item.qian69 + " " + item.qian70 + " " + item.qian71 + " " + item.qian72 + " " + item.qian73 + " " + item.qian74 + " " + item.qian75 + " " + item.qian76 + " " + item.qian77 + " " + item.qian78 + " " + item.qian79 + " " + item.qian80 + " " + item.qian81 + " " + item.qian82 + " " + item.qian83 + " " + item.qian84 + " " + item.qian85 + " " + item.qian86 + " " + item.qian87 + " " + item.qian88 + " " + item.qian89 + " " + item.qian90 + " " + item.qian91 + " " + item.qian92 + " " + item.qian93 + " " + item.qian94 + " " + item.qian95 + " " + item.qian96 + " " + item.qian97 + " " + item.qian98 + " " + item.qian99 + " " + item.qian100 + " "
                                                 + item.qian101 + " "
                                                + item.qian102 + " "
                                                + item.qian103 + " "
                                                + item.qian104 + " "
                                                + item.qian105 + " "
                                                + item.qian106 + " "
                                                + item.qian107 + " "
                                                + item.qian108 + " "
                                                + item.qian109 + " "
                                                + item.qian110 + " "
                                                + item.qian111 + " "
                                                + item.qian112 + " "
                                                + item.qian113 + " "
                                                + item.qian114 + " "
                                                + item.qian115 + " "
                                                + item.qian116 + " "
                                                + item.qian117 + " "
                                                + item.qian118 + " "
                                                + item.qian119 + " "
                                                + item.qian120 + " "
                                                + item.qian121 + " "
                                                + item.qian122 + " "
                                                + item.qian123 + " "
                                                + item.qian124 + " "
                                                + item.qian125 + " "
                                                + item.qian126 + " "
                                                + item.qian127 + " "
                                                + item.qian128 + " "
                                                + item.qian129 + " "
                                                + item.qian130 + " "
                                                + item.qian131 + " "
                                                + item.qian132 + " "
                                                + item.qian133 + " "
                                                + item.qian134 + " "
                                                + item.qian135 + " "
                                                + item.qian136 + " "
                                                + item.qian137 + " "
                                                + item.qian138 + " "
                                                + item.qian139 + " "
                                                + item.qian140 + " "
                                                + item.qian141 + " "
                                                + item.qian142 + " "
                                                + item.qian143 + " "
                                                + item.qian144 + " "
                                                + item.qian145 + " "
                                                + item.qian146 + " "
                                                + item.qian147 + " "
                                                + item.qian148 + " "
                                                + item.qian149 + " "
                                                + item.qian150 + " "
                                                + item.qian151 + " "
                                                + item.qian152 + " "
                                                + item.qian153 + " "
                                                + item.qian154 + " "
                                                + item.qian155 + " "
                                                + item.qian156 + " "
                                                + item.qian157 + " "
                                                + item.qian158 + " "
                                                + item.qian159 + " "
                                                + item.qian160 + " "
                                                + item.qian161 + " "
                                                + item.qian162 + " "
                                                + item.qian163 + " "
                                                + item.qian164 + " "
                                                + item.qian165 + " "
                                                + item.qian166 + " "
                                                + item.qian167 + " "
                                                + item.qian168 + " "
                                                + item.qian169 + " "
                                                + item.qian170 + " "
                                                + item.qian171 + " "
                                                + item.qian172 + " "
                                                + item.qian173 + " "
                                                + item.qian174 + " "
                                                + item.qian175 + " "
                                                + item.qian176 + " "
                                                + item.qian177 + " "
                                                + item.qian178 + " "
                                                + item.qian179 + " "
                                                + item.qian180 + " "
                                                + item.qian181 + " "
                                                + item.qian182 + " "
                                                + item.qian183 + " "
                                                + item.qian184 + " "
                                                + item.qian185 + " "
                                                + item.qian186 + " "
                                                + item.qian187 + " "
                                                + item.qian188 + " "
                                                + item.qian189 + " "
                                                + item.qian190 + " "
                                                + item.qian191 + " "
                                                + item.qian192 + " "
                                                + item.qian193 + " "
                                                + item.qian194 + " "
                                                + item.qian195 + " "
                                                + item.qian196 + " "
                                                + item.qian197 + " "
                                                + item.qian198 + " "
                                                + item.qian199 + " "
                                                + item.qian200 + " "
                            ;
                            int instercloumuinde = Convert.ToInt32(toolStripComboBox4.Text) - add_qianCount;

                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(allqian, " ");
                            for (int i = 0; i < temp1.Length; i++)
                            {
                                cindex++;


                                if (i >= add_qianCount)
                                    break;
                                qtyTable.Rows[jk][cindex - 1] = temp1[instercloumuinde];
                                instercloumuinde++;
                            }
                        }
                        //qtyTable.Rows[jk][0] = item.QiHao;
                        //qtyTable.Rows[jk][1] = item.KaiJianHaoMa;
                        //if (UDF != null && UDF.Count != 0)
                        //{
                        //    for (int m = 0; m < UDF.Count; m++)
                        //    {
                        //        if (UDF[m] == 1)
                        //            qtyTable.Rows[jk][2] = item.JiShu1;
                        //        if (UDF[m] == 2)
                        //            qtyTable.Rows[jk][3] = item.JiShu2;
                        //        if (UDF[m] == 3)
                        //            qtyTable.Rows[jk][4] = item.JiShu3;
                        //        if (UDF[m] == 4)
                        //            qtyTable.Rows[jk][5] = item.JiShu4;
                        //        if (UDF[m] == 5)
                        //            qtyTable.Rows[jk][6] = item.JiShu5;
                        //        if (UDF[m] == 6)
                        //            qtyTable.Rows[jk][7] = item.JiShu6;
                        //        if (UDF[m] == 7)
                        //            qtyTable.Rows[jk][8] = item.JiShu7;
                        //        if (UDF[m] == 8)
                        //            qtyTable.Rows[jk][9] = item.JiShu8;
                        //        if (UDF[m] == 9)
                        //            qtyTable.Rows[jk][10] = item.JiShu9;
                        //    }
                        //}
                        //else
                        //{
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 0)
                        //        qtyTable.Rows[jk][2] = item.JiShu1;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 1)
                        //        qtyTable.Rows[jk][3] = item.JiShu2;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 2)
                        //        qtyTable.Rows[jk][4] = item.JiShu3;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 3)
                        //        qtyTable.Rows[jk][5] = item.JiShu4;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 4)
                        //        qtyTable.Rows[jk][6] = item.JiShu5;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 5)
                        //        qtyTable.Rows[jk][7] = item.JiShu6;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 6)
                        //        qtyTable.Rows[jk][8] = item.JiShu7;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 7)
                        //        qtyTable.Rows[jk][9] = item.JiShu8;
                        //    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 8)
                        //        qtyTable.Rows[jk][10] = item.JiShu9;

                        //}
                        // qtyTable.Rows[1][4] = item.QiHao;
                        jk++;
                    }
                    //清空自定义的列数
                    this.dataGridView1.DataSource = null;
                    //  this.dataGridView1.AutoGenerateColumns = false;
                    this.bindingSource2.DataSource = qtyTable;
                    bindingSource2.Sort = "期号  ASC";
                    this.dataGridView1.DataSource = this.bindingSource2;
                    ////设置【前*】列宽
                    //int qiancout = Convert.ToInt32(toolStripComboBox4.Text);
                    //this.dataGridView1.RowHeadersWidth = 30;
                    //dataGridView1.Columns[1].Width = 40;
                    this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
                    //if (UDF != null && UDF.Count != 0)
                    //{

                    //    for (int j = 2; j < UDF[UDF.Count - 1] + 2 + qiancout; j++)
                    //    {

                    //        dataGridView1.Columns[j].Width = 30;
                    //    }
                    //}
                    //else if (InitialUDF != null && InitialUDF.Count != 0)
                    //{
                    //    for (int j = 2; j < InitialUDF[InitialUDF.Count - 1] + 2 + qiancout; j++)
                    //    {

                    //        dataGridView1.Columns[j].Width = 30;
                    //    }
                    //}

                    if (dataGridView1.Rows.Count != 0)
                    {
                        int ii = dataGridView1.Rows.Count - 1;
                        dataGridView1.CurrentCell = dataGridView1[0, ii]; // 强制将光标指向i行
                        dataGridView1.Rows[ii].Selected = true;   //光标显示至i行 
                    }
                    UDF = new List<int>();
                }
                else if (s == 1)
                {

                    qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
                    tab2();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据构造失败，请检查数据源", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
                throw;
            }
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel8.Text = "";
            #region 获取选中的基数

            List<int> JIDTA1 = new List<int>();

            if (checkedListBox2.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox2.CheckedItems)
                {
                    if (status.Contains("基"))
                    {
                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, "基");
                        JIDTA1.Add(Convert.ToInt32(temp3[1]));
                    }
                    else if (status.Contains("特"))
                    {

                        JIDTA1.Add(10);
                    }
                }


            }

            #endregion
            //toolStripLabel8.Text = GetRowIndexAt(e.Y).ToString();
            int r = this.dataGridView1.HitTest(e.X, e.Y).RowIndex; //行
            int c = this.dataGridView1.HitTest(e.X, e.Y).ColumnIndex; //列

            if (c > 0 && r > 0)
            {
                if (dataGridView1.Columns[c].HeaderText.Contains("前"))
                {
                    int qianqishu = Convert.ToInt32(dataGridView1.Columns[c].HeaderText.Replace("前", "")) + 1;
                    int ii = dataGridView1.Rows.Count - qianqishu;
                    if (ii > 0)
                    {  //  int iia = dataGridView1.Rows.Count - ii;
                        //    dataGridView1.CurrentCell = dataGridView1[0, ii];
                        //得到所有基数
                        for (int m = 2; m <= changeInitialUDF[changeInitialUDF.Count - 1] + 1; m++)
                        {
                            if (dataGridView1.Rows[ii].Cells[m].EditedFormattedValue != null && dataGridView1.Rows[ii].Cells[m].EditedFormattedValue.ToString() != "")
                                toolStripLabel8.Text = toolStripLabel8.Text + dataGridView1.Rows[ii].Cells[m].EditedFormattedValue.ToString();
                            else
                                toolStripLabel8.Text = toolStripLabel8.Text + " ";
                        }

                        #region 切分显示内容
                        if (JIDTA1.Count > 0)
                        {
                            string showmessage = "";

                            string[] temp3 = System.Text.RegularExpressions.Regex.Split(toolStripLabel8.Text, " ");
                            for (int i = 0; i < JIDTA1.Count; i++)
                            {
                                showmessage = showmessage + temp3[JIDTA1[i] - 1];

                            }
                            toolStripLabel8.Text = showmessage;

                        }

                        #endregion
                        toolStripLabel8.Text = "  选中信息：" + toolStripLabel8.Text;
                    }
                }
                else
                    toolStripLabel8.Text = "  选中信息：请鼠标移动到相应的【前】列上!";
            }


        }

        public int GetRowIndexAt(int mouseLocation_Y)
        {
            if (dataGridView1.FirstDisplayedScrollingRowIndex < 0)
            {
                return -1;  // no rows.   
            }
            if (dataGridView1.ColumnHeadersVisible == true && mouseLocation_Y <= dataGridView1.ColumnHeadersHeight)
            {
                return -1;
            }
            int index = dataGridView1.FirstDisplayedScrollingRowIndex;
            int displayedCount = dataGridView1.DisplayedRowCount(true);
            for (int k = 1; k <= displayedCount; )  // 因为行不能ReOrder，故只需要搜索显示的行   
            {
                if (dataGridView1.Rows[index].Visible == true)
                {
                    Rectangle rect = dataGridView1.GetRowDisplayRectangle(index, true);  // 取该区域的显示部分区域   
                    if (rect.Top <= mouseLocation_Y && mouseLocation_Y < rect.Bottom)
                    {
                        return index;
                    }
                    k++;  // 只计数显示的行;   
                }
                index++;
            }
            return -1;
        }

        private void dataGridView2_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel8.Text = "";

            #region 获取选中的基数

            List<int> JIDTA1 = new List<int>();

            if (checkedListBox1.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox1.CheckedItems)
                {
                    if (status.Contains("第"))
                    {
                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, " ");
                        JIDTA1.Add(Convert.ToInt32(temp3[1]));
                    }
                    else if (status.Contains("特"))
                    {

                        JIDTA1.Add(10);
                    }
                }


            }

            #endregion
            //toolStripLabel8.Text = GetRowIndexAt(e.Y).ToString();
            int r = this.dataGridView2.HitTest(e.X, e.Y).RowIndex; //行
            int c = this.dataGridView2.HitTest(e.X, e.Y).ColumnIndex; //列

            if (c > 0 && r > 0)
            {
                if (dataGridView2.Columns[c].HeaderText.Contains("前"))
                {
                    int qianqishu = Convert.ToInt32(dataGridView2.Columns[c].HeaderText.Replace("前", "")) + 1;
                    int ii = dataGridView2.Rows.Count - qianqishu;
                    //  int iia = dataGridView1.Rows.Count - ii;
                    //   dataGridView2.CurrentCell = dataGridView2[0, ii];
                    toolStripLabel8.Text = dataGridView2.Rows[ii].Cells[1].EditedFormattedValue.ToString();

                    #region 切分显示内容
                    if (JIDTA1.Count > 0)
                    {
                        string showmessage = "";

                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(toolStripLabel8.Text, " ");
                        for (int i = 0; i < JIDTA1.Count; i++)
                        {
                            if (JIDTA1[i] - 1 < temp3.Length)
                            {
                                //  新需求 点击鼠标显示双位数 没有用零补 方便查看 后期有作废
                                if (temp3[JIDTA1[i] - 1].Length < 2)
                                {
                                    showmessage = showmessage + " 0" + temp3[JIDTA1[i] - 1];
                                }
                                else
                                    showmessage = showmessage + " " + temp3[JIDTA1[i] - 1];
                            }
                        }
                        toolStripLabel8.Text = showmessage;

                    }

                    #endregion
                    toolStripLabel8.Text = "  选中信息：" + toolStripLabel8.Text;
                }
                else

                    toolStripLabel8.Text = "  选中信息：请鼠标移动到相应的【前】列上!";
            }


        }

        private void dataGridView1_MouseEnter(object sender, EventArgs e)
        {


        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.LightBlue;
            }
        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            //Color c = this.dataGridView1.CurrentRow.Cells[0].InheritedStyle.ForeColor;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 192);
                //dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.Aqua;
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 192);
            }
        }

        private void dataGridView2_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.LightBlue;
            }
        }

        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 192);
            }
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            newi = new List<int>();
            if (checkedListBox4.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox4.CheckedItems)
                {
                    newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                }
            }
            qianqi_newi = new List<int>();
            if (this.checkedListBox3.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox3.CheckedItems)
                {
                    qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));

                }
            }
            qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);
            chufafenxijishu_ZidingYi_tab3();
        }

        private void chufafenxijishu_ZidingYi_tab3()
        {
            {
                clsAllnew BusinessHelp = new clsAllnew();
                List<string> qianmingcheng = new List<string>();
                //ClaimReport_Server = BusinessHelp.ReadclaimreportfromServer();

                ClaimReport_Server.Sort(new CompsSmall());
                int indexing = 0;
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {

                    item.qianAll = "";
                    item.qianMingcheng = "";
                    item.TongAll = "";
                    indexing = 0;
                    string text = "";

                    // List<inputCaipiaoDATA> filtered = ClaimReport_Server.FindAll(s => Convert.ToInt32(s.QiHao) > Convert.ToInt32(item.QiHao));

                    foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                    {
                        string shifouyijingpanduanguozhegeshuzi = "";
                        if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao) && indexing < Convert.ToInt32(qianqiqishu))
                        {
                            indexing++;
                            int xiangtongindex = 0;
                            string jishutotal = "";
                            string jishutota2 = "";

                            jishutotal = hebing_jishu(item, jishutotal);
                            jishutota2 = hebing_jishu(temp, jishutota2);

                            string[] temp3 = System.Text.RegularExpressions.Regex.Split(jishutotal.Trim(), " ");
                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(jishutota2.Trim(), " ");

                            #region 匹配相同次数
                            for (int i = 0; i < temp3.Length; i++)
                            {
                                //判断是否在自定义范围内的数据
                                bool next = false;
                                for (int oi = 0; oi < newi.Count; oi++)
                                {
                                    if (newi[oi] == i + 1)
                                        next = true;
                                }
                                if (next == false)
                                    continue;
                                //前期数据的 分析数据的位置索引

                                for (int j1 = 0; j1 < temp1.Length; j1++)
                                {
                                    //判断是否在自定义范围内的数据
                                    bool nexti = false;
                                    for (int oi = 0; oi < qianqi_newi.Count; oi++)
                                    {
                                        if (qianqi_newi[oi] == j1 + 1)
                                        {
                                            nexti = true;
                                            break;
                                        }
                                    }
                                    if (nexti == false)
                                        continue;
                                    //判断一组号码内相同数字只判断一次
                                    string[] tempi = System.Text.RegularExpressions.Regex.Split(shifouyijingpanduanguozhegeshuzi, " ");
                                    int isruns = 0;

                                    for (int ih = 0; ih < tempi.Length; ih++)
                                    {
                                        if (temp3[i] == tempi[ih])
                                        {
                                            isruns++;
                                            break;

                                        }
                                    }
                                    if (isruns > 0)
                                        break;

                                    if (temp3[i] == temp1[j1])
                                    {
                                        shifouyijingpanduanguozhegeshuzi = temp3[i] + " " + shifouyijingpanduanguozhegeshuzi;
                                        xiangtongindex++;
                                    }
                                }
                            }

                            #endregion
                            //item.qianAll = item.qianAll + "\r\n前" + indexing + " " + xiangtongindex.ToString();
                            text = text + " " + xiangtongindex.ToString();
                            item.qianAll = item.qianAll + " " + xiangtongindex.ToString();
                            item.qianMingcheng = item.qianMingcheng + "\r\n前" + indexing;
                            //  qianmingcheng = item.qianMingcheng + "\r\n前" + indexing; ;
                            int isrun = 0;
                            for (int m = 0; m < qianmingcheng.Count; m++)
                            {
                                if (qianmingcheng[m] == "前" + indexing)
                                    isrun++;
                            }
                            if (isrun == 0)
                                qianmingcheng.Add("前" + indexing);

                        }
                        else if (indexing > Convert.ToInt32(qianqiqishu))
                        {
                            break;

                        }


                    }
                    string[] temptong = System.Text.RegularExpressions.Regex.Split(text, " ");

                    //for (int j = 0; j < 30; j++)
                    //20240328
                    for (int j = 0; j < 30; j++)
                    {
                        int xiangtongindex = 0;

                        for (int i = 1; i < temptong.Length; i++)
                        {
                            if (j.ToString() == temptong[i])
                            {
                                xiangtongindex++;
                            }

                        }
                        item.TongAll = item.TongAll + "\r\n同" + j + " " + xiangtongindex.ToString();

                    }

                }
                var qtyTable = new DataTable();

                int l = 0;
                //添加 抬头名称，如果 选中了前几期的combox 
                indexing = 1;
                qianmingcheng = new List<string>();
                for (int i = 1; i <= qianqiqishu; i++)
                {
                    qianmingcheng.Add("前" + indexing);
                    indexing++;
                }

                qtyTable.Columns.Add("期号", System.Type.GetType("System.Int32"));
                qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));
                qtyTable.Columns.Add("基数", System.Type.GetType("System.String"));
                for (int m = 0; m < qianmingcheng.Count; m++)
                {
                    qtyTable.Columns.Add(qianmingcheng[m], System.Type.GetType("System.String"));

                }
                //  qtyTable.Rows.Add(qtyTable.NewRow());
                foreach (var k in ClaimReport_Server)
                {
                    qtyTable.Rows.Add(qtyTable.NewRow());
                }

                int jk = 0;
                int cindex = 0;

                foreach (var item in ClaimReport_Server)
                {
                    cindex = 1;

                    if (item.qianAll != null)
                    {
                        string[] temp1 = System.Text.RegularExpressions.Regex.Split(item.qianAll, " ");
                        for (int i = 0; i < temp1.Length; i++)
                        {
                            cindex++;

                            if (i == 0 || i >= temp1.Length)
                                continue;

                            qtyTable.Rows[jk][cindex] = temp1[i];
                        }
                    }
                    qtyTable.Rows[jk][0] = item.QiHao;
                    qtyTable.Rows[jk][1] = item.KaiJianHaoMa;
                    // qtyTable.Rows[1][4] = item.QiHao;
                    string jishutotal = "";

                    #region 判断该显示多少个基数 然后合并到一起
                    jishutotal = hebing_jishu(item, jishutotal);
                    qtyTable.Rows[jk][2] = jishutotal.Trim();
                    #endregion
                    jk++;
                }

                //   sortablePendingOrderList = new SortableBindingList<inputCaipiaoDATA>(qtyTable);
                //qtyTable.Sort(new Comp());
                //  this.bindingSource1.DataSource = null;
                this.bindingSource1.DataSource = qtyTable;
                bindingSource1.Sort = "期号  ASC";

                this.dataGridView1.DataSource = this.bindingSource1;

                dataGridView3.DataSource = qtyTable;

                string width = "";

                for (int j = 3; j < dataGridView3.ColumnCount; j++)
                {

                    dataGridView3.Columns[j].Width = 30;
                }
                if (dataGridView3.Rows.Count != 0)
                {
                    int ii = dataGridView3.Rows.Count - 1;
                    dataGridView3.CurrentCell = dataGridView3[0, ii]; // 强制将光标指向i行
                    dataGridView3.Rows[ii].Selected = true;   //光标显示至i行 
                }
                toolStripLabel8.Text = "结束";
            }
        }

        private void tab3()
        {
            try
            {
                clsAllnew BusinessHelp = new clsAllnew();
                List<string> qianmingcheng = new List<string>();

                ClaimReport_Server.Sort(new CompsSmall());
                int indexing = 0;
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {
                    item.qianAll = "";
                    item.qianMingcheng = "";
                    item.TongAll = "";
                    indexing = 0;
                    string text = "";

                    foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                    {
                        if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao) && indexing < Convert.ToInt32(qianqiqishu))
                        {
                            indexing++;
                            int xiangtongindex = 0;
                            if (item.KaiJianHaoMa == null || temp.KaiJianHaoMa == null)
                                continue;
                            string jishutotal = "";
                            string jishutota2 = "";
                            jishutotal = hebing_jishu(item, jishutotal);
                            jishutota2 = hebing_jishu(temp, jishutota2);

                            //2 0 0 0 0 0 0 0 0 0 0 0 6 0 0
                            //1 1 1 1 1 1 1 1 1 1 1 1 4 1 1
                            string ite = jishutotal.Trim();
                            string tem = jishutota2.Trim();

                            string[] temp3 = System.Text.RegularExpressions.Regex.Split(ite, " ");
                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(tem, " ");

                            //string[] temp3 = System.Text.RegularExpressions.Regex.Split(item., " ");
                            //string[] temp1 = System.Text.RegularExpressions.Regex.Split(temp.KaiJianHaoMa, " ");
                            #region 匹配相同次数
                            string shifouyijingpanduanguozhegeshuzi = "";
                            for (int i = 0; i < temp3.Length; i++)
                            {
                                for (int j1 = 0; j1 < temp1.Length; j1++)
                                {
                                    string[] tempi = System.Text.RegularExpressions.Regex.Split(shifouyijingpanduanguozhegeshuzi, " ");
                                    int isruns = 0;

                                    for (int ih = 0; ih < tempi.Length; ih++)
                                    {
                                        if (temp3[i] == tempi[ih])
                                        {
                                            isruns++;
                                            break;

                                        }
                                    }
                                    if (isruns > 0)
                                        break;


                                    if (temp3[i] == temp1[j1])
                                    {
                                        shifouyijingpanduanguozhegeshuzi = temp3[i] + " " + shifouyijingpanduanguozhegeshuzi;
                                        xiangtongindex++;
                                    }
                                }
                            }

                            #endregion

                            text = text + " " + xiangtongindex.ToString();
                            item.qianAll = item.qianAll + " " + xiangtongindex.ToString();
                            item.qianMingcheng = item.qianMingcheng + "\r\n前" + indexing;

                            int isrun = 0;
                            for (int m = 0; m < qianmingcheng.Count; m++)
                            {
                                if (qianmingcheng[m] == "前" + indexing)
                                    isrun++;
                            }
                            if (isrun == 0)
                                qianmingcheng.Add("前" + indexing);
                        }
                        else if (indexing > Convert.ToInt32(qianqiqishu))
                        {
                            break;

                        }
                    }
                    string[] temptong = System.Text.RegularExpressions.Regex.Split(text, " ");

                    //   for (int j = 0; j < 30; j++)
                    //20240328
                    for (int j = 0; j < 30; j++)
                    {
                        int xiangtongindex = 0;

                        for (int i = 1; i < temptong.Length; i++)
                        {
                            if (j.ToString() == temptong[i])
                            {
                                xiangtongindex++;
                            }

                        }
                        item.TongAll = item.TongAll + "\r\n同" + j + " " + xiangtongindex.ToString();

                    }

                }
                var qtyTable = new DataTable();

                int l = 0;
                //添加 抬头名称，如果 选中了前几期的combox 
                indexing = 1;
                qianmingcheng = new List<string>();
                for (int i = 1; i <= qianqiqishu; i++)
                {
                    qianmingcheng.Add("前" + indexing);
                    indexing++;
                }

                qtyTable.Columns.Add("期号", System.Type.GetType("System.Int32"));
                qtyTable.Columns.Add("开奖号码", System.Type.GetType("System.String"));
                qtyTable.Columns.Add("基数", System.Type.GetType("System.String"));
                for (int m = 0; m < qianmingcheng.Count; m++)
                {
                    qtyTable.Columns.Add(qianmingcheng[m], System.Type.GetType("System.String"));

                }
                //  qtyTable.Rows.Add(qtyTable.NewRow());
                foreach (var k in ClaimReport_Server)
                {
                    qtyTable.Rows.Add(qtyTable.NewRow());
                }

                int jk = 0;
                int cindex = 0;

                foreach (var item in ClaimReport_Server)
                {
                    cindex = 1;

                    if (item.qianAll != null)
                    {
                        string[] temp1 = System.Text.RegularExpressions.Regex.Split(item.qianAll, " ");
                        for (int i = 0; i < temp1.Length; i++)
                        {
                            cindex++;

                            if (i == 0 || i >= temp1.Length)
                                continue;

                            qtyTable.Rows[jk][cindex] = temp1[i];
                        }
                    }
                    qtyTable.Rows[jk][0] = item.QiHao;
                    qtyTable.Rows[jk][1] = item.KaiJianHaoMa;

                    string jishutotal = "";

                    #region 判断该显示多少个基数 然后合并到一起
                    jishutotal = hebing_jishu(item, jishutotal);
                    #endregion
                    qtyTable.Rows[jk][2] = jishutotal.Trim();
                    jk++;
                }
                this.bindingSource1.DataSource = qtyTable;
                bindingSource1.Sort = "期号  ASC";
                dataGridView3.DataSource = qtyTable;

                string width = "";

                for (int j = 3; j < dataGridView3.ColumnCount; j++)
                {

                    dataGridView3.Columns[j].Width = 30;
                }
                if (dataGridView3.Rows.Count != 0)
                {
                    int ii = dataGridView3.Rows.Count - 1;
                    dataGridView3.CurrentCell = dataGridView3[0, ii]; // 强制将光标指向i行
                    dataGridView3.Rows[ii].Selected = true;   //光标显示至i行 
                }
                if (tab3shuiji == false)
                    toolStripLabel8.Text = "结束";
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string hebing_jishu(inputCaipiaoDATA item, string jishutotal)
        {
            if (UDF != null && UDF.Count != 0)
            {
                for (int m = 0; m < UDF.Count; m++)
                {
                    if (UDF[m] == 1)
                        jishutotal = item.JiShu1;
                    if (UDF[m] == 2)
                        jishutotal += item.JiShu2;
                    if (UDF[m] == 3)
                        jishutotal += item.JiShu3;
                    if (UDF[m] == 4)
                        jishutotal += item.JiShu4;
                    if (UDF[m] == 5)
                        jishutotal += item.JiShu5;
                    if (UDF[m] == 6)
                        jishutotal += item.JiShu6;
                    if (UDF[m] == 7)
                        jishutotal += item.JiShu7;
                    if (UDF[m] == 8)
                        jishutotal += item.JiShu8;
                    if (UDF[m] == 9)
                        jishutotal += item.JiShu9;

                    if (UDF[m] == 10)
                        jishutotal += item.JiShu10;

                    if (UDF[m] == 11)
                        jishutotal += item.JiShu11;

                    if (UDF[m] == 12)
                        jishutotal += item.JiShu12;

                    if (UDF[m] == 13)
                        jishutotal += item.JiShu13;

                    if (UDF[m] == 14)
                        jishutotal += item.JiShu14;

                    if (UDF[m] == 15)
                        jishutotal += item.JiShu15;
                    //new0621
                    if (UDF[m] == 16)
                        jishutotal += item.JiShu16;

                    if (UDF[m] == 17)
                        jishutotal += item.JiShu17;

                    if (UDF[m] == 18)
                        jishutotal += item.JiShu18;

                    if (UDF[m] == 19)
                        jishutotal += item.JiShu19;

                    if (UDF[m] == 20)
                        jishutotal += item.JiShu20;

                    if (UDF[m] == 21)
                        jishutotal += item.JiShu21;

                    if (UDF[m] == 22)
                        jishutotal += item.JiShu22;

                    if (UDF[m] == 23)
                        jishutotal += item.JiShu23;

                    if (UDF[m] == 24)
                        jishutotal += item.JiShu24;

                    if (UDF[m] == 25)
                        jishutotal += item.JiShu25;

                    if (UDF[m] == 26)
                        jishutotal += item.JiShu26;

                    if (UDF[m] == 27)
                        jishutotal += item.JiShu27;

                    if (UDF[m] == 28)
                        jishutotal += item.JiShu27;

                    if (UDF[m] == 29)
                        jishutotal += item.JiShu29;

                    if (UDF[m] == 30)
                        jishutotal += item.JiShu30;

                    if (UDF[m] == 31)
                        jishutotal += item.JiShu31;

                    if (UDF[m] == 32)
                        jishutotal += item.JiShu32;

                    if (UDF[m] == 33)
                        jishutotal += item.JiShu33;

                    if (UDF[m] == 4)
                        jishutotal += item.JiShu34;

                    if (UDF[m] == 35)
                        jishutotal += item.JiShu35;

                    if (UDF[m] == 36)
                        jishutotal += item.JiShu36;

                    if (UDF[m] == 37)
                        jishutotal += item.JiShu37;

                    if (UDF[m] == 38)
                        jishutotal += item.JiShu38;

                    if (UDF[m] == 39)
                        jishutotal += item.JiShu39;

                    if (UDF[m] == 40)
                        jishutotal += item.JiShu40;

                    if (UDF[m] == 41)
                        jishutotal += item.JiShu41;

                    if (UDF[m] == 42)
                        jishutotal += item.JiShu42;

                    if (UDF[m] == 43)
                        jishutotal += item.JiShu43;

                    if (UDF[m] == 44)
                        jishutotal += item.JiShu44;

                    if (UDF[m] == 45)
                        jishutotal += item.JiShu45;

                    if (UDF[m] == 46)
                        jishutotal += item.JiShu46;

                    if (UDF[m] == 47)
                        jishutotal += item.JiShu47;

                    if (UDF[m] == 48)
                        jishutotal += item.JiShu48;

                    if (UDF[m] == 49)
                        jishutotal += item.JiShu49;

                    if (UDF[m] == 50)
                        jishutotal += item.JiShu50;

                }
            }
            else
            {
                if (InitialUDF.Count > 0)
                {
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 0)
                        jishutotal += item.JiShu1;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 1)
                        jishutotal += item.JiShu2;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 2)
                        jishutotal += item.JiShu3;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 3)
                        jishutotal += item.JiShu4;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 4)
                        jishutotal += item.JiShu5;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 5)
                        jishutotal += item.JiShu6;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 6)
                        jishutotal += item.JiShu7;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 7)
                        jishutotal += item.JiShu8;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 8)
                        jishutotal += item.JiShu9;
                    //new 
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 9)
                        jishutotal += item.JiShu10;

                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 10)
                        jishutotal += item.JiShu11;

                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 11)
                        jishutotal += item.JiShu12;

                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 12)
                        jishutotal += item.JiShu13;

                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 13)
                        jishutotal += item.JiShu14;


                    //new 0621
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 14)
                        jishutotal += item.JiShu15;

                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 15)
                        jishutotal += item.JiShu16;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 16)
                        jishutotal += item.JiShu17;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 17)
                        jishutotal += item.JiShu18;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 18)
                        jishutotal += item.JiShu19;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 19)
                        jishutotal += item.JiShu20;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 20)
                        jishutotal += item.JiShu21;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 21)
                        jishutotal += item.JiShu22;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 22)
                        jishutotal += item.JiShu23;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 23)
                        jishutotal += item.JiShu24;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 24)
                        jishutotal += item.JiShu25;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 25)
                        jishutotal += item.JiShu26;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 26)
                        jishutotal += item.JiShu27;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 27)
                        jishutotal += item.JiShu28;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 28)
                        jishutotal += item.JiShu29;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 29)
                        jishutotal += item.JiShu30;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 30)
                        jishutotal += item.JiShu31;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 31)
                        jishutotal += item.JiShu32;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 32)
                        jishutotal += item.JiShu33;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 33)
                        jishutotal += item.JiShu34;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 34)
                        jishutotal += item.JiShu35;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 35)
                        jishutotal += item.JiShu36;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 36)
                        jishutotal += item.JiShu37;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 37)
                        jishutotal += item.JiShu38;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 38)
                        jishutotal += item.JiShu39;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 39)
                        jishutotal += item.JiShu40;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 40)
                        jishutotal += item.JiShu41;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 41)
                        jishutotal += item.JiShu42;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 42)
                        jishutotal += item.JiShu43;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 43)
                        jishutotal += item.JiShu44;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 44)
                        jishutotal += item.JiShu45;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 45)
                        jishutotal += item.JiShu46;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 46)
                        jishutotal += item.JiShu47;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 47)
                        jishutotal += item.JiShu48;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 48)
                        jishutotal += item.JiShu49;
                    if (Convert.ToInt32(InitialUDF[InitialUDF.Count - 1]) > 49)
                        jishutotal += item.JiShu50;

                }

            }
            return jishutotal;
        }

        private void JISHU_Zidingyi_InitialSystemInfo()
        {

            int vony = this.checkedListBox4.Items.Count;
            for (int i = 0; i < vony; i++)
            {
                checkedListBox4.Items.Remove(checkedListBox4.Items[0]);
                this.checkedListBox3.Items.Remove(checkedListBox3.Items[0]);
            }
            clsAllnew BusinessHelp = new clsAllnew();

            List<CaipiaoZhongLeiDATA> CaipiaozhongleiResult = BusinessHelp.Read_CaiPiaoZhongLei_Moren("YES");

            if (CaipiaozhongleiResult.Count == 0)
            {
                MessageBox.Show("彩票默认运行类型没有选中,请到【彩票类型界面】选中彩票类型，点击确认", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string len = CaipiaozhongleiResult[0].Xuan;
            toolStripComboBox5.Items.Clear();
            toolStripComboBox6.Items.Clear();
            comboBox5.Items.Clear();
            comboBox4.Items.Clear();

            for (int i = 0; i < Convert.ToInt32(len); i++)
            {
                int con = i + 1;

                checkedListBox4.Items.Add("第 " + con + " 位");
                this.checkedListBox3.Items.Add("第 " + con + " 位");

                toolStripComboBox5.Items.Add("随机 " + con + " 位");
                toolStripComboBox6.Items.Add("随机 " + con + " 位");
                this.comboBox5.Items.Add("随机 " + con + " 位");
                this.comboBox4.Items.Add("随机 " + con + " 位");

            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 2)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < checkedListBox4.Items.Count; i++)
                {
                    checkedListBox4.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox5.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    checkedListBox4.SetItemChecked(newlist[i], true);
                }
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sq = this.tabControl1.SelectedIndex;
            if (sq == 2)
            {
                List<int> newlist = new List<int>();

                for (int i = 0; i < checkedListBox3.Items.Count; i++)
                {
                    checkedListBox3.SetItemChecked(i, false);
                    newlist.Add(i);

                }
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();
                string[] temp3 = System.Text.RegularExpressions.Regex.Split(comboBox4.Text, " ");
                string index = temp3[1];
                for (int i = 0; i < Convert.ToInt32(index); i++)
                {

                    checkedListBox3.SetItemChecked(newlist[i], true);
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox4.Items.Count; i++)
            {
                checkedListBox4.SetItemChecked(i, true);
                this.checkedListBox3.SetItemChecked(i, true);

            }
        }

        private void dataGridView3_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.LightBlue;
            }
        }

        private void dataGridView3_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 192);
            }
        }

        private void dataGridView3_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel8.Text = "";

            #region 获取选中的基数

            List<int> JIDTA1 = new List<int>();

            if (checkedListBox3.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox3.CheckedItems)
                {
                    if (status.Contains("第"))
                    {
                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(status, " ");
                        JIDTA1.Add(Convert.ToInt32(temp3[1]));
                    }
                    else if (status.Contains("特"))
                    {

                        JIDTA1.Add(10);
                    }
                }


            }

            #endregion
            //toolStripLabel8.Text = GetRowIndexAt(e.Y).ToString();
            int r = this.dataGridView3.HitTest(e.X, e.Y).RowIndex; //行
            int c = this.dataGridView3.HitTest(e.X, e.Y).ColumnIndex; //列

            if (c > 0 && r > 0)
            {
                if (dataGridView3.Columns[c].HeaderText.Contains("前"))
                {
                    int qianqishu = Convert.ToInt32(dataGridView3.Columns[c].HeaderText.Replace("前", "")) + 1;
                    int ii = dataGridView3.Rows.Count - qianqishu;
                    //  int iia = dataGridView1.Rows.Count - ii;
                    //   dataGridView2.CurrentCell = dataGridView2[0, ii];
                    if (ii < 0)
                        return;

                    toolStripLabel8.Text = dataGridView3.Rows[ii].Cells[2].EditedFormattedValue.ToString();

                    #region 切分显示内容
                    if (JIDTA1.Count > 0)
                    {
                        string showmessage = "";

                        string[] temp3 = System.Text.RegularExpressions.Regex.Split(toolStripLabel8.Text, " ");
                        for (int i = 0; i < JIDTA1.Count; i++)
                        {
                            if (JIDTA1[i] - 1 < temp3.Length)
                                showmessage = showmessage + temp3[JIDTA1[i] - 1];

                        }
                        toolStripLabel8.Text = showmessage;

                    }

                    #endregion
                    toolStripLabel8.Text = "  选中信息：" + toolStripLabel8.Text;
                }
                else

                    toolStripLabel8.Text = "  选中信息：请鼠标移动到相应的【前】列上!";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            tab3shuiji = true;
            qianqiqishu = Convert.ToInt32(toolStripComboBox4.Text);

            button4_Click(this, EventArgs.Empty);
            RunTAB3();
            if (checkedListBox4.CheckedItems.Count > 0 || checkedListBox3.CheckedItems.Count > 0)
            {
                toolStripButton2_Click_1(this, EventArgs.Empty);
            }

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox4.Items.Count; i++)
            {
                checkedListBox4.SetItemChecked(i, false);
                this.checkedListBox3.SetItemChecked(i, false);
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void 批量设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmSetConfigList == null)
            {
                frmSetConfigList = new frmSetConfigList();
                frmSetConfigList.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmSetConfigList == null)
            {
                frmSetConfigList = new frmSetConfigList();
            }
            frmSetConfigList.ShowDialog();


            int s = this.tabControl1.SelectedIndex;
            if (s == 0)
            {
                toolStripLabel8.Text = "系统正在读取数据和内部计算，需要一段时间，请稍后....";
                //GetDataforOutlookThread = new Thread(NewMethodtab1);
                //GetDataforOutlookThread.Start();
                isnewfanganlist = 1;

                // NewMethodtab1();

                #region 批量情况
                if (isnewfanganlist == 1)
                {
                    clsAllnew BusinessHelp = new clsAllnew();
                    piliangfangan_Result = new List<FangAnLieBiaoDATA>();
                    piliangfangan_Result = BusinessHelp.Read_Piliang_AllFangAn();




                }
                #endregion
            }
        }

        private void 自选方案ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkfangan();
            Initial_Resulit();
            NewMethodtab2();

        }

        private void 自选方案ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            checkfangan();
            Initial_Resulit();
            //checkfanganindex
            NewMethodtab2();

        }

        private void Initial_Resulit()
        {
            ClaimReport_Server = new List<inputCaipiaoDATA>();
            ClaimReport_Server = ClaimReport_Server_Initial;
        }

        private void checkfangan()
        {
            if (frmSelfPiLiangFangAN == null)
            {
                frmSelfPiLiangFangAN = new frmSelfPiLiangFangAN(ClaimReport_Server);
                frmSelfPiLiangFangAN.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmSelfPiLiangFangAN == null)
            {
                frmSelfPiLiangFangAN = new frmSelfPiLiangFangAN(ClaimReport_Server);
            }
            frmSelfPiLiangFangAN.ShowDialog();
            #region 批量情况
            isnewfanganlist = 1;
            if (isnewfanganlist == 1)
            {
                clsAllnew BusinessHelp = new clsAllnew();
                piliangfangan_Result = new List<FangAnLieBiaoDATA>();
                piliangfangan_Result = BusinessHelp.Read_Piliang_AllFangAn();




            }
            #endregion
        }

        private void NewMethodtab2()
        {
            try
            {

                UDF = new List<int>();

                clsAllnew BusinessHelp = new clsAllnew();

                #region 添加 基数 和前几期对比

                List<FangAnLieBiaoDATA> Result = BusinessHelp.Read_FangAn("YES");
                if (Result.Count != 0)
                {
                    this.label4.Text = "当前方案名称：　" + Result[0].Name;
                    if (Result[0].Data != null)
                    {
                        toolStripLabel7.Text = Result[0].Data.Replace("\r\n", "* ");
                        //toolStripLabel9.Text = Result[0].Data.Replace("\r\n", "* ");


                    }
                }

                ClaimReport_Server.Sort(new CompsSmall());
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {

                    {

                        if (item.KaiJianHaoMa == null)
                            continue;

                        string[] temp2 = System.Text.RegularExpressions.Regex.Split(item.KaiJianHaoMa, " ");
                        for (int ii = 0; ii < temp2.Length; ii++)
                        {
                            int dsjsk = ii + 1;
                            var itemaa = checkfanganindex.Find((x) => { return x.Contains("第" + dsjsk.ToString() + "位"); });
                            if (itemaa == null)
                                continue;
                            FangAnLieBiaoDATA filtered = piliangfangan_Result.Find(s => itemaa.Contains(s.Name));
                            string[] temp1 = System.Text.RegularExpressions.Regex.Split(filtered.Data, "\r\n");
                            for (int i = 1; i < temp1.Length; i++)
                            {
                                string[] temp3 = System.Text.RegularExpressions.Regex.Split(temp1[i], "段");
                                int ss = ii + 1;
                                #region 如果不再选中范围内不进行检查

                                bool isrun = false;

                                for (int j = 0; j < checkfanganindex.Count; j++)
                                {
                                    if (checkfanganindex[j].Contains("第" + ss.ToString() + "位"))
                                    {
                                        isrun = true;
                                        var nn = UDF.Find((x) => { return x.ToString().Contains(ss.ToString()); });
                                        if (nn == 0)
                                            UDF.Add(Convert.ToInt32(ss));
                                    }
                                }
                                if (isrun == false)
                                    continue;
                                #endregion

                                if (temp3[1].Contains(temp2[ii]))
                                {


                                    item.JiShu = item.JiShu + "基" + ss.ToString() + " " + temp3[0];
                                    if (ss == 1)
                                        item.JiShu1 = temp3[0];
                                    else if (ss == 2)
                                        item.JiShu2 = temp3[0];
                                    else if (ss == 3)
                                        item.JiShu3 = temp3[0];
                                    else if (ss == 4)
                                        item.JiShu4 = temp3[0];
                                    else if (ss == 5)
                                        item.JiShu5 = temp3[0];
                                    else if (ss == 6)
                                        item.JiShu6 = temp3[0];
                                    else if (ss == 7)
                                        item.JiShu7 = temp3[0];
                                    else if (ss == 8)
                                        item.JiShu8 = temp3[0];
                                    else if (ss == 9)
                                        item.JiShu9 = temp3[0];

                                    //new 

                                    else if (ss == 10)
                                        item.JiShu10 = temp3[0];

                                    else if (ss == 11)
                                        item.JiShu11 = temp3[0];

                                    else if (ss == 12)
                                        item.JiShu12 = temp3[0];

                                    else if (ss == 13)
                                        item.JiShu13 = temp3[0];

                                    else if (ss == 14)
                                        item.JiShu14 = temp3[0];

                                    else if (ss == 15)
                                        item.JiShu15 = temp3[0];
                                    //new 0621

                                    else if (ss == 16)
                                        item.JiShu16 = temp3[0];

                                    else if (ss == 17)
                                        item.JiShu17 = temp3[0];

                                    else if (ss == 18)
                                        item.JiShu18 = temp3[0];

                                    else if (ss == 19)
                                        item.JiShu19 = temp3[0];

                                    else if (ss == 20)
                                        item.JiShu20 = temp3[0];

                                    else if (ss == 21)
                                        item.JiShu21 = temp3[0];

                                    else if (ss == 22)
                                        item.JiShu22 = temp3[0];

                                    else if (ss == 23)
                                        item.JiShu23 = temp3[0];

                                    else if (ss == 24)
                                        item.JiShu24 = temp3[0];

                                    else if (ss == 25)
                                        item.JiShu25 = temp3[0];

                                    else if (ss == 26)
                                        item.JiShu26 = temp3[0];

                                    else if (ss == 27)
                                        item.JiShu27 = temp3[0];

                                    else if (ss == 28)
                                        item.JiShu28 = temp3[0];

                                    else if (ss == 29)
                                        item.JiShu29 = temp3[0];

                                    else if (ss == 30)
                                        item.JiShu30 = temp3[0];


                                    else if (ss == 31)
                                        item.JiShu31 = temp3[0];

                                    else if (ss == 32)
                                        item.JiShu32 = temp3[0];

                                    else if (ss == 33)
                                        item.JiShu33 = temp3[0];

                                    else if (ss == 34)
                                        item.JiShu34 = temp3[0];

                                    else if (ss == 35)
                                        item.JiShu35 = temp3[0];

                                    else if (ss == 36)
                                        item.JiShu36 = temp3[0];

                                    else if (ss == 37)
                                        item.JiShu37 = temp3[0];

                                    else if (ss == 38)
                                        item.JiShu38 = temp3[0];

                                    else if (ss == 39)
                                        item.JiShu39 = temp3[0];

                                    else if (ss == 40)
                                        item.JiShu40 = temp3[0];

                                    else if (ss == 41)
                                        item.JiShu41 = temp3[0];

                                    else if (ss == 42)
                                        item.JiShu42 = temp3[0];

                                    else if (ss == 43)
                                        item.JiShu43 = temp3[0];

                                    else if (ss == 44)
                                        item.JiShu44 = temp3[0];

                                    else if (ss == 45)
                                        item.JiShu45 = temp3[0];

                                    else if (ss == 46)
                                        item.JiShu46 = temp3[0];

                                    else if (ss == 47)
                                        item.JiShu47 = temp3[0];

                                    else if (ss == 48)
                                        item.JiShu48 = temp3[0];

                                    else if (ss == 49)
                                        item.JiShu49 = temp3[0];

                                    else if (ss == 50)
                                        item.JiShu50 = temp3[0];

                                    break;

                                }
                            }

                        }
                    }
                }

                #endregion

                int indexing = 0;
                foreach (inputCaipiaoDATA item in ClaimReport_Server)
                {
                    indexing = 0;
                    foreach (inputCaipiaoDATA temp in ClaimReport_Server)
                    {
                        if (Convert.ToInt32(item.QiHao) > Convert.ToInt32(temp.QiHao))
                        {
                            indexing++;
                            int xiangtongindex = 0;
                            #region 判断选中的基数尾数式否应该判断此范围
                            //int dsjsk1 = indexing;
                            //var itemaa1 = checkfanganindex.Find((x) => { return x.Contains("第" + indexing.ToString() + "位"); });
                            //if (itemaa1 == null)
                            //    continue;
                            #endregion
                            #region 匹配相同次数
                            for (int j = 0; j < UDF.Count; j++)
                            {
                                if (item.JiShu1 != null && item.JiShu1 == temp.JiShu1 && UDF[j] == 1)
                                    xiangtongindex++;
                                if (item.JiShu2 != null && item.JiShu2 == temp.JiShu2 && UDF[j] == 2)
                                    xiangtongindex++;
                                if (item.JiShu3 != null && item.JiShu3 == temp.JiShu3 && UDF[j] == 3)
                                    xiangtongindex++;
                                if (item.JiShu4 != null && item.JiShu4 == temp.JiShu4 && UDF[j] == 4)
                                    xiangtongindex++;
                                if (item.JiShu5 != null && item.JiShu5 == temp.JiShu5 && UDF[j] == 5)
                                    xiangtongindex++;
                                if (item.JiShu6 != null && item.JiShu6 == temp.JiShu6 && UDF[j] == 6)
                                    xiangtongindex++;
                                if (item.JiShu7 != null && item.JiShu7 == temp.JiShu7 && UDF[j] == 7)
                                    xiangtongindex++;
                                if (item.JiShu8 != null && item.JiShu8 == temp.JiShu8 && UDF[j] == 8)
                                    xiangtongindex++;
                                if (item.JiShu9 != null && item.JiShu9 == temp.JiShu9 && UDF[j] == 9)
                                    xiangtongindex++;
                                //new
                                if (item.JiShu10 != null && item.JiShu10 == temp.JiShu10)
                                    xiangtongindex++;
                                if (item.JiShu11 != null && item.JiShu11 == temp.JiShu11)
                                    xiangtongindex++;
                                if (item.JiShu12 != null && item.JiShu12 == temp.JiShu12)
                                    xiangtongindex++;
                                if (item.JiShu13 != null && item.JiShu13 == temp.JiShu13)
                                    xiangtongindex++;
                                if (item.JiShu14 != null && item.JiShu14 == temp.JiShu14)
                                    xiangtongindex++;
                                if (item.JiShu15 != null && item.JiShu15 == temp.JiShu15)
                                    xiangtongindex++;
                                //new 0621
                                if (item.JiShu16 != null && item.JiShu16 == temp.JiShu16)
                                    xiangtongindex++;
                                if (item.JiShu17 != null && item.JiShu17 == temp.JiShu17)
                                    xiangtongindex++;
                                if (item.JiShu18 != null && item.JiShu18 == temp.JiShu18)
                                    xiangtongindex++;
                                if (item.JiShu19 != null && item.JiShu19 == temp.JiShu19)
                                    xiangtongindex++;
                                if (item.JiShu20 != null && item.JiShu20 == temp.JiShu20)
                                    xiangtongindex++;
                                if (item.JiShu21 != null && item.JiShu21 == temp.JiShu21)
                                    xiangtongindex++;
                                if (item.JiShu22 != null && item.JiShu22 == temp.JiShu22)
                                    xiangtongindex++;
                                if (item.JiShu23 != null && item.JiShu23 == temp.JiShu23)
                                    xiangtongindex++;
                                if (item.JiShu24 != null && item.JiShu24 == temp.JiShu24)
                                    xiangtongindex++;
                                if (item.JiShu25 != null && item.JiShu25 == temp.JiShu25)
                                    xiangtongindex++;
                                if (item.JiShu26 != null && item.JiShu26 == temp.JiShu26)
                                    xiangtongindex++;
                                if (item.JiShu27 != null && item.JiShu27 == temp.JiShu27)
                                    xiangtongindex++;
                                if (item.JiShu28 != null && item.JiShu28 == temp.JiShu28)
                                    xiangtongindex++;
                                if (item.JiShu29 != null && item.JiShu29 == temp.JiShu29)
                                    xiangtongindex++;
                                if (item.JiShu30 != null && item.JiShu30 == temp.JiShu30)
                                    xiangtongindex++;

                                if (item.JiShu32 != null && item.JiShu32 == temp.JiShu32)
                                    xiangtongindex++;

                                if (item.JiShu33 != null && item.JiShu33 == temp.JiShu33)
                                    xiangtongindex++;

                                if (item.JiShu34 != null && item.JiShu34 == temp.JiShu34)
                                    xiangtongindex++;

                                if (item.JiShu35 != null && item.JiShu35 == temp.JiShu35)
                                    xiangtongindex++;

                                if (item.JiShu36 != null && item.JiShu36 == temp.JiShu36)
                                    xiangtongindex++;

                                if (item.JiShu37 != null && item.JiShu37 == temp.JiShu37)
                                    xiangtongindex++;

                                if (item.JiShu38 != null && item.JiShu38 == temp.JiShu38)
                                    xiangtongindex++;

                                if (item.JiShu39 != null && item.JiShu39 == temp.JiShu39)
                                    xiangtongindex++;

                                if (item.JiShu40 != null && item.JiShu40 == temp.JiShu40)
                                    xiangtongindex++;

                                if (item.JiShu41 != null && item.JiShu41 == temp.JiShu41)
                                    xiangtongindex++;

                                if (item.JiShu42 != null && item.JiShu42 == temp.JiShu42)
                                    xiangtongindex++;

                                if (item.JiShu43 != null && item.JiShu43 == temp.JiShu43)
                                    xiangtongindex++;

                                if (item.JiShu44 != null && item.JiShu44 == temp.JiShu44)
                                    xiangtongindex++;

                                if (item.JiShu45 != null && item.JiShu45 == temp.JiShu45)
                                    xiangtongindex++;

                                if (item.JiShu46 != null && item.JiShu46 == temp.JiShu46)
                                    xiangtongindex++;

                                if (item.JiShu47 != null && item.JiShu47 == temp.JiShu47)
                                    xiangtongindex++;

                                if (item.JiShu48 != null && item.JiShu48 == temp.JiShu48)
                                    xiangtongindex++;

                                if (item.JiShu49 != null && item.JiShu49 == temp.JiShu49)
                                    xiangtongindex++;

                                if (item.JiShu50 != null && item.JiShu50 == temp.JiShu50)
                                    xiangtongindex++;





                            }
                            #endregion
                            #region MyRegion


                            if (indexing == 1)
                                item.qian1 = xiangtongindex.ToString();
                            else if (indexing == 2) item.qian2 = xiangtongindex.ToString();
                            else if (indexing == 3) item.qian3 = xiangtongindex.ToString();
                            else if (indexing == 4) item.qian4 = xiangtongindex.ToString();
                            else if (indexing == 5) item.qian5 = xiangtongindex.ToString();
                            else if (indexing == 6) item.qian6 = xiangtongindex.ToString();
                            else if (indexing == 7) item.qian7 = xiangtongindex.ToString();
                            else if (indexing == 8) item.qian8 = xiangtongindex.ToString();
                            else if (indexing == 9) item.qian9 = xiangtongindex.ToString();
                            else if (indexing == 10) item.qian10 = xiangtongindex.ToString();
                            else if (indexing == 11) item.qian11 = xiangtongindex.ToString();
                            else if (indexing == 12) item.qian12 = xiangtongindex.ToString();
                            else if (indexing == 13) item.qian13 = xiangtongindex.ToString();
                            else if (indexing == 14) item.qian14 = xiangtongindex.ToString();
                            else if (indexing == 15) item.qian15 = xiangtongindex.ToString();
                            else if (indexing == 16) item.qian16 = xiangtongindex.ToString();
                            else if (indexing == 17) item.qian17 = xiangtongindex.ToString();
                            else if (indexing == 18) item.qian18 = xiangtongindex.ToString();
                            else if (indexing == 19) item.qian19 = xiangtongindex.ToString();
                            else if (indexing == 20) item.qian20 = xiangtongindex.ToString();
                            else if (indexing == 21) item.qian21 = xiangtongindex.ToString();
                            else if (indexing == 22) item.qian22 = xiangtongindex.ToString();
                            else if (indexing == 23) item.qian23 = xiangtongindex.ToString();
                            else if (indexing == 24) item.qian24 = xiangtongindex.ToString();
                            else if (indexing == 25) item.qian25 = xiangtongindex.ToString();
                            else if (indexing == 26) item.qian26 = xiangtongindex.ToString();
                            else if (indexing == 27) item.qian27 = xiangtongindex.ToString();
                            else if (indexing == 28) item.qian28 = xiangtongindex.ToString();
                            else if (indexing == 29) item.qian29 = xiangtongindex.ToString();
                            else if (indexing == 30) item.qian30 = xiangtongindex.ToString();
                            else if (indexing == 31) item.qian31 = xiangtongindex.ToString();
                            else if (indexing == 32) item.qian32 = xiangtongindex.ToString();
                            else if (indexing == 33) item.qian33 = xiangtongindex.ToString();
                            else if (indexing == 34) item.qian34 = xiangtongindex.ToString();
                            else if (indexing == 35) item.qian35 = xiangtongindex.ToString();
                            else if (indexing == 36) item.qian36 = xiangtongindex.ToString();
                            else if (indexing == 37) item.qian37 = xiangtongindex.ToString();
                            else if (indexing == 38) item.qian38 = xiangtongindex.ToString();
                            else if (indexing == 39) item.qian39 = xiangtongindex.ToString();
                            else if (indexing == 40) item.qian40 = xiangtongindex.ToString();
                            else if (indexing == 41) item.qian41 = xiangtongindex.ToString();
                            else if (indexing == 42) item.qian42 = xiangtongindex.ToString();
                            else if (indexing == 43) item.qian43 = xiangtongindex.ToString();
                            else if (indexing == 44) item.qian44 = xiangtongindex.ToString();
                            else if (indexing == 45) item.qian45 = xiangtongindex.ToString();
                            else if (indexing == 46) item.qian46 = xiangtongindex.ToString();
                            else if (indexing == 47) item.qian47 = xiangtongindex.ToString();
                            else if (indexing == 48) item.qian48 = xiangtongindex.ToString();
                            else if (indexing == 49) item.qian49 = xiangtongindex.ToString();
                            else if (indexing == 50) item.qian50 = xiangtongindex.ToString();
                            else if (indexing == 51) item.qian51 = xiangtongindex.ToString();
                            else if (indexing == 52) item.qian52 = xiangtongindex.ToString();
                            else if (indexing == 53) item.qian53 = xiangtongindex.ToString();
                            else if (indexing == 54) item.qian54 = xiangtongindex.ToString();
                            else if (indexing == 55) item.qian55 = xiangtongindex.ToString();
                            else if (indexing == 56) item.qian56 = xiangtongindex.ToString();
                            else if (indexing == 57) item.qian57 = xiangtongindex.ToString();
                            else if (indexing == 58) item.qian58 = xiangtongindex.ToString();
                            else if (indexing == 59) item.qian59 = xiangtongindex.ToString();
                            else if (indexing == 60) item.qian60 = xiangtongindex.ToString();
                            else if (indexing == 61) item.qian61 = xiangtongindex.ToString();
                            else if (indexing == 62) item.qian62 = xiangtongindex.ToString();
                            else if (indexing == 63) item.qian63 = xiangtongindex.ToString();
                            else if (indexing == 64) item.qian64 = xiangtongindex.ToString();
                            else if (indexing == 65) item.qian65 = xiangtongindex.ToString();
                            else if (indexing == 66) item.qian66 = xiangtongindex.ToString();
                            else if (indexing == 67) item.qian67 = xiangtongindex.ToString();
                            else if (indexing == 68) item.qian68 = xiangtongindex.ToString();
                            else if (indexing == 69) item.qian69 = xiangtongindex.ToString();
                            else if (indexing == 70) item.qian70 = xiangtongindex.ToString();
                            else if (indexing == 71) item.qian71 = xiangtongindex.ToString();
                            else if (indexing == 72) item.qian72 = xiangtongindex.ToString();
                            else if (indexing == 73) item.qian73 = xiangtongindex.ToString();
                            else if (indexing == 74) item.qian74 = xiangtongindex.ToString();
                            else if (indexing == 75) item.qian75 = xiangtongindex.ToString();
                            else if (indexing == 76) item.qian76 = xiangtongindex.ToString();
                            else if (indexing == 77) item.qian77 = xiangtongindex.ToString();
                            else if (indexing == 78) item.qian78 = xiangtongindex.ToString();
                            else if (indexing == 79) item.qian79 = xiangtongindex.ToString();
                            else if (indexing == 80) item.qian80 = xiangtongindex.ToString();
                            else if (indexing == 81) item.qian81 = xiangtongindex.ToString();
                            else if (indexing == 82) item.qian82 = xiangtongindex.ToString();
                            else if (indexing == 83) item.qian83 = xiangtongindex.ToString();
                            else if (indexing == 84) item.qian84 = xiangtongindex.ToString();
                            else if (indexing == 85) item.qian85 = xiangtongindex.ToString();
                            else if (indexing == 86) item.qian86 = xiangtongindex.ToString();
                            else if (indexing == 87) item.qian87 = xiangtongindex.ToString();
                            else if (indexing == 88) item.qian88 = xiangtongindex.ToString();
                            else if (indexing == 89) item.qian89 = xiangtongindex.ToString();
                            else if (indexing == 90) item.qian90 = xiangtongindex.ToString();
                            else if (indexing == 91) item.qian91 = xiangtongindex.ToString();
                            else if (indexing == 92) item.qian92 = xiangtongindex.ToString();
                            else if (indexing == 93) item.qian93 = xiangtongindex.ToString();
                            else if (indexing == 94) item.qian94 = xiangtongindex.ToString();
                            else if (indexing == 95) item.qian95 = xiangtongindex.ToString();
                            else if (indexing == 96) item.qian96 = xiangtongindex.ToString();
                            else if (indexing == 97) item.qian97 = xiangtongindex.ToString();
                            else if (indexing == 98) item.qian98 = xiangtongindex.ToString();
                            else if (indexing == 99) item.qian99 = xiangtongindex.ToString();
                            //   20180630
                            else if (indexing == 100) item.qian100 = xiangtongindex.ToString();
                            else if (indexing == 101) item.qian101 = xiangtongindex.ToString();
                            else if (indexing == 102) item.qian102 = xiangtongindex.ToString();
                            else if (indexing == 103) item.qian103 = xiangtongindex.ToString();
                            else if (indexing == 104) item.qian104 = xiangtongindex.ToString();
                            else if (indexing == 105) item.qian105 = xiangtongindex.ToString();
                            else if (indexing == 106) item.qian106 = xiangtongindex.ToString();
                            else if (indexing == 107) item.qian107 = xiangtongindex.ToString();
                            else if (indexing == 108) item.qian108 = xiangtongindex.ToString();
                            else if (indexing == 109) item.qian109 = xiangtongindex.ToString();
                            else if (indexing == 110) item.qian110 = xiangtongindex.ToString();
                            else if (indexing == 111) item.qian111 = xiangtongindex.ToString();
                            else if (indexing == 112) item.qian112 = xiangtongindex.ToString();
                            else if (indexing == 113) item.qian113 = xiangtongindex.ToString();
                            else if (indexing == 114) item.qian114 = xiangtongindex.ToString();
                            else if (indexing == 115) item.qian115 = xiangtongindex.ToString();
                            else if (indexing == 116) item.qian116 = xiangtongindex.ToString();
                            else if (indexing == 117) item.qian117 = xiangtongindex.ToString();
                            else if (indexing == 118) item.qian118 = xiangtongindex.ToString();
                            else if (indexing == 119) item.qian119 = xiangtongindex.ToString();
                            else if (indexing == 120) item.qian120 = xiangtongindex.ToString();
                            else if (indexing == 121) item.qian121 = xiangtongindex.ToString();
                            else if (indexing == 122) item.qian122 = xiangtongindex.ToString();
                            else if (indexing == 123) item.qian123 = xiangtongindex.ToString();
                            else if (indexing == 124) item.qian124 = xiangtongindex.ToString();
                            else if (indexing == 125) item.qian125 = xiangtongindex.ToString();
                            else if (indexing == 126) item.qian126 = xiangtongindex.ToString();
                            else if (indexing == 127) item.qian127 = xiangtongindex.ToString();
                            else if (indexing == 128) item.qian128 = xiangtongindex.ToString();
                            else if (indexing == 129) item.qian129 = xiangtongindex.ToString();
                            else if (indexing == 130) item.qian130 = xiangtongindex.ToString();
                            else if (indexing == 131) item.qian131 = xiangtongindex.ToString();
                            else if (indexing == 132) item.qian132 = xiangtongindex.ToString();
                            else if (indexing == 133) item.qian133 = xiangtongindex.ToString();
                            else if (indexing == 134) item.qian134 = xiangtongindex.ToString();
                            else if (indexing == 135) item.qian135 = xiangtongindex.ToString();
                            else if (indexing == 136) item.qian136 = xiangtongindex.ToString();
                            else if (indexing == 137) item.qian137 = xiangtongindex.ToString();
                            else if (indexing == 138) item.qian138 = xiangtongindex.ToString();
                            else if (indexing == 139) item.qian139 = xiangtongindex.ToString();
                            else if (indexing == 140) item.qian140 = xiangtongindex.ToString();
                            else if (indexing == 141) item.qian141 = xiangtongindex.ToString();
                            else if (indexing == 142) item.qian142 = xiangtongindex.ToString();
                            else if (indexing == 143) item.qian143 = xiangtongindex.ToString();
                            else if (indexing == 144) item.qian144 = xiangtongindex.ToString();
                            else if (indexing == 145) item.qian145 = xiangtongindex.ToString();
                            else if (indexing == 146) item.qian146 = xiangtongindex.ToString();
                            else if (indexing == 147) item.qian147 = xiangtongindex.ToString();
                            else if (indexing == 148) item.qian148 = xiangtongindex.ToString();
                            else if (indexing == 149) item.qian149 = xiangtongindex.ToString();
                            else if (indexing == 150) item.qian150 = xiangtongindex.ToString();
                            else if (indexing == 151) item.qian151 = xiangtongindex.ToString();
                            else if (indexing == 152) item.qian152 = xiangtongindex.ToString();
                            else if (indexing == 153) item.qian153 = xiangtongindex.ToString();
                            else if (indexing == 154) item.qian154 = xiangtongindex.ToString();
                            else if (indexing == 155) item.qian155 = xiangtongindex.ToString();
                            else if (indexing == 156) item.qian156 = xiangtongindex.ToString();
                            else if (indexing == 157) item.qian157 = xiangtongindex.ToString();
                            else if (indexing == 158) item.qian158 = xiangtongindex.ToString();
                            else if (indexing == 159) item.qian159 = xiangtongindex.ToString();
                            else if (indexing == 160) item.qian160 = xiangtongindex.ToString();
                            else if (indexing == 161) item.qian161 = xiangtongindex.ToString();
                            else if (indexing == 162) item.qian162 = xiangtongindex.ToString();
                            else if (indexing == 163) item.qian163 = xiangtongindex.ToString();
                            else if (indexing == 164) item.qian164 = xiangtongindex.ToString();
                            else if (indexing == 165) item.qian165 = xiangtongindex.ToString();
                            else if (indexing == 166) item.qian166 = xiangtongindex.ToString();
                            else if (indexing == 167) item.qian167 = xiangtongindex.ToString();
                            else if (indexing == 168) item.qian168 = xiangtongindex.ToString();
                            else if (indexing == 169) item.qian169 = xiangtongindex.ToString();
                            else if (indexing == 170) item.qian170 = xiangtongindex.ToString();
                            else if (indexing == 171) item.qian171 = xiangtongindex.ToString();
                            else if (indexing == 172) item.qian172 = xiangtongindex.ToString();
                            else if (indexing == 173) item.qian173 = xiangtongindex.ToString();
                            else if (indexing == 174) item.qian174 = xiangtongindex.ToString();
                            else if (indexing == 175) item.qian175 = xiangtongindex.ToString();
                            else if (indexing == 176) item.qian176 = xiangtongindex.ToString();
                            else if (indexing == 177) item.qian177 = xiangtongindex.ToString();
                            else if (indexing == 178) item.qian178 = xiangtongindex.ToString();
                            else if (indexing == 179) item.qian179 = xiangtongindex.ToString();
                            else if (indexing == 180) item.qian180 = xiangtongindex.ToString();
                            else if (indexing == 181) item.qian181 = xiangtongindex.ToString();
                            else if (indexing == 182) item.qian182 = xiangtongindex.ToString();
                            else if (indexing == 183) item.qian183 = xiangtongindex.ToString();
                            else if (indexing == 184) item.qian184 = xiangtongindex.ToString();
                            else if (indexing == 185) item.qian185 = xiangtongindex.ToString();
                            else if (indexing == 186) item.qian186 = xiangtongindex.ToString();
                            else if (indexing == 187) item.qian187 = xiangtongindex.ToString();
                            else if (indexing == 188) item.qian188 = xiangtongindex.ToString();
                            else if (indexing == 189) item.qian189 = xiangtongindex.ToString();
                            else if (indexing == 190) item.qian190 = xiangtongindex.ToString();
                            else if (indexing == 191) item.qian191 = xiangtongindex.ToString();
                            else if (indexing == 192) item.qian192 = xiangtongindex.ToString();
                            else if (indexing == 193) item.qian193 = xiangtongindex.ToString();
                            else if (indexing == 194) item.qian194 = xiangtongindex.ToString();
                            else if (indexing == 195) item.qian195 = xiangtongindex.ToString();
                            else if (indexing == 196) item.qian196 = xiangtongindex.ToString();
                            else if (indexing == 197) item.qian197 = xiangtongindex.ToString();
                            else if (indexing == 198) item.qian198 = xiangtongindex.ToString();
                            else if (indexing == 199) item.qian199 = xiangtongindex.ToString();
                            else if (indexing == 200) item.qian200 = xiangtongindex.ToString();
                            #endregion

                        }
                    }
                }
                #region 显示信息

                if (ClaimReport_Server.Count != 0)
                {
                    NewMethod();

                }

                toolStripLabel8.Text = "运行结束";
                MessageBox.Show(toolStripLabel8.Text);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

                throw;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                string getinfo = dataGridView1.Columns[e.ColumnIndex].HeaderText;
                if (getinfo.Contains("基"))
                {
                    var itemaa = checkfanganindex.Find((x) => { return x.Contains("第" + getinfo.Replace("基", "").ToString() + "位"); });
                    if (itemaa == null)
                        return;
                    FangAnLieBiaoDATA filtered = piliangfangan_Result.Find(s => itemaa.Contains(s.Name));
                    toolStripLabel7.Text = "第" + getinfo.Replace("基", "").ToString() + "位 " + filtered.Data.Replace("\r\n", "*");

                }
            }

        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex == 0)
            {
                if (dataGridView1.Rows.Count != 0)
                {
                    int ii = dataGridView1.Rows.Count - 1;
                    dataGridView1.CurrentCell = dataGridView1[0, ii]; // 强制将光标指向i行
                    dataGridView1.Rows[ii].Selected = true;   //光标显示至i行 
                }

            }
        }

        private void 相同性分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (相同性分析ToolStripMenuItem.Text.Contains("取消"))
            {

                相同性分析ToolStripMenuItem.Text = "相同性分析";
                xiangtongxingfenxi = "";

            }

            else
            {

                相同性分析ToolStripMenuItem.Text = "取消相同性分析";
                xiangtongxingfenxi = "YES";
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (toolStripMenuItem2.Text.Contains("取消"))
            {

                toolStripMenuItem2.Text = "相同性分析";
                xiangtongxingfenxi = "";

            }

            else
            {

                toolStripMenuItem2.Text = "取消相同性分析";
                xiangtongxingfenxi = "YES";
            }
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbStatus.Items.Count; i++)
            {
                clbStatus.SetItemChecked(i, true);
                this.checkedListBox1.SetItemChecked(i, true);

            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            suiji20_30 = 1;

            //20230427 添加随机

            comboBox2_SelectedIndexChanged(this, EventArgs.Empty);
            toolStripButton3_Click(this, EventArgs.Empty);







        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {


            bool istrue = true;
            //clsmytest buiness = new clsmytest();

            //bool istue = buiness.checkname("MasterClassified", "yhltd");
            //if (istue == false)
            {
                //Control.CheckForIllegalCrossThreadCalls = false;
                //this.Visible = false;
                MessageBox.Show(toolStripLabel7.Text, "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //var form = new frmAlterinfo(toolStripLabel7.Text);

                //if (form.ShowDialog() == DialogResult.OK)
                //{

                //}


                //System.Environment.Exit(0);
            }

            //IsRun = false;
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            suiji20_30 = 2;

            //20230427 添加随机

            comboBox2_SelectedIndexChanged(this, EventArgs.Empty);
            toolStripButton3_Click(this, EventArgs.Empty);



        }

        private void checkedListBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel8.Text = "   ";
            //  qianqi_newi = new List<int>();
            if (this.checkedListBox1.CheckedItems.Count > 0)
            {
                foreach (string status in this.checkedListBox1.CheckedItems)
                {
                    //  qianqi_newi.Add(Convert.ToInt32(status.Replace("第 ", "").Replace(" 位", "")));
                    toolStripLabel8.Text = toolStripLabel8.Text + status;
                }
            }


        }

        private void 复制选中信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nem = toolStripLabel8.Text.ToString();
            nem = nem.Replace("选中信息：", "");

            //string list1Remove =  listBox3.SelectedItem.ToString();
            Clipboard.SetText(nem);
        }

    }
}
