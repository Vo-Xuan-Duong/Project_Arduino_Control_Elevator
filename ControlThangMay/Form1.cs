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
        private const int MAX_FLOORS = 6; // 0-5 (T?ng tr?t ??n t?ng 5)
        private const int NUM_ELEVATORS = 2;

        // Log system ready flag
        private bool _logSystemReady = false;

        // Database integration
        private SimpleDatabase? _database;
        private bool _isDatabaseEnabled = false;

        // Log filtering
        private List<LogItem> _allLogs = new List<LogItem>();
        private string _currentFilter = "all";

        // Tab notification
        private int _newLogCount = 0;
        private bool _isLogTabActive = false;

        public Form1()
        {
            InitializeComponent();
            InitializeElevators();
            InitializeCustomSettings();
            AssignEventHandlers();
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
            System,     // H? th?ng
            Elevator,   // Ho?t ??ng thang máy
            User,       // Hành ??ng ng??i dùng
            Emergency,  // Kh?n c?p
            Error,      // L?i
            Warning     // C?nh báo
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

            // Log system is now ready
            _logSystemReady = true;

            // Initialize tab features
            InitializeTabFeatures();

            // Add initial log entry
            AddLog("[SYS] He thong dieu khien da ket noi toi thang may", LogType.System);
        }

        private async void InitializeDatabaseService()
        {
            try
            {
                var config = LoadDatabaseConfig();

                if (config.EnableLogging)
                {
                    var connectionString = config.GetConnectionString();
                    _database = new SimpleDatabase(connectionString);

                    var isConnected = await _database.TestConnectionAsync();
                    if (isConnected)
                    {
                        var tablesCreated = await _database.CreateTablesAsync();
                        _isDatabaseEnabled = tablesCreated;

                        if (_isDatabaseEnabled)
                        {
                            AddLog("[DB] Ket noi PostgreSQL thanh cong", LogType.System);
                        }
                        else
                        {
                            AddLog("[DB] Khong the tao bang database", LogType.Warning);
                        }
                    }
                    else
                    {
                        AddLog("[DB] Khong the ket noi PostgreSQL - chi luu log local", LogType.Warning);
                        _isDatabaseEnabled = false;
                    }
                }
                else
                {
                    AddLog("[DB] Database logging da duoc tat", LogType.System);
                }
            }
            catch (Exception ex)
            {
                AddLog($"[ERROR] Loi khoi tao database: {ex.Message}", LogType.Error);
                _isDatabaseEnabled = false;
            }
        }

        private DatabaseConfig LoadDatabaseConfig()
        {
            try
            {
                if (File.Exists("config.json"))
                {
                    var json = File.ReadAllText("config.json");
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };

                    var config = JsonSerializer.Deserialize<ConfigFile>(json, options);
                    if (config?.Database != null)
                    {
                        return config.Database;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }

            // Default config v?i Supabase
            var defaultConfig = new DatabaseConfig
            {
                Host = "localhost",
                Port = 5432,
                Database = "postgres",
                Username = "postgres",
                Password = "123456",
                EnableSSL = true,
                EnableLogging = true
            };

            SaveDatabaseConfig(defaultConfig);
            return defaultConfig;
        }

        private void SaveDatabaseConfig(DatabaseConfig config)
        {
            try
            {
                var configFile = new ConfigFile { Database = config };
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(configFile, options);
                File.WriteAllText("config.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

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
                try
                {
                    rtbLog.Invoke(new Action(() => AddLog(message, type)));
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("ObjectDisposedException in AddLog");
                }
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
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

                if (_allLogs.Count > 500)
                {
                    _allLogs.RemoveAt(0);
                }

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
            catch (Exception ex)
            {
                Console.WriteLine($"Log system error: {ex.Message}");
            }
        }

        private async void SaveLogToDatabase(string message, LogType type, string icon, string? commandSent = null)
        {
            if (!_isDatabaseEnabled || _database == null)
                return;

            try
            {
                int? elevatorNumber = null;
                if (message.Contains("E1") || message.Contains("thang may 1"))
                    elevatorNumber = 1;
                else if (message.Contains("E2") || message.Contains("thang may 2"))
                    elevatorNumber = 2;

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

                await _database.AddLogAsync(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving log to database: {ex.Message}");
            }
        }

        private string GetLogIcon(LogType type)
        {
            return type switch
            {
                LogType.System => "[SYS]",
                LogType.Elevator => "[ELV]",
                LogType.User => "[USR]",
                LogType.Emergency => "[EMG]",
                LogType.Error => "[ERR]",
                LogType.Warning => "[WRN]",
                _ => "[LOG]"
            };
        }

        private Color GetLogColor(LogType type)
        {
            return type switch
            {
                LogType.System => Color.Cyan,
                LogType.Elevator => Color.LimeGreen,
                LogType.User => Color.Yellow,
                LogType.Emergency => Color.Red,
                LogType.Error => Color.Red,
                LogType.Warning => Color.Orange,
                _ => Color.White
            };
        }

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
            AddLog("Giao dien dieu khien da ket noi thanh cong", LogType.User);
            AddLog("He thong log da ket noi toi thang may", LogType.Elevator);
        }

        private void InitializeSerialSettings()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Encoding = Encoding.UTF8;
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

            int[] baudRates = { 9600, 19200, 38400, 57600, 115200 };
            cmbBaudRate.Items.AddRange(baudRates.Cast<object>().ToArray());
            cmbBaudRate.SelectedIndex = 0;
        }

        private void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            LoadAvailablePorts();
            AddLog("[PORT] Lam moi danh sach COM port", LogType.User);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                ConnectSerial();
            }
            else
            {
                DisconnectSerial();
            }
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
                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;

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
            SendCommand("INIT_SYSTEM");
            Thread.Sleep(500);
            SendCommand("E1_INIT");
            Thread.Sleep(100);
            SendCommand("E2_INIT");
            AddLog("Da ket noi va khoi tao he thong thang may", LogType.System);
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
                        await Task.Delay(1500);

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

        private string GetFloorName(int floor)
        {
            return floor == 0 ? "Tang tret (0)" : $"Tang {floor}";
        }

        private Color GetStatusColor(string status)
        {
            return status switch
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
        }

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

        #endregion

        #region Serial Communication

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    string data = _serialPort.ReadLine().Trim();
                    
                    this.Invoke(new Action(() =>
                    {
                        ProcessReceivedData(data);
                    }));
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
                    string group = parts[2];
                    int idx = int.Parse(parts[3]);
                    bool pressed = parts[4] == "1";

                    if (pressed)
                    {
                        switch (group)
                        {
                            case "H":
                                string direction = (idx % 2 == 0) ? "Len" : "Xuong";
                                int floor = GetFloorFromHallIndex(idx);
                                AddLog($"[HALL] Goi thang may tai tang {floor} huong {direction}", LogType.User);
                                break;
                            case "A":
                                AddLog($"[CAB-A] Chon tang {idx} trong thang may A", LogType.Elevator);
                                break;
                            case "B":
                                AddLog($"[CAB-B] Chon tang {idx} trong thang may B", LogType.Elevator);
                                break;
                            case "C":
                                char closeCabin = (idx == 0) ? 'A' : 'B';
                                AddLog($"[DOOR] Nhan nut dong cua thang may {closeCabin}", LogType.Elevator);
                                break;
                            case "O":
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
            }
            catch (Exception ex)
            {
                AddLog($"Loi parse snapshot CSV: {ex.Message}", LogType.Error);
            }
        }

        private void ProcessCommandResponse(string data)
        {
            // CMD,ACK,COMMAND ho?c CMD,ERROR,UNKNOWN_COMMAND
            var parts = data.Split(',');
            if (parts.Length >= 3)
            {
                string status = parts[1];
                string command = parts[2];
                
                if (status == "ACK")
                {
                    AddLog($"[ACK] Arduino xac nhan lenh: {command}", LogType.System);
                }
                else if (status == "ERROR")
                {
                    AddLog($"[ERROR] Arduino bao loi lenh: {command}", LogType.Error);
                }
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
                case 0: // IDLE
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang cho tai {floorText}", LogType.Elevator);
                    break;
                case 1: // DOOR_OPEN
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} mo cua tai {floorText}", LogType.Elevator);
                    break;
                case 2: // WAIT
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang cho khach tai {floorText}", LogType.Elevator);
                    break;
                case 3: // DOOR_CLOSE
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dong cua tai {floorText}", LogType.Elevator);
                    break;
                case 4: // WAIT_PICK
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} san sang di chuyen tu {floorText}", LogType.Elevator);
                    break;
                case 5: // MOVE
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} dang di chuyen {dirText} tu {floorText}", LogType.Elevator);
                    break;
                case 6: // ARRIVED
                    AddLog($"[E{elevatorNum}] Thang may {elevatorNum} da den {floorText}", LogType.Elevator);
                    break;
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
                AddLog("[SYS] He thong Arduino da ket noi va san sang", LogType.System);
            }
            else if (data.Contains("SYSTEM_ERROR"))
            {
                AddLog("[SYS] Loi ket noi he thong Arduino", LogType.Error);
                MessageBox.Show("Loi ket noi he thong thang may!", "Loi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                AddLog($"[RAW] Tu Arduino: {data}", LogType.System);
            }
        }

        private int GetFloorFromHallIndex(int hallIdx)
        {
            int[] hallFloorMap = {0, 1, 1, 2, 2, 3, 3, 4, 4, 5};
            return (hallIdx < hallFloorMap.Length) ? hallFloorMap[hallIdx] : 0;
        }

        private string GetDirectionText(int dir)
        {
            return dir switch
            {
                -1 => "xuong",
                0 => "dung",
                1 => "len",
                _ => "khong xac dinh"
            };
        }

        private string GetStatusFromState(int state)
        {
            return state switch
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
        }

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

        private void FilterLogs()
        {
            if (rtbLog == null || rtbLog.IsDisposed)
                return;

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtering logs: {ex.Message}");
            }
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
                case "system":
                    btnFilterSystem.BackColor = Color.Gold;
                    break;
                case "elevator":
                    btnFilterElevator.BackColor = Color.DeepSkyBlue;
                    break;
                case "emergency":
                    btnFilterEmergency.BackColor = Color.HotPink;
                    break;
                case "date":
                    btnFilterByDate.BackColor = Color.DarkCyan;
                    break;
                case "all":
                    btnResetFilter.BackColor = Color.Silver;
                    break;
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Ban co chac muon xoa tat ca log?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                rtbLog.Clear();
                _allLogs.Clear();
                AddLog("Log da duoc xoa (chi xoa hien thi - database van giu)", LogType.System);
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
                    Thread.Sleep(500);
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
    }

    // Database Classes
    public class DatabaseConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "postgres";
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "123456";
        public bool EnableSSL { get; set; } = false;
        public bool EnableLogging { get; set; } = true;

        public string GetConnectionString()
        {
            var sslMode = EnableSSL ? "Require" : "Disable";
            return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};SSL Mode={sslMode};Timeout=30;";
        }
    }

    public class ConfigFile
    {
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();
    }

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
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT 1", connection);
                await cmd.ExecuteScalarAsync();
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
                await connection.OpenAsync();

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
                await command.ExecuteNonQueryAsync();
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
                await TestConnectionAsync();
                if (!_isConnected) return false;
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                int? elevatorId = null;
                if (logEntry.ElevatorId.HasValue)
                {
                    var elevatorSql = "SELECT elevator_id FROM elevator WHERE code = @code";
                    using var elevatorCmd = new NpgsqlCommand(elevatorSql, connection);
                    elevatorCmd.Parameters.AddWithValue("@code", $"E{logEntry.ElevatorId}");
                    var result = await elevatorCmd.ExecuteScalarAsync();
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

                var id = await command.ExecuteScalarAsync();
                return id != null;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
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