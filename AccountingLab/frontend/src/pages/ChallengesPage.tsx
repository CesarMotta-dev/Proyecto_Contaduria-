import { useState } from "react";
import { ArrowUpRight, BookOpen } from "lucide-react";
import { useApi } from "../hooks/useApi";
import type { Challenge } from "../types/api";
import type { PageContext } from "./ResourcePage";
import { Empty, ErrorNotice, Loading } from "../components/Feedback";
import { ChallengeAction } from "../components/ChallengeAction";

export function ChallengesPage({ token, companyId }: PageContext) {
  const challenges = useApi<Challenge[]>("/challenges", token, companyId);
  const [family, setFamily] = useState("Todas");
  const [progress, setProgress] = useState<Record<string, string>>(() => {
    try {
      return JSON.parse(
        localStorage.getItem(`accountinglab-progress-${companyId}`) || "{}",
      );
    } catch {
      return {};
    }
  });
  function updateProgress(id: string, value: string) {
    const next = { ...progress, [id]: value };
    setProgress(next);
    localStorage.setItem(
      `accountinglab-progress-${companyId}`,
      JSON.stringify(next),
    );
  }
  const families = [
    "Todas",
    ...new Set(challenges.data?.map((c) => c.family) || []),
  ];
  const filtered =
    challenges.data?.filter((c) => family === "Todas" || c.family === family) ||
    [];
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Laboratorio de diseño</span>
          <h1>22 desafíos. Tu criterio de diseño.</h1>
          <p>
            Comprende cada problema, estudia su referencia y desarrolla tu
            propia solución.
          </p>
        </div>
        <span className="catalog-count">
          {challenges.data?.length ?? "…"} desafíos
        </span>
      </header>
      <div className="info-notice">
        <BookOpen size={20} />
        <p>
          El estado de la operación proviene de la API. Tu avance personal es
          una autodeclaración local.{" "}
          <strong>
            No hay reporte de pruebas cargado: los desafíos están sin verificar.
          </strong>
        </p>
      </div>
      <div className="tabs" role="group" aria-label="Filtrar por familia">
        {families.map((item) => (
          <button
            key={item}
            className={family === item ? "selected" : ""}
            onClick={() => setFamily(item)}
          >
            {item}
          </button>
        ))}
      </div>
      {challenges.loading ? (
        <Loading />
      ) : challenges.error ? (
        <ErrorNotice error={challenges.error} />
      ) : !filtered.length ? (
        <Empty />
      ) : (
        <div className="challenge-grid">
          {filtered.map((challenge) => (
            <article className="challenge-card" key={challenge.id}>
              <div className="challenge-card-top">
                <span className="challenge-id">{challenge.id}</span>
                <span
                  className={challenge.available ? "badge" : "badge pending"}
                >
                  {challenge.available
                    ? "Operación disponible"
                    : "Pendiente de implementar"}
                </span>
              </div>
              <p className="eyebrow">{challenge.family}</p>
              <h2>{challenge.name}</h2>
              <p>{challenge.context}</p>
              <a
                className="reference-link"
                href={challenge.referenceUrl}
                target="_blank"
                rel="noreferrer"
              >
                Refactoring.Guru
                <ArrowUpRight size={15} />
              </a>
              <div className="progress-field">
                <label htmlFor={`progress-${challenge.id}`}>
                  Mi avance personal · autodeclarado
                </label>
                <select
                  id={`progress-${challenge.id}`}
                  value={progress[challenge.id] || "Pendiente"}
                  onChange={(e) => updateProgress(challenge.id, e.target.value)}
                >
                  <option>Pendiente</option>
                  <option>En estudio</option>
                  <option>Autodeclaro terminado</option>
                </select>
                <span>Pruebas: sin verificar</span>
              </div>
              <ChallengeAction
                ids={[challenge.id]}
                token={token}
                companyId={companyId}
              />
            </article>
          ))}
        </div>
      )}
    </>
  );
}
