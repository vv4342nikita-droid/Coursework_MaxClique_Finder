using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace CliqueCoursework
{
    public partial class Form1 : Form
    {
    
        private static readonly Color C_BG        = Color.FromArgb(240, 243, 249);
        private static readonly Color C_PANEL     = Color.FromArgb(255, 255, 255);
        private static readonly Color C_ACCENT    = Color.FromArgb(37,  99,  235);
        private static readonly Color C_ACCENT2   = Color.FromArgb(22, 163, 74);
        private static readonly Color C_WARN      = Color.FromArgb(202, 138,  4);
        private static readonly Color C_BORDER    = Color.FromArgb(203, 213, 225);
        private static readonly Color C_TEXT      = Color.FromArgb(15,  23,  42);
        private static readonly Color C_SUBTEXT   = Color.FromArgb(100, 116, 139);
        private static readonly Color C_HEADER_BG = Color.FromArgb(15,  23,  42);

        
        private static readonly Font F_TITLE = new Font("Segoe UI", 12f, FontStyle.Bold);
        private static readonly Font F_GROUP = new Font("Segoe UI", 9f,  FontStyle.Bold);
        private static readonly Font F_BODY  = new Font("Segoe UI", 9f);
        private static readonly Font F_SMALL = new Font("Segoe UI", 8f);
        private static readonly Font F_BTN   = new Font("Segoe UI Semibold", 9f);
        private static readonly Font F_MONO  = new Font("Consolas", 8.5f);

        
        private Graph             _graph;
        private PerformanceTester _tester         = new PerformanceTester();
        private List<Point>       _vertexPos      = new List<Point>();
        private List<int>         _highlighted    = new List<int>();
        private Random            _rnd            = new Random();
        private bool              _hasResult      = false;
        private bool              _updatingMatrix = false;

        
        private PictureBox    _canvas;
        private NumericUpDown _nudVertices;
        private DataGridView  _dgvMatrix;
        private Button        _btnBuild, _btnGenerate;
        private Button        _btnGreedy, _btnBronKerbosch;
        private Button        _btnSave, _btnHelp, _btnClearLog;
        private RichTextBox   _logBox;
        private Label         _lblStatus;

        // ════════════════════════════════════════════════════════════════════
        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        // ════════════════════════════════════════════════════════════════════
        #region SetupUI
        // ════════════════════════════════════════════════════════════════════

        private void SetupUI()
        {
            
            this.Text            = "Пошук кліки у графі (Курсова робота)";
            this.ClientSize      = new Size(1180, 720);
            this.MinimumSize     = new Size(1196, 759);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = true;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = C_BG;
            this.Font            = F_BODY;

            
            var main = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = C_BG
            };
            this.Controls.Add(main);

            

            
            var gbSettings = MakeGroupBox("Налаштування графа", new Rectangle(10, 10, 420, 60));
            gbSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            main.Controls.Add(gbSettings);

            var lblVert = new Label
            {
                Text      = "Кількість вершин:",
                Font      = F_BODY,
                ForeColor = C_TEXT,
                Bounds    = new Rectangle(12, 22, 140, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _nudVertices = new NumericUpDown
            {
                Minimum   = 2,
                Maximum   = 12,
                Value     = 6,
                Font      = F_BODY,
                Bounds    = new Rectangle(158, 20, 58, 26),
                TextAlign = HorizontalAlignment.Center
            };
            _nudVertices.ValueChanged += NudVertices_ValueChanged;
            _nudVertices.Validating   += NudVertices_Validating;

            _btnGenerate = MakeButton("⟳  Згенерувати випадково", C_ACCENT2, Color.White);
            _btnGenerate.Bounds = new Rectangle(228, 18, 168, 30);
            _btnGenerate.Click += BtnGenerate_Click;

            _btnHelp = MakeButton("?", Color.FromArgb(100, 116, 139), Color.White);
            _btnHelp.Bounds = new Rectangle(400, 18, 14, 30);
            _btnHelp.Font   = F_SMALL;
            _btnHelp.Click += BtnHelp_Click;
            new ToolTip().SetToolTip(_btnHelp, "Довідка");

            gbSettings.Controls.AddRange(new Control[] { lblVert, _nudVertices, _btnGenerate, _btnHelp });

            
            var gbMatrix = MakeGroupBox("Матриця суміжності", new Rectangle(10, 78, 420, 368));
            gbMatrix.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            main.Controls.Add(gbMatrix);

            _dgvMatrix = new DataGridView
            {
                Bounds                      = new Rectangle(10, 24, 396, 298),
                Anchor                      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                AllowUserToResizeColumns    = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 26,
                RowHeadersWidth             = 36,
                ScrollBars                  = ScrollBars.Both,
                BorderStyle                 = BorderStyle.None,
                CellBorderStyle             = DataGridViewCellBorderStyle.Single,
                GridColor                   = C_BORDER,
                BackgroundColor             = C_PANEL,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font               = F_MONO,
                    Alignment          = DataGridViewContentAlignment.MiddleCenter,
                    BackColor          = C_PANEL,
                    ForeColor          = C_TEXT,
                    SelectionBackColor = Color.FromArgb(219, 234, 254),
                    SelectionForeColor = C_TEXT
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font               = F_MONO,
                    BackColor          = C_HEADER_BG,
                    ForeColor          = Color.White,
                    Alignment          = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = C_HEADER_BG,
                    SelectionForeColor = Color.White
                },
                RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font               = F_MONO,
                    BackColor          = C_HEADER_BG,
                    ForeColor          = Color.White,
                    Alignment          = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = C_HEADER_BG,
                    SelectionForeColor = Color.White
                },
                EnableHeadersVisualStyles = false,
                SelectionMode             = DataGridViewSelectionMode.CellSelect,
                MultiSelect               = false,
                EditMode                  = DataGridViewEditMode.EditOnKeystroke,
                AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.None,
                AutoSizeRowsMode          = DataGridViewAutoSizeRowsMode.None
            };
            _dgvMatrix.CellValueChanged += DgvMatrix_CellValueChanged;
            _dgvMatrix.CellValidating   += DgvMatrix_CellValidating;
            _dgvMatrix.CellBeginEdit    += DgvMatrix_CellBeginEdit;
            _dgvMatrix.KeyDown          += DgvMatrix_KeyDown;

            gbMatrix.Controls.Add(_dgvMatrix);

            _btnBuild = MakeButton("▶  Побудувати граф за матрицею", C_ACCENT, Color.White);
            _btnBuild.Bounds  = new Rectangle(10, 330, 396, 30);
            _btnBuild.Anchor  = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _btnBuild.Click  += BtnBuild_Click;
            gbMatrix.Controls.Add(_btnBuild);

            SyncMatrixSize();

            
            var gbLegend = MakeGroupBox("Легенда", new Rectangle(10, 454, 420, 78));
            gbLegend.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            main.Controls.Add(gbLegend);

            AddLegendItem(gbLegend, Color.FromArgb(147, 197, 253), "Звичайна вершина",       new Rectangle(12, 22, 14, 14));
            AddLegendItem(gbLegend, Color.FromArgb(251, 191,  36), "Вершина у кліці",         new Rectangle(12, 44, 14, 14));
            AddLegendItem(gbLegend, Color.FromArgb(200, 200, 210), "Звичайне ребро",           new Rectangle(210, 22, 14, 14));
            AddLegendItem(gbLegend, Color.FromArgb(239,  68,  68), "Ребро кліки (виділено)",  new Rectangle(210, 44, 14, 14));

           

            var gbGraph = MakeGroupBox("Візуалізація графа", new Rectangle(440, 8, 520, 550));
            gbGraph.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            main.Controls.Add(gbGraph);

            _canvas = new PictureBox
            {
                Bounds      = new Rectangle(10, 22, 498, 518),
                BackColor   = C_PANEL,
                BorderStyle = BorderStyle.None,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };
            _canvas.Paint += Canvas_Paint;
            _canvas.SizeChanged += Canvas_SizeChanged; 
            gbGraph.Controls.Add(_canvas);

            
            _lblStatus = new Label
            {
                Bounds    = new Rectangle(440, 566, 520, 22),
                Font      = F_SMALL,
                ForeColor = C_SUBTEXT,
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Граф не побудовано. Введіть матрицю або згенеруйте граф.",
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            main.Controls.Add(_lblStatus);

         

            
            var gbAlgo = MakeGroupBox("Алгоритми", new Rectangle(970, 8, 200, 172));
            gbAlgo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            main.Controls.Add(gbAlgo);

            var lblGreedyInfo = new Label
            {
                Text      = "Жадібний — швидкий, знаходить\nодну велику кліку (O(V²)).",
                Font      = F_SMALL,
                ForeColor = C_SUBTEXT,
                Bounds    = new Rectangle(10, 20, 178, 32),
                TextAlign = ContentAlignment.TopLeft
            };

            _btnGreedy = MakeButton("Запустити жадібний", C_ACCENT, Color.White);
            _btnGreedy.Bounds  = new Rectangle(10, 56, 178, 30);
            _btnGreedy.Enabled = false;
            _btnGreedy.Click  += BtnGreedy_Click;

            var lblBKInfo = new Label
            {
                Text      = "Брона–Кербоша — точний, знаходить\nусі максимальні кліки.",
                Font      = F_SMALL,
                ForeColor = C_SUBTEXT,
                Bounds    = new Rectangle(10, 96, 178, 32),
                TextAlign = ContentAlignment.TopLeft
            };

            _btnBronKerbosch = MakeButton("Запустити Бр.–Кербоша", C_ACCENT, Color.White);
            _btnBronKerbosch.Bounds  = new Rectangle(10, 132, 178, 30);
            _btnBronKerbosch.Enabled = false;
            _btnBronKerbosch.Click  += BtnBronKerbosch_Click;

            gbAlgo.Controls.AddRange(new Control[]
                { lblGreedyInfo, _btnGreedy, lblBKInfo, _btnBronKerbosch });

            
            var gbResults = MakeGroupBox("Результати виконання", new Rectangle(970, 188, 200, 376));
            gbResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            main.Controls.Add(gbResults);

            _logBox = new RichTextBox
            {
                Bounds      = new Rectangle(8, 22, 182, 312),
                ReadOnly    = true,
                Font        = F_MONO,
                BackColor   = Color.FromArgb(248, 250, 252),
                ForeColor   = C_TEXT,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            gbResults.Controls.Add(_logBox);

            _btnClearLog = MakeButton("Очистити журнал", Color.FromArgb(226, 232, 240), C_TEXT);
            _btnClearLog.Bounds  = new Rectangle(8, 340, 182, 26);
            _btnClearLog.Anchor  = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _btnClearLog.Font    = F_SMALL;
            _btnClearLog.Click  += (s, e) => {
                _logBox.Clear();
                _tester.ClearResults();
                _btnSave.Enabled = false;
                _highlighted.Clear();
                _canvas.Invalidate();
            };
            gbResults.Controls.Add(_btnClearLog);

            
            var gbFile = MakeGroupBox("Збереження", new Rectangle(970, 572, 200, 62));
            gbFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            main.Controls.Add(gbFile);

            _btnSave = MakeButton("💾  Зберегти результати (.txt)", Color.FromArgb(55, 65, 81), Color.White);
            _btnSave.Bounds  = new Rectangle(8, 20, 182, 30);
            _btnSave.Enabled = false;
            _btnSave.Font    = F_SMALL;
            _btnSave.Click  += BtnSave_Click;
            gbFile.Controls.Add(_btnSave);
        }

        #endregion

        
        #region Матриця суміжності та Валідація
        

        private void NudVertices_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (int.TryParse(_nudVertices.Text, out int val))
            {
                if (val > 12)
                {
                    MessageBox.Show("Максимальна кількість вершин для даної візуалізації — 12.\nЗначення буде автоматично змінено.", 
                                    "Обмеження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (val < 2)
                {
                    MessageBox.Show("Мінімальна кількість вершин графа — 2.\nЗначення буде автоматично змінено.", 
                                    "Обмеження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SyncMatrixSize()
        {
            int n = (int)_nudVertices.Value;

            _updatingMatrix = true;
            _dgvMatrix.SuspendLayout();

            int oldN = _dgvMatrix.ColumnCount;
            var saved = new int[oldN, oldN];
            for (int i = 0; i < oldN; i++)
                for (int j = 0; j < oldN; j++)
                {
                    var raw = (_dgvMatrix.Rows.Count > i && _dgvMatrix.Columns.Count > j)
                        ? _dgvMatrix.Rows[i].Cells[j].Value : null;
                    saved[i, j] = (raw != null && int.TryParse(raw.ToString(), out int x)) ? x : 0;
                }

            _dgvMatrix.Columns.Clear();
            _dgvMatrix.Rows.Clear();

            int cellW = Math.Max(28, Math.Min(48, 390 / n));
            int cellH = Math.Max(22, Math.Min(38, 292 / n));

            for (int j = 0; j < n; j++)
            {
                _dgvMatrix.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText   = j.ToString(),
                    Width        = cellW,
                    SortMode     = DataGridViewColumnSortMode.NotSortable,
                    MinimumWidth = 22
                });
            }

            for (int i = 0; i < n; i++)
            {
                _dgvMatrix.Rows.Add();
                _dgvMatrix.Rows[i].HeaderCell.Value = i.ToString();
                _dgvMatrix.Rows[i].Height           = cellH;
            }

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    var cell = _dgvMatrix.Rows[i].Cells[j];
                    if (i == j)
                    {
                        cell.Value           = "0";
                        cell.ReadOnly        = true;
                        cell.Style.BackColor = Color.FromArgb(226, 232, 240);
                        cell.Style.ForeColor = C_SUBTEXT;
                    }
                    else
                    {
                        int val = (i < oldN && j < oldN) ? saved[i, j] : 0;
                        cell.Value = val.ToString();
                        ApplyCellStyle(cell, val);
                    }
                }

            _dgvMatrix.ResumeLayout();
            _updatingMatrix = false;
        }

        private void NudVertices_ValueChanged(object sender, EventArgs e)
        {
            SyncMatrixSize();
            ResetGraphState();
        }

        private void DgvMatrix_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex == e.ColumnIndex) e.Cancel = true;
        }

        private void DgvMatrix_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex == e.ColumnIndex) return;
            string val = e.FormattedValue?.ToString() ?? "";
            if (val != "0" && val != "1")
            {
                e.Cancel = true;
                SetStatus("⚠  Допустимі значення: лише «0» або «1».", C_WARN);
            }
        }

        private void DgvMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_updatingMatrix) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex == e.ColumnIndex) return;

            var cell = _dgvMatrix.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string raw = cell.Value?.ToString() ?? "0";
            if (raw != "0" && raw != "1") raw = "0";
            int val = int.Parse(raw);

            _updatingMatrix = true;

            var mirror = _dgvMatrix.Rows[e.ColumnIndex].Cells[e.RowIndex];
            mirror.Value = raw;

            ApplyCellStyle(cell, val);
            ApplyCellStyle(mirror, val);

            _updatingMatrix = false;
        }

        private void DgvMatrix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0 ||
                e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
            {
                var cell = _dgvMatrix.CurrentCell;
                if (cell == null || cell.RowIndex == cell.ColumnIndex || cell.ReadOnly) return;
                cell.Value = (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) ? "1" : "0";
                e.Handled = true;
            }
        }

        private void ApplyCellStyle(DataGridViewCell cell, int val)
        {
            cell.Style.BackColor = val == 1 ? Color.FromArgb(219, 234, 254) : C_PANEL;
            cell.Style.ForeColor = val == 1 ? C_ACCENT : C_TEXT;
        }

        #endregion

        
        #region Обробники кнопок
        

        private void BtnBuild_Click(object sender, EventArgs e)
        {
            int n = (int)_nudVertices.Value;
            _graph = new Graph(n);

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (_dgvMatrix.Rows[i].Cells[j].Value?.ToString() == "1")
                        _graph.AddEdge(i, j);

            _highlighted.Clear();
            RecalculateVertexPositions(); 
            _canvas.Invalidate();
            EnableAlgorithmButtons(true);

            int edges = CountEdges(_graph);
            SetStatus($"✔  Граф побудовано: {n} вершин, {edges} ребер.", C_ACCENT2);
            LogLine($"=== Новий граф: {n} вершин, {edges} ребер ===");
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            int n = (int)_nudVertices.Value;

            _updatingMatrix = true;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    int val = _rnd.NextDouble() > 0.38 ? 1 : 0;
                    _dgvMatrix.Rows[i].Cells[j].Value = val.ToString();
                    _dgvMatrix.Rows[j].Cells[i].Value = val.ToString();
                    ApplyCellStyle(_dgvMatrix.Rows[i].Cells[j], val);
                    ApplyCellStyle(_dgvMatrix.Rows[j].Cells[i], val);
                }
            _updatingMatrix = false;

            BtnBuild_Click(sender, e);
        }

        private void BtnGreedy_Click(object sender, EventArgs e)
            => RunAlgorithm(new GreedyCliqueFinder());

        private void BtnBronKerbosch_Click(object sender, EventArgs e)
            => RunAlgorithm(new BronKerboschCliqueFinder());

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title    = "Зберегти результати тестування",
                Filter   = "Текстовий файл (*.txt)|*.txt",
                FileName = $"Результати_кліки_{DateTime.Now:yyyyMMdd_HHmm}.txt"
            };
            
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _tester.SaveResultsToFile(dlg.FileName);
                    SetStatus($"✔  Збережено: {Path.GetFileName(dlg.FileName)}", C_ACCENT2);
                }
                catch (IOException)
                {
                    MessageBox.Show("Не вдалося зберегти файл, оскільки він відкритий в іншій програмі.\nБудь ласка, закрийте файл і спробуйте знову.", 
                                    "Помилка доступу", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Сталася помилка при збереженні:\n{ex.Message}", 
                                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string help =
                "ДОВІДКА З ВИКОРИСТАННЯ ПРОГРАМИ\n\n" +

                "▌ ЯК ЗАДАТИ ГРАФ ВРУЧНУ\n" +
                "  1. Оберіть кількість вершин (2–12).\n" +
                "  2. Таблиця «Матриця суміжності» автоматично\n" +
                "     змінить розмір під вашу кількість вершин.\n" +
                "  3. Вводьте у клітинки «0» (немає ребра) або\n" +
                "     «1» (є ребро). Можна просто натискати\n" +
                "     клавіші 0 або 1 — значення вписується одразу.\n" +
                "     ✦ Головна діагональ заблокована (петлі\n" +
                "       заборонені).\n" +
                "     ✦ Матриця симетрична: [i,j] = [j,i]\n" +
                "       заповнюється автоматично.\n" +
                "  4. Натисніть «Побудувати граф за матрицею».\n\n" +

                "▌ ВИПАДКОВА ГЕНЕРАЦІЯ\n" +
                "  Натисніть «Згенерувати випадково» —\n" +
                "  матриця та граф заповняться автоматично.\n\n" +

                "▌ ЗАПУСК АЛГОРИТМІВ\n" +
                "  • Жадібний — O(V²), швидко знаходить\n" +
                "    одну велику кліку.\n" +
                "  • Брона–Кербоша — знаходить ВСІ\n" +
                "    максимальні кліки (точний алгоритм).\n" +
                "  Кнопки активні лише після побудови графа.\n\n" +

                "▌ ЗБЕРЕЖЕННЯ\n" +
                "  Кнопка «Зберегти результати» доступна після\n" +
                "  запуску хоча б одного алгоритму. Відкривається\n" +
                "  діалог вибору шляху для збереження .txt файлу.";

            MessageBox.Show(help, "Довідка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        
        #region Малювання графа
        

        private void RecalculateVertexPositions()
        {
            if (_graph == null) return;
            int n  = _graph.VertexCount;
            int cx = _canvas.Width  / 2;
            int cy = _canvas.Height / 2;
            int r  = (int)(Math.Min(cx, cy) * 0.78);

            _vertexPos.Clear();
            for (int i = 0; i < n; i++)
            {
                double angle = i * 2 * Math.PI / n - Math.PI / 2;
                _vertexPos.Add(new Point(
                    cx + (int)(r * Math.Cos(angle)),
                    cy + (int)(r * Math.Sin(angle))));
            }
        }

        private void Canvas_SizeChanged(object sender, EventArgs e)
        {
            RecalculateVertexPositions();
            _canvas.Invalidate();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(C_PANEL);

            if (_graph == null || _vertexPos.Count == 0)
            {
                DrawPlaceholder(g);
                return;
            }

            int n = _graph.VertexCount;

            // Ребра
            using var penEdge   = new Pen(Color.FromArgb(200, 200, 210), 1.6f);
            using var penClique = new Pen(Color.FromArgb(239, 68, 68),   2.5f);

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (_graph.HasEdge(i, j))
                    {
                        bool inClique = _highlighted.Contains(i) && _highlighted.Contains(j);
                        g.DrawLine(inClique ? penClique : penEdge,
                            _vertexPos[i], _vertexPos[j]);
                    }

            // Вершини
            int rv  = n <= 8 ? 18 : 14;
            var fnt = n <= 8
                ? new Font("Segoe UI", 9f,   FontStyle.Bold)
                : new Font("Segoe UI", 7.5f, FontStyle.Bold);

            for (int i = 0; i < n; i++)
            {
                bool hi     = _highlighted.Contains(i);
                var  cFill  = hi ? Color.FromArgb(251, 191, 36)  : Color.FromArgb(147, 197, 253);
                var  cBord  = hi ? Color.FromArgb(202, 138,  4)  : Color.FromArgb(59,  130, 246);
                var  rect   = new Rectangle(_vertexPos[i].X - rv, _vertexPos[i].Y - rv, rv * 2, rv * 2);

                using var bFill = new SolidBrush(cFill);
                g.FillEllipse(bFill, rect);
                using var pBord = new Pen(cBord, hi ? 2.5f : 1.8f);
                g.DrawEllipse(pBord, rect);

                string lbl = i.ToString();
                var    sz  = g.MeasureString(lbl, fnt);
                using var bText = new SolidBrush(C_TEXT);
                g.DrawString(lbl, fnt, bText,
                    _vertexPos[i].X - sz.Width  / 2f,
                    _vertexPos[i].Y - sz.Height / 2f);
            }
            fnt.Dispose();
        }

        private void DrawPlaceholder(Graphics g)
        {
            int w = _canvas.Width, h = _canvas.Height;
            const string msg = "Граф не побудовано.\nВведіть матрицю або натисніть\n«Згенерувати випадково».";
            using var f = new Font("Segoe UI", 10f);
            using var b = new SolidBrush(Color.FromArgb(180, 190, 210));
            var sz = g.MeasureString(msg, f, w);
            g.DrawString(msg, f, b,
                new RectangleF((w - sz.Width) / 2f, (h - sz.Height) / 2f, sz.Width, sz.Height));
        }

        #endregion

        
        #region Допоміжні методи
        

        private void RunAlgorithm(ICliqueFinder finder)
        {
            if (_graph == null) return;

            var result = _tester.Run(finder, _graph);
            var cliques = finder.FindCliques(_graph);

            LogLine($"── {result.AlgorithmName} ──");
            LogLine($"  Час виконання : {result.ElapsedMilliseconds:F4} мс");
            LogLine($"  Знайдено клік : {result.CliquesFound}");
            LogLine($"  Макс. розмір  : {result.MaxCliqueSize}");

            if (cliques.Count > 0)
            {
                var best = cliques.OrderByDescending(c => c.Count).First();
                LogLine($"  Вершини кліки : {{ {string.Join(", ", best)} }}");
                _highlighted = best;
            }
            else
            {
                LogLine("  (Клік не знайдено)");
                _highlighted.Clear();
            }
            LogLine("");

            _canvas.Invalidate();
            _hasResult       = true;
            _btnSave.Enabled = true;
            SetStatus(
                $"✔  {result.AlgorithmName}: кліка розміром {result.MaxCliqueSize} за {result.ElapsedMilliseconds:F3} мс.",
                C_ACCENT2);
        }

        private void EnableAlgorithmButtons(bool enabled)
        {
            _btnGreedy.Enabled       = enabled;
            _btnBronKerbosch.Enabled = enabled;
        }

        private void ResetGraphState()
        {
            _graph = null;
            _highlighted.Clear();
            _vertexPos.Clear();
            EnableAlgorithmButtons(false);
            _btnSave.Enabled = false;
            _hasResult       = false;
            _canvas.Invalidate();
            SetStatus("Граф не побудовано. Введіть матрицю або згенеруйте граф.", C_SUBTEXT);
        }

        private void SetStatus(string text, Color color)
        {
            _lblStatus.Text      = text;
            _lblStatus.ForeColor = color;
        }

        private void LogLine(string text)
        {
            _logBox.AppendText(text + "\n");
            _logBox.ScrollToCaret();
        }

        private static int CountEdges(Graph graph)
        {
            int count = 0, n = graph.VertexCount;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (graph.HasEdge(i, j)) count++;
            return count;
        }

        
        private static Button MakeButton(string text, Color back, Color fore)
        {
            var btn = new Button
            {
                Text                    = text,
                Font                    = new Font("Segoe UI Semibold", 9f),
                BackColor               = back,
                ForeColor               = fore,
                FlatStyle               = FlatStyle.Flat,
                Cursor                  = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor  = ControlPaint.Light(back, 0.12f);
            return btn;
        }

        
        private static GroupBox MakeGroupBox(string title, Rectangle bounds)
        {
            return new GroupBox
            {
                Text      = title,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Bounds    = bounds,
                BackColor = Color.White,
                Padding   = new Padding(6)
            };
        }

        
        private static void AddLegendItem(Control parent, Color color, string label, Rectangle iconBounds)
        {
            parent.Controls.Add(new Panel
            {
                Bounds      = iconBounds,
                BackColor   = color,
                BorderStyle = BorderStyle.FixedSingle
            });
            parent.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize  = true,
                Location  = new Point(iconBounds.Right + 4, iconBounds.Y)
            });
        }

        #endregion
    }
}
