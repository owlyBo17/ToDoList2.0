using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace ToDoLis2._0
{
    public partial class Form1 : Form
    {
        private List<TaskItem> tasks = new List<TaskItem>();
        private const string Filename = "tasks.json";
        
        private ListView taskListView = new ListView();
        private TextBox taskEntry = new TextBox();
        private Label counterLabel = new Label();
        
        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            LoadTasks();
            UpdateTaskList();
        }
        
        private void InitializeUI()
        {
            // Configuración básica
            this.Text = "ToDoLis 2.0 - Gestor de Tareas";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Campo de entrada
            taskEntry.Location = new Point(20, 20);
            taskEntry.Size = new Size(400, 30);
            taskEntry.Font = new Font("Segoe UI", 10);
            taskEntry.PlaceholderText = "Escribe una nueva tarea...";
            
            var addButton = new Button
            {
                Text = "➕ Agregar",
                Location = new Point(430, 20),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            addButton.Click += (s, e) => AddTask();
            
            // Lista de tareas
            taskListView.Location = new Point(20, 70);
            taskListView.Size = new Size(540, 300);
            taskListView.View = View.Details;
            taskListView.FullRowSelect = true;
            taskListView.GridLines = true;
            taskListView.Font = new Font("Segoe UI", 9);
            
            taskListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
            taskListView.Columns.Add("Tarea / Задача", 350, HorizontalAlignment.Left);
            taskListView.Columns.Add("Estado", 120, HorizontalAlignment.Center);
            
            // Botones
            var completeBtn = new Button 
            { 
                Text = "✓ Completar", 
                Location = new Point(20, 380), 
                Size = new Size(120, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9)
            };
            completeBtn.Click += (s, e) => MarkCompleted();
            
            var deleteBtn = new Button 
            { 
                Text = "🗑️ Eliminar", 
                Location = new Point(150, 380), 
                Size = new Size(120, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 9)
            };
            deleteBtn.Click += (s, e) => DeleteTask();
            
            var clearBtn = new Button 
            { 
                Text = "🧹 Limpiar Todo", 
                Location = new Point(280, 380), 
                Size = new Size(120, 35),
                BackColor = Color.LightYellow,
                Font = new Font("Segoe UI", 9)
            };
            clearBtn.Click += (s, e) => ClearAll();
            
            var saveBtn = new Button 
            { 
                Text = "💾 Guardar", 
                Location = new Point(410, 380), 
                Size = new Size(120, 35),
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 9)
            };
            saveBtn.Click += (s, e) => SaveTasks();
            
            // Contador
            counterLabel.Location = new Point(20, 430);
            counterLabel.Size = new Size(300, 20);
            counterLabel.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            counterLabel.ForeColor = Color.DarkSlateGray;
            counterLabel.Text = "Tareas: 0 | Completadas: 0";
            
            // Agregar controles
            this.Controls.Add(taskEntry);
            this.Controls.Add(addButton);
            this.Controls.Add(taskListView);
            this.Controls.Add(completeBtn);
            this.Controls.Add(deleteBtn);
            this.Controls.Add(clearBtn);
            this.Controls.Add(saveBtn);
            this.Controls.Add(counterLabel);
            
            // Evento Enter
            taskEntry.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AddTask();
                    e.Handled = true;
                }
            };
        }
        
        private void AddTask()
        {
            string taskText = taskEntry.Text.Trim();
            
            if (!string.IsNullOrWhiteSpace(taskText))
            {
                tasks.Add(new TaskItem
                {
                    Id = tasks.Count + 1,
                    Text = taskText,
                    Completed = false,
                    CreatedAt = DateTime.Now
                });
                
                taskEntry.Clear();
                taskEntry.Focus();
                UpdateTaskList();
                SaveTasks();
            }
            else
            {
                MessageBox.Show("¡Ingrese texto de la tarea!\nВведите текст задачи!", 
                    "Atención", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
        }
        
        private void UpdateTaskList()
        {
            taskListView.Items.Clear();
            
            foreach (var task in tasks)
            {
                string status = task.Completed ? "✅ Completado" : "🔄 En proceso";
                
                var item = new ListViewItem(new[] 
                {
                    task.Id.ToString(),
                    task.Text,
                    status
                });
                
                if (task.Completed)
                {
                    item.ForeColor = Color.Gray;
                    item.Font = new Font(item.Font, FontStyle.Strikeout);
                    item.BackColor = Color.FromArgb(240, 255, 240); // Verde claro
                }
                else
                {
                    item.BackColor = Color.FromArgb(255, 255, 240); // Amarillo claro
                }
                
                taskListView.Items.Add(item);
            }
            
            int completedCount = tasks.Count(t => t.Completed);
            counterLabel.Text = $"Tareas: {tasks.Count} | Completadas: {completedCount}";
        }
        
        private void MarkCompleted()
        {
            if (taskListView.SelectedItems.Count > 0)
            {
                int index = taskListView.SelectedIndices[0];
                if (index < tasks.Count)
                {
                    tasks[index].Completed = !tasks[index].Completed;
                    UpdateTaskList();
                    SaveTasks();
                }
            }
            else
            {
                MessageBox.Show("¡Seleccione una tarea!\nВыберите задачу!", 
                    "Atención", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
        }
        
        private void DeleteTask()
        {
            if (taskListView.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show(
                    "¿Eliminar tarea seleccionada?\nУдалить выбранную задачу?", 
                    "Confirmar", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    int index = taskListView.SelectedIndices[0];
                    if (index < tasks.Count)
                    {
                        tasks.RemoveAt(index);
                        
                        // Reasignar IDs
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            tasks[i].Id = i + 1;
                        }
                        
                        UpdateTaskList();
                        SaveTasks();
                    }
                }
            }
            else
            {
                MessageBox.Show("¡Seleccione una tarea!\nВыберите задачу!", 
                    "Atención", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
        }
        
        private void ClearAll()
        {
            if (tasks.Count > 0)
            {
                var result = MessageBox.Show(
                    "¿Eliminar TODAS las tareas?\nУдалить ВСЕ задачи?", 
                    "Confirmar", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    tasks.Clear();
                    UpdateTaskList();
                    SaveTasks();
                }
            }
        }
        
        private void SaveTasks()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string json = JsonSerializer.Serialize(tasks, options);
                File.WriteAllText(Filename, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
        
        private void LoadTasks()
        {
            if (File.Exists(Filename))
            {
                try
                {
                    string json = File.ReadAllText(Filename);
                    tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar:\n{ex.Message}", 
                        "Error", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    tasks = new List<TaskItem>();
                }
            }
        }
    }
    
    public class TaskItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}