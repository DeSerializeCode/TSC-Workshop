using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsMain
{
    public class InspectionItem
    {
        public string Item { get; set; } = string.Empty;
        public bool Completed { get; set; }
            = false;
        public string IssueCode { get; set; } = string.Empty;
    }

    public class ChecklistRecord
    {
        public string Registration { get; set; } = string.Empty;
        public DateTime SavedOn { get; set; }
            = DateTime.Now;
        public int CompletedCount { get; set; }
            = 0;
        public int IssueCount { get; set; }
            = 0;
        public List<InspectionItem> Items { get; set; }
            = new();
    }

    public class MainForm : Form
    {
        private readonly BindingList<InspectionItem> inspectionItems;
        private readonly BindingList<ChecklistRecord> savedChecklists = new();
        private readonly BindingSource savedBinding = new();

        private TextBox txtRegistration;
        private DataGridView inspectionGrid;
        private DataGridView savedGrid;
        private int inspectionPrintIndex;

        public MainForm()
        {
            inspectionItems = new BindingList<InspectionItem>(CreateInspectionItems().ToList());

            Text = "65 Point Checklist";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
            };
            tabControl.TabPages.Add(BuildChecklistPage());

            Controls.Add(tabControl);

            inspectionGrid.DataSource = inspectionItems;
            savedBinding.DataSource = savedChecklists;
            savedGrid.DataSource = savedBinding;
        }

        private TabPage BuildChecklistPage()
        {
            var page = new TabPage("Checklist")
            {
                BackColor = Color.White,
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(BuildHeaderPanel(), 0, 0);
            layout.Controls.Add(BuildBodyPanel(), 0, 1);

            page.Controls.Add(layout);
            return page;
        }

        private Control BuildHeaderPanel()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 10),
            };

            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

            txtRegistration = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "e.g. ABC123",
            };

            var btnTickAll = new Button
            {
                Text = "Tick all",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 153, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnTickAll.Click += (s, e) => TickAllInspectionItems();

            var btnClear = new Button
            {
                Text = "Clear",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnClear.Click += (s, e) => ClearChecklist();

            var btnSave = new Button
            {
                Text = "Save checklist",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnSave.Click += SaveChecklist_Click;

            var btnPrint = new Button
            {
                Text = "Print preview",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnPrint.Click += (s, e) => PrintInspectionChecklist();

            header.Controls.Add(new Label
            {
                Text = "Registration",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            }, 0, 0);
            header.Controls.Add(txtRegistration, 1, 0);
            header.Controls.Add(btnTickAll, 2, 0);
            header.Controls.Add(btnClear, 3, 0);
            header.Controls.Add(btnSave, 4, 0);
            header.Controls.Add(btnPrint, 5, 0);

            return header;
        }

        private Control BuildBodyPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            panel.Controls.Add(BuildChecklistPanel(), 0, 0);
            panel.Controls.Add(BuildSavedPanel(), 1, 0);

            return panel;
        }

        private Control BuildChecklistPanel()
        {
            var group = new GroupBox
            {
                Text = "65 point checklist",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
            };

            inspectionGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            var completedColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "✔",
                DataPropertyName = nameof(InspectionItem.Completed),
                Width = 50,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            };

            var itemColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Inspection point",
                DataPropertyName = nameof(InspectionItem.Item),
                ReadOnly = true,
                FillWeight = 70,
            };

            var issueColumn = new DataGridViewComboBoxColumn
            {
                HeaderText = "Issue (M/R)",
                DataPropertyName = nameof(InspectionItem.IssueCode),
                DataSource = new[] { string.Empty, "M", "R" },
                FlatStyle = FlatStyle.Flat,
                FillWeight = 30,
            };

            inspectionGrid.Columns.Add(completedColumn);
            inspectionGrid.Columns.Add(itemColumn);
            inspectionGrid.Columns.Add(issueColumn);

            group.Controls.Add(inspectionGrid);
            return group;
        }

        private Control BuildSavedPanel()
        {
            var group = new GroupBox
            {
                Text = "Saved checklists",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            savedGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            };

            savedGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Registration",
                DataPropertyName = nameof(ChecklistRecord.Registration),
                FillWeight = 40,
            });
            savedGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Saved on",
                DataPropertyName = nameof(ChecklistRecord.SavedOn),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" },
                FillWeight = 35,
            });
            savedGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done",
                DataPropertyName = nameof(ChecklistRecord.CompletedCount),
                FillWeight = 12,
            });
            savedGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Issues",
                DataPropertyName = nameof(ChecklistRecord.IssueCount),
                FillWeight = 13,
            });

            savedGrid.CellDoubleClick += (s, e) => LoadSelectedChecklist();

            var buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                Padding = new Padding(0, 10, 0, 0),
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnLoad = new Button
            {
                Text = "Load selected",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnLoad.Click += (s, e) => LoadSelectedChecklist();

            var btnDelete = new Button
            {
                Text = "Delete selected",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(192, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnDelete.Click += (s, e) => DeleteSelectedChecklist();

            buttons.Controls.Add(btnLoad, 0, 0);
            buttons.Controls.Add(btnDelete, 1, 0);

            layout.Controls.Add(savedGrid, 0, 0);
            layout.Controls.Add(buttons, 0, 1);

            group.Controls.Add(layout);
            return group;
        }

        private IEnumerable<InspectionItem> CreateInspectionItems()
        {
            var checklist = new[]
            {
                "Verify registration displayed",
                "VIN plate condition",
                "Wiper blades condition",
                "Windscreen cracks or chips",
                "Front left tyre tread",
                "Front right tyre tread",
                "Rear left tyre tread",
                "Rear right tyre tread",
                "Spare tyre condition",
                "Wheel nuts torque/condition",
                "Tyre pressures set",
                "Brake pad thickness front",
                "Brake pad thickness rear",
                "Brake rotor condition front",
                "Brake rotor condition rear",
                "Parking brake operation",
                "Brake fluid level",
                "Clutch operation (if manual)",
                "Steering free play",
                "Power steering fluid level",
                "Suspension noise front left",
                "Suspension noise front right",
                "Suspension noise rear left",
                "Suspension noise rear right",
                "Shock absorber leaks front",
                "Shock absorber leaks rear",
                "Ball joints and control arms",
                "Tie rod ends condition",
                "CV boots front",
                "CV boots rear",
                "Exhaust mounts and leaks",
                "Engine oil level",
                "Engine oil leaks",
                "Coolant level and condition",
                "Radiator hoses",
                "Drive belt condition",
                "Battery test and terminals",
                "Air filter condition",
                "Cabin filter condition",
                "Spark plugs/ignition leads",
                "Fuel system lines leaks",
                "Transmission fluid level",
                "Transmission leaks",
                "Differential leaks",
                "Underbody inspection",
                "Front brakes hydraulic lines",
                "Rear brakes hydraulic lines",
                "ABS warning light check",
                "Engine warning lights",
                "Service reminder reset",
                "Headlights low beam",
                "Headlights high beam",
                "Indicators/hazards",
                "Brake lights",
                "Reverse lights",
                "Fog lights",
                "Park lights",
                "Number plate lights",
                "Interior lights",
                "Horn operation",
                "Washer jets aim",
                "Seat belts condition",
                "Airbag light status",
                "Heater/Air con operation",
                "Road test completed",
            };

            return checklist.Select(item => new InspectionItem { Item = item });
        }

        private void SaveChecklist_Click(object? sender, EventArgs e)
        {
            var registration = txtRegistration.Text.Trim();

            if (string.IsNullOrWhiteSpace(registration))
            {
                MessageBox.Show("Enter a registration before saving.", "Missing registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var clonedItems = inspectionItems
                .Select(item => new InspectionItem
                {
                    Item = item.Item,
                    Completed = item.Completed,
                    IssueCode = item.IssueCode,
                })
                .ToList();

            var completedCount = clonedItems.Count(i => i.Completed);
            var issueCount = clonedItems.Count(i => !string.IsNullOrWhiteSpace(i.IssueCode));

            var existing = savedChecklists.FirstOrDefault(c =>
                c.Registration.Equals(registration, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                savedChecklists.Add(new ChecklistRecord
                {
                    Registration = registration,
                    SavedOn = DateTime.Now,
                    CompletedCount = completedCount,
                    IssueCount = issueCount,
                    Items = clonedItems,
                });
            }
            else
            {
                existing.SavedOn = DateTime.Now;
                existing.CompletedCount = completedCount;
                existing.IssueCount = issueCount;
                existing.Items = clonedItems;
                savedBinding.ResetBindings(false);
            }

            MessageBox.Show("Checklist saved against registration.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSelectedChecklist()
        {
            if (savedGrid.CurrentRow?.DataBoundItem is not ChecklistRecord record)
            {
                MessageBox.Show("Select a saved checklist to load.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtRegistration.Text = record.Registration;

            inspectionItems.Clear();
            foreach (var item in record.Items.Select(i => new InspectionItem
                     {
                         Item = i.Item,
                         Completed = i.Completed,
                         IssueCode = i.IssueCode,
                     }))
            {
                inspectionItems.Add(item);
            }
        }

        private void DeleteSelectedChecklist()
        {
            if (savedGrid.CurrentRow?.DataBoundItem is not ChecklistRecord record)
            {
                MessageBox.Show("Select a saved checklist to delete.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            savedChecklists.Remove(record);
        }

        private void TickAllInspectionItems()
        {
            foreach (var item in inspectionItems)
            {
                item.Completed = true;
            }

            inspectionGrid.Refresh();
        }

        private void ClearChecklist()
        {
            foreach (var item in inspectionItems)
            {
                item.Completed = false;
                item.IssueCode = string.Empty;
            }

            inspectionGrid.Refresh();
        }

        private void PrintInspectionChecklist()
        {
            inspectionPrintIndex = 0;

            using var printDoc = new PrintDocument
            {
                DocumentName = $"Inspection-{txtRegistration.Text.Trim()}",
            };

            printDoc.PrintPage += PrintInspectionDocument;

            try
            {
                using var preview = new PrintPreviewDialog
                {
                    Document = printDoc,
                    Width = 1000,
                    Height = 800,
                };

                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to print checklist: {ex.Message}", "Print error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                printDoc.PrintPage -= PrintInspectionDocument;
            }
        }

        private void PrintInspectionDocument(object? sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            var margin = e.MarginBounds;
            var y = margin.Top;

            using var headerFont = new Font("Segoe UI", 14, FontStyle.Bold);
            using var subHeaderFont = new Font("Segoe UI", 10, FontStyle.Regular);
            using var bodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
            using var boldFont = new Font("Segoe UI", 10, FontStyle.Bold);

            var registration = string.IsNullOrWhiteSpace(txtRegistration.Text)
                ? "N/A"
                : txtRegistration.Text.Trim();

            g.DrawString("65 Point Vehicle Check", headerFont, Brushes.Black, margin.Left, y);
            y += 28;
            g.DrawString($"Registration: {registration}", subHeaderFont, Brushes.Black, margin.Left, y);
            y += 18;
            g.DrawString($"Completed: {DateTime.Now:dd MMM yyyy}", subHeaderFont, Brushes.Black, margin.Left, y);
            y += 24;

            var checkWidth = 60;
            var issueWidth = 80;
            var itemWidth = margin.Width - checkWidth - issueWidth;

            g.DrawString("Done", boldFont, Brushes.Black, new RectangleF(margin.Left, y, checkWidth, 18));
            g.DrawString("Issue", boldFont, Brushes.Black, new RectangleF(margin.Left + checkWidth, y, issueWidth, 18));
            g.DrawString("Inspection point", boldFont, Brushes.Black, new RectangleF(margin.Left + checkWidth + issueWidth, y, itemWidth, 18));
            y += 20;

            while (inspectionPrintIndex < inspectionItems.Count)
            {
                if (y + 20 > margin.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                var item = inspectionItems[inspectionPrintIndex];
                var checkMark = item.Completed ? "✔" : "☐";
                var issue = string.IsNullOrWhiteSpace(item.IssueCode) ? "-" : item.IssueCode.ToUpperInvariant();

                g.DrawString(checkMark, bodyFont, Brushes.Black, new RectangleF(margin.Left, y, checkWidth, 18));
                g.DrawString(issue, bodyFont, Brushes.Black, new RectangleF(margin.Left + checkWidth, y, issueWidth, 18));
                g.DrawString(item.Item, bodyFont, Brushes.Black, new RectangleF(margin.Left + checkWidth + issueWidth, y, itemWidth, 18));

                y += 18;
                inspectionPrintIndex++;
            }

            inspectionPrintIndex = 0;
            e.HasMorePages = false;
        }
    }
}
