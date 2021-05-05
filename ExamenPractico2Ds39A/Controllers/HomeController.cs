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
                return Json(new { result = "https://localhost:44354/Home/MiCuenta" });
            }

            return Json(new { result = false});
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
                return View();
            }
        }

        public ActionResult CerrarSession()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CuentaInfo()
        {
            cliente us = (cliente)Session["cliente"];
            var cuenta = contexto.cuenta.Where(c => c.cod_cliente == us.cod_cliente).FirstOrDefault();
            var transacciones = contexto.transacciones.Where(t => t.ncta == cuenta.ncta);
            ViewBag.cuenta = cuenta;
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