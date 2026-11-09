import {
  ArrowDownLeft,
  ArrowUpRight,
  BookOpen,
  CircleDollarSign,
  Landmark,
  ReceiptText,
} from "lucide-react";
import { Link } from "react-router-dom";
import { useApi } from "../hooks/useApi";
import type { Entity, Page, Summary } from "../types/api";
import type { PageContext } from "./ResourcePage";
import { money } from "../services/api";
import { DataTable } from "../components/DataTable";
import { ErrorNotice, Loading } from "../components/Feedback";

export function Dashboard({ token, companyId, currency }: PageContext) {
  const summary = useApi<Summary>("/reports/summary", token, companyId);
  const entries = useApi<Page<Entity>>(
    "/journal-entries?page=1&pageSize=5",
    token,
    companyId,
  );
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Vista general</span>
          <h1>Tu contabilidad, en perspectiva.</h1>
          <p>Resumen de movimientos confirmados de la empresa activa.</p>
        </div>
        <Link className="button" to="/journal-entries">
          Registrar asiento
          <ArrowUpRight size={17} />
        </Link>
      </header>
      {summary.loading ? (
        <Loading />
      ) : summary.error ? (
        <ErrorNotice error={summary.error} />
      ) : (
        summary.data && (
          <div className="metric-grid">
            {[
              {
                label: "Débitos registrados",
                value: money(summary.data.debits, currency),
                icon: ArrowDownLeft,
              },
              {
                label: "Créditos registrados",
                value: money(summary.data.credits, currency),
                icon: ArrowUpRight,
              },
              {
                label: "Diferencia contable",
                value: money(summary.data.balance, currency),
                icon: Landmark,
              },
              {
                label: "Asientos confirmados",
                value: String(summary.data.entryCount),
                icon: BookOpen,
              },
              {
                label: "Cuentas por cobrar",
                value: money(summary.data.receivableTotal, currency),
                icon: CircleDollarSign,
              },
              {
                label: "Cuentas por pagar",
                value: money(summary.data.payableTotal, currency),
                icon: ReceiptText,
              },
            ].map((metric) => (
              <article className="metric" key={metric.label}>
                <div>
                  <span>{metric.label}</span>
                  <metric.icon size={20} />
                </div>
                <strong>{metric.value}</strong>
              </article>
            ))}
          </div>
        )
      )}
      <div className="dashboard-grid">
        <section className="panel">
          <div className="section-heading">
            <div>
              <span className="eyebrow">Actividad contable</span>
              <h2>Últimos asientos</h2>
            </div>
            <Link to="/journal">
              Ver libro diario
              <ArrowUpRight size={15} />
            </Link>
          </div>
          {entries.loading ? (
            <Loading />
          ) : entries.error ? (
            <ErrorNotice error={entries.error} />
          ) : (
            <DataTable
              items={entries.data?.items || []}
              columns={[
                { key: "reference", label: "Referencia" },
                { key: "date", label: "Fecha" },
                { key: "totalDebit", label: "Débitos", money: true },
              ]}
              currency={currency}
            />
          )}
        </section>
        <aside className="learning-card">
          <span className="eyebrow">Tu espacio de aprendizaje</span>
          <h2>22 decisiones de diseño por explorar.</h2>
          <p>
            Los módulos básicos son operativos. Las capacidades avanzadas
            conservan sus desafíos académicos pendientes.
          </p>
          <Link to="/challenges" className="button light">
            Explorar desafíos
            <ArrowUpRight size={17} />
          </Link>
          <div className="learning-footnote">
            <BookOpen size={18} />
            <span>
              Las pruebas aportan evidencia.
              <br />
              Una autodeclaración no acredita aprobación.
            </span>
          </div>
        </aside>
      </div>
    </>
  );
}
