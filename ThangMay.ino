#include <Arduino.h>
#include <LiquidCrystal.h>
#include <stdint.h>
#include <string.h>
#include <stdio.h>

// ================== Tham s? h? th?ng ==================
const uint8_t  NUM_FLOORS             = 6;
const uint16_t DOOR_OPEN_TIME         = 1200;
const uint16_t DOOR_CLOSE_TIME        = 800;
const uint16_t STEP_MS                = 1000;
const uint16_t POLL_MS                = 20;
const uint16_t DEBOUNCE_MS            = 25;
const uint16_t WAIT_OPEN_MS           = 2000;
const uint16_t WAIT_AFTER_CLOSE_MS    = 1500;
const uint16_t ARRIVED_MS             = 1000;
const uint16_t SNAPSHOT_HEARTBEAT_MS  = 300;   // CSV snapshot ??nh k?

// ================== Enum ==================
enum Dir : int8_t { DN = -1, ST = 0, UP = 1 };
enum CarState : uint8_t { IDLE, DOOR_OPEN, WAIT, DOOR_CLOSE, WAIT_PICK, MOVE, ARRIVED };

// ================== C?u trúc d? li?u ==================
struct Car {
  uint8_t  floor;
  Dir      dir;
  CarState st;
  bool     reqUpMask[NUM_FLOORS] = {false};
  bool     reqDnMask[NUM_FLOORS] = {false};
  uint32_t t0;
  uint32_t displayTimer;
  char     name;
  bool     active;
};

struct HallCall { uint8_t floor; Dir dir; };

// ================== LCD ==================
LiquidCrystal lcdA(2, 3, 4, 5, 6, 7);
LiquidCrystal lcdB(8, 9, 10, 11, 12, 13);

struct Toast { char line[21]; uint32_t until; };
Toast toastA{"", 0};
Toast toastB{"", 0};

// In LCD không dùng String ?? tránh phân m?nh heap
static void displayLCD(LiquidCrystal &lcd, uint8_t row, const char *s) {
  lcd.setCursor(0, row);
  for (uint8_t i = 0; i < 20; ++i) lcd.write(' ');
  lcd.setCursor(0, row);
  for (uint8_t i = 0; i < 20 && s[i]; ++i) lcd.write(s[i]);
}

static void showToastFor(char carName, const char *msg, uint32_t now, uint16_t dur_ms = 900) {
  if (carName == 'A') {
    strncpy(toastA.line, msg, 20); toastA.line[20] = '\0'; toastA.until = now + dur_ms;
  } else {
    strncpy(toastB.line, msg, 20); toastB.line[20] = '\0'; toastB.until = now + dur_ms;
  }
}

static void showStateLine(Car &c, const char *msg) {
  LiquidCrystal &lcd = (c.name == 'A') ? lcdA : lcdB;
  displayLCD(lcd, 3, msg);
}

// ================== Pin c?u hình ==================
const uint8_t closed_cabA = 22;
const uint8_t opend_cabA  = A6;
const uint8_t cabA[NUM_FLOORS]     = {23, 24, 25, 26, 27, 28};
const uint8_t led_cabA[NUM_FLOORS] = {46, 47, 48, 49, 50, 51};

const uint8_t closed_cabB = 29;
const uint8_t opend_cabB  = A7;
const uint8_t cabB[NUM_FLOORS]     = {30, 31, 32, 33, 34, 35};
const uint8_t led_cabB[NUM_FLOORS] = {52, 53, A0, A1, A2, A3};

const uint8_t led_choiceA = A4;
const uint8_t led_choiceB = A5;

const uint8_t hallPins[10]  = {36, 37, 38, 39, 40, 41, 42, 43, 44, 45};
const int8_t  hallFloor[10] = {0, 1, 1, 2, 2, 3, 3, 4, 4, 5};
const int8_t  hallDirArr[10]= {+1, -1, +1, -1, +1, -1, +1, -1, +1, -1};

// ================== Debounce nhóm ==================
struct DebounceGroup {
  const uint8_t *pins{}; uint8_t count{};
  bool raw[16]{}; bool stable[16]{}; uint32_t lastChange[16]{};

  void begin(const uint8_t *arr, uint8_t n) {
    pins = arr; count = n;
    for (uint8_t i = 0; i < n; ++i) {
      pinMode(pins[i], INPUT_PULLUP);
      bool pressed = (digitalRead(pins[i]) == LOW);
      raw[i] = pressed; stable[i] = pressed; lastChange[i] = millis();
    }
  }
  void update(uint32_t now) {
    for (uint8_t i = 0; i < count; ++i) {
      bool pressed = (digitalRead(pins[i]) == LOW);
      if (pressed != raw[i]) { lastChange[i] = now; raw[i] = pressed; }
      if ((now - lastChange[i]) > DEBOUNCE_MS && stable[i] != raw[i]) stable[i] = raw[i];
    }
  }
  bool read(uint8_t i) { return stable[i]; } // true = ?ang NH?N (INPUT_PULLUP)
};

DebounceGroup hallDB, cabADB, cabBDB, closedDB, openDB;
bool hallDB_prev[10];
bool cabADB_prev[NUM_FLOORS];
bool cabBDB_prev[NUM_FLOORS];
const uint8_t closedPins[2] = {closed_cabA, closed_cabB};
bool closedDB_prev[2];
const uint8_t openPins[2] = {opend_cabA, opend_cabB};
bool openDB_prev[2];

// ================== Helper Label ==================
inline const char *floorLabel(uint8_t f) { static char buf[4]; if (f == 0) return "G"; buf[0] = '0' + f; buf[1] = '\0'; return buf; }
inline const char *dirLabel(Dir d)       { switch (d) { case UP: return "UP"; case DN: return "DN"; default: return "ST"; } }
inline const char *stateLabel(CarState s){
  switch (s) { case IDLE: return "IDLE"; case DOOR_OPEN: return "OPEN"; case WAIT: return "WAIT"; case DOOR_CLOSE: return "CLOSE"; case WAIT_PICK: return "W-PICK"; case MOVE: return "MOVE"; case ARRIVED: return "ARR"; }
  return "?";
}

// ================== Queue logic ==================
inline void addReq(Car &c, uint8_t f, Dir d) { if (f == c.floor) return; if (d == UP) c.reqUpMask[f] = true; if (d == DN) c.reqDnMask[f] = true; }
int  nextUp(const Car &c)        { for (int f = 0; f < (int)NUM_FLOORS; ++f) if (c.reqUpMask[f]) return f; return -1; }
int  nextDn(const Car &c)        { for (int f = NUM_FLOORS - 1; f >= 0; --f) if (c.reqDnMask[f]) return f; return -1; }
void clearHere(Car &c)           { c.reqUpMask[c.floor] = false; c.reqDnMask[c.floor] = false; }
int  nextAboveAny(const Car &c)  { for (int f = c.floor + 1; f < NUM_FLOORS; ++f) if (c.reqUpMask[f] || c.reqDnMask[f]) return f; return -1; }
int  nextBelowAny(const Car &c)  { for (int f = c.floor - 1; f >= 0; --f) if (c.reqUpMask[f] || c.reqDnMask[f]) return f; return -1; }
inline Dir dirTo(uint8_t from, uint8_t to) { if (to == from) return ST; return (to > from) ? UP : DN; }
int  popcount(bool arr[NUM_FLOORS]) { int c = 0; for (int i = 0; i < NUM_FLOORS; i++) if (arr[i]) c++; return c; }
int  queueLen(const Car &c)         { return popcount(c.reqUpMask) + popcount(c.reqDnMask); }

int score(const Car &c, const HallCall &h) {
  int s = abs((int)c.floor - (int)h.floor) * 10;
  bool onWay = (c.dir == UP && h.floor >= c.floor) || (c.dir == DN && h.floor <= c.floor);
  if (c.dir == h.dir && onWay) s -= 12;
  if (c.dir == ST) s -= 3;
  if (c.dir != ST && c.dir != h.dir) s += 10;
  s += queueLen(c);
  return s;
}
Car &pick(Car &A, Car &B, const HallCall &h) {
  if (A.active && !B.active) return A;
  if (!A.active && B.active) return B;
  if (A.active && B.active) {
    int a = score(A, h), b = score(B, h);
    if (a < b) return A; if (b < a) return B;
    return (queueLen(A) <= queueLen(B)) ? A : B;
  }
  return A;
}

// ================== Hi?n th? ==================
void displayCarStatus(Car &c, LiquidCrystal &lcd, uint32_t now) {
  if ((c.name == 'A' && now < toastA.until) || (c.name == 'B' && now < toastB.until)) {
    const char *t = (c.name == 'A') ? toastA.line : toastB.line;
    displayLCD(lcd, 2, t);
  } else {
    char line[21];
    snprintf(line, sizeof(line), "  Floor:%-2s  Dir:%-2s", floorLabel(c.floor), dirLabel(c.dir));
    displayLCD(lcd, 1, line);

    char dirLine[21];
    if (c.dir == UP && c.st == MOVE) {
      snprintf(dirLine, sizeof(dirLine), "      UP( %-2s)", floorLabel(c.floor));
      displayLCD(lcd, 2, dirLine);
    } else if (c.dir == DN && c.st == MOVE) {
      snprintf(dirLine, sizeof(dirLine), "     DOWN( %-2s)", floorLabel(c.floor));
      displayLCD(lcd, 2, dirLine);
    } else {
      displayLCD(lcd, 2, "              ");
    }
  }
}

void renderLed(const Car &c) {
  const uint8_t *ledPins = (c.name == 'A') ? led_cabA : led_cabB;
  for (int i = 0; i < NUM_FLOORS; i++) digitalWrite(ledPins[i], (c.reqUpMask[i] || c.reqDnMask[i]) ? HIGH : LOW);
}

// ================== Pick/Unpick LED ==================
void pickCabin(char cabinName)  { if (cabinName == 'A') digitalWrite(led_choiceA, HIGH); else if (cabinName == 'B') digitalWrite(led_choiceB, HIGH); }
void unpickCabin(char cabinName){ if (cabinName == 'A') digitalWrite(led_choiceA, LOW);  else if (cabinName == 'B') digitalWrite(led_choiceB, LOW); }

// ================== State machine ==================
bool checkRequest(bool reqMask[NUM_FLOORS]){ for (int i=0;i<NUM_FLOORS;i++) if (reqMask[i]) return true; return false; }

void loopCar(Car &c, uint32_t now) {
  switch (c.st) {
    case IDLE: {
      if (checkRequest(c.reqUpMask) || checkRequest(c.reqDnMask)) {
        int above = nextAboveAny(c), below = nextBelowAny(c);
        if      (above != -1 && below != -1) c.dir = ((above - c.floor) <= (c.floor - below)) ? UP : DN;
        else if (above != -1) c.dir = UP;
        else if (below != -1) c.dir = DN;
        else { c.dir = ST; displayLCD((c.name=='A')?lcdA:lcdB,3,"                    "); }
        if (c.dir != ST) { c.st = MOVE; c.t0 = now; showStateLine(c, "     MOVING...      "); }
      }
    } break;

    case ARRIVED: {
      if (now - c.t0 < ARRIVED_MS) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"    ARRIVED    ");
      } else {
        clearHere(c);
        c.st = DOOR_OPEN; c.t0 = now; showStateLine(c, "  DOOR OPENED      ");
        int aboveAny = nextAboveAny(c), belowAny = nextBelowAny(c);
        if      (c.dir == UP && aboveAny != -1) { /* keep */ }
        else if (c.dir == DN && belowAny != -1) { /* keep */ }
        else if (aboveAny != -1) c.dir = UP;
        else if (belowAny != -1) c.dir = DN;
        else { /* s? v? IDLE sau khi ?óng */ }
      }
    } break;

    case DOOR_OPEN: {
      if (now - c.t0 < DOOR_OPEN_TIME) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"  DOOR OPENED      ");
      } else { c.st = WAIT; c.t0 = now; showStateLine(c, "  WAITING...       "); }
    } break;

    case WAIT: {
      if (now - c.t0 < WAIT_OPEN_MS) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"  WAITING...       ");
      } else { c.st = DOOR_CLOSE; c.t0 = now; showStateLine(c, "  DOOR CLOSED      "); }
    } break;

    case DOOR_CLOSE: {
      if (now - c.t0 < DOOR_CLOSE_TIME) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"  DOOR CLOSED      ");
      } else { c.st = WAIT_PICK; c.t0 = now; showStateLine(c, "  WAITING CHOICE.. "); }
    } break;

    case WAIT_PICK: {
      if (now - c.t0 < WAIT_AFTER_CLOSE_MS) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"  WAITING CHOICE.. ");
      } else {
        int above = nextAboveAny(c), below = nextBelowAny(c);
        if      (above != -1 && below != -1) { c.dir = ((above - c.floor) <= (c.floor - below)) ? UP : DN; c.st = MOVE; showStateLine(c, "     MOVING...      "); }
        else if (above != -1) { c.dir = UP; c.st = MOVE; showStateLine(c, "     MOVING...      "); }
        else if (below != -1) { c.dir = DN; c.st = MOVE; showStateLine(c, "     MOVING...      "); }
        else { c.dir = ST; c.st = IDLE; showStateLine(c,"                    "); if (c.name=='A') unpickCabin('A'); else unpickCabin('B'); }
        c.t0 = now;

        if (!checkRequest(c.reqUpMask) && !checkRequest(c.reqDnMask)) {
          c.dir = ST; c.st = IDLE; showStateLine(c,"                    "); if (c.name=='A') unpickCabin('A'); else unpickCabin('B');
        }
      }
    } break;

    case MOVE: {
      int target = (c.dir == UP) ? nextAboveAny(c) : nextBelowAny(c);
      if (target == -1) {
        if      (c.dir == UP && nextBelowAny(c) != -1) c.dir = DN;
        else if (c.dir == DN && nextAboveAny(c) != -1) c.dir = UP;
        else { c.dir = ST; c.st = IDLE; showStateLine(c,"                    "); if (c.name=='A') unpickCabin('A'); else unpickCabin('B'); }
        c.t0 = now; break;
      }
      if (now - c.t0 < STEP_MS) {
        displayLCD((c.name=='A')?lcdA:lcdB,3,"     MOVING...      ");
      } else {
        if      (c.dir == UP) { if (c.floor < NUM_FLOORS - 1) c.floor++; }
        else if (c.dir == DN) { if (c.floor > 0) c.floor--; }
        c.t0 = now;
        if (c.floor == NUM_FLOORS - 1) c.dir = DN;
        if ((int)c.floor == target) { c.st = ARRIVED; showStateLine(c, "    ARRIVED    "); }
      }
    } break;
  }
}

// ===================== Bi?n toàn c?c =====================
uint32_t lastTick = 0;
Car A, B;

// ===================== CSV EVENT (edge/state) =====================
// EV,BTN,<grp>,<idx>,<pressed>,<t>\n  (grp: H=hall, A=cabA, B=cabB, O=open, C=close)
static void sendBtnEventCSV(char grp, uint8_t idx, bool pressed, uint32_t t) {
  Serial.print(F("EV,BTN,")); Serial.print(grp); Serial.print(',');
  Serial.print(idx); Serial.print(','); Serial.print(pressed?1:0); Serial.print(',');
  Serial.print(t); Serial.print('\n');
}

// EV,ST,<car>,<floor>,<dir>,<state>,<t>\n   (car: A/B; dir:-1/0/1; state:0..6)
static void sendStateEventCSV(char car, uint8_t f, int8_t d, uint8_t s, uint32_t t) {
  Serial.print(F("EV,ST,")); Serial.print(car); Serial.print(',');
  Serial.print(f); Serial.print(','); Serial.print((int)d); Serial.print(',');
  Serial.print(s); Serial.print(','); Serial.print(t); Serial.print('\n');
}

// Watcher phát EV,ST khi floor/dir/state ??i
struct PubState { uint8_t f; int8_t d; uint8_t s; };
PubState lastA{255,0,255}, lastB{255,0,255};
static void publishIfChanged(uint32_t now){
  if (A.floor!=lastA.f || (int8_t)A.dir!=lastA.d || (uint8_t)A.st!=lastA.s){
    sendStateEventCSV('A',A.floor,(int8_t)A.dir,(uint8_t)A.st,now);
    lastA={A.floor,(int8_t)A.dir,(uint8_t)A.st};
  }
  if (B.floor!=lastB.f || (int8_t)B.dir!=lastB.d || (uint8_t)B.st!=lastB.s){
    sendStateEventCSV('B',B.floor,(int8_t)B.dir,(uint8_t)B.st,now);
    lastB={B.floor,(int8_t)B.dir,(uint8_t)B.st};
  }
}

// ================== Telemetry helpers (CSV Snapshot) ==================
namespace Tele {

  struct CsvSnap {
    uint8_t Af,Bf; int8_t Ad,Bd; uint8_t As,Bs;
    uint8_t Aup,Adn,Bup,Bdn;
    uint16_t hall; uint8_t cabA,cabB,open,close;
  };

  inline uint8_t qMask6(const bool* m) {
    uint8_t r=0; for(uint8_t i=0;i<NUM_FLOORS;i++) if(m[i]) r|=(1<<i); return r;
  }

  inline uint8_t mask8(DebounceGroup &db, uint8_t n) {
    uint8_t r=0; for(uint8_t i=0;i<n;i++) if(db.read(i)) r|=(1<<i); return r;
  }

  inline uint16_t mask16(DebounceGroup &db, uint8_t n) {
    uint16_t r=0; for(uint8_t i=0;i<n;i++) if(db.read(i)) r|=((uint16_t)1<<i); return r;
  }

  inline void makeSnap(CsvSnap &o,
                       const Car &A, const Car &B,
                       DebounceGroup &hallDB, DebounceGroup &cabADB, DebounceGroup &cabBDB,
                       DebounceGroup &openDB, DebounceGroup &closedDB) {
    o.Af=A.floor; o.Bf=B.floor;
    o.Ad=(int8_t)A.dir; o.Bd=(int8_t)B.dir;
    o.As=(uint8_t)A.st; o.Bs=(uint8_t)B.st;
    o.Aup=qMask6(A.reqUpMask); o.Adn=qMask6(A.reqDnMask);
    o.Bup=qMask6(B.reqUpMask); o.Bdn=qMask6(B.reqDnMask);
    o.hall = mask16(hallDB,10);
    o.cabA = mask8(cabADB,NUM_FLOORS);
    o.cabB = mask8(cabBDB,NUM_FLOORS);
    o.open = mask8(openDB,2);
    o.close= mask8(closedDB,2);
  }

  inline void printSnap(const CsvSnap &s, uint32_t t) {
    Serial.print(F("S,")); Serial.print(t); Serial.print(',');
    Serial.print(s.Af); Serial.print(','); Serial.print((int)s.Ad); Serial.print(','); Serial.print(s.As); Serial.print(',');
    Serial.print(s.Aup); Serial.print(','); Serial.print(s.Adn); Serial.print(',');
    Serial.print(s.Bf); Serial.print(','); Serial.print((int)s.Bd); Serial.print(','); Serial.print(s.Bs); Serial.print(',');
    Serial.print(s.Bup); Serial.print(','); Serial.print(s.Bdn); Serial.print(',');
    Serial.print(s.hall); Serial.print(','); Serial.print(s.cabA); Serial.print(','); Serial.print(s.cabB); Serial.print(',');
    Serial.print(s.open); Serial.print(','); Serial.print(s.close); Serial.print('\n');
  }

} // namespace Tele

// ================== COMMAND PROCESSING FROM PC ==================
String inputBuffer = "";

void processCommand(String cmd) {
  cmd.trim();
  
  if (cmd == "INIT_SYSTEM") {
    Serial.println("CMD,ACK,INIT_SYSTEM");
    Serial.println("SYSTEM_READY");
  }
  else if (cmd == "E1_INIT") {
    A.active = true;
    Serial.println("CMD,ACK,E1_INIT");
  }
  else if (cmd == "E2_INIT") {
    B.active = true;
    Serial.println("CMD,ACK,E2_INIT");
  }
  else if (cmd.startsWith("E1_GOTO_")) {
    uint8_t targetFloor = cmd.substring(8).toInt();
    if (targetFloor < NUM_FLOORS) {
      Dir d = dirTo(A.floor, targetFloor);
      if (d != ST) addReq(A, targetFloor, d);
      pickCabin('A');
      Serial.print("CMD,ACK,E1_GOTO_"); Serial.println(targetFloor);
    }
  }
  else if (cmd.startsWith("E2_GOTO_")) {
    uint8_t targetFloor = cmd.substring(8).toInt();
    if (targetFloor < NUM_FLOORS) {
      Dir d = dirTo(B.floor, targetFloor);
      if (d != ST) addReq(B, targetFloor, d);
      pickCabin('B');
      Serial.print("CMD,ACK,E2_GOTO_"); Serial.println(targetFloor);
    }
  }
  else if (cmd == "E1_STOP") {
    A.dir = ST;
    A.st = IDLE;
    showStateLine(A, "   STOPPED    ");
    unpickCabin('A');
    Serial.println("CMD,ACK,E1_STOP");
  }
  else if (cmd == "E2_STOP") {
    B.dir = ST;
    B.st = IDLE;
    showStateLine(B, "   STOPPED    ");
    unpickCabin('B');
    Serial.println("CMD,ACK,E2_STOP");
  }
  else if (cmd == "E1_OPEN_DOOR") {
    if (A.st != MOVE) {
      A.st = DOOR_OPEN;
      A.t0 = millis();
      showStateLine(A, "  DOOR OPENED      ");
    }
    Serial.println("CMD,ACK,E1_OPEN_DOOR");
  }
  else if (cmd == "E2_OPEN_DOOR") {
    if (B.st != MOVE) {
      B.st = DOOR_OPEN;
      B.t0 = millis();
      showStateLine(B, "  DOOR OPENED      ");
    }
    Serial.println("CMD,ACK,E2_OPEN_DOOR");
  }
  else if (cmd == "E1_CLOSE_DOOR") {
    if (A.st == DOOR_OPEN || A.st == WAIT || A.st == WAIT_PICK) {
      A.st = DOOR_CLOSE;
      A.t0 = millis();
      showStateLine(A, "  DOOR CLOSED      ");
    }
    Serial.println("CMD,ACK,E1_CLOSE_DOOR");
  }
  else if (cmd == "E2_CLOSE_DOOR") {
    if (B.st == DOOR_OPEN || B.st == WAIT || B.st == WAIT_PICK) {
      B.st = DOOR_CLOSE;
      B.t0 = millis();
      showStateLine(B, "  DOOR CLOSED      ");
    }
    Serial.println("CMD,ACK,E2_CLOSE_DOOR");
  }
  else if (cmd == "E1_RESTART") {
    A.floor = 0;
    A.dir = ST;
    A.st = IDLE;
    A.t0 = millis();
    for (int i = 0; i < NUM_FLOORS; i++) {
      A.reqUpMask[i] = false;
      A.reqDnMask[i] = false;
    }
    showStateLine(A, "  RESTARTED    ");
    unpickCabin('A');
    Serial.println("CMD,ACK,E1_RESTART");
  }
  else if (cmd == "E2_RESTART") {
    B.floor = 0;
    B.dir = ST;
    B.st = IDLE;
    B.t0 = millis();
    for (int i = 0; i < NUM_FLOORS; i++) {
      B.reqUpMask[i] = false;
      B.reqDnMask[i] = false;
    }
    showStateLine(B, "  RESTARTED    ");
    unpickCabin('B');
    Serial.println("CMD,ACK,E2_RESTART");
  }
  else if (cmd.startsWith("CALL_TO_FLOOR_")) {
    // Format: CALL_TO_FLOOR_<floor>_<UP/DOWN> ho?c CALL_TO_FLOOR_<floor>
    int firstUnderscore = cmd.indexOf('_', 14); // sau "CALL_TO_FLOOR_"
    if (firstUnderscore > 0) {
      uint8_t floor = cmd.substring(14, firstUnderscore).toInt();
      String dirStr = cmd.substring(firstUnderscore + 1);
      Dir direction = ST;
      
      if (dirStr == "UP") direction = UP;
      else if (dirStr == "DOWN") direction = DN;
      
      if (floor < NUM_FLOORS && direction != ST) {
        HallCall h{floor, direction};
        Car &chosen = pick(A, B, h);
        pickCabin(chosen.name);
        
        if (chosen.floor == floor) {
          clearHere(chosen);
          chosen.st = DOOR_OPEN;
          chosen.t0 = millis();
          showStateLine(chosen, "  DOOR OPENED      ");
        } else {
          addReq(chosen, floor, direction);
        }
        
        Serial.print("CMD,ACK,CALL_TO_FLOOR_"); Serial.print(floor);
        Serial.print("_"); Serial.print(dirStr);
        Serial.print(",CABIN_"); Serial.println(chosen.name);
      }
    } else {
      // Format c?: CALL_TO_FLOOR_<floor>
      uint8_t floor = cmd.substring(14).toInt();
      if (floor < NUM_FLOORS) {
        // Ch?n h??ng m?c ??nh d?a trên v? trí
        Dir direction = (floor == 0) ? UP : ((floor == NUM_FLOORS-1) ? DN : UP);
        HallCall h{floor, direction};
        Car &chosen = pick(A, B, h);
        pickCabin(chosen.name);
        
        if (chosen.floor == floor) {
          clearHere(chosen);
          chosen.st = DOOR_OPEN;
          chosen.t0 = millis();
          showStateLine(chosen, "  DOOR OPENED      ");
        } else {
          addReq(chosen, floor, direction);
        }
        
        Serial.print("CMD,ACK,CALL_TO_FLOOR_"); Serial.print(floor);
        Serial.print(",CABIN_"); Serial.println(chosen.name);
      }
    }
  }
  else if (cmd == "EMERGENCY_STOP_ALL") {
    A.dir = ST; A.st = IDLE; showStateLine(A, "  EMERGENCY STOP  "); unpickCabin('A');
    B.dir = ST; B.st = IDLE; showStateLine(B, "  EMERGENCY STOP  "); unpickCabin('B');
    Serial.println("CMD,ACK,EMERGENCY_STOP_ALL");
  }
  else if (cmd == "FIRE_ALARM") {
    // ??a t?t c? thang máy v? t?ng tr?t
    for (int i = 0; i < NUM_FLOORS; i++) {
      A.reqUpMask[i] = false; A.reqDnMask[i] = false;
      B.reqUpMask[i] = false; B.reqDnMask[i] = false;
    }
    if (A.floor > 0) addReq(A, 0, DN);
    if (B.floor > 0) addReq(B, 0, DN);
    showStateLine(A, "   FIRE ALARM   "); showStateLine(B, "   FIRE ALARM   ");
    Serial.println("CMD,ACK,FIRE_ALARM");
  }
  else if (cmd == "MAINTENANCE_MODE") {
    A.active = false; B.active = false;
    A.dir = ST; A.st = IDLE; showStateLine(A, "  MAINTENANCE  ");
    B.dir = ST; B.st = IDLE; showStateLine(B, "  MAINTENANCE  ");
    unpickCabin('A'); unpickCabin('B');
    Serial.println("CMD,ACK,MAINTENANCE_MODE");
  }
  else if (cmd == "SYSTEM_RESTART") {
    // Reset toàn b? h? th?ng
    initCar(A, 0, 'A');
    initCar(B, 0, 'B');
    unpickCabin('A'); unpickCabin('B');
    Serial.println("CMD,ACK,SYSTEM_RESTART");
    Serial.println("SYSTEM_READY");
  }
  else if (cmd == "SYSTEM_SHUTDOWN") {
    A.active = false; B.active = false;
    Serial.println("CMD,ACK,SYSTEM_SHUTDOWN");
  }
  else {
    Serial.print("CMD,ERROR,UNKNOWN_COMMAND,"); Serial.println(cmd);
  }
}

// ===================== Kh?i t?o Car & IO =====================
static void initCar(Car &c, uint8_t startFloor, char name) {
  c.floor = startFloor; c.dir = ST; c.st = IDLE; c.t0 = millis();
  c.name = name; c.active = true;
  for (uint8_t i=0;i<NUM_FLOORS;++i){ c.reqUpMask[i]=false; c.reqDnMask[i]=false; }
}

void setup() {
  Serial.begin(9600);

  hallDB.begin(hallPins, 10);
  cabADB.begin(cabA, NUM_FLOORS);
  cabBDB.begin(cabB, NUM_FLOORS);
  closedDB.begin(closedPins, 2);
  openDB.begin(openPins, 2);

  for (uint8_t i=0;i<10;i++) hallDB_prev[i] = hallDB.read(i);
  for (uint8_t i=0;i<NUM_FLOORS;i++) {
    cabADB_prev[i] = cabADB.read(i);
    cabBDB_prev[i] = cabBDB.read(i);
    pinMode(led_cabA[i], OUTPUT); digitalWrite(led_cabA[i], LOW);
    pinMode(led_cabB[i], OUTPUT); digitalWrite(led_cabB[i], LOW);
  }
  for (uint8_t i=0;i<2;i++) closedDB_prev[i] = closedDB.read(i);
  for (uint8_t i=0;i<2;i++) openDB_prev[i] = openDB.read(i);

  pinMode(led_choiceA, OUTPUT); digitalWrite(led_choiceA, LOW);
  pinMode(led_choiceB, OUTPUT); digitalWrite(led_choiceB, LOW);

  lcdA.begin(20,4); lcdB.begin(20,4);
  displayLCD(lcdA, 0, "       Cabin A");
  displayLCD(lcdB, 0, "       Cabin B");

  initCar(A, 0, 'A');
  initCar(B, 3, 'B');

  // G?i snapshot ??u
  Tele::CsvSnap s; Tele::makeSnap(s, A, B, hallDB, cabADB, cabBDB, openDB, closedDB);
  Tele::printSnap(s, millis());
  lastA={A.floor,(int8_t)A.dir,(uint8_t)A.st};
  lastB={B.floor,(int8_t)B.dir,(uint8_t)B.st};
  
  Serial.println("SYSTEM_READY");
}

// ===================== Loop =====================
void loop() {
  uint32_t now = millis();
  static uint32_t lastSnapSend = 0;
  static bool haveLastSnap = false;
  static Tele::CsvSnap lastSnap{};

  if (now - lastTick < POLL_MS) return;
  lastTick = now;

  // 0) X? lý l?nh t? PC
  while (Serial.available()) {
    char c = Serial.read();
    if (c == '\n' || c == '\r') {
      if (inputBuffer.length() > 0) {
        processCommand(inputBuffer);
        inputBuffer = "";
      }
    } else {
      inputBuffer += c;
    }
  }

  // 1) Debounce
  hallDB.update(now); cabADB.update(now); cabBDB.update(now); closedDB.update(now); openDB.update(now);

  // 2) Hall: phát c?nh NH?N/TH? + phân b? cabin
  for (uint8_t i = 0; i < 10; i++) {
    bool curr = hallDB.read(i);
    if (!hallDB_prev[i] && curr) {
      uint8_t f = hallFloor[i]; Dir d = (hallDirArr[i] > 0) ? UP : DN; HallCall h{f, d};
      Car &c = pick(A, B, h);
      pickCabin(c.name);
      if (c.floor == f) { c.floor = f; clearHere(c); c.st = DOOR_OPEN; c.t0 = now; showStateLine(c, "  DOOR OPENED      "); }
      else { addReq(c, f, d); }
      sendBtnEventCSV('H', i, true, now);
    } else if (hallDB_prev[i] && !curr) {
      sendBtnEventCSV('H', i, false, now);
    }
    hallDB_prev[i] = curr;
  }

  // 3) Cab A
  for (uint8_t f=0; f<NUM_FLOORS; ++f) {
    bool curr = cabADB.read(f);
    if (!cabADB_prev[f] && curr) {
      Dir d = dirTo(A.floor, f); if (d != ST) addReq(A, f, d);
      sendBtnEventCSV('A', f, true, now);
    } else if (cabADB_prev[f] && !curr) {
      sendBtnEventCSV('A', f, false, now);
    }
    cabADB_prev[f] = curr;
  }

  // 4) Cab B
  for (uint8_t f=0; f<NUM_FLOORS; ++f) {
    bool curr = cabBDB.read(f);
    if (!cabBDB_prev[f] && curr) {
      Dir d = dirTo(B.floor, f); if (d != ST) addReq(B, f, d);
      sendBtnEventCSV('B', f, true, now);
    } else if (cabBDB_prev[f] && !curr) {
      sendBtnEventCSV('B', f, false, now);
    }
    cabBDB_prev[f] = curr;
  }

  // 5) CLOSE buttons
  {
    bool curr = closedDB.read(0);
    if (!closedDB_prev[0] && curr) {
      if (A.st == DOOR_OPEN || A.st == WAIT || A.st == WAIT_PICK) { A.st = DOOR_CLOSE; A.t0 = now; showStateLine(A,"  DOOR CLOSED      "); }
      sendBtnEventCSV('C', 0, true, now);
    } else if (closedDB_prev[0] && !curr) {
      sendBtnEventCSV('C', 0, false, now);
    }
    closedDB_prev[0] = curr;
  }
  {
    bool curr = closedDB.read(1);
    if (!closedDB_prev[1] && curr) {
      if (B.st == DOOR_OPEN || B.st == WAIT || B.st == WAIT_PICK) { B.st = DOOR_CLOSE; B.t0 = now; showStateLine(B,"  DOOR CLOSED      "); }
      sendBtnEventCSV('C', 1, true, now);
    } else if (closedDB_prev[1] && !curr) {
      sendBtnEventCSV('C', 1, false, now);
    }
    closedDB_prev[1] = curr;
  }

  // 6) OPEN buttons
  {
    bool curr = openDB.read(0);
    if (!openDB_prev[0] && curr) {
      if      (A.st == DOOR_OPEN) { A.t0 = now; showStateLine(A, "  DOOR OPENED      "); }
      else if (A.st != MOVE)      { A.st = DOOR_OPEN; A.t0 = now; showStateLine(A, "  DOOR OPENED      "); }
      sendBtnEventCSV('O', 0, true, now);
    } else if (openDB_prev[0] && !curr) {
      sendBtnEventCSV('O', 0, false, now);
    }
    openDB_prev[0] = curr;
  }
  {
    bool curr = openDB.read(1);
    if (!openDB_prev[1] && curr) {
      if      (B.st == DOOR_OPEN) { B.t0 = now; showStateLine(B, "  DOOR OPENED      "); }
      else if (B.st != MOVE)      { B.st = DOOR_OPEN; B.t0 = now; showStateLine(B, "  DOOR OPENED      "); }
      sendBtnEventCSV('O', 1, true, now);
    } else if (openDB_prev[1] && !curr) {
      sendBtnEventCSV('O', 1, false, now);
    }
    openDB_prev[1] = curr;
  }

  // 7) LED theo hàng ??i
  renderLed(A); renderLed(B);

  // 8) state machine 2 cabin
  loopCar(A, now);
  loopCar(B, now);

  // 9) phát event state khi ??i
  publishIfChanged(now);

  // 10) C?p nh?t LCD ~200ms
  static uint32_t lastUi = 0;
  if (now - lastUi >= 200) {
    lastUi = now;
    displayCarStatus(A, lcdA, now);
    displayCarStatus(B, lcdB, now);
  }

  // 11) CSV snapshot khi ??i ho?c heartbeat
  Tele::CsvSnap cur; Tele::makeSnap(cur, A, B, hallDB, cabADB, cabBDB, openDB, closedDB);
  bool changed   = (!haveLastSnap) || memcmp(&cur, &lastSnap, sizeof(cur)) != 0;
  bool heartbeat = (now - lastSnapSend >= SNAPSHOT_HEARTBEAT_MS);
  if (changed || heartbeat) {
    Tele::printSnap(cur, now);
    lastSnap = cur; haveLastSnap = true; lastSnapSend = now;
  }
}
