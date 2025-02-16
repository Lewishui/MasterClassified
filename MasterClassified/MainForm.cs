﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MC.Common;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using MC.Buiness;
using MC.DB;

namespace MasterClassified
{
    public partial class MainForm : Form
    {
        private frmNavigate frmNavigate;
        private frmTracing_Analysis frmTracing_Analysis;
        private frmImport_Data frmImport_Data;
        public log4net.ILog ProcessLogger;
        public log4net.ILog ExceptionLogger;
        public string useadmin;
        public string usename;
        private System.Timers.Timer timerAlter1;
        Sunisoft.IrisSkin.SkinEngine se = null;
        private TextBox txtSAPPassword;
        private CheckBox chkSaveInfo;
        private frmDataCenter frmDataCenter;
        private bool IsRun1 = false;
        private Thread GetDataforRawDataThread;
        private int alterisrun;
        private frmMCdata frmMCdata;
        private frmImport_MCleixing_Data frmImport_MCleixing_Data;
        List<int> newlist;
        List<string> showSuijiResultlist = new List<string>();
        private System.Timers.Timer timerAlter_new;
        private bool IsRun = false;
        public MainForm()
        {
            InitializeComponent();
           // InitialPassword();
            InitialSystemInfo();
            //临时代码
            usename = txtSAPUserId.Text.Trim();
            se = new Sunisoft.IrisSkin.SkinEngine();
            se.SkinAllForm = true;
            se.SkinFile = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""), "GlassBrown.ssk");
            this.Text = "Master Classified System  " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ProcessLogger.Fatal("initialize" + DateTime.Now.ToString());

            Version ver = System.Environment.OSVersion.Version;
            #region Noway
            //DateTime oldDate = DateTime.Now;
            //DateTime dt3;
            //string endday = DateTime.Now.ToString("yyyy/MM/dd");
            //dt3 = Convert.ToDateTime(endday);
            //DateTime dt2;
            //dt2 = Convert.ToDateTime("2017/03/11");

            //TimeSpan ts = dt2 - dt3;
            //int timeTotal = ts.Days;
            //if (timeTotal < 0)
            //{
            //    MessageBox.Show("Please Contact your administrator !");
            //    return;
            //}
            #endregion
            //NewMethod();

        }
        void FrmOMS_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sender is frmNavigate)
            {
                frmNavigate = null;
            }
            if (sender is frmTracing_Analysis)
            {
                frmTracing_Analysis = null;
            }
            if (sender is frmImport_Data)
            {
                frmImport_Data = null;
            }
            if (sender is frmDataCenter)
            {
                frmDataCenter = null;
            }
            if (sender is frmMCdata)
            {
                frmMCdata = null;
            }
            if (sender is frmImport_MCleixing_Data)
            {
                frmImport_MCleixing_Data = null;
            }

        }
        private void InitialSystemInfo()
        {
            #region 初始化配置
            ProcessLogger = log4net.LogManager.GetLogger("ProcessLogger");
            ExceptionLogger = log4net.LogManager.GetLogger("SystemExceptionLogger");
            ProcessLogger.Fatal("System Start " + DateTime.Now.ToString());
            #endregion
        }
        
        #region control

        private void NewMethod()
        {
            timerAlter_new = new System.Timers.Timer(6666);
            timerAlter_new.Elapsed += new System.Timers.ElapsedEventHandler(TimeControl);
            timerAlter_new.AutoReset = true;
            timerAlter_new.Start();
        }
        private void TimeControl(object sender, EventArgs e)
        {
            if (!IsRun)
            {
                IsRun = true;
                GetDataforRawDataThread = new Thread(TimeMethod);
                GetDataforRawDataThread.Start();
            }
        }
        private void TimeMethod()
        {
            bool istrue = true;
            clsmytest buiness = new clsmytest();

            bool istue = buiness.checkname("MasterClassified", "yhltd");
            if (istue == false)
            {
                Control.CheckForIllegalCrossThreadCalls = false;
                this.Visible = false;
                //MessageBox.Show("缺失系统文件，或电脑系统更新导致，请联系开发人员 !", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var form = new frmAlterinfo("缺失系统文件，或电脑系统更新导致，请联系开发人员 !");

                if (form.ShowDialog() == DialogResult.OK)
                {

                }


                System.Environment.Exit(0);
            }

            IsRun = false;
        }

        #endregion
        private void pBBToolStripMenuItem_Click(object sender, EventArgs e)
        {


            if (frmNavigate == null)
            {
                frmNavigate = new frmNavigate();
                frmNavigate.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmNavigate == null)
            {
                frmNavigate = new frmNavigate();
            }
            frmNavigate.Show();//this.dockPanel2
        }
        private void InitialPassword()
        {
            try
            {
                txtSAPPassword = new TextBox();
                txtSAPPassword.PasswordChar = '*';
                ToolStripControlHost t = new ToolStripControlHost(txtSAPPassword);
                t.Width = 100;
                t.AutoSize = false;
                t.Alignment = ToolStripItemAlignment.Right;
                this.toolStrip1.Items.Insert(this.toolStrip1.Items.Count - 4, t);

                chkSaveInfo = new CheckBox();
                chkSaveInfo.Text = "";
                chkSaveInfo.Padding = new Padding(5, 2, 0, 0);
                ToolStripControlHost t1 = new ToolStripControlHost(chkSaveInfo);
                t1.AutoSize = true;

                t1.ToolTipText = clsShowMessage.MSG_002;
                t1.Alignment = ToolStripItemAlignment.Right;
                this.toolStrip1.Items.Insert(this.toolStrip1.Items.Count - 5, t1);
                getUserAndPassword();
                chkSaveInfo.Checked = false;

            }
            catch (Exception ex)
            {
                //clsLogPrint.WriteLog("<frmMain> InitialPassword:" + ex.Message);
                throw ex;
            }
        }
        private void getUserAndPassword()
        {
            try
            {
                RegistryKey rkLocalMachine = Registry.LocalMachine;
                RegistryKey rkSoftWare = rkLocalMachine.OpenSubKey(clsConstant.RegEdit_Key_SoftWare);
                RegistryKey rkAmdape2e = rkSoftWare.OpenSubKey(clsConstant.RegEdit_Key_AMDAPE2E);
                if (rkAmdape2e != null)
                {
                    this.txtSAPUserId.Text = clsCommHelp.encryptString(clsCommHelp.NullToString(rkAmdape2e.GetValue(clsConstant.RegEdit_Key_User)));
                    this.txtSAPPassword.Text = clsCommHelp.encryptString(clsCommHelp.NullToString(rkAmdape2e.GetValue(clsConstant.RegEdit_Key_PassWord)));
                    if (clsCommHelp.NullToString(rkAmdape2e.GetValue(clsConstant.RegEdit_Key_Date)) != "")
                    {
                        this.chkSaveInfo.Checked = true;
                    }
                    else
                    {
                        this.chkSaveInfo.Checked = false;
                    }
                    rkAmdape2e.Close();
                }
                rkSoftWare.Close();
                rkLocalMachine.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }

        private void tsbLogin_Click(object sender, EventArgs e)
        {

        }

        private void aboutUnlieverChinaSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void 追踪分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmTracing_Analysis == null)
            {
                frmTracing_Analysis = new frmTracing_Analysis();
                frmTracing_Analysis.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmTracing_Analysis == null)
            {
                frmTracing_Analysis = new frmTracing_Analysis();
            }
            frmTracing_Analysis.Show(this.dockPanel2);
        }

        private void 导入彩票数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (frmImport_Data == null)
            {
                frmImport_Data = new frmImport_Data();
                frmImport_Data.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmImport_Data == null)
            {
                frmImport_Data = new frmImport_Data();
            }
            frmImport_Data.Show();
        }

        private void 自动安装数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                #region 创建文件夹和 log 记事本

                ProcessLogger.Fatal("1001--Create Folder txt" + DateTime.Now.ToString());
                string spath = @"C:\Program Files\mongodb\bin";

                if (Directory.Exists(spath))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                    directoryInfo.Create();
                }


                spath = @"C:\Program Files\mongodb\data\db";

                if (Directory.Exists(spath))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                    directoryInfo.Create();
                }

                spath = @"C:\Program Files\mongodb\data\log";

                if (Directory.Exists(spath))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                    directoryInfo.Create();
                }
                spath = @"C:\Program Files\mongodb\data\log\MongoDB.log";

                if (File.Exists(spath))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(spath);

                    System.IO.File.Create(spath);
                }

                #endregion
                #region 复制文件BIN 到指定目录
                ProcessLogger.Fatal("1002--copy bin" + DateTime.Now.ToString());
                string srcdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System\\bin");
                string todir = @"C:\Program Files\mongodb\";
                string dstdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System\\bin");
                bool overwrite = true;
                CopyDirIntoDestDirectory(srcdir, todir, overwrite);


                #endregion

                #region 调用CMD 命令
                ProcessLogger.Fatal("1003--install db Start" + DateTime.Now.ToString());
                string cmd = @"C:&cd C:\Program Files\mongodb\bin&&mongod --dbpath ""C:\Program Files\mongodb\data\db""";
                string output = "";
                //cmd = @"ipconfig/all";
                RunCmd(cmd, out output);
                //  MessageBox.Show(output);

                ProcessLogger.Fatal("1004--install servers" + DateTime.Now.ToString());
                timerAlter1 = new System.Timers.Timer(200000);
                timerAlter1.Elapsed += new System.Timers.ElapsedEventHandler(TimeControl1);
                timerAlter1.AutoReset = true;
                timerAlter1.Start();
                cmd = @"C:&cd C:\Program Files\mongodb\bin&&mongod --dbpath ""C:\Program Files\mongodb\data\db"" --logpath ""C:\Program Files\mongodb\data\log\MongoDB.log"" --install --serviceName ""MongoDB""";
                RunCmd(cmd, out output);
                #endregion

                //MessageBox.Show("运行结束 ，后台数据配置成功 ", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("AccessException"))
                {
                    string dstdir = "";
                    Version ver = System.Environment.OSVersion.Version;
                    if (ver.Major.ToString().Contains("10"))
                    {
                        dstdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System\\win10Admin.reg");
                    }
                    else if (ver.Major.ToString().Contains("6"))
                    {
                        dstdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System\\win7Admin.reg");
                    }
                    Process.Start(dstdir);

                }
                MessageBox.Show("由于您未获得管理员权限，请尝试取得管理员权限\r\n（系统(仅支持Window10，win7版本)已自动尝试获取权限，如重试启动系统还未正常运行则请手动获取windows 的权限） ！" + ex, "安装错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

                throw;
            }
        }
        public static void CopyDirIntoDestDirectory(string srcdir, string dstdir, bool overwrite)
        {
            string todir = Path.Combine(dstdir,
                                        Path.GetFileName(srcdir)
                                        );

            if (!Directory.Exists(todir))
                Directory.CreateDirectory(todir);

            foreach (var s in Directory.GetFiles(srcdir))
            {
                string news = s.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System\\bin"), todir);
                if (File.Exists(news))
                {
                }
                else
                {
                    File.Copy(s, Path.Combine(todir, Path.GetFileName(s)), overwrite);
                }
            }
            foreach (var s in Directory.GetDirectories(srcdir))
                CopyDirIntoDestDirectory(s, todir, overwrite);
        }
        
        public static void RunCmd(string cmd, out string output)
        {
            try
            {
                string CmdPath = @"C:\Windows\System32\cmd.exe";
                cmd = cmd.Trim().TrimEnd('&') + "&exit";//说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = CmdPath;
                    p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                    p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                    p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                    p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                    p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                    p.Start();//启动程序

                    //向cmd窗口写入命令
                    p.StandardInput.WriteLine(cmd);
                    p.StandardInput.AutoFlush = true;

                    //获取cmd窗口的输出信息
                    output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();//等待程序执行完退出进程
                    p.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("EX:数据库配置失败 ：" + ex);


                throw;
            }
        }

        private void invoiceProcessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmDataCenter == null)
            {
                frmDataCenter = new frmDataCenter();
                frmDataCenter.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmDataCenter == null)
            {
                frmDataCenter = new frmDataCenter();
            }
            frmDataCenter.Show(this.dockPanel2);
        }
        private void TimeControl1(object sender, EventArgs e)
        {
            if (!IsRun1)
            {
                IsRun1 = true;
                GetDataforRawDataThread = new Thread(TimeMethod1);
                GetDataforRawDataThread.Start();
            }
        }
        private void TimeMethod1()
        {
            string output = "";
            string cmd = @"C:&cd C:\Program Files\mongodb\bin&&mongod --dbpath ""C:\Program Files\mongodb\data\db"" --logpath ""C:\Program Files\mongodb\data\log\MongoDB.log"" --install --serviceName ""MongoDB""";
            RunCmd(cmd, out output);

            alterisrun = 0;
            IsRun1 = false;
            MessageBox.Show("运行结束 ，后台数据配置成功 ,系统即将关闭，请自行重启即可", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {


            if (frmMCdata == null)
            {
                frmMCdata = new frmMCdata();
                frmMCdata.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmMCdata == null)
            {
                frmMCdata = new frmMCdata();
            }
            frmMCdata.Show(this.dockPanel2);



        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (frmImport_MCleixing_Data == null)
            {
                frmImport_MCleixing_Data = new frmImport_MCleixing_Data();
                frmImport_MCleixing_Data.FormClosed += new FormClosedEventHandler(FrmOMS_FormClosed);
            }
            if (frmImport_MCleixing_Data == null)
            {
                frmImport_MCleixing_Data = new frmImport_MCleixing_Data();
            }
            frmImport_MCleixing_Data.Show();
        }

        private void 导入彩票数据xlsxToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }
        private void NewMethod1()
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
                //newlist.Add(10);
                newlist = newlist.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a).ToList();

                int duan = 3;
                int evertduan = 10 / duan;
                int ilast = 0;
                ilast = duan * evertduan;


                string first = "";
                showSuijiResultlist = new List<string>();
                for (int iq = 0; iq < duan; iq++)
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
                item.Name = "默认方案";//保存名称
                item.DuanShu = showSuijiResultlist.Count.ToString();
                item.Mobanleibie = "默认";

                Result.Add(item);
                clsAllnew BusinessHelp = new clsAllnew();
                BusinessHelp.Save_FangAn(Result);

            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return;

                throw;
            }
        }

        private void 一键配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "Resources\\彩票类型.txt";
                clsAllnew BusinessHelp = new clsAllnew();
                List<CaipiaoZhongLeiDATA> Result = BusinessHelp.Readcaipiaoleixing_TXT(path);
                if (Result.Count != 0)
                    BusinessHelp.Save_CaiPiaoZhongLei(Result);

                path = AppDomain.CurrentDomain.BaseDirectory + "Resources\\号码单.txt";
                List<inputCaipiaoDATA> Result2 = BusinessHelp.ReadcaipiaoFile_TXT(path);
                if (Result2.Count != 0)
                    BusinessHelp.SPInputclaimreport_Server(Result2);

                NewMethod1();

                MessageBox.Show("导入成功,可以使用了！");

            }
            catch (Exception ex)
            {
                MessageBox.Show("导入数据错误，请确认本地文件包解压是否正常" + ex, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

                throw;
            }
        }

        private void 打开本地ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ZFCEPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"), "");
            System.Diagnostics.Process.Start("explorer.exe", ZFCEPath);
        }


    }
}
