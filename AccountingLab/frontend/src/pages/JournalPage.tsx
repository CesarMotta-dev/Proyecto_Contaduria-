import { useState } from "react";
import type { FormEvent } from "react";
import { Plus, Trash2 } from "lucide-react";
import { useApi } from "../hooks/useApi";
import type { Entity, Page } from "../types/api";
import type { PageContext } from "./ResourcePage";
import { isDecimal, request } from "../services/api";
import { DataTable } from "../components/DataTable";
import { ErrorNotice, Loading } from "../components/Feedback";
import { ChallengeAction } from "../components/ChallengeAction";

type Line = {
  key: string;
  accountId: string;
  debit: string;
  credit: string;
  costCenterId: string;
};
const newLine = (): Line => ({
  key: crypto.randomUUID(),
  accountId: "",
  debit: "0",
  credit: "0",
  costCenterId: "",
});
export function JournalPage({
  token,
  companyId,
  currency,
  role,
  readOnly = false,
}: PageContext & { readOnly?: boolean }) {
  const [page, setPage] = useState(1);
  const [revision, setRevision] = useState(0);
  const [creating, setCreating] = useState(false);
  const [reference, setReference] = useState("");
  const [date, setDate] = useState("");
  const [operationId, setOperationId] = useState(() => crypto.randomUUID());
  const [lines, setLines] = useState<Line[]>(() => [newLine(), newLine()]);
  const [error, setError] = useState<unknown>();
  const [validation, setValidation] = useState("");
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState("");
  const entries = useApi<Page<Entity>>(
    `/journal-entries?page=${page}&pageSize=10`,
    token,
    companyId,
    revision,
  );
  const accounts = useApi<Page<Entity>>(
    "/accounts?page=1&pageSize=100",
    token,
    companyId,
  );
  const centers = useApi<Page<Entity>>(
    "/cost-centers?page=1&pageSize=100",
    token,
    companyId,
  );
  function changeLine(index: number, field: keyof Line, value: string) {
    setLines((current) =>
      current.map((line, i) =>
        i === index ? { ...line, [field]: value } : line,
      ),
    );
  }
  async function submit(event: FormEvent) {
    event.preventDefault();
    setValidation("");
    setError(undefined);
    setSuccess("");
    if (
      lines.some(
        (line) =>
          !line.accountId || !isDecimal(line.debit) || !isDecimal(line.credit),
      )
    ) {
      setValidation(
        "Cada línea necesita una cuenta y débitos/créditos con máximo cuatro decimales y separador punto.",
      );
      return;
    }
    setSaving(true);
    try {
      await request("/journal-entries", token, companyId, {
        method: "POST",
        body: JSON.stringify({
          operationId,
          reference: reference.trim(),
          date,
          lines: lines.map(({ key: _key, ...line }) => ({
            ...line,
            costCenterId: line.costCenterId || null,
          })),
        }),
      });
      setCreating(false);
      setReference("");
      setDate("");
      setLines([newLine(), newLine()]);
      setOperationId(crypto.randomUUID());
      setPage(1);
      setRevision((x) => x + 1);
      setSuccess("Asiento confirmado por la API.");
    } catch (err) {
      setError(err);
    } finally {
      setSaving(false);
    }
  }
  return (
    <>
      <header className="page-heading">
        <div>
          <span className="eyebrow">Contabilidad</span>
          <h1>{readOnly ? "Libro diario" : "Asientos contables"}</h1>
          <p>
            {readOnly
              ? "Consulta cronológica de registros contables confirmados."
              : "Registro manual completo. El servidor verifica partida doble y consistencia transaccional."}
          </p>
        </div>
        {!readOnly && role !== "Consulta" && !creating && (
          <button
            className="button"
            onClick={() => {
              setCreating(true);
              setSuccess("");
            }}
          >
            <Plus size={18} />
            Nuevo asiento
          </button>
        )}
      </header>
      {creating && (
        <section className="form-panel">
          <div className="panel-title">
            <h2>Registrar asiento manual</h2>
          </div>
          <form onSubmit={submit}>
            <div className="form-grid">
              <label className="field">
                <span>Referencia *</span>
                <input
                  required
                  value={reference}
                  onChange={(e) => setReference(e.target.value)}
                  maxLength={100}
                />
              </label>
              <label className="field">
                <span>Fecha contable *</span>
                <input
                  required
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                />
              </label>
            </div>
            <div className="journal-lines">
              {lines.map((line, index) => (
                <fieldset key={line.key}>
                  <legend>Línea {index + 1}</legend>
                  <div className="journal-line-grid">
                    <label className="field">
                      <span>Cuenta *</span>
                      <select
                        required
                        aria-label={`Cuenta línea ${index + 1}`}
                        value={line.accountId}
                        onChange={(e) =>
                          changeLine(index, "accountId", e.target.value)
                        }
                      >
                        <option value="">Seleccionar cuenta…</option>
                        {accounts.data?.items
                          .filter(
                            (account) =>
                              !account.isGroup && account.isActive !== false,
                          )
                          .map((account) => (
                            <option key={account.id} value={account.id}>
                              {String(account.code)} · {String(account.name)}
                            </option>
                          ))}
                      </select>
                    </label>
                    <label className="field">
                      <span>Débito *</span>
                      <input
                        aria-label={`Débito línea ${index + 1}`}
                        required
                        inputMode="decimal"
                        value={line.debit}
                        onChange={(e) =>
                          changeLine(index, "debit", e.target.value)
                        }
                      />
                    </label>
                    <label className="field">
                      <span>Crédito *</span>
                      <input
                        aria-label={`Crédito línea ${index + 1}`}
                        required
                        inputMode="decimal"
                        value={line.credit}
                        onChange={(e) =>
                          changeLine(index, "credit", e.target.value)
                        }
                      />
                    </label>
                    <label className="field">
                      <span>Centro de costos</span>
                      <select
                        aria-label={`Centro de costos línea ${index + 1}`}
                        value={line.costCenterId}
                        onChange={(e) =>
                          changeLine(index, "costCenterId", e.target.value)
                        }
                      >
                        <option value="">Sin centro</option>
                        {centers.data?.items.map((center) => (
                          <option key={center.id} value={center.id}>
                            {String(center.name)}
                          </option>
                        ))}
                      </select>
                    </label>
                    <button
                      type="button"
                      className="icon-button danger"
                      aria-label={`Eliminar línea ${index + 1}`}
                      disabled={lines.length <= 2 || saving}
                      onClick={() =>
                        setLines((current) =>
                          current.filter((_, i) => i !== index),
                        )
                      }
                    >
                      <Trash2 size={17} />
                    </button>
                  </div>
                </fieldset>
              ))}
            </div>
            <button
              type="button"
              className="button secondary"
              disabled={saving}
              onClick={() => setLines((current) => [...current, newLine()])}
            >
              <Plus size={16} />
              Agregar línea
            </button>
            <p className="small">
              Débitos y créditos deben coincidir. El servidor calcula y valida
              los importes exactos; la web conserva los decimales originales. El
              mismo identificador de operación se conserva al reintentar.
            </p>
            <ErrorNotice error={accounts.error || centers.error || error} />
            {validation && (
              <p role="alert" className="validation">
                {validation}
              </p>
            )}
            <div className="button-row form-actions">
              <button
                type="button"
                className="button secondary"
                disabled={saving}
                onClick={() => setCreating(false)}
              >
                Cerrar borrador
              </button>
              <button
                className="button"
                disabled={
                  saving ||
                  accounts.loading ||
                  centers.loading ||
                  !!accounts.error ||
                  !!centers.error
                }
              >
                {saving ? "Confirmando…" : "Confirmar asiento"}
              </button>
            </div>
          </form>
        </section>
      )}
      {success && (
        <div role="status" className="success-notice">
          {success}
        </div>
      )}
      <section className="panel">
        <div className="section-heading">
          <h2>Registros confirmados</h2>
          <span className="muted">{entries.data?.total || 0} asientos</span>
        </div>
        {entries.loading ? (
          <Loading />
        ) : entries.error ? (
          <ErrorNotice error={entries.error} />
        ) : (
          <DataTable
            items={entries.data?.items || []}
            currency={currency}
            columns={[
              { key: "reference", label: "Referencia" },
              { key: "date", label: "Fecha" },
              { key: "totalDebit", label: "Débitos", money: true },
              { key: "totalCredit", label: "Créditos", money: true },
            ]}
          />
        )}
        <div className="pagination">
          <span>Página {page}</span>
          <div>
            <button
              className="button secondary small-button"
              onClick={() => setPage((p) => p - 1)}
              disabled={page === 1 || entries.loading}
            >
              Anterior
            </button>
            <button
              className="button secondary small-button"
              onClick={() => setPage((p) => p + 1)}
              disabled={
                !entries.data ||
                page * 10 >= entries.data.total ||
                entries.loading
              }
            >
              Siguiente
            </button>
          </div>
        </div>
      </section>
      <ChallengeAction
        ids={readOnly ? ["DP-15"] : ["DP-01", "DP-04", "DP-14"]}
        token={token}
        companyId={companyId}
      />
    </>
  );
}
