import { AlertCircle, BookOpen, LoaderCircle } from "lucide-react";
import { ApiError } from "../services/api";

export function Loading() {
  return (
    <div className="loading" role="status">
      <LoaderCircle className="spin" size={20} /> Consultando información…
    </div>
  );
}
export function ErrorNotice({ error }: { error: unknown }) {
  if (!error) return null;
  if (error instanceof ApiError && error.status === 501)
    return (
      <section className="pending-notice" role="alert">
        <BookOpen size={24} />
        <div>
          <span className="eyebrow">Funcionalidad no disponible</span>
          <h3>
            {error.problem.challengeId} · {error.problem.patternName}
          </h3>
          <p>{error.message}</p>
          {error.problem.referenceUrl && (
            <a
              href={error.problem.referenceUrl}
              target="_blank"
              rel="noreferrer"
            >
              Estudiar el concepto en Refactoring.Guru ↗
            </a>
          )}
          <p className="small">La operación contable no se ha confirmado.</p>
        </div>
      </section>
    );
  return (
    <div className="error-notice" role="alert">
      <AlertCircle size={20} />
      <div>
        <strong>
          {error instanceof ApiError
            ? error.status
              ? `Error ${error.status}`
              : "Sin conexión"
            : "No se pudo completar la operación"}
        </strong>
        <p>{error instanceof Error ? error.message : "Vuelve a intentarlo."}</p>
        {error instanceof ApiError && error.problem.errors && (
          <ul>
            {Object.entries(error.problem.errors).flatMap(([field, errors]) =>
              errors.map((message) => (
                <li key={`${field}-${message}`}>{message}</li>
              )),
            )}
          </ul>
        )}
      </div>
    </div>
  );
}
export function Empty({
  title = "Todavía no hay registros",
  detail = "Los registros confirmados por la API aparecerán aquí.",
}: {
  title?: string;
  detail?: string;
}) {
  return (
    <div className="empty">
      <BookOpen size={28} />
      <h3>{title}</h3>
      <p>{detail}</p>
    </div>
  );
}
