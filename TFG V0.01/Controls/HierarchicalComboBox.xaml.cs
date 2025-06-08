using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TFG_V0._01.Controls
{
    public partial class HierarchicalComboBox : UserControl, INotifyPropertyChanged
    {
        public HierarchicalComboBox()
        {
            InitializeComponent();
            // this.DataContext = this; // Eliminar para heredar el DataContext del padre
            PART_ComboBox.DropDownOpened += PART_ComboBox_DropDownOpened;
        }

        // Propiedad para enlazar la jerarquía de datos (comunidades y provincias)
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(HierarchicalComboBox), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HierarchicalComboBox;
            control?.SetTreeViewItemsSource();
            control?.HookCheckChanged();
            control?.UpdateSummaryText();
        }

        private void PART_ComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            SetTreeViewItemsSource();
            HookCheckChanged();
            UpdateSummaryText();
        }

        private void SetTreeViewItemsSource()
        {
            var treeView = GetTreeView();
            if (treeView != null && treeView.ItemsSource != ItemsSource)
            {
                treeView.ItemsSource = ItemsSource;
                treeView.UpdateLayout();
            }
        }

        // Propiedad para exponer los elementos seleccionados (más adelante)
        public IList<object> SelectedItems { get; set; } = new List<object>();

        // Resumen de la selección
        private string _summaryText = "Todas";
        public string SummaryText
        {
            get => _summaryText;
            set { _summaryText = value; OnPropertyChanged(nameof(SummaryText)); }
        }

        // Engancharse a los eventos Checked/Unchecked de los checkboxes
        private void HookCheckChanged()
        {
            if (ItemsSource == null) return;
            foreach (var comunidad in ItemsSource)
            {
                HookCheckChangedRecursive(comunidad);
            }
        }

        private void HookCheckChangedRecursive(object item)
            {
            if (item is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= Item_PropertyChanged;
                    npc.PropertyChanged += Item_PropertyChanged;
            }

            // Provincias
            var provinciasProp = item.GetType().GetProperty("Provincias");
            if (provinciasProp != null)
            {
                var provincias = provinciasProp.GetValue(item) as IEnumerable<object>;
                if (provincias != null)
                {
                    foreach (var provincia in provincias)
                        HookCheckChangedRecursive(provincia);
                }
            }
            // Sedes
            var sedesProp = item.GetType().GetProperty("Sedes");
            if (sedesProp != null)
            {
                var sedes = sedesProp.GetValue(item) as IEnumerable<object>;
                if (sedes != null)
                {
                    foreach (var sede in sedes)
                        HookCheckChangedRecursive(sede);
                }
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                UpdateSummaryText();
            }
        }

        // Actualizar el resumen de la selección
        private void UpdateSummaryText()
        {
            var seleccionados = new List<string>();
            if (ItemsSource != null)
            {
                foreach (var comunidad in ItemsSource)
                {
                    var tipo = comunidad.GetType();
                    var isCheckedProp = tipo.GetProperty("IsChecked");
                    var nombreProp = tipo.GetProperty("Nombre");
                    var provinciasProp = tipo.GetProperty("Provincias");
                    if (isCheckedProp != null && nombreProp != null)
                    {
                        bool isChecked = (bool)(isCheckedProp.GetValue(comunidad) ?? false);
                        string nombre = nombreProp.GetValue(comunidad)?.ToString() ?? "";
                        if (isChecked)
                        {
                            seleccionados.Add($"{nombre.ToUpper()}(C)");
                        }
                        // Provincias
                        if (provinciasProp != null)
                        {
                            var provincias = provinciasProp.GetValue(comunidad) as IEnumerable<object>;
                            if (provincias != null)
                            {
                                foreach (var provincia in provincias)
                                {
                                    var isCheckedP = provincia.GetType().GetProperty("IsChecked");
                                    var nombreP = provincia.GetType().GetProperty("Nombre");
                                    var sedesProp = provincia.GetType().GetProperty("Sedes");
                                    if (isCheckedP != null && nombreP != null)
                                    {
                                        bool isCheckedProv = (bool)(isCheckedP.GetValue(provincia) ?? false);
                                        string nombreProv = nombreP.GetValue(provincia)?.ToString() ?? "";
                                        if (isCheckedProv)
                                            seleccionados.Add($"{nombreProv.ToUpper()}(P)");
                                    }
                                    // Sedes
                                    if (sedesProp != null)
                                    {
                                        var sedes = sedesProp.GetValue(provincia) as IEnumerable<object>;
                                        if (sedes != null)
                                        {
                                            foreach (var sede in sedes)
                                            {
                                                var isCheckedS = sede.GetType().GetProperty("IsChecked");
                                                var nombreS = sede.GetType().GetProperty("Nombre");
                                                if (isCheckedS != null && nombreS != null)
                                                {
                                                    bool isCheckedSede = (bool)(isCheckedS.GetValue(sede) ?? false);
                                                    string nombreSede = nombreS.GetValue(sede)?.ToString() ?? "";
                                                    if (isCheckedSede)
                                                        seleccionados.Add($"{nombreSede.ToUpper()}(S)");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (seleccionados.Count == 0)
                SummaryText = "Todas";
            else
                SummaryText = string.Join(" | ", seleccionados) + " |";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // --- Utilidad para encontrar el TreeView en el árbol visual ---
        private TreeView GetTreeView()
        {
            return FindVisualChild<TreeView>(this);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
} 