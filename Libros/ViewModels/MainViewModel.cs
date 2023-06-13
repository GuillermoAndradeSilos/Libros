using GalaSoft.MvvmLight.Command;
using Libros.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly LibreriaContext cx;
        public List<string> Vista { get; set; }
        public string Error { get; set; } = "";
        public Libro Libro { get; set; } = new Libro();
        public Libro LibroCopia { get; set; } = new Libro();
        public ObservableCollection<Libro> Libros { get; set; }= new ObservableCollection<Libro>();
        public ICommand VistaCommand { get; set; }
        public ICommand GuardarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        Regex regex = new Regex(@"^\d$");
        public MainViewModel()
        {
            cx = new LibreriaContext();
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
            if (Libro != null)
            {
                cx.Libro.Remove(Libro);
                cx.SaveChanges();
                MostrarLibros();
            }
            else
                Error = "Favor de seleccionar el libro a eliminar";
            Vistas("");
            Actualizar();
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
                        if (!regex.IsMatch(Libro.YearPublicacion))
                            Error = "Favor de introducir solamente numeros en el apartado del año.";
                        if (string.IsNullOrWhiteSpace(Libro.YearPublicacion))
                            Error = "Favor de dar el año en el que se publico el libro.";
                        var validar = cx.Libro.FirstOrDefault(x => x.Titulo == Libro.Titulo && x.YearPublicacion == Libro.YearPublicacion);
                        if (string.IsNullOrWhiteSpace(Error))
                            if (validar == null)
                            {
                                cx.Libro.Add(Libro);
                                cx.SaveChanges();
                                MostrarLibros();
                                Vistas("");
                            }
                            else
                                Error = "El libro ya se encuentra registrado.";
                    }
                    else
                    {
                        //Aqui seria del editar, que es lo mismo que agregar solo que no se editara el nombre
                        //Debi haber creado un metodo para esto pero ya fue
                        if (string.IsNullOrWhiteSpace(Libro.Autor))
                            Error = "Favor de escribir el nombre del autor.";
                        if (string.IsNullOrWhiteSpace(Libro.Editorial))
                            Error = "Favor de escribir el nombre de la editorial.";
                        if (string.IsNullOrWhiteSpace(Libro.Titulo))
                            Error = "Favor de escribir el titulo del libro.";
                        if (string.IsNullOrWhiteSpace(Libro.Isbn))
                            Error = "Favor de dar el isbn correspondiente.";
                        if (regex.IsMatch(Libro.Isbn.ToString()))
                            Error = "Favor de introducir solamente numeros en el apartado de ISBN.";
                        if (string.IsNullOrWhiteSpace(Libro.YearPublicacion))
                            Error = "Favor de dar el año en el que se publico el libro.";
                        var original = cx.Libro.Where(x => x.Titulo == LibroCopia.Titulo && x.YearPublicacion == Libro.YearPublicacion).FirstOrDefault();
                        if (original != null)
                        {
                            original.Autor = LibroCopia.Autor;
                            original.Editorial = LibroCopia.Editorial;
                            original.Titulo = LibroCopia.Titulo;
                            original.Isbn = LibroCopia.Isbn;
                            original.YearPublicacion = LibroCopia.YearPublicacion;
                        }
                        if (string.IsNullOrWhiteSpace(Error))
                        {
                            cx.Libro.Update(original);
                            cx.SaveChanges();
                            MostrarLibros();
                            Vistas("");
                            Libro = new Libro();
                        }
                    }
                else
                {
                    Error = "Esto no deberia salir, si sale es null, regvisar bindings x.x";
                }
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

        //Mostrar todas las pinacotecas
        public void MostrarLibros()
        {
            Libros.Clear();
            var pinas = cx.Libro.OrderBy(x => x.YearPublicacion);
            pinas.ForEachAsync(x => Libros.Add(x));
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