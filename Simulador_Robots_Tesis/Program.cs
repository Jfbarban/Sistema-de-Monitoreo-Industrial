using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;

namespace Simulador_Robots_Tesis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .Build();

            Console.WriteLine("=== SIMULADOR DE ROBOTS FARMACÉUTICOS v2 (ESTADOS DINÁMICOS) ===");

            try
            {
                await mqttClient.ConnectAsync(options);
                Console.WriteLine("Conectado al Broker MQTT.");

                Random rnd = new Random();
                string[] robots = { "Envasado-01", "Etiquetado-02" };

                while (true)
                {
                    foreach (var robotId in robots)
                    {
                        // --- LÓGICA DE ESTADO DINÁMICO ---
                        // Generamos un número aleatorio para decidir el estado
                        int chance = rnd.Next(0, 100);
                        string estado;
                        if (chance > 95) estado = "FALLA";        // 5% de probabilidad de falla
                        else if (chance > 85) estado = "MANTENIMIENTO"; // 10% de mantenimiento
                        else estado = "PRODUCCION";              // 85% operativo

                        var payload = new
                        {
                            id_robot = robotId,
                            lote_produccion = "LOTE-2025-X100",
                            estado_operacional = estado, // <--- CAMBIO: Ahora es dinámico

                            // VARIABLES DE PROCESO (FIELDS)
                            temperatura_motor_C = robotId.Contains("Envasado") ? 38 + rnd.NextDouble() * 5 : 34 + rnd.NextDouble() * 4,
                            conteo_piezas = (estado == "PRODUCCION") ? rnd.Next(1, 10) : 0, // Si no produce, el conteo es 0
                            oee = (estado == "PRODUCCION") ? 88 + rnd.NextDouble() * 7 : 40 + rnd.NextDouble() * 10,

                            // NUEVAS VARIABLES PARA LOS WIDGETS PROFESIONALES
                            vibracion_mm_s = (estado == "PRODUCCION") ? 1.5 + rnd.NextDouble() * 2 : 0.2,
                            consumo_kwh = (estado == "PRODUCCION") ? 12.5 + rnd.NextDouble() : 1.2,
                            latencia_ms = rnd.Next(5, 25),

                            timestamp_robot_utc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        };

                        string jsonPayload = JsonSerializer.Serialize(payload);

                        var message = new MqttApplicationMessageBuilder()
                            .WithTopic("industria/telemetria")
                            .WithPayload(jsonPayload)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .Build();

                        await mqttClient.PublishAsync(message);

                        // Consola con colores para identificar estados rápidamente
                        Console.ForegroundColor = estado == "PRODUCCION" ? ConsoleColor.Green :
                                                 estado == "MANTENIMIENTO" ? ConsoleColor.Yellow : ConsoleColor.Red;

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {robotId} | Estado: {estado} | Lote: {payload.lote_produccion}");
                        Console.ResetColor();
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}