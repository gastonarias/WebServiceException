
namespace Console.Logger
{
    public class WebServiceException : Exception
    {
        #region Internal
        private static readonly Dictionary<WSCodError, ItemError> Errores;

        private class ItemError
        {
            public HttpStatusCode Estado { get; set; }
            public string Novedad { get; set; }
        }

        static WSException()
        {
            Errores = new Dictionary<WSCodError, ItemError>();

            Errores.Add(WSCodError.ParametroObligatorio,
                new ItemError { Estado = HttpStatusCode.BadRequest, Novedad = "Parámetro {0} obligatorio" });
            Errores.Add(WSCodError.ParametroInvalido,
                new ItemError { Estado = HttpStatusCode.BadRequest, Novedad = "Parámetro {0} inválido" });
            Errores.Add(WSCodError.ErrorInternoEnWeb,
                new ItemError { Estado = HttpStatusCode.InternalServerError, Novedad = "Error interno. Ref={0}" });
            Errores.Add(WSCodError.ErrorInternoEnMotor,
                new ItemError { Estado = HttpStatusCode.InternalServerError, Novedad = "Error interno. Ref={0}" });
            Errores.Add(WSCodError.CredencialesInvalidas,
              new ItemError { Estado = HttpStatusCode.Unauthorized, Novedad = "Credenciales inválidas" });
            Errores.Add(WSCodError.AmbienteNoAutorizado,
              new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Ambiente no autorizado" });
            Errores.Add(WSCodError.ProtocoloNoAutorizado,
             new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Protocolo {0} no autorizado" });
            Errores.Add(WSCodError.EndpointNoAutorizado,
           new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Acceso {0} no autorizado" });
            Errores.Add(WSCodError.ServicioDeshabilitado,
              new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Servicio {0} no autorizado" });
            Errores.Add(WSCodError.IPNoAutorizada,
              new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Origen no autorizado" });
            Errores.Add(WSCodError.LimiteExcedido,
             new ItemError { Estado = HttpStatusCode.Forbidden, Novedad = "Límite de concurrencia excedido" });
        }

        private static string FormatearNovedadInterna(WSCodError codError, params object[] args)
        {
            var item = Errores[codError];
            var sb = new StringBuilder();

            sb.AppendFormat("(E{0:00}) ", (int)codError);
            sb.AppendFormat(item.Novedad, args);

            return sb.ToString();
        }

        private static string FormatearNovedad(WSCodError codError, string novedad)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("(E{0:00}) ", (int)codError);
            sb.AppendFormat(novedad);

            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Devuelve el estado que se informa al WebService
        /// Referente al status code de HTTP
        /// </summary>
        public HttpStatusCode Estado { get; private set; }

        /// <summary>
        /// Devuelve la novedad que se informa al WebService.
        /// Incluye el código de error formateado como (EXX)
        /// </summary>
        public string Novedad { get; private set; }

        /// <summary>
        /// Devuelve el código de referencia (GUID) para identificar errores internos
        /// </summary>
        public string Referencia { get; private set; }

        /// <summary>
        /// Constructor privado usado para helper de creación
        /// </summary>
        private WSException(HttpStatusCode estado, string novedad)
            : base(novedad)
        {
            this.Estado = estado;
            this.Novedad = novedad;
        }

        /// <summary>
        /// Constructor general para todos los códigos de error (excepto los errores internos)
        /// </summary>
        public WSException(WSCodError codError, params object[] args)
            : base(FormatearNovedadInterna(codError, args))
        {
            var item = Errores[codError];
            this.Estado = item.Estado;
            this.Novedad = FormatearNovedadInterna(codError, args);
        }

        /// <summary>
        /// Helper de creación para los errores internos
        /// </summary>
        public static WSException CrearErrorInterno(WSCodError codError)
        {
            var referencia = Guid.NewGuid().ToString();
            var novedad = FormatearNovedadInterna((WSCodError)codError, referencia);
            return new WSException(HttpStatusCode.InternalServerError, novedad) { Referencia = referencia };
        }

        /// <summary>
        /// Helper de creación para los errores de validación de negocio
        /// </summary>
        public static WSException CrearErrorNegocio(int codError, string mensaje, params object[] args)
        {
            var mensajeFormateado = string.Format(mensaje, args);
            var novedad = FormatearNovedad((WSCodError)codError, mensajeFormateado);
            return new WSException((HttpStatusCode)422, novedad);
        }
    }

    /// <summary>
    /// Códigos de error
    /// </summary>
    public enum WSCodError
    {
        /// <summary>
        /// 400 BAD REQUEST - Parámetro obligatorio
        /// </summary>
        ParametroObligatorio = 1,
        /// <summary>
        /// 400 BAD REQUEST - Parámetro inválido
        /// </summary>
        ParametroInvalido = 2,
        /// <summary>
        /// 401 UNAUTHORIZED - Usuario o clave inválidos
        /// </summary>
        CredencialesInvalidas = 10,
        /// <summary>
        /// 403 FORBIDDEN - El cliente no tiene permiso para acceder al servicio
        /// </summary>
        UsuarioNoAutorizado = 11,
        /// <summary>
        /// 403 FORBIDDEN - El cliente tiene configurado restricciones de IP
        /// </summary>
        IPNoAutorizada = 12,
        /// <summary>
        /// 403 FORBIDDEN - El cliente está intentando ingresar a un ambiente productivo con credenciales de desarrollo
        /// </summary>
        AmbienteNoAutorizado = 13,
        /// <summary>
        /// 403 FORBIDDEN - No implementado
        /// </summary>
        ProtocoloNoAutorizado = 14,
        /// <summary>
        /// 403 FORBIDDEN - No implementado
        /// </summary>
        EndpointNoAutorizado = 15,
        /// <summary>
        /// 403 FORBIDDEN - Servicio no habilitado
        /// </summary>
        ServicioDeshabilitado = 16,
        /// <summary>
        /// 429 TOO MANY REQUESTS - Límite de concurrencia excedido
        /// </summary>
        LimiteExcedido = 19,
        /// <summary>
        /// 422 UNPROCESSABLE ENTITY - No puede ser procesado por regla de negocio.
        /// </summary>
        EntidadSinProcesar = 20,
        /// <summary>
        /// 500 INTERNAL ERROR - Excepción no controlada en el webservice
        /// </summary>
        ErrorInternoEnWeb = 90,
        /// <summary>
        /// 500 INTERNAL ERROR - Excepción no controlada en el backend (motor)
        /// </summary>
        ErrorInternoEnMotor = 91,
    }
}
