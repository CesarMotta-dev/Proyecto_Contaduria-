import { useState } from "react";
import { useApi } from "../hooks/useApi";
import type { Entity, Page } from "../types/api";
import type { PageContext } from "./ResourcePage";
import { DataTable } from "../components/DataTable";
import { ErrorNotice, Loading } from "../components/Feedback";
import { ChallengeAction } from "../components/ChallengeAction";

export function LedgerPage({ token, companyId, currency }: PageContext) {
  const [accountId, setAccountId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const accounts = useApi<Page<Entity>>(
    "/accounts?page=1&pageSize=100",
    token,
    companyId,
  );
  const query = new URLSearchParams({
    page: String(page),
    pageSize: "20",
    ...(accountId ? { accountId } : {}),
    ...(from ? { from } : {}),
    ...(to ? { to } : {}),
  });
  const ledger = useApi<Page<Entity>>(`/ledger?${query}`, token, companyId);
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Contabilidad</span>
          <h1>Libro mayor</h1>
          <p>
            Movimientos y saldos calculados por el servidor a partir de asientos
            confirmados.
          </p>
        </div>
      </header>
      <section className="panel">
        <div className="filter-grid">
          <label className="field">
            <span>Cuenta</span>
            <select
              value={accountId}
              onChange={(e) => {
                setAccountId(e.target.value);
                setPage(1);
              }}
            >
              <option value="">Todas las cuentas</option>
              {accounts.data?.items.map((account) => (
                <option key={account.id} value={account.id}>
                  {String(account.code)} · {String(account.name)}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Desde</span>
            <input
              type="date"
              value={from}
              onChange={(e) => {
                setFrom(e.target.value);
                setPage(1);
              }}
            />
          </label>
          <label className="field">
            <span>Hasta</span>
            <input
              type="date"
              value={to}
              onChange={(e) => {
                setTo(e.target.value);
                setPage(1);
              }}
            />
          </label>
        </div>
        <ErrorNotice error={accounts.error} />
        {ledger.loading ? (
          <Loading />
        ) : ledger.error ? (
          <ErrorNotice error={ledger.error} />
        ) : (
          <DataTable
            items={ledger.data?.items || []}
            currency={currency}
            columns={[
              { key: "date", label: "Fecha" },
              { key: "reference", label: "Referencia" },
              { key: "accountName", label: "Cuenta" },
              { key: "debit", label: "Débito", money: true },
              { key: "credit", label: "Crédito", money: true },
              { key: "balance", label: "Saldo", money: true },
            ]}
          />
        )}
        <div className="pagination">
          <span>
            Página {page} · {ledger.data?.total || 0} movimientos
          </span>
          <div>
            <button
              className="button secondary small-button"
              disabled={page === 1 || ledger.loading}
              onClick={() => setPage((p) => p - 1)}
            >
              Anterior
            </button>
            <button
              className="button secondary small-button"
              disabled={
                !ledger.data || page * 20 >= ledger.data.total || ledger.loading
              }
              onClick={() => setPage((p) => p + 1)}
            >
              Siguiente
            </button>
          </div>
        </div>
      </section>
      <ChallengeAction
        ids={["DP-15", "DP-11"]}
        token={token}
        companyId={companyId}
      />
    </>
  );
}
