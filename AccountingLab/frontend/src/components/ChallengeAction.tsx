import { useState } from "react";
import { ArrowUpRight } from "lucide-react";
import { request } from "../services/api";
import { ErrorNotice } from "./Feedback";

export function ChallengeAction({
  ids,
  token,
  companyId,
}: {
  ids: string[];
  token: string;
  companyId: string;
}) {
  const [active, setActive] = useState("");
  const [error, setError] = useState<unknown>();
  const [message, setMessage] = useState("");
  async function execute(id: string) {
    setActive(id);
    setError(undefined);
    setMessage("");
    try {
      await request(`/challenges/${id}/execute`, token, companyId, {
        method: "POST",
        body: "{}",
      });
      setMessage(
        `${id}: la API recibió la solicitud. Consulta el resultado del servidor; esto no acredita pruebas académicas.`,
      );
    } catch (err) {
      setError(err);
    } finally {
      setActive("");
    }
  }
  return (
    <section className="challenge-strip">
      <div>
        <span className="eyebrow">Capacidades académicas</span>
        <p>
          Consulta la disponibilidad del desafío. No se ejecutan soluciones
          desde la web.
        </p>
      </div>
      <div className="button-row">
        {ids.map((id) => (
          <button
            key={id}
            className="button ghost"
            onClick={() => execute(id)}
            disabled={!!active}
          >
            {active === id ? "Consultando…" : id}
            <ArrowUpRight size={15} />
          </button>
        ))}
      </div>
      <ErrorNotice error={error} />
      {message && <p role="status">{message}</p>}
    </section>
  );
}
