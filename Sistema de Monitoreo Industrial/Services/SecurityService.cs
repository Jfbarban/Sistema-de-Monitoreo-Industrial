using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public class SecurityService
    {
        private const string UsersFilePath = "users.json";

        // 1. Método para verificar credenciales
        public bool Autenticar(string username, string password)
        {
            var usuarios = CargarUsuarios();
            string hashEntrada = GenerarHash(password);

            var usuarioEncontrado = usuarios.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == hashEntrada);

            if (usuarioEncontrado != null)
            {
                SessionManager.IniciarSesion(usuarioEncontrado);
                return true;
            }
            return false;
        }

        // 2. Generar el Hash SHA256 de la contraseña
        public string GenerarHash(string textoPlano)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // 3. Gestión del archivo JSON
        public List<User> CargarUsuarios()
        {
            if (!File.Exists(UsersFilePath))
            {
                // Si el archivo no existe, creamos un Admin por defecto para no quedar fuera
                var adminPorDefecto = new List<User>
                {
                    new User {
                        Username = "admin",
                        PasswordHash = GenerarHash("admin123"),
                        Role = UserRole.Administrador
                    }
                };
                GuardarUsuarios(adminPorDefecto);
                return adminPorDefecto;
            }

            string json = File.ReadAllText(UsersFilePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void GuardarUsuarios(List<User> usuarios)
        {
            string json = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(UsersFilePath, json);
        }
    }
}