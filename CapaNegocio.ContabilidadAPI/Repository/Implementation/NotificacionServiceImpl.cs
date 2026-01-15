using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class NotificacionServiceImpl : INotificacionService
    {
        private readonly IGeneralService _generalService;
        private readonly ISviaticoService _sviaticoService;

        public NotificacionServiceImpl(IGeneralService general, ISviaticoService sviaticoService)
        {
            _generalService = general;
            _sviaticoService = sviaticoService;
        }
        public async Task<bool> SendMail(int id)
        {
            var cabecera = await _sviaticoService.GetSviaticoCabecera(id);
            var empleado = await _generalService.GetEmpleado(cabecera.Data.SvEmpDni);
            var DniEmpleadoResponsable = (string.IsNullOrEmpty(empleado.U_MVT_RESPONSABLE)) ? "" : empleado.U_MVT_RESPONSABLE;
            var empleadoResponsable = (string.IsNullOrEmpty(DniEmpleadoResponsable)) ? null : await _generalService.GetEmpleado(DniEmpleadoResponsable);
            var viewModel = new ViaticoViewModel();
            viewModel.SviaticoCabecera = cabecera.Data;
            viewModel.Empleado = empleado;

            string SMTP = "smtp.office365.com";
            string Usuario = "notificaciones@movitecnica.com.pe";
            string Contraseña = "NTmvt$1604";

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Correo", "_EmailTemplate.cshtml");
            string body = await EmailTemplateHelper.GetEmailTemplateAsync(filePath, viewModel);

            string asunto = ValidateAsunto(cabecera.Data);
            string destinatario = GetCorreoEmpleado(empleado.U_MVT_CORREO, empleado.U_MVT_CORREOPERSONAL);
            string copiaCorreo = (empleadoResponsable != null) ? GetCorreoEmpleado(empleadoResponsable.U_MVT_CORREO, empleadoResponsable.U_MVT_CORREOPERSONAL) : "" ;


            //Declaro la variable para enviar el correo
            System.Net.Mail.MailMessage correo = new System.Net.Mail.MailMessage();
            correo.From = new System.Net.Mail.MailAddress(Usuario);
            correo.Subject = asunto;
            correo.Priority = MailPriority.High;
            correo.IsBodyHtml = true;

            //Aquien va dirigido
            correo.To.Add(destinatario);
            correo.CC.Add(copiaCorreo);
            correo.Body = body;

            //Configuracion del servidor
            //Creamos un objeto de cliente de correo
            System.Net.Mail.SmtpClient Servidor = new System.Net.Mail.SmtpClient();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            Servidor.Port = 587; //587;
            Servidor.EnableSsl = true; //true;
            Servidor.DeliveryMethod = SmtpDeliveryMethod.Network;
            Servidor.UseDefaultCredentials = false;

            //Hay que crear las credenciales del correo emisor
            Servidor.Credentials = new System.Net.NetworkCredential(Usuario, Contraseña);

            Servidor.Host = SMTP;

            try
            {
                //Enviamos el mensaje      
                Servidor.Send(correo);
                return true;
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                //Aquí gestionamos los errores al intentar enviar el correo, atualizamos campo Envia Correo al Cliente
                Console.WriteLine("Error al enviar correo: " + ex.Message);
                return false;
            }
        }

        public string ValidateAsunto(SviaticosCabeceraDTOResponse sviaticoCabecera)
        {
            var asunto = "";
            switch (sviaticoCabecera.SvSefId)
            {
                case 1:
                    asunto = $"Solicitud  de viático nro. {sviaticoCabecera.SvNumero} ha sido creada.";
                    break;

                case 2: break;
                case 3: break;
                case 4: break;
                case 5: break;
            }
            return asunto;
        }

        public string GetCorreoEmpleado(string correo, string correo2)
        {
            if (!string.IsNullOrEmpty(correo))
                return correo;
            else if (!string.IsNullOrEmpty(correo2))
                return correo2;
            else
                return "";
        }
    }
}
