using GalaSoft.MvvmLight.Command;
using Libros.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Libros.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public List<string> Vista { get; set; }
        public string Error { get; set; } = "";
        public Libro Libro { get; set; } = new Libro();
        public Libro LibroCopia { get; set; } = new Libro();
        public ObservableCollection<Libro> Libros { get; set; } = new ObservableCollection<Libro>();
        public ICommand VistaCommand { get; set; }
        public ICommand GuardarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        Regex regex = new Regex(@"^[0-9]+$");
        public MainViewModel()
        {
            Vista = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                Vista.Add("");
            }
            Vista[0] = Vista[1] = Vista[2] = "Collapsed";
            VistaCommand = new RelayCommand<string>(Vistas);
            GuardarCommand = new RelayCommand<string>(Guardar);
            EliminarCommand = new RelayCommand(Eliminar);
            MostrarLibros();
        }

        private void Eliminar()
        {
            if (Libro != null && Libro.Titulo != null)
                Libros.Remove(Libro);
            else
                Error = "Favor de seleccionar el libro a eliminar";
            GuardarLibros();
            Actualizar();
        }

        private void GuardarLibros()
        {
            var json = JsonConvert.SerializeObject(Libros);
            File.WriteAllText("Libros.json", json);
        }

        //Agregar y editar
        private void Guardar(string tipo)
        {
            try
            {
                Error = "";
                if (Libro != null)
                    if (tipo == "Agregar")
                    {
                        if (string.IsNullOrWhiteSpace(Libro.Autor))
                            Error = "Favor de escribir el nombre del autor.";
                        if (string.IsNullOrWhiteSpace(Libro.Editorial))
                            Error = "Favor de escribir el nombre de la editorial.";
                        if (string.IsNullOrWhiteSpace(Libro.Titulo))
                            Error = "Favor de escribir el titulo del libro.";
                        if (string.IsNullOrWhiteSpace(Libro.Isbn))
                            Error = "Favor de dar el isbn correspondiente.";
                        if (!regex.IsMatch(Libro.Isbn))
                            Error = "Favor de introducir solamente numeros en el apartado de ISBN.";
                        if (string.IsNullOrWhiteSpace(Libro.YearPublicacion))
                            Error = "Favor de dar el año en el que se publico el libro.";
                        if (!regex.IsMatch(Libro.YearPublicacion))
                            Error = "Favor de introducir solamente numeros en el apartado del año.";
                        var validar = Libros.FirstOrDefault(x => x.Titulo == Libro.Titulo && x.YearPublicacion == Libro.YearPublicacion);
                        if (string.IsNullOrWhiteSpace(Error))
                            if (validar == null)
                            {
                                Libros.Add(Libro);
                                Vistas("");
                            }
                            else
                                Error = "El libro ya se encuentra registrado.";
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(LibroCopia.Autor))
                            Error = "Favor de escribir el nombre del autor.";
                        if (string.IsNullOrWhiteSpace(LibroCopia.Editorial))
                            Error = "Favor de escribir el nombre de la editorial.";
                        if (string.IsNullOrWhiteSpace(LibroCopia.Titulo))
                            Error = "Favor de escribir el titulo del libro.";
                        if (string.IsNullOrWhiteSpace(LibroCopia.Isbn))
                            Error = "Favor de dar el isbn correspondiente.";
                        if (!regex.IsMatch(LibroCopia.Isbn.ToString()))
                            Error = "Favor de introducir solamente numeros en el apartado de ISBN.";
                        if (string.IsNullOrWhiteSpace(LibroCopia.YearPublicacion))
                            Error = "Favor de dar el año en el que se publico el libro.";
                        if (!regex.IsMatch(LibroCopia.YearPublicacion.ToString()))
                            Error = "Favor de introducir solamente numeros en el apartado del año.";
                        var original = Libros.Where(x => x.Titulo == LibroCopia.Titulo && x.YearPublicacion == Libro.YearPublicacion).FirstOrDefault();
                        if (original != null && string.IsNullOrWhiteSpace(Error))
                        {
                            Libros.Remove(original);
                            Libros.Add(LibroCopia);
                            Vistas("");
                            Libro = new Libro();
                        }
                    }
                else
                {
                    Error = "Esto no deberia salir, si sale es null, revisar bindings x.x";
                }
                GuardarLibros();
                Actualizar();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
        //Mover entre usercontrols
        private void Vistas(string vista)
        {
            Error = "";
            if (((Libro != null && Libro.Titulo != null) && !string.IsNullOrWhiteSpace(vista)) || vista == "Agregar")
                switch (vista)
                {
                    case "Agregar":
                        Vista[0] = "Visible";
                        Vista[1] = "Collapsed";
                        Vista[2] = "Collapsed";
                        Libro = new Libro();
                        break;
                    case "Editar":
                        Vista[0] = "Collapsed";
                        Vista[1] = "Visible";
                        Vista[2] = "Collapsed";
                        LibroCopia = new Libro
                        {
                            Autor = Libro.Autor,
                            Titulo = Libro.Titulo,
                            Editorial = Libro.Editorial,
                            YearPublicacion = Libro.YearPublicacion,
                            Isbn = Libro.Isbn
                        };
                        break;
                    case "Eliminar":
                        Vista[0] = "Collapsed";
                        Vista[1] = "Collapsed";
                        Vista[2] = "Visible";
                        break;
                    default:
                        Vista[0] = Vista[1] = Vista[2] = "Collapsed";
                        break;
                }
            else if (string.IsNullOrWhiteSpace(vista))
                Vista[0] = Vista[1] = Vista[2] = "Collapsed";
            else
                Error = "Favor de seleccionar el libro a editar/eliminar";
            Actualizar();
        }
        public void MostrarLibros()
        {
            Libros.Clear();
            if (File.Exists("Libros.json"))
            {
                var json = File.ReadAllText("Libros.json");
                Libros = JsonConvert.DeserializeObject<ObservableCollection<Libro>>(json);
            }
            Actualizar();
        }
        //Nada, eso actualizar
        public void Actualizar(string? prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}