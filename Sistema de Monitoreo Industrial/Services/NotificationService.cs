using Sistema_de_Monitoreo_Industrial.Views;
using System;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public static class NotificationService
    {
        public static async Task EnviarEmailGmailAsync(string asunto, string mensaje)
        {

            MainWindow mainWin = Application.Current.MainWindow as Sistema_de_Monitoreo_Industrial.Views.MainWindow;

            try
            {
                var senderEmail = "jfbarban@gmail.com";
                var password = "ncycyhfoydijfgbo"; // Contraseña de 16 dígitos de Google

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Vertex-IoT System"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                mailMessage.To.Add("jfbarban@gmail.com");
                await smtpClient.SendMailAsync(mailMessage);

                mainWin.EscribirEnConsola($"[NOTIFICACIÓN] Email enviado: {asunto}\n");

            }
            catch (Exception ex)
            {

                mainWin.EscribirEnConsola($"[ERROR] No se pudo enviar el email: {ex.Message}\n");

            }
        }
    }
}