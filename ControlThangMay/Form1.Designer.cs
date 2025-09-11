namespace ControlThangMay
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxConnection = new GroupBox();
            btnConnect = new Button();
            btnRefreshPorts = new Button();
            cmbBaudRate = new ComboBox();
            cmbComPort = new ComboBox();
            lblBaudRate = new Label();
            lblComPort = new Label();
            tabControlMain = new TabControl();
            tabPageControl = new TabPage();
            groupBoxElevator1 = new GroupBox();
            btnE1Restart = new Button();
            btnE1CloseDoor = new Button();
            btnE1OpenDoor = new Button();
            btnE1Stop = new Button();
            btnE1Floor5 = new Button();
            btnE1Floor4 = new Button();
            btnE1Floor3 = new Button();
            btnE1Floor2 = new Button();
            btnE1Floor1 = new Button();
            btnE1Floor0 = new Button();
            lblElevator1Position = new Label();
            lblElevator1Status = new Label();
            groupBoxElevator2 = new GroupBox();
            btnE2Restart = new Button();
            btnE2CloseDoor = new Button();
            btnE2OpenDoor = new Button();
            btnE2Stop = new Button();
            btnE2Floor5 = new Button();
            btnE2Floor4 = new Button();
            btnE2Floor3 = new Button();
            btnE2Floor2 = new Button();
            btnE2Floor1 = new Button();
            btnE2Floor0 = new Button();
            lblElevator2Position = new Label();
            lblElevator2Status = new Label();
            groupBoxFloorCalls = new GroupBox();
            btnCallFloor5Down = new Button();
            btnCallFloor4Up = new Button();
            btnCallFloor4Down = new Button();
            btnCallFloor3Up = new Button();
            btnCallFloor3Down = new Button();
            btnCallFloor2Up = new Button();
            btnCallFloor2Down = new Button();
            btnCallFloor1Up = new Button();
            btnCallFloor1Down = new Button();
            btnCallFloor0Up = new Button();
            lblFloorCallsTitle = new Label();
            groupBoxEmergency = new GroupBox();
            btnMaintenanceMode = new Button();
            btnFireAlarm = new Button();
            btnEmergencyStop = new Button();
            tabPageLogs = new TabPage();
            groupBoxLog = new GroupBox();
            lblSelectDate = new Label();
            rtbLog = new RichTextBox();
            lblLogTitle = new Label();
            dtpFilterDate = new DateTimePicker();
            btnFilterEmergency = new Button();
            btnFilterElevator = new Button();
            btnFilterByDate = new Button();
            btnFilterSystem = new Button();
            btnClearLog = new Button();
            btnResetFilter = new Button();
            lblConnectionStatus = new Label();
            groupBoxConnection.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageControl.SuspendLayout();
            groupBoxElevator1.SuspendLayout();
            groupBoxElevator2.SuspendLayout();
            groupBoxFloorCalls.SuspendLayout();
            groupBoxEmergency.SuspendLayout();
            tabPageLogs.SuspendLayout();
            groupBoxLog.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxConnection
            // 
            groupBoxConnection.Controls.Add(btnConnect);
            groupBoxConnection.Controls.Add(btnRefreshPorts);
            groupBoxConnection.Controls.Add(cmbBaudRate);
            groupBoxConnection.Controls.Add(cmbComPort);
            groupBoxConnection.Controls.Add(lblBaudRate);
            groupBoxConnection.Controls.Add(lblComPort);
            groupBoxConnection.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxConnection.Location = new Point(20, 20);
            groupBoxConnection.Name = "groupBoxConnection";
            groupBoxConnection.Size = new Size(688, 80);
            groupBoxConnection.TabIndex = 0;
            groupBoxConnection.TabStop = false;
            groupBoxConnection.Text = "KẾT NỐI HỆ THỐNG";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.LightBlue;
            btnConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConnect.Location = new Point(552, 26);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(100, 30);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "KẾT NỐI";
            btnConnect.UseVisualStyleBackColor = false;
            // 
            // btnRefreshPorts
            // 
            btnRefreshPorts.BackColor = Color.LightYellow;
            btnRefreshPorts.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRefreshPorts.Location = new Point(423, 30);
            btnRefreshPorts.Name = "btnRefreshPorts";
            btnRefreshPorts.Size = new Size(70, 23);
            btnRefreshPorts.TabIndex = 4;
            btnRefreshPorts.Text = "Làm mới";
            btnRefreshPorts.UseVisualStyleBackColor = false;
            // 
            // cmbBaudRate
            // 
            cmbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBaudRate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cmbBaudRate.FormattingEnabled = true;
            cmbBaudRate.Location = new Point(280, 30);
            cmbBaudRate.Name = "cmbBaudRate";
            cmbBaudRate.Size = new Size(100, 23);
            cmbBaudRate.TabIndex = 3;
            // 
            // cmbComPort
            // 
            cmbComPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbComPort.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cmbComPort.FormattingEnabled = true;
            cmbComPort.Location = new Point(80, 30);
            cmbComPort.Name = "cmbComPort";
            cmbComPort.Size = new Size(100, 23);
            cmbComPort.TabIndex = 2;
            // 
            // lblBaudRate
            // 
            lblBaudRate.AutoSize = true;
            lblBaudRate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBaudRate.Location = new Point(210, 33);
            lblBaudRate.Name = "lblBaudRate";
            lblBaudRate.Size = new Size(63, 15);
            lblBaudRate.TabIndex = 1;
            lblBaudRate.Text = "Baud Rate:";
            // 
            // lblComPort
            // 
            lblComPort.AutoSize = true;
            lblComPort.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblComPort.Location = new Point(20, 33);
            lblComPort.Name = "lblComPort";
            lblComPort.Size = new Size(63, 15);
            lblComPort.TabIndex = 0;
            lblComPort.Text = "COM Port:";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageControl);
            tabControlMain.Controls.Add(tabPageLogs);
            tabControlMain.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControlMain.Location = new Point(20, 110);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(1060, 530);
            tabControlMain.TabIndex = 1;
            // 
            // tabPageControl
            // 
            tabPageControl.BackColor = SystemColors.Control;
            tabPageControl.Controls.Add(groupBoxElevator1);
            tabPageControl.Controls.Add(groupBoxElevator2);
            tabPageControl.Controls.Add(groupBoxFloorCalls);
            tabPageControl.Controls.Add(groupBoxEmergency);
            tabPageControl.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabPageControl.Location = new Point(4, 24);
            tabPageControl.Name = "tabPageControl";
            tabPageControl.Padding = new Padding(3);
            tabPageControl.Size = new Size(1052, 502);
            tabPageControl.TabIndex = 0;
            tabPageControl.Text = "[>] ĐIỀU KHIỂN THANG MÁY";
            // 
            // groupBoxElevator1
            // 
            groupBoxElevator1.Controls.Add(btnE1Restart);
            groupBoxElevator1.Controls.Add(btnE1CloseDoor);
            groupBoxElevator1.Controls.Add(btnE1OpenDoor);
            groupBoxElevator1.Controls.Add(btnE1Stop);
            groupBoxElevator1.Controls.Add(btnE1Floor5);
            groupBoxElevator1.Controls.Add(btnE1Floor4);
            groupBoxElevator1.Controls.Add(btnE1Floor3);
            groupBoxElevator1.Controls.Add(btnE1Floor2);
            groupBoxElevator1.Controls.Add(btnE1Floor1);
            groupBoxElevator1.Controls.Add(btnE1Floor0);
            groupBoxElevator1.Controls.Add(lblElevator1Position);
            groupBoxElevator1.Controls.Add(lblElevator1Status);
            groupBoxElevator1.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxElevator1.Location = new Point(20, 20);
            groupBoxElevator1.Name = "groupBoxElevator1";
            groupBoxElevator1.Size = new Size(280, 420);
            groupBoxElevator1.TabIndex = 0;
            groupBoxElevator1.TabStop = false;
            groupBoxElevator1.Text = "THANG MÁY 1";
            // 
            // btnE1Restart
            // 
            btnE1Restart.BackColor = Color.LightBlue;
            btnE1Restart.Enabled = false;
            btnE1Restart.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Restart.Location = new Point(163, 375);
            btnE1Restart.Name = "btnE1Restart";
            btnE1Restart.Size = new Size(100, 30);
            btnE1Restart.TabIndex = 11;
            btnE1Restart.Text = ">> RESTART";
            btnE1Restart.UseVisualStyleBackColor = false;
            // 
            // btnE1CloseDoor
            // 
            btnE1CloseDoor.BackColor = Color.LightCoral;
            btnE1CloseDoor.Enabled = false;
            btnE1CloseDoor.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1CloseDoor.Location = new Point(15, 375);
            btnE1CloseDoor.Name = "btnE1CloseDoor";
            btnE1CloseDoor.Size = new Size(100, 30);
            btnE1CloseDoor.TabIndex = 10;
            btnE1CloseDoor.Text = "ĐÓNG CỬA";
            btnE1CloseDoor.UseVisualStyleBackColor = false;
            // 
            // btnE1OpenDoor
            // 
            btnE1OpenDoor.BackColor = Color.LightGreen;
            btnE1OpenDoor.Enabled = false;
            btnE1OpenDoor.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1OpenDoor.Location = new Point(163, 315);
            btnE1OpenDoor.Name = "btnE1OpenDoor";
            btnE1OpenDoor.Size = new Size(100, 30);
            btnE1OpenDoor.TabIndex = 9;
            btnE1OpenDoor.Text = "MỞ CỬA";
            btnE1OpenDoor.UseVisualStyleBackColor = false;
            // 
            // btnE1Stop
            // 
            btnE1Stop.BackColor = Color.Red;
            btnE1Stop.Enabled = false;
            btnE1Stop.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Stop.Location = new Point(15, 315);
            btnE1Stop.Name = "btnE1Stop";
            btnE1Stop.Size = new Size(100, 30);
            btnE1Stop.TabIndex = 8;
            btnE1Stop.Text = "DỪNG";
            btnE1Stop.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor5
            // 
            btnE1Floor5.BackColor = Color.LightSteelBlue;
            btnE1Floor5.Enabled = false;
            btnE1Floor5.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor5.Location = new Point(163, 85);
            btnE1Floor5.Name = "btnE1Floor5";
            btnE1Floor5.Size = new Size(100, 30);
            btnE1Floor5.TabIndex = 6;
            btnE1Floor5.Text = "TẦNG 5";
            btnE1Floor5.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor4
            // 
            btnE1Floor4.BackColor = Color.LightSteelBlue;
            btnE1Floor4.Enabled = false;
            btnE1Floor4.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor4.Location = new Point(15, 85);
            btnE1Floor4.Name = "btnE1Floor4";
            btnE1Floor4.Size = new Size(100, 30);
            btnE1Floor4.TabIndex = 5;
            btnE1Floor4.Text = "TẦNG 4";
            btnE1Floor4.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor3
            // 
            btnE1Floor3.BackColor = Color.LightSteelBlue;
            btnE1Floor3.Enabled = false;
            btnE1Floor3.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor3.Location = new Point(163, 144);
            btnE1Floor3.Name = "btnE1Floor3";
            btnE1Floor3.Size = new Size(100, 30);
            btnE1Floor3.TabIndex = 4;
            btnE1Floor3.Text = "TẦNG 3";
            btnE1Floor3.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor2
            // 
            btnE1Floor2.BackColor = Color.LightSteelBlue;
            btnE1Floor2.Enabled = false;
            btnE1Floor2.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor2.Location = new Point(15, 144);
            btnE1Floor2.Name = "btnE1Floor2";
            btnE1Floor2.Size = new Size(100, 30);
            btnE1Floor2.TabIndex = 3;
            btnE1Floor2.Text = "TẦNG 2";
            btnE1Floor2.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor1
            // 
            btnE1Floor1.BackColor = Color.LightSteelBlue;
            btnE1Floor1.Enabled = false;
            btnE1Floor1.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor1.Location = new Point(89, 205);
            btnE1Floor1.Name = "btnE1Floor1";
            btnE1Floor1.Size = new Size(100, 30);
            btnE1Floor1.TabIndex = 2;
            btnE1Floor1.Text = "TẦNG 1";
            btnE1Floor1.UseVisualStyleBackColor = false;
            // 
            // btnE1Floor0
            // 
            btnE1Floor0.BackColor = Color.LightSteelBlue;
            btnE1Floor0.Enabled = false;
            btnE1Floor0.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE1Floor0.Location = new Point(89, 262);
            btnE1Floor0.Name = "btnE1Floor0";
            btnE1Floor0.Size = new Size(100, 30);
            btnE1Floor0.TabIndex = 1;
            btnE1Floor0.Text = "TẦNG TRỆT";
            btnE1Floor0.UseVisualStyleBackColor = false;
            // 
            // lblElevator1Position
            // 
            lblElevator1Position.AutoSize = true;
            lblElevator1Position.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblElevator1Position.ForeColor = Color.Green;
            lblElevator1Position.Location = new Point(153, 37);
            lblElevator1Position.Name = "lblElevator1Position";
            lblElevator1Position.Size = new Size(110, 15);
            lblElevator1Position.TabIndex = 1;
            lblElevator1Position.Text = "Vị trí: Tầng trệt (0)";
            // 
            // lblElevator1Status
            // 
            lblElevator1Status.AutoSize = true;
            lblElevator1Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblElevator1Status.ForeColor = Color.Blue;
            lblElevator1Status.Location = new Point(15, 37);
            lblElevator1Status.Name = "lblElevator1Status";
            lblElevator1Status.Size = new Size(90, 15);
            lblElevator1Status.TabIndex = 0;
            lblElevator1Status.Text = "Trạng thái: Chờ";
            // 
            // groupBoxElevator2
            // 
            groupBoxElevator2.Controls.Add(btnE2Restart);
            groupBoxElevator2.Controls.Add(btnE2CloseDoor);
            groupBoxElevator2.Controls.Add(btnE2OpenDoor);
            groupBoxElevator2.Controls.Add(btnE2Stop);
            groupBoxElevator2.Controls.Add(btnE2Floor5);
            groupBoxElevator2.Controls.Add(btnE2Floor4);
            groupBoxElevator2.Controls.Add(btnE2Floor3);
            groupBoxElevator2.Controls.Add(btnE2Floor2);
            groupBoxElevator2.Controls.Add(btnE2Floor1);
            groupBoxElevator2.Controls.Add(btnE2Floor0);
            groupBoxElevator2.Controls.Add(lblElevator2Position);
            groupBoxElevator2.Controls.Add(lblElevator2Status);
            groupBoxElevator2.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxElevator2.Location = new Point(320, 20);
            groupBoxElevator2.Name = "groupBoxElevator2";
            groupBoxElevator2.Size = new Size(278, 420);
            groupBoxElevator2.TabIndex = 1;
            groupBoxElevator2.TabStop = false;
            groupBoxElevator2.Text = "THANG MÁY 2";
            // 
            // btnE2Restart
            // 
            btnE2Restart.BackColor = Color.LightBlue;
            btnE2Restart.Enabled = false;
            btnE2Restart.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Restart.Location = new Point(161, 375);
            btnE2Restart.Name = "btnE2Restart";
            btnE2Restart.Size = new Size(100, 30);
            btnE2Restart.TabIndex = 11;
            btnE2Restart.Text = ">> RESTART";
            btnE2Restart.UseVisualStyleBackColor = false;
            // 
            // btnE2CloseDoor
            // 
            btnE2CloseDoor.BackColor = Color.LightCoral;
            btnE2CloseDoor.Enabled = false;
            btnE2CloseDoor.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2CloseDoor.Location = new Point(15, 375);
            btnE2CloseDoor.Name = "btnE2CloseDoor";
            btnE2CloseDoor.Size = new Size(100, 30);
            btnE2CloseDoor.TabIndex = 10;
            btnE2CloseDoor.Text = "ĐÓNG CỬA";
            btnE2CloseDoor.UseVisualStyleBackColor = false;
            // 
            // btnE2OpenDoor
            // 
            btnE2OpenDoor.BackColor = Color.LightGreen;
            btnE2OpenDoor.Enabled = false;
            btnE2OpenDoor.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2OpenDoor.Location = new Point(161, 315);
            btnE2OpenDoor.Name = "btnE2OpenDoor";
            btnE2OpenDoor.Size = new Size(100, 30);
            btnE2OpenDoor.TabIndex = 9;
            btnE2OpenDoor.Text = "MỞ CỬA";
            btnE2OpenDoor.UseVisualStyleBackColor = false;
            // 
            // btnE2Stop
            // 
            btnE2Stop.BackColor = Color.Red;
            btnE2Stop.Enabled = false;
            btnE2Stop.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Stop.Location = new Point(15, 315);
            btnE2Stop.Name = "btnE2Stop";
            btnE2Stop.Size = new Size(100, 30);
            btnE2Stop.TabIndex = 8;
            btnE2Stop.Text = "DỪNG";
            btnE2Stop.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor5
            // 
            btnE2Floor5.BackColor = Color.LightSteelBlue;
            btnE2Floor5.Enabled = false;
            btnE2Floor5.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor5.Location = new Point(161, 86);
            btnE2Floor5.Name = "btnE2Floor5";
            btnE2Floor5.Size = new Size(100, 30);
            btnE2Floor5.TabIndex = 6;
            btnE2Floor5.Text = "TẦNG 5";
            btnE2Floor5.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor4
            // 
            btnE2Floor4.BackColor = Color.LightSteelBlue;
            btnE2Floor4.Enabled = false;
            btnE2Floor4.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor4.Location = new Point(15, 86);
            btnE2Floor4.Name = "btnE2Floor4";
            btnE2Floor4.Size = new Size(100, 30);
            btnE2Floor4.TabIndex = 5;
            btnE2Floor4.Text = "TẦNG 4";
            btnE2Floor4.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor3
            // 
            btnE2Floor3.BackColor = Color.LightSteelBlue;
            btnE2Floor3.Enabled = false;
            btnE2Floor3.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor3.Location = new Point(161, 144);
            btnE2Floor3.Name = "btnE2Floor3";
            btnE2Floor3.Size = new Size(100, 30);
            btnE2Floor3.TabIndex = 4;
            btnE2Floor3.Text = "TẦNG 3";
            btnE2Floor3.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor2
            // 
            btnE2Floor2.BackColor = Color.LightSteelBlue;
            btnE2Floor2.Enabled = false;
            btnE2Floor2.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor2.Location = new Point(15, 144);
            btnE2Floor2.Name = "btnE2Floor2";
            btnE2Floor2.Size = new Size(100, 30);
            btnE2Floor2.TabIndex = 3;
            btnE2Floor2.Text = "TẦNG 2";
            btnE2Floor2.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor1
            // 
            btnE2Floor1.BackColor = Color.LightSteelBlue;
            btnE2Floor1.Enabled = false;
            btnE2Floor1.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor1.Location = new Point(85, 205);
            btnE2Floor1.Name = "btnE2Floor1";
            btnE2Floor1.Size = new Size(100, 30);
            btnE2Floor1.TabIndex = 2;
            btnE2Floor1.Text = "TẦNG 1";
            btnE2Floor1.UseVisualStyleBackColor = false;
            // 
            // btnE2Floor0
            // 
            btnE2Floor0.BackColor = Color.LightSteelBlue;
            btnE2Floor0.Enabled = false;
            btnE2Floor0.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnE2Floor0.Location = new Point(85, 262);
            btnE2Floor0.Name = "btnE2Floor0";
            btnE2Floor0.Size = new Size(100, 30);
            btnE2Floor0.TabIndex = 1;
            btnE2Floor0.Text = "TẦNG TRẸT";
            btnE2Floor0.UseVisualStyleBackColor = false;
            // 
            // lblElevator2Position
            // 
            lblElevator2Position.AutoSize = true;
            lblElevator2Position.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblElevator2Position.ForeColor = Color.Green;
            lblElevator2Position.Location = new Point(151, 37);
            lblElevator2Position.Name = "lblElevator2Position";
            lblElevator2Position.Size = new Size(110, 15);
            lblElevator2Position.TabIndex = 1;
            lblElevator2Position.Text = "Vị trí: Tầng trệt (0)";
            lblElevator2Position.Click += lblElevator2Position_Click;
            // 
            // lblElevator2Status
            // 
            lblElevator2Status.AutoSize = true;
            lblElevator2Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblElevator2Status.ForeColor = Color.Blue;
            lblElevator2Status.Location = new Point(15, 37);
            lblElevator2Status.Name = "lblElevator2Status";
            lblElevator2Status.Size = new Size(90, 15);
            lblElevator2Status.TabIndex = 0;
            lblElevator2Status.Text = "Trạng thái: Chờ";
            lblElevator2Status.Click += lblElevator2Status_Click;
            // 
            // groupBoxFloorCalls
            // 
            groupBoxFloorCalls.Controls.Add(btnCallFloor5Down);
            groupBoxFloorCalls.Controls.Add(btnCallFloor4Up);
            groupBoxFloorCalls.Controls.Add(btnCallFloor4Down);
            groupBoxFloorCalls.Controls.Add(btnCallFloor3Up);
            groupBoxFloorCalls.Controls.Add(btnCallFloor3Down);
            groupBoxFloorCalls.Controls.Add(btnCallFloor2Up);
            groupBoxFloorCalls.Controls.Add(btnCallFloor2Down);
            groupBoxFloorCalls.Controls.Add(btnCallFloor1Up);
            groupBoxFloorCalls.Controls.Add(btnCallFloor1Down);
            groupBoxFloorCalls.Controls.Add(btnCallFloor0Up);
            groupBoxFloorCalls.Controls.Add(lblFloorCallsTitle);
            groupBoxFloorCalls.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxFloorCalls.Location = new Point(618, 20);
            groupBoxFloorCalls.Name = "groupBoxFloorCalls";
            groupBoxFloorCalls.Size = new Size(220, 270);
            groupBoxFloorCalls.TabIndex = 2;
            groupBoxFloorCalls.TabStop = false;
            groupBoxFloorCalls.Text = "GỌI THANG MÁY";
            // 
            // btnCallFloor5Down
            // 
            btnCallFloor5Down.BackColor = Color.LightCoral;
            btnCallFloor5Down.Enabled = false;
            btnCallFloor5Down.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor5Down.Location = new Point(12, 55);
            btnCallFloor5Down.Name = "btnCallFloor5Down";
            btnCallFloor5Down.Size = new Size(197, 25);
            btnCallFloor5Down.TabIndex = 10;
            btnCallFloor5Down.Text = "TẦNG 5 ↓ XUỐNG";
            btnCallFloor5Down.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor4Up
            // 
            btnCallFloor4Up.BackColor = Color.LightGreen;
            btnCallFloor4Up.Enabled = false;
            btnCallFloor4Up.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor4Up.Location = new Point(12, 90);
            btnCallFloor4Up.Name = "btnCallFloor4Up";
            btnCallFloor4Up.Size = new Size(93, 25);
            btnCallFloor4Up.TabIndex = 8;
            btnCallFloor4Up.Text = "TẦNG 4 ↑ LÊN";
            btnCallFloor4Up.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor4Down
            // 
            btnCallFloor4Down.BackColor = Color.LightCoral;
            btnCallFloor4Down.Enabled = false;
            btnCallFloor4Down.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor4Down.Location = new Point(111, 90);
            btnCallFloor4Down.Name = "btnCallFloor4Down";
            btnCallFloor4Down.Size = new Size(98, 25);
            btnCallFloor4Down.TabIndex = 9;
            btnCallFloor4Down.Text = "TẦNG 4 ↓ XUỐNG";
            btnCallFloor4Down.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor3Up
            // 
            btnCallFloor3Up.BackColor = Color.LightGreen;
            btnCallFloor3Up.Enabled = false;
            btnCallFloor3Up.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor3Up.Location = new Point(12, 125);
            btnCallFloor3Up.Name = "btnCallFloor3Up";
            btnCallFloor3Up.Size = new Size(93, 25);
            btnCallFloor3Up.TabIndex = 6;
            btnCallFloor3Up.Text = "TẦNG 3 ↑ LÊN";
            btnCallFloor3Up.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor3Down
            // 
            btnCallFloor3Down.BackColor = Color.LightCoral;
            btnCallFloor3Down.Enabled = false;
            btnCallFloor3Down.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor3Down.Location = new Point(111, 125);
            btnCallFloor3Down.Name = "btnCallFloor3Down";
            btnCallFloor3Down.Size = new Size(98, 25);
            btnCallFloor3Down.TabIndex = 7;
            btnCallFloor3Down.Text = "TẦNG 3 ↓ XUỐNG";
            btnCallFloor3Down.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor2Up
            // 
            btnCallFloor2Up.BackColor = Color.LightGreen;
            btnCallFloor2Up.Enabled = false;
            btnCallFloor2Up.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor2Up.Location = new Point(12, 160);
            btnCallFloor2Up.Name = "btnCallFloor2Up";
            btnCallFloor2Up.Size = new Size(93, 25);
            btnCallFloor2Up.TabIndex = 4;
            btnCallFloor2Up.Text = "TẦNG 2 ↑ LÊN";
            btnCallFloor2Up.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor2Down
            // 
            btnCallFloor2Down.BackColor = Color.LightCoral;
            btnCallFloor2Down.Enabled = false;
            btnCallFloor2Down.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor2Down.Location = new Point(111, 160);
            btnCallFloor2Down.Name = "btnCallFloor2Down";
            btnCallFloor2Down.Size = new Size(98, 25);
            btnCallFloor2Down.TabIndex = 5;
            btnCallFloor2Down.Text = "TẦNG 2 ↓ XUỐNG";
            btnCallFloor2Down.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor1Up
            // 
            btnCallFloor1Up.BackColor = Color.LightGreen;
            btnCallFloor1Up.Enabled = false;
            btnCallFloor1Up.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor1Up.Location = new Point(12, 195);
            btnCallFloor1Up.Name = "btnCallFloor1Up";
            btnCallFloor1Up.Size = new Size(93, 25);
            btnCallFloor1Up.TabIndex = 2;
            btnCallFloor1Up.Text = "TẦNG 1 ↑ LÊN";
            btnCallFloor1Up.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor1Down
            // 
            btnCallFloor1Down.BackColor = Color.LightCoral;
            btnCallFloor1Down.Enabled = false;
            btnCallFloor1Down.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor1Down.Location = new Point(111, 195);
            btnCallFloor1Down.Name = "btnCallFloor1Down";
            btnCallFloor1Down.Size = new Size(98, 25);
            btnCallFloor1Down.TabIndex = 3;
            btnCallFloor1Down.Text = "TẦNG 1 ↓ XUỐNG";
            btnCallFloor1Down.UseVisualStyleBackColor = false;
            // 
            // btnCallFloor0Up
            // 
            btnCallFloor0Up.BackColor = Color.LightGreen;
            btnCallFloor0Up.Enabled = false;
            btnCallFloor0Up.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCallFloor0Up.Location = new Point(12, 230);
            btnCallFloor0Up.Name = "btnCallFloor0Up";
            btnCallFloor0Up.Size = new Size(197, 25);
            btnCallFloor0Up.TabIndex = 1;
            btnCallFloor0Up.Text = "TẦNG TRỆT ↑ LÊN";
            btnCallFloor0Up.UseVisualStyleBackColor = false;
            // 
            // lblFloorCallsTitle
            // 
            lblFloorCallsTitle.AutoSize = true;
            lblFloorCallsTitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFloorCallsTitle.Location = new Point(15, 25);
            lblFloorCallsTitle.Name = "lblFloorCallsTitle";
            lblFloorCallsTitle.Size = new Size(194, 13);
            lblFloorCallsTitle.TabIndex = 0;
            lblFloorCallsTitle.Text = "Gọi thang máy với hướng di chuyển:";
            // 
            // groupBoxEmergency
            // 
            groupBoxEmergency.Controls.Add(btnMaintenanceMode);
            groupBoxEmergency.Controls.Add(btnFireAlarm);
            groupBoxEmergency.Controls.Add(btnEmergencyStop);
            groupBoxEmergency.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxEmergency.Location = new Point(618, 310);
            groupBoxEmergency.Name = "groupBoxEmergency";
            groupBoxEmergency.Size = new Size(220, 130);
            groupBoxEmergency.TabIndex = 3;
            groupBoxEmergency.TabStop = false;
            groupBoxEmergency.Text = "KHẨN CẤP & ĐIỀU KHIỂN";
            // 
            // btnMaintenanceMode
            // 
            btnMaintenanceMode.BackColor = Color.Yellow;
            btnMaintenanceMode.Enabled = false;
            btnMaintenanceMode.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMaintenanceMode.Location = new Point(15, 90);
            btnMaintenanceMode.Name = "btnMaintenanceMode";
            btnMaintenanceMode.Size = new Size(180, 25);
            btnMaintenanceMode.TabIndex = 2;
            btnMaintenanceMode.Text = "BẢO TRÌ";
            btnMaintenanceMode.UseVisualStyleBackColor = false;
            // 
            // btnFireAlarm
            // 
            btnFireAlarm.BackColor = Color.Orange;
            btnFireAlarm.Enabled = false;
            btnFireAlarm.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFireAlarm.Location = new Point(15, 60);
            btnFireAlarm.Name = "btnFireAlarm";
            btnFireAlarm.Size = new Size(180, 25);
            btnFireAlarm.TabIndex = 1;
            btnFireAlarm.Text = "BÁO CHÁY";
            btnFireAlarm.UseVisualStyleBackColor = false;
            // 
            // btnEmergencyStop
            // 
            btnEmergencyStop.BackColor = Color.Red;
            btnEmergencyStop.Enabled = false;
            btnEmergencyStop.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEmergencyStop.Location = new Point(15, 30);
            btnEmergencyStop.Name = "btnEmergencyStop";
            btnEmergencyStop.Size = new Size(180, 25);
            btnEmergencyStop.TabIndex = 0;
            btnEmergencyStop.Text = "DỪNG KHẨN CẤP";
            btnEmergencyStop.UseVisualStyleBackColor = false;
            // 
            // tabPageLogs
            // 
            tabPageLogs.BackColor = SystemColors.Control;
            tabPageLogs.Controls.Add(groupBoxLog);
            tabPageLogs.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabPageLogs.Location = new Point(4, 24);
            tabPageLogs.Name = "tabPageLogs";
            tabPageLogs.Padding = new Padding(3);
            tabPageLogs.Size = new Size(1052, 502);
            tabPageLogs.TabIndex = 1;
            tabPageLogs.Text = "[LOG] NHẬT KÝ HỆ THỐNG";
            // 
            // groupBoxLog
            // 
            groupBoxLog.Controls.Add(lblSelectDate);
            groupBoxLog.Controls.Add(rtbLog);
            groupBoxLog.Controls.Add(lblLogTitle);
            groupBoxLog.Controls.Add(dtpFilterDate);
            groupBoxLog.Controls.Add(btnFilterEmergency);
            groupBoxLog.Controls.Add(btnFilterElevator);
            groupBoxLog.Controls.Add(btnFilterByDate);
            groupBoxLog.Controls.Add(btnFilterSystem);
            groupBoxLog.Controls.Add(btnClearLog);
            groupBoxLog.Controls.Add(btnResetFilter);
            groupBoxLog.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxLog.Location = new Point(20, 20);
            groupBoxLog.Name = "groupBoxLog";
            groupBoxLog.Size = new Size(1012, 460);
            groupBoxLog.TabIndex = 0;
            groupBoxLog.TabStop = false;
            groupBoxLog.Text = "NHẬT KÝ HỆ THỐNG";
            // 
            // lblSelectDate
            // 
            lblSelectDate.AutoSize = true;
            lblSelectDate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSelectDate.Location = new Point(13, 30);
            lblSelectDate.Name = "lblSelectDate";
            lblSelectDate.Size = new Size(101, 15);
            lblSelectDate.TabIndex = 2;
            lblSelectDate.Text = "[CAL] Chọn ngày:";
            lblSelectDate.Click += lblSelectDate_Click;
            // 
            // rtbLog
            // 
            rtbLog.BackColor = Color.Black;
            rtbLog.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rtbLog.ForeColor = Color.LimeGreen;
            rtbLog.Location = new Point(13, 83);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbLog.Size = new Size(978, 356);
            rtbLog.TabIndex = 1;
            rtbLog.Text = "";
            // 
            // lblLogTitle
            // 
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLogTitle.Location = new Point(13, 65);
            lblLogTitle.Name = "lblLogTitle";
            lblLogTitle.Size = new Size(402, 15);
            lblLogTitle.TabIndex = 10;
            lblLogTitle.Text = "[STATS] Theo dõi hoạt động thang máy - Lọc theo loại log hoặc chọn ngày";
            // 
            // dtpFilterDate
            // 
            dtpFilterDate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dtpFilterDate.Format = DateTimePickerFormat.Short;
            dtpFilterDate.Location = new Point(120, 27);
            dtpFilterDate.Name = "dtpFilterDate";
            dtpFilterDate.Size = new Size(120, 23);
            dtpFilterDate.TabIndex = 3;
            dtpFilterDate.Value = new DateTime(2025, 9, 11, 0, 0, 0, 0);
            // 
            // btnFilterEmergency
            // 
            btnFilterEmergency.BackColor = Color.LightPink;
            btnFilterEmergency.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFilterEmergency.Location = new Point(619, 27);
            btnFilterEmergency.Name = "btnFilterEmergency";
            btnFilterEmergency.Size = new Size(120, 25);
            btnFilterEmergency.TabIndex = 8;
            btnFilterEmergency.Text = "[!] Khẩn cấp";
            btnFilterEmergency.UseVisualStyleBackColor = false;
            // 
            // btnFilterElevator
            // 
            btnFilterElevator.BackColor = Color.LightSkyBlue;
            btnFilterElevator.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFilterElevator.Location = new Point(493, 27);
            btnFilterElevator.Name = "btnFilterElevator";
            btnFilterElevator.Size = new Size(120, 25);
            btnFilterElevator.TabIndex = 7;
            btnFilterElevator.Text = "[E] Thang máy";
            btnFilterElevator.UseVisualStyleBackColor = false;
            // 
            // btnFilterByDate
            // 
            btnFilterByDate.BackColor = Color.LightCyan;
            btnFilterByDate.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFilterByDate.Location = new Point(261, 27);
            btnFilterByDate.Name = "btnFilterByDate";
            btnFilterByDate.Size = new Size(100, 25);
            btnFilterByDate.TabIndex = 4;
            btnFilterByDate.Text = "[FIND] Lọc ngày";
            btnFilterByDate.UseVisualStyleBackColor = false;
            // 
            // btnFilterSystem
            // 
            btnFilterSystem.BackColor = Color.LightYellow;
            btnFilterSystem.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnFilterSystem.Location = new Point(367, 28);
            btnFilterSystem.Name = "btnFilterSystem";
            btnFilterSystem.Size = new Size(120, 25);
            btnFilterSystem.TabIndex = 6;
            btnFilterSystem.Text = "[SYS] Hệ thống";
            btnFilterSystem.UseVisualStyleBackColor = false;
            // 
            // btnClearLog
            // 
            btnClearLog.BackColor = Color.LightCoral;
            btnClearLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClearLog.Location = new Point(871, 27);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(120, 35);
            btnClearLog.TabIndex = 5;
            btnClearLog.Text = "[DEL] XÓA LOG";
            btnClearLog.UseVisualStyleBackColor = false;
            // 
            // btnResetFilter
            // 
            btnResetFilter.BackColor = Color.LightGray;
            btnResetFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnResetFilter.Location = new Point(745, 27);
            btnResetFilter.Name = "btnResetFilter";
            btnResetFilter.Size = new Size(120, 25);
            btnResetFilter.TabIndex = 9;
            btnResetFilter.Text = "[ALL] Tất cả";
            btnResetFilter.UseVisualStyleBackColor = false;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblConnectionStatus.ForeColor = Color.Red;
            lblConnectionStatus.Location = new Point(746, 54);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(167, 19);
            lblConnectionStatus.TabIndex = 6;
            lblConnectionStatus.Text = "Trạng thái: Ngắt kết nối";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 653);
            Controls.Add(lblConnectionStatus);
            Controls.Add(tabControlMain);
            Controls.Add(groupBoxConnection);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hệ thống điều khiển Thang máy tòa nhà - 6 tầng, 2 thang máy";
            Load += Form1_Load;
            groupBoxConnection.ResumeLayout(false);
            groupBoxConnection.PerformLayout();
            tabControlMain.ResumeLayout(false);
            tabPageControl.ResumeLayout(false);
            groupBoxElevator1.ResumeLayout(false);
            groupBoxElevator1.PerformLayout();
            groupBoxElevator2.ResumeLayout(false);
            groupBoxElevator2.PerformLayout();
            groupBoxFloorCalls.ResumeLayout(false);
            groupBoxFloorCalls.PerformLayout();
            groupBoxEmergency.ResumeLayout(false);
            tabPageLogs.ResumeLayout(false);
            groupBoxLog.ResumeLayout(false);
            groupBoxLog.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxConnection;
        private Button btnConnect;
        private Button btnRefreshPorts;
        private ComboBox cmbBaudRate;
        private ComboBox cmbComPort;
        private Label lblBaudRate;
        private Label lblComPort;
        private TabControl tabControlMain;
        private TabPage tabPageControl;
        private TabPage tabPageLogs;
        private GroupBox groupBoxElevator1;
        private Button btnE1CloseDoor;
        private Button btnE1OpenDoor;
        private Button btnE1Stop;
        private Button btnE1Restart;
        private Button btnE1Floor5;
        private Button btnE1Floor4;
        private Button btnE1Floor3;
        private Button btnE1Floor2;
        private Button btnE1Floor1;
        private Button btnE1Floor0;
        private Label lblElevator1Position;
        private Label lblElevator1Status;
        private GroupBox groupBoxElevator2;
        private Button btnE2CloseDoor;
        private Button btnE2OpenDoor;
        private Button btnE2Stop;
        private Button btnE2Restart;
        private Button btnE2Floor5;
        private Button btnE2Floor4;
        private Button btnE2Floor3;
        private Button btnE2Floor2;
        private Button btnE2Floor1;
        private Button btnE2Floor0;
        private Label lblElevator2Position;
        private Label lblElevator2Status;
        private GroupBox groupBoxFloorCalls;
        private Button btnCallFloor5Down;
        private Button btnCallFloor4Up;
        private Button btnCallFloor4Down;
        private Button btnCallFloor3Up;
        private Button btnCallFloor3Down;
        private Button btnCallFloor2Up;
        private Button btnCallFloor2Down;
        private Button btnCallFloor1Up;
        private Button btnCallFloor1Down;
        private Button btnCallFloor0Up;
        private Label lblFloorCallsTitle;
        private GroupBox groupBoxEmergency;
        private Button btnMaintenanceMode;
        private Button btnFireAlarm;
        private Button btnEmergencyStop;
        private GroupBox groupBoxLog;
        private Button btnClearLog;
        private Button btnFilterSystem;
        private Button btnFilterElevator;
        private Button btnFilterEmergency;
        private Button btnResetFilter;
        private DateTimePicker dtpFilterDate;
        private Button btnFilterByDate;
        private Label lblSelectDate;
        private RichTextBox rtbLog;
        private Label lblLogTitle;
        private Label lblConnectionStatus;
    }
}