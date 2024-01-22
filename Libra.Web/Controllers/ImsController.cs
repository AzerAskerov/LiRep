using System;
using System.Net;
using System.Web.Mvc;
using Libra.Contract;
using Libra.Contract.Models;
using Libra.Services;
//using System.Web.Http;

namespace Libra.Web.Controllers
{
    public class ImsController : Controller
    {
        public IInvoiceService InvoiceService { get; set; }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportInvoices()
        {
            try
            {
                byte[] buffer = new byte[Request.ContentLength];
                System.IO.Stream stream = Request.GetBufferedInputStream();
                int readbytes = stream.Read(buffer, 0, buffer.Length);
                string xml = System.Text.Encoding.UTF8.GetString(buffer);
                DbLogHelper.CreateLogRecord(xml, null, null, "Ims/ImportInvoices", null, null);
                InvoiceService.ImportInvoices(xml);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Content(ex.ToString(), "plain/text", System.Text.Encoding.UTF8);
            }
        }
    }
}