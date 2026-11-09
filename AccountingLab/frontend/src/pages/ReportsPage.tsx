import { useState } from "react";
import { ArrowUpRight, Download, FileSpreadsheet } from "lucide-react";
import { useApi } from "../hooks/useApi";
import type { Summary } from "../types/api";
import type { PageContext } from "./ResourcePage";
import { ApiError, money, request } from "../services/api";
import { ErrorNotice, Loading } from "../components/Feedback";
import { ChallengeAction } from "../components/ChallengeAction";

export function ReportsPage({ token, companyId, currency }: PageContext) {
  const summary = useApi<Summary>("/reports/summary", token, companyId);
  const [error, setError] = useState<unknown>();
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState("");
  async function balanceSheet() {
    setLoading(true);
    setError(undefined);
    setResult("");
    try {
      const data = await request("/reports/balance-sheet", token, companyId);
      setResult(JSON.stringify(data, null, 2));
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Análisis financiero</span>
          <h1>Reportes</h1>
          <p>
            Consulta el resumen básico. Los informes configurables conservan sus
            desafíos pendientes.
          </p>
        </div>
      </header>
      <section className="panel">
        <div className="section-heading">
          <h2>Resumen operativo</h2>
          <span className="badge">Datos confirmados por la API</span>
        </div>
        {summary.loading ? (
          <Loading />
        ) : summary.error ? (
          <ErrorNotice error={summary.error} />
        ) : (
          summary.data && (
            <dl className="summary-list">
              {[
                ["Débitos", money(summary.data.debits, currency)],
                ["Créditos", money(summary.data.credits, currency)],
                ["Diferencia", money(summary.data.balance, currency)],
                ["Asientos", String(summary.data.entryCount)],
              ].map(([label, value]) => (
                <div key={label}>
                  <dt>{label}</dt>
                  <dd>{value}</dd>
                </div>
              ))}
            </dl>
          )
        )}
      </section>
      <section className="report-card">
        <FileSpreadsheet size={32} />
        <div>
          <h2>Balance general configurable</h2>
          <p>
            El informe avanzado requiere las capacidades académicas indicadas
            por la API.
          </p>
          <button
            className="button secondary"
            onClick={balanceSheet}
            disabled={loading}
          >
            {loading ? "Consultando…" : "Consultar disponibilidad"}
            <ArrowUpRight size={16} />
          </button>
        </div>
      </section>
      <ErrorNotice error={error} />
      {result && <pre className="json-result">{result}</pre>}
      <ChallengeAction
        ids={["DP-03", "DP-07", "DP-12", "DP-22"]}
        token={token}
        companyId={companyId}
      />
    </>
  );
}

export function ExportsPage({ token, companyId }: PageContext) {
  const [error, setError] = useState<unknown>();
  const [busy, setBusy] = useState("");
  const [success, setSuccess] = useState("");
  async function download(resource: string) {
    setBusy(resource);
    setError(undefined);
    setSuccess("");
    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_URL || "/api"}/exports/${resource}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "X-Company-Id": companyId,
            Accept: "text/csv",
          },
        },
      );
      if (!response.ok)
        throw new ApiError(
          response.status,
          await response
            .json()
            .catch(() => ({ title: "No se pudo exportar la información." })),
        );
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = `${resource}.csv`;
      anchor.click();
      URL.revokeObjectURL(url);
      setSuccess("Archivo recibido de la API.");
    } catch (err) {
      setError(err);
    } finally {
      setBusy("");
    }
  }
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Datos de la empresa</span>
          <h1>Exportación de información</h1>
          <p>
            Descarga consultas básicas autorizadas. Las extensiones de
            documentos permanecen pendientes.
          </p>
        </div>
      </header>
      <div className="export-grid">
        {[
          {
            resource: "accounts",
            name: "Catálogo de cuentas",
            detail: "Cuentas y agrupaciones registradas.",
          },
          {
            resource: "journal-entries",
            name: "Asientos contables",
            detail: "Registros contables confirmados.",
          },
        ].map((item) => (
          <section className="panel export-card" key={item.resource}>
            <FileSpreadsheet size={30} />
            <h2>{item.name}</h2>
            <p>{item.detail}</p>
            <button
              className="button secondary"
              disabled={!!busy}
              onClick={() => download(item.resource)}
            >
              <Download size={16} />
              {busy === item.resource ? "Descargando…" : "Descargar CSV"}
            </button>
          </section>
        ))}
      </div>
      <ErrorNotice error={error} />
      {success && (
        <p className="success-notice" role="status">
          {success}
        </p>
      )}
      <ChallengeAction ids={["DP-09"]} token={token} companyId={companyId} />
    </>
  );
}
