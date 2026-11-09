namespace AccountingLab.Domain.Education;

public sealed record ChallengeDescriptor(string Id, string Name, string Family, string Context, string ReferenceUrl);

public static class ChallengeCatalog
{
    public static IReadOnlyList<ChallengeDescriptor> Items { get; } = Array.AsReadOnly(new[]
    {
        new ChallengeDescriptor("DP-01", "Factory Method", "Creacional", "Generación de comprobantes contables", "https://refactoring.guru/es/design-patterns/factory-method"),
        new ChallengeDescriptor("DP-02", "Abstract Factory", "Creacional", "Familias contables por perfil de empresa", "https://refactoring.guru/es/design-patterns/abstract-factory"),
        new ChallengeDescriptor("DP-03", "Builder", "Creacional", "Informes con secciones, filtros y anexos", "https://refactoring.guru/es/design-patterns/builder"),
        new ChallengeDescriptor("DP-04", "Prototype", "Creacional", "Copias independientes de plantillas recurrentes", "https://refactoring.guru/es/design-patterns/prototype"),
        new ChallengeDescriptor("DP-05", "Singleton", "Creacional", "Registro global inmutable por proceso", "https://refactoring.guru/es/design-patterns/singleton"),
        new ChallengeDescriptor("DP-06", "Adapter", "Estructural", "Importación de dos bancos ficticios", "https://refactoring.guru/es/design-patterns/adapter"),
        new ChallengeDescriptor("DP-07", "Bridge", "Estructural", "Tipos de reporte y representaciones independientes", "https://refactoring.guru/es/design-patterns/bridge"),
        new ChallengeDescriptor("DP-08", "Composite", "Estructural", "Cuentas agrupadoras y de movimiento", "https://refactoring.guru/es/design-patterns/composite"),
        new ChallengeDescriptor("DP-09", "Decorator", "Estructural", "Exportaciones con características opcionales", "https://refactoring.guru/es/design-patterns/decorator"),
        new ChallengeDescriptor("DP-10", "Facade", "Estructural", "Entrada coherente para cierre contable", "https://refactoring.guru/es/design-patterns/facade"),
        new ChallengeDescriptor("DP-11", "Flyweight", "Estructural", "Clasificación inmutable compartida", "https://refactoring.guru/es/design-patterns/flyweight"),
        new ChallengeDescriptor("DP-12", "Proxy", "Estructural", "Consulta autorizada de reportes sensibles", "https://refactoring.guru/es/design-patterns/proxy"),
        new ChallengeDescriptor("DP-13", "Chain of Responsibility", "Comportamiento", "Validaciones configurables de aprobación de pagos", "https://refactoring.guru/es/design-patterns/chain-of-responsibility"),
        new ChallengeDescriptor("DP-14", "Command", "Comportamiento", "Aplicación de cobros/pagos y reversiones identificadas", "https://refactoring.guru/es/design-patterns/command"),
        new ChallengeDescriptor("DP-15", "Iterator", "Comportamiento", "Recorrido de libros sin exponer su almacenamiento", "https://refactoring.guru/es/design-patterns/iterator"),
        new ChallengeDescriptor("DP-16", "Mediator", "Comportamiento", "Coordinación del procesamiento de facturas", "https://refactoring.guru/es/design-patterns/mediator"),
        new ChallengeDescriptor("DP-17", "Memento", "Comportamiento", "Versiones de borradores presupuestales", "https://refactoring.guru/es/design-patterns/memento"),
        new ChallengeDescriptor("DP-18", "Observer", "Comportamiento", "Suscripciones a hechos financieros", "https://refactoring.guru/es/design-patterns/observer"),
        new ChallengeDescriptor("DP-19", "State", "Comportamiento", "Comportamientos y transiciones de facturas", "https://refactoring.guru/es/design-patterns/state"),
        new ChallengeDescriptor("DP-20", "Strategy", "Comportamiento", "Impuestos ficticios configurables por vigencia", "https://refactoring.guru/es/design-patterns/strategy"),
        new ChallengeDescriptor("DP-21", "Template Method", "Comportamiento", "Modalidades de conciliación bancaria", "https://refactoring.guru/es/design-patterns/template-method"),
        new ChallengeDescriptor("DP-22", "Visitor", "Comportamiento", "Métricas sobre documentos heterogéneos", "https://refactoring.guru/es/design-patterns/visitor")
    });

    public static ChallengeDescriptor Get(string id) => Items.Single(item => item.Id == id);
}
