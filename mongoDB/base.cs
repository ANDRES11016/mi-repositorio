// Modelo de Factura (Factura.cs)
public class Factura
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string IdCliente { get; set; }
    public string NumeroFactura { get; set; }
    public DateTime FechaEmision { get; set; }
    public string Estado { get; set; } // "primerrecordatorio", "segundorecordatorio", "desactivado"
    public string EmailCliente { get; set; }
}

// Servicio de MongoDB (MongoDbService.cs)
public class MongoDbService
{
    private readonly IMongoCollection<Factura> _facturasCollection;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _facturasCollection = database.GetCollection<Factura>("Facturas");
    }

    public async Task<List<Factura>> ObtenerFacturasPorEstado(string estado)
    {
        return await _facturasCollection.Find(factura => factura.Estado == estado).ToListAsync();
    }

    public async Task ActualizarEstadoFactura(string id, string nuevoEstado)
    {
        var filter = Builders<Factura>.Filter.Eq(factura => factura.Id, id);
        var update = Builders<Factura>.Update.Set(factura => factura.Estado, nuevoEstado);

        await _facturasCollection.UpdateOneAsync(filter, update);
    }
}

// Clase de configuración de MongoDB (MongoDbSettings.cs)
public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName factura{ get; set; }
}



// Servicio de Correo Electrónico (EmailService.cs)
public class EmailService
{
    public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
    {
        // Implementa el envío de correo utilizando MailKit o SmtpClient
    }
}



// Servicio de Recordatorio (RecordatorioService.cs)
public class RecordatorioService
{
    private readonly MongoDbService _mongoDbService;
    private readonly EmailService _emailService;

    public RecordatorioService(MongoDbService mongoDbService, EmailService emailService)
    {
        _mongoDbService = mongoDbService;
        _emailService = emailService;
    }

    public async Task ProcesarRecordatorios()
    {
        var facturasPrimerRecordatorio = await _mongoDbService.ObtenerFacturasPorEstado("primerrecordatorio");
        foreach (var factura in facturasPrimerRecordatorio)
        {
            await _emailService.EnviarCorreo(factura.EmailCliente, "Recordatorio de Factura", "Su factura ha pasado a segundo recordatorio.");
            await _mongoDbService.ActualizarEstadoFactura(factura.Id, "segundorecordatorio");
        }

        var facturasSegundoRecordatorio = await _mongoDbService.ObtenerFacturasPorEstado("segundorecordatorio");
        foreach (var factura in facturasSegundoRecordatorio)
        {
            await _emailService.EnviarCorreo(factura.EmailCliente, "Desactivación de Factura", "Su factura será desactivada.");
            await _mongoDbService.ActualizarEstadoFactura(factura.Id, "desactivado");
        }
    }
}



// Controlador de Recordatorio (RecordatorioController.cs)
[ApiController]
[Route("[controller]")]
public class RecordatorioController : ControllerBase
{
    private readonly RecordatorioService _recordatorioService;

    public RecordatorioController(RecordatorioService recordatorioService)
    {
        _recordatorioService = recordatorioService;
    }

    [HttpPost("procesar")]
    public async Task<IActionResult> ProcesarRecordatorios()
    {
        await _recordatorioService.ProcesarRecordatorios();
        return Ok();
    }
}



// Ejemplo de prueba unitaria para RecordatorioService
[Fact]
public async Task ProcesarRecordatorios_DebeActualizarEstadosYEnviarCorreos()
{
    // Configura mocks para MongoDbService y EmailService
    // Llama a RecordatorioService.ProcesarRecordatorios()
    // Verifica que los métodos de los mocks se llamaron correctamente
}