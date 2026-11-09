import { useState } from "react";
import { Plus, RefreshCw, Search } from "lucide-react";
import type { Module } from "../services/modules";
import type { Entity, Page } from "../types/api";
import { useApi } from "../hooks/useApi";
import { request } from "../services/api";
import { DataTable } from "../components/DataTable";
import { EntryForm } from "../components/EntryForm";
import { ErrorNotice, Loading } from "../components/Feedback";
import { ChallengeAction } from "../components/ChallengeAction";

export type PageContext = {
  token: string;
  companyId: string;
  currency: string;
  role: string;
};
export function ResourcePage({
  module,
  token,
  companyId,
  currency,
  role,
}: PageContext & { module: Module }) {
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState("");
  const [revision, setRevision] = useState(0);
  const [creating, setCreating] = useState(false);
  const [success, setSuccess] = useState("");
  const data = useApi<Page<Entity>>(
    `/${module.path}?page=${page}&pageSize=10`,
    token,
    companyId,
    revision,
  );
  const canCreate =
    !module.readonly &&
    role !== "Consulta" &&
    (!["users", "companies"].includes(module.path) || role === "Administrador");
  async function save(payload: Record<string, unknown>) {
    await request(`/${module.path}`, token, companyId, {
      method: "POST",
      body: JSON.stringify(payload),
    });
    setCreating(false);
    setPage(1);
    setRevision((x) => x + 1);
    setSuccess(
      module.path === "companies"
        ? "Empresa guardada. Usa Actualizar empresas en la barra superior para seleccionarla."
        : "Registro guardado. La lista se ha vuelto a consultar.",
    );
  }
  const items = (data.data?.items || []).filter((item) =>
    module.columns.some((column) =>
      String(item[column.key] ?? "")
        .toLocaleLowerCase("es")
        .includes(filter.toLocaleLowerCase("es")),
    ),
  );
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">{module.category}</span>
          <h1>{module.title}</h1>
          <p>{module.description}</p>
        </div>
        {canCreate && !creating && (
          <button
            className="button"
            onClick={() => {
              setCreating(true);
              setSuccess("");
            }}
          >
            <Plus size={18} />
            {module.createLabel || "Nuevo registro"}
          </button>
        )}
      </header>
      {creating && (
        <EntryForm
          fields={module.fields}
          title={module.createLabel || `Nuevo registro · ${module.title}`}
          token={token}
          companyId={companyId}
          onSave={save}
          onCancel={() => setCreating(false)}
        />
      )}
      {success && (
        <div role="status" className="success-notice">
          {success}
        </div>
      )}
      <section className="panel">
        <div className="table-toolbar">
          <label className="search-field">
            <Search size={17} />
            <input
              aria-label="Filtrar registros de esta página"
              placeholder="Filtrar esta página…"
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
            />
          </label>
          <button
            className="icon-button"
            onClick={() => setRevision((x) => x + 1)}
            aria-label="Actualizar registros"
            disabled={data.loading}
          >
            <RefreshCw size={17} />
          </button>
        </div>
        {data.loading ? (
          <Loading />
        ) : data.error ? (
          <ErrorNotice error={data.error} />
        ) : (
          <DataTable
            items={items}
            columns={module.columns}
            currency={currency}
          />
        )}
        <footer className="pagination">
          <span>
            {data.data?.total || 0} registros · Página {page}
          </span>
          <div>
            <button
              className="button secondary small-button"
              disabled={page === 1 || data.loading}
              onClick={() => {
                setPage((x) => x - 1);
                setFilter("");
              }}
            >
              Anterior
            </button>
            <button
              className="button secondary small-button"
              disabled={
                !data.data || page * 10 >= data.data.total || data.loading
              }
              onClick={() => {
                setPage((x) => x + 1);
                setFilter("");
              }}
            >
              Siguiente
            </button>
          </div>
        </footer>
      </section>
      {module.challenges && (
        <ChallengeAction
          ids={module.challenges}
          token={token}
          companyId={companyId}
        />
      )}
    </>
  );
}
