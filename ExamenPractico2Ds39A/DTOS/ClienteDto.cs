using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExamenPractico2Ds39A.DTOS
{
    public class ClienteDto
    {
        public int cod_cliente { get; set; }
        public string nombre_cliente { get; set; }
        public string nit { get; set; }
        public string rol { get; set; }

        public int ncta { get; set; }
        public double saldo { get; set; }
    }
}