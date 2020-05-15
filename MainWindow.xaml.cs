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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tienda_Ocho
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtbxUsuario.Focus();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtbxUsuario.Text;
            string contrasena = pwdbxContraseña.Password;

            if (usuario != "")
            {
                if (contrasena != "")
                {
                    validarUsuario(usuario, contrasena);
                    if (valoresEstaticos.accesoCorrecto == "SI")
                    {
                        wndwLogeo.Hide();
                        PantallaPrincipal wndwPantallaPrin = new PantallaPrincipal();
                        wndwPantallaPrin.Show();
                    }
                }
                else
                    MessageBox.Show("Ingresa la contraseña!", "Abarrotes Ocho");
            }
            else
                MessageBox.Show("Ingresa el Usuario", "Abarrotes Ocho");
        }

        private void validarUsuario(string usuario, string contrasena)
        {
            List<UserAcess> usuariosInput = (from row in DataSetsInputs.Usuarios.Tables[0].AsEnumerable()
                                             select new UserAcess
                                             {
                                                 id = Int32.Parse(row["Id"].ToString()),
                                                 Usuario = row["Usuario"].ToString(),
                                                 Contrasena = row["Contrasena"].ToString(),
                                                 NombreS = row["Nombres"].ToString(),
                                                 ApellidoPaterno = row["ApellidoPaterno"].ToString(),
                                                 ApellidoMaterno = row["ApellidoMaterno"].ToString(),
                                                 Perfil = row["Perfil"].ToString()
                                             }).Where(x => x.Usuario == usuario).ToList();
            int cnt = 0;
            foreach (var item in usuariosInput)
            {
                cnt++;
            }

            if (cnt == 0)
                MessageBox.Show("Usuario Invalido!!", "Abarrotes Ocho");
            else if (contrasena != usuariosInput[0].Contrasena)
                MessageBox.Show("Contraseña Invalida!!", "Abarrotes Ocho");
            else
            {
                UsuarioAcceso.id = usuariosInput[0].id;
                UsuarioAcceso.usuario = usuariosInput[0].Usuario;
                UsuarioAcceso.NombreS = usuariosInput[0].NombreS;
                UsuarioAcceso.apellidoP = usuariosInput[0].ApellidoPaterno;
                UsuarioAcceso.apellidoM = usuariosInput[0].ApellidoMaterno;
                UsuarioAcceso.Perfil = usuariosInput[0].Perfil;
                valoresEstaticos.accesoCorrecto = "SI";
                //MessageBox.Show("Bienvenido " + UsuarioAcceso.NombreS, " Abarrotes Ocho");
            }
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

                if (SheetName == "Usuarios$")
                    DataSetsInputs.Usuarios = dataset;
                if (SheetName == "Inventario$")
                    DataSetsInputs.Inventario = dataset;
            }
            catch (Exception err)
            {
                MessageBox.Show("Error"+ err, "Error");
            }
        }

        private void WndwLogeo_Loaded(object sender, RoutedEventArgs e)
        {
            valoresEstaticos.ubiProyecto = AppDomain.CurrentDomain.BaseDirectory;
            excelBaseDatos("Usuarios$");
        }

        public class UserAcess
        {
            public int id { get; set; }
            public string NumeroEmpleado { get; set; }
            public string Usuario { get; set; }
            public string Contrasena { get; set; }
            public string NombreS { get; set; }
            public string ApellidoPaterno { get; set; }
            public string ApellidoMaterno { get; set; }
            public string telefono { get; set; }
            public string correo { get; set; }
            public string Puesto { get; set; }
            public string Perfil { get; set; }
        }

        public static class DataSetsInputs
        {
            public static DataSet Usuarios { get; set; }
            public static DataSet Inventario { get; set; }
            public static DataSet Clientes { get; set; }
        }

        public static class valoresEstaticos
        {
            public static string ubiProyecto { get; set; }
            public static string accesoCorrecto { get; set; }   
        }

        public static class UsuarioAcceso
        {
            public static int id { get; set; }
            public static string usuario { get; set; }
            public static string NombreS { get; set; }
            public static string apellidoP { get; set; }
            public static string apellidoM { get; set; }
            public static string Perfil { get; set; }
        }
    }
}
