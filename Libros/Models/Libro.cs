using System;
using System.Collections.Generic;

namespace Libros.Models;

public partial class Libro
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string Autor { get; set; } = null!;

    public string Editorial { get; set; } = null!;

    public string YearPublicacion { get; set; } = null!;

    public string Isbn { get; set; } = null!;
}
