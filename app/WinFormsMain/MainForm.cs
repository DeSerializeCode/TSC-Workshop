using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsMain.Invoicing;
using WinFormsMain.VehicleLookup;

namespace WinFormsMain
{
    public class Vehicle
    {
        public string Registration { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
    }

    public class Job
    {
        public string VehicleRegistration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
            = DateTime.Today.AddDays(7);
    }

    public class Customer
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class InspectionItem
    {
        public string Item { get; set; } = string.Empty;
        public bool Completed { get; set; }
            = false;
        public string IssueCode { get; set; } = string.Empty;
    }

    public class InvoiceLine
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
            = 0m;
    }

    public class MainForm : Form
    {
        private readonly BindingList<Vehicle> vehicles = new();
        private readonly BindingList<Job> jobs = new();
        private readonly BindingSource vehicleBinding = new();
        private readonly BindingList<Customer> customers = new();
        private readonly BindingSource customerBinding = new();
        private readonly BindingList<InvoiceLine> invoiceLines = new();
        private readonly BindingList<InspectionItem> inspectionItems;
        private readonly PdfInvoiceService pdfInvoiceService;
        private readonly EmailService emailService;
        private readonly VehicleLookupController vehicleLookupController;

        private TextBox txtRegistration;
        private ComboBox cboState;
        private TextBox txtVin;
        private TextBox txtMake;
        private TextBox txtModel;
        private TextBox txtEngine;
        private TextBox txtTransmission;
        private TextBox txtOwnerName;
        private TextBox txtOwnerPhone;
        private Button btnAddVehicle;
        private DataGridView vehicleGrid;

        private ComboBox cboJobVehicle;
        private TextBox txtJobDescription;
        private DateTimePicker dtpJobDate;
        private Button btnAddJob;
        private DataGridView jobGrid;

        private ComboBox cboSmsVehicle;
        private TextBox txtSmsMessage;
        private Button btnServiceReminder;
        private Button btnBookingConfirmation;
        private Button btnLookupMotorWeb;

        private TextBox txtCustomerName;
        private TextBox txtCustomerEmail;
        private TextBox txtCustomerPhone;
        private Button btnAddCustomer;
        private DataGridView customerGrid;

        private ComboBox cboInvoiceCustomer;
        private ComboBox cboInvoiceVehicle;
        private TextBox txtInvoiceLineDescription;
        private NumericUpDown numInvoiceLineAmount;
        private Button btnAddInvoiceLine;
        private DataGridView invoiceLineGrid;
        private Label lblInvoiceTotal;
        private Button btnGenerateInvoice;
        private Button btnEmailInvoice;
        private string? lastInvoicePath;

        private TextBox txtInspectionRegistration;
        private DataGridView inspectionGrid;
        private int inspectionPrintIndex;

        public MainForm(
            VehicleLookupController vehicleLookupController,
            PdfInvoiceService pdfInvoiceService,
            EmailService emailService)
        {
            this.vehicleLookupController = vehicleLookupController
                ?? throw new ArgumentNullException(nameof(vehicleLookupController));
            this.pdfInvoiceService = pdfInvoiceService
                ?? throw new ArgumentNullException(nameof(pdfInvoiceService));
            this.emailService = emailService
                ?? throw new ArgumentNullException(nameof(emailService));

            inspectionItems = new BindingList<InspectionItem>(CreateInspectionItems().ToList());

            Text = "Workshop Scheduler";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15),
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            var vehiclePanel = BuildVehiclePanel();
            var jobPanel = BuildJobPanel();
            var smsPanel = BuildSmsPanel();

            layout.Controls.Add(vehiclePanel, 0, 0);
            layout.Controls.Add(jobPanel, 1, 0);
            layout.Controls.Add(smsPanel, 0, 1);
            layout.SetColumnSpan(smsPanel, 2);

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
            };

            var workshopPage = new TabPage("Workshop")
            {
                BackColor = Color.White,
            };
            workshopPage.Controls.Add(layout);

            var customersPage = BuildCustomersTab();
            var inspectionPage = BuildInspectionTab();

            tabControl.TabPages.Add(workshopPage);
            tabControl.TabPages.Add(customersPage);
            tabControl.TabPages.Add(inspectionPage);

            Controls.Add(tabControl);

            // Bindings for tables and selectors
            vehicleBinding.DataSource = vehicles;
            vehicleGrid.DataSource = vehicleBinding;
            cboJobVehicle.DataSource = vehicleBinding;
            cboSmsVehicle.DataSource = vehicleBinding;
            cboJobVehicle.DisplayMember = nameof(Vehicle.Registration);
            cboSmsVehicle.DisplayMember = nameof(Vehicle.Registration);
            jobGrid.DataSource = jobs;

            customerBinding.DataSource = customers;
            customerGrid.DataSource = customerBinding;
            cboInvoiceCustomer.DataSource = customerBinding;
            cboInvoiceCustomer.DisplayMember = nameof(Customer.Name);

            cboInvoiceVehicle.DataSource = vehicleBinding;
            cboInvoiceVehicle.DisplayMember = nameof(Vehicle.Registration);

            invoiceLineGrid.DataSource = invoiceLines;
            UpdateInvoiceTotal();

            inspectionGrid.DataSource = inspectionItems;
        }

        private Control BuildVehiclePanel()
        {
            var panel = new GroupBox
            {
                Text = "Vehicles",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtRegistration = CreateTextBox();
            cboState = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            cboState.Items.AddRange(new object[] { "ACT", "NSW", "NT", "QLD", "SA", "TAS", "VIC", "WA" });
            cboState.SelectedIndex = 1;
            txtVin = CreateTextBox();
            txtMake = CreateTextBox();
            txtModel = CreateTextBox();
            txtEngine = CreateTextBox();
            txtTransmission = CreateTextBox();
            txtOwnerName = CreateTextBox();
            txtOwnerPhone = CreateTextBox();

            AddLabeledControl(table, "Registration", txtRegistration, 0);
            AddLabeledControl(table, "State", cboState, 1);
            AddLabeledControl(table, "VIN", txtVin, 2);
            AddLabeledControl(table, "Make", txtMake, 3);
            AddLabeledControl(table, "Model", txtModel, 4);
            AddLabeledControl(table, "Engine", txtEngine, 5);
            AddLabeledControl(table, "Transmission", txtTransmission, 6);
            AddLabeledControl(table, "Owner Name", txtOwnerName, 7);
            AddLabeledControl(table, "Owner Phone", txtOwnerPhone, 8);

            btnAddVehicle = new Button
            {
                Text = "Add / Update Vehicle",
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnAddVehicle.Click += AddVehicle_Click;

            btnLookupMotorWeb = new Button
            {
                Text = "Lookup via MotorWeb",
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnLookupMotorWeb.Click += async (sender, args) => await LookupVehicleFromMotorWebAsync();

            vehicleGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            var lowerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
            };
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lowerPanel.Controls.Add(btnAddVehicle, 0, 0);
            lowerPanel.Controls.Add(btnLookupMotorWeb, 0, 1);
            lowerPanel.Controls.Add(vehicleGrid, 0, 2);

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            container.Controls.Add(table, 0, 0);
            container.Controls.Add(lowerPanel, 0, 1);

            panel.Controls.Add(container);
            return panel;
        }

        private Control BuildJobPanel()
        {
            var panel = new GroupBox
            {
                Text = "Jobs",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cboJobVehicle = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            txtJobDescription = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Describe the work or service",
            };

            dtpJobDate = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
            };

            AddLabeledControl(table, "Vehicle", cboJobVehicle, 0);
            AddLabeledControl(table, "Description", txtJobDescription, 1);
            AddLabeledControl(table, "Scheduled", dtpJobDate, 2);

            btnAddJob = new Button
            {
                Text = "Add Job",
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(0, 153, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnAddJob.Click += AddJob_Click;

            jobGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            var lowerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lowerPanel.Controls.Add(btnAddJob, 0, 0);
            lowerPanel.Controls.Add(jobGrid, 0, 1);

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            container.Controls.Add(table, 0, 0);
            container.Controls.Add(lowerPanel, 0, 1);

            panel.Controls.Add(container);
            return panel;
        }

        private Control BuildSmsPanel()
        {
            var panel = new GroupBox
            {
                Text = "SMS Messaging",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            cboSmsVehicle = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            txtSmsMessage = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                Height = 80,
                PlaceholderText = "Message preview...",
            };

            btnServiceReminder = new Button
            {
                Text = "Send Service Reminder",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
            };
            btnServiceReminder.Click += (s, e) => SendSms(SmsTemplate.ServiceDue);

            btnBookingConfirmation = new Button
            {
                Text = "Send Booking Confirmation",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
            };
            btnBookingConfirmation.Click += (s, e) => SendSms(SmsTemplate.BookingConfirmation);

            AddLabeledControl(table, "Vehicle", cboSmsVehicle, 0);
            table.Controls.Add(new Label
            {
                Text = "Message",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            }, 0, 1);
            table.Controls.Add(txtSmsMessage, 1, 1);
            table.SetColumnSpan(txtSmsMessage, 2);

            table.Controls.Add(btnServiceReminder, 1, 2);
            table.Controls.Add(btnBookingConfirmation, 2, 2);

            panel.Controls.Add(table);
            return panel;
        }

        private TabPage BuildCustomersTab()
        {
            var page = new TabPage("Customers & Invoices")
            {
                BackColor = Color.White,
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            layout.Controls.Add(BuildCustomerPanel(), 0, 0);
            layout.Controls.Add(BuildInvoicePanel(), 1, 0);

            page.Controls.Add(layout);
            return page;
        }

        private Control BuildCustomerPanel()
        {
            var panel = new GroupBox
            {
                Text = "Customers",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtCustomerName = CreateTextBox();
            txtCustomerEmail = CreateTextBox();
            txtCustomerPhone = CreateTextBox();

            AddLabeledControl(table, "Name", txtCustomerName, 0);
            AddLabeledControl(table, "Email", txtCustomerEmail, 1);
            AddLabeledControl(table, "Phone", txtCustomerPhone, 2);

            btnAddCustomer = new Button
            {
                Text = "Add / Update Customer",
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnAddCustomer.Click += AddCustomer_Click;

            customerGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            var lowerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lowerPanel.Controls.Add(btnAddCustomer, 0, 0);
            lowerPanel.Controls.Add(customerGrid, 0, 1);

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
            };
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            container.Controls.Add(table, 0, 0);
            container.Controls.Add(lowerPanel, 0, 1);

            panel.Controls.Add(container);
            return panel;
        }

        private Control BuildInvoicePanel()
        {
            var panel = new GroupBox
            {
                Text = "Invoices",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cboInvoiceCustomer = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            cboInvoiceVehicle = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            txtInvoiceLineDescription = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Labour, parts, or notes",
            };

            numInvoiceLineAmount = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                DecimalPlaces = 2,
                Maximum = 1000000,
                ThousandsSeparator = true,
                Increment = 10,
            };

            btnAddInvoiceLine = new Button
            {
                Text = "Add Line",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 153, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 35,
            };
            btnAddInvoiceLine.Click += AddInvoiceLine_Click;

            invoiceLineGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 220,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            lblInvoiceTotal = new Label
            {
                Text = "Total: $0.00",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            btnGenerateInvoice = new Button
            {
                Text = "Generate PDF",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
            };
            btnGenerateInvoice.Click += async (s, e) => await GenerateInvoiceAsync();

            btnEmailInvoice = new Button
            {
                Text = "Email Invoice",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
            };
            btnEmailInvoice.Click += async (s, e) => await EmailInvoiceAsync();

            AddLabeledControl(table, "Customer", cboInvoiceCustomer, 0);
            AddLabeledControl(table, "Vehicle", cboInvoiceVehicle, 1);

            AddLabeledControl(table, "Line description", txtInvoiceLineDescription, 2);
            AddLabeledControl(table, "Amount", numInvoiceLineAmount, 3);

            table.Controls.Add(btnAddInvoiceLine, 1, 4);

            var bottomPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            bottomPanel.Controls.Add(btnGenerateInvoice, 0, 0);
            bottomPanel.Controls.Add(btnEmailInvoice, 1, 0);
            bottomPanel.Controls.Add(lblInvoiceTotal, 0, 1);
            bottomPanel.SetColumnSpan(lblInvoiceTotal, 2);

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
            };
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            container.Controls.Add(table, 0, 0);
            container.Controls.Add(invoiceLineGrid, 0, 1);
            container.Controls.Add(bottomPanel, 0, 2);

            panel.Controls.Add(container);
            return panel;
        }

        private TabPage BuildInspectionTab()
        {
            var page = new TabPage("Inspection")
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

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                AutoSize = true,
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            txtInspectionRegistration = CreateTextBox();

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

            var btnPrintChecklist = new Button
            {
                Text = "Print checklist",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 36,
            };
            btnPrintChecklist.Click += (s, e) => PrintInspectionChecklist();

            header.Controls.Add(new Label
            {
                Text = "Registration",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            }, 0, 0);
            header.Controls.Add(txtInspectionRegistration, 1, 0);
            header.Controls.Add(btnTickAll, 2, 0);
            header.Controls.Add(btnPrintChecklist, 3, 0);

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

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(inspectionGrid, 0, 1);

            page.Controls.Add(layout);
            return page;
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

        private static TextBox CreateTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
            };
        }

        private static void AddLabeledControl(TableLayoutPanel table, string label, Control control, int row)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            }, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private async Task LookupVehicleFromMotorWebAsync()
        {
            var registration = txtRegistration.Text.Trim();
            var state = (cboState.SelectedItem as string)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(registration) || string.IsNullOrWhiteSpace(state))
            {
                MessageBox.Show("Enter a registration and select a state before lookup.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLookupMotorWeb.Enabled = false;
            btnLookupMotorWeb.Text = "Looking up...";

            try
            {
                var lookup = await vehicleLookupController.GetVehicleByRegoAsync(registration, state).ConfigureAwait(true);

                if (lookup is null)
                {
                    MessageBox.Show("No vehicle details were returned for this rego and state.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ApplyVehicleInfo(lookup);
                MergeVehicleRecord(lookup);
                RefreshVehicleSelectors(lookup.RegistrationNumber);
            }
            finally
            {
                btnLookupMotorWeb.Enabled = true;
                btnLookupMotorWeb.Text = "Lookup via MotorWeb";
            }
        }

        private void ApplyVehicleInfo(VehicleInfo lookup)
        {
            txtRegistration.Text = lookup.RegistrationNumber;

            var stateIndex = cboState.FindStringExact(lookup.State.ToUpperInvariant());
            if (stateIndex >= 0)
            {
                cboState.SelectedIndex = stateIndex;
            }

            txtVin.Text = lookup.Vin;
            txtMake.Text = lookup.Make;

            var modelWithBadge = string.Join(" ", new[] { lookup.Model, lookup.Badge }.Where(x => !string.IsNullOrWhiteSpace(x)));
            txtModel.Text = modelWithBadge;

            var engineDetails = string.Join(" ", new[] { lookup.EngineCapacity, lookup.Drivetrain }.Where(x => !string.IsNullOrWhiteSpace(x)));
            txtEngine.Text = engineDetails;
            txtTransmission.Text = lookup.Transmission;

            if (string.IsNullOrWhiteSpace(txtOwnerName.Text))
            {
                txtOwnerName.Text = "";
            }

            if (string.IsNullOrWhiteSpace(txtOwnerPhone.Text))
            {
                txtOwnerPhone.Text = "";
            }
        }

        private void MergeVehicleRecord(VehicleInfo lookup)
        {
            var existing = vehicles.FirstOrDefault(v => v.Registration.Equals(lookup.RegistrationNumber, StringComparison.OrdinalIgnoreCase));

            var engineDetails = string.Join(" ", new[] { lookup.EngineCapacity, lookup.Drivetrain }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (existing is null)
            {
                vehicles.Add(new Vehicle
                {
                    Registration = lookup.RegistrationNumber,
                    State = lookup.State,
                    Vin = lookup.Vin,
                    Make = lookup.Make,
                    Model = string.Join(" ", new[] { lookup.Model, lookup.Badge }.Where(x => !string.IsNullOrWhiteSpace(x))),
                    Engine = engineDetails,
                    Transmission = lookup.Transmission,
                    OwnerName = txtOwnerName.Text.Trim(),
                    OwnerPhone = txtOwnerPhone.Text.Trim(),
                });
            }
            else
            {
                existing.State = string.IsNullOrWhiteSpace(lookup.State) ? existing.State : lookup.State;
                existing.Vin = string.IsNullOrWhiteSpace(lookup.Vin) ? existing.Vin : lookup.Vin;
                existing.Make = string.IsNullOrWhiteSpace(lookup.Make) ? existing.Make : lookup.Make;
                existing.Model = string.Join(" ", new[] { lookup.Model, lookup.Badge }.Where(x => !string.IsNullOrWhiteSpace(x)))
                    .Trim();
                if (!string.IsNullOrWhiteSpace(engineDetails))
                {
                    existing.Engine = engineDetails;
                }
                existing.Transmission = string.IsNullOrWhiteSpace(lookup.Transmission) ? existing.Transmission : lookup.Transmission;
                vehicleGrid.Refresh();
            }
        }

        private void AddVehicle_Click(object? sender, EventArgs e)
        {
            var registration = txtRegistration.Text.Trim();
            var state = (cboState.SelectedItem as string)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(registration))
            {
                MessageBox.Show("Registration is required to add a vehicle.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                MessageBox.Show("Select a state for the registration.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = vehicles.FirstOrDefault(v => v.Registration.Equals(registration, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                vehicles.Add(new Vehicle
                {
                    Registration = registration,
                    State = state,
                    Vin = txtVin.Text.Trim(),
                    Make = txtMake.Text.Trim(),
                    Model = txtModel.Text.Trim(),
                    Engine = txtEngine.Text.Trim(),
                    Transmission = txtTransmission.Text.Trim(),
                    OwnerName = txtOwnerName.Text.Trim(),
                    OwnerPhone = txtOwnerPhone.Text.Trim(),
                });
            }
            else
            {
                existing.State = state;
                existing.Vin = txtVin.Text.Trim();
                existing.Make = txtMake.Text.Trim();
                existing.Model = txtModel.Text.Trim();
                existing.Engine = txtEngine.Text.Trim();
                existing.Transmission = txtTransmission.Text.Trim();
                existing.OwnerName = txtOwnerName.Text.Trim();
                existing.OwnerPhone = txtOwnerPhone.Text.Trim();
                vehicleGrid.Refresh();
            }

            ClearVehicleInputs();
            RefreshVehicleSelectors(registration);
        }

        private void AddJob_Click(object? sender, EventArgs e)
        {
            if (cboJobVehicle.SelectedItem is not Vehicle selectedVehicle)
            {
                MessageBox.Show("Select a vehicle before adding a job.", "Select vehicle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtJobDescription.Text))
            {
                MessageBox.Show("Provide a description for the job.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            jobs.Add(new Job
            {
                VehicleRegistration = selectedVehicle.Registration,
                Description = txtJobDescription.Text.Trim(),
                ScheduledDate = dtpJobDate.Value.Date,
            });

            txtJobDescription.Clear();
        }

        private void AddCustomer_Click(object? sender, EventArgs e)
        {
            var name = txtCustomerName.Text.Trim();
            var email = txtCustomerEmail.Text.Trim();
            var phone = txtCustomerPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Customer name is required.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Customer email is required to send invoices.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = customers.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                customers.Add(new Customer
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                });
            }
            else
            {
                existing.Name = name;
                existing.Phone = phone;
                customerGrid.Refresh();
            }

            txtCustomerName.Clear();
            txtCustomerEmail.Clear();
            txtCustomerPhone.Clear();

            if (customers.Any())
            {
                cboInvoiceCustomer.SelectedItem = customers.Last();
            }
        }

        private void AddInvoiceLine_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInvoiceLineDescription.Text))
            {
                MessageBox.Show("Add a description for the invoice line.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numInvoiceLineAmount.Value <= 0)
            {
                MessageBox.Show("Amount must be greater than zero.", "Invalid amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            invoiceLines.Add(new InvoiceLine
            {
                Description = txtInvoiceLineDescription.Text.Trim(),
                Amount = numInvoiceLineAmount.Value,
            });

            txtInvoiceLineDescription.Clear();
            numInvoiceLineAmount.Value = 0;
            UpdateInvoiceTotal();
        }

        private enum SmsTemplate
        {
            ServiceDue,
            BookingConfirmation
        }

        private void SendSms(SmsTemplate template)
        {
            if (cboSmsVehicle.SelectedItem is not Vehicle selectedVehicle)
            {
                MessageBox.Show("Select a vehicle to send a message.", "Select vehicle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedVehicle.OwnerPhone))
            {
                MessageBox.Show("Add an owner phone number before sending SMS.", "Missing phone", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var message = template switch
            {
                SmsTemplate.BookingConfirmation =>
                    $"Hi {selectedVehicle.OwnerName}, your booking for {selectedVehicle.Registration} is confirmed. We look forward to seeing you!",
                _ =>
                    $"Hi {selectedVehicle.OwnerName}, your {selectedVehicle.Make} {selectedVehicle.Model} ({selectedVehicle.Registration}) is due for service. Reply YES to confirm.",
            };

            txtSmsMessage.Text = message;

            var number = string.IsNullOrWhiteSpace(selectedVehicle.OwnerPhone)
                ? "(no number on file)"
                : selectedVehicle.OwnerPhone;

            MessageBox.Show(
                $"Sending to {number}:\n\n{message}",
                "SMS queued",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async Task GenerateInvoiceAsync()
        {
            if (cboInvoiceCustomer.SelectedItem is not Customer customer)
            {
                MessageBox.Show("Select a customer before generating an invoice.", "Missing customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboInvoiceVehicle.SelectedItem is not Vehicle vehicle)
            {
                MessageBox.Show("Select a vehicle before generating an invoice.", "Missing vehicle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (invoiceLines.Count == 0)
            {
                MessageBox.Show("Add at least one line item before generating an invoice.", "Missing lines", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lastInvoicePath = await pdfInvoiceService.GenerateInvoiceAsync(customer, vehicle, invoiceLines.ToList()).ConfigureAwait(true);
                MessageBox.Show($"Invoice saved to {lastInvoicePath}", "Invoice created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to generate invoice: {ex.Message}", "Invoice error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EmailInvoiceAsync()
        {
            if (cboInvoiceCustomer.SelectedItem is not Customer customer)
            {
                MessageBox.Show("Select a customer before emailing an invoice.", "Missing customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboInvoiceVehicle.SelectedItem is not Vehicle vehicle)
            {
                MessageBox.Show("Select a vehicle before emailing an invoice.", "Missing vehicle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                MessageBox.Show("The selected customer does not have an email address.", "Missing email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(lastInvoicePath) || !File.Exists(lastInvoicePath))
            {
                await GenerateInvoiceAsync().ConfigureAwait(true);
                if (string.IsNullOrEmpty(lastInvoicePath) || !File.Exists(lastInvoicePath))
                {
                    return;
                }
            }

            try
            {
                var subject = $"Invoice for {vehicle.Registration}";
                var body = $"Hi {customer.Name},\n\nAttached is the invoice for {vehicle.Make} {vehicle.Model} ({vehicle.Registration}).\n\nThank you.";
                await emailService.SendInvoiceAsync(customer.Email, subject, body, lastInvoicePath!).ConfigureAwait(true);
                MessageBox.Show($"Invoice emailed to {customer.Email}.", "Invoice sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to email invoice: {ex.Message}", "Email error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInvoiceTotal()
        {
            var total = invoiceLines.Sum(l => l.Amount);
            lblInvoiceTotal.Text = $"Total: {total:C2}";
        }

        private void TickAllInspectionItems()
        {
            foreach (var item in inspectionItems)
            {
                item.Completed = true;
            }

            inspectionGrid.Refresh();
        }

        private void PrintInspectionChecklist()
        {
            inspectionPrintIndex = 0;

            using var printDoc = new PrintDocument
            {
                DocumentName = $"Inspection-{txtInspectionRegistration.Text.Trim()}",
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

            var registration = string.IsNullOrWhiteSpace(txtInspectionRegistration.Text)
                ? "N/A"
                : txtInspectionRegistration.Text.Trim();

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

        private void RefreshVehicleSelectors(string? registrationToSelect = null)
        {
            vehicleBinding.ResetBindings(false);

            if (!string.IsNullOrWhiteSpace(registrationToSelect))
            {
                var selected = vehicles.FirstOrDefault(v =>
                    v.Registration.Equals(registrationToSelect, StringComparison.OrdinalIgnoreCase));

                if (selected is not null)
                {
                    cboJobVehicle.SelectedItem = selected;
                    cboSmsVehicle.SelectedItem = selected;
                    cboInvoiceVehicle.SelectedItem = selected;
                }
            }
        }

        private void ClearVehicleInputs()
        {
            txtRegistration.Clear();
            if (cboState.Items.Count > 0)
            {
                var defaultIndex = cboState.FindStringExact("NSW");
                cboState.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            }
            txtVin.Clear();
            txtMake.Clear();
            txtModel.Clear();
            txtEngine.Clear();
            txtTransmission.Clear();
            txtOwnerName.Clear();
            txtOwnerPhone.Clear();
        }
    }
}
