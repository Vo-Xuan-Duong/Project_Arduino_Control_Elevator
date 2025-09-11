using System.IO.Ports;
using System.Text;
using System.Text.Json;
using Npgsql;

namespace ControlThangMay
{
    public partial class Form1 : Form
    {
        private SerialPort _serialPort;
        private bool _isConnected = false;

        // Elevator status tracking
        private ElevatorInfo _elevator1;
        private ElevatorInfo _elevator2;

        // Building configuration
        private const int MAX_FLOORS = 6;
        private const int NUM_ELEVATORS = 2;

        // Log system ready flag
        private bool _logSystemReady = false;

        // Database integration
        private SimpleDatabase? _database;
        private bool _isDatabaseEnabled = false;

        // Log filtering
        private readonly List<LogItem> _allLogs = new();
        private string _currentFilter = "all";

        // Tab notification
        private int _newLogCount = 0;
        private bool _isLogTabActive = false;

        // ==== Realtime additions ====
        private readonly StringBuilder _rxBuf = new StringBuilder(4096);
        private volatile bool _awaitingReady = false;

        // Map nút (grp, idx) -> Button; lưu màu gốc & auto-off
        private readonly Dictionary<(char grp, int idx), Button> _btnMap = new();
        private readonly Dictionary<Button, Color> _btnOrig = new();
        private readonly Dictionary<(char grp, int idx), DateTime> _btnLastOn = new();
        private readonly System.Windows.Forms.Timer _btnAutoOffTimer = new() { Interval = 200 };

        public Form1()
        {
            InitializeComponent();
            InitializeElevators();
            InitializeCustomSettings();
            AssignEventHandlers();
        }

        // Add missing AssignEventHandlers stub to avoid build error (designer usually wires events)
        private void AssignEventHandlers()
        {
            btnConnect.Click += btnConnect_Click;
            btnRefreshPorts.Click += btnRefreshPorts_Click;
            this.FormClosing += Form1_FormClosing;

            // Log buttons
            btnClearLog.Click += BtnClearLog_Click;
            btnFilterSystem.Click += BtnFilterSystem_Click;
            btnFilterElevator.Click += BtnFilterElevator_Click;
            btnFilterEmergency.Click += BtnFilterEmergency_Click;
            btnResetFilter.Click += BtnResetFilter_Click;
            btnFilterByDate.Click += BtnFilterByDate_Click;
            dtpFilterDate.ValueChanged += DtpFilterDate_ValueChanged;

            // Elevator 1 floor buttons
            btnE1Floor0.Click += (s, e) => ElevatorGoToFloor(1, 0);
            btnE1Floor1.Click += (s, e) => ElevatorGoToFloor(1, 1);
            btnE1Floor2.Click += (s, e) => ElevatorGoToFloor(1, 2);
            btnE1Floor3.Click += (s, e) => ElevatorGoToFloor(1, 3);
            btnE1Floor4.Click += (s, e) => ElevatorGoToFloor(1, 4);
            btnE1Floor5.Click += (s, e) => ElevatorGoToFloor(1, 5);

            // Elevator 2 floor buttons  
            btnE2Floor0.Click += (s, e) => ElevatorGoToFloor(2, 0);
            btnE2Floor1.Click += (s, e) => ElevatorGoToFloor(2, 1);
            btnE2Floor2.Click += (s, e) => ElevatorGoToFloor(2, 2);
            btnE2Floor3.Click += (s, e) => ElevatorGoToFloor(2, 3);
            btnE2Floor4.Click += (s, e) => ElevatorGoToFloor(2, 4);
            btnE2Floor5.Click += (s, e) => ElevatorGoToFloor(2, 5);

            // Elevator control buttons
            btnE1Stop.Click += (s, e) => ElevatorStop(1);
            btnE2Stop.Click += (s, e) => ElevatorStop(2);
            btnE1OpenDoor.Click += (s, e) => ElevatorOpenDoor(1);
            btnE2OpenDoor.Click += (s, e) => ElevatorOpenDoor(2);
            btnE1CloseDoor.Click += (s, e) => ElevatorCloseDoor(1);
            btnE2CloseDoor.Click += (s, e) => ElevatorCloseDoor(2);
            btnE1Restart.Click += (s, e) => RestartElevator(1);
            btnE2Restart.Click += (s, e) => RestartElevator(2);

            // Floor call buttons with direction
            btnCallFloor0Up.Click += (s, e) => CallElevatorToFloorWithDirection(0, "UP");
            btnCallFloor1Up.Click += (s, e) => CallElevatorToFloorWithDirection(1, "UP");
            btnCallFloor1Down.Click += (s, e) => CallElevatorToFloorWithDirection(1, "DOWN");
            btnCallFloor2Up.Click += (s, e) => CallElevatorToFloorWithDirection(2, "UP");
            btnCallFloor2Down.Click += (s, e) => CallElevatorToFloorWithDirection(2, "DOWN");
            btnCallFloor3Up.Click += (s, e) => CallElevatorToFloorWithDirection(3, "UP");
            btnCallFloor3Down.Click += (s, e) => CallElevatorToFloorWithDirection(3, "DOWN");
            btnCallFloor4Up.Click += (s, e) => CallElevatorToFloorWithDirection(4, "UP");
            btnCallFloor4Down.Click += (s, e) => CallElevatorToFloorWithDirection(4, "DOWN");
            btnCallFloor5Down.Click += (s, e) => CallElevatorToFloorWithDirection(5, "DOWN");

            // Emergency buttons
            btnEmergencyStop.Click += (s, e) => EmergencyStop();
            btnFireAlarm.Click += (s, e) => FireAlarm();
            btnMaintenanceMode.Click += (s, e) => MaintenanceMode();
        }

        // Log item class for filtering
        private class LogItem
        {
            public DateTime Timestamp { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string FormattedText { get; set; } = string.Empty;
            public Color Color { get; set; }
        }

        private enum LogType
        {
            System,     // Hệ thống
            Elevator,   // Hoạt động thang máy
            User,       // Hành động người dùng
            Emergency,  // Khẩn cấp
            Error,      // Lỗi
            Warning     // Cảnh báo
        }

        private void InitializeElevators()
        {
            _elevator1 = new ElevatorInfo(1);
            _elevator2 = new ElevatorInfo(2);
        }

        private void InitializeCustomSettings()
        {
            // Initialize database
            InitializeDatabaseService();

            // Build UI button map & auto-off
            BuildButtonMap();
            SetupAutoOff();

            // Log system is now ready
            _logSystemReady = true;

            // Initialize tab features
            InitializeTabFeatures();

            // Add initial log entry
            AddLog("[SYS] He thong dieu khien da khoi dong", LogType.System);
        }

        private void BuildButtonMap()
        {
            // HALL 10 nút theo thứ tự index 0..9 (khớp mảng hallPins trên Arduino)
            _btnMap[('H', 0)] = btnCallFloor0Up;
            _btnMap[('H', 1)] = btnCallFloor1Down;
            _btnMap[('H', 2)] = btnCallFloor1Up;
            _btnMap[('H', 3)] = btnCallFloor2Down;
            _btnMap[('H', 4)] = btnCallFloor2Up;
            _btnMap[('H', 5)] = btnCallFloor3Down;
            _btnMap[('H', 6)] = btnCallFloor3Up;
            _btnMap[('H', 7)] = btnCallFloor4Down;
            _btnMap[('H', 8)] = btnCallFloor4Up;
            _btnMap[('H', 9)] = btnCallFloor5Down;

            // Cabin A → dùng cụm nút chọn tầng thang 1
            _btnMap[('A', 0)] = btnE1Floor0;
            _btnMap[('A', 1)] = btnE1Floor1;
            _btnMap[('A', 2)] = btnE1Floor2;
            _btnMap[('A', 3)] = btnE1Floor3;
            _btnMap[('A', 4)] = btnE1Floor4;
            _btnMap[('A', 5)] = btnE1Floor5;

            // Cabin B → dùng cụm nút chọn tầng thang 2
            _btnMap[('B', 0)] = btnE2Floor0;
            _btnMap[('B', 1)] = btnE2Floor1;
            _btnMap[('B', 2)] = btnE2Floor2;
            _btnMap[('B', 3)] = btnE2Floor3;
            _btnMap[('B', 4)] = btnE2Floor4;
            _btnMap[('B', 5)] = btnE2Floor5;

            // Door: C=Close, O=Open ; idx 0 -> E1, idx 1 -> E2
            _btnMap[('C', 0)] = btnE1CloseDoor;
            _btnMap[('C', 1)] = btnE2CloseDoor;
            _btnMap[('O', 0)] = btnE1OpenDoor;
            _btnMap[('O', 1)] = btnE2OpenDoor;

            foreach (var kv in _btnMap)
            {
                var b = kv.Value;
                if (b != null && !_btnOrig.ContainsKey(b)) _btnOrig[b] = b.BackColor;
            }
        }

        private void SetupAutoOff()
        {
            _btnAutoOffTimer.Tick += (s, e) =>
            {
                var now = DateTime.UtcNow;
                var keys = _btnLastOn.Keys.ToList();
                foreach (var k in keys)
                {
                    if ((now - _btnLastOn[k]).TotalMilliseconds > 800) // mất gói release → tự nhả
                    {
                        SetButtonPressed(k.grp, k.idx, false);
                        _btnLastOn.Remove(k);
                    }
                }
            };
            _btnAutoOffTimer.Start();
        }

        private void InitializeTabFeatures()
        {
            if (tabControlMain != null)
            {
                tabControlMain.SelectedIndexChanged += TabControlMain_SelectedIndexChanged;
            }
            _newLogCount = 0;
            UpdateLogTabTitle();
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedIndex == 1) // Log tab
            {
                _isLogTabActive = true;
                _newLogCount = 0;
                UpdateLogTabTitle();
                // Refresh from DB (or local) when switching to Logs tab
                FilterLogs();
                AddLog("[TAB] Dang xem nhat ky he thong", LogType.System);
            }
            else
            {
                _isLogTabActive = false;
                AddLog("[TAB] Chuyen sang dieu khien thang may", LogType.User);
            }
        }

        private void UpdateLogTabTitle()
        {
            if (tabPageLogs != null)
            {
                if (_newLogCount > 0 && !_isLogTabActive)
                {
                    tabPageLogs.Text = $"[LOG] NHAT KY HE THONG ({_newLogCount})";
                }
                else
                {
                    tabPageLogs.Text = "[LOG] NHAT KY HE THONG";
                }
            }
        }

        private void AddLog(string message, LogType type)
        {
            if (!_logSystemReady || rtbLog == null || rtbLog.IsDisposed)
            {
                Console.WriteLine("Log system not ready or rtbLog is null/disposed");
                return;
            }

            if (rtbLog.InvokeRequired)
            {
                try { rtbLog.Invoke(new Action(() => AddLog(message, type))); }
                catch (ObjectDisposedException) { }
                return;
            }

            try
            {
                // Show both date and time
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string icon = GetLogIcon(type);
                string logEntry = $"[{timestamp}] {icon} {message}\n";
                Color logColor = GetLogColor(type);

                var logItem = new LogItem
                {
                    Timestamp = DateTime.Now,
                    Type = type.ToString(),
                    Message = message,
                    FormattedText = logEntry,
                    Color = logColor
                };
                _allLogs.Add(logItem);
                if (_allLogs.Count > 500) _allLogs.RemoveAt(0);

                if (ShouldShowLog(logItem))
                {
                    rtbLog.SelectionStart = rtbLog.TextLength;
                    rtbLog.SelectionLength = 0;
                    rtbLog.SelectionColor = logColor;
                    rtbLog.AppendText(logEntry);
                    rtbLog.SelectionColor = rtbLog.ForeColor;
                    rtbLog.ScrollToCaret();
                }

                if (!_isLogTabActive)
                {
                    _newLogCount++;
                    UpdateLogTabTitle();
                }

                SaveLogToDatabase(message, type, icon);
            }
            catch { }
        }

        private async void SaveLogToDatabase(string message, LogType type, string icon, string? commandSent = null)
        {
            if (!_isDatabaseEnabled || _database == null) return;

            try
            {
                int? elevatorNumber = null;
                if (message.Contains("E1") || message.Contains("thang may 1")) elevatorNumber = 1;
                else if (message.Contains("E2") || message.Contains("thang may 2")) elevatorNumber = 2;

                int priority = type switch
                {
                    LogType.Emergency => 4,
                    LogType.Error => 3,
                    LogType.Warning => 2,
                    LogType.System => 2,
                    LogType.Elevator => 1,
                    LogType.User => 1,
                    _ => 2
                };

                var logEntry = new LogEntry
                {
                    CreatedAt = DateTime.UtcNow,
                    Type = type.ToString(),
                    Message = message,
                    ElevatorId = elevatorNumber,
                    CommandSent = commandSent,
                    SessionId = _database.SessionId,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    Priority = priority
                };

                // Avoid resuming on UI context to reduce UI freezes under heavy DB load
                await _database.AddLogAsync(logEntry).ConfigureAwait(false);
            }
            catch { }
        }

        private string GetLogIcon(LogType type) => type switch
        {
            LogType.System => "[SYS]",
            LogType.Elevator => "[ELV]",
            LogType.User => "[USR]",
            LogType.Emergency => "[EMG]",
            LogType.Error => "[ERR]",
            LogType.Warning => "[WRN]",
            _ => "[LOG]"
        };

        private Color GetLogColor(LogType type) => type switch
        {
            LogType.System => Color.Cyan,
            LogType.Elevator => Color.LimeGreen,
            LogType.User => Color.Yellow,
            LogType.Emergency => Color.Red,
            LogType.Error => Color.Red,
            LogType.Warning => Color.Orange,
            _ => Color.White
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeSerialSettings();
            LoadAvailablePorts();
            LoadBaudRates();
            UpdateUI();
            TestLogSystem();
            AddLog("He thong da ket noi toi thang may va san sang hoat dong", LogType.System);
        }

        private void TestLogSystem()
        {
            AddLog("Khoi tao ket noi voi he thong dieu khien thang may", LogType.System);
            AddLog("Dang kiem tra cau hinh ket noi...", LogType.Warning);
            AddLog("Giao dien dieu khien da san sang", LogType.User);
        }

        private void InitializeSerialSettings()
        {
            _serialPort = new SerialPort
            {
                Encoding = Encoding.UTF8,
                NewLine = "\n",
                ReadTimeout = 50,
                WriteTimeout = 200,
                ReadBufferSize = 64 * 1024,
                WriteBufferSize = 16 * 1024
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void LoadAvailablePorts()
        {
            if (cmbComPort == null) return;

            cmbComPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length > 0)
            {
                cmbComPort.Items.AddRange(ports);
                cmbComPort.SelectedIndex = 0;
                AddLog($"Tim thay {ports.Length} COM port", LogType.System);
            }
            else
            {
                cmbComPort.Items.Add("Khong co COM port");
                cmbComPort.SelectedIndex = 0;
                AddLog("Khong tim thay COM port nao", LogType.Warning);
            }
        }

        private void LoadBaudRates()
        {
            if (cmbBaudRate == null) return;

            // Giữ <= 57600 theo yêu cầu
            int[] baudRates = { 9600, 19200, 38400, 57600 };
            cmbBaudRate.Items.Clear();
            cmbBaudRate.Items.AddRange(baudRates.Cast<object>().ToArray());
            cmbBaudRate.SelectedItem = 57600;  // mặc định 57k6 cho realtime ổn
        }

        private void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            LoadAvailablePorts();
            AddLog("[PORT] Lam moi danh sach COM port", LogType.User);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!_isConnected) ConnectSerial();
            else DisconnectSerial();
        }

        private void ConnectSerial()
        {
            try
            {
                if (cmbComPort.SelectedItem == null || cmbBaudRate.SelectedItem == null)
                {
                    MessageBox.Show("Vui long chon COM Port va Baud Rate!", "Loi",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    AddLog("Thieu thong tin ket noi COM Port", LogType.Warning);
                    return;
                }

                string portName = cmbComPort.SelectedItem.ToString();
                if (portName == "Khong co COM port")
                {
                    MessageBox.Show("Khong co COM port kha dung!", "Loi",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddLog("Khong co COM port kha dung", LogType.Error);
                    return;
                }

                _serialPort.PortName = portName;
                _serialPort.BaudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;

                _serialPort.Open();
                _isConnected = true;

                InitializeElevatorSystem();

                AddLog($"Da ket noi toi thang may qua {portName} @ {_serialPort.BaudRate}", LogType.System);
                MessageBox.Show($"Da ket noi thanh cong toi thang may qua {portName}!\nHe thong thang may da san sang dieu khien.", "Ket noi thanh cong",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                UpdateUI();
            }
            catch (Exception ex)
            {
                AddLog($"Loi ket noi toi thang may: {ex.Message}", LogType.Error);
                MessageBox.Show($"Loi ket noi toi thang may: {ex.Message}", "Loi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectSerial()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    SendCommand("SYSTEM_SHUTDOWN");
                    _serialPort.Close();
                }
                _isConnected = false;

                _elevator1.Reset();
                _elevator2.Reset();

                AddLog("Ngat ket noi khoi he thong thang may", LogType.System);
                MessageBox.Show("Da ngat ket noi khoi he thong thang may!", "Thong bao",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                UpdateUI();
            }
            catch (Exception ex)
            {
                AddLog($"Loi ngat ket noi khoi thang may: {ex.Message}", LogType.Error);
                MessageBox.Show($"Loi ngat ket noi khoi thang may: {ex.Message}", "Loi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeElevatorSystem()
        {
            // Handshake không sleep: chờ SYSTEM_READY rồi mới init cabin
            _awaitingReady = true;
            SendCommand("INIT_SYSTEM");
            AddLog("Gui INIT_SYSTEM, cho SYSTEM_READY...", LogType.System);
        }

        private void UpdateUI()
        {
            if (lblConnectionStatus == null) return;

            lblConnectionStatus.Text = _isConnected ? "Trang thai: Da ket noi toi thang may" : "Trang thai: Chua ket noi toi thang may";
            lblConnectionStatus.ForeColor = _isConnected ? Color.Green : Color.Red;

            if (btnConnect != null)
            {
                btnConnect.Text = _isConnected ? "NGAT KET NOI" : "KET NOI";
                btnConnect.BackColor = _isConnected ? Color.LightCoral : Color.LightBlue;
            }

            if (cmbComPort != null) cmbComPort.Enabled = !_isConnected;
            if (cmbBaudRate != null) cmbBaudRate.Enabled = !_isConnected;
            if (btnRefreshPorts != null) btnRefreshPorts.Enabled = !_isConnected;

            UpdateElevatorUI(1, _elevator1);
            UpdateElevatorUI(2, _elevator2);

            EnableElevatorControls(_isConnected);
        }

        private void UpdateElevatorUI(int elevatorNum, ElevatorInfo elevator)
        {
            if (elevatorNum == 1 && lblElevator1Status != null && lblElevator1Position != null)
            {
                lblElevator1Status.Text = $"Trang thai: {elevator.Status}";
                lblElevator1Position.Text = $"Vi tri: {GetFloorName(elevator.CurrentFloor)}";
                lblElevator1Status.ForeColor = GetStatusColor(elevator.Status);
            }
            else if (elevatorNum == 2 && lblElevator2Status != null && lblElevator2Position != null)
            {
                lblElevator2Status.Text = $"Trang thai: {elevator.Status}";
                lblElevator2Position.Text = $"Vi tri: {GetFloorName(elevator.CurrentFloor)}";
                lblElevator2Status.ForeColor = GetStatusColor(elevator.Status);
            }
        }

        private void EnableElevatorControls(bool enabled)
        {
            // Elevator 1 controls
            if (btnE1Floor0 != null) btnE1Floor0.Enabled = enabled;
            if (btnE1Floor1 != null) btnE1Floor1.Enabled = enabled;
            if (btnE1Floor2 != null) btnE1Floor2.Enabled = enabled;
            if (btnE1Floor3 != null) btnE1Floor3.Enabled = enabled;
            if (btnE1Floor4 != null) btnE1Floor4.Enabled = enabled;
            if (btnE1Floor5 != null) btnE1Floor5.Enabled = enabled;
            if (btnE1Stop != null) btnE1Stop.Enabled = enabled;
            if (btnE1OpenDoor != null) btnE1OpenDoor.Enabled = enabled;
            if (btnE1CloseDoor != null) btnE1CloseDoor.Enabled = enabled;
            if (btnE1Restart != null) btnE1Restart.Enabled = enabled;

            // Elevator 2 controls
            if (btnE2Floor0 != null) btnE2Floor0.Enabled = enabled;
            if (btnE2Floor1 != null) btnE2Floor1.Enabled = enabled;
            if (btnE2Floor2 != null) btnE2Floor2.Enabled = enabled;
            if (btnE2Floor3 != null) btnE2Floor3.Enabled = enabled;
            if (btnE2Floor4 != null) btnE2Floor4.Enabled = enabled;
            if (btnE2Floor5 != null) btnE2Floor5.Enabled = enabled;
            if (btnE2Stop != null) btnE2Stop.Enabled = enabled;
            if (btnE2OpenDoor != null) btnE2OpenDoor.Enabled = enabled;
            if (btnE2CloseDoor != null) btnE2CloseDoor.Enabled = enabled;
            if (btnE2Restart != null) btnE2Restart.Enabled = enabled;

            // Floor call buttons with direction
            if (btnCallFloor0Up != null) btnCallFloor0Up.Enabled = enabled;
            if (btnCallFloor1Up != null) btnCallFloor1Up.Enabled = enabled;
            if (btnCallFloor1Down != null) btnCallFloor1Down.Enabled = enabled;
            if (btnCallFloor2Up != null) btnCallFloor2Up.Enabled = enabled;
            if (btnCallFloor2Down != null) btnCallFloor2Down.Enabled = enabled;
            if (btnCallFloor3Up != null) btnCallFloor3Up.Enabled = enabled;
            if (btnCallFloor3Down != null) btnCallFloor3Down.Enabled = enabled;
            if (btnCallFloor4Up != null) btnCallFloor4Up.Enabled = enabled;
            if (btnCallFloor4Down != null) btnCallFloor4Down.Enabled = enabled;
            if (btnCallFloor5Down != null) btnCallFloor5Down.Enabled = enabled;

            // Emergency buttons
            if (btnEmergencyStop != null) btnEmergencyStop.Enabled = enabled;
            if (btnFireAlarm != null) btnFireAlarm.Enabled = enabled;
            if (btnMaintenanceMode != null) btnMaintenanceMode.Enabled = enabled;
        }

        #region Elevator Control Methods

        private void ElevatorGoToFloor(int elevatorNum, int targetFloor)
        {
            string command = $"E{elevatorNum}_GOTO_{targetFloor}";
            if (SendCommand(command))
            {
                AddLog($"[E{elevatorNum}] Lenh thang may {elevatorNum} di den {GetFloorName(targetFloor)}", LogType.Elevator);
            }
        }

        private void ElevatorStop(int elevatorNum)
        {
            string command = $"E{elevatorNum}_STOP";
            if (SendCommand(command))
            {
                AddLog($"[E{elevatorNum}] Lenh dung thang may {elevatorNum}", LogType.Emergency);
            }
        }

        private void ElevatorOpenDoor(int elevatorNum)
        {
            string command = $"E{elevatorNum}_OPEN_DOOR";
            if (SendCommand(command))
            {
                AddLog($"[E{elevatorNum}] Lenh mo cua thang may {elevatorNum}", LogType.Elevator);
            }
        }

        private void ElevatorCloseDoor(int elevatorNum)
        {
            string command = $"E{elevatorNum}_CLOSE_DOOR";
            if (SendCommand(command))
            {
                AddLog($"[E{elevatorNum}] Lenh dong cua thang may {elevatorNum}", LogType.Elevator);
            }
        }

        private void RestartElevator(int elevatorNum)
        {
            DialogResult result = MessageBox.Show(
                $"Ban co chac muon khoi dong lai Thang may {elevatorNum}?\n\nViec nay se:\n- Dung thang may {elevatorNum}\n- Reset ve tang tret\n- Khoi tao lai thang may {elevatorNum}",
                $"Xac nhan khoi dong lai Thang may {elevatorNum}",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string command = $"E{elevatorNum}_RESTART";
                if (SendCommand(command))
                {
                    var elevator = elevatorNum == 1 ? _elevator1 : _elevator2;
                    elevator.Reset();

                    AddLog($"[SYS] LENH KHOI DONG LAI THANG MAY {elevatorNum}", LogType.System);
                    AddLog($"[SYS] Dang reset thang may {elevatorNum} ve trang thai ban dau...", LogType.System);

                    Task.Run(async () =>
                    {
                        await Task.Delay(800);
                        if (_isConnected && _serialPort != null && _serialPort.IsOpen)
                        {
                            this.Invoke(new Action(() =>
                            {
                                SendCommand($"E{elevatorNum}_INIT");
                                UpdateUI();
                                AddLog($"[SYS] Thang may {elevatorNum} da duoc khoi dong lai thanh cong", LogType.System);
                            }));
                        }
                    });

                    MessageBox.Show($"Da gui lenh khoi dong lai thang may {elevatorNum}!", "Khoi dong lai",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void CallElevatorToFloorWithDirection(int floor, string direction)
        {
            string command = $"CALL_TO_FLOOR_{floor}_{direction}";
            if (SendCommand(command))
            {
                string directionText = direction == "UP" ? "len" : "xuong";
                AddLog($"[HALL] Goi thang may den {GetFloorName(floor)} huong {directionText}", LogType.User);
            }
        }

        private void EmergencyStop()
        {
            if (SendCommand("EMERGENCY_STOP_ALL"))
            {
                AddLog("[EMG] LENH DUNG KHAN CAP - Tat ca thang may", LogType.Emergency);
                MessageBox.Show("DUNG KHAN CAP!\nDa gui lenh dung tat ca thang may.", "Khan cap",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FireAlarm()
        {
            if (SendCommand("FIRE_ALARM"))
            {
                AddLog("[EMG] LENH BAO CHAY - Che do khan cap", LogType.Emergency);
                MessageBox.Show("BAO CHAY!\nDa gui lenh chuyen sang che do khan cap.", "Bao chay",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MaintenanceMode()
        {
            if (SendCommand("MAINTENANCE_MODE"))
            {
                AddLog("[SYS] LENH CHUYEN SANG CHE DO BAO TRI", LogType.System);
                MessageBox.Show("Da gui lenh chuyen sang che do bao tri.", "Bao tri",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Helper Methods

        private string GetFloorName(int floor) => floor == 0 ? "Tang tret (0)" : $"Tang {floor}";

        private Color GetStatusColor(string status) => status switch
        {
            "Cho" => Color.Blue,
            "Dang di chuyen" => Color.Orange,
            "Mo cua" => Color.Green,
            "Dong cua" => Color.DarkGreen,
            "Da dung" => Color.Red,
            "Khan cap" => Color.Red,
            "Bao chay" => Color.Red,
            "Bao tri" => Color.Purple,
            "Dang phuc vu" => Color.DarkBlue,
            _ => Color.Gray
        };

        private bool SendCommand(string command)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(command);
                    AddLog($"[CMD] Gui lenh: {command}", LogType.System);
                    SaveLogToDatabase($"Gui lenh: {command}", LogType.System, "[CMD]", command);
                    return true;
                }
                else
                {
                    AddLog("[ERROR] Chua ket noi he thong", LogType.Warning);
                    MessageBox.Show("Chua ket noi he thong!", "Loi",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"[ERROR] Loi gui lenh: {ex.Message}", LogType.Error);
                MessageBox.Show($"Loi gui lenh: {ex.Message}", "Loi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetButtonPressed(char grp, int idx, bool pressed)
        {
            if (_btnMap.TryGetValue((grp, idx), out var btn) && btn != null)
            {
                var onColor = Color.Gold;
                var offColor = _btnOrig.TryGetValue(btn, out var c) ? c : SystemColors.Control;
                btn.BackColor = pressed ? onColor : offColor;

                if (pressed) _btnLastOn[(grp, idx)] = DateTime.UtcNow;
                else _btnLastOn.Remove((grp, idx));
            }
        }

        #endregion

        #region Serial Communication

        // Efficiently find the first newline index in StringBuilder without ToString() allocations
        private static int IndexOfNewline(StringBuilder sb)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '\n') return i;
            }
            return -1;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string chunk = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                lock (_rxBuf)
                {
                    _rxBuf.Append(chunk);
                    int idx;
                    while ((idx = IndexOfNewline(_rxBuf)) >= 0)
                    {
                        string line = _rxBuf.ToString(0, idx).TrimEnd('\r');
                        _rxBuf.Remove(0, idx + 1);
                        this.BeginInvoke(new Action(() => ProcessReceivedData(line)));
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"Loi doc du lieu: {ex.Message}", LogType.Error);
            }
        }

        private void ProcessReceivedData(string data)
        {
            try
            {
                if (data.StartsWith("EV,"))
                {
                    ProcessEventData(data);
                }
                else if (data.StartsWith("S,"))
                {
                    // snapshot: dùng để đồng bộ trạng thái nền – không log để tránh spam
                    ProcessSnapshotData(data);
                }
                else if (data.StartsWith("CMD,"))
                {
                    ProcessCommandResponse(data);
                }
                else
                {
                    ProcessLegacyTextData(data);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Loi xu ly du lieu: {ex.Message} - Data: {data}", LogType.Error);
            }
        }

        private void ProcessEventData(string csvLine)
        {
            var parts = csvLine.Split(',');
            if (parts.Length < 3) return;

            try
            {
                if (parts[1] == "BTN" && parts.Length >= 6)
                {
                    char group = parts[2][0];
                    int idx = int.Parse(parts[3]);
                    bool pressed = parts[4] == "1";

                    // Đổi màu nút ngay
                    SetButtonPressed(group, idx, pressed);

                    // Log (chỉ khi pressed để gọn)
                    if (pressed)
                    {
                        switch (group)
                        {
                            case 'H':
                                string direction = (idx % 2 == 0) ? "Len" : "Xuong";
                                int floor = GetFloorFromHallIndex(idx);
                                AddLog($"[HALL] Goi thang may tai tang {floor} huong {direction}", LogType.User);
                                break;
                            case 'A':
                                AddLog($"[CAB-A] Chon tang {idx} trong thang may A", LogType.Elevator);
                                break;
                            case 'B':
                                AddLog($"[CAB-B] Chon tang {idx} trong thang may B", LogType.Elevator);
                                break;
                            case 'C':
                                char closeCabin = (idx == 0) ? 'A' : 'B';
                                AddLog($"[DOOR] Nhan nut dong cua thang may {closeCabin}", LogType.Elevator);
                                break;
                            case 'O':
                                char openCabin = (idx == 0) ? 'A' : 'B';
                                AddLog($"[DOOR] Nhan nut mo cua thang may {openCabin}", LogType.Elevator);
                                break;
                        }
                    }
                }
                else if (parts[1] == "ST" && parts.Length >= 7)
                {
                    char car = parts[2][0];
                    int floor = int.Parse(parts[3]);
                    int dir = int.Parse(parts[4]);
                    int state = int.Parse(parts[5]);

                    ProcessElevatorStateChange(car, floor, dir, state);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Loi parse event CSV: {ex.Message}", LogType.Error);
            }
        }

        private void ProcessSnapshotData(string csvLine)
        {
            var parts = csvLine.Split(',');
            if (parts.Length < 16) return;

            try
            {
                int floorA = int.Parse(parts[2]);
                int stateA = int.Parse(parts[4]);
                int floorB = int.Parse(parts[7]);
                int stateB = int.Parse(parts[9]);

                UpdateElevatorFromSnapshot('A', floorA, stateA);
                UpdateElevatorFromSnapshot('B', floorB, stateB);

                UpdateUI(); // đồng bộ UI nhẹ nhàng theo snapshot
            }
            catch (Exception ex)
            {
                AddLog($"Loi parse snapshot CSV: {ex.Message}", LogType.Error);
            }
        }

        private void ProcessCommandResponse(string data)
        {
            var parts = data.Split(',');
            if (parts.Length >= 3)
            {
                string status = parts[1];
                string command = parts[2];

                if (status == "ACK") AddLog($"[ACK] Arduino xac nhan lenh: {command}", LogType.System);
                else if (status == "ERROR") AddLog($"[ERROR] Arduino bao loi lenh: {command}", LogType.Error);
            }
        }

        private void ProcessElevatorStateChange(char car, int floor, int dir, int state)
        {
            var elevator = (car == 'A') ? _elevator1 : _elevator2;
            int elevatorNum = (car == 'A') ? 1 : 2;

            elevator.CurrentFloor = floor;
            elevator.Status = GetStatusFromState(state);

            string dirText = GetDirectionText(dir);
            string floorText = GetFloorName(floor);

            switch (state)
            {
                case 0: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang cho tai {floorText}", LogType.Elevator); break;
                case 1: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} mo cua tai {floorText}", LogType.Elevator); break;
                case 2: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang cho khach tai {floorText}", LogType.Elevator); break;
                case 3: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dong cua tai {floorText}", LogType.Elevator); break;
                case 4: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} san sang di chuyen tu {floorText}", LogType.Elevator); break;
                case 5: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang di chuyen {dirText} tu {floorText}", LogType.Elevator); break;
                case 6: AddLog($"[E{elevatorNum}] Thang may {elevatorNum} da den {floorText}", LogType.Elevator); break;
            }

            UpdateUI();
        }

        private void UpdateElevatorFromSnapshot(char car, int floor, int state)
        {
            var elevator = (car == 'A') ? _elevator1 : _elevator2;
            elevator.CurrentFloor = floor;
            elevator.Status = GetStatusFromState(state);
        }

        private void ProcessLegacyTextData(string data)
        {
            if (data.Contains("SYSTEM_READY"))
            {
                AddLog("[SYS] He thong Arduino da san sang", LogType.System);
                if (_awaitingReady)
                {
                    _awaitingReady = false;
                    SendCommand("E1_INIT");
                    SendCommand("E2_INIT");
                    AddLog("[SYS] Da gui E1_INIT, E2_INIT", LogType.System);
                }
            }
            else if (data.Contains("SYSTEM_ERROR"))
            {
                AddLog("[SYS] Loi ket noi he thong Arduino", LogType.Error);
                MessageBox.Show("Loi ket noi he thong thang may!", "Loi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Không log RAW để tránh spam
            }
        }

        private int GetFloorFromHallIndex(int hallIdx)
        {
            int[] hallFloorMap = { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5 };
            return (hallIdx < hallFloorMap.Length) ? hallFloorMap[hallIdx] : 0;
        }

        private string GetDirectionText(int dir) => dir switch
        {
            -1 => "xuong",
            0 => "dung",
            1 => "len",
            _ => "khong xac dinh"
        };

        private string GetStatusFromState(int state) => state switch
        {
            0 => "Cho",
            1 => "Mo cua",
            2 => "Dang cho",
            3 => "Dong cua",
            4 => "San sang",
            5 => "Dang di chuyen",
            6 => "Da den",
            _ => "Khong xac dinh"
        };

        #endregion

        #region Filter Event Handlers

        private void BtnFilterSystem_Click(object sender, EventArgs e)
        {
            _currentFilter = "system";
            FilterLogs();
            AddLog("[FILTER] Hien thi log he thong", LogType.System);
        }

        private void BtnFilterElevator_Click(object sender, EventArgs e)
        {
            _currentFilter = "elevator";
            FilterLogs();
            AddLog("[FILTER] Hien thi log thang may", LogType.System);
        }

        private void BtnFilterEmergency_Click(object sender, EventArgs e)
        {
            _currentFilter = "emergency";
            FilterLogs();
            AddLog("[FILTER] Hien thi log khan cap", LogType.System);
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            _currentFilter = "all";
            FilterLogs();
            AddLog("[FILTER] Hien thi tat ca log", LogType.System);
        }

        private void BtnFilterByDate_Click(object sender, EventArgs e)
        {
            _currentFilter = "date";
            FilterLogs();
            var selectedDate = dtpFilterDate.Value.ToString("dd/MM/yyyy");
            AddLog($"[FILTER] Hien thi log ngay {selectedDate}", LogType.System);
        }

        private void DtpFilterDate_ValueChanged(object sender, EventArgs e)
        {
            if (_currentFilter == "date")
            {
                FilterLogs();
                var selectedDate = dtpFilterDate.Value.ToString("dd/MM/yyyy");
                AddLog($"[FILTER] Da thay doi loc theo ngay {selectedDate}", LogType.System);
            }
        }

        // Updated: if DB is enabled, query from DB; otherwise fallback to local buffer
        private void FilterLogs()
        {
            if (_isDatabaseEnabled && _database != null)
            {
                _ = LoadLogsFromDatabaseAsync();
                return;
            }

            if (rtbLog == null || rtbLog.IsDisposed) return;

            try
            {
                rtbLog.Clear();
                var filteredLogs = _allLogs.Where(log => ShouldShowLog(log)).ToList();

                foreach (var log in filteredLogs)
                {
                    rtbLog.SelectionStart = rtbLog.TextLength;
                    rtbLog.SelectionLength = 0;
                    rtbLog.SelectionColor = log.Color;
                    rtbLog.AppendText(log.FormattedText);
                }

                rtbLog.SelectionColor = rtbLog.ForeColor;
                rtbLog.ScrollToCaret();

                UpdateFilterButtonStates();
            }
            catch { }
        }

        private bool ShouldShowLog(LogItem log)
        {
            return _currentFilter switch
            {
                "system" => log.Type == "System" || log.Type == "Warning" || log.Type == "Error",
                "elevator" => log.Type == "Elevator",
                "emergency" => log.Type == "Emergency",
                "date" => log.Timestamp.Date == dtpFilterDate.Value.Date,
                "all" => true,
                _ => true
            };
        }

        private void UpdateFilterButtonStates()
        {
            btnFilterSystem.BackColor = Color.LightYellow;
            btnFilterElevator.BackColor = Color.LightSkyBlue;
            btnFilterEmergency.BackColor = Color.LightPink;
            btnResetFilter.BackColor = Color.LightGray;
            btnFilterByDate.BackColor = Color.LightCyan;

            switch (_currentFilter)
            {
                case "system": btnFilterSystem.BackColor = Color.Gold; break;
                case "elevator": btnFilterElevator.BackColor = Color.DeepSkyBlue; break;
                case "emergency": btnFilterEmergency.BackColor = Color.HotPink; break;
                case "date": btnFilterByDate.BackColor = Color.DarkCyan; break;
                case "all": btnResetFilter.BackColor = Color.Silver; break;
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Ban co chac muon xoa tat ca log?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                rtbLog.Clear();
                _allLogs.Clear();
                AddLog("Log da duoc xoa (chi xoa hien thi - database van giu)", LogType.System);
            }
        }

        // Query logs from database according to current filter and render
        private async Task LoadLogsFromDatabaseAsync()
        {
            try
            {
                if (_database == null || !_isDatabaseEnabled) return;

                DateTime? date = _currentFilter == "date" ? dtpFilterDate.Value.Date : (DateTime?)null;
                var logs = await _database.QueryLogsAsync(_currentFilter, date);

                if (rtbLog == null || rtbLog.IsDisposed) return;
                rtbLog.Clear();

                foreach (var log in logs)
                {
                    // Map type string to color/icon
                    if (!Enum.TryParse<LogType>(log.Type, out var t)) t = LogType.System;
                    string icon = GetLogIcon(t);
                    Color color = GetLogColor(t);
                    string ts = log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    string line = $"[{ts}] {icon} {log.Message}\n";

                    rtbLog.SelectionStart = rtbLog.TextLength;
                    rtbLog.SelectionLength = 0;
                    rtbLog.SelectionColor = color;
                    rtbLog.AppendText(line);
                }

                rtbLog.SelectionColor = rtbLog.ForeColor;
                rtbLog.ScrollToCaret();
                UpdateFilterButtonStates();
            }
            catch (Exception ex)
            {
                AddLog($"[DB] Loi truy van log: {ex.Message}", LogType.Error);
            }
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                AddLog("[SYS] Dang tat ung dung...", LogType.System);

                if (_serialPort != null && _serialPort.IsOpen)
                {
                    SendCommand("SYSTEM_SHUTDOWN");
                    _serialPort.Close();
                }

                if (_isDatabaseEnabled)
                {
                    AddLog("[SYS] Ket thuc session - da luu vao database", LogType.System);
                    // Không sleep để tránh chặn tắt ứng dụng
                }
            }
            catch (Exception ex)
            {
                AddLog($"[ERROR] Loi dong Serial Port: {ex.Message}", LogType.Error);
            }
        }

        private void lblSelectDate_Click(object sender, EventArgs e) { }
        private void lblElevator2Position_Click(object sender, EventArgs e) { }
        private void lblElevator2Status_Click(object sender, EventArgs e) { }

        // ====== Database bootstrap ======
        private async void InitializeDatabaseService()
        {
            try
            {
                var cfg = LoadDbConfigOrCreateTemplate();
                var connString = BuildConnectionString(cfg);

                _database = new SimpleDatabase(connString);
                var isConnected = await _database.TestConnectionAsync().ConfigureAwait(false);
                if (isConnected)
                {
                    var tablesCreated = await _database.CreateTablesAsync().ConfigureAwait(false);
                    _isDatabaseEnabled = tablesCreated;

                    if (_isDatabaseEnabled) AddLog("[DB] Ket noi PostgreSQL thanh cong", LogType.System);
                    else AddLog("[DB] Khong the tao bang database", LogType.Warning);
                }
                else
                {
                    AddLog("[DB] Khong the ket noi PostgreSQL - chi luu log local", LogType.Warning);
                    _isDatabaseEnabled = false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"[ERROR] Loi khoi tao database: {ex.Message}", LogType.Error);
                _isDatabaseEnabled = false;
            }
        }

        private static DbBasicConfig LoadDbConfigOrCreateTemplate()
        {
            const string file = "config.json";
            try
            {
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    var cfg = JsonSerializer.Deserialize<DbBasicConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (cfg == null) throw new Exception("File config.json khong hop le");
                    if (string.IsNullOrWhiteSpace(cfg.Host) || string.IsNullOrWhiteSpace(cfg.Username))
                        throw new Exception("Thieu Host/Username trong config.json");
                    if (cfg.Port <= 0) cfg.Port = 5432;
                    return cfg;
                }
                else
                {
                    var template = new DbBasicConfig
                    {
                        Host = "127.0.0.1",
                        Port = 5432,
                        Username = "postgres",
                        Password = "123456"
                    };
                    var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(file, json);
                    return template;
                }
            }
            catch
            {
                // Fallback mặc định local
                return new DbBasicConfig
                {
                    Host = "127.0.0.1",
                    Port = 5432,
                    Username = "postgres",
                    Password = "123456"
                };
            }
        }

        private static string BuildConnectionString(DbBasicConfig cfg)
        {
            // Ép SSL Require nếu dùng Supabase
            bool isSupabase = cfg.Host.EndsWith(".supabase.co", StringComparison.OrdinalIgnoreCase);
            string sslMode = isSupabase ? "Require" : "Disable";
            var cs = $"Host={cfg.Host};Port={cfg.Port};Database=postgres;Username={cfg.Username};Password={cfg.Password};SSL Mode={sslMode};Timeout=30;";
            return cs;
        }
    }

    // ==== DB Config types ====
    public class DbBasicConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5432;
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "123456";
    }

    // ==== Database runtime ====
    public class LogEntry
    {
        public long LogId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? ElevatorId { get; set; }
        public int? CurrentFloor { get; set; }
        public int? TargetFloor { get; set; }
        public string? CommandSent { get; set; }
        public string? SessionId { get; set; }
        public string? MachineName { get; set; }
        public string? UserName { get; set; }
        public int Priority { get; set; } = 2;
    }

    public class SimpleDatabase
    {
        private readonly string _connectionString;
        private readonly string _sessionId;
        private bool _isConnected = false;

        public bool IsConnected => _isConnected;
        public string SessionId => _sessionId;

        public SimpleDatabase(string connectionString)
        {
            _connectionString = connectionString;
            _sessionId = Guid.NewGuid().ToString("N")[..8];
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                using var cmd = new NpgsqlCommand("SELECT 1", connection);
                await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                _isConnected = true;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> CreateTablesAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                var sql = @"
                    CREATE TABLE IF NOT EXISTS elevator (
                      elevator_id   SMALLSERIAL PRIMARY KEY,
                      code          VARCHAR(16)  NOT NULL UNIQUE,
                      display_name  VARCHAR(64)  NOT NULL
                    );

                    INSERT INTO elevator (code, display_name)
                    VALUES ('E1','Thang máy 1'), ('E2','Thang máy 2')
                    ON CONFLICT (code) DO NOTHING;

                    CREATE TABLE IF NOT EXISTS log (
                      log_id        BIGSERIAL    PRIMARY KEY,
                      created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
                      type          TEXT         NOT NULL,
                      message       TEXT         NOT NULL,
                      elevator_id   SMALLINT     NULL REFERENCES elevator(elevator_id) ON DELETE SET NULL,
                      current_floor SMALLINT     NULL CHECK (current_floor BETWEEN 0 AND 5),
                      target_floor  SMALLINT     NULL CHECK (target_floor BETWEEN 0 AND 5),
                      command_sent  VARCHAR(128) NULL,
                      session_id    VARCHAR(50)  NULL,
                      machine_name  VARCHAR(100) NULL,
                      user_name     VARCHAR(100) NULL,
                      priority      SMALLINT     NOT NULL DEFAULT 2 CHECK (priority BETWEEN 1 AND 4)
                    );

                    CREATE INDEX IF NOT EXISTS ix_log_created_at ON log (created_at DESC);
                    CREATE INDEX IF NOT EXISTS ix_log_type_time ON log (type, created_at DESC);
                ";

                using var command = new NpgsqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _isConnected = true;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> AddLogAsync(LogEntry logEntry)
        {
            if (!_isConnected)
            {
                await TestConnectionAsync().ConfigureAwait(false);
                if (!_isConnected) return false;
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                int? elevatorId = null;
                if (logEntry.ElevatorId.HasValue)
                {
                    var elevatorSql = "SELECT elevator_id FROM elevator WHERE code = @code";
                    using var elevatorCmd = new NpgsqlCommand(elevatorSql, connection);
                    elevatorCmd.Parameters.AddWithValue("@code", $"E{logEntry.ElevatorId}");
                    var result = await elevatorCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null)
                        elevatorId = Convert.ToInt32(result);
                }

                var sql = @"
                    INSERT INTO log 
                    (created_at, type, message, elevator_id, current_floor, target_floor, 
                     command_sent, session_id, machine_name, user_name, priority)
                    VALUES 
                    (@created_at, @type, @message, @elevator_id, @current_floor, @target_floor,
                     @command_sent, @session_id, @machine_name, @user_name, @priority)
                    RETURNING log_id;
                ";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@created_at", logEntry.CreatedAt);
                command.Parameters.AddWithValue("@type", logEntry.Type);
                command.Parameters.AddWithValue("@message", logEntry.Message);
                command.Parameters.AddWithValue("@elevator_id", elevatorId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@current_floor", logEntry.CurrentFloor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@target_floor", logEntry.TargetFloor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@command_sent", logEntry.CommandSent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@session_id", logEntry.SessionId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@machine_name", logEntry.MachineName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@user_name", logEntry.UserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@priority", logEntry.Priority);

                var id = await command.ExecuteScalarAsync().ConfigureAwait(false);
                return id != null;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        // New: query logs by filter and optional date
        public async Task<List<LogEntry>> QueryLogsAsync(string filter, DateTime? date)
        {
            var results = new List<LogEntry>();

            if (!_isConnected)
            {
                await TestConnectionAsync().ConfigureAwait(false);
                if (!_isConnected) return results;
            }

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            var conditions = new List<string>();
            var cmd = new NpgsqlCommand();
            cmd.Connection = connection;

            // Type filter
            if (string.Equals(filter, "elevator", StringComparison.OrdinalIgnoreCase))
            {
                conditions.Add("type = 'Elevator'");
            }
            else if (string.Equals(filter, "emergency", StringComparison.OrdinalIgnoreCase))
            {
                conditions.Add("type = 'Emergency'");
            }
            else if (string.Equals(filter, "system", StringComparison.OrdinalIgnoreCase))
            {
                conditions.Add("type IN ('System','Warning','Error')");
            }

            // Date filter (by local day)
            if (date.HasValue)
            {
                var dayLocal = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Local);
                var fromUtc = dayLocal.ToUniversalTime();
                var toUtc = dayLocal.AddDays(1).ToUniversalTime();
                conditions.Add("created_at >= @from AND created_at < @to");
                cmd.Parameters.AddWithValue("@from", fromUtc);
                cmd.Parameters.AddWithValue("@to", toUtc);
            }

            var where = conditions.Count > 0 ? (" WHERE " + string.Join(" AND ", conditions)) : string.Empty;
            cmd.CommandText = $@"SELECT log_id, created_at, type, message, elevator_id, current_floor, target_floor,
                                         command_sent, session_id, machine_name, user_name, priority
                                  FROM log{where}
                                  ORDER BY created_at ASC
                                  LIMIT 500";

            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                results.Add(new LogEntry
                {
                    LogId = reader.GetInt64(0),
                    CreatedAt = reader.GetDateTime(1),
                    Type = reader.GetString(2), 
                    Message = reader.GetString(3),
                    ElevatorId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    CurrentFloor = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    TargetFloor = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    CommandSent = reader.IsDBNull(7) ? null : reader.GetString(7),
                    SessionId = reader.IsDBNull(8) ? null : reader.GetString(8),
                    MachineName = reader.IsDBNull(9) ? null : reader.GetString(9),
                    UserName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Priority = reader.GetInt32(11)
                });
            }

            return results;
        }
    }

    public class ElevatorInfo
    {
        public int ElevatorNumber { get; set; }
        public int CurrentFloor { get; set; }
        public int TargetFloor { get; set; }
        public string Status { get; set; }

        public ElevatorInfo(int elevatorNumber)
        {
            ElevatorNumber = elevatorNumber;
            CurrentFloor = 0;
            TargetFloor = 0;
            Status = "Cho";
        }

        public void Reset()
        {
            CurrentFloor = 0;
            TargetFloor = 0;
            Status = "Cho";
        }
    }
}
