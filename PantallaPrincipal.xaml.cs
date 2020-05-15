using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Tienda_Ocho.MainWindow;

namespace Tienda_Ocho
{
    /// <summary>
    /// Interaction logic for PantallaPrincipal.xaml
    /// </summary>
    public partial class PantallaPrincipal : Window
    {
        public PantallaPrincipal()
        {
            InitializeComponent();
            txtbxProducto.Focus();
        }
        private void WndwPantallaPrin_Loaded(object sender, RoutedEventArgs e)
        {
            if (UsuarioAcceso.id != 0)
                lblUsuario.Content = "Usuario: " + UsuarioAcceso.apellidoP + ", " + UsuarioAcceso.NombreS;
            else
                lblUsuario.Content = "Admin";

            excelBaseDatos("Clientes$");
            excelBaseDatos("Inventario$");
            cargarInventarioInicial();

            try
            {
                List<tbClientes> ClientesInput = (from row in DataSets.Clientes.Tables[0].AsEnumerable()
                                                  select new tbClientes
                                                  {
                                                      NombreCompleto = row["NombreCompleto"].ToString()
                                                  }).Distinct().ToList();

                foreach (var item in ClientesInput)
                    cmbBxClientes.Items.Add(item.NombreCompleto);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: " + e, "Abarrotes Ocho");
            }
            btnAgregar.IsEnabled = false;
            btnEfectivo.IsEnabled = false;
            btnPagarTarjeta.IsEnabled = false;
            limpiarTodo();

            if (UsuarioAcceso.Perfil == "Admin")
                btnInventarioInicial.IsEnabled = true;
            else if (UsuarioAcceso.Perfil == "Vendedor")
                btnInventarioInicial.IsEnabled = false;
            else
                btnInventarioInicial.IsEnabled = false;

            cmbBxProducto.IsEnabled = false;
            txtBxCantidad.IsEnabled = false;
            btnIngresar.IsEnabled = false;
            cmbBxCambioProducto.IsEnabled = false;
            txtBxCambioCantidad.IsEnabled = false;
            txtBxComentarios.IsEnabled = false;
            btnCambiar.IsEnabled = false;
        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //borrar
        }

        private void CmbBxClientes_DropDownClosed(object sender, EventArgs e)
        {
            if (txtbxProducto.Text != "")
                btnAgregar.IsEnabled = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Botton Mas uno
            int cnt = valoresEstaticos.cnt;
            cnt++;
            lblCnt.Content = cnt;
            valoresEstaticos.cnt = cnt;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            pagarTodo("Efectivo");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            pagarTodo("Tarjeta");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        { //agregar
            int cnt = valoresEstaticos.cnt;
            List<Productos> mostrarProductos = (from row in valoresEstaticos.productos
                                                select new Productos
                                                {
                                                    id = row.id,
                                                    NomProducto = row.NomProducto,
                                                    Proveedor = row.Proveedor,
                                                    Descripcion = row.Descripcion,
                                                    PrecioUnitario = row.PrecioUnitario,
                                                    Cantidad = cnt,
                                                    Total = row.PrecioUnitario * cnt
                                                }).ToList();
            int paCnt = valoresEstaticos.prdctAgrgds;
            if (paCnt != 0)
            {
                List<Productos> agregados = valoresEstaticos.productosAgregados;
                mostrarProductos = mostrarProductos.Union(agregados).OrderBy(x => x.id).ToList();
            }
            paCnt++;
            valoresEstaticos.prdctAgrgds = paCnt;
            valoresEstaticos.productosAgregados = mostrarProductos;
            valoresEstaticos.ventaPorPagar = mostrarProductos;
            dtgrdProductos.ItemsSource = mostrarProductos;

            dtgrdProductos.Columns[0].Header = "Id Producto";
            dtgrdProductos.Columns[1].Header = "Producto";
            dtgrdProductos.Columns[2].Header = "Proveedor";
            dtgrdProductos.Columns[3].Header = "Descripcion";
            dtgrdProductos.Columns[4].Header = "Precio Uni";
            dtgrdProductos.Columns[5].Header = "Cantidad";
            dtgrdProductos.Columns[6].Header = "Total";
            dtgrdProductos.Columns[7].Visibility = Visibility.Hidden;
            dtgrdProductos.Columns[8].Visibility = Visibility.Hidden;

            int wid = 120;
            for (int i = 1; i <= 4; i++)
            {
                dtgrdProductos.Columns[i].Width = wid;
                dtgrdProductos.Columns[i].MinWidth = wid;
                dtgrdProductos.Columns[i].MaxWidth = wid;
            }
            double subtotal = 0;
            int cntProd = 0;
            double cntItems = 0;
            foreach (var item in mostrarProductos)
            {
                subtotal += item.Total;
                cntProd++;
                cntItems += item.Cantidad;
            }

            lblSubtotal.Content = "$ " + String.Format("{0:0.##}", subtotal); ;
            double iva = 12;
            lblIVA.Content = "I.V.A " + iva + "%";
            double ivaAmount = subtotal / iva;
            lblIVAvalue.Content = "$ " + String.Format("{0:0.##}", ivaAmount);
            lblTotal.Content = "$ " + String.Format("{0:0.##}", (subtotal + ivaAmount));
            lblProductos.Content = "Productos: " + cntProd;
            lblCantArticulos.Content = "Piezas: " + cntItems;

            btnEfectivo.IsEnabled = true;
            btnPagarTarjeta.IsEnabled = true;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Cancelar
            limpiarTodo();
        }

        public void excelBaseDatos(string SheetName)
        {
            try
            {
                OleDbConnection objConnection = new OleDbConnection();
                OleDbDataAdapter objDataAdapter = new OleDbDataAdapter();

                string tableSheet = valoresEstaticos.ubiProyecto + "Base de Datos.xlsx";
                string connString = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + tableSheet + "; Extended Properties =\"Excel 12.0 XML;HDR=YES;\"";
                objConnection.ConnectionString = connString;
                objConnection.Open();

                objDataAdapter.SelectCommand = new OleDbCommand("SELECT * FROM [" + SheetName + "]", objConnection);
                DataSet dataset = new DataSet();
                objDataAdapter.Fill(dataset);
                objDataAdapter.Dispose();
                objConnection.Close();

                if (SheetName == "Inventario$")
                    DataSets.Inventario = dataset;
                else if (SheetName == "Clientes$")
                    DataSets.Clientes = dataset;
            }
            catch (Exception err)
            {
                MessageBox.Show("Error" + err, "Error");
            }
        }
        private void TxtbxProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtbxProducto.Text == "")
                    MessageBox.Show("Ingresa un codigo de producto", "Abarrotes Ocho");
                else
                {
                    try
                    {
                        int id = int.Parse(txtbxProducto.Text);
                        List<Productos> listaInicial = (from row in DataSets.Inventario.Tables[0].AsEnumerable()
                                                        select new Productos
                                                        {
                                                            id = Int32.Parse(row["ID"].ToString()),
                                                            NomProducto = row["Producto"].ToString(),
                                                            Proveedor = row["Proveedor"].ToString(),
                                                            Descripcion = row["Descripcion"].ToString(),
                                                            PrecioUnitario = double.Parse(row["Precio"].ToString()),
                                                        }).Where(x => x.id == id).ToList();
                        int pds = 0;
                        foreach (var item in listaInicial)
                        {
                            pds++;
                        }

                        if (pds != 0)
                        {
                            lblProductoNombre.Content = listaInicial[0].NomProducto;
                            lblDescripcion.Content = listaInicial[0].Descripcion;
                            lblPrecioUnitario.Content = listaInicial[0].PrecioUnitario;
                            lblCnt.Content = "1";
                            valoresEstaticos.cnt = 1;
                            string cliente = cmbBxClientes.Text;
                            if (cliente != "")
                            {
                                btnAgregar.IsEnabled = true;
                                btnAgregar.Focus();
                            }
                            valoresEstaticos.productos = listaInicial;
                        }
                        else
                            MessageBox.Show("Id de Producto Invalido!!", "Abarrotes Ocho");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error: " + e, "Abarrotes Ocho");
                    }
                }
            }
        }
        public void limpiarTodo()
        {
            txtbxProducto.Text = "";
            lblCnt.Content = "[cnt]";
            lblDescripcion.Content = "[Descripcion]";
            lblPrecioUnitario.Content = "[Precio Unitario]";
            lblProductoNombre.Content = "[Nombre del Producto]";
            valoresEstaticos.cnt = 0;
            valoresEstaticos.prdctAgrgds = 0;
            btnAgregar.IsEnabled = false;
            cmbBxClientes.Text = "";
            lblSubtotal.Content = "[subTotal]";
            lblIVA.Content = "IVA";
            lblIVAvalue.Content = "[IVA monto]";
            lblTotal.Content = "[Total a Pagar]";
            lblProductos.Content = "[Cnt Productos]";
            lblCantArticulos.Content = "[Cnt Items]";
            btnEfectivo.IsEnabled = false;
            btnPagarTarjeta.IsEnabled = false;

            List<Productos> mostrarProductos = (from row in DataSets.Inventario.Tables[0].AsEnumerable()
                                                select new Productos
                                                {
                                                    id = Int32.Parse(row["ID"].ToString()),
                                                }).Where(x => x.id == 99).ToList();

            dtgrdProductos.ItemsSource = mostrarProductos;
            dtgrdProductos.Columns[0].Header = "Id Producto";
            dtgrdProductos.Columns[1].Header = "Producto";
            dtgrdProductos.Columns[2].Header = "Proveedor";
            dtgrdProductos.Columns[3].Header = "Descripcion";
            dtgrdProductos.Columns[4].Header = "Precio Unitario";
            dtgrdProductos.Columns[5].Header = "Cantidad";
            dtgrdProductos.Columns[6].Header = "Total";
            dtgrdProductos.Columns[7].Visibility = Visibility.Hidden;
            dtgrdProductos.Columns[8].Visibility = Visibility.Hidden;

            int wid = 120;
            for (int i = 1; i <= 4; i++)
            {
                dtgrdProductos.Columns[i].Width = wid;
                dtgrdProductos.Columns[i].MinWidth = wid;
                dtgrdProductos.Columns[i].MaxWidth = wid;
            }
            txtbxProducto.Focus();
        }

        public void pagarTodo(string tipoPago)
        {
            int ventas = valoresEstaticos.ventas;
            ventas++;
            valoresEstaticos.ventas = ventas;
            pagarVenta(tipoPago, ventas);
            limpiarTodo();
            MessageBox.Show("Pagado con " + tipoPago, "Abarrotes Ocho");
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        public void pagarVenta(string pago, int numVenta)
        {
            string usuario = UsuarioAcceso.usuario;
            string FechaHora = GetTimestamp(DateTime.Now);
            string cliente = cmbBxClientes.Text;

            List<ventaPagada> nuevaVenta = (from row in valoresEstaticos.ventaPorPagar
                                            select new ventaPagada
                                            {
                                                NumVenta = numVenta,
                                                fechaHora = FechaHora,
                                                usuario = usuario,
                                                nomCliente = cliente,
                                                idProducto = row.id,
                                                Producto = row.NomProducto,
                                                cantidad = row.Cantidad,
                                                precioUnitario = row.PrecioUnitario,
                                                total = row.Total,
                                                pago = pago
                                            }).ToList();
            List<ventaPagada> ventasAnteriores = valoresEstaticos.VentasDB;
            if (numVenta > 1)
                nuevaVenta = ventasAnteriores.Union(nuevaVenta).ToList();

            List<Productos> inventarioActual = valoresEstaticos.InventarioActual;
            foreach (var idInventario in inventarioActual)
            {
                foreach (var idVenta in nuevaVenta)
                {
                    if (idInventario.id == idVenta.idProducto)
                    {
                        idInventario.Stock = idInventario.Stock - idVenta.cantidad;
                    }
                }
            }
            valoresEstaticos.InventarioActual = inventarioActual;
            valoresEstaticos.VentasDB = nuevaVenta;
        }

        public void cargarInventarioInicial()
        {
            List<Productos> inventarioInicial = (from row in DataSets.Inventario.Tables[0].AsEnumerable()
                                                 select new Productos
                                                 {
                                                     id = Int32.Parse(row["ID"].ToString()),
                                                     NomProducto = row["Producto"].ToString(),
                                                     Proveedor = row["Proveedor"].ToString(),
                                                     Descripcion = row["Descripcion"].ToString(),
                                                     PrecioUnitario = double.Parse(row["Precio"].ToString()),
                                                     Cantidad = double.Parse(row["Precio"].ToString()) - (double.Parse(row["Precio"].ToString()) / 10),
                                                     Total = double.Parse(row["Precio"].ToString()) / 10,
                                                     Stock = Int32.Parse(row["Stock"].ToString()),
                                                     Caducidad = row["Caducidad"].ToString()
                                                 }).ToList();

            valoresEstaticos.InventarioInicial = inventarioInicial;
            if (valoresEstaticos.ventas == 0 && valoresEstaticos.ComprasRealizadas == 0)
                valoresEstaticos.InventarioActual = inventarioInicial;
        }
        private void BtnInventarioInicial_Click(object sender, RoutedEventArgs e)
        {
            cargarInventarioInicial();
            mostrarInventario("Inicial");
            lblMostrando.Content = "Inventario Inicial";
        }

        private void BtnInventarioActual_Click(object sender, RoutedEventArgs e)
        {
            mostrarInventario("Actual");
            lblMostrando.Content = "Inventario Actual";
        }

        public void mostrarInventario(string tipoInventario)
        {
            List<Productos> inventarioAmostrar;
            if (tipoInventario == "Inicial")
                inventarioAmostrar = valoresEstaticos.InventarioInicial;
            else
                inventarioAmostrar = valoresEstaticos.InventarioActual;

            DtaGrdInventario.ItemsSource = inventarioAmostrar;
            DtaGrdInventario.Columns[0].Header = "Id Producto";
            DtaGrdInventario.Columns[0].Width = 50;
            DtaGrdInventario.Columns[1].Header = "Producto";
            DtaGrdInventario.Columns[1].Width = 120;
            DtaGrdInventario.Columns[2].Header = "Proveedor";
            DtaGrdInventario.Columns[2].Width = 120;
            DtaGrdInventario.Columns[3].Header = "Descripcion";
            DtaGrdInventario.Columns[3].Width = 120;
            DtaGrdInventario.Columns[4].Header = "$ Venta";
            DtaGrdInventario.Columns[4].Width = 50;
            DtaGrdInventario.Columns[5].Header = "$ Compra";
            DtaGrdInventario.Columns[5].Width = 50;
            DtaGrdInventario.Columns[6].Header = "Ganancia";
            DtaGrdInventario.Columns[6].Width = 50;
            DtaGrdInventario.Columns[7].Header = "Existencia";
            DtaGrdInventario.Columns[7].Width = 50;
            DtaGrdInventario.Columns[8].Header = "Caducidad";
            DtaGrdInventario.Columns[8].Width = 120;
            if (txtbxBuscar.Text != "")
                txtbxBuscar.Text = "";
        }
        private void TabAltas_GotFocus(object sender, RoutedEventArgs e)
        {
            mostrarInventario("Actual");
            lblMostrando.Content = "Inventario Actual";
        }
        private void TxtbxBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtbxBuscar.Text != "")
                    {
                        int idBusqueda = Int32.Parse(txtbxBuscar.Text);
                        List<Productos> BusquedaProductos = (from row in valoresEstaticos.InventarioActual
                                                             where row.id == idBusqueda
                                                             select new Productos
                                                             {
                                                                 id = row.id,
                                                                 NomProducto = row.NomProducto,
                                                                 Proveedor = row.Proveedor,
                                                                 Descripcion = row.Descripcion,
                                                                 PrecioUnitario = row.PrecioUnitario,
                                                                 Cantidad = row.Cantidad,
                                                                 Total = row.Total,
                                                                 Stock = row.Stock,
                                                                 Caducidad = row.Caducidad
                                                             }).ToList();

                        DtaGrdInventario.ItemsSource = BusquedaProductos;
                        DtaGrdInventario.Columns[0].Header = "Id Producto";
                        DtaGrdInventario.Columns[0].Width = 50;
                        DtaGrdInventario.Columns[1].Header = "Producto";
                        DtaGrdInventario.Columns[1].Width = 120;
                        DtaGrdInventario.Columns[2].Header = "Proveedor";
                        DtaGrdInventario.Columns[2].Width = 120;
                        DtaGrdInventario.Columns[3].Header = "Descripcion";
                        DtaGrdInventario.Columns[3].Width = 120;
                        DtaGrdInventario.Columns[4].Header = "$ Venta";
                        DtaGrdInventario.Columns[4].Width = 50;
                        DtaGrdInventario.Columns[5].Header = "$ Compra";
                        DtaGrdInventario.Columns[5].Width = 50;
                        DtaGrdInventario.Columns[6].Header = "Ganancia";
                        DtaGrdInventario.Columns[6].Width = 50;
                        DtaGrdInventario.Columns[7].Header = "Existencia";
                        DtaGrdInventario.Columns[7].Width = 50;
                        DtaGrdInventario.Columns[8].Header = "Caducidad";
                        DtaGrdInventario.Columns[8].Width = 120;
                        lblMostrando.Content = "Busqueda";
                    }
                    else
                        MessageBox.Show("Ingresa Un Id de Producto", "Abarrotes Ocho");
                }
                catch (Exception)
                {
                    MessageBox.Show("Producto Invalido", "Abarrotes Ocho");
                }
            }
        }
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {//Compras

            if (valoresEstaticos.CargoProveedores != "Si")
            {
                cmbBxProveedor.Items.Clear();
                List<Productos> proveedores = (from row in valoresEstaticos.InventarioActual
                                               select new Productos
                                               {
                                                   Proveedor = row.Proveedor
                                               }).Distinct().ToList();

                var anterior = "";
                foreach (var item in proveedores)
                {
                    if (anterior != item.Proveedor.ToString())
                        cmbBxProveedor.Items.Add(item.Proveedor);
                    anterior = item.Proveedor.ToString();
                }
                valoresEstaticos.CargoProveedores = "Si";
            }
        }

        private void CmbBxProveedor_DropDownClosed(object sender, EventArgs e)
        {
            string proveedor = cmbBxProveedor.SelectedItem.ToString();
            cmbBxProducto.Items.Clear();
            List<Productos> products = (from row in valoresEstaticos.InventarioActual
                                        where row.Proveedor == proveedor
                                        select new Productos
                                        {
                                            NomProducto = row.NomProducto
                                        }).ToList();
            foreach (var item in products)
            {
                cmbBxProducto.Items.Add(item.NomProducto);
            }
            cmbBxProducto.IsEnabled = true;
            cmbBxProducto.Focus();
        }
        private void CmbBxProducto_DropDownClosed(object sender, EventArgs e)
        {
            txtBxCantidad.IsEnabled = true;
            txtBxCantidad.Focus();
        }

        private void TxtBxCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string ingresado = txtBxCantidad.Text;
                try
                {
                    int cantidad = int.Parse(ingresado);
                    valoresEstaticos.ComprasCantidad = cantidad;
                    btnIngresar.IsEnabled = true;
                    btnIngresar.Focus();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ingresa un Numero!!", "Abarrotes Ocho");
                    txtBxCantidad.Text = "";
                    txtBxCantidad.Focus();
                }
            }
        }
        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            int cr = valoresEstaticos.ComprasRealizadas;
            cr++;
            valoresEstaticos.ComprasRealizadas = cr;
            var inventario = valoresEstaticos.InventarioActual;
            string producto = cmbBxProducto.Text;
            int cant = valoresEstaticos.ComprasCantidad;
            foreach (var item in inventario)
            {
                if (producto == item.NomProducto)
                    item.Stock += cant;
            }
            valoresEstaticos.InventarioActual = inventario;
            cmbBxProducto.Items.Clear();
            txtBxCantidad.Text = "";
            btnIngresar.IsEnabled = false;
            txtBxCantidad.IsEnabled = false;
            cmbBxProducto.IsEnabled = false;
            MessageBox.Show("Productos Agregados =)", "Abarrotes Ocho");
        }
        public static class valoresEstaticos
        {
            public static int cnt { get; set; }
            public static string ubiProyecto { get; set; }
            public static int prdctAgrgds { get; set; }
            public static List<Productos> productos { get; set; }
            public static List<Productos> productosAgregados { get; set; }
            public static List<Productos> InventarioActual { get; set; }
            public static List<Productos> InventarioInicial { get; set; }
            public static List<Productos> ventaPorPagar { get; set; }
            public static List<ventaPagada> VentasDB { get; set; }
            public static int ventas { get; set; }
            public static string CargoProveedores { get; set; }
            public static string CargoCambiosProveedores { get; set; }
            public static int ComprasCantidad { get; set; }
            public static int ComprasRealizadas { get; set; }
            public static int cambiosCantidad { get; set; }
            public static string CambiosComentario { get; set; }
        }

        public class Productos
        {
            public int id { get; set; }
            public string NomProducto { get; set; }
            public string Proveedor { get; set; }
            public string Descripcion { get; set; }
            public double PrecioUnitario { get; set; }
            public double Cantidad { get; set; }
            public double Total { get; set; }
            public double Stock { get; set; }
            public string Caducidad { get; set; }
        }

        public class tbClientes
        {
            public int ID { get; set; }
            public string NombreCompleto { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string CorreoElectrónico { get; set; }
            public int NumeroTelefonico { get; set; }
            public string Direccion { get; set; }
            public string CodigoPostal { get; set; }
            public string Poblacion { get; set; }
        }

        public class ventaPagada
        {
            public int NumVenta { get; set; }
            public string fechaHora { get; set; }
            public string usuario { get; set; }
            public string nomCliente { get; set; }
            public int idProducto { get; set; }
            public string Producto { get; set; }
            public double cantidad { get; set; }
            public double precioUnitario { get; set; }
            public double total { get; set; }
            public string pago { get; set; }
        }

        public class Cambios
        {
            public int Id { get; set; }
            public string Producto { get; set; }
            public string Proveedor { get; set; }
            public string Usuario { get; set; }
            public int Cantidad { get; set; }
            public int Precio { get; set; }
            public string Comentario { get; set; }
        }

        public static class DataSets
        {
            public static DataSet Inventario { get; set; }
            public static DataSet Clientes { get; set; }
        }

        private void CmbBxCambioProveedor_DropDownClosed(object sender, EventArgs e)
        {
            string proveedor = cmbBxCambioProveedor.SelectedItem.ToString();
            cmbBxCambioProducto.Items.Clear();
            List<Productos> products = (from row in valoresEstaticos.InventarioActual
                                        where row.Proveedor == proveedor
                                        select new Productos
                                        {
                                            NomProducto = row.NomProducto
                                        }).ToList();
            foreach (var item in products)
            {
                cmbBxCambioProducto.Items.Add(item.NomProducto);
            }
            cmbBxCambioProducto.IsEnabled = true;
        }

        private void TbCambios_GotFocus(object sender, RoutedEventArgs e)
        {
            if (valoresEstaticos.CargoCambiosProveedores != "Si")
            {
                cmbBxCambioProducto.Items.Clear();
                List<Productos> proveedores = (from row in valoresEstaticos.InventarioActual
                                               select new Productos
                                               {
                                                   Proveedor = row.Proveedor
                                               }).Distinct().ToList();

                var anterior = "";
                foreach (var item in proveedores)
                {
                    if (anterior != item.Proveedor.ToString())
                        cmbBxCambioProveedor.Items.Add(item.Proveedor);
                    anterior = item.Proveedor.ToString();
                }
                valoresEstaticos.CargoCambiosProveedores = "Si";
            }
        }

        private void CmbBxCambioProducto_DropDownClosed(object sender, EventArgs e)
        {
            txtBxCambioCantidad.IsEnabled = true;
            txtBxCambioCantidad.Focus();
        }

        private void TxtBxCambioCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string ingresado = txtBxCambioCantidad.Text;
                try
                {
                    int cantidad = int.Parse(ingresado);
                    valoresEstaticos.cambiosCantidad = cantidad;
                    txtBxComentarios.IsEnabled = true;
                    txtBxComentarios.Focus();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ingresa un Numero!!", "Abarrotes Ocho");
                    txtBxCambioCantidad.Text = "";
                    txtBxCambioCantidad.Focus();
                }
            }
        }

        private void BtnCambiar_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void TxtBxComentarios_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string Comentario = txtBxComentarios.Text;
                if (Comentario != "")
                {
                    btnCambiar.IsEnabled = true;
                    btnCambiar.Focus();
                    valoresEstaticos.CambiosComentario = Comentario;
                }else
                {
                    MessageBox.Show("Debes Ingresar un Comentario!!", "Abarrotes Ocho");
                    txtBxComentarios.Focus();
                }  
            }
        }

    }
}