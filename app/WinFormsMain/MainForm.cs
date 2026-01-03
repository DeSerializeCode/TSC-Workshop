using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsMain
{
    public class Vehicle
    {
        public string Registration { get; set; } = string.Empty;
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

    public class MainForm : Form
    {
        private readonly BindingList<Vehicle> vehicles = new();
        private readonly BindingList<Job> jobs = new();
        private readonly BindingSource vehicleBinding = new();

        private readonly TextBox txtRegistration;
        private readonly TextBox txtVin;
        private readonly TextBox txtMake;
        private readonly TextBox txtModel;
        private readonly TextBox txtEngine;
        private readonly TextBox txtTransmission;
        private readonly TextBox txtOwnerName;
        private readonly TextBox txtOwnerPhone;
        private readonly Button btnAddVehicle;
        private readonly DataGridView vehicleGrid;

        private readonly ComboBox cboJobVehicle;
        private readonly TextBox txtJobDescription;
        private readonly DateTimePicker dtpJobDate;
        private readonly Button btnAddJob;
        private readonly DataGridView jobGrid;

        private readonly ComboBox cboSmsVehicle;
        private readonly TextBox txtSmsMessage;
        private readonly Button btnServiceReminder;
        private readonly Button btnBookingConfirmation;

        public MainForm()
        {
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

            Controls.Add(layout);

            // Bindings for tables and selectors
            vehicleBinding.DataSource = vehicles;
            vehicleGrid.DataSource = vehicleBinding;
            cboJobVehicle.DataSource = vehicleBinding;
            cboSmsVehicle.DataSource = vehicleBinding;
            cboJobVehicle.DisplayMember = nameof(Vehicle.Registration);
            cboSmsVehicle.DisplayMember = nameof(Vehicle.Registration);
            jobGrid.DataSource = jobs;
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
                RowCount = 8,
                AutoSize = true,
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtRegistration = CreateTextBox();
            txtVin = CreateTextBox();
            txtMake = CreateTextBox();
            txtModel = CreateTextBox();
            txtEngine = CreateTextBox();
            txtTransmission = CreateTextBox();
            txtOwnerName = CreateTextBox();
            txtOwnerPhone = CreateTextBox();

            AddLabeledControl(table, "Registration", txtRegistration, 0);
            AddLabeledControl(table, "VIN", txtVin, 1);
            AddLabeledControl(table, "Make", txtMake, 2);
            AddLabeledControl(table, "Model", txtModel, 3);
            AddLabeledControl(table, "Engine", txtEngine, 4);
            AddLabeledControl(table, "Transmission", txtTransmission, 5);
            AddLabeledControl(table, "Owner Name", txtOwnerName, 6);
            AddLabeledControl(table, "Owner Phone", txtOwnerPhone, 7);

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
                RowCount = 2,
            };
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lowerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lowerPanel.Controls.Add(btnAddVehicle, 0, 0);
            lowerPanel.Controls.Add(vehicleGrid, 0, 1);

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

        private void AddVehicle_Click(object? sender, EventArgs e)
        {
            var registration = txtRegistration.Text.Trim();

            if (string.IsNullOrWhiteSpace(registration))
            {
                MessageBox.Show("Registration is required to add a vehicle.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = vehicles.FirstOrDefault(v => v.Registration.Equals(registration, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                vehicles.Add(new Vehicle
                {
                    Registration = registration,
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
                }
            }
        }

        private void ClearVehicleInputs()
        {
            txtRegistration.Clear();
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
