import type { Column } from "../components/DataTable";

export type Field = {
  key: string;
  label: string;
  kind?:
    | "date"
    | "decimal"
    | "integer"
    | "checkbox"
    | "select"
    | "password"
    | "email";
  required?: boolean;
  options?: { value: string; label: string }[];
  resource?: string;
  hint?: string;
};
export type Module = {
  path: string;
  title: string;
  description: string;
  category: string;
  fields: Field[];
  columns: Column[];
  challenges?: string[];
  readonly?: boolean;
  createLabel?: string;
};
const required = (key: string, label: string, kind?: Field["kind"]): Field => ({
  key,
  label,
  kind,
  required: true,
});
const thirdParty: Field = {
  ...required("thirdPartyId", "Tercero", "select"),
  resource: "third-parties",
};
const amount = required("amount", "Importe", "decimal");
const columns = (...pairs: string[]): Column[] =>
  pairs.map((pair) => {
    const [key, label, money] = pair.split("|");
    return { key, label, money: money === "$" };
  });

export const modules: Module[] = [
  {
    path: "accounts",
    title: "Cuentas contables",
    category: "Contabilidad",
    description: "Organiza las cuentas de movimiento y sus agrupaciones.",
    fields: [
      required("code", "Código"),
      required("name", "Nombre"),
      { key: "isGroup", label: "Cuenta agrupadora", kind: "checkbox" },
      {
        key: "parentId",
        label: "Cuenta superior",
        kind: "select",
        resource: "accounts",
      },
      { key: "isActive", label: "Cuenta activa", kind: "checkbox" },
    ],
    columns: columns(
      "code|Código",
      "name|Cuenta",
      "isGroup|Agrupadora",
      "isActive|Activa",
    ),
    challenges: ["DP-08"],
  },
  {
    path: "companies",
    title: "Empresas",
    category: "Configuración",
    description:
      "Identificación y moneda de referencia de las empresas autorizadas.",
    fields: [
      required("name", "Razón social"),
      required("taxId", "Identificación tributaria"),
      {
        ...required("currency", "Moneda", "select"),
        options: ["COP", "USD", "EUR"].map((value) => ({
          value,
          label: value,
        })),
      },
    ],
    columns: columns(
      "name|Razón social",
      "taxId|Identificación",
      "currency|Moneda",
    ),
    challenges: ["DP-02", "DP-05"],
  },
  {
    path: "users",
    title: "Usuarios",
    category: "Configuración",
    description:
      "Crea usuarios dentro de la empresa activa según tus permisos.",
    fields: [
      required("name", "Nombre completo"),
      required("email", "Correo electrónico", "email"),
      required("password", "Contraseña inicial", "password"),
      {
        ...required("role", "Rol", "select"),
        options: ["Administrador", "Contador", "Consulta"].map((value) => ({
          value,
          label: value,
        })),
      },
    ],
    columns: columns("name|Nombre", "email|Correo", "role|Rol"),
  },
  {
    path: "third-parties",
    title: "Terceros",
    category: "Configuración",
    description: "Clientes, proveedores y otros terceros de esta empresa.",
    fields: [
      required("name", "Nombre"),
      required("document", "Documento"),
      { key: "email", label: "Correo electrónico", kind: "email" },
    ],
    columns: columns("name|Nombre", "document|Documento", "email|Correo"),
  },
  {
    path: "cost-centers",
    title: "Centros de costos",
    category: "Configuración",
    description: "Clasifica los movimientos por centro de responsabilidad.",
    fields: [required("code", "Código"), required("name", "Nombre")],
    columns: columns("code|Código", "name|Centro de costos"),
  },
  {
    path: "periods",
    title: "Periodos contables",
    category: "Contabilidad",
    description:
      "Consulta y registra periodos abiertos. El cierre avanzado es un desafío pendiente.",
    fields: [
      required("name", "Nombre"),
      required("startDate", "Fecha inicial", "date"),
      required("endDate", "Fecha final", "date"),
    ],
    columns: columns(
      "name|Periodo",
      "startDate|Inicio",
      "endDate|Fin",
      "isClosed|Cerrado",
    ),
    challenges: ["DP-10"],
  },
  {
    path: "tax-rules",
    title: "Impuestos configurables",
    category: "Configuración",
    description:
      "Reglas ficticias configurables. Registrar una tasa no ejecuta cálculos tributarios.",
    fields: [
      required("name", "Nombre"),
      {
        ...required("rate", "Tasa decimal", "decimal"),
        hint: "Por ejemplo 0.050000. Valores académicos, sin tasas legales predefinidas.",
      },
      required("validFrom", "Vigente desde", "date"),
      { key: "validTo", label: "Vigente hasta", kind: "date" },
    ],
    columns: columns(
      "name|Regla",
      "rate|Tasa decimal",
      "validFrom|Desde",
      "validTo|Hasta",
    ),
    challenges: ["DP-20"],
  },
  {
    path: "receivables",
    title: "Cuentas por cobrar",
    category: "Tesorería",
    description: "Consulta y registra obligaciones de clientes.",
    fields: [
      thirdParty,
      required("description", "Descripción"),
      amount,
      required("dueDate", "Vencimiento", "date"),
    ],
    columns: columns(
      "description|Descripción",
      "amount|Importe|$",
      "dueDate|Vencimiento",
      "status|Estado",
    ),
    challenges: ["DP-14"],
  },
  {
    path: "payables",
    title: "Cuentas por pagar",
    category: "Tesorería",
    description: "Consulta y registra obligaciones con proveedores.",
    fields: [
      thirdParty,
      required("description", "Descripción"),
      amount,
      required("dueDate", "Vencimiento", "date"),
    ],
    columns: columns(
      "description|Descripción",
      "amount|Importe|$",
      "dueDate|Vencimiento",
      "status|Estado",
    ),
    challenges: ["DP-13", "DP-14"],
  },
  {
    path: "payments",
    title: "Pagos y cobros",
    category: "Tesorería",
    description:
      "Registra solicitudes en borrador. Su aplicación contable permanece pendiente.",
    createLabel: "Nuevo borrador",
    fields: [
      required("reference", "Referencia"),
      thirdParty,
      amount,
      {
        ...required("direction", "Tipo", "select"),
        options: [
          { value: "Payment", label: "Pago" },
          { value: "Receipt", label: "Cobro" },
        ],
      },
    ],
    columns: columns(
      "reference|Referencia",
      "direction|Tipo",
      "amount|Importe|$",
      "status|Estado",
    ),
    challenges: ["DP-13", "DP-14", "DP-18"],
  },
  {
    path: "invoices",
    title: "Facturación",
    category: "Operaciones",
    description:
      "Prepara facturas en borrador sin emitirlas ni contabilizarlas automáticamente.",
    createLabel: "Nueva factura borrador",
    fields: [
      required("number", "Número de factura"),
      thirdParty,
      amount,
      required("issueDate", "Fecha de emisión", "date"),
      required("dueDate", "Fecha de vencimiento", "date"),
    ],
    columns: columns(
      "number|Factura",
      "amount|Importe|$",
      "issueDate|Emisión",
      "dueDate|Vencimiento",
      "status|Estado",
    ),
    challenges: ["DP-16", "DP-19", "DP-20"],
  },
  {
    path: "bank-statements",
    title: "Extractos bancarios",
    category: "Tesorería",
    description:
      "Registra movimientos normalizados manualmente. La importación de proveedores sigue pendiente.",
    fields: [
      required("reference", "Referencia"),
      required("bookingDate", "Fecha del movimiento", "date"),
      {
        ...amount,
        hint: "Importe normalizado no negativo; el servidor valida el movimiento.",
      },
      required("description", "Descripción"),
    ],
    columns: columns(
      "reference|Referencia",
      "bookingDate|Fecha",
      "description|Descripción",
      "amount|Importe|$",
    ),
    challenges: ["DP-06"],
  },
  {
    path: "bank-reconciliation",
    title: "Conciliación bancaria",
    category: "Tesorería",
    description:
      "Crea sesiones de trabajo. Ejecutar una conciliación requiere completar el desafío correspondiente.",
    fields: [
      required("name", "Nombre de sesión"),
      required("statementDate", "Fecha de extracto", "date"),
    ],
    columns: columns("name|Sesión", "statementDate|Extracto", "status|Estado"),
    challenges: ["DP-06", "DP-21"],
  },
  {
    path: "budgets",
    title: "Presupuestos",
    category: "Planificación",
    description:
      "Mantén borradores presupuestales. Las versiones y restauración siguen pendientes.",
    fields: [
      required("name", "Nombre del presupuesto"),
      required("year", "Año", "integer"),
      amount,
    ],
    columns: columns(
      "name|Presupuesto",
      "year|Año",
      "amount|Importe|$",
      "status|Estado",
    ),
    challenges: ["DP-17"],
  },
  {
    path: "approvals",
    title: "Aprobaciones",
    category: "Operaciones",
    description:
      "Registra solicitudes para revisión sin ejecutar una aprobación automática.",
    fields: [
      required("reference", "Referencia"),
      amount,
      required("description", "Descripción"),
    ],
    columns: columns(
      "reference|Referencia",
      "description|Descripción",
      "amount|Importe|$",
      "status|Estado",
    ),
    challenges: ["DP-13"],
  },
  {
    path: "notifications",
    title: "Notificaciones",
    category: "Operaciones",
    description:
      "Avisos existentes de la empresa. Los avisos demo se identifican como datos académicos.",
    fields: [],
    readonly: true,
    columns: columns("title|Aviso", "message|Mensaje", "createdAt|Fecha"),
    challenges: ["DP-18"],
  },
  {
    path: "audit",
    title: "Auditoría",
    category: "Operaciones",
    description: "Trazabilidad de hechos básicos confirmados por el servidor.",
    fields: [],
    readonly: true,
    columns: columns(
      "action|Acción",
      "entityType|Entidad",
      "description|Detalle",
      "createdAt|Fecha UTC",
    ),
  },
];
