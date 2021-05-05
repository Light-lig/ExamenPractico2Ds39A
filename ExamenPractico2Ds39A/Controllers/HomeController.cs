using ExamenPractico2Ds39A.DTOS;
using ExamenPractico2Ds39A.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExamenPractico2Ds39A.Controllers
{
    public class HomeController : Controller
    {
        examen2ds39Entities contexto = new examen2ds39Entities();
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(cliente cli)
        {
            var cliente = contexto.cliente.Where(c => c.cod_cliente.Equals(cli.cod_cliente) && c.nit.Equals(cli.nit)).FirstOrDefault();
            if(cliente != null)
            {
                Session["cliente"] = cliente;
                switch (cliente.rol)
                {
                    case "cliente":
                        return Json(new { result = "/Home/MiCuenta" });
                    default:
                        return Json(new { result = "/Home/Clientes" });
                }


            }

            return Json(new { result = false});
        }

        public ActionResult Clientes()
        {
            cliente us = (cliente)Session["cliente"];
            if (us != null) {
                if (us.rol.Equals("cliente"))
                {
                    return RedirectToAction("MiCuenta");
                }
                else
                {
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult MiCuenta()
        {
            cliente us = (cliente)Session["cliente"];
            if (us == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if(us.rol == "administrador")
                {
                    return RedirectToAction("Clientes");
                }
                return View();
            }
        }

        [HttpPost]
        public ActionResult getTablaClientes()
        {
            var clientes = (from c in contexto.cliente
                            join cu in contexto.cuenta on c.cod_cliente equals cu.cod_cliente
                            select new ClienteDto()
                            {
                                cod_cliente = c.cod_cliente,
                                nombre_cliente = c.nombre_cliente,
                                nit = c.nit,
                                ncta = cu.ncta,
                                saldo = (double)cu.saldo
                            }
                                ).ToList();
            var cantidad = contexto.cliente.OrderByDescending(c => c.cod_cliente).Count();
            ViewBag.clientes = clientes;
            ViewBag.cantidad = cantidad;
            return View("TablaClientes");
        }

        public ActionResult CerrarSession()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddCliente(ClienteDto cli)
        {
            try
            {
                int id = 0;
                var cliente = contexto.cliente.OrderByDescending(c => c.cod_cliente).FirstOrDefault();
                if (cliente != null)
                {
                    id = cliente.cod_cliente + 1;
                }
                cliente cl = new cliente();
                cl.cod_cliente = id;
                cl.nombre_cliente = cli.nombre_cliente;
                cl.nit = cli.nit;
                cl.rol = "cliente";
                int idCuenta = 0;
                cuenta cu = new cuenta();
                var cuenta = contexto.cuenta.OrderByDescending(cue => cue.ncta).FirstOrDefault();
                if (cuenta != null)
                {
                    idCuenta = cuenta.ncta + 1;
                }
                cu.ncta = idCuenta;
                cu.saldo = cli.saldo;
                cu.cod_cliente = id;
                contexto.cliente.Add(cl);
                contexto.cuenta.Add(cu);
                contexto.SaveChanges();
                return Json(new { resultado = true });
            }
            catch (Exception e)
            {

                return Json(new { resultado = e.Message.ToString() });
            }
          
        }

        [HttpPost]
        public ActionResult CuentaInfo()
        {
            cliente us = (cliente)Session["cliente"];
            var cuenta = contexto.cuenta.Where(c => c.cod_cliente == us.cod_cliente).FirstOrDefault();

            var transacciones = contexto.transacciones.Where(t => t.ncta == cuenta.ncta);
            var cantidad = contexto.transacciones.Where(t => t.ncta == cuenta.ncta).Count();
            ViewBag.cuenta = cuenta;
            ViewBag.cantidad = cantidad;
            ViewBag.transacciones = transacciones;
            return PartialView("CuentaInfo");
        }
        [HttpPost]
        public ActionResult AddTransaccion(transacciones tran)
        {
            var cuenta = contexto.cuenta.Where(c => c.ncta == tran.ncta).FirstOrDefault();
            tran.fecha = DateTime.Now;
            var transaccion = contexto.transacciones.OrderByDescending(t => t.cod_transac).FirstOrDefault();
            var dias = (from t in contexto.transacciones
                        where tran.ncta == t.ncta && DbFunctions.TruncateTime(DateTime.Now) == DbFunctions.TruncateTime(t.fecha)
                        select t).Count();
            if(dias <= 10)
            {
                int id = 0;
                if (transaccion != null)
                {
                    id = transaccion.cod_transac + 1;
                }
                tran.cod_transac = id;

                if (tran.tipo == "deposito")
                {
                    cuenta.saldo = cuenta.saldo + tran.monto;
                    contexto.transacciones.Add(tran);
                    contexto.SaveChanges();
                    return Json(new { resultado = true });
                }
                else
                {
                    if (tran.monto < cuenta.saldo)
                    {
                        cuenta.saldo = cuenta.saldo - tran.monto;
                        contexto.transacciones.Add(tran);
                        contexto.SaveChanges();
                        return Json(new { resultado = true });
                    }
                    else
                    {
                        return Json(new { resultado = "El monto de la transaccion es mayor al saldo de su cuenta." });
                    }
                }
            }
            else
            {
                return Json(new { resultado = "Usted ya excedio el limite de transacciones diaras, favor intentarlo maniana." });

            }


        }

    }
}