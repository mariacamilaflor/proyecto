-- ============================================================
-- CREACION DE LA BASE DE DATOS
-- ============================================================

CREATE DATABASE EduDirectory;
GO

USE EduDirectory;
GO
-- ============================================================
-- TABLAS SIN DEPENDENCIAS (sin FK entrantes de otras tablas)
-- ============================================================


 
CREATE TABLE Usuario (
    IdUsuario   INT PRIMARY KEY IDENTITY(1,1),
    Nombre      NVARCHAR(100)  NOT NULL,
    Apellido    NVARCHAR(100)  NOT NULL,
    Rol         NVARCHAR(50)   NOT NULL,
    Correo      NVARCHAR(150)  NOT NULL UNIQUE,
    Contrasena  NVARCHAR(255)  NOT NULL
);
GO
 
-- ============================================================
-- TABLAS DEPENDIENTES DE Institucion
-- ============================================================
 
CREATE TABLE Rector (
    IdRector        INT PRIMARY KEY IDENTITY(1,1),
    Nombre          NVARCHAR(100) NOT NULL,
    Apellido        NVARCHAR(100) NOT NULL,
    Telefono        NVARCHAR(20)  NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    IdInstitucion   INT           NOT NULL,
    CONSTRAINT FK_Rector_Institucion FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO
 
CREATE TABLE Email (
    IdEmail         INT PRIMARY KEY IDENTITY(1,1),
    Email           NVARCHAR(150)   NOT NULL,
    IdInstitucion   INT             NOT NULL,
    PerteneceA      NVARCHAR(150)   NOT NULL
    CONSTRAINT FK_Email_Institucion FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO
 
CREATE TABLE ResultadosIcfes (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Puntaje         DECIMAL(10,2)   NOT NULL,
    Anio            INT             NOT NULL,
    IdInstitucion   INT             NOT NULL,
    CONSTRAINT FK_ResultadosIcfes_Institucion FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO
 
CREATE TABLE Sede (
    IdSede          INT PRIMARY KEY IDENTITY(1,1),
    NombreSede      NVARCHAR(150)   NOT NULL,
    IdInstitucion   INT             NOT NULL,
    CONSTRAINT FK_Sede_Institucion FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO
 
-- ============================================================
-- TABLAS DEPENDIENTES DE Sede
-- ============================================================
 
CREATE TABLE Ubicacion (
    IdUbicacion INT PRIMARY KEY IDENTITY(1,1),
    Direccion   NVARCHAR(200) NOT NULL,
    Barrio      NVARCHAR(100) NOT NULL,
    Comuna      NVARCHAR(100) NOT NULL,
    IdSede      INT           NOT NULL,
    CONSTRAINT FK_Ubicacion_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO

CREATE TABLE Coordenada (
    IdCoordenada    INT PRIMARY KEY IDENTITY(1,1),
    CoordenadaX     DECIMAL(18,10)  NOT NULL,
    CoordenadaY     DECIMAL(18,10)  NOT NULL,
    IdUbicacion     INT             NOT NULL,
	CONSTRAINT FK_Coordenada_Ubicacion FOREIGN KEY (IdUbicacion)
        REFERENCES Ubicacion(IdUbicacion)
);
GO

CREATE TABLE Nivel (
    IdNivel     INT PRIMARY KEY IDENTITY(1,1),
    NombreNivel NVARCHAR(100)   NOT NULL,
    IdSede      INT             NOT NULL,
    CONSTRAINT FK_Nivel_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO
 
CREATE TABLE Telefono (
    IdTelefono      INT PRIMARY KEY IDENTITY(1,1),
    NumeroTelefono  NVARCHAR(20)    NOT NULL,
    IdSede          INT             NOT NULL,
    CONSTRAINT FK_Telefono_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO
 
CREATE TABLE Servicio (
    IdServicio      INT PRIMARY KEY IDENTITY(1,1),
    NombreServicio  NVARCHAR(150)   NOT NULL,
    IdSede          INT             NOT NULL,
    CONSTRAINT FK_Servicio_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO
 
CREATE TABLE ActividadesExtracurriculares (
    IdActividadesExtracurriculares  INT PRIMARY KEY IDENTITY(1,1),
    NombreActividad                 NVARCHAR(150)   NOT NULL,
    IdSede                          INT             NOT NULL,
    CONSTRAINT FK_ActividadesExtracurriculares_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO
 
CREATE TABLE Jornada (
    IdJornada       INT PRIMARY KEY IDENTITY(1,1),
    NombreJornada   NVARCHAR(100)   NOT NULL,
    IdSede          INT             NOT NULL,
    CONSTRAINT FK_Jornada_Sede FOREIGN KEY (IdSede)
        REFERENCES Sede(IdSede)
);
GO
 
-- ============================================================
-- TABLAS DEPENDIENTES DE Usuario e Institucion
-- ============================================================
 
CREATE TABLE Evaluacion (
    IdEvaluacion    INT PRIMARY KEY IDENTITY(1,1),
    Comentario      NVARCHAR(500) NOT NULL,
    IdUsuario       INT           NOT NULL,
    IdInstitucion   INT           NOT NULL,
    CONSTRAINT FK_Evaluacion_Usuario      FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Evaluacion_Institucion  FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO

CREATE TABLE Calificacion (
    IdCalificacion                          INT PRIMARY KEY IDENTITY(1,1),
    CalificacionAmbienteEscolar             DECIMAL(5,2)  NOT NULL,
    CalificacionMetodologia                 DECIMAL(5,2)  NOT NULL,
    CalificacionSeguridad                   DECIMAL(5,2)  NOT NULL,
    CalificacionProfesores                  DECIMAL(5,2)  NOT NULL,
    CalificacionActividadesExtracurriculares DECIMAL(5,2) NOT NULL,
    CalificacionInfraestructura             DECIMAL(5,2)  NOT NULL,
    IdEvaluacion                            INT           NOT NULL,
    CONSTRAINT FK_Calificacion_Evaluacion FOREIGN KEY (IdEvaluacion)
        REFERENCES Evaluacion(IdEvaluacion)
);
GO
 
CREATE TABLE FormularioContacto (
    IdFormularioContacto    INT PRIMARY KEY IDENTITY(1,1),
    Nombre                  NVARCHAR(100)   NOT NULL,
    Mensaje                 NVARCHAR(1000)  NOT NULL,
    IdUsuario               INT             NOT NULL,
    IdInstitucion           INT             NOT NULL,
    CONSTRAINT FK_FormularioContacto_Usuario      FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_FormularioContacto_Institucion  FOREIGN KEY (IdInstitucion)
        REFERENCES Institucion(IdInstitucion)
);
GO
 
-- ===================================================================
-- CHATBOT, FORMULARIO PREFERENCIA y FILTRO (dependen de Usuario)
-- ===================================================================
 
CREATE TABLE Chatbot (
    IdChatbot   INT PRIMARY KEY IDENTITY(1,1),
    Fecha       DATE            NOT NULL,
    Mensaje     NVARCHAR(1000)  NOT NULL,
    IdUsuario   INT             NOT NULL,
    CONSTRAINT FK_Chatbot_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario)
);
GO
 
CREATE TABLE Filtro (
    IdFiltro        INT PRIMARY KEY IDENTITY(1,1),
    NombreFiltro    NVARCHAR(100)   NOT NULL,
    IdChatbot       INT             NOT NULL,
    CONSTRAINT FK_Filtro_Chatbot FOREIGN KEY (IdChatbot)
        REFERENCES Chatbot(IdChatbot)
);
GO

CREATE TABLE Institucion (
    IdInstitucion   INT PRIMARY KEY IDENTITY(1,1),
    Nombre          NVARCHAR(150) NOT NULL,
    PaginaWeb       NVARCHAR(255) NOT NULL,
    Imagen          NVARCHAR(255) NOT NULL,
    Metodologia     NVARCHAR(100) NOT NULL,
    Calendario      NVARCHAR(50)  NOT NULL,
    Costo           DECIMAL(18,2) NOT NULL,
    Tipo            NVARCHAR(50)  NOT NULL,
    Descripcion     NVARCHAR(1000) NOT NULL,
    HorarioAtencion NVARCHAR(80)  NOT NULL,
    IdUsuario       INT           NOT NULL,
    CONSTRAINT FK_Institucion_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario)
);
GO

CREATE TABLE FormularioPreferencia (
    IdFormularioPreferencia INT PRIMARY KEY IDENTITY(1,1),
    Presupuesto             DECIMAL(18,2)  NOT NULL,
    EdadEstudiante          INT            NOT NULL,
    TipoPreferencia         NVARCHAR(100)  NOT NULL,
    MetodologiaPreferencia  NVARCHAR(100)  NOT NULL,
    IdUsuario               INT            NOT NULL,
    CONSTRAINT FK_FormularioPreferencia_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario)
);
GO
 
-- ============================================================
-- TABLAS DE PREFERENCIA (dependen de FormularioPreferencia)
-- ============================================================
 
CREATE TABLE ServiciosPreferencia (
    IdServicios                 INT PRIMARY KEY IDENTITY(1,1),
    NombreServiciosPreferencia  NVARCHAR(150)   NOT NULL,
    IdFormularioPreferencia     INT             NOT NULL,
    CONSTRAINT FK_ServiciosPreferencia_FormularioPreferencia FOREIGN KEY (IdFormularioPreferencia)
        REFERENCES FormularioPreferencia(IdFormularioPreferencia)
);
GO
 
CREATE TABLE JornadaPreferencia (
    IdJornadaPreferencia        INT PRIMARY KEY IDENTITY(1,1),
    NombreJornadaPreferencia    NVARCHAR(100)   NOT NULL,
    IdFormularioPreferencia     INT             NOT NULL,
    CONSTRAINT FK_JornadaPreferencia_FormularioPreferencia FOREIGN KEY (IdFormularioPreferencia)
        REFERENCES FormularioPreferencia(IdFormularioPreferencia)
);
GO
 
CREATE TABLE NivelPreferencia (
    IdNivel                     INT PRIMARY KEY IDENTITY(1,1),
    NombreNivelPreferencia      NVARCHAR(100)   NOT NULL,
    IdFormularioPreferencia     INT             NOT NULL,
    CONSTRAINT FK_NivelPreferencia_FormularioPreferencia FOREIGN KEY (IdFormularioPreferencia)
        REFERENCES FormularioPreferencia(IdFormularioPreferencia)
);
GO
 
CREATE TABLE ActividadExtracurricularPreferencia (
    IdActividadExtracurricularPreferencia   INT PRIMARY KEY IDENTITY(1,1),
    NombreActividadExtracurricularPreferencia NVARCHAR(150) NOT NULL,
    IdFormularioPreferencia                 INT             NOT NULL,
    CONSTRAINT FK_ActividadExtracurricularPreferencia_FormularioPreferencia FOREIGN KEY (IdFormularioPreferencia)
        REFERENCES FormularioPreferencia(IdFormularioPreferencia)
);
GO
