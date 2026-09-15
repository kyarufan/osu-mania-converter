// OsuMania7kTo10k.cs
// 빌드: dotnet build (net10.0-windows)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OsuMemoryDataProvider;   // NuGet: OsuMemoryDataProvider (+ ProcessMemoryDataFinder)

public class MainForm : Form
{
    private Button btnOpen, btnConvert;
    private Label lblStatus, lblFile, lblUsage;a
    private RadioButton rbKey8, rbKey10;                 // 타깃 키 (그룹1)
    private RadioButton rbDplain, rbDdense, rbDmore;     // 밀도 (그룹2)
    private RadioButton rbLayOld, rbLayNew;              // 8→10 배치 타입 (그룹3)
    private string loadedFilePath = null;

    public MainForm()
    {
        Text = "Converter";
        Size = new Size(700, 300);
        MinimumSize = new Size(460, 260);
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Color.FromArgb(30, 30, 34);
        ForeColor = Color.FromArgb(220, 220, 230);

        var panelTop = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(16, 14, 16, 0) };
        panelTop.BackColor = Color.FromArgb(38, 38, 44);

        lblFile = new Label
        {
            Text = "파일을 선택하세요",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = Color.FromArgb(160, 160, 175),
            Font = new Font("Segoe UI", 9f)
        };

        var panelBtns = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        btnOpen = MakeButton("📂  파일 열기", Color.FromArgb(60, 130, 210));
        btnConvert = MakeButton("▶  변환 & 저장", Color.FromArgb(50, 170, 110));
        btnConvert.Enabled = false;

        panelBtns.Controls.Add(btnOpen);
        panelBtns.Controls.Add(btnConvert);
        panelTop.Controls.Add(lblFile);
        panelTop.Controls.Add(panelBtns);

        lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = Color.FromArgb(24, 24, 28),
            ForeColor = Color.FromArgb(140, 200, 140),
            Text = "준비"
        };

        lblUsage = new Label
        {
            Text = "Ctrl+D 로 인게임에서 변환할 수 있습니다",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = Color.FromArgb(30, 30, 34),
            ForeColor = Color.FromArgb(150, 165, 200),
            Font = new Font("Segoe UI", 9f)
        };

        // 변환 모드 3택 (라디오) — 패널이 그룹 컨테이너라 하나만 선택됨
        var panelMode = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(12, 8, 0, 0),
            BackColor = Color.FromArgb(30, 30, 34)
        };
        // 두 독립 그룹: 각 그룹은 별도 컨테이너라 서로 영향 없이 각각 하나씩만 선택됨.
        var grpKey = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                                           WrapContents = false, Margin = new Padding(0, 0, 8, 0), BackColor = Color.Transparent };
        var grpDensity = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                                               WrapContents = false, Margin = new Padding(0, 0, 8, 0), BackColor = Color.Transparent };
        var grpLayout = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                                              WrapContents = false, Margin = new Padding(0), BackColor = Color.Transparent };
        rbKey8   = MakeRadio("8K");
        rbKey10  = MakeRadio("10K");
        rbKey10.Checked = true;                 // 기본 타깃 10K
        grpKey.Controls.Add(rbKey8);
        grpKey.Controls.Add(rbKey10);

        rbDplain = MakeRadio("기본");
        rbDdense = MakeRadio("Dense");
        rbDmore  = MakeRadio("More Dense");
        rbDdense.Checked = true;                // 기본 밀도 Dense
        grpDensity.Controls.Add(rbDplain);
        grpDensity.Controls.Add(rbDdense);
        grpDensity.Controls.Add(rbDmore);

        // 8→10 배치 타입: Old = 기존 0123/1234 두 가지만, New = 빈칸 위치 0~4 다섯 가지
        rbLayOld = MakeRadio("Old 배치");
        rbLayNew = MakeRadio("New 배치");
        rbLayNew.Checked = true;                // 기본 New
        var tip = new ToolTip();
        tip.SetToolTip(rbLayOld, "8→10 손별 배치를 기존 0123 / 1234 두 가지로만 (예전 결과 그대로)");
        tip.SetToolTip(rbLayNew, "빈칸이 0~4 어디든 올 수 있는 5가지 배치 — 겹침·잭으로 기존 두 배치가 막힐 때 사용");
        grpLayout.Controls.Add(rbLayOld);
        grpLayout.Controls.Add(rbLayNew);

        Label Sep() => new Label { Text = "|", AutoSize = true, ForeColor = Color.FromArgb(90, 90, 100),
                                   Margin = new Padding(0, 3, 8, 0), Font = new Font("Segoe UI", 10f) };
        panelMode.Controls.Add(grpKey);
        panelMode.Controls.Add(Sep());
        panelMode.Controls.Add(grpDensity);
        panelMode.Controls.Add(Sep());
        panelMode.Controls.Add(grpLayout);

        Controls.Add(panelMode);
        Controls.Add(lblUsage);
        Controls.Add(lblStatus);
        Controls.Add(panelTop);

        btnOpen.Click += BtnOpen_Click;
        btnConvert.Click += BtnConvert_Click;

        AllowDrop = true;
        DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
        DragDrop += (s, e) =>
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) LoadFile(files[0]);
        };
    }

    private Button MakeButton(string text, Color bg)
    {
        var btn = new Button
        {
            Text = text,
            Height = 36,
            AutoSize = true,
            Padding = new Padding(12, 0, 12, 0),
            Margin = new Padding(0, 0, 10, 0),
            BackColor = bg,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private RadioButton MakeRadio(string text)
    {
        return new RadioButton
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 0, 18, 0),
            ForeColor = Color.FromArgb(210, 210, 225),
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9.5f),
            Cursor = Cursors.Hand
        };
    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog { Filter = "osu! beatmap (*.osu)|*.osu", Title = "7K .osu 파일 선택" };
        if (dlg.ShowDialog() == DialogResult.OK) LoadFile(dlg.FileName);
    }

    private void LoadFile(string path)
    {
        if (!path.EndsWith(".osu", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("❌ .osu 파일만 지원합니다.", false);
            return;
        }
        loadedFilePath = path;
        lblFile.Text = Path.GetFileName(path);
        btnConvert.Enabled = true;
        SetStatus("파일 로드 완료 — 변환 버튼을 누르세요.", true);
    }

    private void BtnConvert_Click(object sender, EventArgs e)
    {
        if (loadedFilePath == null) return;
        ConvertFileAt(loadedFilePath);
    }

    // 파일 경로 하나를 변환·저장 (버튼/단축키 공용). 성공하면 true
    private bool ConvertFileAt(string path)
    {
        try
        {
            if (!File.Exists(path)) { SetStatus("❌ 파일을 찾을 수 없습니다: " + path, false); return false; }
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            double bpm = GetBpm(lines);
            List<string> converted;
            bool tgt10 = rbKey10.Checked;                       // 타깃: 10K(true) / 8K(false)
            bool dense = rbDdense.Checked, more = rbDmore.Checked;  // 밀도: Dense / More / (둘 다 false=기본)
            if (IsFourKey(lines))
            {
                if (tgt10)   // 4 → 10 (손폭5, 빈칸3 → More는 2개 복제)
                {
                    if (dense)     converted = Convert4To10(lines, bpm, true, 1.01, 1);
                    else if (more) converted = Convert4To10(lines, bpm, true, 0.375, 2);
                    else           converted = Convert4To10(lines, bpm, false);
                }
                else         // 4 → 8 (손폭4, 빈칸2 → More는 2개 복제)
                {
                    if (dense)     converted = Convert4To8(lines, bpm, true, 1.01, 1);
                    else if (more) converted = Convert4To8(lines, bpm, true, 0.375, 2);
                    else           converted = Convert4To8(lines, bpm, false);
                }
            }
            else if (IsSevenKey(lines))
            {
                converted = ConvertLines(lines, bpm);   // 7 → 8
                if (tgt10)
                {
                    bool wide = rbLayNew.Checked;   // New 배치: 빈칸 0~4 다섯 가지 / Old: 기존 두 가지만
                    if (dense)     converted = Convert8To10(converted, bpm, true, 1.01, wide);
                    else if (more) converted = Convert8To10(converted, bpm, true, 0.375, wide);
                    else           converted = Convert8To10(converted, bpm, false, 1.01, wide);
                }
                // 8K 타깃: 7→8 그대로(밀도 무시 — 7→8은 채움 없음)
            }
            else { SetStatus("❌ 4K/7K 맵이 아닙니다.", false); return false; }

            string dir = Path.GetDirectoryName(path);
            // osu! 표준 파일명 규칙으로 저장해야 osu!가 새 난이도로 즉시 핫임포트함
            string outPath = Path.Combine(dir, BuildOsuFileName(converted, path));
            File.WriteAllLines(outPath, converted, Encoding.UTF8);

            SetStatus($"✅ 저장 완료: {Path.GetFileName(outPath)}", true);
            return true;
        }
        catch (Exception ex) { SetStatus($"❌ 오류: {ex.Message}", false); return false; }
    }

    // 변환 결과 메타데이터로 osu! 표준 파일명 "Artist - Title (Creator) [Version].osu" 생성
    private static string BuildOsuFileName(List<string> lines, string fallbackPath)
    {
        string Meta(string key)
        {
            var l = lines.FirstOrDefault(x => x.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase));
            return l == null ? "" : l.Substring(key.Length + 1).Trim();
        }
        string San(string s) => string.Concat((s ?? "").Split(Path.GetInvalidFileNameChars()));

        string artist = San(Meta("Artist"));
        string title = San(Meta("Title"));
        string creator = San(Meta("Creator"));
        string version = San(Meta("Version"));

        if (artist == "" || title == "")   // 메타데이터 못 읽으면 안전한 대체 이름
            return Path.GetFileNameWithoutExtension(fallbackPath) + "_8k.osu";

        string name = $"{artist} - {title} ({creator}) [{version}]";
        if (name.Length > 240) name = name.Substring(0, 240);   // 경로 길이 안전장치
        return name + ".osu";
    }

    // ===== 전역 단축키 (osu!가 포커스를 잡고 있어도 동작) =====
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);

    // ── SendInput (스캔코드 기반) ── osu!는 raw input/스캔코드로 키를 읽으므로
    //    keybd_event(bScan=0)로는 인식 못 함. SendInput + KEYEVENTF_SCANCODE 사용.
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT { public uint uMsg; public ushort wParamL; public ushort wParamH; }

    private const uint INPUT_KEYBOARD     = 1;
    private const uint KEYEVENTF_KEYUP     = 0x0002;
    private const uint KEYEVENTF_SCANCODE  = 0x0008;
    private const ushort SCAN_F5 = 0x3F;   // F5 스캔코드(Set 1 make code)
    private const int HOTKEY_ID = 0x4B52;                 // 임의 ID
    private const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_NOREPEAT = 0x4000;
    private const uint VK_D = 0x44;                        // 기본: Ctrl+D

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_NOREPEAT, VK_D))
            SetStatus("⚠ 단축키(Ctrl+D) 등록 실패 — 다른 프로그램이 점유 중일 수 있어요.", false);
        else
            SetStatus("준비됨 — osu!에서 맵 선택/재생 중 Ctrl+D 로 변환", true);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        UnregisterHotKey(Handle, HOTKEY_ID);
        base.OnHandleDestroyed(e);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY = 0x0312;
        if (m.Msg == WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
            ConvertCurrentBeatmap();
        base.WndProc(ref m);
    }

    // ===== osu! 메모리에서 현재 비트맵 경로 읽어 변환 =====
#pragma warning disable CS0618   // 구버전 리더지만 폴더/파일명 읽기엔 충분
    private IOsuMemoryReader _osuReader;
#pragma warning restore CS0618
    private string _songsPath;

    private void ConvertCurrentBeatmap()
    {
        try
        {
#pragma warning disable CS0618
            _osuReader ??= OsuMemoryReader.Instance;
#pragma warning restore CS0618
            EnsureSongsPath();
            if (string.IsNullOrEmpty(_songsPath))
            { SetStatus("❌ osu! 프로세스를 찾을 수 없습니다. (osu! 실행 확인)", false); return; }

            string folder = _osuReader.GetMapFolderName();
            string file = _osuReader.GetOsuFileName();
            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(file))
            { SetStatus("❌ 현재 맵을 읽지 못했습니다. (곡 선택/재생 중인지 확인)", false); return; }

            string full = Path.Combine(_songsPath, folder, file);
            loadedFilePath = full;
            lblFile.Text = Path.GetFileName(full);
            btnConvert.Enabled = true;
            if (ConvertFileAt(full))
                SendF5ToOsu();   // osu!가 새 난이도를 바로 읽도록 F5(소프트 갱신) 전송
        }
        catch (Exception ex) { SetStatus($"❌ 단축키 변환 오류: {ex.Message}", false); }
    }

    // osu! 창을 앞으로 가져온 뒤 F5 키 입력 전송 (소프트 갱신 — 전체 재스캔 아님)
    private void SendF5ToOsu()
    {
        try
        {
            var procs = Process.GetProcessesByName("osu!");
            if (procs.Length > 0 && procs[0].MainWindowHandle != IntPtr.Zero)
                SetForegroundWindow(procs[0].MainWindowHandle);   // 보통 인게임이라 이미 포커스 상태
            System.Threading.Thread.Sleep(80);                    // 포커스 전환 안정화

            // Ctrl+D 핫키 직후라 Ctrl이 아직 눌려 있으면 osu!가 Ctrl+F5로 받음 → 먼저 Ctrl up 보장
            SendScan(0x1D, up: true);   // Left Ctrl up (혹시 눌려있다면 해제)

            // F5 down → up (스캔코드 기반)
            SendScan(SCAN_F5, up: false);
            System.Threading.Thread.Sleep(15);
            SendScan(SCAN_F5, up: true);
        }
        catch { /* 전송 실패는 무시 */ }
    }

    private static void SendScan(ushort scan, bool up)
    {
        uint flags = KEYEVENTF_SCANCODE | (up ? KEYEVENTF_KEYUP : 0);
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].U.ki = new KEYBDINPUT { wVk = 0, wScan = scan, dwFlags = flags, time = 0, dwExtraInfo = IntPtr.Zero };
        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    // osu! 프로세스 exe 위치에서 Songs 폴더 자동 획득 (커스텀 Songs 경로면 '파일 열기'로 대체 가능)
    private void EnsureSongsPath()
    {
        if (!string.IsNullOrEmpty(_songsPath) && Directory.Exists(_songsPath)) return;
        var procs = Process.GetProcessesByName("osu!");
        if (procs.Length == 0) { _songsPath = null; return; }
        try
        {
            string exe = procs[0].MainModule?.FileName;
            string dir = exe != null ? Path.GetDirectoryName(exe) : null;
            if (dir != null && !dir.ToUpperInvariant().Contains("SYSTEM32"))
            {
                string songs = Path.Combine(dir, "Songs");
                if (Directory.Exists(songs)) _songsPath = songs;
            }
        }
        catch { /* MainModule 접근 실패 시 무시 */ }
    }

    private void SetStatus(string msg, bool ok)
    {
        lblStatus.ForeColor = ok ? Color.FromArgb(100, 210, 140) : Color.FromArgb(220, 100, 100);
        lblStatus.Text = "  " + msg;
    }

    // ── 변환 핵심 로직 ──────────────────────────────────────────

    private bool IsSevenKey(string[] lines)
    {
        foreach (var line in lines)
            if (line.StartsWith("CircleSize:", StringComparison.OrdinalIgnoreCase))
                if (double.TryParse(line.Split(':')[1].Trim(), out double cs))
                    return Math.Abs(cs - 7) < 0.01;
        return false;
    }

    private bool IsFourKey(string[] lines)
    {
        foreach (var line in lines)
            if (line.StartsWith("CircleSize:", StringComparison.OrdinalIgnoreCase))
                if (double.TryParse(line.Split(':')[1].Trim(), out double cs))
                    return Math.Abs(cs - 4) < 0.01;
        return false;
    }

    // 4K → 10K 직행. 구조는 Convert8To10과 동일(손별 독립, 박 단위, 매 박 flip 시도,
    //   Safe/JackFree/룩어헤드/LN 락). 차이는 후보뿐: 오프셋 2택 → 페어 10택.
    //   손별 5칸 중 페어 (lo,hi) 두 칸만 실노트. 왼손 base=0, 오른손 base=5.
    //   소스 로컬 loc(0/1) → base + PAIRS[s][loc].  "other" = 순환 다음 페어.
    //   ≤16비트 동컬럼(진짜 잭)은 한 박 그룹 안이라 같은 페어로 자동 보존. 새 잭·겹침 0.
    //   fill=true면 손별 빈칸(3칸)에 트리거 복제(최대 fillMax개), Conflicts로 검증.
    // 얇은 래퍼: 4→10(손폭5) / 4→8(손폭4). 본체는 Convert4ToN 하나로 공유.
    private List<string> Convert4To10(string[] lines, double bpm, bool fill = false, double fillGapMult = 1.01, int fillMax = 1)
        => Convert4ToN(lines, bpm, 5, fill, fillGapMult, fillMax);
    private List<string> Convert4To8(string[] lines, double bpm, bool fill = false, double fillGapMult = 1.01, int fillMax = 1)
        => Convert4ToN(lines, bpm, 4, fill, fillGapMult, fillMax);

    private List<string> Convert4ToN(string[] lines, double bpm, int handW, bool fill = false, double fillGapMult = 1.01, int fillMax = 1)
    {
        int totalKeys = handW * 2;   // 8 또는 10
        // 페어 후보(순환 순서: 거리1→거리 W-1). W=5→10종, W=4→6종. "other" = 다음 인덱스.
        var PAIRS = new List<(int lo, int hi)>();
        for (int d = 1; d < handW; d++) for (int lo0 = 0; lo0 + d < handW; lo0++) PAIRS.Add((lo0, lo0 + d));
        int NP = PAIRS.Count;

        // 타이밍포인트 (Convert8To10과 동일)
        var tps = new List<(double off, double bm)>();
        {
            bool inT = false;
            foreach (var line in lines)
            {
                if (line.TrimStart().Equals("[TimingPoints]", StringComparison.OrdinalIgnoreCase)) { inT = true; continue; }
                if (inT && line.StartsWith("[")) break;
                if (inT && line.Contains(","))
                {
                    var p = line.Split(',');
                    if (p.Length >= 7 && p[6].Trim() == "1" &&
                        double.TryParse(p[1].Trim(), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double bm) && bm > 0 &&
                        double.TryParse(p[0].Trim(), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double off))
                        tps.Add((off, bm));
                }
            }
            tps.Sort((a, b) => a.off.CompareTo(b.off));
            if (tps.Count == 0) tps.Add((0.0, 60000.0 / Math.Max(1e-6, bpm)));
        }
        int TpIndex(int time)
        {
            int idx = 0;
            for (int k = 0; k < tps.Count; k++) { if (time >= tps[k].off - 1e-6) idx = k; else break; }
            return idx;
        }
        double BeatMsAt(int time) => tps[TpIndex(time)].bm;
        double JackGapAt(int time) => BeatMsAt(time) * 0.375;   // 16비트 잭 임계
        int firstNoteTime = 0;
        (int tp, long b) BeatOf(int time)
        {
            int idx = TpIndex(time);
            double origin = (idx == 0) ? firstNoteTime : tps[idx].off;
            long lb = (long)Math.Floor((time - origin) / Math.Max(1e-6, tps[idx].bm) + 1e-6);
            return (idx, lb);
        }

        // 1) 헤더 통과 + HitObject 수집 (메타는 ConvertLines 규칙)
        var result = new List<string>();
        var objLines = new List<string>();
        bool inHO = false;
        string originalCreator = null;
        foreach (var line in lines)
        {
            if (!inHO && line.StartsWith("CircleSize:", StringComparison.OrdinalIgnoreCase))
            { result.Add("CircleSize:" + totalKeys); continue; }
            if (!inHO && line.StartsWith("Creator:", StringComparison.OrdinalIgnoreCase))
            { originalCreator = line.Substring("Creator:".Length).Trim(); result.Add("Creator:나래"); continue; }
            if (!inHO && line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
            {
                string v = line.Substring("Version:".Length).Trim();
                string suffix = " " + totalKeys + "K";
                if (fill) suffix += (fillGapMult < 1.0) ? " More Dense" : " Dense";
                result.Add("Version:" + v + suffix); continue;
            }
            if (!inHO && line.StartsWith("BeatmapID:", StringComparison.OrdinalIgnoreCase))
            { result.Add("BeatmapID:0"); continue; }
            if (line.TrimStart().Equals("[HitObjects]", StringComparison.OrdinalIgnoreCase))
            { inHO = true; result.Add(line); continue; }
            if (inHO) { objLines.Add(line); continue; }
            result.Add(line);
        }

        // 2) 오브젝트 파싱 (col4, 시작/끝, LN 여부)
        var objs = new List<(int time, int col4, int endTime, bool isLN, string[] parts)>();
        foreach (var line in objLines)
        {
            if (!line.Contains(",")) continue;
            var parts = line.Split(',');
            if (parts.Length < 4 ||
                !int.TryParse(parts[0].Trim(), out int x) ||
                !int.TryParse(parts[2].Trim(), out int t) ||
                !int.TryParse(parts[3].Trim(), out int type))
                continue;
            int col4 = Math.Max(0, Math.Min(3, (int)Math.Floor(x * 4.0 / 512)));
            bool isLN = (type & 128) != 0;
            int endTime = t;
            if (isLN && parts.Length >= 6)
            {
                var endField = parts[5].Split(':')[0].Trim();
                if (int.TryParse(endField, out int e)) endTime = e;
            }
            objs.Add((t, col4, endTime, isLN, parts));
        }
        firstNoteTime = objs.Count > 0 ? objs.Min(o => o.time) : (int)tps[0].off;

        // 3) 손별 박 단위 처리 (Convert8To10과 동일 골격, shift→페어인덱스)
        const int LockMinMs = 100;
        var placedAll = new List<(int c10, int start, int end)>();
        var addCands  = new List<(int c10, int start, int end, string[] parts)>();

        for (int hand = 0; hand < 2; hand++)
        {
            int baseC = hand * handW;   // 왼손 0.., 오른손 handW..
            var handObjs = objs.Where(o => (o.col4 <= 1 ? 0 : 1) == hand)
                               .OrderBy(o => o.time).ThenBy(o => o.col4).ToList();
            int Loc(int k) => handObjs[k].col4 - hand * 2;               // 소스 로컬 0/1
            int Col10(int k, int s) => baseC + (Loc(k) == 0 ? PAIRS[s].lo : PAIRS[s].hi);  // 페어 s로 둘 때 c10

            var placedLN = new List<(int c10, int src4, int start, int end)>();
            int lnPi = 0, prevPi = 0;
            var lastRel = new Dictionary<int, (int end, int c4)>();

            int i = 0;
            while (i < handObjs.Count)
            {
                var beat = BeatOf(handObjs[i].time);
                int j = i;
                while (j < handObjs.Count && BeatOf(handObjs[j].time) == beat) j++;
                int gStart = handObjs[i].time;

                placedLN.RemoveAll(L => L.end < gStart);
                bool hasActive = false;
                foreach (var L in placedLN) if (L.end > gStart && (L.end - L.start) >= LockMinMs) { hasActive = true; break; }
                int other = (prevPi + 1) % NP;   // 순환 다음 페어(8→10의 flip 일반화)

                bool SafeRange(int bi, int bj, int s, List<(int c10, int src4, int start, int end)> pLN)
                {
                    for (int k = bi; k < bj; k++)
                    {
                        int c10 = Col10(k, s);
                        int t = handObjs[k].time;
                        int te = handObjs[k].isLN ? handObjs[k].endTime : handObjs[k].time;
                        foreach (var L in pLN)
                        {
                            if (L.c10 != c10) continue;
                            if (t < L.end && te > L.start) return false;
                            if (t == L.end && handObjs[k].col4 != L.src4) return false;
                        }
                    }
                    return true;
                }
                bool JackFreeRange(int bi, int bj, int s, Dictionary<int, (int end, int c4)> lr)
                {
                    for (int k = bi; k < bj; k++)
                    {
                        int c10 = Col10(k, s);
                        if (lr.TryGetValue(c10, out var pr))
                        {
                            int delta = handObjs[k].time - pr.end;
                            if (pr.c4 != handObjs[k].col4 && delta > 0 && delta <= JackGapAt(handObjs[k].time))
                                return false;
                        }
                    }
                    return true;
                }
                bool Safe(int s)     => SafeRange(i, j, s, placedLN);
                bool JackFree(int s) => JackFreeRange(i, j, s, lastRel);

                Dictionary<int, (int end, int c4)> SimLastRel(int s)
                {
                    var lr = new Dictionary<int, (int end, int c4)>(lastRel);
                    for (int k = i; k < j; k++)
                    {
                        int c10 = Col10(k, s);
                        int relEnd = handObjs[k].isLN ? handObjs[k].endTime : handObjs[k].time;
                        if (!lr.TryGetValue(c10, out var ex) || relEnd >= ex.end) lr[c10] = (relEnd, handObjs[k].col4);
                    }
                    return lr;
                }
                List<(int c10, int src4, int start, int end)> SimPlacedLN(int s)
                {
                    var p = new List<(int c10, int src4, int start, int end)>(placedLN);
                    for (int k = i; k < j; k++)
                        if (handObjs[k].isLN) p.Add((Col10(k, s), handObjs[k].col4, handObjs[k].time, handObjs[k].endTime));
                    return p;
                }

                bool startsLong = false;
                for (int k = i; k < j; k++)
                    if (handObjs[k].isLN && (handObjs[k].endTime - handObjs[k].time) >= LockMinMs) { startsLong = true; break; }

                int NextJackPenalty(int s)
                {
                    if (j >= handObjs.Count) return 0;
                    var nb = BeatOf(handObjs[j].time);
                    int nj = j;
                    while (nj < handObjs.Count && BeatOf(handObjs[nj].time) == nb) nj++;
                    int ngStart = handObjs[j].time;
                    var simLN = SimPlacedLN(s);
                    bool nActive = simLN.Any(L => L.end > ngStart && (L.end - L.start) >= LockMinMs);
                    if (!nActive) return 0;
                    return JackFreeRange(j, nj, s, SimLastRel(s)) ? 0 : 1;
                }

                int pi;
                if (hasActive)
                {
                    // LN 잡힌 동안은 lnPi 고정(잡힌 LN은 못 옮김). 단 lnPi가 잭이고 대안이 겹침안전+무잭이면 대안.
                    int altLn = (lnPi + 1) % NP;
                    if      (Safe(lnPi) && JackFree(lnPi)) pi = lnPi;
                    else if (Safe(altLn) && JackFree(altLn)) pi = altLn;
                    else                                    pi = lnPi;
                }
                else
                {
                    bool oClean = Safe(other) && JackFree(other);
                    bool pClean = Safe(prevPi) && JackFree(prevPi);
                    if (startsLong && oClean && pClean && other != prevPi)
                        pi = (NextJackPenalty(other) <= NextJackPenalty(prevPi)) ? other : prevPi;
                    else if (oClean)      pi = other;
                    else if (pClean)      pi = prevPi;
                    else if (Safe(other)) pi = other;
                    else                  pi = prevPi;
                }

                bool startedLongLN = false;
                int lo = PAIRS[pi].lo, hi = PAIRS[pi].hi;
                // 채움 빈칸: 이 손 5칸 중 페어에 안 든 3칸
                var gaps = new HashSet<int>();
                for (int x = 0; x < handW; x++) if (x != lo && x != hi) gaps.Add(baseC + x);
                var beatPlaced = new List<(string[] parts, int c10, int time, int relEnd, bool isLN, int col4)>();

                for (int k = i; k < j; k++)
                {
                    var o = handObjs[k];
                    int c10 = Col10(k, pi);
                    o.parts[0] = ((int)((c10 + 0.5) * 512 / totalKeys)).ToString();
                    int relEnd = o.isLN ? o.endTime : o.time;
                    if (fill) placedAll.Add((c10, o.time, relEnd));
                    if (!lastRel.TryGetValue(c10, out var ex) || relEnd >= ex.end) lastRel[c10] = (relEnd, o.col4);
                    if (o.isLN) { placedLN.Add((c10, o.col4, o.time, o.endTime));
                                  if (o.endTime - o.time >= LockMinMs) startedLongLN = true; }
                    beatPlaced.Add((o.parts, c10, o.time, relEnd, o.isLN, o.col4));
                }

                // 채움(선택적, 8→10식): 실노트마다 인접 빈칸 1개(c10±1)에만 복제, 박당 최대 fillMax개.
                //   무차별 복제 금지 → LN 겹침·꼬리 스택 방지. 최종 확정은 아래 Conflicts가 검증.
                if (fill)
                {
                    var used = new HashSet<int>();
                    int made = 0;
                    foreach (var bp in beatPlaced)
                    {
                        if (made >= fillMax) break;
                        foreach (int cand in new[] { bp.c10 - 1, bp.c10 + 1 })
                        {
                            if (cand >= baseC && cand < baseC + 5 && gaps.Contains(cand) && !used.Contains(cand))
                            {
                                var tp = (string[])bp.parts.Clone();
                                tp[0] = ((int)((cand + 0.5) * 512 / totalKeys)).ToString();
                                addCands.Add((cand, bp.time, bp.relEnd, tp));
                                used.Add(cand); made++;
                                break;
                            }
                        }
                    }
                }
                if (!hasActive && startedLongLN) lnPi = pi;

                prevPi = pi;
                i = j;
            }
        }

        // 3.5) 채움 검증 (Convert8To10과 동일 Conflicts)
        if (fill && addCands.Count > 0)
        {
            var occ = new Dictionary<int, List<(int s, int e)>>();
            foreach (var p in placedAll)
            {
                if (!occ.TryGetValue(p.c10, out var lst)) { lst = new List<(int, int)>(); occ[p.c10] = lst; }
                lst.Add((p.start, p.end));
            }
            bool Conflicts(int c10, int start, int end)
            {
                if (!occ.TryGetValue(c10, out var lst)) return false;
                foreach (var n in lst)
                {
                    if (start == n.s) return true;                 // 같은 시각 스택
                    if (start < n.e && n.s < end) return true;     // 내부 겹침
                    if (start == n.e || end == n.s) return true;   // 맞닿음(LN 꼬리/머리 스택)도 거부 — 채움은 체인 예외 없음
                    int gap = Math.Abs(n.s - start);
                    if (gap > 0 && gap <= BeatMsAt(start) * fillGapMult) return true;  // 근접 재타
                }
                return false;
            }
            foreach (var cand in addCands.OrderBy(a => a.start).ThenBy(a => a.c10))
            {
                if (Conflicts(cand.c10, cand.start, cand.end)) continue;
                objs.Add((cand.start, cand.c10, cand.end, cand.end > cand.start, cand.parts));
                if (!occ.TryGetValue(cand.c10, out var lst)) { lst = new List<(int, int)>(); occ[cand.c10] = lst; }
                lst.Add((cand.start, cand.end));
            }
        }

        // 4) 시간순 출력
        objs.Sort((a, b) => a.time != b.time ? a.time.CompareTo(b.time) : a.col4.CompareTo(b.col4));
        foreach (var o in objs) result.Add(string.Join(",", o.parts));

        // 원 제작자를 Tags로 이동
        if (!string.IsNullOrEmpty(originalCreator))
        {
            int tagIdx = result.FindIndex(l => l.StartsWith("Tags:", StringComparison.OrdinalIgnoreCase));
            if (tagIdx >= 0)
            {
                string cur = result[tagIdx].Substring(result[tagIdx].IndexOf(':') + 1).Trim();
                var toks = cur.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (!toks.Contains(originalCreator, StringComparer.OrdinalIgnoreCase))
                    cur = string.IsNullOrEmpty(cur) ? originalCreator : cur + " " + originalCreator;
                result[tagIdx] = "Tags:" + cur;
            }
            else
            {
                int cIdx = result.FindIndex(l => l.StartsWith("Creator:", StringComparison.OrdinalIgnoreCase));
                if (cIdx >= 0) result.Insert(cIdx + 1, "Tags:" + originalCreator);
            }
        }
        return result;
    }

    private double GetBpm(string[] lines)
    {
        bool inTiming = false;
        foreach (var line in lines)
        {
            if (line.TrimStart().Equals("[TimingPoints]", StringComparison.OrdinalIgnoreCase)) { inTiming = true; continue; }
            if (inTiming && line.StartsWith("[")) break;
            if (inTiming && line.Contains(","))
            {
                var parts = line.Split(',');
                if (parts.Length >= 7 && parts[6].Trim() == "1" &&
                    double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double ms) && ms > 0)
                    return 60000.0 / ms;
            }
        }
        return 120.0;
    }

    // 7→8 미들 판정에서 '로컬 16비트' 계산용: uninherited 타이밍포인트 (offset, beatMs)
    private List<(double off, double bm)> _tps7to8 = new List<(double, double)>();
    private void BuildTps(string[] lines, double bpmFallback)
    {
        _tps7to8.Clear();
        bool inTiming = false;
        foreach (var line in lines)
        {
            if (line.TrimStart().Equals("[TimingPoints]", StringComparison.OrdinalIgnoreCase)) { inTiming = true; continue; }
            if (inTiming && line.StartsWith("[")) break;
            if (inTiming && line.Contains(","))
            {
                var p = line.Split(',');
                if (p.Length >= 7 && p[6].Trim() == "1" &&
                    double.TryParse(p[1].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double bm) && bm > 0 &&
                    double.TryParse(p[0].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double off))
                    _tps7to8.Add((off, bm));
            }
        }
        _tps7to8.Sort((a, b) => a.off.CompareTo(b.off));
        if (_tps7to8.Count == 0) _tps7to8.Add((0.0, 60000.0 / Math.Max(1e-6, bpmFallback)));
    }
    private double BeatMsAt(int time)
    {
        double bm = _tps7to8[0].bm;
        foreach (var (off, b) in _tps7to8) { if (time >= off - 1e-6) bm = b; else break; }
        return bm;
    }

    // ── 8K → 10K : 박마다 양손 시프트를 번갈아 변경 ───────────────
    //   왼손  8K{0,1,2,3} → 10K {0,1,2,3}(+0) 또는 {1,2,3,4}(+1)
    //   오른손 8K{4,5,6,7} → 10K {5,6,7,8}(+1) 또는 {6,7,8,9}(+2)
    //   같은 박 안의 노트는 같은 시프트를 공유.
    //   매 박 시프트를 바꾸되, 직전 박 경계와 연타(잭)가 생기면 그 박만 유지.
    //   LN(롱노트) 진행 중인 손은 그 LN이 끝날 때까지 시프트 고정 → 겹침 방지(최우선).
    // wideLayout: true = 빈칸(gap) 0~4 다섯 가지 배치 사용 / false = 기존 0123·1234 두 가지만(예전 동작)
    private List<string> Convert8To10(List<string> lines, double bpm, bool fill = false, double fillGapMult = 1.01, bool wideLayout = true)
    {
        // 모든 uninherited(빨간선) 타이밍포인트 = (offset, beatMs). BPM 변경 맵 대응.
        var tps = new List<(double off, double bm)>();
        {
            bool inT = false;
            foreach (var line in lines)
            {
                if (line.TrimStart().Equals("[TimingPoints]", StringComparison.OrdinalIgnoreCase)) { inT = true; continue; }
                if (inT && line.StartsWith("[")) break;
                if (inT && line.Contains(","))
                {
                    var p = line.Split(',');
                    if (p.Length >= 7 && p[6].Trim() == "1" &&
                        double.TryParse(p[1].Trim(), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double bm) && bm > 0 &&
                        double.TryParse(p[0].Trim(), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double off))
                        tps.Add((off, bm));
                }
            }
            tps.Sort((a, b) => a.off.CompareTo(b.off));
            if (tps.Count == 0) tps.Add((0.0, 60000.0 / Math.Max(1e-6, bpm)));   // 폴백
        }

        // time에 유효한 타이밍포인트 인덱스 (그 시각 이전의 마지막 빨간선)
        int TpIndex(int time)
        {
            int idx = 0;
            for (int k = 0; k < tps.Count; k++) { if (time >= tps[k].off - 1e-6) idx = k; else break; }
            return idx;
        }
        double BeatMsAt(int time) => tps[TpIndex(time)].bm;
        // 잭 임계값: 그 시각의 로컬 16비트(1박/4×1.5 = 0.375박). 4분음표 동컬럼은 잭으로 안 봄 →
        //   8→10 시프트 교대 자유도 확보. JackFree(시프트)·Conflicts(Dense 채움) 둘 다 이 값 사용.
        double JackGapAt(int time) => BeatMsAt(time) * 0.375;
        // 박 그룹 키: (타이밍포인트 인덱스, 그 안에서의 로컬 박번호) — tp가 다르면 무조건 다른 박
        //   첫 구간(idx==0) 박 그리드 원점은 tp 오프셋이 아니라 '첫 노트 시각'으로 잡는다
        //   (첫 노트가 그리드에서 벗어나 시작하는 맵에서 그룹 경계를 첫 노트에 맞춤).
        //   firstNoteTime은 objs 파싱 직후 채워짐(아래). 그 전엔 BeatOf 미호출.
        int firstNoteTime = 0;
        (int tp, long b) BeatOf(int time)
        {
            int idx = TpIndex(time);
            double origin = (idx == 0) ? firstNoteTime : tps[idx].off;
            long lb = (long)Math.Floor((time - origin) / Math.Max(1e-6, tps[idx].bm) + 1e-6);
            return (idx, lb);
        }

        // 1) 헤더 통과 + HitObject 라인 수집
        var result = new List<string>();
        var objLines = new List<string>();
        bool inHO = false;
        foreach (var line in lines)
        {
            if (!inHO && line.StartsWith("CircleSize:", StringComparison.OrdinalIgnoreCase))
            { result.Add("CircleSize:10"); continue; }
            if (!inHO && line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
            {
                string v = line.Substring("Version:".Length).Trim();
                if (v.EndsWith(" 8K", StringComparison.OrdinalIgnoreCase))
                    v = v.Substring(0, v.Length - 3);
                string suffix = " 10K";
                if (fill) suffix += (fillGapMult < 1.0) ? " More Dense" : " Dense";   // 채움 모드 표기
                result.Add("Version:" + v + suffix);
                continue;
            }
            if (line.TrimStart().Equals("[HitObjects]", StringComparison.OrdinalIgnoreCase))
            { inHO = true; result.Add(line); continue; }
            if (inHO) { objLines.Add(line); continue; }   // [HitObjects]는 항상 마지막 섹션
            result.Add(line);
        }

        // 2) 오브젝트 파싱 (col8, 시작/끝 시각, LN 여부)
        var objs = new List<(int time, int col8, int endTime, bool isLN, string[] parts)>();
        foreach (var line in objLines)
        {
            if (!line.Contains(",")) continue;
            var parts = line.Split(',');
            if (parts.Length < 4 ||
                !int.TryParse(parts[0].Trim(), out int x) ||
                !int.TryParse(parts[2].Trim(), out int t) ||
                !int.TryParse(parts[3].Trim(), out int type))
                continue;

            int col8 = Math.Max(0, Math.Min(7, (int)Math.Floor(x * 8.0 / 512)));
            bool isLN = (type & 128) != 0;               // mania 홀드 = 타입 비트 7
            int endTime = t;
            if (isLN && parts.Length >= 6)               // 홀드: parts[5] = "endTime:hitSample"
            {
                var endField = parts[5].Split(':')[0].Trim();
                if (int.TryParse(endField, out int e)) endTime = e;
            }
            objs.Add((t, col8, endTime, isLN, parts));
        }

        // 박 그리드 원점: 첫 노트 시각(없으면 첫 tp 오프셋). BeatOf의 idx==0 구간이 이 값을 씀.
        firstNoteTime = objs.Count > 0 ? objs.Min(o => o.time) : (int)tps[0].off;

        // 3) 손별로 박(beat) 단위 처리
        //    - 박마다 시프트를 바꾸되(변화 ↑), 직전 박과 연타(잭)가 생기면 그 박만 유지.
        //      단, LN 끝 → 단/롱노트 시작은 연타가 아님(누르다 떼고 누르는 것).
        //    - 동시에 잡힌 LN이 있는 동안만 시프트 고정(겹침 방지). 연속 LN끼리는 자유.
        //    - 자유 전환 시에도 LN 꼬리에 다른 노트가 떨어지는 겹침은 명시적으로 차단.
        //      꼬리 시각 == 다음 노트 시작 시각(맞닿음)도 소스 컬럼과 무관하게 금지.
        // 채움 모드용: 배치된 실노트(컬럼별 점유)와 추가생성 후보
        // 빈칸 컬럼은 손 안에서만 생기고(왼손 0~4 / 오른손 5~9) 손끼리 겹치지 않아 손별 독립 검증 가능
        const int LockMinMs = 100;   // 이 길이 미만 LN은 단노트로 간주 → 시프트 락 안 걸림(흐름 유지)
        var placedAll = new List<(int c10, int start, int end)>();
        var addCands  = new List<(int c10, int start, int end, string[] parts)>();

        for (int hand = 0; hand < 2; hand++)
        {
            // 레이아웃 = '비는 칸(gap)'의 손 기준 로컬 위치(0~4). 나머지 4칸에 8K 4열을 순서대로 넣는다.
            //   gap4 = 0123(기존 shift0) / gap0 = 1234(기존 shift1)  ← 기본 후보, 우선순위 종전과 동일
            //   gap1 = 0234 / gap3 = 0124 / gap2 = 0134             ← 기본 둘이 다 막혔을 때만 쓰는 대체 레이아웃
            int hb8  = hand * 4;   // 이 손의 8K 첫 열 (0 / 4)
            int hb10 = hand * 5;   // 이 손의 10K 첫 열 (0 / 5)
            int Map(int col8, int g) { int l = col8 - hb8; return hb10 + (l < g ? l : l + 1); }
            const int baseA = 4, baseB = 0;   // 기존 두 레이아웃
            var handObjs = objs.Where(o => (o.col8 <= 3 ? 0 : 1) == hand)
                               .OrderBy(o => o.time).ThenBy(o => o.col8).ToList();

            var placedLN = new List<(int c10, int src8, int start, int end)>();  // 배치된 LN들
            int lnShift = baseA;             // 현재 동시에 잡혀 있는 LN들의 공유 레이아웃
            int prevShift = baseA;
            var lastUsed = new int[5];       // 레이아웃별 마지막 사용 순번(New 배치의 라운드로빈용)
            int useSeq = 0;
            // 컬럼별 '마지막 릴리즈'(탭=시작, LN=끝)와 소스 c8. 직전 박 경계 + purge된 직전 LN까지 포함.
            var lastRel = new Dictionary<int, (int end, int c8)>();

            int i = 0;
            while (i < handObjs.Count)
            {
                var beat = BeatOf(handObjs[i].time);
                int j = i;
                while (j < handObjs.Count && BeatOf(handObjs[j].time) == beat) j++;   // 같은 박 그룹 [i, j)
                int gStart = handObjs[i].time;

                placedLN.RemoveAll(L => L.end < gStart);    // 더 이상 영향 없는 LN 제거
                // 락(시프트 고정)은 '진짜 지속 홀드'에만. 짧은 LN(<LockMinMs)은 단노트처럼 쳐서
                // 락 트리거에서 제외 → 흐름 유지. (LN 자체는 그대로 두고 Safe가 겹침은 계속 차단)
                bool hasActive = false;
                foreach (var L in placedLN) if (L.end > gStart && (L.end - L.start) >= LockMinMs) { hasActive = true; break; }
                int other = (prevShift == baseA) ? baseB : baseA;

                // 겹침 안전: 범위 [bi,bj)를 placedLN 스냅샷과 대조. 같은 col10 안에서
                //   내부 겹침도, 꼬리↔머리 맞닿음도 전부 금지.
                bool SafeRange(int bi, int bj, int s, List<(int c10, int src8, int start, int end)> pLN)
                {
                    for (int k = bi; k < bj; k++)
                    {
                        int c10 = Map(handObjs[k].col8, s);
                        int t = handObjs[k].time;
                        int te = handObjs[k].isLN ? handObjs[k].endTime : handObjs[k].time;
                        foreach (var L in pLN)
                        {
                            if (L.c10 != c10) continue;
                            if (t < L.end && te > L.start) return false;   // 내부 겹침
                            if (t == L.end) return false;                  // LN 꼬리에 노트 시작(단노트·LN 머리 모두 금지)
                            if (te == L.start) return false;               // 이 LN의 꼬리에 기존 노트 시작
                        }
                    }
                    return true;
                }
                // 잭 안전: 범위 [bi,bj)의 모든 노트를 lastRel 스냅샷(lr)과 대조.
                //   - 소스 c8 같으면 원본 잭 보존(허용) / 그 외 jackGap 이내 재타 금지
                //     (LN 꼬리 맞닿음은 SafeRange가 이미 막음)
                bool JackFreeRange(int bi, int bj, int s, Dictionary<int, (int end, int c8)> lr)
                {
                    for (int k = bi; k < bj; k++)
                    {
                        int c10 = Map(handObjs[k].col8, s);
                        if (lr.TryGetValue(c10, out var pr))
                        {
                            int delta = handObjs[k].time - pr.end;   // 직전 릴리즈→이 노트 시작
                            if (pr.c8 != handObjs[k].col8 && delta > 0 && delta <= JackGapAt(handObjs[k].time))
                                return false;
                        }
                    }
                    return true;
                }
                bool Safe(int s)     => SafeRange(i, j, s, placedLN);
                bool JackFree(int s) => JackFreeRange(i, j, s, lastRel);

                // 이 박을 시프트 s로 배치했다고 가정한 lastRel/placedLN 스냅샷 (룩어헤드용)
                Dictionary<int, (int end, int c8)> SimLastRel(int s)
                {
                    var lr = new Dictionary<int, (int end, int c8)>(lastRel);
                    for (int k = i; k < j; k++)
                    {
                        int c10 = Map(handObjs[k].col8, s);
                        int relEnd = handObjs[k].isLN ? handObjs[k].endTime : handObjs[k].time;
                        if (!lr.TryGetValue(c10, out var ex) || relEnd >= ex.end) lr[c10] = (relEnd, handObjs[k].col8);
                    }
                    return lr;
                }
                List<(int c10, int src8, int start, int end)> SimPlacedLN(int s)
                {
                    var p = new List<(int c10, int src8, int start, int end)>(placedLN);
                    for (int k = i; k < j; k++)
                        if (handObjs[k].isLN) p.Add((Map(handObjs[k].col8, s), handObjs[k].col8, handObjs[k].time, handObjs[k].endTime));
                    return p;
                }

                // 이 박이 지속 LN(>=LockMinMs)을 '시작'하는가 → 시작하면 다음 박이 이 시프트로 잠김
                bool startsLong = false;
                for (int k = i; k < j; k++)
                    if (handObjs[k].isLN && (handObjs[k].endTime - handObjs[k].time) >= LockMinMs) { startsLong = true; break; }

                // 시프트 s로 이 박을 두면 '다음 박'이 (잠겼을 때) 잭 나는가? 나면 1, 아니면 0
                int NextJackPenalty(int s)
                {
                    if (j >= handObjs.Count) return 0;
                    var nb = BeatOf(handObjs[j].time);
                    int nj = j;
                    while (nj < handObjs.Count && BeatOf(handObjs[nj].time) == nb) nj++;
                    int ngStart = handObjs[j].time;
                    var simLN = SimPlacedLN(s);
                    bool nActive = simLN.Any(L => L.end > ngStart && (L.end - L.start) >= LockMinMs);
                    if (!nActive) return 0;   // 다음 박이 안 잠기면 자유라 미리 막을 필요 없음
                    return JackFreeRange(j, nj, s, SimLastRel(s)) ? 0 : 1;
                }

                // 기본 후보(0123/1234)가 다 막혔을 때만 찾는 대체 레이아웃: 겹침·잭 둘 다 안전한 첫 번째.
                //   가운데가 뚫리는 gap2(0134)는 손 모양을 제일 많이 흐트러뜨려서 마지막에 본다.
                int FirstClean(int skipA, int skipB)
                {
                    if (!wideLayout) return -1;   // Old 배치: 대체 레이아웃 없음 → 아래 예전 폴백 그대로
                    foreach (int g in new[] { baseA, baseB, 1, 3, 2 })
                    {
                        if (g == skipA || g == skipB) continue;
                        if (Safe(g) && JackFree(g)) return g;
                    }
                    return -1;
                }

                int shift;
                if (hasActive)
                {
                    // 동시 LN 잡힌 동안은 lnShift 고정이 원칙. 단 그게 잭을 만들고
                    //   대안 레이아웃이 겹침 안전 + 무잭이면 대안으로 (락이 잭예방을 건너뛰지 않게).
                    int altLn = (lnShift == baseA) ? baseB : baseA;
                    int alt2;
                    if      (Safe(lnShift) && JackFree(lnShift)) shift = lnShift;
                    else if (Safe(altLn)   && JackFree(altLn))   shift = altLn;
                    else if ((alt2 = FirstClean(lnShift, altLn)) >= 0) shift = alt2;
                    else                                          shift = lnShift;
                }
                else if (wideLayout)
                {
                    // New 배치: 쓸 수 있는 레이아웃(겹침·잭 안전) 중 '가장 오래 안 쓴' 것 → 5가지가 고르게 돌아감.
                    //   지속 LN을 시작하는 박은 다음 박이 잠기므로, 1박 룩어헤드 페널티가 최소인 것들로 먼저 거른다.
                    var clean = new List<int>();
                    for (int g = 0; g < 5; g++) if (Safe(g) && JackFree(g)) clean.Add(g);
                    if (clean.Count == 0)
                    {
                        shift = Safe(other) ? other : prevShift;   // 전부 막히면 예전 폴백(겹침만 회피)
                    }
                    else
                    {
                        if (startsLong && clean.Count > 1)
                        {
                            int best = clean.Min(g => NextJackPenalty(g));
                            clean = clean.Where(g => NextJackPenalty(g) == best).ToList();
                        }
                        shift = clean[0];
                        foreach (int g in clean) if (lastUsed[g] < lastUsed[shift]) shift = g;
                    }
                }
                else
                {
                    bool oClean = Safe(other) && JackFree(other);
                    bool pClean = Safe(prevShift) && JackFree(prevShift);
                    int alt2;
                    if (startsLong && oClean && pClean && other != prevShift)
                    {
                        // 둘 다 깨끗 + 이 박이 지속 LN 시작 → 다음(잠길) 박이 잭 안 나는 쪽 선택.
                        //   동점이면 flip(other) 우선(변화 유지). 1박 룩어헤드로 강제잭 예방.
                        shift = (NextJackPenalty(other) <= NextJackPenalty(prevShift)) ? other : prevShift;
                    }
                    else if (oClean)      shift = other;       // 기본: 안전+무잭이면 flip
                    else if (pClean)      shift = prevShift;   // 그다음 유지
                    else if ((alt2 = FirstClean(other, prevShift)) >= 0) shift = alt2;   // 대체 레이아웃으로 잭 회피
                    else if (Safe(other)) shift = other;       // 겹침만 피함
                    else                  shift = prevShift;
                }
                lastUsed[shift] = ++useSeq;   // 라운드로빈 기준 갱신(락 걸린 박도 포함)

                bool startedLongLN = false;

                for (int k = i; k < j; k++)
                {
                    var o = handObjs[k];
                    int c10 = Map(o.col8, shift);
                    o.parts[0] = ((int)((c10 + 0.5) * 512 / 10)).ToString();
                    int relEnd = o.isLN ? o.endTime : o.time;
                    if (fill) placedAll.Add((c10, o.time, relEnd));
                    // 컬럼별 마지막 릴리즈 갱신(끝이 더 늦은 것 유지) → 다음 박 잭 판정 기준
                    if (!lastRel.TryGetValue(c10, out var ex) || relEnd >= ex.end) lastRel[c10] = (relEnd, o.col8);
                    if (o.isLN) { placedLN.Add((c10, o.col8, o.time, o.endTime));
                                  if (o.endTime - o.time >= LockMinMs) startedLongLN = true; }
                }

                // 채움 후보: 빈칸(gap) 바로 옆 노트를 같은 시각·같은 타입(LN이면 같은 길이)으로 빈칸에 복제.
                //   이웃 우선순위는 손 안쪽(로컬 2번) 쪽 → 막히면 반대쪽.
                //   gap0→1 / gap1→2,0 / gap3→2,4 / gap4→3 / gap2는 1·3이 대칭이라 건반 중앙 쪽(왼손3·오른손1) 먼저.
                if (fill)
                {
                    int gapC10 = hb10 + shift;
                    int[] nbOrder;
                    if      (shift == 0) nbOrder = new[] { 1 };
                    else if (shift == 4) nbOrder = new[] { 3 };
                    else if (shift == 1) nbOrder = new[] { 2, 0 };
                    else if (shift == 3) nbOrder = new[] { 2, 4 };
                    else                 nbOrder = (hand == 0) ? new[] { 3, 1 } : new[] { 1, 3 };

                    foreach (int nb in nbOrder)
                    {
                        int srcC8 = hb8 + (nb < shift ? nb : nb - 1);   // 그 로컬 칸을 쓰는 8K 열
                        for (int k = i; k < j; k++)
                        {
                            var o = handObjs[k];
                            if (o.col8 != srcC8) continue;
                            var tp = (string[])o.parts.Clone();
                            tp[0] = ((int)((gapC10 + 0.5) * 512 / 10)).ToString();
                            addCands.Add((gapC10, o.time, o.isLN ? o.endTime : o.time, tp));
                        }
                    }
                }
                // 지속 홀드(>=LockMinMs)가 '새로' 시작될 때만 락 기준 갱신 → 체인 경계에서 flip 여지 유지
                if (!hasActive && startedLongLN) lnShift = shift;

                prevShift = shift;
                i = j;
            }
        }

        // 3.5) 채움 노트 검증·확정: 같은 컬럼에서 겹침·잭이면 그 노트는 버림
        if (fill && addCands.Count > 0)
        {
            // 컬럼별 점유 구간(실노트 + 확정된 추가노트). LN끝↔머리 맞닿음도 금지.
            var occ = new Dictionary<int, List<(int s, int e)>>();
            foreach (var p in placedAll)
            {
                if (!occ.TryGetValue(p.c10, out var lst)) { lst = new List<(int, int)>(); occ[p.c10] = lst; }
                lst.Add((p.start, p.end));
            }
            bool Conflicts(int c10, int start, int end)
            {
                if (!occ.TryGetValue(c10, out var lst)) return false;
                foreach (var n in lst)
                {
                    if (start == n.s) return true;                 // 같은 컬럼·같은 시각 = 스택
                    if (start < n.e && n.s < end) return true;     // 내부 겹침
                    if (start == n.e || end == n.s) return true;   // LN 꼬리↔머리 맞닿음도 금지
                    int gap = Math.Abs(n.s - start);
                    // 채움 잭 기준 = 모드별 fillGapMult박 (Dense=1박/성김, More Dense=0.375박/빽빽).
                    //   시프트용 JackGapAt(0.375)와는 분리.
                    if (gap > 0 && gap <= BeatMsAt(start) * fillGapMult)
                    {
                        return true;                                    // 근접 재타 = 잭
                    }
                }
                return false;
            }
            foreach (var cand in addCands.OrderBy(a => a.start).ThenBy(a => a.c10))
            {
                if (Conflicts(cand.c10, cand.start, cand.end)) continue;
                // 확정: objs에 추가하고 점유에 반영(이후 후보가 이 노트를 존중)
                objs.Add((cand.start, cand.c10, cand.end, cand.end > cand.start, cand.parts));
                if (!occ.TryGetValue(cand.c10, out var lst)) { lst = new List<(int, int)>(); occ[cand.c10] = lst; }
                lst.Add((cand.start, cand.end));
            }
        }

        // 4) 시간순으로 출력
        objs.Sort((a, b) => a.time != b.time ? a.time.CompareTo(b.time) : a.col8.CompareTo(b.col8));
        foreach (var o in objs)
            result.Add(string.Join(",", o.parts));

        return result;
    }

    // 첫 비유전(uninherited) 타이밍포인트의 오프셋 = 박자 그리드 기준점
    private double GetFirstOffset(List<string> lines)
    {
        bool inTiming = false;
        foreach (var line in lines)
        {
            if (line.TrimStart().Equals("[TimingPoints]", StringComparison.OrdinalIgnoreCase)) { inTiming = true; continue; }
            if (inTiming && line.StartsWith("[")) break;
            if (inTiming && line.Contains(","))
            {
                var p = line.Split(',');
                if (p.Length >= 2 &&
                    double.TryParse(p[1].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double bl) && bl > 0 &&
                    double.TryParse(p[0].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double off))
                    return off;
            }
        }
        return 0;
    }

    private List<string> ConvertLines(string[] lines, double bpm)
    {
        var result = new List<string>();
        bool inHitObjects = false;
        var hitObjects = ParseHitObjects(lines);
        // 미들 판정 상태 초기화
        _lastMiddleCol = 4;
        BuildTps(lines, bpm);   // 로컬 16비트 계산용
        string originalCreator = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("CircleSize:", StringComparison.OrdinalIgnoreCase))
            {
                result.Add("CircleSize:8");
                continue;
            }
            if (line.StartsWith("Creator:", StringComparison.OrdinalIgnoreCase))
            {
                originalCreator = line.Substring("Creator:".Length).Trim();
                result.Add("Creator:나래");
                continue;
            }
            if (line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
            {
                // 난이도명을 바꿔야 osu!가 '새 난이도'로 인식해 곡 목록에 바로 뜸
                string v = line.Substring("Version:".Length).Trim();
                result.Add("Version:" + v + " 8K");
                continue;
            }
            if (line.StartsWith("BeatmapID:", StringComparison.OrdinalIgnoreCase))
            {
                // 원본(랭크) ID와 충돌 방지 — 0으로 초기화 (BeatmapSetID는 유지)
                result.Add("BeatmapID:0");
                continue;
            }
            if (line.TrimStart().Equals("[HitObjects]", StringComparison.OrdinalIgnoreCase))
            {
                inHitObjects = true;
                result.Add(line);
                foreach (var ho in hitObjects)
                    result.AddRange(ConvertHitObject(ho, hitObjects, bpm));
                continue;
            }
            if (inHitObjects) continue;
            result.Add(line);
        }

        // 원래 제작자를 Tags로 이동 (없으면 Creator 뒤에 Tags 삽입)
        if (!string.IsNullOrEmpty(originalCreator))
        {
            int tagIdx = result.FindIndex(l => l.StartsWith("Tags:", StringComparison.OrdinalIgnoreCase));
            if (tagIdx >= 0)
            {
                string cur = result[tagIdx].Substring(result[tagIdx].IndexOf(':') + 1).Trim();
                var toks = cur.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (!toks.Contains(originalCreator, StringComparer.OrdinalIgnoreCase))
                    cur = string.IsNullOrEmpty(cur) ? originalCreator : cur + " " + originalCreator;
                result[tagIdx] = "Tags:" + cur;
            }
            else
            {
                int cIdx = result.FindIndex(l => l.StartsWith("Creator:", StringComparison.OrdinalIgnoreCase));
                if (cIdx >= 0) result.Insert(cIdx + 1, "Tags:" + originalCreator);
            }
        }
        return result;
    }

    private List<HitObject> ParseHitObjects(string[] lines)
    {
        var list = new List<HitObject>();
        bool inHO = false;
        foreach (var line in lines)
        {
            if (line.TrimStart().Equals("[HitObjects]", StringComparison.OrdinalIgnoreCase)) { inHO = true; continue; }
            if (inHO && line.StartsWith("[")) break;
            if (inHO && line.Contains(","))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;
                if (!int.TryParse(parts[0].Trim(), out int x)) continue;
                if (!int.TryParse(parts[2].Trim(), out int time)) continue;
                int col7 = Math.Max(0, Math.Min(6, (int)Math.Floor((double)x * 7 / 512)));
                list.Add(new HitObject { OriginalLine = line, Time = time, Col7 = col7, Parts = parts });
            }
        }
        return list;
    }

    // 동률이면 col4, col5 둘 다 반환
    private List<string> ConvertHitObject(HitObject ho, List<HitObject> allObjects, double bpm)
    {
        var cols = MapColumns(ho.Col7, ho.Time, allObjects, bpm);
        return cols.Select(col10 =>
        {
            int newX = (int)((col10 + 0.5) * 512 / 8);
            var parts = ho.Parts.ToArray();
            parts[0] = newX.ToString();
            return string.Join(",", parts);
        }).ToList();
    }

    private int _lastMiddleCol = 3;

    // col7 → 8k col 목록 (동률이면 {3,4} 반환)
    private List<int> MapColumns(int col7, int time, List<HitObject> allObjects, double bpm)
    {
        switch (col7)
        {
            case 0: return new List<int> { 0 };
            case 1: return new List<int> { 1 };
            case 2: return new List<int> { 2 };
            case 4: return new List<int> { 5 };
            case 5: return new List<int> { 6 };
            case 6: return new List<int> { 7 };
            case 3:
                var result = DecideMiddle(time, allObjects, bpm);
                if (result.Count == 1) _lastMiddleCol = result[0];
                return result;
            default: return new List<int> { col7 };
        }
    }

    // 미들노트 판정
    private List<int> DecideMiddle(int time, List<HitObject> allObjects, double bpm)
    {
        double msPerBeat = 60000.0 / bpm;
        double nearWindow = msPerBeat / 2.0 + 1.0;

        var others = allObjects.Where(o => o.Col7 != 3).ToList();
        var middleTimes = new HashSet<int>(allObjects.Where(o => o.Col7 == 3).Select(o => o.Time));
        var allTimes = others.Select(o => o.Time).Distinct().OrderBy(t => t).ToList();

        List<int> GetCols(int t) => others.Where(o => o.Time == t).Select(o => o.Col7).ToList();

        double? SingleHandCol(int t)
        {
            var cols = GetCols(t);
            var left  = cols.Where(c => c <= 2).ToList();
            var right = cols.Where(c => c >= 4).ToList();
            if (left.Count > 0 && right.Count == 0) return left.Average();
            if (right.Count > 0 && left.Count == 0) return right.Average();
            return null;
        }

        bool HasAdjacent(int t)
        {
            var cols = GetCols(t);
            if (cols.Count != 2) return false;
            if (!cols.Any(c => c <= 2) || !cols.Any(c => c >= 4)) return false;
            if (!cols.Contains(2) && !cols.Contains(4)) return false;
            if (cols.Any(c => c > 5)) return false;  // col6 이상 제외
            if (cols.Contains(2) && cols.Contains(4) && cols.Count == 2) return false; // col2+col4만이면 제외
            return true;
        }

        (int L, int R) LRCount(int t)
        {
            var cols = GetCols(t);
            return (cols.Count(c => c <= 2), cols.Count(c => c >= 4));
        }

        double? RightAvg(int t)
        {
            var cols = others.Where(o => o.Time == t && o.Col7 >= 4).Select(o => o.Col7).ToList();
            return cols.Count > 0 ? cols.Average() : (double?)null;
        }

        string StairDirection(List<int> adjTimes)
        {
            var ras = adjTimes.Select(t => (t, RightAvg(t))).Where(x => x.Item2 != null).OrderBy(x => x.t).ToList();
            if (ras.Count < 2) return null;
            if (ras[1].Item2 > ras[0].Item2) return "up";
            if (ras[1].Item2 < ras[0].Item2) return "down";
            return null;
        }

        // near 안에서 첫 양손계단의 방향을 Search와 동일하게 추출 (sameflow 판정용)
        string AdjDir(List<int> times)
        {
            for (int i = 0; i < times.Count; i++)
            {
                int t = times[i];
                if (Math.Abs(time - t) > nearWindow) break;
                if (middleTimes.Contains(t)) continue;
                var cols = GetCols(t);
                if (cols.Count == 1) continue;
                if (HasAdjacent(t))
                {
                    var (pl, pr) = LRCount(t);
                    if (pl != pr) return pl > pr ? "L" : "R";
                    var adjTimes = new List<int> { t };
                    int lastT = t;
                    for (int j = i + 1; j < times.Count; j++)
                    {
                        int t2 = times[j];
                        if (middleTimes.Contains(t2)) continue;
                        if (Math.Abs(t2 - lastT) > nearWindow) break;
                        if (HasAdjacent(t2)) { adjTimes.Add(t2); lastT = t2; }
                    }
                    return StairDirection(adjTimes);
                }
            }
            return null;
        }

        // near 안에서 양손노트는 건너뛰고 첫 단일노트의 손(L/R) 반환 (대칭 단일 판정용)
        string NearestSingleHand(List<int> times)
        {
            foreach (int t in times)
            {
                if (Math.Abs(time - t) > nearWindow) break;
                if (middleTimes.Contains(t)) continue;
                var cols = GetCols(t);
                if (cols.Count == 1 && SingleHandCol(t) != null)
                    return cols[0] <= 2 ? "L" : "R";
            }
            return null;
        }

        bool SameHand(int a, int b) => (a <= 2 && b <= 2) || (a >= 4 && b >= 4);

        // 동률 tiebreak: 미들 옆에 col2만 붙으면 3, col4만 붙으면 4, 둘다/없으면 동률
        List<int> Tie()
        {
            var cols = GetCols(time);
            bool h2 = cols.Contains(2), h4 = cols.Contains(4);
            if (h2 && !h4) return new List<int> { 3 };
            if (h4 && !h2) return new List<int> { 4 };
            return new List<int> { 3, 4 };
        }

        int AllCount(int t) => allObjects.Count(o => o.Time == t);  // 그 시점 전체 노트수(잼 크기)

        // 미들을 통과하는 단조 계단 라인. 대칭(좌우 균등) 라인 우선, 그 중 최장.
        // strong(풀 대계단) = left>=2 && right>=2 + 라인 내부에 4개+ 잼 스텝 없음
        List<int> StairSpan(out bool strong)
        {
            strong = false;
            var nxt = allTimes.Where(t => t > time).OrderBy(t => t).ToList();
            var prv = allTimes.Where(t => t < time).OrderByDescending(t => t).ToList();
            var cands = new List<(bool sym, int span, int lo, int hi, List<int> line)>();
            foreach (int d in new[] { 1, -1 })   // d=1: 시간↑=col↑ / d=-1: 시간↑=col↓
            {
                int lo = 3, hi = 3, expect = 3 + d, last = time;
                var line = new List<int> { time };
                foreach (int t in nxt)
                {
                    if (t - last > nearWindow) break;
                    if (expect >= 0 && expect <= 6 && GetCols(t).Contains(expect))
                    { hi = Math.Max(hi, expect); lo = Math.Min(lo, expect); expect += d; last = t; line.Add(t); }
                    else
                    {
                        var ct = GetCols(t);
                        bool opp = ct.Count == 2 && Math.Abs(ct[0] - ct[1]) == 1 && !middleTimes.Contains(t) && (d < 0 ? ct.All(c => c >= 4) : ct.All(c => c <= 2));
                        if (opp) { last = t; continue; }
                        break;
                    }
                }
                expect = 3 - d; last = time;
                foreach (int t in prv)
                {
                    if (last - t > nearWindow) break;
                    if (expect >= 0 && expect <= 6 && GetCols(t).Contains(expect))
                    { hi = Math.Max(hi, expect); lo = Math.Min(lo, expect); expect -= d; last = t; line.Add(t); }
                    else
                    {
                        var ct = GetCols(t);
                        bool opp = ct.Count == 2 && Math.Abs(ct[0] - ct[1]) == 1 && !middleTimes.Contains(t) && (d < 0 ? ct.All(c => c <= 2) : ct.All(c => c >= 4));
                        if (opp) { last = t; continue; }
                        break;
                    }
                }
                if (hi > 3 && lo < 3) cands.Add((3 - lo == hi - 3, hi - lo, lo, hi, line));
            }
            if (cands.Count == 0) return null;
            cands.Sort((a, b) => a.sym != b.sym ? b.sym.CompareTo(a.sym) : b.span.CompareTo(a.span));
            var c = cands[0];
            int left = 3 - c.lo, right = c.hi - 3;
            // 진짜 대계단: 라인 내부(양 끝점 제외)에 4개+ 잼이 없어야 (잼에서 빼낸 가짜 차단)
            var sortedLine = c.line.OrderBy(x => x).ToList();
            int nGe4 = 0;
            for (int k = 1; k < sortedLine.Count - 1; k++)
                if (AllCount(sortedLine[k]) >= 4) nGe4++;
            strong = (left >= 2 && right >= 2) && nGe4 == 0;
            if (left == right) return new List<int> { 3, 4 };
            return new List<int> { left > right ? 3 : 4 };
        }

        // 한방향 계단: 미들 '직후'로 이어지는 단조라인만. 오름(d+1)→4, 내림(d-1)→3.
        // 미들이 계단을 시작할 때만 (앞에서 끝나는 계단은 미들이 끝점이라 제외).
        List<int> OneWayStair()
        {
            var nxt = allTimes.Where(t => t > time).OrderBy(t => t).ToList();
            List<(int t, int col)> Fwd(int d)
            {
                int expect = 3 + d, last = time;
                var ts = new List<(int, int)>();
                foreach (int t in nxt)
                {
                    if (Math.Abs(t - last) > nearWindow) break;
                    if (expect >= 0 && expect <= 6 && GetCols(t).Contains(expect))
                    { ts.Add((t, expect)); expect += d; last = t; }
                    else break;
                }
                return ts;
            }
            bool Clean(List<(int t, int col)> ts)
            {
                for (int k = 0; k < ts.Count; k++)
                    if (AllCount(ts[k].t) >= 4 && ts[k].col != 0 && ts[k].col != 6) return false;
                return true;
            }
            var up = Fwd(1); var dn = Fwd(-1);
            // 첫 스텝이 반대쪽 인접col 포함(양손 점프)이면 한방향계단 아님
            if (up.Count > 0 && GetCols(up[0].Item1).Contains(2)) up = new List<(int, int)>();
            if (dn.Count > 0 && GetCols(dn[0].Item1).Contains(4)) dn = new List<(int, int)>();
            bool denseMid = GetCols(time).Count + (middleTimes.Contains(time) ? 1 : 0) >= 4;
            bool asc  = up.Count >= 2 && Clean(up) && dn.Count < 2;
            bool desc = dn.Count >= 2 && Clean(dn) && up.Count < 2 && !denseMid;
            if (asc && !desc) return new List<int> { 4 };
            if (desc && !asc) return new List<int> { 3 };
            return null;
        }

        // 양옆 잭: 미들 직전·직후가 둘 다 단일 col4면 [4], 단일 col2면 [3]
        List<int> FlankJack()
        {
            var pv = allTimes.Where(t => t < time).ToList();
            var nx = allTimes.Where(t => t > time).ToList();
            if (pv.Count == 0 || nx.Count == 0) return null;
            int p = pv.Max(), n = nx.Min();
            if (time - p > nearWindow || n - time > nearWindow) return null;
            var pc = GetCols(p); var nc = GetCols(n);
            if (pc.Count == 1 && pc[0] == 4 && nc.Count == 1 && nc[0] == 4) return new List<int> { 4 };
            if (pc.Count == 1 && pc[0] == 2 && nc.Count == 1 && nc[0] == 2) return new List<int> { 3 };
            return null;
        }

        // 위브 따라가기(나선): 동시노트 없는 미들이 양옆 '반대' 위브(한쪽 col2,한쪽 col4)
        // 사이에 있으면 바로 앞 위브를 따라감. 앞 col2→[3], 앞 col4→[4].
        int? Weave(int t)
        {
            var cs = GetCols(t);
            bool h2 = cs.Contains(2) && !cs.Contains(4), h4 = cs.Contains(4) && !cs.Contains(2);
            return h2 ? 2 : (h4 ? 4 : (int?)null);
        }
        List<int> WeaveFollow()
        {
            if (GetCols(time).Count > 0) return null;
            var pv = allTimes.Where(t => t < time).ToList();
            var nx = allTimes.Where(t => t > time).ToList();
            if (pv.Count == 0 || nx.Count == 0) return null;
            int p = pv.Max(), n = nx.Min();
            if (time - p > nearWindow || n - time > nearWindow) return null;
            int? pb = Weave(p), nb = Weave(n);
            if (pb == null || nb == null || pb == nb) return null;
            return pb == 2 ? new List<int> { 3 } : new List<int> { 4 };
        }

        // 단일 노트 t의 계단 연속 확인:
        // 직전(isPrev): t2가 t보다 3에서 멀거나 같고, t2 이전에 미들이 있어야
        // 단일 노트 t가 미들 time과 직접 연결되는 계단인지 확인
        // (a) t 직전/직후에 미들이 near_window 안에 있음
        // (b) t 직전/직후에 더 멀어지는 단일 노트가 있고, 그 노트 직전/직후에 미들이 있음
        bool HasStairCont(int t, int time, bool isPrev)
        {
            var tCols = GetCols(t);
            if (tCols.Count != 1) return false;
            int tCol = tCols[0];
            if (isPrev)
            {
                // t와 time 사이에 다른 미들 없어야
                if (middleTimes.Any(m => t < m && m < time)) return false;
                if (time - t > nearWindow) return false;
                // (a) t 이전에 미들
                if (middleTimes.Any(m => m < t && t - m <= nearWindow)) return true;
                // (b) t 이전에 더 멀어지는 단일 노트 → 그 이전에 미들
                var cont = allTimes.Where(x => x < t && t - x <= nearWindow).OrderByDescending(x => x);
                foreach (int t2 in cont)
                {
                    if (middleTimes.Contains(t2)) break;
                    var cols2 = GetCols(t2);
                    if (cols2.Count != 1) continue;
                    if (SingleHandCol(t2) == null) continue;
                    int t2Col = cols2[0];
                    if (Math.Abs(t2Col - 3) > Math.Abs(tCol - 3) && SameHand(t2Col, tCol))  // 더 멀어지고 + 같은 손
                    {
                        if (middleTimes.Any(m => m < t2 && t2 - m <= nearWindow)) return true;
                    }
                    break;
                }
                return false;
            }
            else
            {
                if (middleTimes.Any(m => time < m && m < t)) return false;
                if (t - time > nearWindow) return false;
                if (middleTimes.Any(m => m > t && m - t <= nearWindow)) return true;
                var cont = allTimes.Where(x => x > t && x - t <= nearWindow).OrderBy(x => x);
                foreach (int t2 in cont)
                {
                    if (middleTimes.Contains(t2)) break;
                    var cols2 = GetCols(t2);
                    if (cols2.Count != 1) continue;
                    if (SingleHandCol(t2) == null) continue;
                    int t2Col = cols2[0];
                    if (Math.Abs(t2Col - 3) > Math.Abs(tCol - 3) && SameHand(t2Col, tCol))
                    {
                        if (middleTimes.Any(m => m > t2 && m - t2 <= nearWindow)) return true;
                    }
                    break;
                }
                return false;
            }
        }

        List<int> Search(List<int> times, bool isPrev, bool adjOnly = false)
        {
            for (int i = 0; i < times.Count; i++)
            {
                int t = times[i];
                if (Math.Abs(time - t) > nearWindow) break;
                if (middleTimes.Contains(t)) continue;
                var cols = GetCols(t);
                if (cols.Count == 1)
                {
                    if (adjOnly) continue;  // adjOnly: 단일계단은 대계단 아래에서 처리
                    double? rc = SingleHandCol(t);
                    if (rc != null && rc >= 1.0 && rc <= 5.0)
                    {
                        // col1/col5(2칸) 단일계단은 미들에 반대쪽 인접 동시노트 있으면 양보
                        var sc = GetCols(time);
                        int slc = sc.Count(c => c < 3), src = sc.Count(c => c > 3);
                        // 미들이 순수 한쪽(왼손만/오른손만)이면 반대쪽 단일계단 양보
                        if ((rc > 3 && slc > 0 && src == 0) || (rc < 3 && src > 0 && slc == 0)) continue;
                        bool yield = Math.Abs(rc.Value - 3) == 2 && ((rc > 3 && sc.Contains(2)) || (rc < 3 && sc.Contains(4)));
                        if (!yield && HasStairCont(t, time, isPrev)) return new List<int> { rc > 3 ? 4 : 3 };
                    }
                    continue;
                }
                if (HasAdjacent(t))
                {
                    var (pl, pr) = LRCount(t);
                    if (pl > pr) return new List<int> { 3 };
                    if (pr > pl) return new List<int> { 4 };
                    // adj 체인 수집: 마지막 adj 타임 기준 near_window로 연장
                    var adjTimes = new List<int> { t };
                    int lastT = t;
                    for (int j = i + 1; j < times.Count; j++)
                    {
                        int t2 = times[j];
                        if (middleTimes.Contains(t2)) continue;
                        if (Math.Abs(t2 - lastT) > nearWindow) break;
                        if (HasAdjacent(t2)) { adjTimes.Add(t2); lastT = t2; }
                    }
                    string dir = StairDirection(adjTimes);
                    if (dir != null)
                    {
                        if (isPrev) return new List<int> { dir == "down" ? 4 : 3 };
                        else        return new List<int> { dir == "down" ? 3 : 4 };
                    }
                    if (adjOnly) continue;  // adjOnly: 방향 확실한 양손계단만 채택, 약한 fallback은 통과
                    // direction 불명확
                    if (adjTimes.Count == 1)
                    {
                        // isPrev: continue (더 탐색)
                        // isNext: 동시간대 참조 후 기본값
                        if (!isPrev)
                        {
                            var sameCols = GetCols(time);
                            int sameL = sameCols.Count(c => c <= 2), sameR = sameCols.Count(c => c >= 4);
                            if (sameL > sameR) return new List<int> { 3 };
                            if (sameR > sameL) return new List<int> { 4 };
                            if (sameL == sameR && sameL > 0) return Tie();
                            return null;  // 동시노트 없음: 보류 → 대칭 단일 룰로 위임
                        }
                        // isPrev이고 adj 1개: continue
                    }
                    else
                    {
                        if (isPrev) break;
                        return new List<int> { 4 };
                    }
                }
                // 스킵
            }
            return null;
        }

        var prevTimes = others.Where(o => o.Time < time).Select(o => o.Time).Distinct().OrderByDescending(t => t).ToList();
        var nextTimes = others.Where(o => o.Time > time).Select(o => o.Time).Distinct().OrderBy(t => t).ToList();

        var same = GetCols(time);
        int sl = same.Count(c => c <= 2), sr = same.Count(c => c >= 4);

        // [특수] 미들 16비트 연타 풀기: 미들(col3)이 16비트 이하 간격으로 2개 이상 연속이면
        //   그 런 안에서 좌(8col3)·우(8col4)를 한 번씩 번갈아 → 같은 컬럼 잭을 양손 교대로 분산.
        //   런은 미들 노트 시각만으로 결정(결정적)이라 매 노트 호출이 같은 교대 패턴을 냄.
        {
            var mids = allObjects.Where(o => o.Col7 == 3).Select(o => o.Time).Distinct().OrderBy(t => t).ToList();
            int idx = mids.IndexOf(time);
            if (idx >= 0)
            {
                double thr = BeatMsAt(time) / 4.0 * 1.5;   // 16비트(및 그보다 빠른 32비트 등) 포함, 8비트는 제외
                int start = idx, end = idx;
                while (start > 0 && (mids[start] - mids[start - 1]) <= thr) start--;
                while (end < mids.Count - 1 && (mids[end + 1] - mids[end]) <= thr) end++;
                if (end - start >= 1)   // 런 길이 2 이상
                    return new List<int> { ((idx - start) % 2 == 0) ? 3 : 4 };
            }
        }

        // 동시간대 좌우 동수 + 직전·직후 양손계단이 같은 방향이면 진행방향 우선 (up→4, down→3)
        if (sl == sr && sl > 0)
        {
            string dp = AdjDir(prevTimes), dn = AdjDir(nextTimes);
            if (dp == dn && (dp == "up" || dp == "down"))
                return new List<int> { dp == "up" ? 4 : 3 };
        }

        var ssRes = StairSpan(out bool ssStrong);

        // 0) strong 대칭 대계단이면 tie — 단 미들에 col0·col6 둘다 있으면 adjOnly에 양보
        if (ssRes != null && ssStrong && ssRes.Count == 2 && !(GetCols(time).Contains(0) && GetCols(time).Contains(6))) return Tie();

        // 1) 양손계단(adj)만 먼저 — 대계단보다 상위
        var rAdj = Search(prevTimes, true, true);
        if (rAdj != null) return rAdj;
        rAdj = Search(nextTimes, false, true);
        if (rAdj != null) return rAdj;

        // 2) 강한 대계단(풀 대계단)은 단일계단/동시노트보다 우선
        if (ssRes != null && ssStrong) return ssRes.Count == 2 ? Tie() : ssRes;

        // 2.5) 한방향 계단 (동시노트보다 상위)
        var ow = OneWayStair();
        if (ow != null) return ow;

        // 2.6) 양옆 단일 잭 (col4/col2)
        var fj = FlankJack();
        if (fj != null) return fj;

        // 2.7) 미들이 밀집 잼(>=4노트) 안이면 동시노트 균형으로 결정 (단일계단 이웃보다 우선)
        if (sl + sr >= 3)
        {
            if (sl > sr) return new List<int> { 3 };
            if (sr > sl) return new List<int> { 4 };
            return Tie();
        }

        // 2.8) 나선 묶음
        var sb = WeaveFollow();
        if (sb != null) return sb;

        // 2.9) 백워드 계단: 미들로 '들어와' 끝나는 왼손 오름런(..1,0→3), col4 없으면 [3]
        List<int> BackwardStair()
        {
            var prvB = allTimes.Where(t => t < time).OrderByDescending(t => t).ToList();
            int expect = 2, last = time;   // d=+1 백워드: col 2,1,0
            var ts = new List<(int t, int col)>();
            foreach (int t in prvB)
            {
                if (last - t > nearWindow) break;
                if (expect >= 0 && expect <= 6 && GetCols(t).Contains(expect))
                { ts.Add((t, expect)); expect -= 1; last = t; }
                else break;
            }
            bool clean = ts.All(s => AllCount(s.t) < 4 || s.col == 0 || s.col == 6);
            var scB = GetCols(time);
            int slcB = scB.Count(c => c < 3), srcB = scB.Count(c => c > 3);
            if (slcB == srcB && slcB > 0) return null;   // 좌우 균형 미들은 tie가 처리
            if (ts.Count >= 2 && clean && ts[ts.Count - 1].col <= 1 && !scB.Contains(4))
                return new List<int> { 3 };
            return null;
        }
        var bs = BackwardStair();
        if (bs != null) return bs;

        // 3) 전체 Search (단일계단 포함)
        var res = Search(prevTimes, true);
        if (res != null) return res;

        var resNext = Search(nextTimes, false);
        if (resNext != null) return resNext;

        // 동시간대 동수면 동률
        if (sl == sr && sl > 0) return Tie();
        if (sl > sr) return new List<int> { 3 };
        if (sr > sl) return new List<int> { 4 };

        // 약한 대계단
        if (ssRes != null) return ssRes.Count == 2 ? Tie() : ssRes;

        // 대칭 단일: 미들 양옆 최근접 단일노트가 반대 손이면 동률
        string ps = NearestSingleHand(prevTimes), ns = NearestSingleHand(nextTimes);
        if (ps != null && ns != null && ps != ns) return Tie();

        int l = others.Count(o => Math.Abs(o.Time - time) <= nearWindow && o.Col7 <= 2);
        int r = others.Count(o => Math.Abs(o.Time - time) <= nearWindow && o.Col7 >= 4);
        if (l > r) return new List<int> { 3 };
        if (r > l) return new List<int> { 4 };

        return Tie();
    }
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class HitObject
{
    public string OriginalLine;
    public int Time;
    public int Col7;
    public string[] Parts;
}
