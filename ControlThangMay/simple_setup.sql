-- ===================================================================
-- SCRIPT DATABASE ??N GI?N CHO H? TH?NG THANG MÁY  
-- ===================================================================

-- B?ng thang máy
CREATE TABLE IF NOT EXISTS elevator (
  elevator_id   SMALLSERIAL PRIMARY KEY,
  code          VARCHAR(16)  NOT NULL UNIQUE,
  display_name  VARCHAR(64)  NOT NULL
);

-- Thêm RLS (Row Level Security) cho Supabase
ALTER TABLE elevator ENABLE ROW LEVEL SECURITY;

-- Policy cho phép t?t c? operations (có th? tùy ch?nh sau)
DROP POLICY IF EXISTS "Allow all operations on elevator" ON elevator;
CREATE POLICY "Allow all operations on elevator" ON elevator FOR ALL USING (true);

-- Seed data
INSERT INTO elevator (code, display_name)
VALUES ('E1','Thang máy 1'), ('E2','Thang máy 2')
ON CONFLICT (code) DO NOTHING;

-- B?ng log
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

-- RLS cho b?ng log
ALTER TABLE log ENABLE ROW LEVEL SECURITY;

-- Policy cho b?ng log
DROP POLICY IF EXISTS "Allow all operations on log" ON log;
CREATE POLICY "Allow all operations on log" ON log FOR ALL USING (true);

-- Indexes c? b?n
CREATE INDEX IF NOT EXISTS ix_log_created_at ON log (created_at DESC);
CREATE INDEX IF NOT EXISTS ix_log_type_time ON log (type, created_at DESC);

-- Ki?m tra setup
SELECT 'Supabase database setup completed!' as status;